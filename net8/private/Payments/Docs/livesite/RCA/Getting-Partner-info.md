# Getting Partner info

`let startDateTime = datetime("2022-06-20 00:00:00");`

`let endDateTime = datetime_add('hour', 720, startDateTime);`

`PidlSdkTelemetryEvents`

`//| where IngestTime > datetime_add('hour', -2, startDateTime) and IngestTime < datetime_add('hour', 2, endDateTime)`

`| where IngestTime > startDateTime and IngestTime < endDateTime`

`| where Environment =~ "prod"`

`| where name == "Ms.Webi.OutgoingRequest"`

`| extend OSRUri = parse_url(OSRTargetUri)`

`| where OSRUri.Path contains "paymentMethodDescriptions"`

`| extend ClientPartnerName = tolower(tostring(OSRUri.["Query Parameters"].partner)), ClientOperationName = tolower(tostring(OSRUri.["Query Parameters"].operation))`

`| summarize count() by ClientPartnerName, SubPartnerName`

`let startDateTime = datetime("2022-06-20 00:00:00");`

`let endDateTime = datetime_add('hour', 720, startDateTime);`

`PidlSdkTelemetryEvents`

`| where IngestTime > startDateTime and IngestTime < endDateTime`

`| where Environment =~ "prod"`

`| where name == "Ms.Webi.OutgoingRequest"`

`| extend OSRUri = parse_url(OSRTargetUri)`

`| where OSRUri.Path contains "paymentMethodDescriptions"`

`| extend ClientPartnerName = tolower(tostring(OSRUri.["Query Parameters"].partner)), ClientOperationName = tolower(tostring(OSRUri.["Query Parameters"].operation))`

`// | project ClientPartnerName, CV, BrowserDomain, SdkVersion, SdkViewType, ClientOperationName`

`| summarize count() by ClientPartnerName`