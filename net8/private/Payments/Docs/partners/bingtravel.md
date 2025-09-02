
# Bing Travel
Refers to Bing travel

## Target audience
PX Engineering and Payments PM teams

## Contacts
- PM: nikunjdaga
- Dev: abhinavreddy
- PM manager: gokul.singh
- Dev manager: goyalpankaj

## Team
- Service tree: - 
- ICM team: [Travel UX](https://portal.microsofticm.com/imp/v3/administration/teamdashboard/details?id=78646)
- Email alias for oncall: traveluxdri@microsoft.com
- Email alias for support: 

### Rollout Timeline: 
-

### UI surfaces: 
[<!--e.g. During Windows setup, on https://signup.microsoft.com/, on xbox console or so-->]()
Bing travel will be using PX's Select PI, Add PI and handle CVV challenge when user enter checkout page to book hotels/flights in bing travel

### Traffic estimation: 
 .2 RPS

### Code repos: 
https://msasg.visualstudio.com/Falcon/_git/TravelHubUX

### Feature flag control: 
Yes - This feature will be flighted along with hotels/flights booking page.
Will update flight details once the pages are ready.

### How Pidlsdk is used?
Javascript PIDL

### Tracking on Kusto: 
- Service side tracking via RequestTelemetry
- Client side tracking via PidlSdkTelemetryEvents
- Sample, where the string in EventParameters is the piid 
```
PidlSdkTelemetryEvents
| where PartnerName contains "bingtravel"
| where CV contains "[CV]"
| where EventParameters contains "[piid]"
| limit 10
```

<div style="position:fixed; bottom:30px">

For questions/clarifications, email [author/s of this doc and PXSupport](mailto:traveluxwg@microsoft.com?subject=Bing%20travel%20Partner%20Documentatio%20in%20PX%20Repo).

</div>