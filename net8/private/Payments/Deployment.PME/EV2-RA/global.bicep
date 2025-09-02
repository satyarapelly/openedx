param location string = az.resourceGroup().location

@maxLength(13)
param workloadName string

@minLength(2)
param appServiceKeyword string

param userAssignedMIName string

param hostName string

param originEndPoint string

var pxFrontDoorName = 'frontdoor-${workloadName}'

var pxUserAssignedIdName = userAssignedMIName

// replace the name with the managed identity used for allowlisting in downstream services
module pxUserAssignedMI 'br/public:avm/res/managed-identity/user-assigned-identity:0.2.1' = {
  name: pxUserAssignedIdName
  params: {
    location: location
    name: pxUserAssignedIdName
    lock: {
      kind: 'CanNotDelete'
    }
  }
}

var logAnalyticsWorkspaceName = 'log-${workloadName}'

module logAnalyticsWorkspace 'br:pifddev.azurecr.io/bicep/modules/loganalytics/loganalytics:v0.0.1' = {
  name: logAnalyticsWorkspaceName
  params: {
    workspaceName: logAnalyticsWorkspaceName
    location: location
  }
}

module nprsFrontDoor 'br:pifddev.azurecr.io/bicep/modules/frontdoor/frontdoor:v0.0.2' = {
  name: pxFrontDoorName
  params: {
    frontDoorName: pxFrontDoorName
    originHostHeaderCommon: hostName
    customDomain: 'edge.${hostName}'
    logAnalyticsWorkspaceId: logAnalyticsWorkspace.outputs.workspaceId
    wafPolicyMode: 'Detection'
    healthProbePath: '/probe'
    originEndpoints: [
      {
        endpoint: originEndPoint
        weight: 100
      }
    ]
    wafManagedRuleSets: [
      {
        ruleSetType: 'Microsoft_DefaultRuleSet'
        ruleSetVersion: '2.1'
        ruleSetAction: 'Block'
        ruleGroupOverrides: [
          {
            ruleGroupName: 'SQLI'
            exclusions: [
              {
                matchVariable: 'RequestCookieNames'
                selectorMatchOperator: 'Equals'
                selector: 'msresearch'
              }
            ]
          }
        ]
        exclusions: []
      }
      {
        ruleSetType: 'Microsoft_BotManagerRuleSet'
        ruleSetVersion: '1.0'
        ruleSetAction: 'Block'
        ruleGroupOverrides: []
        exclusions: []
      }
    ]
  }
}