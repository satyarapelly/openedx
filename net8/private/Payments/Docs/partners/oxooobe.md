# OXO OOBE

## Target audience

PX Engineering and Payments PM teams

## Partner Introduction

OXO is the Office Experience Organization. OXO OOBE is used in flows that allows users to purchase M365 in the Windows OOBE (Out of Box Experience).

## Co-dev Design Docs

[Creating OXO OOBE Partner](https://office.visualstudio.com/OC/_workitems/edit/4490500)

## Contacts

- PM: [Caoimhe Grace Dowling](mailto:caoimhegrace@microsoft.com)
- Dev: [Ebin Benny](mailto:ebbenny@microsoft.com)
- PM manager: [Terry Farrell](mailto:terryf@microsoft.com)
- Dev manager: [Vahan Hovhannisyan](mailto:vahovhan@microsoft.com)

## Team

- Service tree: [OMEX Office Checkout](https://servicetree.msftcloudes.com/?pathSet=true#/ServiceModel/Component/Profile/076caa1d-1e9e-4afc-ae0c-3ed3ec1f96b3?UpdateNav=false)
- ICM team: OMEX - Corrib Shield
- Email alias for oncall: omexoce@microsoft.com
- Email alias for support: omexslishield@microsoft.com

### UI surfaces:

During the Windows OOBE(Out of Box Experience) flow, while collecting PI information for Microsoft 365 Checkout.

### Traffic estimation: 

Estimated TPS: 0.75

### Code repos:

[Office Checkout UX](https://dev.azure.com/office/OC/_git/OfficeCheckoutUX)

### Dashboard:

In progress, to be updated when completed.

### SLA:

OMEX assigns each user scenario with an importance tier based on traffic and on business needs. Incidents are then assigned severities depending on the impact level and the importance of the scenario.

| Impact level | Description |
|---|---|
| Catastrophic | Experience as a whole is largely unavailable or ceases to function to a substantial degree. All monitoring streams are completley down. The majority of customers are impacted. (50% to 100%) |
| Severe | A service is not operating, rendering or is unusable to customers to a substantial degree, security issues in the wild. One or more, but not all, monitoring streams are completely down. A significant percentage of customers are impacted. (25% to 49%) |
| Moderate | Widespread impact to customers but the experience remains usable for the key scenarios. One or more monitoring streams are online bug degraded systematically. A moderate percentage of customers are implacted. (5% to 24%) |
| Minor | No material impact to functionality. Fit and finish issues. A small percentage of customers are impacted. (less than 5%) |
| Resilient | Scenario is resilient to partner service impacts and self-mitigates to give customers best possible experience in the event of outages. Effective time to mitigate is 0. |

| Impact Level     | 1 - Catastrophic | 2 - Severe | 3 - Moderate | 4 - Minor                                   | 5 - Resilient |
| ---------------- | ---------------- | ---------- | ------------ | ------------------------------------------- | ------------- |
| Tier 0 scenarios | Sev1 - BCP/DR    | Sev1       | Sev1         | Sev1 if >1% of users impact, otherwise Sev2 | Sev2          |
| Tier 1 scenarios | Sev1 - BCP/DR    | Sev1       | Sev2         | Sev3                                        | Sev3          |
| Tier 2 scenarios | Sev1             | Sev1       | Sev2         | Sev3                                        | Sev3          |
| Tier 3 scenarios | Sev1             | Sev2       | Sev2         | Sev3                                        | Sev3          |
| Tier 4 scenarios | Sev1             | Sev2       | Backlog      | Sev3                                        | Sev3          |

### Feature flag control:

ECS will be used for feature flag control.

### Test portal:

[Lumos Sandbox - Office OOBE](https://www.microsoft.com/officeoobe/sandbox)

### How Pidlsdk is used?

React with custom element factory. The element factory used can be found here: [Office Checkout Storybook](https://checkout.office-int.com/content/storybooks/scenarios-framework/)

<div style="position:fixed; bottom:30px">

>For questions/clarifications, email [author/s of this doc and PXSupport](mailto:ebbenny@microsoft.com?cc=PXSupport@microsoft.com&subject=Docs%20-%20oxooobe.md).
</div>