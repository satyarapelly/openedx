@description('The name of the workload using this NETWORK infrastructure. Use a distinct name for this network infrastructure template.')
param workloadName string

@description('The address prefix for the applications vnet. This should be a /16 CIDR block first 2 octets. Example: "10.2"')
param vnetAddressPrefix16Apps string

@description('The address prefix for the firewall vnet. This should be a /16 CIDR block first 2 octets. Example: "10.3"')
param vnetAddressPrefix16Firewall string

@description('The location of the resources. Example: "eastus"')
param location string = az.resourceGroup().location

@minLength(2)
@maxLength(4)
@description('The abbreviation of the location of the resources. Example: "eus2"')
param shortLocation string

@maxLength(2)
@description('The stamp of the resources. Example: "1"')
param stamp string

@description('The availability zones to use for the resources that support them. Example: ["1", "2", "3"]')
param availabilityZones array

@description('A list of DNS zones to create as private DNS zones in the subnet.')
param additionalPrivateDnsZones array

@description('The outgoing FirstParty Service Tag. Example: "GPCPXINT"')
param outgoingIPFirstPartyServiceTag string

// param FQDNAllowList array

var pxSubnetName = 'px'
var pxEmulatorSubnetName = 'pxEmulator'

var additionalSubnetsForApps = [
  {
    name: pxSubnetName
    addressSuffix: '6.0/26'
    delegationToServerFarms: true
  }
  {
    name: pxEmulatorSubnetName
    addressSuffix: '6.64/26'
    delegationToServerFarms: true
  }
]

module gondolinNetwork 'br:pifddev.azurecr.io/bicep/modules/gondolin/network:v1.1.1' = {
  name: 'net-${workloadName}-${location}-${stamp}'
  params: {
    networkWorkloadName: workloadName
    vnetAddressPrefix16Apps: vnetAddressPrefix16Apps
    vnetAddressPrefix16Firewall: vnetAddressPrefix16Firewall
    location: location
    shortLocation: shortLocation
    stamp: stamp
    availabilityZones: availabilityZones
    additionalSubnetsForApps: additionalSubnetsForApps
    additionalSubnetsForRedis: []
    gatewayAccessAllowAllInternetAccess: true
    gatewayAccessSourceIpAddressPrefixAllowList: []
    additionalPrivateDnsZones: additionalPrivateDnsZones
    firewallOutgoingFQDNAllowList: [
      '*'
    ]
    isMultiFirewall: false
    outgoingIPFirstPartyServiceTag: outgoingIPFirstPartyServiceTag
    useFirewallManagementSubnet: false
  }
}
