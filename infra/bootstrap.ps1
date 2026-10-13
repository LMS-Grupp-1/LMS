<#
.SYNOPSIS
    One-time setup of an Azure subscription for Lexicon LMS.

.DESCRIPTION
    Run once per subscription by the subscription Owner. Windows only (uses winget).

    1. Installs the Azure CLI and Bicep if they are missing.
    2. Logs in to Azure and lets you choose the subscription.
    3. Registers the resource providers used by the project.

    Safe to run more than once: steps that are already done are skipped.

.EXAMPLE
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

# --- 1. Tools ---------------------------------------------------------------

Write-Step "1. Checking tools"

if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
    Write-Host "Azure CLI not found. Installing with winget..."

    if (-not (Get-Command winget -ErrorAction SilentlyContinue)) {
        Stop-Script "winget not found. Install the Azure CLI manually from https://aka.ms/installazurecliwindowsx64 and run this script again."
    }

    winget install -e --id Microsoft.AzureCLI --accept-source-agreements --accept-package-agreements
    if ($LASTEXITCODE -ne 0) {
        Stop-Script ("Installation failed. Close Visual Studio, VS Code and other terminals and try again, " +
                     "or install manually from https://aka.ms/installazurecliwindowsx64")
    }

    Update-SessionPath
    if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
        Stop-Script "Azure CLI was installed. Open a new terminal and run this script again."
    }
}

$azVersion = (az version --output json | ConvertFrom-Json).'azure-cli'
Write-Host "Azure CLI $azVersion"

az bicep version --only-show-errors *> $null
if ($LASTEXITCODE -ne 0) {
    Write-Host "Bicep not found. Installing..."
    az bicep install --only-show-errors
    if ($LASTEXITCODE -ne 0) { Stop-Script "Failed to install Bicep." }
}
Write-Host (az bicep version --only-show-errors)

# --- 2. Login and subscription ----------------------------------------------

Write-Step "2. Azure login"

function Invoke-Login {
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
    Invoke-Login
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
        "t" { Invoke-Login; continue }
        "q" { Write-Host "Aborted."; exit 0 }
        default { continue }
    }
    break
}

# --- 3. Resource providers --------------------------------------------------

Write-Step "3. Registering resource providers (can take a few minutes)"

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

# --- 4. Deploy bootstrap.bicep ----------------------------------------------

Write-Step "4. Deploying bootstrap.bicep (resource group, identities, Key Vaults, roles)"

# Your user's object id. Gives you access to the secrets in the Key Vaults.
$adminPrincipalId = az ad signed-in-user show --query id --output tsv
if ($LASTEXITCODE -ne 0 -or -not $adminPrincipalId) { Stop-Script "Could not read your user object id." }

$outputs = az deployment sub create `
    --name "lms-bootstrap" `
    --location "swedencentral" `
    --template-file "$PSScriptRoot\bootstrap.bicep" `
    --parameters adminPrincipalId=$adminPrincipalId githubRepository=$githubRepository `
    --query properties.outputs `
    --output json | ConvertFrom-Json

if ($LASTEXITCODE -ne 0 -or -not $outputs) { Stop-Script "Deployment failed. See the error above." }

# --- 5. GitHub secrets ------------------------------------------------------

Write-Step "5. GitHub secrets for $githubRepository"

$clientId = $outputs.AZURE_CLIENT_ID.value
$tenantId = $outputs.AZURE_TENANT_ID.value
$subscriptionId = $outputs.AZURE_SUBSCRIPTION_ID.value

$answer = Read-Host "Set the GitHub secrets now? Requires admin on the repository (y/n)"
if ($answer -ne "y") {
    Write-Host ""
    Write-Host "Add these manually: GitHub -> Settings -> Secrets and variables -> Actions -> New repository secret"
    Write-Host "  AZURE_CLIENT_ID        $clientId"
    Write-Host "  AZURE_TENANT_ID        $tenantId"
    Write-Host "  AZURE_SUBSCRIPTION_ID  $subscriptionId"
    exit 0
}

# Install the GitHub CLI if it is missing.
if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
    Write-Host "GitHub CLI not found. Installing with winget..."
    winget install -e --id GitHub.cli --accept-source-agreements --accept-package-agreements
    if ($LASTEXITCODE -ne 0) { Stop-Script "Failed to install the GitHub CLI. Install it from https://cli.github.com and run this script again." }
    Update-SessionPath
    if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
        Stop-Script "GitHub CLI was installed. Open a new terminal and run this script again."
    }
}

# Log in to GitHub if needed (opens the browser).
gh auth status --hostname github.com *> $null
if ($LASTEXITCODE -ne 0) {
    gh auth login --hostname github.com --web --git-protocol https
    if ($LASTEXITCODE -ne 0) { Stop-Script "GitHub login failed." }
}

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
Set-GitHubSecret "AZURE_CLIENT_ID" $clientId
Set-GitHubSecret "AZURE_TENANT_ID" $tenantId
Set-GitHubSecret "AZURE_SUBSCRIPTION_ID" $subscriptionId

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