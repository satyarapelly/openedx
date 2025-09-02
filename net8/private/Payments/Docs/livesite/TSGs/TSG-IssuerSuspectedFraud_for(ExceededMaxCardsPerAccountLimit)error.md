# TSG--Issuer Suspected Fraud for "ExceededMaxCardsPerAccountLimit" error

**Reference:** [Incident 496270763](https://icmcdn.akamaized.net/imp/v3/incidents/details/496270763/home) : [M365][APAC]Unable Add PI

**Issue** :customer could not add card due to "ExceededMaxCardsPerAccountLimit" error for such errors check below query and fetch PaymentID for the given AccountID


**Transactions**
```
| where AccountId == "1b662705-22e2-4b7b-ba32-c57c9ac4dc30"
| summarize count() by Timestamp,PaymentId, PaymentInstrumentId, AccountId
```

**And use the PaymentID to get more details of error using below Transactions**

```
|where PaymentId =="Z10084COIAQQd6261457-2bba-46f7-9d98-a5975afb93f7"
```


**Use below query to check the no of times users try to add  different card within certain time limits.**

```
RequestTelemetry
| where TIMESTAMP > ago(30d)
| where (name == "Microsoft.Commerce.Tracing.Sll.PXServiceIncomingOperation" or name == "Microsoft.Commerce.Tracing.Sll.PXServiceOutgoingOperation")
| extend res = parse_json(data_ResponseDetails)
| extend error = res['ErrorCode']
| where  data_baseData_targetUri contains "1b662705-22e2-4b7b-ba32-c57c9ac4dc30"
| project res['ErrorCode'],TIMESTAMP,cV, name, data_baseData_callerName,  data_baseData_operationName,data_correlationId , data_baseData_protocolStatusCode,data_baseData_dependencyName,data_baseData_dependencyOperationName, data_baseData_latencyMs, data_baseData_targetUri, data_RequestDetails,data_ResponseDetails , data_ResponseHeader,data_RequestHeader, data_AccountId, data_Partner, data_Country,data_faultDetail
```