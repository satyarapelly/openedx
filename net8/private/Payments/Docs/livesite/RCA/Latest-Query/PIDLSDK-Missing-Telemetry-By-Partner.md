# PIDLSDK Missing Telemetry By Partner

`let startDateTime = datetime("2022-07-07 00:00:00");`

`let endDateTime = datetime_add('day', 10, startDateTime);`

`    let PXCVs =`

```
RequestTelemetry
```

`| where TIMESTAMP > startDateTime and TIMESTAMP < endDateTime`

`and name == "Microsoft.Commerce.Tracing.Sll.PXServiceIncomingOperation"`

`and SourceNamespace == "paymentexperiencelogsprod"`

`and data_baseData_operationName == "PaymentMethodDescriptionsController-GET"`

`and data_baseData_targetUri !contains "partner=xboxsettings" and data_baseData_targetUri !contains "partner=storify"and data_baseData_targetUri !contains "partner=xboxsubs"`

`and data_RequestHeader contains "x-ms-pidlsdk-version"`

`//and data_baseData_targetUri !contains "storify" and data_baseData_targetUri !contains "xbox"`

`//and data_AccountId !in ("5662f816-97b9-4c2f-9b28-0c09d2c15a1e", "097a070b-603c-4ad1-bc63-2d9b7f111c11", "8e342cdc-771b-4b19-84a0-bef4c44911f7", "876f8839-aae6-45a0-b7f3-679e5385035a")`

`| extend TargetUri = parse_url(data_baseData_targetUri)`

`     | extend ServerPartner = tolower(tostring(TargetUri.["Query Parameters"].partner)), ServerOperationName = tolower(tostring(TargetUri.["Query Parameters"].operation))`

`| project cvBase = GetCVBase(cV), data_baseData_targetUri, ServerIP = data_baseData_callerIpAddress, data_referrer, data_Origin, ServerOperationName, ServerPartner, TIMESTAMP`

`, PIDLSDKVersion = extract("Key\":\"x-ms-pidlsdk-version\",\"Value\":\"(.+?)\"", 1, data_RequestHeader)`

`| summarize count() by cvBase, PIDLSDKVersion, ServerPartner//, TimeDay = bin(TIMESTAMP, 1d) //, ServerPartner//, ServerOperationName, JoinCol = strcat(cvBase, "_", ServerPartner, "_", ServerOperationName)`

```
;
```

`    let PIDLSDKCVs =`

`    PidlSdkTelemetryEvents`

`| where IngestTime > datetime_add('hour', -24, startDateTime) and IngestTime < datetime_add('hour', 24, endDateTime)`

`| where Environment =~ "prod"`

`| where name == "Ms.Webi.OutgoingRequest"`

`| extend OSRUri = parse_url(OSRTargetUri)`

`| where OSRUri.Path contains "paymentMethodDescriptions"`

`| extend ClientPartnerName = tolower(tostring(OSRUri.["Query Parameters"].partner)), ClientOperationName = tolower(tostring(OSRUri.["Query Parameters"].operation))`

`| project ClientPartnerName, CV, BrowserDomain, SdkVersion, SdkViewType, ClientOperationName, ServerIP = ClientIP, IngestTime`

`| extend cvBase = GetCVBase(CV)`

`//| where ClientOperationName == "selectinstance" and ClientPartnerName == "azure"`

`| summarize count() by cvBase//, TimeDay = bin(IngestTime, 1d) //, ServerPartner, ServerOperationName, JoinCol = strcat(cvBase, "_", ServerPartner, "_", ServerOperationName)`

```
;
```

`let results =`

`PXCVs`

`| join kind=leftouter PIDLSDKCVs on cvBase//, TimeDay`

`| summarize TotalCount = dcount(cvBase), HasOnlyServiceTelemetry = dcountif(cvBase, cvBase1 == "") by PIDLSDKVersion, ServerPartner`

`| extend MissingPercent = round((HasOnlyServiceTelemetry*100.0)/TotalCount, 2)`

```
;

results
```

`|where TotalCount > 1000;`