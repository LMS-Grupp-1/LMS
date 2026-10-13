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
    --parameters adminPrincipalId=$adminPrincipalId `
    --query properties.outputs `
    --output json | ConvertFrom-Json

if ($LASTEXITCODE -ne 0 -or -not $outputs) { Stop-Script "Deployment failed. See the error above." }

# --- 5. Values for GitHub ---------------------------------------------------

Write-Step "5. Add these as repository secrets in GitHub"
Write-Host "GitHub -> Settings -> Secrets and variables -> Actions -> New repository secret"
Write-Host ""
Write-Host "  AZURE_CLIENT_ID        $($outputs.AZURE_CLIENT_ID.value)"
Write-Host "  AZURE_TENANT_ID        $($outputs.AZURE_TENANT_ID.value)"
Write-Host "  AZURE_SUBSCRIPTION_ID  $($outputs.AZURE_SUBSCRIPTION_ID.value)"
Write-Host ""
Write-Host "Key Vaults: $($outputs.keyVaultTest.value), $($outputs.keyVaultProd.value)"
Write-Host ""
Write-Host "Done." -ForegroundColor Green