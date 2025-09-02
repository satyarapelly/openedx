#  TSG: ClientErrorMessageAlert

**1.PidlErrorOnTaxIdUpdate**

**Rerference:**

• [Incident 424579733](https://icmcdn.akamaized.net/imp/v3/incidents/details/424579733/home) : [ClientErrorMessageAlert] [PORTAL.AZURE.COM] [All Stages] [All Datacenters] [Area Microsoft_Azure_GTM] [Code 1002] [Microsoft_Azure_GTM] For the last 60 minutes, 1 users have had 1 client errors that contain [[Tier2 - BMX][BillingAccountEditTaxIdBlade]PidlErrorOnTaxIdUpdate] error string(s).

• [Incident 401316653](https://icmcdn.akamaized.net/imp/v3/incidents/details/401316653/home) : [ClientErrorMessageAlert] [PORTAL.AZURE.COM] [All Stages] [All Datacenters] [Area Microsoft_Azure_GTM] [Code 1002] [Microsoft_Azure_GTM] For the last 60 minutes, 1 users have had 1 client errors that contain [[Tier2 - BMX][BillingAccountEditTaxIdBlade]PidlErrorOnTaxIdUpdate] error string(s).

• [Incident 431527782](https://icmcdn.akamaized.net/imp/v3/incidents/details/431527782/home) : [ClientErrorMessageAlert] [PORTAL.AZURE.COM] [All Stages] [All Datacenters] [Area Microsoft_Azure_GTM] [Code 1002] [Microsoft_Azure_GTM] For the last 60 minutes, 3 users have had 5 client errors that contain [[Tier2 - BMX][BillingAccountEditTaxIdBlade]PidlErrorOnTaxIdUpdate] error string(s).

**Investigation:**
   
    I. Review the entire summary and discussion to understand the concept or the cause of the incident. 
    II. Utilize the Request Telemetry table with details like CV, using below query 
    III. After executing, examine all error logs.

**Check cv with  With below**

**PidlSdkTelemetryEvents**

**| where CV startswith "HxZ0uYLB5qHAdnSLwCAa+2.3"**
 


**After investigation :**

identify error and highlight the same if url is "url": "https://commerceapi.office.net/v1/me/taxids?accountId=cc780533-f1e5-4ab7-9996-e4990a991941&organizationId=abd19c35-de21-450e-b2d6-cdfcacf3f9ba_2019-05-31",

make a note of errorcode and transfer to Demeter-CommerceApi/CXG Purchase along with regex detail as seen in reference icm.

**2.PIDL_Error**
```
Query:
Execute: [Web] [Desktop] [Web (Lens)] [Desktop (SAW)] https://pst.kusto.windows.net/Prod
PidlSdkTelemetryEvents
| where EventTime > ago(30d)
| where EventParameters.error.name == "mandatoryInputParameterMissingOrNull"
| where PartnerName == "Azure"
| summarize count() by PartnerName, bin(EventTime,1d)
| render timechart
```
