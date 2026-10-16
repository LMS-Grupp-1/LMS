<#
.SYNOPSIS
    Deletes all Azure resources for Lexicon LMS.

.DESCRIPTION
    Deletes the resource group rg-lms and permanently removes (purges) its Key Vaults,
    so the same names can be used again by bootstrap.ps1.

    Only touches:
    - the resource group rg-lms, and only if it has the tag project = lexicon-lms
    - Key Vaults that were inside rg-lms
    - the subscription deployment history entry "lms-bootstrap"

    Nothing else in the subscription is changed.

.EXAMPLE
    Double-click infra\teardown.cmd, or run:
    powershell -ExecutionPolicy Bypass -File .\infra\teardown.ps1
#>

$resourceGroup = "rg-lms"
$requiredTag = "lexicon-lms"

function Stop-Script([string]$message) {
    Write-Host $message -ForegroundColor Red
    exit 1
}

# --- 1. Check login and resource group --------------------------------------

if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
    Stop-Script "Azure CLI not found. Nothing to delete from this computer."
}

$account = az account show --output json 2>$null | ConvertFrom-Json
if ($LASTEXITCODE -ne 0 -or -not $account) {
    Stop-Script "Not logged in. Run: az login --tenant <tenant id>"
}

$group = az group show --name $resourceGroup --output json 2>$null | ConvertFrom-Json
if ($LASTEXITCODE -ne 0 -or -not $group) {
    Stop-Script "Resource group '$resourceGroup' does not exist in subscription '$($account.name)'. Nothing to delete."
}

# Safety check: only delete the resource group created by bootstrap.bicep.
if ($group.tags.project -ne $requiredTag) {
    Stop-Script ("Resource group '$resourceGroup' does not have the tag project = $requiredTag. " +
                 "It was not created by bootstrap.ps1, so it is not deleted.")
}

# --- 2. Show what will be deleted -------------------------------------------

Write-Host ""
Write-Host "Subscription: $($account.name) ($($account.id))"
Write-Host "User:         $($account.user.name)"
Write-Host ""
Write-Host "These resources in '$resourceGroup' will be deleted:" -ForegroundColor Yellow
az resource list --resource-group $resourceGroup --query "[].{Name:name, Type:type}" --output table

# Remember the Key Vault names now. After the resource group is deleted they are only in the soft-delete list.
$keyVaults = @(az keyvault list --resource-group $resourceGroup --query "[].name" --output tsv)

Write-Host ""
Write-Host "Key Vaults that will also be purged (permanently removed): $($keyVaults -join ', ')" -ForegroundColor Yellow
Write-Host ""
Write-Host "This cannot be undone. All data in the databases and blob storage is lost." -ForegroundColor Red
$confirmation = Read-Host "Type the resource group name ($resourceGroup) to confirm"
if ($confirmation -ne $resourceGroup) {
    Write-Host "Aborted. Nothing was deleted."
    exit 0
}

# --- 3. Delete --------------------------------------------------------------

Write-Host ""
Write-Host "Deleting $resourceGroup (takes 5-15 minutes)..." -ForegroundColor Cyan
az group delete --name $resourceGroup --yes
if ($LASTEXITCODE -ne 0) { Stop-Script "Failed to delete $resourceGroup." }
Write-Host "$resourceGroup deleted." -ForegroundColor Green

# Key Vaults stay in soft-delete for 7 days and block their names. Purge only the ones from rg-lms.
foreach ($vault in $keyVaults) {
    if ([string]::IsNullOrWhiteSpace($vault)) { continue }
    Write-Host "Purging Key Vault $vault..."
    az keyvault purge --name $vault --output none
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  Could not purge $vault. It is removed automatically after 7 days." -ForegroundColor Yellow
    }
}

# Remove the bootstrap entry from the subscription's deployment history. It is only a log entry.
az deployment sub delete --name "lms-bootstrap" --output none 2>$null

Write-Host ""
Write-Host "Done. Run infra\bootstrap.cmd to set everything up again." -ForegroundColor Green