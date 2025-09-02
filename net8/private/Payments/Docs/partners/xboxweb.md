
# XboxWeb
Refers to Xbox Marketpace
<!-- 
Replace "Partner Name" with real partner name e.g. SMB 
Can you also add a brief introduction of your team? e.g. SMB stands for Small Medium Business. SMB helps small medium businesses to easily purchase any M365 product.
-->

## Target audience
PX Engineering and Payments PM teams
<!-- 
Select one the following:

PX Engineering team
PX Engineering and Payments PM teams
Developers and PMs contributing code or docs to PX service
-->

## Contacts
<!--Please help us collect the following information if available-->
- PM: miranda
- Dev: paul.carranzacambronero@microsoft.com
- PM manager: miranda
- Dev manager: avinashu

## Team
<!--Please help us collect the following information if available-->
- Service tree: https://servicetree.msftcloudes.com/?pathSet=true#/ServiceModel/Service/Profile/c190f9ba-a5e8-4949-bd7b-76d1d12a12e0?UpdateNav=false <!--Please put your service tree link here [your service tree name](link)-->
- ICM team: https://portal.microsofticm.com/imp/v3/administration/teamdashboard/details?id=69111 <!--Please put your icm team link here[you icm team name](link)-->
- Email alias for oncall: xbetexppd@microsoft.com
- Email alias for support: 

### Rollout Timeline: 
End of February

### UI surfaces: 
[<!--e.g. During Windows setup, on https://signup.microsoft.com/, on xbox console or so-->]()
Marketplace will be using PX's ValidateAddress API while initiating purchase. 

### Traffic estimation: 
 .2 RPS
<!--Can you share with us your rough TPS?-->
### Code repos: 
https://microsoft.visualstudio.com/DefaultCollection/Universal%20Store/_git/XBC.Service.Web.Live
<!--please put your repo link here which leverages pidlsdk [your repo name](link)-->
<!-- ### Dashboard:  -->
<!--Can you share with us your main dashboard link? [your dashboard name](link)-->
<!-- ### SLA: -->
<!--Can you share what is your SLA for Sev1, Sev2 and Sev3?-->
### Feature flag control: 
No flighting on partner side
<!--If Yes, can you share more detail? eg. azure app config, azure exp or so-->
<!-- ### Test portal:  -->
<!--Do you have any test portal to allow us to test end to end? Any guidance?-->
### How Pidlsdk is used?
Javascript PIDL
<!--e.g. react with office fabric element factory-->
### Tracking on Kusto: 
- Service side tracking via RequestTelemetry
- Client side tracking via PidlSdkTelemetryEvents
- Sample, where the string in EventParameters is the piid 
```
PidlSdkTelemetryEvents
| where PartnerName contains "xboxweb"
| where CV contains "[CV]"
| where EventParameters contains "[piid]"
| limit 10
```
<!-- 
Optionally, list references to other docs that the reader may want to read next
--> 

<div style="position:fixed; bottom:30px">

>For questions/clarifications, email [author/s of this doc and PXSupport](mailto:PXSupport@microsoft.com?subject=Xboxweb%20Partner%20Documentatio%20in%20PX%20Repo).
</div>