# SMBOOBE

## Target audience

PX Engineering and Payments PM teams

## Partner Introduction
SMB OOBE stands for Small Medium Business Out of Box Experience. It is a flow which allows the user to start a M365 SMB Trial during Windows OOBE. 

## Co-dev Design Docs
[SMB OOBE Trial with PI PIDL Co-Dev Design Document](https://microsoft.sharepoint.com/:w:/t/PaymentExperience-CoDev-OMEXSMD/EQND9u9HjlVJpZAdY9rzIScBreR96lo3kc1t65iNtpyrxQ?e=vEDtlo)

## Contacts

- PM: [Enda Glacken](mailto:endaglacken@microsoft.com)
- Dev: [Ebin Benny](mailto:ebbenny@microsoft.com)
- PM manager: [David Mowatt](mailto:dmowatt@microsoft.com)
- Dev manager: [Sara Olsson](mailto:sarao@microsoft.com)

## Team

- Service tree: [OMEX Small and Medium Business Purchase](https://servicetree.msftcloudes.com/#/ServiceModel/Component/Profile/2d9827a3-1da2-477a-8a5e-dc6ec6a9149c)
- ICM team: OfficeOMEX/OMEX - Shannon
- Email alias for oncall: omexoce@microsoft.com
- Email alias for support: omexshannonshield@microsoft.com

### UI surfaces:

During the Windows OOBE(Out of Box Experience) flow, while collecting PI information for Microsoft 365 SMB Trial.

### Traffic estimation:

Estimated TPS: 0.12

### Code repos:

SMBOOBE uses PIDL with PurchaseBlends which can be found here: [CXG.Commerce.UI.PurchaseBlends](https://microsoft.visualstudio.com/Universal%20Store/_git/CXG.Commerce.UI.PurchaseBlends)

The feature repo for SMBOOBE is [OmexSmbPurchase](https://dev.azure.com/office/OC/_git/OmexSmbPurchase)

### Dashboard:

[SMB OOBE Dashboard](https://portal.microsoftgeneva.com/dashboard/OMEXExternalProd/SMB%2520OOBE)

### SLA:

| Severity | Time to Detect Goal | Time to Engage Goal | Time to Mitigate Goal |
| -------- | ------------------- | ------------------- | --------------------- |
| Sev1     | < 5 mins            | < 15 mins           | < 30 mins             |
| Sev2     | < 5 mins            | < 30 mins           | < 12 hours            |
| Sev3     | < 5 mins            | < 30 mins           | Backlog               |

#### Determining Severity

OMEX assigns each user scenario with an importance tier based on traffic and on business needs. Incidents are then assigned severities depending on the impact level and the importance of the scenario.

| Impact level | Description                                                                                                                                                                                                                                                |
| ------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Catastrophic | Experience as a whole is largely unavailable or ceases to function to a substantial degree. All monitoring streams are completley down. The majority of customers are impacted. (50% to 100%)                                                              |
| Severe       | A service is not operating, rendering or is unusable to customers to a substantial degree, security issues in the wild. One or more, but not all, monitoring streams are completely down. A significant percentage of customers are impacted. (25% to 49%) |
| Moderate     | Widespread impact to customers but the experience remains usable for the key scenarios. One or more monitoring streams are online bug degraded systematically. A moderate percentage of customers are implacted. (5% to 24%)                               |
| Minor        | No material impact to functionality. Fit and finish issues. A small percentage of customers are impacted. (less than 5%)                                                                                                                                   |
| Resilient    | Scenario is resilient to partner service impacts and self-mitigates to give customers best possible experience in the event of outages. Effective time to mitigate is 0.                                                                                   |

| Impact Level     | 1 - Catastrophic | 2 - Severe | 3 - Moderate | 4 - Minor                                   | 5 - Resilient |
| ---------------- | ---------------- | ---------- | ------------ | ------------------------------------------- | ------------- |
| Tier 0 scenarios | Sev1 - BCP/DR    | Sev1       | Sev1         | Sev1 if >1% of users impact, otherwise Sev2 | Sev2          |
| Tier 1 scenarios | Sev1 - BCP/DR    | Sev1       | Sev2         | Sev3                                        | Sev3          |
| Tier 2 scenarios | Sev1             | Sev1       | Sev2         | Sev3                                        | Sev3          |
| Tier 3 scenarios | Sev1             | Sev2       | Sev2         | Sev3                                        | Sev3          |
| Tier 4 scenarios | Sev1             | Sev2       | Backlog      | Sev3                                        | Sev3          |

[OMEX/OCE#Severity SLA](https://www.owiki.ms/wiki/OMEX/OCE#Severity_SLA)

### Feature flag control:

PI collection in SMB OOBE will be done as an experiment on Azure EXP. The link to the experiment will be added once the experiment is ready.

### Test portal:

[Lumos Sandbox - Office OOBE](https://www.microsoft.com/officeoobe/sandbox)

**NOTE, the steps below will be updated while this feature is being worked on.**

#### Testing Steps:

1. Install Fiddler from: [Download Fiddler Clasic](https://www.telerik.com/download/fiddler)
2. In Fiddler, select from menu Tools > HOSTS and add the following:

```
40.74.163.96 www.microsoft.com
40.74.163.96 wwwqa.microsoft.com
```

3. In Fiddler, select from menu Tools > Options and enable the following:
   1. Decrypt HTTPS
   2. Actions > Trust Root Certificate
4. Open the [Lumos Sandbox - Office OOBE](https://www.microsoft.com/officeoobe/sandbox)
5. Enable Auth in the sandbox, and choose Business Machine Usage Intent.

### How Pidlsdk is used?

SMBOOBE uses PurchaseBlends which in turn uses PIDL React with the office fabric element factory.

<div style="position:fixed; bottom:30px">

> For questions/clarifications, email [author/s of this doc and PXSupport](mailto:ebbenny@microsoft.com?cc=PXSupport@microsoft.com&subject=Docs%20-%20development/documentation-template.md).

</div>
