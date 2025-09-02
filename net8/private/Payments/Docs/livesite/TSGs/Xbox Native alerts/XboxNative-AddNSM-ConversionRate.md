# XboxNative-AddNSM-ConversionRate

    1. Identify partner that is having the issues
        a. If the conversion rate ever drops to 0% for an extended time (several hours), that means there was either a bug in PX Service or PIDLSDK
    2. Use query below to get error codes and messages by country and partner
        a. Identify if there is particular error occurring at a high rate in a single market or partner
        b. ErrorCodes
            i. SessionExpired
                1. Potential PIDL/PX deployment issue.  Check with PX team to see if there is a deployment that happened recently.  For PIDL team check with selvanrfte alias
            ii. InvalidChallengeCode
                1. OTP invalid, potential attack.  To confirm see if multiple tries coming in the same market from the same account
            iii. MOAccountBarred
                1. Phone unauthorized for purchases, potential attack.  To confirm see if multiple tries coming in the same market from the same account
    3. If there was a PX deployment that matches with 0% drop, we need to rollback or fix forward PX Service
        a. If there is a particular partner failing excessively, we need to communicate with the PIDL team and partner to potentially rollback that release
            ▪ Storify/Saturn– PM, Miranda Chao (include selvanrfte)
            ▪ Xboxsettings/Xboxsubs - selvanrfte

**// Query to get error codes and messages by market**
```
let endTime = ago(1m);
let startTime = ago(1d);
RequestTelemetry
| where TIMESTAMP between (startTime .. endTime)
  and name == "Microsoft.Commerce.Tracing.Sll.PXServiceIncomingOperation"
    and tostring(parse_json(data_RequestDetails).PaymentSessionData.partner) != "AppInsights"
    and data_baseData_targetUri has "paymentInstrumentsEx" and data_baseData_targetUri has "resume"
    and data_baseData_targetUri !has"8e342cdc-771b-4b19-84a0-bef4c44911f7"
  and data_baseData_requestMethod != "OPTIONS"  
  //and data_baseData_protocolStatusCode startswith "2"
| extend partner = tolower(parse_url(data_baseData_targetUri)["Query Parameters"].partner)
| extend language = tolower(parse_url(data_baseData_targetUri)["Query Parameters"].language)
| extend reqObj = parse_json(data_RequestDetails)
| extend resp = parse_json(data_ResponseDetails)
| extend country = tolower(parse_url(data_baseData_targetUri)["Query Parameters"].country)
| where tolower(reqObj.challengeType) == "sms"
| where partner in ("storify", "xboxsubs", "saturn", "xboxsettings")
| where data_baseData_protocolStatusCode !startswith "2"
| extend resp = parse_json(data_ResponseDetails)
| extend errorCode = tostring(resp.ErrorCode)
| extend errorMessage = tostring(resp.InnerError.Message)
| summarize count() by data_AccountId, errorCode, errorMessage, country, language, partner
```