<!--Steps:
1.create partner md eg. smboobe.md
2.copy all the content from partner-info-template
3.add partner in the overview and link to partner md-->

# OXOWEBDIRECT
<!-- 
Replace "Partner Name" with real partner name e.g. SMBOOBE 
-->

## Target audience
<!-- 
Select one the following:

PX Engineering team
PX Engineering and Payments PM teams
Developers and PMs contributing code or docs to PX service
-->
PX Engineering and Payments PM teams

## Partner Introduction
<!-- Please add a brief introduction of the partner. 
1. what does partner name stand for?
2. How the flow is used?
e.g.
SMB OOBE stands for Small Medium Business Out of Box Experience. It is a flow which allows the user to start a M365 SMB Trial during Windows OOBE. 
-->
OXO stands for Office Experience Organization and
OXOWEBDIRECT partner is used in a flow that allows uers to start M365 subscriptions purchase on officecheckout web site.

## Co-dev Design Docs
<!--
Please put the link of Co-dev design docs. 
[you doc name](link of your doc)
-->
[Add PIDL support for oxowebdirect flow](https://microsoft.visualstudio.com/OSGS/_workitems/edit/32088051)

## Contacts
<!--Please help us collect the following information if available-->
- PM: [Caoimhe Grace Dowling](caoimhegrace@microsoft.com)
- Dev: [Vahan Hovhannisyan](vahovhan@microsoft.com)
- PM manager: [David Mowatt](mailto:dmowatt@microsoft.com)
- Dev manager: [Anna Koloskova](annak@microsoft.com)

## Team
<!--Please help us collect the following information if available-->
- Service tree: [OMEX Office Checkout](https://servicetree.msftcloudes.com/?pathSet=true#/ServiceModel/Component/Profile/076caa1d-1e9e-4afc-ae0c-3ed3ec1f96b3?UpdateNav=false)
- ICM team: OMEX - Corrib Shield
- Email alias for oncall: omexoce@microsoft.com
- Email alias for support: omexslishield@microsoft.com

### UI surfaces: 
[<!--e.g. During Windows setup, on https://signup.microsoft.com/, on xbox console or so-->]()
On https://checkout.office.com/acquire/purchase?market=us&language=en-us&scenario=M365Personal

### Traffic estimation: 
<!--Can you share with us your rough TPS?-->
### Code repos: 
[OfficeCheckoutUX](https://office.visualstudio.com/OC/_git/OfficeCheckoutUX)

### Dashboard: 
<!--Can you share with us your main dashboard link? [your dashboard name](link)-->
in Progress work

### SLA:
<!--Can you share what is your SLA for Sev1, Sev2 and Sev3?-->
| Severity | Time to Detect Goal | Time to Engage Goal | Time to Mitigate Goal |
| -------- | ------------------- | ------------------- | --------------------- |
| Sev1     | < 5 mins            | < 15 mins           | < 30 mins             |
| Sev2     | < 5 mins            | < 30 mins           | < 12 hours            |
| Sev3     | < 5 mins            | < 30 mins           | Backlog               |

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
<!--If Yes, can you share more detail? eg. azure app config, azure exp or so-->
In general feature gating, setting overrides are handled via [OMEX SOS system](https://sos.omex.office.net/overrides/Prod/omexexternal-prod/OfficePurchaseExperienceApplication)
Experimentations are done on [EXP](https://exp.microsoft.com/)

### Test portal: 
<!--Do you have any test portal to allow us to test end to end? Any guidance?-->
Sign-in into [OfficeCheckout](https://checkout.office.com/acquire/purchase?market=us&language=en-us&scenario=M365Personal) with a test MSA account
### How Pidlsdk is used?
<!-- 
Optionally, list references to other docs that the reader may want to read next
--> 
OfficeCheckout application uses PIDL react with fluent UI

<div >

>For questions/clarifications, email [author/s of this doc and PXSupport](mailto:vahovhan@microsoft.com?cc=PXSupport@microsoft.com&subject=Docs%20-%20development/documentation-template.md).
</div>