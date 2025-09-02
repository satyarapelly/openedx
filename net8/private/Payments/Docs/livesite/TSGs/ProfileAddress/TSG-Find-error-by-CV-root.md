# TSG: Find error by CV root

CV root is the part highlight in yellow. The part before the first dot.

**CV sample**:<span style="background-color:yellow"> </span>`MZr9X8weGExX+powjhJIXd`.9

The following query will show client side error and server failures regarding to  a CV.

`let startTime = datetime(2022-08-18 01:00:00);`

`let endTime = datetime(2022-08-19 23:00:00);`

`let cVRoot = "MZr9X8weGExX+powjhJIXd";`

`let clientErrors = GetPidlSdkFailureDetails(startTime, endTime, "", "", "")`

`| project PidlSdkOperationId, IngestTime, CV, PartnerName, Market, PidlResourceName, ErrorType, ErrorCode, EventParameters, UserId, ClientIP, BrowserDomain, SdkVersion`

`|where CV startswith cVRoot;`

`let serverFailures = RequestTelemetry`

`| where TIMESTAMP  between (startTime..endTime)`

`| where cV startswith cVRoot and data_baseData_protocolStatusCode !in ("200", "OK", "True")`

`| project TIMESTAMP,cV, data_baseData_latencyMs, ext_cloud_role, data_baseData_protocolStatusCode, data_baseData_operationName, data_RequestDetails, data_ResponseDetails, data_baseData_targetUri, name;`

`serverFailures;`

`clientErrors;`