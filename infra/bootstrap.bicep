// One-time setup of an Azure subscription for Lexicon LMS.
// Deployed by the subscription Owner with infra/bootstrap.ps1.
//
// Creates everything that needs Owner permissions:
// - the resource group
// - the identity GitHub Actions logs in with (OIDC, no passwords)
// - one identity per environment for the container apps
// - the container registry for the images
// - one Key Vault per environment
// - all role assignments
//
// Everything else is created by shared.bicep and environment.bicep,
// which only need the Contributor role.

targetScope = 'subscription'

@description('Azure region for all resources.')
param location string = 'swedencentral'

@description('Name of the resource group for the project.')
param resourceGroupName string = 'rg-lms'

@description('GitHub repository as owner/name. Must match the casing on GitHub exactly.')
param githubRepository string = 'LMS-Grupp-1/LMS'

@description('GitHub id of the repository owner (organization or user). Used in the OIDC subject.')
param githubOwnerId string

@description('GitHub id of the repository. Used in the OIDC subject.')
param githubRepositoryId string

@description('Object id of the person running the bootstrap. Gets access to read and manage Key Vault secrets.')
param adminPrincipalId string

resource resourceGroup 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: resourceGroupName
  location: location
}

// Resources inside the resource group must be deployed through a module.
module resources 'modules/bootstrap-resources.bicep' = {
  name: 'bootstrap-resources'
  scope: resourceGroup
  params: {
    location: location
    githubRepository: githubRepository
    githubOwnerId: githubOwnerId
    githubRepositoryId: githubRepositoryId
    adminPrincipalId: adminPrincipalId
  }
}

// Values to store as repository secrets in GitHub.
output AZURE_CLIENT_ID string = resources.outputs.githubClientId
output AZURE_TENANT_ID string = tenant().tenantId
output AZURE_SUBSCRIPTION_ID string = subscription().subscriptionId

// Value to store as a repository variable in GitHub.
output AZURE_CONTAINER_REGISTRY string = resources.outputs.containerRegistryName

output keyVaultTest string = resources.outputs.keyVaultTestName
output keyVaultProd string = resources.outputs.keyVaultProdName
