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

var commonName = '${workloadName}-${shortLocation}-${stamp}'
var appServiceName = 'app-${appServiceKeyword}-${commonName}'

module swap 'br:pifddev.azurecr.io/bicep/modules/appservice/appserviceswapslots:v0.0.2' = {
  name: '${appServiceName}-swapSlots'
  params: {
    location: location
    webAppName: appServiceName
    buildVersion: buildVersion
  }
}