TSG: PSD2 Auth Status Unverified Count
======================================

Explanation of the Impact
-------------------------

When you receive an alert - **PSD2-AuthStatus-UnverifiedCount-60minute-Lookback**, it means there is a high number of unverified PSD2 sessions. This can impact the authentication process, potentially leading to failed transactions and a degraded customer experience.

Severity Levels Explanation
---------------------------

*   **Sev4**: Not urgent, no SLA impact such as routine maintenance.
*   **Sev3**: Urgent or high business impact with no SLA impact such as service/component below SLA, not impacting major or minor customer scenario.
*   **Sev2**: Partial workflow or use case issue impacting service(s) or customer experience with errors or degraded performance resulting in impact as defined by service group.
*   **Sev1**: Multi-Region, multi-service, or major service degradation issue which severely impacts customers as defined by service group.
*   **Sev0**: Entire platform (Identity, Networking, etc.) globally offline, with no workaround, directly impacting customers or an entire channel (Azure, Xbox, Office, etc.).

SLA Definition for Incidents
----------------------------

[Commerce + Ecosystems](https://microsoft.sharepoint.com/teams/OSOC1/SitePages/CEN.aspx?ovuser=72f988bf-86f1-41af-91ab-2d7cd011db47%2csivakot%40microsoft.com&OR=Teams-HL&CT=1721845686569&clickparams=eyJBcHBOYW1lIjoiVGVhbXMtRGVza3RvcCIsIkFwcFZlcnNpb24iOiI0OS8yNDA2MjcyNDgxNCIsIkhhc0ZlZGVyYXRlZFVzZXIiOmZhbHNlfQ%3d%3d) has a goal of mitigating Sev1/2 outages within 60 mins (TTM < 60). In addition to C+E TTM goal, your service may have additional SLAs for this specific incident.

Investigation Steps
-------------------

Here is a kusto query you can use to check partner wise breakdown of the unverified sessions:

Open in [ADX Web](https://dataexplorer.azure.com/clusters/pst/databases/Prod?query=H4sIAAAAAAAAA0WNTQrCMBCF955ilil0U11nIfUApRX30UzagElwMiNUPLxprbh7P9%2FjdcNpfxSeBjYsuceHYOaszJjUwVY1LKIJRWQJwZB%2FoWYS%2FPm5TxLtOelmqnbvPwQSL0jeebRtIRj00nmntoc1rOG5MaA1OHPPWMF1BvYBW8M4JpproO%2BiM8QRqZwQRou0UreppB9Khs5RwwAAAA%3D%3D) | [Kusto Desktop](https://pst.kusto.windows.net/Prod?query=H4sIAAAAAAAAA0WNTQrCMBCF955ilil0U11nIfUApRX30UzagElwMiNUPLxprbh7P9%2FjdcNpfxSeBjYsuceHYOaszJjUwVY1LKIJRWQJwZB%2FoWYS%2FPm5TxLtOelmqnbvPwQSL0jeebRtIRj00nmntoc1rOG5MaA1OHPPWMF1BvYBW8M4JpproO%2BiM8QRqZwQRou0UreppB9Khs5RwwAAAA%3D%3D&web=0) | [Real-Time Intelligence](https://msit.fabric.microsoft.com/groups/me/queryworkbenches/querydeeplink?cluster=https://pst.kusto.windows.net/&database=Prod&query=H4sIAAAAAAAAA0WNTQrCMBCF955ilil0U11nIfUApRX30UzagElwMiNUPLxprbh7P9%2FjdcNpfxSeBjYsuceHYOaszJjUwVY1LKIJRWQJwZB%2FoWYS%2FPm5TxLtOelmqnbvPwQSL0jeebRtIRj00nmntoc1rOG5MaA1OHPPWMF1BvYBW8M4JpproO%2BiM8QRqZwQRou0UreppB9Khs5RwwAAAA%3D%3D) | [cluster('pst.kusto.windows.net').database('Prod')](https://dataexplorer.azure.com/clusters/pst/databases/Prod)
```
PSD2AuthStatusRequests(ago(3d), ago(1m), summarize=true, summaryRoundTo=1h)
| summarize unVerifiedCount = sumif(RequestCount, verified == false) by timeCategory, requestPartner
| render timechart
```

Note down which partner(s) has the highest number of unverified sessions.

Check the PSD2 challenge completion rates for this partner. You can refer to this TSG to get that info:  
[PSD2 ChallengeCompletion Success Rate](https://microsoft.sharepoint.com/teams/PaymentExperience/_layouts/OneNote.aspx?id=%2Fteams%2FPaymentExperience%2FSiteAssets%2FPayment%20Experience&wd=target%28LiveSite%2FTSGs.one%7CB6F43CCE-B562-4FFF-B2C6-930873727180%2FPSD2%20ChallengeCompletion%20Success%20Rate%7C4720AA7D-CA61-4D9C-A629-92A5273890EB%2F%29onenote:https://microsoft.sharepoint.com/teams/PaymentExperience/SiteAssets/Payment%20Experience/LiveSite/TSGs.one#PSD2%20ChallengeCompletion%20Success%20Rate&section-id={B6F43CCE-B562-4FFF-B2C6-930873727180}&page-id={4720AA7D-CA61-4D9C-A629-92A5273890EB}&end)

If the completion rates drop, then likely there is a different alert triggered already. Investigate that as per the TSG linked above and take the right action.

Mitigation Steps
----------------

#### How should I fix things if PSD2 challenge completion rates remain unaffected?

*   Was there a PX flight that was recently enabled that matches with the increase in the unverified rate?
    *   Roll back flight.
*   Was there a PX deployment that week that included PSD2 changes?
    *   If there was flight around that change, revert that flight.
    *   Roll back PX deployment or deploy hotfix fixing that issue.
        *   Contact Kolby Chien/Selvan Ramasamy/Anushri Marar
*   Is there a particular partner that is having a high error rate?
    *   Need to involve the on-call for the problem partner.

Validation Steps
----------------

*   Confirm that the number of unverified sessions has decreased after implementing the mitigation steps.
*   Re-run the KQL query to ensure that the unverified count is within acceptable limits.
*   Verify that no new alerts are triggered for the same issue.

Root Cause Analysis (RCA)
-------------------------

Conduct a thorough investigation to determine the root cause of the high number of unverified PSD2 sessions. Document findings to prevent recurrence and improve future response strategies.

Escalation Contact or Point-of-Contact (POC):
---------------------------------------------
For further assistance, contact the PX service team at pxoncall@microsoft.com.

Acronyms or Glossary
--------------------

*   **PSD2**: Payment Services Directive 2
*   **PX**: Payment Experience
*   **KQL**: Kusto Query Language
*   **RCA**: Root Cause Analysis

Kusto Links
===========

| Cluster | Database | Link |  
| - | - | - |  
| pst | Prod | [Link](https://dataexplorer.azure.com/clusters/pst/databases/Prod?query=H4sIAAAAAAAAA0WNTQrCMBCF955ilil0U11nIfUApRX30UzagElwMiNUPLxprbh7P9%2FjdcNpfxSeBjYsuceHYOaszJjUwVY1LKIJRWQJwZB%2FoWYS%2FPm5TxLtOelmqnbvPwQSL0jeebRtIRj00nmntoc1rOG5MaA1OHPPWMF1BvYBW8M4JpproO%2BiM8QRqZwQRou0UreppB9Khs5RwwAAAA%3D%3D)

Deployment Links
================

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