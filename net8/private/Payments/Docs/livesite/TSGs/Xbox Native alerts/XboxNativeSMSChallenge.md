# XboxNativeSMSChallenge

**• Affected users who could not complete challenge**

```
    let startTime = todatetime("2023-09-24 00:00:00");
    let endTime = todatetime("2023-09-25 00:00:00");
    RequestTelemetry
    | where TIMESTAMP between (startTime .. endTime)
    | where data_baseData_operationName contains ("ChallengeDescriptionsController-GET")
    | extend type = tolower(parse_url(data_baseData_targetUri)["Query Parameters"].type)
    | extend partner = tolower(parse_url(data_baseData_targetUri)["Query Parameters"].partner)
    | extend sessionId = tolower(parse_url(data_baseData_targetUri)["Query Parameters"].sessionId)
    | where partner in ("storify", "xboxsubs", "xboxsettings", "saturn", "xbox")
    | where type == "sms"
    | join kind=inner  (
        RequestTelemetry
        | where TIMESTAMP between (startTime .. endTime)
        | where data_baseData_operationName contains ("NonSimMobi-GET-validateotp")
        | parse data_baseData_targetUri with * '/sessions/' sessionId '/otp' *
        | where isnotempty(sessionId)
        | project TIMESTAMP, cV, otpUrl = data_baseData_targetUri, data_baseData_operationName, otpRequest = data_RequestDetails, otpResponse = data_ResponseDetails, sessionId
        ) on sessionId
    | summarize arg_max(TIMESTAMP, *) by data_AccountId
    | where otpResponse != '"Success"'
    | extend ErrorCode = tostring(parse_json(otpResponse).ErrorCode)
    | summarize count() by ErrorCode, partner
```
    • If the conversion rate ever drops to 0% for an extended time, that means there was either a bug in PX Service or PIDLSDK
        ○ If there was a PX deployment that matches with 0% drop, we need to rollback or fix forward PX Service
        ○ If there is a particular partner failing excessively, we need to communicate with the PIDL team and partner to rollback that release
            ▪ Xbox – PM, Miranda Chao
            ▪ PIDL – Selvan Ramasamy
    • ErrorCodes
        ○ InvalidChallengeCode
            ▪ User fails to enter right OTP, likely user error or otherwise is an attack
        ○ SessionExpired
            ▪ User took too long to enter OTP
        ○ InternalError
            ▪ Can be a variety of errors, need to investigate these to see if there's a problem with a mobile operator or if there's a PX error
            ▪ PIMS on call can help with investigation (Cen Cen as well)

            
