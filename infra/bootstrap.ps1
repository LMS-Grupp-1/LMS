<#
.SYNOPSIS
    One-time setup of an Azure subscription for Lexicon LMS.

.DESCRIPTION
    Run once per subscription by the subscription Owner. Windows only (uses winget).

    1. Installs the Azure CLI, Bicep and the GitHub CLI if they are missing.
    2. Logs in to Azure and lets you choose the subscription.
    3. Logs in to GitHub.
    4. Registers the resource providers used by the project.
    5. Deploys bootstrap.bicep (resource group, identities, Key Vaults, roles).
    6. Sets the GitHub secrets.

    Safe to run more than once: steps that are already done are skipped.

.EXAMPLE
    Double-click infra\bootstrap.cmd, or run:
    powershell -ExecutionPolicy Bypass -File .\infra\bootstrap.ps1
#>

# GitHub repository (owner/name). Must match the casing on GitHub exactly.
$githubRepository = "LMS-Grupp-1/LMS"

# The resource providers (Azure services) the project uses.
$providers = @(
    "Microsoft.App",                 # Container Apps
    "Microsoft.OperationalInsights", # Log Analytics
    "Microsoft.Sql",                 # Azure SQL
    "Microsoft.Storage",             # Blob Storage
    "Microsoft.KeyVault",            # Key Vault
    "Microsoft.ManagedIdentity"      # User-assigned managed identities
)

function Write-Step([string]$text) {
    Write-Host ""
    Write-Host "== $text ==" -ForegroundColor Cyan
}

function Stop-Script([string]$message) {
    Write-Host $message -ForegroundColor Red
    exit 1
}

# Reloads PATH so a tool installed by this script can be used without opening a new terminal.
function Update-SessionPath {
    $machinePath = [Environment]::GetEnvironmentVariable("Path", "Machine")
    $userPath = [Environment]::GetEnvironmentVariable("Path", "User")
    $env:Path = "$machinePath;$userPath"
}

# Installs a tool with winget if the command is missing.
function Install-Tool([string]$command, [string]$wingetId, [string]$manualUrl) {
    if (Get-Command $command -ErrorAction SilentlyContinue) { return }

    Write-Host "$command not found. Installing with winget..."
    if (-not (Get-Command winget -ErrorAction SilentlyContinue)) {
        Stop-Script "winget not found. Install it manually from $manualUrl and run this script again."
    }

    winget install -e --id $wingetId --accept-source-agreements --accept-package-agreements
    if ($LASTEXITCODE -ne 0) {
        Stop-Script ("Installation failed. Close Visual Studio, VS Code and other terminals and try again, " +
                     "or install manually from $manualUrl")
    }

    Update-SessionPath
    if (-not (Get-Command $command -ErrorAction SilentlyContinue)) {
        Stop-Script "$command was installed. Open a new terminal and run this script again."
    }
}

# --- 1. Tools ---------------------------------------------------------------

Write-Step "1. Checking tools"

Install-Tool "az" "Microsoft.AzureCLI" "https://aka.ms/installazurecliwindowsx64"
Install-Tool "gh" "GitHub.cli" "https://cli.github.com"

$azVersion = (az version --output json | ConvertFrom-Json).'azure-cli'
Write-Host "Azure CLI $azVersion"

az bicep version --only-show-errors *> $null
if ($LASTEXITCODE -ne 0) {
    Write-Host "Bicep not found. Installing..."
    az bicep install --only-show-errors
    if ($LASTEXITCODE -ne 0) { Stop-Script "Failed to install Bicep." }
}
Write-Host (az bicep version --only-show-errors)
Write-Host (gh --version | Select-Object -First 1)

# --- 2. Azure login and subscription ----------------------------------------

Write-Step "2. Azure login"

function Invoke-AzureLogin {
    $tenant = Read-Host "Tenant id (Directory ID). Leave empty to use your default directory"
    if ([string]::IsNullOrWhiteSpace($tenant)) {
        az login --output none
    }
    else {
        az login --tenant $tenant.Trim() --output none
    }
    if ($LASTEXITCODE -ne 0) { Stop-Script "Login failed." }
}

$account = az account show --output json 2>$null | ConvertFrom-Json
if ($LASTEXITCODE -ne 0 -or -not $account) {
    Invoke-AzureLogin
}

while ($true) {
    $account = az account show --output json | ConvertFrom-Json

    Write-Host ""
    Write-Host "Subscription: $($account.name)"
    Write-Host "Id:           $($account.id)"
    Write-Host "Tenant:       $($account.tenantId)"
    Write-Host "User:         $($account.user.name)"
    Write-Host ""
    Write-Host "  y = continue with this subscription"
    Write-Host "  s = choose another subscription"
    Write-Host "  t = log in to another directory (tenant)"
    Write-Host "  q = quit"
    $choice = Read-Host "Choice"

    switch ($choice) {
        "y" { break }
        "s" {
            az account list --query "[].{Name:name, Id:id, Tenant:tenantId}" --output table
            $subscriptionId = Read-Host "Subscription Id"
            az account set --subscription $subscriptionId.Trim()
            if ($LASTEXITCODE -ne 0) { Write-Host "Could not select that subscription." -ForegroundColor Yellow }
            continue
        }
        "t" { Invoke-AzureLogin; continue }
        "q" { Write-Host "Aborted."; exit 0 }
        default { continue }
    }
    break
}

# --- 3. GitHub login --------------------------------------------------------

Write-Step "3. GitHub login"

gh auth status --hostname github.com *> $null
if ($LASTEXITCODE -ne 0) {
    gh auth login --hostname github.com --web --git-protocol https
    if ($LASTEXITCODE -ne 0) { Stop-Script "GitHub login failed." }
}

# The ids are part of the OIDC subject GitHub sends when a workflow logs in to Azure.
$githubOwnerId = gh api "repos/$githubRepository" --jq '.owner.id'
$githubRepositoryId = gh api "repos/$githubRepository" --jq '.id'
if ($LASTEXITCODE -ne 0 -or -not $githubRepositoryId) { Stop-Script "Could not read $githubRepository from GitHub." }
Write-Host "Repository: $githubRepository (owner id $githubOwnerId, repository id $githubRepositoryId)"

# --- 4. Resource providers --------------------------------------------------

Write-Step "4. Registering resource providers (can take a few minutes)"

foreach ($provider in $providers) {
    $state = az provider show --namespace $provider --query registrationState --output tsv

    if ($state -eq "Registered") {
        Write-Host "  $provider - already registered"
        continue
    }

    Write-Host "  $provider - registering..."
    # --wait blocks until the registration is complete.
    az provider register --namespace $provider --wait --output none
    if ($LASTEXITCODE -ne 0) { Stop-Script "Failed to register $provider." }
    Write-Host "  $provider - registered" -ForegroundColor Green
}

# --- 5. Deploy bootstrap.bicep ----------------------------------------------

Write-Step "5. Deploying bootstrap.bicep (resource group, identities, Key Vaults, roles)"

# Your user's object id. Gives you access to the secrets in the Key Vaults.
$adminPrincipalId = az ad signed-in-user show --query id --output tsv
if ($LASTEXITCODE -ne 0 -or -not $adminPrincipalId) { Stop-Script "Could not read your user object id." }

$outputs = az deployment sub create `
    --name "lms-bootstrap" `
    --location "swedencentral" `
    --template-file "$PSScriptRoot\bootstrap.bicep" `
    --parameters adminPrincipalId=$adminPrincipalId githubRepository=$githubRepository githubOwnerId=$githubOwnerId githubRepositoryId=$githubRepositoryId `
    --query properties.outputs `
    --output json | ConvertFrom-Json

if ($LASTEXITCODE -ne 0 -or -not $outputs) { Stop-Script "Deployment failed. See the error above." }

Write-Host "Key Vaults: $($outputs.keyVaultTest.value), $($outputs.keyVaultProd.value)"

# --- 6. GitHub secrets ------------------------------------------------------

Write-Step "6. GitHub secrets for $githubRepository"

# Random string from a cryptographically secure generator. Letters and digits only,
# so it is safe in connection strings and environment variables.
function New-Secret([int]$length) {
    $chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789".ToCharArray()
    $bytes = New-Object byte[] $length
    [System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
    return -join ($bytes | ForEach-Object { $chars[$_ % $chars.Length] })
}

# Names of the secrets that already exist (repository level, or in an environment).
function Get-SecretNames([string]$environmentName) {
    if ($environmentName) {
        $names = gh secret list --repo $githubRepository --env $environmentName --json name --jq '.[].name'
    }
    else {
        $names = gh secret list --repo $githubRepository --json name --jq '.[].name'
    }
    if ($LASTEXITCODE -ne 0) { Stop-Script "Could not read the secrets. Are you admin on $githubRepository, and does the environment '$environmentName' exist?" }
    return @($names)
}

function Set-GitHubSecret([string]$name, [string]$value, [string]$environmentName) {
    if ($environmentName) {
        gh secret set $name --repo $githubRepository --env $environmentName --body $value
    }
    else {
        gh secret set $name --repo $githubRepository --body $value
    }
    if ($LASTEXITCODE -ne 0) { Stop-Script "Failed to set $name." }
}

# Azure login values: always updated, because they change when moving to another subscription.
Set-GitHubSecret "AZURE_CLIENT_ID" $outputs.AZURE_CLIENT_ID.value
Set-GitHubSecret "AZURE_TENANT_ID" $outputs.AZURE_TENANT_ID.value
Set-GitHubSecret "AZURE_SUBSCRIPTION_ID" $outputs.AZURE_SUBSCRIPTION_ID.value

# Passwords: only created when missing, so running the script again never changes them.
$repositorySecrets = Get-SecretNames ""
if ($repositorySecrets -notcontains "SQL_ADMIN_PASSWORD") {
    # Azure SQL requires 3 of 4 character types. The suffix guarantees that.
    Set-GitHubSecret "SQL_ADMIN_PASSWORD" ((New-Secret 28) + "-Aa1")
}
else {
    Write-Host "  SQL_ADMIN_PASSWORD - already set"
}

foreach ($environmentName in @("test", "prod")) {
    $environmentSecrets = Get-SecretNames $environmentName

    if ($environmentSecrets -notcontains "JWT_SECRET_KEY") {
        Set-GitHubSecret "JWT_SECRET_KEY" (New-Secret 64) $environmentName
    }
    else {
        Write-Host "  JWT_SECRET_KEY ($environmentName) - already set"
    }

    if ($environmentSecrets -notcontains "SEED_PASSWORD") {
        $secure = Read-Host "Password for the seeded users in '$environmentName' (save it in a password manager)" -AsSecureString
        $plain = (New-Object System.Net.NetworkCredential("", $secure)).Password
        if ($plain.Length -lt 3) { Stop-Script "The password must be at least 3 characters." }
        Set-GitHubSecret "SEED_PASSWORD" $plain $environmentName
    }
    else {
        Write-Host "  SEED_PASSWORD ($environmentName) - already set"
    }
}

Write-Host ""
Write-Host "Done." -ForegroundColor Green