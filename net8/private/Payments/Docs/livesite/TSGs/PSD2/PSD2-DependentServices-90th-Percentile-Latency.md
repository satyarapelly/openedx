# PSD2-DependentServices-90th-Percentile-Latency

    • Get seession ID of affected sessions
        ○ let endTime = ago(1m);
        ○ let startTime = ago(4h);
        ○ RequestTelemetry
        ○ | where TIMESTAMP between (startTime..endTime)
        ○ | where Role == "PxAppService" and data_baseData_operationName in ("Put the Operation Name Here With High Latency")
        ○ | extend LatencyMs = toint(data_baseData_latencyMs), ext_cloud_location
        ○ | extend request = parse_json(data_RequestDetails)
        ○ | extend accountId = tostring(request.payment_session.account_id)
        ○ | extend country = tostring(request.payment_session.country)
        ○ | extend sessionId = tostring(request.payment_session.payment_session_id)
        ○ | extend resSessionId = tostring(parse_json(data_ResponseDetails).payment_session_id)
        ○ | where LatencyMs > 20000
        ○ | project sessionId, resSessionId
    
    • Service spikes
        ○ PayerAuthService/SessionService
            ▪ Email paytran@microsoft.com for information on their side for help identifying cause of spike
            ▪ In the email, specify the service and endpoint, provide some a few sessionIds from the query above
