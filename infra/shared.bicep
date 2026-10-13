// Resources shared by the test and prod environments.
// Deployed to the resource group created by bootstrap.bicep. Needs the Contributor role.
//
// - Log Analytics workspace (logs from the container apps)
// - Container Apps environment (hosts all four container apps)
// - Azure SQL server (one database per environment is added by environment.bicep)
// - Storage account (one blob container per environment is added by environment.bicep)

@description('Azure region for all resources.')
param location string = resourceGroup().location

@description('Administrator login for the SQL server.')
param sqlAdminLogin string = 'lmsadmin'

@description('Administrator password for the SQL server.')
@secure()
param sqlAdminPassword string

// Same suffix as in bootstrap.bicep, so all names in the resource group match.
var suffix = take(uniqueString(resourceGroup().id), 8)

// --- Logging ----------------------------------------------------------------

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: 'log-lms'
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

// --- Container Apps environment ---------------------------------------------

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: 'cae-lms'
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
    // Consumption only: pay per use, and apps can scale to zero.
    workloadProfiles: [
      {
        name: 'Consumption'
        workloadProfileType: 'Consumption'
      }
    ]
    zoneRedundant: false
  }
}

// --- SQL server -------------------------------------------------------------

resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' = {
  name: 'sql-lms-${suffix}'
  location: location
  properties: {
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
    version: '12.0'
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }
}

// "Allow Azure services and resources to access this server".
// The special range 0.0.0.0 - 0.0.0.0 lets the container apps connect.
resource sqlAllowAzureServices 'Microsoft.Sql/servers/firewallRules@2023-08-01-preview' = {
  parent: sqlServer
  name: 'AllowAllWindowsAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// --- Storage account --------------------------------------------------------

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: 'stlms${suffix}'
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: false
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
  }
}

// Deleted files and containers can be restored for 7 days.
resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  parent: storageAccount
  name: 'default'
  properties: {
    deleteRetentionPolicy: {
      enabled: true
      days: 7
    }
    containerDeleteRetentionPolicy: {
      enabled: true
      days: 7
    }
  }
}

output containerAppsEnvironmentName string = containerAppsEnvironment.name
output containerAppsDefaultDomain string = containerAppsEnvironment.properties.defaultDomain
output sqlServerName string = sqlServer.name
output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName
output storageAccountName string = storageAccount.name
