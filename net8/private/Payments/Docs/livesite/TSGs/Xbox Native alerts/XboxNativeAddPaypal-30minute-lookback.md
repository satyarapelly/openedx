# XboxNativeAddPaypal-30minute-lookback

**Alert firing means long term drop in success rate for xbox native scenarios.**

Query to pull for affected time:
```
let start = todatetime("2024-02-08 04:00:00");

let end = todatetime("2024-02-08 19:00:00");

RequestTelemetry
| where TIMESTAMP between (start .. end)
    and tostring(parse_json(data_RequestDetails).PaymentSessionData.partner) != "AppInsights"
    and name == "Microsoft.Commerce.Tracing.Sll.PXServiceIncomingOperation"
    and data_baseData_operationName in ("PaymentMethodDescriptionsController-GET", "PaymentInstrumentsExController-GET-GetModernPI")
    and data_baseData_targetUri !has"8e342cdc-771b-4b19-84a0-bef4c44911f7"
| extend partner = tolower(parse_url(data_baseData_targetUri)["Query Parameters"].partner)
| extend country = parse_url(data_baseData_targetUri)["Query Parameters"].country
| extend operation = tolower(parse_url(data_baseData_targetUri)["Query Parameters"].operation)
| extend pmt = tolower(parse_url(data_baseData_targetUri)["Query Parameters"].type)
| extend scenario = tolower(parse_url(data_baseData_targetUri)["Query Parameters"].scenario)
| extend resp = parse_json(data_ResponseDetails)
| extend status = tolower(resp.status), pmt2 = tolower(resp.paymentMethod.paymentMethodType)
| where (pmt == "paypal" or pmt2 == "paypal")
   and partner in ("xboxsettings","storify", "xboxsubs", "saturn")
| summarize addAttemptCount = dcountif(data_AccountId, operation == "add" and data_baseData_operationName == "PaymentMethodDescriptionsController-GET"), addSuccessCount = dcountif(data_AccountId, pmt2 == "paypal" and status == "active" and scenario == "paypalqrcode") by partner
| project partner, addAttemptCount, addSuccessCount, successRate = round(todouble(addSuccessCount) * 100 / todouble(addAttemptCount), 2)
```


1. If there is low volume (<100 attempts), dismiss alert
2. Identify whether the issue is with a single partner or multiple partners
3. If single partner, start an investigation email thread with following and provide timestamps

    • Storify/Saturn - Miranda.Chao@microsoft.com,  loganapple@microsoft.com, selvanrfte@microsoft.com
    
    • XboxSubs - matlop@microsoft.com, selvanrfte@microsoft.com
XboxSettings - selvanrfte@microsoft.com