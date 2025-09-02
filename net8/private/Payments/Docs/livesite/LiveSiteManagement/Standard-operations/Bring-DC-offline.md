# Bring DC offline

## Disable a Region

If it is your first time to disable a region, please engage secondary with you to do it together

- Use PME account to login [PX ATM](https://portal.azure.com/#@mspmecloud.onmicrosoft.com/resource/subscriptions/9b6168fd-7d68-47e1-9c71-e51828aa62c0/resourceGroups/PX-Services-PROD-TM/providers/Microsoft.Network/trafficmanagerprofiles/paymentexperience-cp/overview)
- Pick the region having issue
- Update "Status" with "Disable"
- Click "Save" button

![](/images/livesite/1-25f5c5a0cbf14d72b49b318336fe5cf7.png)
After disabling the DC, make sure there is no traffic flowing in by going to:

[Loading (microsoftgeneva.com)](https://portal.microsoftgeneva.com/dashboard/paymentexperience-metrics-prod/Service%2520QoS?overrides=[{%22query%22:%22//*[id%3D%27OperationName%27]%22,%22key%22:%22value%22,%22replacement%22:%22%22},{%22query%22:%22//*[id%3D%27CloudLocation%27]%22,%22key%22:%22value%22,%22replacement%22:%22%22},{%22query%22:%22//*[id%3D%27CloudRole%27]%22,%22key%22:%22value%22,%22replacement%22:%22%22},{%22query%22:%22//*[id%3D%27RoleInstance%27]%22,%22key%22:%22value%22,%22replacement%22:%22%22},{%22query%22:%22//*[id%3D%27CloudRoleInstance%27]%22,%22key%22:%22value%22,%22replacement%22:%22%22},{%22query%22:%22//*[id%3D%27CallerName%27]%22,%22key%22:%22value%22,%22replacement%22:%22%22}])

![](/images/livesite/1-70a80d5094fa44c68063ea3fe945eeaf.png)

Release pipeline link: [Release Progress (visualstudio.com)](https://microsoft.visualstudio.com/Universal%20Store/_releaseProgress?_a=release-pipeline-progress&amp;releaseId=2598590)

Monitor DC traffic: [PX Reliablity Dashboard - Dashboard - Lens Explorer (msftcloudes.com)](https://lens.msftcloudes.com/#/dashboard/5dc09589-664c-4f9e-b41e-02bb73b36eab?temp=0&amp;isSample=false&amp;_g=%28ws:e377c266-06a6-4858-9d50-4e24c8d098ed%29)