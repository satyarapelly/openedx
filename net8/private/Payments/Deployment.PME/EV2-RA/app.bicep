@description('The name of the workload using this infrastructure. Usually it is a keyword or name of the main service.')
@maxLength(13)
param workloadName string
@description('A short keyword to name this particular app service.')
param appServiceKeyword string
@description('The "workloadName" used in the network deployment.')
param networkWorkloadName string
@description('The name of the new subnet to deploy the app service to.')
param subnetName string

param location string = az.resourceGroup().location

@minLength(2)
@maxLength(4)
param shortLocation string

@maxLength(2)
param stamp string

@description('Health Check Path for app')
param healthCheckPath string

@description('Whether to deploy PX client certificates (commerce, aad, px) for the app service plan')
param deployPXCertificates string = 'false'

param appServicePlanSkuTier string
param appServicePlanSkuName string
param appServicePlanMinCapacity string
param appServicePlanMaxCapacity string
param appServicePlanDefaultCapacity string

param publicAccessEnabled string
param certificateAuthEnabled string
param userAssignedMIName string
param userAssignedMIRG string

param monitoringGcsEnvironment string
param monitoringGcsAccount string
param monitoringGcsNamespace string
param monitoringGcsAuthId string
param monitoringConfigVersion string
param monitoringRole string
param monitoringGenevaCertSecretName string

param commerceAccountClientCertificateSecret string
param pxClientCertificateSecret string
param aadAMEClientCertificateSecret string
param genevaCertificateSecret string

var commonName = '${workloadName}-${shortLocation}-${stamp}'
var appServiceName = 'app-${appServiceKeyword}-${commonName}'
var appServicePlanName = 'plan-${appServiceKeyword}-${commonName}'
var keyVaultName = 'kv-${commonName}'

module gondolinApp 'br:pifddev.azurecr.io/bicep/modules/gondolin/app/appservice-win-geneva:v1.1.1' = {
  name: 'app-win-${appServiceName}'
  params: {
    workloadName: workloadName
    appServiceKeyword: appServiceKeyword
    networkWorkloadName: networkWorkloadName
    subnetName: subnetName
    location: location
    shortLocation: shortLocation
    stamp: stamp
    serviceplanAutoscaleMinCapacity: appServicePlanMinCapacity
    serviceplanAutoscaleMaxCapacity: appServicePlanMaxCapacity
    serviceplanAutoscaleDefaultCapacity: appServicePlanDefaultCapacity
    serviceplanAutoscaleNotifyEmail: ''
    appServicePlanSkuTier: appServicePlanSkuTier
    appServicePlanSkuName: appServicePlanSkuName
    appAuthUAMIName: userAssignedMIName
    appAuthUAMIRG: userAssignedMIRG
    healthCheckPath: healthCheckPath
    netFrameworkVersion: 'v6.0'
    publicNetworkAccess: bool(publicAccessEnabled) ? 'Enabled' : 'Disabled'
    ipSecurityRestrictions: bool(publicAccessEnabled)
      ? [
          {
            ipAddress: '23.103.190.208/30'
            action: 'Allow'
            priority: 100
            name: 'Allow Corpnet 1'
            description: 'Allow corpnet access 1'
          }
          {
            ipAddress: '131.107.0.0/16'
            action: 'Allow'
            priority: 101
            name: 'Allow Corpnet 2'
            description: 'Allow corpnet access 2'
          }
        ]
      : []
    clientCertEnabled: bool(certificateAuthEnabled)
    clientCertMode: 'Optional'
    useDefaultLocalCacheSettings: true
    monitoringGcsEnvironment: monitoringGcsEnvironment
    monitoringGcsAccount: monitoringGcsAccount
    monitoringGcsNamespace: monitoringGcsNamespace
    monitoringGcsAuthId: monitoringGcsAuthId
    monitoringConfigVersion: monitoringConfigVersion
    monitoringRole: monitoringRole
    monitoringGenevaCertSecretName: monitoringGenevaCertSecretName
  }
}

resource kv 'Microsoft.KeyVault/vaults@2023-02-01' existing = {
  name: keyVaultName
}

resource serviceplan 'Microsoft.Web/serverfarms@2023-01-01' existing = {
  name: appServicePlanName
}

resource commerceCertificate 'Microsoft.Web/certificates@2023-01-01' = if(bool(deployPXCertificates)) {
  name: 'cert-commerce-client${appServiceName}'
  location: location
  properties: {
    keyVaultId: kv.id
    keyVaultSecretName: commerceAccountClientCertificateSecret
    serverFarmId: serviceplan.id
  }
}

resource pxClientCertificate 'Microsoft.Web/certificates@2023-01-01' = if(bool(deployPXCertificates)) {
  name: 'cert-px-client${appServiceName}'
  location: location
  properties: {
    keyVaultId: kv.id
    keyVaultSecretName: pxClientCertificateSecret
    serverFarmId: serviceplan.id
  }
}

resource aadAMEClientCertificate 'Microsoft.Web/certificates@2023-01-01' = if(bool(deployPXCertificates)) {
  name: 'cert-aad-ame-client${appServiceName}'
  location: location
  properties: {
    keyVaultId: kv.id
    keyVaultSecretName: aadAMEClientCertificateSecret
    serverFarmId: serviceplan.id
  }
}

resource genevaCertificate 'Microsoft.Web/certificates@2023-01-01' = if(bool(deployPXCertificates)) {
  name: 'cert-genvea${appServiceName}'
  location: location
  properties: {
    keyVaultId: kv.id
    keyVaultSecretName: genevaCertificateSecret
    serverFarmId: serviceplan.id
  }
}
