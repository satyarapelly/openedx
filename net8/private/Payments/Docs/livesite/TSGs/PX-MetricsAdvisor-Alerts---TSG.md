# PX MetricsAdvisor Alerts - TSG

## Background and Issue
PX MetricsAdvisor alerts are triggered when anomalies or failures are detected in the reliability or performance of services relying on PX metrics. These alerts may indicate issues such as degraded performance, errors in operations (e.g., adding or updating payment information), or failures in data feeds.

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
   - Retrieve the timestamp and dimensions from the ICM alert (e.g., [Incident 600835713](https://portal.microsofticm.com/imp/v5/incidents/details/600835713/summary),[Incident 60025888](https://portal.microsofticm.com/imp/v5/incidents/details/600258889/summary)).
   - Analyze the last 15 minutes of logs for anomalies or errors.

        ![alt text](./Images/PX%20Metrics.png)

2. **Execute Kusto Queries**:
   - Execute the following function GetPXFailureDetails in our PST PROD Kusto cluster, using the above dimensions and time interval. If a dimension value is empty, use "*" as its value (indicating the value for all) 

        ```
        GetPXFailureDetails(inputStartTime:datetime=datetime(null), inputEndTime:datetime=datetime(null), operationName:string="", partner:string="", market:string="", resourceFamily:string="", resourceType:string="", serviceRegion:string="", errorType:string="systemerror") 
        ```

    - **Sample Query**:
         ```
        GetPXFailureDetails(ago(30m), now(), "paymentinstrumentsexcontroller-post", "commercialstores", "us", "credit_card", "visa",  "PaymentExperience-PROD-centralus", "*") 
        ```
    - Analyze the results for error codes and types. Use additional KQL features for deeper analysis.

   **Note**: In that above function, we can see 'errorType' given as "systermerror", to get only system errors. If we want to get all errors, we need to use 'errorType' as "\*"
    1. The results set includes various interesting columns as shown in the above screenshot. We try to extract the error code from the response in '**ErrorCode**' column and error type in '**ErrorType**' column. We can see the complete error details in '**EventParameters**' column.
    2. On top of this results set, we can apply additional where clauses and other KQL features.
        1. For example, to get unique ClientIPs experiencing system error by various operations and partner

        ```
        GetPXFailureDetails(ago(30m), now(), "*", "*", "*", "*", "*",  "*", "systemerror") 
        | summarize dcount(AccountId) by OperationName, PartnerName 
        ```
    3. Identify the pattern of errors per account ids, see if there's an increasing trend of failures or whether most of the failures is for a specific partner/market/region 
        [TODO] Add query to find this out, also make a list of known errors 

        1. For example, we could group by partner/market/region/instance, and look into if errors happen for a specific dimension 
        ```
        GetPXFailureDetails(ago(1h), now(), "paymentinstrumentsexcontroller-post", "*", "*", "*", "*",  "*", "systemerror") 
       | summarize count() by PartnerName, bin(TIMESTAMP, 10m)//OperationName, PartnerName, RoleInstance, ServiceRegion, Market, ErrorCode, ErrorSource 
       | render timechart 
        ``` 

        - If errors were mainly from a single region/instance, then offline the region or remove the bac instance 
        - If errors were mainly from a single ErrorSource, engage the ErrorSource team  

    4. If we want to find the unique accounts experiencing the failures even after client retries, we can use the following query 
    ``` 
    let startDate = ago(1h); 
    let endDate = now(); 
    (GetPXFailureDetails(startDate, endDate, errorType="systemerror") 
    |union  
    GetPXFailureDetails(startDate, endDate, errorType="unknown")) 
    | extend TIMESTAMP, CV, CVBase = GetCVBase(CV), OriginatedFromPIDLSDK = RequestHeader has "x-ms-pidlsdk-version" 
    | join kind=leftouter 
    (RequestTelemetry 
    | where TIMESTAMP between (startDate .. endDate) 
    and name == "Microsoft.Commerce.Tracing.Sll.PXServiceIncomingOperation" 
    and data_baseData_succeeded == true 
    | project CVBase = GetCVBase(cV), TargetUri = data_baseData_targetUri) 
    on TargetUri, CVBase 
    | where CVBase1 == "" 
    | extend PartnerType = iff(tolower(PartnerName) in ("commercialstores", "azure"), "Commercial", "Consumer") 
    | summarize UniqueUsers = dcount(AccountId), arg_max(TIMESTAMP, CV) by OperationName, PartnerType, ErrorCode, ErrorSource 
    ``` 

3. **Metrics Analysis**:
   - Run the following function to analyze PX MetricsAdvisor data feeds:
     ```kql
     GetPXReliabilityMetrics(ago(30m), now(), time(null), "paymentinstrumentsexcontroller-post", "commercialstores", "us", "credit_card", "visa", "") 
     ```
   - Use the `binRange` parameter for time-series analysis.

   __Note__: In that above function, for any dimension,  

        1. we can either specify a specific value to get metrics only for that dimension value 

        2. we can specify empty value ("") to get all the metrics for various dimension values. 

        3. we can specify  "*" to get aggregated metrics for that dimension. 

    - To get the time series/ trend for any metric we can use the 'binRange' parameter in the above function. 

        For example, to get the hourly trend for UserErrorRate for a particular dimension 
        ```
        GetPXReliabilityMetrics(ago(24h), now(), 1h, "paymentinstrumentsexcontroller-post", "commercialstores", "*", "*", "*", "*") 
        | project Timestamp, UserErrorRate 
        | render timechart 
        ```
4. We can use above two functions, to pretty much answer all the questions related to the error details, unique users, partners affected, error trend analysis, to make sure whether the issue is still ongoing or mitigated. 

- If we need further analysis of any issue for a given CV, or AccountId or other details, we can fetch all the log details (across payment services) from 'RequestTelemetry' table in PST kusto cluster 

- Here are some of the sample queries , To get all Incoming and Outgoing PX call details for a given CV 
    ```
    RequestTelemetry 
    | where TIMESTAMP > ago(60m) 
    and cV startswith "KBHWXC6cOk6sltr" 
    and name in ("Microsoft.Commerce.Tracing.Sll.PXServiceIncomingOperation", "Microsoft.Commerce.Tracing.Sll.PXServiceOutgoingOperation") 
    | project TIMESTAMP, cV, data_baseData_operationName, data_baseData_targetUri, data_baseData_protocolStatusCode, data_RequestDetails, data_ResponseDetails, data_RequestTraceId, data_ServerTraceId, data_AccountId 
    ```

- Note: To get all the Outgoing calls for a given Incoming call, get 'data_ServerTraceId' from Incoming PX call and look for all outgoing PX calls with that Id in 'data_RequestTraceid' column. 
    
    -  To get all telemetry events (including PIFD and PIMS) for a given CV 
    ``` 
    RequestTelemetry 
    | where TIMESTAMP > ago(30m) 
    and cV startswith "KBHWXC6cOk6sltr+.35.1.24" 
    | project TIMESTAMP, cV, SourceNamespace, name, data_baseData_operationName, data_baseData_targetUri, data_baseData_requestMethod 
    , data_baseData_succeeded, data_baseData_protocolStatusCode, data_baseData_latencyMs, data_baseData_responseSizeBytes 
    , data_RequestDetails, data_ResponseDetails, data_RequestHeader, data_ResponseHeader 
    , data_baseData_dependencyName, data_baseData_dependencyOperationName 
    , data_RequestTraceId, data_ServerTraceId, data_correlationId, data_externalServiceRequestId, data_requestId 
    , data_AccountId, data_baseData_callerIpAddress, data_baseData_callerName 
    ```
- Note: PIFD Incoming calls are logged with 'name' as 'Microsoft.MarketplaceServices.PaymentInstrumentService.Logging.IncomingServiceRequest', and 'SourceNamespace' as 'PIFDLogs' 

    - PIMS logs can be found with 'SourceNamespace' as 'PIMSPRODLogAcct' 
    - PX logs can be found with 'SourceNamespace' as 'paymentexperiencelogsprod' 
    - PayMod logs can be found with 'SourceNamespace' as 'PayModLogsProd' 

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

