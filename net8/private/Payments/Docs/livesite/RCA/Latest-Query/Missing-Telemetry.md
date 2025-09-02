# Missing Telemetry

`let startDateTime = datetime("2022-06-20 00:00:00");`

`let endDateTime = datetime_add('hour', 280, startDateTime);`

`let serviceCVs = RequestTelemetry`

`| where TIMESTAMP > startDateTime and TIMESTAMP < endDateTime`

`and name == "Microsoft.Commerce.Tracing.Sll.PXServiceIncomingOperation"`

`and SourceNamespace == "paymentexperiencelogsprod"`

`and data_baseData_operationName == "PaymentMethodDescriptionsController-GET"//"PaymentInstrumentsExController-POST"// "PaymentMethodDescriptionsController-GET"`

`and data_RequestHeader contains "x-ms-pidlsdk-version"`

`and data_baseData_targetUri !contains "storify" and data_baseData_targetUri !contains "xbox"`

`and data_AccountId !in (`

```
"5662f816-97b9-4c2f-9b28-0c09d2c15a1e"
```

`, "097a070b-603c-4ad1-bc63-2d9b7f111c11"`

`, "8e342cdc-771b-4b19-84a0-bef4c44911f7"`

`, "876f8839-aae6-45a0-b7f3-679e5385035a")`

`| project data_Partner, cV, TIMESTAMP`

`| extend cvBase = GetCVBase(cV)`

`| summarize count() by cvBase, data_Partner, TimeDay = bin(TIMESTAMP, 1d);`

`let clientCVs = PidlSdkTelemetryEvents`

`| where IngestTime > datetime_add('hour', -2, startDateTime) and IngestTime < datetime_add('hour', 2, endDateTime)`

`| where Environment =~ "prod"`

```
//| where PIDLResourceType contains "paymentmethod" or PIDLResourceType contains "paymentinstrument"// | take 10
```

`| project PartnerName, CV`

`| extend cvBase = GetCVBase(CV)`

`| summarize count() by cvBase, PartnerName;`

```
serviceCVs
```

`| join kind=fullouter clientCVs on cvBase`

```
//| where cvBase1 == ""
//| take 100
```

`| summarize HasOnlyServiceTelemetry = dcountif(cvBase, cvBase1 == ""), HasAllTelemetry = dcountif(cvBase, cvBase1 !="" and cvBase != "")`

`, HasOnlyClientTelemetry = dcountif(cvBase1, cvBase == "")//, HasServicTelemetry = dcountif(cvBase, cvBase1 !="")`

`by TimeDay`

```
//by PartnerName, data_Partner
```

`| extend percentage = round((HasOnlyServiceTelemetry*100.0)/(HasAllTelemetry + HasOnlyServiceTelemetry),4)`

`| project TimeDay, percentage`

`| render timechart`

Sample query to find the missing telemetry on client side. Results are copied to https://microsoft.sharepoint.com/:x:/t/PXEngineering/EQEAQZq9GIJPm4lksa5pv5QBjwEwW5ALTzO5GdWoAyYHcg?e=KBJ98S

Note: If Interchange is down, it is due to the following incident:

[Incident 321837179](https://portal.microsofticm.com/imp/v3/incidents/details/321837179/home) : Interchange Drop In Volume