# Enable auto heal in PX

## Target Audience
PX Engineering

## Background
We hit 
- [Incident 244674370](https://portal.microsofticm.com/imp/v3/incidents/details/244674370/home) : AppService brought servers in PXService-CentralUS online with missing app files
- [Incident 210457291](https://portal.microsofticm.com/imp/v3/incidents/details/210457291/home) : CreateAndAuthenticate and Get PaymentMethodDescription API Reliablity Drop to 99.5% for the Past 10 Minutes

The incident is mitigated after the impacted data center app restarts. Per azure app service suggestion, we should enable [auto heal](https://azure.github.io/AppService/2018/09/10/Announcing-the-New-Auto-Healing-Experience-in-App-Service-Diagnostics.html) to reduce incident impact.

## Exception in PX during the impacted window
- TypeInitializationException: Azure app service has  monthly storage upgrade. During upgrade, app service restarted our apps, restart fails to initialize PidlFactory due to not loading part of .csv files during [Incident 244674370](https://portal.microsofticm.com/imp/v3/incidents/details/244674370/home) 
```
System.TypeInitializationException: The type initializer for 'Microsoft.Commerce.Payments.PidlFactory.V7.PIDLResourceDisplayHintFactory' threw an exception. ---> System.IO.IOException: An unexpected network error occurred.
```
- MissingMethodException: So far we only hit the issue, after our deployment, app service fails to load latest dll. The issue has been fixed in our deployment pipeline by [Commit cd37a7c6](https://microsoft.visualstudio.com/Universal%20Store/_git/SC.CSPayments.PX/commit/cd37a7c6ad67037ac7da1ed7eff3440d714457be?refName=refs%2Fheads%2Fengineering) which will restart the app after each deployment before bringing the app online.
```
Microsoft.Commerce.Payments.PXService.V7.PaymentMethodDescriptionsController.<GetByFamilyAndTypeWithCompletePrerequisitesOption>d__3.MoveNext(), and the missing method 'System.Collections.Generic.List`1<Microsoft.Commerce.Payments.PidlModel.V7.PIDLResource> Microsoft.Commerce.Payments.PidlFactory.V7.PIDLResourceFactory.GetAddressDescriptions(System.String, System.String, System.String, System.String, System.String, Boolean, System.String, System.Collections.Generic.List`1<System.String>, System.String).
```
In both cases, restarting the app service will mitigate the issue. Auto heal can restart the unhealthy instances to reduce the impact.

Since the issue MissingMethodException has been fixed in the deployment pipeline by [Commit cd37a7c6](https://microsoft.visualstudio.com/Universal%20Store/_git/SC.CSPayments.PX/commit/cd37a7c6ad67037ac7da1ed7eff3440d714457be?refName=refs%2Fheads%2Fengineering), 
auto heal feature will only be triggered by TypeInitializationException for now.

## Auto heal concept
We need to specify a rule to let app service identity failures. Once rules are met, app service will restart instances for us. 

For instance, the following ARM configuration means after app restart 10 mins, if there are 15 503s returned by one app service instance within 5 mins, app service will restart the instance

[!code-json[auto heal rule in int](../../Deployment.PME/INT/Parameters/PXService.SiteSlot.Parameters.INT.json?start=47&end=69)]

In case, the json code snippet doesn't show correctly, here is a copy.
```
 "autoHealRules": {
      "value": {
        "triggers": {
          // auto heal will be triggered when there are 15 503 returns with 5 mins per instance
          "statusCodes": [
            {
              "status": 503,
              "count": 15,
              "timeInterval": "00:05:00"
            }
          ]
        },
        "actions": {
          // restart the instance
          "actionType": "Recycle",
          // Mapping to the start up time in the UI. By modifying it, we can specify how much time the mitigation rule should wait after the process startup before the mitigation rule kicks off.
          "minProcessExecutionTime": "00:10:00"
        }
      }
    }
```
Per [app service diagnostic tool recommendation](https://github.com/Azure/Azure-AppServices-Diagnostics-Portal/blob/main/AngularApp/projects/app-service-diagnostics/src/app/auto-healing/autohealing.component.ts) minProcessExecutionTime should be at least 10 minutes

```
To avoid mitigation actions to kick in immediately after the app starts, it is advisable to set the startup time to at least 600 seconds (10 minutes). This will ensure that mitigation actions don't kick in during app's cold start.
```

## The Fix
1.  PX returns 502 when downstream service return 503 (ServiceUnavailable)  (In the engineering region, need move back to master)
[Commit b8320bd4](https://microsoft.visualstudio.com/Universal%20Store/_git/SC.CSPayments.PX/commit/b8320bd430c78fb08629a3accf29d4cbfcf9dd70?refName=refs%2Fheads%2Fengineering): Merged PR 6506879: Return 502 instead of 503 when PX service is still available while downstream services are unavailable

2. PX returns 503 (ServiceUnavailable) when we hit TypeInitializationException and hook up azure app service autoheal to recycle the instance.
[Pull Request 6586640](https://microsoft.visualstudio.com/Universal%20Store/_git/SC.CSPayments.PX/pullrequest/6586640): Enable auto heal for PX service INT, PPE and PROD engineering region

## Manual Test
Since the failures are triggered by azure app service, we can't create a test case to trigger it. Therefore we did a manual test as below.
1. We instrumented the following code generates [TypeInitializationException](https:////docs.microsoft.com/en-us/dotnet/api/system.typeinitializationexception?view=net-5.0) in the PXServiceAuthorizationFilterAttribute.cs to trigger failure. 
2. Deployed the build to PX engineering INT and ran COTs. All the COTs expected to fail.
3. Verify in the [applens](https://applens.azurewebsites.net/subscriptions/230ef3cc-8fdd-4f26-bf9c-10131b4080e5/resourceGroups/PX-Services-INT-WestUS/providers/Microsoft.Web/sites/PaymentExperience-INT-westus/detectors/AutoHeal?startTime=2021-10-19T17:39&endTime=2021-10-20T17:23), the auto heal is triggered.

```
var testClass = new TypeInitializationExceptionExample();
testClass.GetName();
public class TypeInitializationExceptionExample
{
    private static InnerBug bug = new InnerBug(-3);
    private string name = "generate TypeInitializationException";
    public string GetName()
    {
        return bug.Value + this.name;
    }
}

public class InnerBug
{
    public readonly int Value;

    public InnerBug(int value)
    {
        if (value < 0)
        {
            throw new System.ArgumentOutOfRangeException("value");
        }
        this.Value = value;
    }
}
```

## Rollout plan
- Engineering Region （1 week）
- West US 2 and South Central US (1 week)
- Central US (1 month)
- East US and West US

We only hit the issues in central us. In case, we find any issue during Central US flighting, we can failover to another 2 DC East US and West US

## Additional thoughts
### How to quickly disable auto heal in case it doesn't work as expected?
 1. *To be verify* (Enable auto heal in the arm template -> Disable the configuration in portal -> restart app -> will auto heal keep disabled)
 2. when it doesn't fully roll out all the DC, we can always failover to the DC which doesn't have it
 ### How to monitor auto heal?
 need add




