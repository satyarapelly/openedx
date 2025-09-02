@description('The name of the workload using this infrastructure. Usually it is a keyword or name of the main service.')
@maxLength(13)
param workloadName string
@description('A short keyword to name this particular app service.')
param appServiceKeyword string

@minLength(2)
@maxLength(4)
param shortLocation string

@maxLength(2)
param stamp string

var commonName = '${workloadName}-${shortLocation}-${stamp}'

var appServiceName = 'app-${appServiceKeyword}-${commonName}'

module gondolinAppRbac 'br:pifddev.azurecr.io/bicep/modules/gondolin/app/appservice-rbac:v1.1.1' = {
  name: 'app-rbac-${appServiceName}'
  params: {
    workloadName: workloadName
    appServiceKeyword: appServiceKeyword
    shortLocation: shortLocation
    stamp: stamp
  }
}