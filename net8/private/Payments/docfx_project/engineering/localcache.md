# Enable local cache in PX

## Target Audience
PX Engineering

## Background
We hit [Incident 244674370](https://portal.microsofticm.com/imp/v3/incidents/details/244674370/home) : AppService brought servers in PXService-CentralUS online with missing app files. Here are details coped from [Postmortem 505326](https://portal.microsofticm.com/imp/v3/incidents/postmortem/505326):

Per the app Service team who joined the bridge and investigated this, files under the App_Data folder are not really local files.  Instead, they get moved to a network share and that share gets mapped to the local server.  Also, server disk operations (like ReadFile, CreateFile etc.) gets intercepted by App Service to make these files appear local (even though they are on a share) to the service.

During this incident, the app service team was performing maintenance on the network share that backs the App_Data folder. App service team knows about an issue where the App_Data folder becomes read-only for about 5 minutes.  During this maintenance, PX service seems to have been restarted during this window.  Per the App Service team, if a file handle is opened when it's Read-only, those file handles will become invalid after the share transitions to Read-write mode.  This most likely caused the PX service to fail loading files under the App_Data folder.

Per app service team suggestion, we should consider switching from dynamic cache to [local cache](https://docs.microsoft.com/en-us/azure/app-service/overview-local-cache) to avoid hitting the issue. 

## Exception in PX during the impacted window
- TypeInitializationException: Azure app service has  monthly storage upgrade. During upgrade, app service restarted our apps, restart fails to initialize PidlFactory due to not loading part of .csv files during [Incident 244674370](https://portal.microsofticm.com/imp/v3/incidents/details/244674370/home) 
```
System.TypeInitializationException: The type initializer for 'Microsoft.Commerce.Payments.PidlFactory.V7.PIDLResourceDisplayHintFactory' threw an exception. ---> System.IO.IOException: An unexpected network error occurred.
```

## Contacts
- Azure web apps (Antares) discussions antr-talk@microsoft.com 
- Azure web apps team joined investigation and helped us understand the issue
  - kranthim@microsoft.com
  - mikono@microsoft.com
  - phil.luo@microsoft.com