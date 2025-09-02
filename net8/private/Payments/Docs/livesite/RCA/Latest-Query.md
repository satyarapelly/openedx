# Latest Query

`let startDateTime = datetime("2022-07-08 00:00:00");`

`let endDateTime = datetime_add('hour', 240, startDateTime);`

`     let PIFDCVs =`

```
RequestTelemetry
```

`| where TIMESTAMP > startDateTime and TIMESTAMP < endDateTime`

`and name == "Microsoft.MarketplaceServices.PaymentInstrumentService.Logging.IncomingServiceRequest"`

`and data_baseData_targetUri startswith "`https://paymentinstruments.mp.microsoft.com/`" and data_baseData_targetUri contains "/paymentMethodDescriptions?" and data_baseData_requestMethod != "OPTIONS" and data_baseData_targetUri !contains "ApplicationInsights"`

`| extend TargetUri = parse_url(data_baseData_targetUri)`

`| extend ServerPartner = tolower(iif(data_Partner != "", data_Partner, tostring(TargetUri.["Query Parameters"].partner))), ServerOperationName = tolower(tostring(TargetUri.["Query Parameters"].operation))`

`| extend ReferrerHost = tolower(tostring(parse_url(data_referrer).Host))`

`| where ServerPartner !in ("xboxsettings", "storify", "xboxsubs", "setupofficesdx", "amcxbox", "consumersupport", "commercialsupport", "oxowebdirect", "payin")`

`| where ReferrerHost !in ("localhost", "pidlsdktestportal.azurewebsites.net", "portal.office.com")`

`| project  cvBase = GetCVBase(cV), data_baseData_targetUri, ServerIP = data_baseData_callerIpAddress, data_referrer, ReferrerHost, data_Origin, ServerOperationName, ServerPartner, TimeDay = bin(TIMESTAMP, 1d)`

`| summarize count() by cvBase, ServerPartner, ServerOperationName, JoinCol = strcat(cvBase, "_", ServerPartner, "_", ServerOperationName), TimeDay, ReferrerHost`

`//| take 10`

```
;
```

`let PXCVs =`

```
RequestTelemetry
```

`| where TIMESTAMP > startDateTime and TIMESTAMP < endDateTime`

`and name == "Microsoft.Commerce.Tracing.Sll.PXServiceIncomingOperation"`

`and SourceNamespace == "paymentexperiencelogsprod"`

`and data_baseData_operationName == "PaymentMethodDescriptionsController-GET"`

`//and data_RequestHeader contains "x-ms-pidlsdk-version"`

`//and data_baseData_targetUri !contains "storify" and data_baseData_targetUri !contains "xbox"`

`and data_AccountId !in ("5662f816-97b9-4c2f-9b28-0c09d2c15a1e", "097a070b-603c-4ad1-bc63-2d9b7f111c11", "8e342cdc-771b-4b19-84a0-bef4c44911f7", "876f8839-aae6-45a0-b7f3-679e5385035a")`

`| extend TargetUri = parse_url(data_baseData_targetUri)`

`    | extend ServerPartner = tolower(tostring(TargetUri.["Query Parameters"].partner)), ServerOperationName = tolower(tostring(TargetUri.["Query Parameters"].operation))`

`| project cvBase = GetCVBase(cV), data_baseData_targetUri, ServerIP = data_baseData_callerIpAddress, data_referrer, data_Origin, ServerOperationName, ServerPartner`

`| where ServerPartner !in ("xboxsettings", "storify", "xboxsubs", "setupofficesdx", "amcxbox", "consumersupport", "commercialsupport", "oxowebdirect", "payin")`

`//| where ServerPartner != "xboxsettings"`

`//| extend SessionId = tostring(parse_url(data_baseData_targetUri).["Query Parameters"].sessionId)`

`//| where SessionId == "f2144a27-aefc-42a4-9ba7-c90613920b15"`

`//| project ServerPartner, cV, TIMESTAMP, data_baseData_targetUri`

`   //| project data_baseData_targetUri, cV, SessionId`

`//| summarize count() by cvBase, ServerPartner, TimeDay = bin(TIMESTAMP, 1d);`

`//| project data_Partner, data_baseData_targetUri`

`//| take 100`

`| summarize count() by cvBase, ServerPartner, ServerOperationName, JoinCol = strcat(cvBase, "_", ServerPartner, "_", ServerOperationName)`

```
;

//let startDateTime = datetime("2022-07-08 00:00:00");
```

`//let endDateTime = datetime_add('hour', 24, startDateTime);`

`let clientCVs =`

```
PidlSdkTelemetryEvents
```

`//| where IngestTime > datetime_add('hour', -2, startDateTime) and IngestTime < datetime_add('hour', 2, endDateTime)`

`| where IngestTime > startDateTime and IngestTime < endDateTime`

`| where Environment =~ "prod"`

`| where name == "Ms.Webi.OutgoingRequest"`

`| extend OSRUri = parse_url(OSRTargetUri)`

`| where OSRUri.Path contains "paymentMethodDescriptions"`

`| extend ClientPartnerName = tolower(tostring(OSRUri.["Query Parameters"].partner)), ClientOperationName = tolower(tostring(OSRUri.["Query Parameters"].operation))`

`| project ClientPartnerName, CV, BrowserDomain, SdkVersion, SdkViewType, ClientOperationName`

`| extend cvBase = GetCVBase(CV)`

`| summarize count() by cvBase, ClientPartnerName, ClientOperationName, JoinCol = strcat(cvBase, "_", ClientPartnerName, "_", ClientOperationName)`

`   //| take 100`

```
;

PIFDCVs
```

`| join kind=fullouter clientCVs on JoinCol`

`| summarize HasOnlyServiceTelemetry = dcountif(cvBase, cvBase1 == ""), HasAllTelemetry = dcountif(cvBase, cvBase1 !="" and cvBase != "")`

`, HasOnlyClientTelemetry = dcountif(cvBase1, cvBase == "") //, HasServicTelemetry = dcountif(cvBase, cvBase1 !="")`

```
//by TimeDay
```

`by ServerPartner, ReferrerHost`

`| extend percentage = round((HasOnlyServiceTelemetry*100.0)/(HasAllTelemetry + HasOnlyServiceTelemetry),4)`

`//| project TimeDay, percentage, HasOnlyServiceTelemetry, TotalTelemetry = HasAllTelemetry + HasOnlyServiceTelemetry, HasOnlyClientTelemetry`

`//| project ClientPartnerName, ServerPartner, ServerOperationName, ClientOperationName, ReferrerHost, percentage, HasOnlyServiceTelemetry, TotalTelemetry = HasAllTelemetry + HasOnlyServiceTelemetry, HasOnlyClientTelemetry`

`| project ServerPartner, ReferrerHost, percentage, HasOnlyServiceTelemetry, TotalTelemetry = HasAllTelemetry + HasOnlyServiceTelemetry`

`//| render timechart`

```
;
```

`//33183/197356`

`PXCVs`

`| join kind=fullouter clientCVs on JoinCol`

`| summarize HasOnlyServiceTelemetry = dcountif(cvBase, cvBase1 == ""), HasAllTelemetry = dcountif(cvBase, cvBase1 !="" and cvBase != "")`

`, HasOnlyClientTelemetry = dcountif(cvBase1, cvBase == "")//, HasServicTelemetry = dcountif(cvBase, cvBase1 !="")`

```
//by TimeDay
```

`by ClientPartnerName, ServerPartner, ServerOperationName, ClientOperationName;`

`//| extend percentage = round((HasOnlyServiceTelemetry*100.0)/(HasAllTelemetry + HasOnlyServiceTelemetry),4)`

`//| project TimeDay, percentage`

` //| render timechart`