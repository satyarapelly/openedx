# [Draft] Reliability of PIFD GET API - 1D < 99.99% (actual 99.9%)

we can see  some latency issues when it comes to the StoresValueService call. we believe that increasing the amount of latency time to ~20 seconds will improve the reliability for  "GET\_users/{userId}/paymentInstrumentsEx/{id}/redeem\_v6.0\_{}" API call.

    We can check errors using below Kusto

```
PIFDIncomingSllFailures
| where Timestamp > ago(1d) and OperationName startswith "GET_users/{userId}/paymentInstrumentsEx/"
| where ProtocolStatusCode >= 500
| project Timestamp, CV, OperationName, ProtocolStatusCode, ErrorCode, ErrorMessage, LatencyMs
| summarize count() by OperationName
```

![](/.attachments/1-608931824b354105887239404037fe3b.png)

```
Getting below errors after executing below query with latest CV
PidlSdkTelemetryEvents
|where CV startswith "oBEmbrGS+oiQ6ooCjuaKZL"
|where OSRProtocolStatusCode >=500
```

    `"message": "{\"code\":\"InternalServerError\",\"data\":[],\"details\":[],\"innererror\":{\"code\":`
    `\"UnknownInternalFailure\",\"data\":[],\"details\":[],\`
    `"message\":\"The service has encountered an unknown internal server`
    `error: Microsoft.MarketplaceServices.PaymentInstrumentService.PaymentInstrumentFD.SanitizedException:`

```
Error calling PXService from PaymentInstrumentFD\"},\
"message\":\"An internal server error occurred while processing the operation.\",\
```

    `"source\":\"PaymentInstrumentFD\"}"`

    **Task:**

- [Task 41689436: StoredValueService 404 errors - Boards (azure.com)](https://dev.azure.com/microsoft/OSGS/_workitems/edit/41689436)
- [Task 41700137: StoredValueService Timeout failures - Boards (visualstudio.com)](https://microsoft.visualstudio.com/OSGS/_workitems/edit/41700137)

**Example:**

- [Incident 337779968](https://portal.microsofticm.com/imp/v3/incidents/details/337779968/home) : (For Quarantined API - PX controllers) Reliability of PIFD GET API - 1D &lt; 99.99% (actual 99.9%): Role: PIFDWebApp GET\_users/{userId}/paymentInstrumentsEx/{id}/redeem\_v6.0\_{}
- [Incident 341489776](https://portal.microsofticm.com/imp/v3/incidents/details/341489776/home) : (For Quarantined API - PX controllers) Reliability of PIFD GET API - 1D &lt; 99.99% (actual 99.9%): Role: PIFDWebApp GET\_users/{userId}/paymentInstrumentsEx/{id}/redeem\_v6.0\_{}