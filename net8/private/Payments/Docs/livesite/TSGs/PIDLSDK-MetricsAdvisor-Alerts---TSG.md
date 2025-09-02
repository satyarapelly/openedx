# PIDLSDK MetricsAdvisor Alerts - TSG

## Background and Issue
PIDLSDK MetricsAdvisor alerts are triggered when anomalies or failures are detected in the reliability or performance of services relying on PIDLSDK metrics. These alerts may indicate issues such as degraded performance, errors in operations (e.g., adding or updating payment information), or failures in data feeds.

## Impact
These issues can affect customer-facing services, leading to degraded performance, failed operations, or SLA breaches. For example, customers may experience errors when performing critical operations such as payment updates, impacting their overall experience and business continuity.

## SLA Definition
- **Sev0**: Entire platform globally offline, no workaround, directly impacting customers or an entire channel (e.g., Azure, Xbox).
- **Sev1**: Multi-region or major service degradation severely impacting customers as defined by the service group. SLA: Mitigate within 60 minutes.
- **Sev2**: Partial workflow or use case issue impacting services or customer experience with errors or degraded performance. Incidents are considered Sev2 if there are >1000 consumer users affected or >100 commercial users affected. SLA: Mitigate within 60 minutes.
- **Sev3**: High business impact with no SLA impact, such as degraded performance below SLA but not affecting major customer scenarios.
- **Sev4**: Non-urgent issues with no SLA impact, such as routine maintenance.

## Investigation Steps
1. **Gather Initial Data**:
   - Retrieve the timestamp and dimensions from the ICM alert (e.g., [Incident 323893290](https://portal.microsofticm.com/imp/v3/incidents/details/323893290/home)).
   - Analyze the last 15 minutes of logs for anomalies or errors.

   ![](/images/livesite/1-37eb7d6e089f40659d3f5a1c5ae7fe58.png)

2. **Execute Kusto Queries**:
   - Run the following function in the PST PROD Kusto cluster using the gathered dimensions and time interval. Use `*` for empty dimension values:
     ```kql
     GetPidlSdkFailureDetails(datetime(ago(15m)), datetime(now()), "operationName", "*", "*", "errorType")
     ```
   - **Sample Query**:
     ```kql
     GetPidlSdkFailureDetails(datetime(2022-07-29 03:30:00), datetime(2022-07-29 04:00:00), "addorupdatepi", "*", "*", "systemerror")
     ```
   - Analyze the results for error codes and types. Use additional KQL features for deeper analysis.

   ![](/images/livesite/1-798745300c6c498abb0fd96659ab2f92.png)

   **Note**: In that above function, we can see 'errorType' given as "systermerror", to get only system errors. If we want to get all errors, we need to use 'errorType' as "\*"
    1. The results set includes various interesting columns as shown in the above screenshot. We try to extract the error code from the response in '**ErrorCode**' column and error type in '**ErrorType**' column. We can see the complete error details in '**EventParameters**' column.
    2. On top of this results set, we can apply additional where clauses and other KQL features.
        1. For example, to get unique ClientIPs experiencing system error by various operations and partner
            ```kql
            GetPidlSdkFailureDetails(ago(30m), now(), "*", "*", "*", "systemerror")
            | summarize UniqueClientIPs = dcount(ClientIP), TotalOperations = dcount(PidlSdkOperationId) by PidlResourceName, PartnerName
           ```

3. **Metrics Analysis**:
   - Run the following function to analyze PX MetricsAdvisor data feeds:
     ```kql
     GetPidlSdkReliabilityMetrics(ago(24h), now(), time(null), "operationName", "*", "*")
     ```
   - Use the `binRange` parameter for time-series analysis.

## Mitigation Steps
1. **Regional Issues**:
   - Engage SRE on-call or use Traffic Manager Contributor access to take the affected region offline.

2. **Flight Changes**:
   - Disable recent flight changes if they are identified as the root cause. Refer to the [Feature Flighting Documentation](https://microsoft.visualstudio.com/Universal%20Store/_git/SC.CSPayments.PX?path=/private/Payments/Docs/operations/feature-flighting.md&_a=preview).

3. **Deployment Rollback**:
   - Engage the Engineering Manager on-call to assess and execute a rollback if necessary.

4. **Service Recycle**:
   - Recycle the affected service to restore functionality quickly.

## Validation Steps
- Re-run the Kusto queries to confirm that errors or performance issues are resolved.
- Verify that the affected services are functioning as expected.
- Monitor for any recurring alerts or anomalies for at least 30 minutes post-mitigation.

## Root Cause Analysis (RCA)
- Conduct a detailed RCA to identify the underlying cause of the issue.
- Document findings and corrective actions to prevent recurrence.
- Share the RCA with stakeholders and update the TSG if necessary.

## Escalation Contact
- For further assistance, escalate via ICM using the defined escalation path.
- Contact the Engineering Manager on-call or the appropriate team as per the [Live Site SOP](https://microsoft.visualstudio.com/Universal%20Store/_git/SC.CSPayments.PX?path=/private/Payments/Docs/operations/livesite-sop.md&_a=preview).

## Acronyms or Glossary
- **ICM**: Incident Management
- **KQL**: Kusto Query Language
- **PX**: Payment Experience
- **SRE**: Site Reliability Engineering
- **OSR**: Outgoing Service Request

## Notes
- Ensure all URLs are valid and accessible to DRIs.
- Keep this TSG up-to-date with the latest processes and tools.
- Prioritize fast mitigation techniques such as failover, rollback, or service recycle.

## Additional Queries
1. All the PX MetricsAdvisor data feeds are sourced from the following function **GetPidlSdkReliabilityMetrics** in PST PROD kusto cluster.
    ```kql
    GetPidlSdkReliabilityMetrics(inputStartTime:datetime=datetime(null), inputEndTime:datetime=datetime(null), binRange:timespan=time(null), operationName:string="", partner:string="", market:string="", extractErrorType:bool=false, byUniqueIP:bool=false, byUniqueUser:bool=false)
    ```
        
    **Sample Query**
    ```kql
    GetPidlSdkReliabilityMetrics(ago(24h), now(), time(null), "addorupdatepi", "", "*")
    ```

![](/images/livesite/1-f7923cbb255d4dd4997662f3bc49c2af.png)

1. we can either specify a specific value to get metrics only for that dimension value
2. we can specify empty value ("") to get all the metrics for various dimension values.
3. we can specify  "\*" to get aggregated metrics for that dimension.
4. To get the time series/ trend for any metric we can use the 'binRange' parameter in the above function.
- For example, to get the houly trend for UserErrorRate for a particular dimension     

        GetPidlSdkReliabilityMetrics(ago(24h), now(), 1h, "addorupdatepi", "commercialstores", "*")
        | project Timestamp, UserErrorRate
        | render timechart

5. We can use above two functions, to pretty much answer all the questions related to the error details, unique users, partners affected, error trend analysis, to make sure whether the issue is still ongoing or mitigated.