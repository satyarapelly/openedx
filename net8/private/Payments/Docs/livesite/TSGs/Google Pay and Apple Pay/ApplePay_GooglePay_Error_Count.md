# ApplePay GooglePay Error Count 

## Background

Users can use ApplePay or GooglePay as a a payment method to complete their purchase. This query tracks the number of errors occuring when users are trying to pay with ApplePay or GooglePay on a per partner basis.

## Investigate

If this alert occurs, a high number of users are unable to complete their purchase with ApplePay or GooglePay.

Use below query to find the amount distinct users encountering errors:
```
let startTime = ago(1d);
let endTime = now();
PidlSdkTelemetryEvents
| where EventTime  > startTime and EventTime < endTime
| where PageName != "React PIDL SDK portal"
| where EventName in ("failure")
| extend piid = tostring(parse_json(ResourceParameters).paymentSessionOrData.piid)
| where piid == "cdc85313-9b57-4052-81fb-dea336132cbf" or piid == "be4de87d-7e38-4b2d-8836-9237eb32848e"
| extend paymentType = iif(piid == "be4de87d-7e38-4b2d-8836-9237eb32848e", "applepay", "googlepay")| extend params = parse_json(EventParameters)
| extend error = tostring(params.error.name)
| extend message = tostring(params.error.message)
| extend innerErrorMessage = tostring(params.error.innererror.message)
| extend debugMessage = tostring(params.error.debugMessage)
| extend baseType = tostring(parse_json(OriginalMessage).baseType)
| extend partner = tostring(parse_json(ResourceParameters).partner)
| extend message = iif(message != "", message, iif(debugMessage != "", debugMessage, innerErrorMessage))
| project EventTime, CV, partner, Browser, ClientOS, DeviceModel, error, errorType = substring(message, 0, 120), paymentType
| project cvRoot = GetCVBase(CV), EventTime, partner, Browser, ClientOS, DeviceModel, error, errorType, paymentType
| summarize  min(EventTime), max(EventTime),  dcount(cvRoot) by errorType, paymentType, partner
| where errorType !contains "failed to open window"
```
Severity:
<ul>
    <li>Sev 3 - If there are less than 1000 distinct users impacted by errors</li>
    <li>Sev 2 - If there are greater than 1000 distinct users impacted by errors (Engage on call FTE)</li>
</ul>

Using the above details, analyze the impact and engage the on call FTE. Provide information on the following:<br>
    <ul>
        <li>What is the specific error driving the higher error count?</li>
        <li>If the issue is occurring for a specific payment type (i.e. only Applepay or only GooglePay) and across partners or for only a specific partner.</li>
        <li>Engage PIDL team to get information on any recent PIDL SDK releases</li>
    </ul>

## Mitigate

Errors and Resolutions:<br>
<ul>
    <li>{"error":{"statusCode":"DEVELOPER_ERROR","errorCode":2,"statusMessage":"No key found matching key provided in request."}</li>
    <ul>
        <li>This is likely an issue with public key rotation. Engage "StoreCore-PST-PCE" On Call to get information if key rotation ocurred.</li>
        <li>Follow PCE rotation guide provided by PCE on call</li>
    </ul>
    <li>Check if there are any ApplePay/GooglePay related PX flights enabled by PX On Call
    <ul>
        <li>If flights were enabled, pause the flights. Refer to [feature-flighting.md - Repos (visualstudio.com)](https://microsoft.visualstudio.com/Universal%20Store/_git/SC.CSPayments.PX?path=/private/Payments/Docs/operations/feature-flighting.md&_a=preview) for flight config management</li>
    </ul>
</ul>

## Validation

Run the query in the Investigate section again.  There is a maximum time for when the error last occurred.  Confirm that the error has not occurred since mitigating the issue.

## Other

Dev Owners: Jie Fan, Kolby Chien