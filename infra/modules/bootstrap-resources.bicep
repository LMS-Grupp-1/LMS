// Resources created by bootstrap.bicep inside the resource group.

param location string
param githubRepository string
param githubOwnerId string
param githubRepositoryId string
param adminPrincipalId string

// Short suffix that makes globally unique names (Key Vault names are global).
// Same resource group in the same subscription always gives the same suffix.
var suffix = take(uniqueString(resourceGroup().id), 8)

var environments = [
  'test'
  'prod'
]

// Built-in Azure role ids (the same in every subscription).
var roles = {
  contributor: 'b24988ac-6180-42a0-ab88-20f7382dd24c'
  keyVaultSecretsUser: '4633458b-17de-408a-b874-0445c86b69e6'
  keyVaultSecretsOfficer: 'b86a8fe4-44ce-4948-aee5-eccb2c155cd7'
}

// --- Identity for GitHub Actions --------------------------------------------

resource githubIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: 'id-lms-github'
  location: location
}

// Trust tokens from GitHub Actions jobs that run in the 'test' or 'prod' environment.
// GitHub sends the subject in one of two formats, so both are trusted:
//   repo:Owner/Repo:environment:test                     (classic)
//   repo:Owner@<ownerId>/Repo@<repoId>:environment:test  (with immutable ids)
// Federated credentials on one identity cannot be created in parallel,
// so batchSize(1) and dependsOn create them one at a time.
var repositoryParts = split(githubRepository, '/')
var subjectWithIds = 'repo:${repositoryParts[0]}@${githubOwnerId}/${repositoryParts[1]}@${githubRepositoryId}'

@batchSize(1)
resource githubFederatedCredentials 'Microsoft.ManagedIdentity/userAssignedIdentities/federatedIdentityCredentials@2023-01-31' = [
  for env in environments: {
    parent: githubIdentity
    name: 'github-${env}'
    properties: {
      issuer: 'https://token.actions.githubusercontent.com'
      subject: 'repo:${githubRepository}:environment:${env}'
      audiences: [
        'api://AzureADTokenExchange'
      ]
    }
  }
]

@batchSize(1)
resource githubFederatedCredentialsWithIds 'Microsoft.ManagedIdentity/userAssignedIdentities/federatedIdentityCredentials@2023-01-31' = [
  for env in environments: {
    parent: githubIdentity
    name: 'github-${env}-ids'
    properties: {
      issuer: 'https://token.actions.githubusercontent.com'
      subject: '${subjectWithIds}:environment:${env}'
      audiences: [
        'api://AzureADTokenExchange'
      ]
    }
    dependsOn: [
      githubFederatedCredentials
    ]
  }
]

// GitHub Actions may create and change resources in the resource group, but not assign roles.
resource githubContributor 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, githubIdentity.id, roles.contributor)
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roles.contributor)
    principalId: githubIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// --- Identities for the container apps -------------------------------------

resource appIdentities 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = [
  for env in environments: {
    name: 'id-lms-app-${env}'
    location: location
  }
]

// --- Key Vaults ------------------------------------------------------------

resource keyVaults 'Microsoft.KeyVault/vaults@2023-07-01' = [
  for env in environments: {
    name: 'kv-lms-${env}-${suffix}'
    location: location
    properties: {
      tenantId: tenant().tenantId
      sku: {
        family: 'A'
        name: 'standard'
      }
      enableRbacAuthorization: true
      enableSoftDelete: true
      softDeleteRetentionInDays: 7
      publicNetworkAccess: 'Enabled'
    }
  }
]

// Each app identity may read the secrets in its own environment's Key Vault only.
resource appSecretsUser 'Microsoft.Authorization/roleAssignments@2022-04-01' = [
  for (env, i) in environments: {
    name: guid(keyVaults[i].id, appIdentities[i].id, roles.keyVaultSecretsUser)
    scope: keyVaults[i]
    properties: {
      roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roles.keyVaultSecretsUser)
      principalId: appIdentities[i].properties.principalId
      principalType: 'ServicePrincipal'
    }
  }
]

// The person running the bootstrap may read and manage secrets in both Key Vaults.
resource adminSecretsOfficer 'Microsoft.Authorization/roleAssignments@2022-04-01' = [
  for (env, i) in environments: {
    name: guid(keyVaults[i].id, adminPrincipalId, roles.keyVaultSecretsOfficer)
    scope: keyVaults[i]
    properties: {
      roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roles.keyVaultSecretsOfficer)
      principalId: adminPrincipalId
      principalType: 'User'
    }
  }
]

output githubClientId string = githubIdentity.properties.clientId
output keyVaultTestName string = keyVaults[0].name
output keyVaultProdName string = keyVaults[1].name
