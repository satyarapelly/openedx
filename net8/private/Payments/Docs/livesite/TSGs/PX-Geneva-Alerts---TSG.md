PX Geneva Alerts - TSG
======================

Severity Levels and SLA Definitions
-----------------------------------

*   **Sev0**: Entire platform (Identity, Networking, etc.) globally offline, with no workaround, directly impacting customers or an entire channel (Azure, Xbox, Office, etc.).
*   **Sev1**: Multi-Region, multi-service, or major service degradation issue which severely impacts customers as defined by service group. Goal: Mitigate within 60 mins.
*   **Sev2**: Partial workflow or use case issue impacting service(s) or customer experience with errors or degraded performance resulting in impact as defined by service group.
*   **Sev3**: Urgent or high business impact with no SLA impact such as service/Component below SLA, not impacting major or minor customer scenario.
*   **Sev4**: Not urgent, no SLA impact such as routine maintenance.

Investigation Steps
-------------------

1.  We can use the following Geneva dashboard to see initial data about PX service health: [Service QoS | Jarvis (microsoftgeneva.com)](https://portal.microsoftgeneva.com/dashboard/paymentexperience-metrics-prod/Service%2520QoS)
    
2.  We can use the following LensExplorer dashboard to get more details on PX service health: [PX Reliability Dashboard - Dashboard - Lens Explorer (msftcloudes.com)](https://lens.msftcloudes.com/#/dashboard/5dc09589-664c-4f9e-b41e-02bb73b36eab?temp=0&isSample=false&_g=%28ws:e377c266-06a6-4858-9d50-4e24c8d098ed%29)  
    Note: This is sourced from Kusto cluster and there would be some delay in getting the recent data.
    
3.  We can use the following dashboard to see the failures (system errors) summary: [ServiceHealth | Jarvis (microsoftgeneva.com)](https://portal.microsoftgeneva.com/dashboard/paymentexperience-metrics-prod/ServiceHealth)
    
    *    In the Summary section, we can see the following details:
    ![](/images/livesite/1-9216490d62fa40d8905be6124111c89f.png)
    *   Column's description,
        *   **Region**: Azure region from which the request is served.
        *   **Count**: Total number of failures.
        *   **UniqueUsers**: Unique users who experience the failures.
        *   **PXInternalErrors**: Out of total failure, how many are caused by PX internal errors (no dependency service issue).
        *   **PXInternalErrors_UniqueUsers**: Number of unique users who experience the failures caused by PX internal errors.

    *    In the 'Dependency_Service_Issues' section, we can see the following details:
    ![](/images/livesite/1-a7542e7283cc4b76b69b29267a150ba6.png)
    *   Column's description,
        *   **Region** : Azure region from which the request is served.
        *   **Operation** : Incoming Operation Name.
        *   **HttpStatus** : Response sent to the caller (PIFD).
        *   **OOperationName** : Outgoing Operation Name.
        *   **OHttpStatus** : Outgoing call HttpStatus.
        *   **Count** : Number of these errors for the given combination (Region, Operation, HttpStatus, OOperationName, OHttpStatus).
        *   **OResponse** : One of the outgoing service responses for the given combination.
        *   **Response** : One of Incoming call's response sent back to the caller.
        *   **cV** : One of the Incoming call's Correlation Vector (we can use this to find detailed logs).   

    *    In the 'PX_InternalErrors' section, we can see the following details:
    ![](/images/livesite/1-615a7393236b4aacb4dea17cc23102ff.png)
    *   Column's description,
        *   **TIMESTAMP** : TimeStamp of the Incoming request.
        *   **cV** : Correlation Vector.
        *   **OperationName** : Incoming Operation Name.
        *   **HttpStatus** : HttpStatus code sent to the caller.
        *   **Response** : Response details sent to the caller.
        *   **Region** : Azure region from which the request is served.
        *   **RoleInstance** : Role Instance.
        *   **ServerTraceId** : Server Trace Id (we can use this to correlate outgoing service calls).
        *   **RequestTraceId** : Request Trace Id (we can use this to correlate outgoing service calls).
        *   **TargetUri** : TargetUri for the request.
        *   **PaymentMethodFamily** : Payment Method Family.
        *   **PaymentMethodType** : Payment Method Type.
        *   **AccountId** : User's Jarvis AccountId.
        *   **Edata_ServerTraceId** : Not significant.
        *   **Edata_RequestTraceId** : Request Trace Id (It'll be the same as 'ServeTraceId' column).
        *   **Edata_Exception** : It'll have the complete exception details.
        *   **Edata_cloud_location** : It'll be same as 'Region' column.

4.  Using the above details, analyze the impact and engage the correct team as mentioned at [livesite-sop.md](https://microsoft.visualstudio.com/Universal%20Store/_git/SC.CSPayments.PX?path=/private/Payments/Docs/operations/livesite-sop.md&_a=preview).
    
    *   If the issue is within PX service, see if the issue is limited to a specific region. If so, engage SRE oncall to take the impacted region offline, or get 'Traffic Manager Contributor' access to our PROD subscription using JIT and take the region offline. Our PROD Azure Traffic Manager is at [Azure Portal](https://portal.azure.com/#@mspmecloud.onmicrosoft.com/resource/subscriptions/9b6168fd-7d68-47e1-9c71-e51828aa62c0/resourceGroups/PX-Services-PROD-TM/providers/Microsoft.Network/trafficmanagerprofiles/paymentexperience-cp/overview). You need to use PME account and Yubi Key to login.
    *   If the issue is caused by any recent flight changes, see if we need to turn off those flights. Refer to [feature-flighting.md](https://microsoft.visualstudio.com/Universal%20Store/_git/SC.CSPayments.PX?path=/private/Payments/Docs/operations/feature-flighting.md&_a=preview) for flight config management.
    *   If the issue is within PX service and not specific to a region and not specific to any flight config, engage Eng Manager Oncall and see if any recent deployment needs to be rolled back.

Root Cause Analysis (RCA)
-------------------------

*   Identify the root cause by analyzing logs and dashboards.
*   Use correlation vectors (cV) and trace IDs to pinpoint the source of the issue.
*   Document findings and any contributing factors.

Validation Steps
----------------

*   Confirm that the issue is resolved by rechecking the dashboards and logs.
*   Ensure that the service health metrics return to normal levels.
*   Validate with impacted users or regions to ensure the issue is no longer present.

Escalation Contact or Point-of-Contact (POC)
--------------------------------------------

*   For immediate assistance, contact the SRE oncall team.
*   For deployment issues, engage the Eng Manager Oncall.
*   Use the [livesite-sop.md](https://microsoft.visualstudio.com/Universal%20Store/_git/SC.CSPayments.PX?path=/private/Payments/Docs/operations/livesite-sop.md&_a=preview) for detailed escalation paths.

Acronyms
--------

*   **SLA**: Service Level Agreement
*   **DRI**: Directly Responsible Individual
*   **SRE**: Site Reliability Engineering
*   **JIT**: Just-In-Time
*   **PX**: Payment Experience

Investigation Time-Window
-------------------------

*   Focus on logs from the time the issue was first reported.
*   Analyze logs from the previous 24 hours to identify any anomalies leading up to the incident.

Airgap / Sovereign Cloud Deployments
------------------------------------

To find deployments to Sovereign Clouds (USNat, USSec, etc.) for this service, navigate to [Azure Bridge](https://bridge.azure.com/#/ReleaseStatus/Latest) and filter by the service's name.

Public Cloud Deployments
------------------------

Below are links to the EV2 rollouts in the last 14 days for the service "Payment Experience Service" (ServiceTreeID: e50abb8e-e976-4311-b12b-85156f4abc0e) service groups:

| Service Group | Classic EV2 Portal | New EV2 Portal |
|---------------|--------------------|----------------|
| Microsoft.CFS.PC.StaticResources.PROD | [Link](https://ev2portal.azure.net/#/?servicegroups=microsoft.staticresources&daterange=14&rolloutinfra=Prod) | [Link](https://ra.ev2portal.azure.net/#/Prod/e50abb8e-e976-4311-b12b-85156f4abc0e/Microsoft.CFS.PC.StaticResources.PROD?dateRange=14) |
| Microsoft.CFS.PC.SecurePX.PROD | [Link](https://ev2portal.azure.net/#/?servicegroups=securepx&daterange=14&rolloutinfra=Prod) | [Link](https://ra.ev2portal.azure.net/#/Prod/e50abb8e-e976-4311-b12b-85156f4abc0e/Microsoft.CFS.PC.SecurePX.PROD?dateRange=14) |
| Microsoft.CFS.PC.SellerMarketplace.PROD | [Link](https://ev2portal.azure.net/#/?servicegroups=microsoft.cfs.pc.sellermarketplace.prod&daterange=14&rolloutinfra=Prod) | [Link](https://ra.ev2portal.azure.net/#/Prod/e50abb8e-e976-4311-b12b-85156f4abc0e/Microsoft.CFS.PC.SellerMarketplace.PROD?dateRange=14) |
| Microsoft.CFS.PC.PSS.PROD | [Link](https://ev2portal.azure.net/#/?servicegroups=pss&daterange=14&rolloutinfra=Prod) | [Link](https://ra.ev2portal.azure.net/#/Prod/e50abb8e-e976-4311-b12b-85156f4abc0e/Microsoft.CFS.PC.PSS.PROD?dateRange=14) |
| Microsoft.CFS.PC.PX.PROD | [Link](https://ev2portal.azure.net/#/?servicegroups=PX&daterange=14&rolloutinfra=Prod) | [Link](https://ra.ev2portal.azure.net/#/Prod/e50abb8e-e976-4311-b12b-85156f4abc0e/Microsoft.CFS.PC.PX.PROD?dateRange=14) |
| Microsoft.CFS.PC.MSPayments.PROD | [Link](https://ev2portal.azure.net/#/?servicegroups=MSPayments&daterange=14&rolloutinfra=Prod) | [Link](https://ra.ev2portal.azure.net/#/Prod/e50abb8e-e976-4311-b12b-85156f4abc0e/Microsoft.CFS.PC.MSPayments.PROD?dateRange=14)