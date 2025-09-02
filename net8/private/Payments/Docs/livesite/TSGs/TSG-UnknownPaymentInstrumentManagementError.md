# TSG:UnknownPaymentInstrumentManagementError

Reference icms:[Incident 399180832](https://icmcdn.akamaized.net/imp/v3/incidents/details/399180832/home) : Reliability of PIFD PX API (Hourly 99.75%|50|20k, Daily 99.8%|150|400k) Role: PIFDWebApp - GET_users/{userId}/paymentMethodDescriptions_v6.0_{}--------------- UnknownPaymentInstrumentManagementError(PidlService)

  [Incident 399469127](https://icmcdn.akamaized.net/imp/v3/incidents/details/399469127/home) : [Scenario Health Alert] Office Commercial - Add or update payment method 

**Query:**
```
cluster("PST").database("Prod").RequestTelemetry
 //| where (name == "Microsoft.Commerce.Tracing.Sll.PXServiceIncomingOperation" or name == "Microsoft.Commerce.Tracing.Sll.PXServiceOutgoingOperation")
 | where TIMESTAMP > ago(30d)
 | where cV startswith "em0fbvQ1DwwYBtEGrQfZGU.0.6.2"
 | extend url = parse_url(data_baseData_targetUri)
 | extend partnerName = tolower(url["Query Parameters"].partner)
 | extend state = parse_json(data_ResponseDetails).state
 //| project TIMESTAMP,cV, name, data_baseData_callerName,  data_baseData_operationName, data_baseData_protocolStatusCode,data_baseData_dependencyName,data_baseData_dependencyOperationName, data_baseData_latencyMs, data_baseData_targetUri, data_RequestDetails,data_ResponseDetails , data_ResponseHeader,data_RequestHeader, data_AccountId, data_Partner, data_Country, partnerName
 | top 10 by TIMESTAMP
```
**To Check the Treand**
```
RequestTelemetry
| where TIMESTAMP > ago(30d)
| where data_faultDetail contains "ModernCPPaymentInstruments" 
| where data_baseData_dependencyOperationName == "GET_payment-instruments-ex"
| summarize count() by ext_cloud_location, bin(TIMESTAMP, 1h)
| render timechart 
```

**ShieldTeam Investigation.**

We have observed the spike related to “UnknownPaymentInstrumentManagementError” for operation “GET_users/{userId}/paymentInstrumentsEx/{id}_v6.0_{}” in South East Asia(SEA) region, During Investigation we analyzed that PX don’t have any incomings call for this.

It Seems like PIFD call to PX but we didn’t see any logs capture by PX. It’s an infra issue between PIFD and PX. So, engaged PIMS and PX OnCall to find out the root cause of the issue.

**After checking from PX(oncall) side:**

• PX Team identified the issue is due to some Unicode / Japanese characters in the referrer header i.e., in the signup.microsoft.com URL, which is being passed down from PIFD to PX in the x-ms-riskinfo header.

• PX service / IIS are somehow treating these characters to be badly formatted and returning the **Bad Request failure.**

• The change to send referrer header in x-ms-riskinfo was recently deployed to PIFD [(Commit eb06338a)](https://nam06.safelinks.protection.outlook.com/?url=https%3A%2F%2Fmicrosoft.visualstudio.com%2FUniversal%2520Store%2F_git%2FSC.csPayments.PaymentInstrumentFrontDoor%2Fcommit%2Feb06338aa413acbaecb667d064b198bab44e9ae1%3FrefName%3Drefs%2Fheads%2Fmaster&data=05%7C01%7Cv-ranjithau%40microsoft.com%7C30c2f2b220074d4f7ea408db76cceee7%7C72f988bf86f141af91ab2d7cd011db47%7C1%7C0%7C638234394829202530%7CUnknown%7CTWFpbGZsb3d8eyJWIjoiMC4wLjAwMDAiLCJQIjoiV2luMzIiLCJBTiI6Ik1haWwiLCJXVCI6Mn0%3D%7C3000%7C%7C%7C&sdata=jcsywoJzO6XUU3XfCnTekgxeyazFcxrF%2FjLS6I9GJZs%3D&reserved=0) on 6/7, which correlates with the timeframe when these errors started happening.

**Once the issue is identified by PX, PIFD has done the deployment for the changes.**

        • PIMS team roll back the change and deployed new bit to Prod in 6/22. SEA has completed the deployment.


**Note:** Always downgrade icm severity from 3 to 4 before transferring to Risk team unless it requires a bridge call meeting
