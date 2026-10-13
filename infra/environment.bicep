// Resources for one environment (test or prod).
// Deployed after shared.bicep, once per environment. Needs the Contributor role.
//
// - SQL database (Basic, fixed price)
// - Blob container for documents
// - Secrets in the environment's Key Vault
// - Container app for the API (internal, only reachable from the BFF)
// - Container app for the Blazor BFF (public)

@description('Which environment to deploy.')
@allowed([
  'test'
  'prod'
])
param environmentName string

@description('Azure region for all resources.')
param location string = resourceGroup().location

@description('Administrator login for the SQL server. Must match shared.bicep.')
param sqlAdminLogin string = 'lmsadmin'

@description('Administrator password for the SQL server. Must match shared.bicep.')
@secure()
param sqlAdminPassword string

@description('Key used by the API to sign JWTs. At least 32 characters.')
@secure()
param jwtSecretKey string

@description('Password for the seeded users.')
@secure()
param seedPassword string

@description('Use Microsoft\'s test image instead of our own. Only for the first deployment, before any image exists.')
param useQuickstartImage bool = false

@description('Container registry path for our images (lowercase).')
param imageRepository string = 'ghcr.io/lms-grupp-1'

// Same suffix as in bootstrap.bicep and shared.bicep.
var suffix = take(uniqueString(resourceGroup().id), 8)

// ASP.NET Core environment name: 'Staging' for test, 'Production' for prod.
var aspNetCoreEnvironment = environmentName == 'test' ? 'Staging' : 'Production'

// The quickstart image listens on port 80, .NET images on port 8080.
var quickstartImage = 'mcr.microsoft.com/k8se/quickstart:latest'
var apiImage = useQuickstartImage ? quickstartImage : '${imageRepository}/lms-api:${environmentName}'
var webImage = useQuickstartImage ? quickstartImage : '${imageRepository}/lms-web:${environmentName}'
var targetPort = useQuickstartImage ? 80 : 8080

// --- Existing resources from bootstrap.bicep and shared.bicep ---------------

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2024-03-01' existing = {
  name: 'cae-lms'
}

resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' existing = {
  name: 'sql-lms-${suffix}'
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: 'stlms${suffix}'
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' existing = {
  parent: storageAccount
  name: 'default'
}

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: 'kv-lms-${environmentName}-${suffix}'
}

resource appIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: 'id-lms-app-${environmentName}'
}

// --- App URLs ---------------------------------------------------------------
// Calculated from the environment's domain, so the API and the web app do not
// depend on each other (the API needs the web URL for CORS, the web app needs
// the API URL for YARP).

var webFqdn = 'lms-web-${environmentName}.${containerAppsEnvironment.properties.defaultDomain}'
var apiInternalFqdn = 'lms-api-${environmentName}.internal.${containerAppsEnvironment.properties.defaultDomain}'

// --- Database ---------------------------------------------------------------

resource database 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
  parent: sqlServer
  name: 'lms-db-${environmentName}'
  location: location
  sku: {
    name: 'Basic'
    tier: 'Basic'
    capacity: 5 // 5 DTU, the fixed-price Basic level
  }
  properties: {
    maxSizeBytes: 2147483648 // 2 GB, the maximum for Basic
    requestedBackupStorageRedundancy: 'Local'
  }
}

// --- Blob container ---------------------------------------------------------

resource documentsContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  parent: blobService
  name: 'documents-${environmentName}'
  properties: {
    publicAccess: 'None'
  }
}

// --- Key Vault secrets ------------------------------------------------------

resource sqlConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'sql-connection-string'
  properties: {
    value: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=${database.name};User ID=${sqlAdminLogin};Password=${sqlAdminPassword};MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
  }
}

resource blobConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'blob-connection-string'
  properties: {
    value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage}'
  }
}

resource jwtSecretKeySecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'jwt-secret-key'
  properties: {
    value: jwtSecretKey
  }
}

resource seedPasswordSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'seed-password'
  properties: {
    value: seedPassword
  }
}

// --- API container app ------------------------------------------------------

resource apiApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'lms-api-${environmentName}'
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${appIdentity.id}': {}
    }
  }
  properties: {
    environmentId: containerAppsEnvironment.id
    workloadProfileName: 'Consumption'
    configuration: {
      // Internal: only reachable from other apps in cae-lms (the BFF), not from the internet.
      ingress: {
        external: false
        targetPort: targetPort
        transport: 'auto'
      }
      // Read from Key Vault with the app's identity. Nothing secret is stored in the app itself.
      secrets: [
        {
          name: 'sql-connection-string'
          keyVaultUrl: sqlConnectionStringSecret.properties.secretUri
          identity: appIdentity.id
        }
        {
          name: 'blob-connection-string'
          keyVaultUrl: blobConnectionStringSecret.properties.secretUri
          identity: appIdentity.id
        }
        {
          name: 'jwt-secret-key'
          keyVaultUrl: jwtSecretKeySecret.properties.secretUri
          identity: appIdentity.id
        }
        {
          name: 'seed-password'
          keyVaultUrl: seedPasswordSecret.properties.secretUri
          identity: appIdentity.id
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'api'
          image: apiImage
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          // '__' in a variable name corresponds to ':' in appsettings.json.
          env: [
            { name: 'ASPNETCORE_ENVIRONMENT', value: aspNetCoreEnvironment }
            { name: 'ASPNETCORE_FORWARDEDHEADERS_ENABLED', value: 'true' }
            { name: 'ConnectionStrings__ApplicationDbContext', secretRef: 'sql-connection-string' }
            { name: 'JwtSettings__SecretKey', secretRef: 'jwt-secret-key' }
            { name: 'password', secretRef: 'seed-password' }
            { name: 'Cors__AllowedOrigins__0', value: 'https://${webFqdn}' }
            { name: 'BlobStorage__ConnectionString', secretRef: 'blob-connection-string' }
            { name: 'BlobStorage__ContainerName', value: documentsContainer.name }
          ]
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 1
      }
    }
  }
}

// --- Web (Blazor BFF) container app -----------------------------------------

resource webApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'lms-web-${environmentName}'
  location: location
  properties: {
    environmentId: containerAppsEnvironment.id
    workloadProfileName: 'Consumption'
    configuration: {
      // External: the public website.
      ingress: {
        external: true
        targetPort: targetPort
        transport: 'auto'
      }
    }
    template: {
      containers: [
        {
          name: 'web'
          image: webImage
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          env: [
            { name: 'ASPNETCORE_ENVIRONMENT', value: aspNetCoreEnvironment }
            { name: 'ASPNETCORE_FORWARDEDHEADERS_ENABLED', value: 'true' }
            // Where YARP forwards /api/proxy/* (the API's internal address).
            { name: 'ReverseProxy__Clusters__remote-api__Destinations__primary__Address', value: 'https://${apiInternalFqdn}/' }
          ]
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 1
      }
    }
  }
}

output webUrl string = 'https://${webApp.properties.configuration.ingress.fqdn}'
output apiInternalUrl string = 'https://${apiApp.properties.configuration.ingress.fqdn}'
