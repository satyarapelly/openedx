# XboxNativeCVVChallenge-30minute-lookback

Alert firing means long term drop in success rate for xbox native cvv challenges.


Query to pull data:

let start = todatetime("2024-02-08 04:00:00");
let end = todatetime("2024-02-08 19:00:00");
```
RequestTelemetry

| where TIMESTAMP between (startTime .. endTime)

| where data_baseData_operationName contains ("ChallengeDescriptionsController-GET")

| extend type = tolower(parse_url(data_baseData_targetUri)["Query Parameters"].type)

| extend partner = tolower(parse_url(data_baseData_targetUri)["Query Parameters"].partner)

| extend sessionId = tolower(parse_url(data_baseData_targetUri)["Query Parameters"].sessionId)

| where partner in ("storify", "xboxsettings", "xboxsubs", "saturn", "xbox")

| where type == "cvv"

| summarize cvvAttemptCount = count(), cvvSuccessCount = dcountif(sessionId, data_baseData_protocolStatusCode startswith "2") by partner

| extend partner, cvvAttemptCount, cvvSuccessCount, cvvStartSuccessRate = round((cvvSuccessCount * 1.0 / cvvAttemptCount) * 100, 2)
```

1. Identify whether the issue is with a single partner or multiple partners
2. If single partner, start an investigation email thread with following and provide timestamp

    • Storify/Saturn/Xbox - Miranda.Chao@microsoft.com,  loganapple@microsoft.com, selvanrfte@microsoft.com

    • XboxSubs - matlop@microsoft.com, selvanrfte@microsoft.com

    • XboxSettings - selvanrfte@microsoft.com
