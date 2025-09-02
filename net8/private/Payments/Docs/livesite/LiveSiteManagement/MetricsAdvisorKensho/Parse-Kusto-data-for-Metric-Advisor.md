# Parse Kusto data for Metric Advisor

Do you want to trigger a function to parse incoming data and put the resultant data into another table?

You may need this when you need data in a certain format for creating Data feed in Metric Advisor/ creating Kensho alerts.

Here is an example from 3PP project.

**Input table:**  '`ThirdPartyPaymentsTelemetry'`

**Goal:**  Get page load time and other data from **original\_message**

Sample data:
 "data": {

"baseData": {

"properties": {

"version": "PostChannel=2.4.6;SystemPropertiesCollector=2.4.6;WebAnalyticsPlugin=2.4.6"

}

},

"Cv": "nB7f57udNdTfZ62Mjk3Qtc.0",

"sessionId": "8cd9726a-0a8c-4bf1-1362-8b422bf210bc",

"telemetryContext": {

"userContext": {

"userId": "me"

},

"partnerContext": {

"partnerId": "msteams",

"env": "int"

}

},

"eventName": "GuestCheckout\_Form\_Loaded",

"eventProperties": {

"timeTaken": 1016

}

}

**Query to parse:**

**Step 1:**

```
ThirdPartyPaymentsTelemetry
```

`| extend msgJson = parse_json(original_message)`

`| extend ext = msgJson.ext, data = msgJson.data`

`| extend EventTime = msgJson["time"], IngestTime = ext.ingest["time"], ClientIP = ext.ingest.clientIp, BrowserUserAgent = ext.ingest.userAgent, Browser = ext.web.browser, BrowserVer = ext.web.browserVer, BrowserDomain = ext.web.domain,`

`UserId = ext.user.id, ClientOS = ext.os.name, DeviceClass = ext.device.deviceClass, DeviceMake = ext.device.make, DeviceModel = ext.device.model,`

`CV = data.Cv, SessionId = data.sessionId,`

`PageUri = data.baseData.uri,`

`TelemetryContext = data.telemetryContext,`

`EventName = data.eventName,`

`EventProperties = data.eventProperties`

`| extend PartnerName = TelemetryContext.partnerContext.partnerId, Environment = TelemetryContext.partnerContext.env`

`| project name, todatetime(IngestTime), todatetime(EventTime), tostring(CV), tostring(UserId), tostring(ClientIP), tostring(ClientOS),`

`tostring(DeviceClass), tostring(DeviceMake), tostring(DeviceModel), tostring(Browser), tostring(BrowserVer), tostring(BrowserUserAgent), tostring(BrowserDomain), tostring(PageUri),`

`tostring(SessionId), tostring(PartnerName), tostring(Environment), tostring(EventName), EventProperties, TelemetryContext, LoadTimeInMS = toint(EventProperties.timeTaken), OriginalMessage = msgJson`

`The data is parsed into different columns like below`
![](/images/livesite/1-c5522e3eb6fc448d97b791f77dddf20e.png)

```
Step 2:
```

**`Create a function in Kusto to put the parsed data into another table`** `(You have to have the right permissions to create this )`
`.create-or-alter function with (folder = "Payments3PP/ExtractThridPartyPaymentsTelemetryEvents", docstring = "Extract Third Party Payments Telemetry events", skipvalidation = "true") ExtractThridPartyPaymentsTelemetryEvents() {`

```
ThirdPartyPaymentsTelemetry
```

`| extend msgJson = parse_json(original_message)`

`| extend ext = msgJson.ext, data = msgJson.data`

`| extend EventTime = msgJson["time"], IngestTime = ext.ingest["time"], ClientIP = ext.ingest.clientIp, BrowserUserAgent = ext.ingest.userAgent, Browser = ext.web.browser, BrowserVer = ext.web.browserVer, BrowserDomain = ext.web.domain,`

`UserId = ext.user.id, ClientOS = ext.os.name, DeviceClass = ext.device.deviceClass, DeviceMake = ext.device.make, DeviceModel = ext.device.model,`

`CV = data.Cv, SessionId = data.sessionId,`

`PageUri = data.baseData.uri,`

`TelemetryContext = data.telemetryContext,`

`EventName = data.eventName,`

`EventProperties = data.eventProperties`

`| extend PartnerName = TelemetryContext.partnerContext.partnerId, Environment = TelemetryContext.partnerContext.env`

`| project name, todatetime(IngestTime), todatetime(EventTime), tostring(CV), tostring(UserId), tostring(ClientIP), tostring(ClientOS),`

`tostring(DeviceClass), tostring(DeviceMake), tostring(DeviceModel), tostring(Browser), tostring(BrowserVer), tostring(BrowserUserAgent), tostring(BrowserDomain), tostring(PageUri),`

`tostring(SessionId), tostring(PartnerName), tostring(Environment), tostring(EventName), EventProperties, TelemetryContext, LoadTimeInMS = toint(EventProperties.timeTaken),`

`OriginalMessage = msgJson`

```
}
```

**`Step 3: (`**`Set policy to run the function every time there is an incoming data into the original table)`

`.set-or-append ThirdPartyPaymentsTelemetryEvents with (folder = "Payments3PP") <|ExtractThridPartyPaymentsTelemetryEvents()`

```
You are all set to create Data feed in Metric Advisor

Data feed query:

et endTime = datetime_add("minute", 5, datetime(@IntervalStart));
let startTime = datetime_add("minute", -30, endTime);
ThirdPartyPaymentsTelemetryEvents
| where IngestTime between (startTime .. endTime)
and name == "Microsoft.Commerce.Payments.ThirdPartyPayments.PageEvent"
and EventName == "GuestCheckout_Form_Loaded"
| summarize Timestamp = datetime(@IntervalStart), AverageLoadTime = iff(isnan(avg(LoadTimeInMS)), 0.0, round(avg(LoadTimeInMS),2))

For 24 hour window:
```

`let endTime = datetime_add("minute", 5, datetime(@IntervalStart));`
`let startTime = datetime_add("hour", -24, endTime);`

```
ThirdPartyPaymentsTelemetryEvents
| where IngestTime between (startTime .. endTime)
and name == "Microsoft.Commerce.Payments.ThirdPartyPayments.PageEvent"
and EventName == "GuestCheckout_Form_Loaded"
| summarize Timestamp = datetime(@IntervalStart), AverageLoadTime = iff(isnan(avg(LoadTimeInMS)), 0.0, round(avg(LoadTimeInMS),2))
```

`The next steps can be followed here:`
[PX MetricsAdvisor Alerts - TSG](onenote:#PX%20MetricsAdvisor%20Alerts%20-%20TSG&amp;section-id={7299def3-4ab2-4959-a423-5742d6772c8a}&amp;page-id={d3447f71-050b-4b0d-a79f-01479d82b220}&amp;end)`(`[Web view](https://microsoft.sharepoint.com/teams/PaymentExperience/_layouts/15/Doc.aspx?sourcedoc={171acb93-a045-434e-938d-ccefe55457ad}&amp;action=edit&amp;wd=target%28LiveSite%2FLive-Site-Management.one%7C7299def3-4ab2-4959-a423-5742d6772c8a%2FPX%20MetricsAdvisor%20Alerts%20-%20TSG%7Cd3447f71-050b-4b0d-a79f-01479d82b220%2F%29&amp;wdorigin=703&amp;wdpreservelink=1)`)`