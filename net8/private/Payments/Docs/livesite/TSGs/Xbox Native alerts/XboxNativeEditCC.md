# XboxNativeEditCC
```
    • Number of users who could not Edit CC
        ○ let startTime = todatetime("2023-09-24 23:00:00");
        let endTime = todatetime("2023-09-25 00:00:00");
        RequestTelemetry
        | where TIMESTAMP between (startTime .. endTime)
            and data_baseData_targetUri !has"8e342cdc-771b-4b19-84a0-bef4c44911f7"
            and data_baseData_requestMethod != "OPTIONS"
        | where data_baseData_operationName == "PaymentInstrumentsExController-POST-UpdateModernPI"
        | where data_Partner in ("storify", "xboxsettings", "xboxsubs", "saturn")
        | extend req = parse_json(data_RequestDetails)
        | where data_PaymentMethodFamily == "credit_card"
        | summarize arg_max(TIMESTAMP, *) by data_AccountId
        | extend status = tolower(parse_json(data_ResponseDetails).status)
        | where status != "active"
        | summarize count() by data_Partner
```

**• How to resolve**

        ○ View data trend in metrics advisor
            ▪ Is there a very low request count?
                • Low request count (< 30) can skew this    statistic, in that case we need to adjust the alert to filter for low request count
            ▪ Seeing rising trend in errors open investigation?
                • Any PX deployments coinciding with errors?
                • Any particular partner seeing a rise in errors?

                • Does partner seeing error have a high total update count or very low usage (which can lead to a perceived higher failure rate)?
        ○ Rise in particular ErrorCode?
            ▪ ValidationFailed/ InvalidCvv
                • Investigate with PIMS On Call about rise of validation errors.  Is there a problem with a particular processor?

