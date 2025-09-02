param location string
@description('Array of stamp configurations. Each configuration is an object with the properties necessary to build the resource names of a stamp.')
param stampConfigs array

@maxLength(20)
param networkWorkloadName string

@description('Array of IP addresses to allow access DB account. If empty, all IPs are allowed.')
param ipAllowList array = []

param cosmosDbName string
param cosmosDbLocations array = []

param cosmosdbDataContributorPrincipalIds array = []

param sqlDatabases array = [
  {
    name: 'ShortUrlDB'
    containers: [
      {
        name: 'Codes'
        paths: [
          '/id'
        ]
        indexingPolicy: {
          indexingMode: 'consistent'
          automatic: true 
          includedPaths: [
            {
              path: '/*'
            }
          ]
          excludedPaths: [
            {
              path: '/"_etag"/?'
            }
          ]
        }
        throughput: 400
        authoscalSettingsMaxThroughput: 4000
        defaultTtl: 2592000 // 30 days in seconds
      }
    ]
  }
]

var privateEndpointsSnetName = 'privateendpoints'

var builtInRoleNames = {
  'Cosmos DB Built-in Data Contributor': resourceId(
    'Microsoft.DocumentDB/databaseAccounts/sqlRoleDefinitions',
    cosmosDbName,
    '00000000-0000-0000-0000-000000000002'
  )
}

// Based on the resource naming pattern in Gondolin infrastructure: we build references to existing subnets and private DNS zones.
var networkResources = [for (config, i) in stampConfigs: {
  location: config.location
  rg: 'rg-${networkWorkloadName}-${config.location}-${config.stamp}'
  appsVnet: 'vnet-apps-${networkWorkloadName}-${config.shortLocation}-${config.stamp}'
  privateEndpointsSnetName: privateEndpointsSnetName
  cosmosDbPrivateDnsZoneName: 'privatelink.documents.azure.com'
  storageBlobPrivateDnsZoneName: 'privatelink.blob.${environment().suffixes.storage}'
  eventHubPrivateDnsZoneName: 'privatelink.servicebus.windows.net'
}]

resource privateEndpointSubnets 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = [
  for r in networkResources: {
    name: '${r.appsVnet}/${r.privateEndpointsSnetName}'
    scope: resourceGroup(r.rg)
  }
]

resource cosmosDbPrivateDnsZones 'Microsoft.Network/privateDnsZones@2020-06-01' existing = [
  for r in networkResources: {
    name: r.cosmosDbPrivateDnsZoneName
    scope: resourceGroup(r.rg)
  }
]

var cosmosDbPrivateEndpoints = [
  for (config, i) in stampConfigs: {
    subnetResourceId: privateEndpointSubnets[i].id
    location: config.location
    service: 'Sql'
    privateDnsZoneGroup: {
      privateDnsZoneGroupConfigs:  [
        {
          privateDnsZoneResourceId: cosmosDbPrivateDnsZones[i].id
        }
      ]
    }
  }
]

module cosmosDb 'br/public:avm/res/document-db/database-account:0.8.0' = {
  name: cosmosDbName
  params: {
    name: cosmosDbName
    location: location
    disableLocalAuth: true
    automaticFailover: false
    enableMultipleWriteLocations: true
    databaseAccountOfferType: 'Standard'
    defaultConsistencyLevel: 'Session'
    minimumTlsVersion: 'Tls12'
    locations: [ for writeRegion in cosmosDbLocations:{
        locationName: writeRegion.location
        failoverPriority: writeRegion.failoverPriority
        isZoneRedundant: false
      }
    ]
    networkRestrictions: {
      ipRules: ipAllowList
      virtualNetworkRules: []
      networkAclBypass: 'None'
      publicNetworkAccess: 'Enabled'
    }
    privateEndpoints: cosmosDbPrivateEndpoints
    sqlDatabases: sqlDatabases
    backupPolicyType: 'Periodic'
    backupIntervalInMinutes: 240
    backupRetentionIntervalInHours: 8
    backupStorageRedundancy: 'Geo'
  }
}

resource cosmosDBDataContribureRoleAssignments 'Microsoft.DocumentDB/databaseAccounts/sqlRoleAssignments@2024-05-15' = [
  for principalId in cosmosdbDataContributorPrincipalIds: {
    name: '${cosmosDbName}/${guid(cosmosDbName, principalId, 'Cosmos DB Built-in Data Contributor')}'
    properties: {
      principalId: principalId
      #disable-next-line use-resource-id-functions
      roleDefinitionId: builtInRoleNames['Cosmos DB Built-in Data Contributor']
      scope: cosmosDb.outputs.resourceId
    }
  }
]

output cosmosDbId string = cosmosDb.outputs.resourceId
