# TSG: How to investigate: SanitizedException in PIFD

[Incident 329792334](https://portal.microsofticm.com/imp/v3/incidents/details/329792334/home) : Help investigating 500's from PIDL

**Summary**:

commercialstores observe 500s in profile description API.

The call stack: commercialstores (500) -&gt; PIDLSDK (500)-&gt; PIFD(500) -&gt; PX (200)

On the PIFD side, there are 2 cases we know SanitizedException will returns

1. Network error between PIFD and PX
2. Exceed max timeout: normally 8 secs
    1. [PidlClientV7.cs - Repos (visualstudio.com)](https://microsoft.visualstudio.com/Universal%20Store/_git/SC.csPayments.PaymentInstrumentFrontDoor?path=/private/InstrumentManagement/PaymentInstrumentService/Clients/PIDL/V7/PidlClientV7.cs&amp;_a=contents&amp;version=GBmaster)
    2. [PXClientV7.cs - Repos (visualstudio.com)](https://microsoft.visualstudio.com/Universal%20Store/_git/SC.csPayments.PaymentInstrumentFrontDoor?path=/private/InstrumentManagement/PaymentInstrumentService/Clients/PX/V7/PXClientV7.cs&amp;_a=contents&amp;version=GBmaster)

**Investigation:**

- **Locate the error by CV root**

`let startTime = datetime(2022-09-15);`

`let endTime = datetime(2022-09-30);`

`let cVRoot = "AzjUmJoz1uLh7lZx";`

`let clientErrors = GetPidlSdkFailureDetails(startTime, endTime, "", "", "")`

`| project PidlSdkOperationId, IngestTime, CV, PartnerName, Market, PidlResourceName, ErrorType, ErrorCode, EventParameters, UserId, ClientIP, BrowserDomain, SdkVersion`

`|where CV startswith cVRoot;`

`let serverRequests = RequestTelemetry`

`| where TIMESTAMP  between (startTime..endTime)`

`| where cV startswith cVRoot`

`| project TIMESTAMP,cV, data_baseData_latencyMs, ext_cloud_role, data_baseData_protocolStatusCode, data_baseData_operationName, data_RequestDetails, data_ResponseDetails, data_baseData_targetUri, name;`

`serverRequests;`

`clientErrors;`

![](/images/livesite/1-bb85276110f746909a76cc7e3eb8370f.png)

- **Understand the overall impact: 7 customers over 30 days**

**PIFD query to get all the failures on profile with SanitizedException on pidlService**

`let startTime = datetime(2022-08-30);`

`let endTime = datetime(2022-09-30);`

```
RequestTelemetry
```

`| where TIMESTAMP  between (startTime..endTime)`

`and data_baseData_operationName == strcat("GET_users/{","userId}/profileDescriptions_v6.0_{}")`

`| where data_baseData_protocolStatusCode == "InternalServerError" and data_faultDetail contains "aymentInstrumentFD.SanitizedException: Error calling PidlService from PaymentInstrumentFD"`

`| where ext_cloud_role !contains "ppe"`

`| project TIMESTAMP,cV, data_baseData_latencyMs, data_users, ext_cloud_role, data_baseData_protocolStatusCode, data_faultDetail, data_baseData_operationName, data_RequestDetails, data_ResponseDetails, data_baseData_targetUri`

`| where isnotempty(data_users)`

`| summarize count() by data_users`

![](/images/livesite/1-ee34ff98519742c7ba4ebca580aeadf2.png)

**Potential fix**:

- Including ProfileDescriptions in the PIDLSDK retry too
- Consider increase latency 8s to 16s