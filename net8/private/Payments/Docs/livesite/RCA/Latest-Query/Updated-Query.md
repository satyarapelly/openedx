# Updated Query

`let startDateTime = datetime("2022-07-07 00:00:00");`

`let endDateTime = datetime_add('hour', 24, startDateTime);`

`let serviceCVs =`

```
RequestTelemetry
```

`| where TIMESTAMP > startDateTime and TIMESTAMP < endDateTime`

`and name == "Microsoft.Commerce.Tracing.Sll.PXServiceIncomingOperation"`

`and SourceNamespace == "paymentexperiencelogsprod"`

`and data_baseData_operationName == "PaymentMethodDescriptionsController-GET"//"PaymentInstrumentsExController-POST"// "PaymentMethodDescriptionsController-GET"`

`and data_RequestHeader contains "x-ms-pidlsdk-version"`

`//and data_baseData_targetUri !contains "storify" and data_baseData_targetUri !contains "xbox"`

`and data_AccountId !in (`

`"5662f816-97b9-4c2f-9b28-0c09d2c15a1e"`

`, "097a070b-603c-4ad1-bc63-2d9b7f111c11"`

`, "8e342cdc-771b-4b19-84a0-bef4c44911f7"`

`, "876f8839-aae6-45a0-b7f3-679e5385035a")`

`| extend ServerPartner = iif(data_Partner != "", data_Partner, tostring(parse_url(data_baseData_targetUri).["Query Parameters"].partner))`

`| where ServerPartner != "xboxsettings"`

`| extend SessionId = tostring(parse_url(data_baseData_targetUri).["Query Parameters"].sessionId)`

`//| where SessionId == "f2144a27-aefc-42a4-9ba7-c90613920b15"`

`| project ServerPartner, cV, TIMESTAMP, data_baseData_targetUri`

`    //| project data_baseData_targetUri, cV, SessionId`

`    | extend cvBase = GetCVBase(cV)`

`| summarize count() by cvBase, ServerPartner, TimeDay = bin(TIMESTAMP, 1d);`

`//| project data_Partner, data_baseData_targetUri`

`//| take 100;`

```
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

`| extend parterOrSubPartnerName = iif (PartnerName == "", SubPartnerName, PartnerName)`

`| extend OSRTargetPartner = tostring(parse_url(OSRTargetUri).["Query Parameters"].partner)`

`| extend AmcFundCSVPartner = iff (BrowserDomain == "account.microsoft.com" and PidlSdkOperationType == "fundStoredValue", "NorthStar", "")`

`| extend ClientPartnerName = iif (parterOrSubPartnerName  != "", parterOrSubPartnerName, iif(OSRTargetPartner != "", OSRTargetPartner, iif (AmcFundCSVPartner != "", AmcFundCSVPartner, BrowserDomain)))`

`| where (parse_url(OSRTargetUri).Path) contains "paymentMethodDescriptions"`

`| where name == "Ms.Webi.OutgoingRequest"`

`| where PidlSdkOperationType != "createPaymentSession" // It is not logging service telemetry for webblendsxbox partner`

`| where PidlSdkOperationType  != "handlePaymentChallenge" and PidlSdkOperationType  != "handlePurchaseRiskChallenge" and PidlSdkOperationType != "ValidateResource"`

`//| where PIDLResourceType contains "paymentmethod" or PIDLResourceType contains "paymentinstrument"// | take 10`

`| project ClientPartnerName, CV`

`| extend cvBase = GetCVBase(CV)`

`//| where ClientPartnerName == "amcweb" and cvBase contains "/Lk5vGpsvnIZe+om2MKNuI"`

`| summarize count() by cvBase, ClientPartnerName;`

`    //| take 100;`

```
serviceCVs
```

`| join kind=fullouter clientCVs on cvBase`

`//| where cvBase == "" and ClientPartnerName == "amcweb"`

`//| take 100;`

`| summarize HasOnlyServiceTelemetry = dcountif(cvBase, cvBase1 == ""), HasAllTelemetry = dcountif(cvBase, cvBase1 !="" and cvBase != "")`

`, HasOnlyClientTelemetry = dcountif(cvBase1, cvBase == "")//, HasServicTelemetry = dcountif(cvBase, cvBase1 !="")`

```
//by TimeDay
```

`by ClientPartnerName, ServerPartner;`

`//| extend percentage = round((HasOnlyServiceTelemetry*100.0)/(HasAllTelemetry + HasOnlyServiceTelemetry),4)`

`//| project TimeDay, percentage`

`//| render timechart`