@description('The name of the workload using this infrastructure. Usually it is a keyword or name of the main service.')
@maxLength(13)
param workloadName string

@description('The "workloadName" used in the network deployment.')
param networkWorkloadName string

@description('The deployment identity object id. This is used to grant the deployment identity access to the key vault.')
param deploymentIdentityObjectId string
param ev2IpAddresses array
param ev2KeyVaultExtensionIpAddresses array

param location string = az.resourceGroup().location

@minLength(2)
@maxLength(4)
param shortLocation string

@maxLength(2)
param stamp string

module gondolinMainKeyVault 'br:pifddev.azurecr.io/bicep/modules/gondolin/main-keyvault:v1.1.1' = {
  name: 'mkv-${workloadName}-${location}-${stamp}'
  params: {
    workloadName: workloadName
    networkWorkloadName: networkWorkloadName
    deploymentIdentityObjectId: deploymentIdentityObjectId
    allowedIpAddresses: union(ev2IpAddresses, ev2KeyVaultExtensionIpAddresses)
    location: location
    shortLocation: shortLocation
    stamp: stamp
  }
}