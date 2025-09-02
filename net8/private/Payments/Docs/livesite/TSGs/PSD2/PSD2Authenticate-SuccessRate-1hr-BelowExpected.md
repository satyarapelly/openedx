#  PSD2Authenticate SuccessRate BelowExpected TSG

## Background
PSD2 (Payment Services Directive 2) is an EU regulation that came into effect in 2018, designed to enhance the security, innovation, and efficiency of payments within the EU. It updates the original Payment Services Directive (PSD) and introduces key changes in how financial services and payments are managed. PSD2 is mandatory for the European market, and missing PSD2 compliance in required cases will lead to purchase failures, preventing users from buying Microsoft products. Authentication is the first step in PSD2, so maintaining a stable authentication success rate is crucial, and this alert is set up to monitor it.

## Investigate
There are a few reasons which will lead PSD2 Authentication fail. It includes
1. Fraud 
2. Bank issues
3. Payment network issues
4. Payment service unavailability 
5. Traffic pattern changes
6. Partner changes  

By following the steps and running the queries below, we can help identify the potential root cause. Please execute the queries and include both the queries and results in the ICM discussion.
### 1. Identify Failure Investigation by Partner: ###
Since this ICM is triggered per partner, do we see multiple ICMs triggered per partner or just one? If triggered by multiple partners, identify whether the failures are the same reason or different across partners. Run queries #2 for each partner to investigate further.
### 2. Identify Failure Investigation by Market: ###
Please run the following Kusto query to check if the failures are specific to any market:
1. Identify the market 

        PSD2PayerAuthAuthenticationEvents
        | where transTime > ago(7d) and transStatus in ("N", "R", "U", "")
        | summarize count() by market, bin(transTime, 1h)
        | render timechart 
1. If any market shows a significant increase in failures, for example, "GB," use the following query to understand the failure reasons:

        PSD2PayerAuthAuthenticationEvents
        | where transTime > ago(7d) and transStatus in ("N", "R", "U", "")
        | where market == "GB"
        | summarize count() by transStatus, bin(transTime, 1h)
        | render timechart
1. If a specific transStatus is significantly higher, refer to [this documentation](https://docs.3dsecure.io/3dsv2/specification_220.html#attr-ARes-transStatus) to understand the transaction failure reason: 3DS Specification. Use the following query:
 to understand transaction fail reason.

        PSD2PayerAuthAuthenticationEvents
        | where transTime > ago(7d) and transStatus in ("N", "R", "U", "")
        | where market == "GB"  and transStatus == "N"
        | summarize count() by transStatusReason, bin(transTime, 1h)
        | render timechart
1. If the transStatus is empty (""), run this query to gather more details on the failure reason.
                
        RequestTelemetry
        | where TIMESTAMP > ago(1h)
        | where data_baseData_operationName == "MasterCardAuthenticateRequest"
        | extend resp = parse_json(data_ResponseDetails)
        | extend transStatus = tostring(resp.transStatus)
        | extend cardholderInfo = tostring(resp.cardholderInfo)
        | where transStatus in ("")
        | project TIMESTAMP, cV, data_baseData_operationName, transStatus, cardholderInfo, data_RequestDetails, data_ResponseDetail

## Mitigate

Complete the queries in Investigate section, include both the queries and results in the discussion, and ask the on-call team to review. If the on-call team is uncertain about whether mitigation is appropriate, contact the scenario owner, Kolby, for review. If the failure rate is 100% higher than normal, ensure Kolby reviews it.
## Validation


## Other
