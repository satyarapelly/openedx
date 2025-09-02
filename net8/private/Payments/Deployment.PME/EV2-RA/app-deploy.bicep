@description('Special ev2 parameter to pass a reference to the application package.')
param servicePackageLink string
@description('The name of the workload using this infrastructure. Usually it is a keyword or name of the main service.')
@maxLength(13)
param workloadName string
@description('A short keyword to name this particular app service.')
param appServiceKeyword string
@description('The build version of the application package.')
param buildVersion string

param location string = az.resourceGroup().location

@minLength(2)
@maxLength(4)
param shortLocation string

@maxLength(2)
param stamp string

param environment string

@description('Monitoring Role')
param MonitoringRole string

@description('Monitoring Tenant')
param MonitoringTenant string

@description('Monitoring Account Name')
param monitoringGcsAccount string

@description('Azure Client ID')
param ClientId string

@description('Environment name')
param EnvironmentName string

@description('Health Check Path for app')
param healthCheckPath string

@description('Whether to deploy to the staging slot. If true, the staging slot will be deployed. If false, the production slot will be deployed.')
param deployStagingSlot string

@description('Whether to swap slots after deploying to the staging slot. This parameter is used only if "deployStagingSlot" is true')
param swapSlots string

var commonName = '${workloadName}-${shortLocation}-${stamp}'
var appServiceName = 'app-${appServiceKeyword}-${commonName}'
var KeyVaultName = 'kv-${commonName}'
var fullEnvironmentName = '${EnvironmentName}-${location}'

var additionalAppSettings = {
  KeyVaultName: KeyVaultName
  WEBSITE_FIRST_PARTY_ID: 'AntMDS'
  WEBSITE_NODE_DEFAULT_VERSION: '6.7.0'
  MonitoringRole: MonitoringRole
  MonitoringTenant: MonitoringTenant
  MonitoringAccountName: monitoringGcsAccount
  AZURE_CLIENT_ID: ClientId
  Environment: fullEnvironmentName
  WEBSITE_DELAY_CERT_DELETION: '1'
  WEBSITE_RECYCLE_ON_CERT_ROTATION: '1'
  WEBSITE_ADD_SITENAME_BINDINGS_IN_APPHOST_CONFIG: '1'
  WEBSITE_LOAD_CERTIFICATES: '*'
  WEBSITE_SWAP_WARMUP_PING_PATH: '/probe'
  WEBSITE_SWAP_WARMUP_PING_STATUSES: 200
}

module gondolinAppDeploy 'br:pifddev.azurecr.io/bicep/modules/gondolin/app/appservice-deploy:v1.1.1' = {
  name: 'app-deploy-${appServiceName}'
  params: {
    servicePackageLink: servicePackageLink
    workloadName: workloadName
    appServiceKeyword: appServiceKeyword
    location: location
    shortLocation: shortLocation
    stamp: stamp
    environment: environment
    buildVersion: buildVersion
    healthCheckPath: healthCheckPath
    deployStagingSlot: deployStagingSlot
    swapSlots: swapSlots
    additionalAppSettings: additionalAppSettings
  }
}
