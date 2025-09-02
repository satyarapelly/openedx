# PX AAD Applications

## Target audience
PX Engineering team

## PX AAD Applications
AAD applications used by PX  code
### PX PROD/PPE
| Application ID | Application Name| Tenant | SAW Access Only| Manual Renew Certificate | Cert Expiration | Used in| Next Step| Reason |
| -----| --------------- | -------| -------------| --------------- | --------------|--------------|------------- |-----------|
| [e27b8ebe-29c8-4a4c-aa58-f2a2dac9a973](https://ms.portal.azure.com/#blade/Microsoft_AAD_RegisteredApps/ApplicationMenuBlade/Credentials/appId/e27b8ebe-29c8-4a4c-aa58-f2a2dac9a973/isMSAApp/)|Payment Experience Prod| MSIT (72f988bf-86f1-41af-91ab-2d7cd011db47) |No|Yes                      | 6/11/2022       | Call services e.g. purchase| Deprecation
| [8be7ced7-e5fe-40a8-81c1-de6363e41d41](https://portal.azure.com/#blade/Microsoft_AAD_RegisteredApps/ApplicationMenuBlade/Credentials/appId/8be7ced7-e5fe-40a8-81c1-de6363e41d41/isMSAApp/)|PX-Service-PROD-PME |PME (975f013f-7f24-47e8-a7d3-abc4752bf346)| Yes| Yes | 6/11/2022 | Call services e.g. payerAuth| NA|
| [997f2cfc-edc7-47c2-8103-9837cf31e9f1](https://firstpartyportal.msidentity.com/applicationDetails/GetApplicationDetails?appId=997f2cfc-edc7-47c2-8103-9837cf31e9f1&environment=PROD)|Payment Experience Service| 1st party (f8cdef31-a31e-4b4a-93e4-5f571e91255a) |Yes|No                      |   NA     | Accept AAD call|  NA|

### PX INT
| Application ID | Application Name| Tenant | SAW Access Only| Manual Renew Certificate | Cert Expiration | Used in| Next Step|
| -----| --------------- | -------| -------------| --------------- | --------------|--------------|-------------
| [53fe0d08-a3e3-4bd8-af64-08006b1869d6](https://ms.portal.azure.com/#blade/Microsoft_AAD_RegisteredApps/ApplicationMenuBlade/Overview/appId/53fe0d08-a3e3-4bd8-af64-08006b1869d6/isMSAApp/)|Payment Experience INT| MSIT (72f988bf-86f1-41af-91ab-2d7cd011db47) |No|Yes                      | 3/20/2022       | Call services e.g. purchase| Deprecation
| [3e88f276-2b04-48e2-a702-0a75a5284af4](https://portal.azure.com/#blade/Microsoft_AAD_RegisteredApps/ApplicationMenuBlade/Credentials/appId/3e88f276-2b04-48e2-a702-0a75a5284af4/isMSAApp/)|PX-Service-INT-PME |PME (975f013f-7f24-47e8-a7d3-abc4752bf346)| Yes| Yes | 3/20/2022 | Call services e.g. payerAuth| NA|
| [ede6832e-2581-4c10-8141-9b4cbe81e06c](https://firstpartyportal.msidentity.com/applicationDetails/GetApplicationDetails?appId=ede6832e-2581-4c10-8141-9b4cbe81e06c&environment=PROD)|Payment Experience Service INT| 1st party (f8cdef31-a31e-4b4a-93e4-5f571e91255a) |Yes|No                      |   NA     | Accept AAD call|  NA|

## Notes
To access PME application in the portal, it requires your PME account to login otherwise you will see 404.

## Reference
To understand how we rotate aad client certificate, please read [here](https://microsoft.visualstudio.com/Universal%20Store/_git/SC.CSPayments.PX?path=/private/Payments/Docs/operations/ECRDrill.md&_a=preview&version=GBmaster). The aad client certificate expires every 1 year. We normally start rotation at the 9th month. We will receive icm email with title "[PROD--PUBLIC] Sev 3: ID 242505675: [Publisher-Prod] Secrets Management--Certificate expiration notice < 90 days - aad-pxclientauth-paymentexperience-azclient-ms : WUS : Version e7d30fc0457a458788ed6a23a7418be1 : Thumbprint EE249C59B1B58435AE992A91E1C9..." and S360 should show.

---
For questions/clarifications, email [author/s of this doc and PX support](mailto:JieFan@microsoft.com?cc=PXSupport@microsoft.com&subject=Docs%20-%20engineering/local-cache.md).

---