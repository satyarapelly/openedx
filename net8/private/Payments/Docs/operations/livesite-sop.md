# Livesite SOP

## Target Audience
PX Engineering team

## Prerequisites
1. [Microsoft On-Call Handbook](https://microsoft.sharepoint.com/teams/DRICulturev-Team/Shared%20Documents/Forms/AllItems.aspx?id=%2Fteams%2FDRICulturev%2DTeam%2FShared%20Documents%2FGeneral%2FPublished%20Handbook%2FOnCall%20DRI%20Handbook%2Dv3%2Epublished%2Epdf&parent=%2Fteams%2FDRICulturev%2DTeam%2FShared%20Documents%2FGeneral%2FPublished%20Handbook&p=true)

## Preparation
Things to setup or verify before an on-call rotation:
1. Join `redmond\PXDevsLocalGroup` at [idweb](https://idweb/identitymanagement/default.aspx)<sup>[*](#footnotes)</sup> to access PX logs in Geneva
1. Join `redmond\PXDevs` at [idweb](https://idweb/identitymanagement/default.aspx)<sup>[*](#footnotes)</sup> to access PX logs in Kusto cluster [CommerceSharedCluster](https://commercesharedcluster.southcentralus.kusto.windows.net), database `commerceshareddb`
1. Ensure SAW machines is up to date with all patches
1. To access service resources in Azure (PROD, PPE and INT) ensure the following:
   1. You have a [Yubikey](https://microsoft.sharepoint.com/teams/CDOCIDM/SitePages/YubiKey-Management.aspx) and it has a valid certificate
   1. You have an active [PME account](https://msazure.visualstudio.com/AzureWiki/_wiki/wikis/AzureWiki.wiki/29758/Account-Creation-and-YubiKeys)
1. Ensure you have access to dashboards and queries in [Observability](../operations/observability.md)
1. Get familiar with information available on an IcM ticket by reviewing
   1. [PX Geneva ICM Alert](icm-ticket-guide.md)
   1. [PidlSdk Kensho ICM Alert](icm-ticket-guide-pidlsdk.md)

## During a Livesite
1. Review List of IcM for our Team
   1. Ack Incident and see if multiple incidents exist
1. Review PX Dashboard
1. Determine [severity](#severity) based on number of unique users affected
1. If the issue is because of a [dependency](#dependencies), engage corresponding service owner
1. If Sev 2 or higher and not mitigated within 15 minutes, request OSOC to engage EIM

## Standard Operations
1. [Take a region offline](take-dc-offline.md): If we see all the failures in one specific region, we may consider take the region offline to mitigate the issue. 

## Severity
|Unique users affected|Severity|
|----|----|
|Consumer stores - More than 1 or<br>Commercial stores - More than 1|Sev-4|
|Consumer stores - More than 100 or<br>Commercial stores - More than 10|Sev-3|
|Consumer stores - More than 1,000 or<br>Commercial stores - More than 30|Sev-2|
|Consumer stores - More than 10,000 or<br>Commercial stores - More than 300|Sev-1|

## Dependencies

### PX Service

#### Accounts
Implemented as [AccountServiceAccessor.cs](https://microsoft.visualstudio.com/DefaultCollection/Universal%20Store/_git/SC.CSPayments.PX?path=/private/Payments/PXService/Accessors/AccountService/AccountServiceAccessor.cs). Usages examples include: address managment, profile managment and address validation.
  * Prod endpoint : `https://accounts.cp.microsoft.com`
  * IcM On-call   : [Customer Insights First Party/ Customer Master](https://portal.microsofticm.com/imp/v3/oncall/current?serviceId=27369&teamIds=69008)
  * Service       : [Customer Master](https://servicetree.msftcloudes.com/#/ServiceModel/Service/Profile/8455d779-89f1-45c6-bf2c-b55ba37fdb67)
  * Support email : [JarvisCmSupport](mailto:jarviscmsupport)

#### Accounts Enrichment
Implemented as [AddressEnrichmentServiceAccessor.cs](https://microsoft.visualstudio.com/DefaultCollection/Universal%20Store/_git/SC.CSPayments.PX?path=/private/Payments/PXService/Accessors/AddressEnrichmentService/AddressEnrichmentServiceAccessor.cs). Usages examples include: address validation for both consumer and commercial flow
  * Prod endpoint : `https://avs.enrichment.pai.microsoft.com`
  * IcM On-call   : [Customer Insights First Party/ Customer Master](https://portal.microsofticm.com/imp/v3/oncall/current?serviceId=27369&teamIds=69008)
  * Service       : [Customer Master](https://servicetree.msftcloudes.com/#/ServiceModel/Service/Profile/8455d779-89f1-45c6-bf2c-b55ba37fdb67)
  * Support email : [JarvisCmSupport](mailto:jarviscmsupport)

#### Legacy accounts
Implemented as [CommerceAccountDataAccessor.cs](https://microsoft.visualstudio.com/DefaultCollection/Universal%20Store/_git/SC.CSPayments.PX?path=/private/Payments/PXService/Accessors/LegacyCommerceService/CommerceAccountDataAccessor.cs). Usage examples: Commerce account managment.
  * Prod endpoint : `https://sps.msn.com/Commerce/Account/AccountWebService.svc`
  * IcM On-call   : [Commerce Transaction Platform/CTP](https://portal.microsofticm.com/imp/v3/oncall/current?serviceId=29259&teamIds=82827)
  * Service       : [Classic CTP](https://servicetree.msftcloudes.com/#/ServiceModel/Service/Profile/9ce2a564-dc95-4cb9-92aa-f3f00856548d)
  * Support email : [CTP On Call](mailto:ctponcall@microsoft.com)


#### Legacy orders & subscriptions
Implemented as [CTPCommerceDataAccessor.cs](https://microsoft.visualstudio.com/DefaultCollection/Universal%20Store/_git/SC.CSPayments.PX?path=/private/Payments/PXService/Accessors/LegacyCommerceService/CTPCommerceDataAccessor.cs). Used for legacy subscription retrieval.
  * Prod endpoint : `https://sps.msn.com/CTPCommerce/CommerceAPI.svc`
  * IcM On-call   : [Commerce Transaction Platform/ StoreCore-PST-CTPAPIs](https://portal.microsofticm.com/imp/v3/oncall/current?serviceId=29259&teamIds=82926)
  * Service       : [Classic CTP](https://servicetree.msftcloudes.com/#/ServiceModel/Service/Profile/9ce2a564-dc95-4cb9-92aa-f3f00856548d)
    * Component : [CTP, MINT and SCS Endpoints](https://servicetree.msftcloudes.com/#/ServiceModel/Component/Profile/e45e603a-dc16-406b-97a1-19d8304e75f6)
  * Support email : [IshtiyaSDevs; CtpOwners](mailto:ishtiyasdevs;ctpowners)


#### OrchestrationService - PIMS
Implemented as [OrchestrationServiceAccessor.cs](https://microsoft.visualstudio.com/DefaultCollection/Universal%20Store/_git/SC.CSPayments.PX?path=/private/Payments/PXService/Accessors/OrchestrationService/OrchestrationServiceAccessor.cs). Used in Replace and Remove flows (ReplaceModernPi / RemoveModernPi).
  * Prod endpoint : `https://orchestration.paymentsinstrument.commerce.microsoft.com`
  * IcM On-call   : [Payment Services - ICM Tenant/ StoreCore-PST-PIMS](https://portal.microsofticm.com/imp/v3/oncall/current?serviceId=23443&teamIds=45908)
  * Service       : [PIMS (Payment Instrument Management Service)](https://servicetree.msftcloudes.com/#/ServiceModel/Service/Profile/0b98fe24-9a94-4657-8f7e-c4f10d3443df)
  * Support email : [PimsEngg;PimsOncallTask](mailto:pimsengg;pimsoncalltask)


#### PayMod - PayerAuth
Implemented as [PayerAuthServiceAccessor.cs](https://microsoft.visualstudio.com/DefaultCollection/Universal%20Store/_git/SC.CSPayments.PX?path=/private/Payments/PXService/Accessors/PayerAuthService/PayerAuthServiceAccessor.cs). Used to create payment session ids, authentication and challenge completion.
  * Prod endpoint : `https://payerauthservice.cp.microsoft.com/PayerAuthService`
  * IcM On-call   : [Payment Services - ICM Tenant/ StoreCore-PST-TX](https://portal.microsofticm.com/imp/v3/oncall/current?serviceId=23443&teamIds=75758)
  * Service       : [Paymod (Payment Transaction Services)](https://servicetree.msftcloudes.com/#/ServiceModel/Service/Profile/64b14190-f480-4781-a67c-55526002f3c2)
  * Support email : [PayTranOncall](mailto:paytranoncall)

#### PayMod - Transaction
Implemented as [TransactionServiceAccessor.cs](https://microsoft.visualstudio.com/DefaultCollection/Universal%20Store/_git/SC.CSPayments.PX?path=/private/Payments/PXService/Accessors/TransactionService/TransactionServiceAccessor.cs). Usages include: payment object creation and cvv validation.
  * Prod endpoint : `https://paymentstransactionservice.cp.microsoft.com/transactionService`
  * IcM On-call   : [Payment Services - ICM Tenant/ StoreCore-PST-TX](https://portal.microsofticm.com/imp/v3/oncall/current?serviceId=23443&teamIds=75758)
  * Service       : [Paymod (Payment Transaction Services)](https://servicetree.msftcloudes.com/#/ServiceModel/Service/Profile/64b14190-f480-4781-a67c-55526002f3c2)
  * Support email : [PayTranOncall](mailto:paytranoncall)

#### PayMod - Session
Implemented as [SessionServiceAccessor.cs](https://microsoft.visualstudio.com/DefaultCollection/Universal%20Store/_git/SC.CSPayments.PX?path=/private/Payments/PXService/Accessors/SessionService/SessionServiceAccessor.cs). Usages include: Generating session id, get session data, create session from data, update session data.
  * Prod endpoint : `https://sessionservice.cp.microsoft.com`
  * IcM On-call   : [Payment Services - ICM Tenant/ StoreCore-PST-TX](https://portal.microsofticm.com/imp/v3/oncall/current?serviceId=23443&teamIds=75758)
  * Service       : [Paymod (Payment Transaction Services)](https://servicetree.msftcloudes.com/#/ServiceModel/Service/Profile/64b14190-f480-4781-a67c-55526002f3c2)
  * Support email : [PayTranOncall](mailto:paytranoncall)

#### PIMS
Implemented as [PIMSAccessor.cs](https://microsoft.visualstudio.com/DefaultCollection/Universal%20Store/_git/SC.CSPayments.PX?path=/private/Payments/PXService/Accessors/PIMS/PIMSAccessor.cs). Usages include: retrieving payment methods and payment instrument managment.
  * Prod endpoint : `https://paymentsinstrumentservice.cp.microsoft.com/InstrumentManagementService`
  * IcM On-call   : [Payment Services - ICM Tenant/ StoreCore-PST-PIMS](https://portal.microsofticm.com/imp/v3/oncall/current?serviceId=23443&teamIds=45908)
  * Service       : [PIMS (Payment Instrument Management Service)](https://servicetree.msftcloudes.com/#/ServiceModel/Service/Profile/0b98fe24-9a94-4657-8f7e-c4f10d3443df)
  * Support email : [PimsEngg;PimsOncallTask](mailto:pimsengg;pimsoncalltask)

#### M$ MDollar Purchase
Implemented as [PurchaseServiceAccessor.cs](https://microsoft.visualstudio.com/DefaultCollection/Universal%20Store/_git/SC.CSPayments.PX?path=/private/Payments/PXService/Accessors/PurchaseService/PurchaseServiceAccessor.cs). Usages include: Subscription retrieval and order retrieval.
  * Prod endpoint : `https://purchase.md.mp.microsoft.com`
  * IcM On-call   : [UST Purchase/ PurchasePD](https://portal.microsofticm.com/imp/v3/oncall/current?serviceId=22602&teamIds=38418)
  * Service       : [UniversalStorePurchase](https://servicetree.msftcloudes.com/#/ServiceModel/Service/Profile/a7520d0b-248d-43c1-9560-7dce340e7fb9)
  * Support email : [MdPurchase911](mailto:mdpurchase911)

#### Token Policy Service
Implemented as [TokenPolicyServiceAccessor.cs](https://microsoft.visualstudio.com/DefaultCollection/Universal%20Store/_git/SC.CSPayments.PX?path=/private/Payments/PXService/Accessors/TokenPolicyService/TokenPolicyServiceAccessor.cs). Usages include: Subscription retrieval and order retrieval.
  * Prod endpoint : `https://tops.mp.microsoft.com`
  * IcM On-call   : [UST Purchase/ PurchasePD](https://portal.microsofticm.com/imp/v3/oncall/current?serviceId=22602&teamIds=38418)
  * Service       : [UniversalStorePurchase](https://servicetree.msftcloudes.com/#/ServiceModel/Service/Profile/a7520d0b-248d-43c1-9560-7dce340e7fb9)
  * Support email : [MdPurchase911](mailto:mdpurchase911)

#### Microsoft Rewards
Implemented as [MSRewardsServiceAccessor.cs](https://microsoft.visualstudio.com/DefaultCollection/Universal%20Store/_git/SC.CSPayments.PX?path=/private/Payments/PXService/Accessors/MSRewardsService/MSRewardsServiceAccessor.cs). Usages include: Subscription retrieval and order retrieval.
  * Prod endpoint : `https://prod.rewardsplatform.microsoft.com`
  * IcM On-call   : [Bing / Microsoft Rewards](https://portal.microsofticm.com/imp/v3/oncall/current?serviceId=25120&teamIds=53471)
  * Service       : [MicrosoftRewards]((https://microsoftservicetree.com/services/34e9703a-61ed-4cbe-8a5e-fda321ae1efa)
  * Support email : [brserdri](brserdri@microsoft.com)

#### BigCat Catalog
Implemented as [CatalogServiceAccessor.cs](https://microsoft.visualstudio.com/DefaultCollection/Universal%20Store/_git/SC.CSPayments.PX?path=/private/Payments/PXService/Accessors/CatalogService/CatalogServiceAccessor.cs). Usages include: product and market retrieval.
  * Prod endpoint : `https://frontdoor-displaycatalog.bigcatalog.microsoft.com`
  * IcM On-call   : [UST Catalog/ BigCat Frontdoor](https://portal.microsofticm.com/imp/v3/oncall/current?serviceId=22607&teamIds=82644)
  * Service tree  : [BigCat Frontdoor](https://servicetree.msftcloudes.com/#/ServiceModel/Service/Profile/1b312ad3-d6f0-485f-a824-37fa2bbe1f02)
  * Support email : [MdCatalog911](mailto:mdcatalog911)

#### Risk (Engineering)
Implemented as [RiskServiceAccessor.cs](https://microsoft.visualstudio.com/DefaultCollection/Universal%20Store/_git/SC.CSPayments.PX?path=/private/Payments/PXService/Accessors/RiskService/RiskServiceAccessor.cs)
  * Prod endpoint : `https://ks.cp.microsoft.com`
  * IcM On-call   : [Modern Risk Service/ MCKP_Risk](https://portal.microsofticm.com/imp/v3/oncall/current?serviceId=26070&teamIds=66239)
  * Service       : [Modern Risk Service](https://servicetree.msftcloudes.com/#/ServiceModel/Service/Profile/cb94646f-318c-4340-bd7a-c7533d0eb7b4)
  * Support email : [MckpRiskDri](mailto:mckpriskdri)

#### Risk (Sciences)
Implemented as [RiskServiceAccessor.cs](https://microsoft.visualstudio.com/DefaultCollection/Universal%20Store/_git/SC.CSPayments.PX?path=/private/Payments/PXService/Accessors/RiskService/RiskServiceAccessor.cs)
  * IcM On-call   : [Modern Risk Service/ MCKP_Risk_Sciences](https://portal.microsofticm.com/imp/v3/oncall/current?serviceId=26070&teamIds=66347)
  * Service       : [Modern Risk Service](https://servicetree.msftcloudes.com/#/ServiceModel/Service/Profile/cb94646f-318c-4340-bd7a-c7533d0eb7b4)
  * Support email : [Risk1pDs;1pRiskDev](mailto:risk1pds;1priskdev)

#### Tax Id
Implemented as [TaxIdServiceAccessor.cs](https://microsoft.visualstudio.com/DefaultCollection/Universal%20Store/_git/SC.CSPayments.PX?path=/private/Payments/PXService/Accessors/TaxService/TaxIdServiceAccessor.cs). Used for tax id retrieval.
  * Prod endpoint : `https://taxidmanagement.cp.microsoft.com`
  * IcM On-call   : [Billing Services/ StoreCore-BST-Core](https://portal.microsofticm.com/imp/v3/oncall/current?serviceId=20633&teamIds=27156) 
  * Service       : [Tax Service](https://servicetree.msftcloudes.com/#/ServiceModel/Service/Profile/2703dde6-4490-45e3-af2d-4470cf553a87)
  * Support email : [TaxServiceTeamSg](mailto:taxserviceteamsg)

#### Infrastructure (Server login, ATM, AKS, etc.)
  * Prod endpoint : N/A
  * IcM On-call   : [Payment Services - ICM Tenant/ StoreCore-PST-SRE](https://portal.microsofticm.com/imp/v3/oncall/current?serviceId=23443&teamIds=45248)
  * Service       : [Payments SRE Infra & POC](https://servicetree.msftcloudes.com/main.html#/ServiceModel/Service/Profile/87df3e45-57a1-429e-b3b0-63501965eeeb)
  * Support email : [UST-PST-SRE;PayTaskReq](mailto:UST-PST-SRE;paytaskreq)

#### Azure App Service (Web Apps) A.K.A. Antares
  * Prod endpoint : N/A
  * IcM On-call   : [Antares APIHub Loop](https://portal.microsofticm.com/imp/v3/oncall/current?serviceId=10060&teamIds=0)
  * Service       : N/A
  * API Hub Loop  : [ANTAPIHubLoop](mailto:ANTAPIHubLoop@microsoft.com)
  * Discussions   : [antr-talk](mailto:antr-talk@microsoft.com)

#### Network/PCE (Connectivity issues, PCE etc.)
  * Prod endpoint : N/A
  * IcM On-call   : [CCF Infrastructure Services/COMIAM](https://portal.microsofticm.com/imp/v3/oncall/current?serviceId=20506&teamIds=72262)
  * Service       : [ALL - [ND] Network Devices](https://servicetree.msftcloudes.com/#/ServiceModel/Service/Profile/df4d066a-1f39-4cec-8ab2-20d4096fe44c)
  * Support email : [ComIam](mailto:comiam@microsoft.com)

#### Stored Value
  * Prod endpoint : `https://storedvalue.cp.microsoft.com`
  * IcM On-call   : 
  * Service       : [CSV Funding Service](https://servicetree.msftcloudes.com/?pathSet=true#/ServiceModel/Home/4e794e19-3a46-4735-a766-8d64d9128ea7)
  * Support email : [Balance Management Online](bmo59s@microsoft.com)

### PIDLSDK

#### Accounts FD 
  * Prod endpoint : `https://jcmsfd.account.microsoft.com`
  * IcM On-Call   : [Customer Insights First Party/ Customer Master FrontDoor](https://portal.microsofticm.com/imp/v3/oncall/current?serviceId=27369&teamIds=71195&shiftType=current)
  * Service       : [Customer Master FrontDoor](https://servicetree.msftcloudes.com/#/ServiceModel/Component/Profile/88c81a7f-8171-42b4-ba5c-7abb5ae64496)
  * Support email : [JarvisCmSupport](mailto:jarviscmsupport)

#### HAPI 
  * Prod endpoint : `https://commerceapi.office.net`
  * IcM On-Call   : [Demeter-CommerceApi/ CXG Purchase](https://portal.microsofticm.com/imp/v3/oncall/current?serviceId=25648&teamIds=58216)
  * Service       : [Demeter-CommerceApi](https://servicetree.msftcloudes.com/#/ServiceModel/Service/Profile/df684dd7-e3af-4e80-9466-fb81a47326cf)
  * Support email : [DemeterCommerceApi](mailto:demetercommerceapi)

### Azure

#### Azure App Service (Web Apps) A.K.A. Antares
Note: Prefer to contact "Windows Azure Websites Servicing" first.
  * Prod endpoint : N/A
  * IcM On-call   : [Antares APIHub Loop](https://portal.microsofticm.com/imp/v3/oncall/current?serviceId=10060&teamIds=0&scheduleType=current&shiftType=current&viewType=1)
  * Service       : N/A
  * API Hub Loop  : [ANTAPIHubLoop](mailto:ANTAPIHubLoop@microsoft.com)
  * Discussions   : [antr-talk](mailto:antr-talk@microsoft.com)

#### Windows Azure Websites Servicing:
 * Prod endpoint : N/A
 * IcM On-call   : [Windows Azure Websites Servicing](https://portal.microsofticm.com/imp/v3/oncall/current?serviceId=10060&teamIds=10468)
 * Service       : [App Service (Web Apps)](https://servicetree.msftcloudes.com/main.html#/ServiceModel/Service/Profile/df36aee8-c644-400b-a0ab-fd0f1191211d)
 * Support email : [antst](mailto:antst@microsoft.com)
 * Discussions   : [antr-talk](mailto:antr-talk@microsoft.com)

## JIT (Just-In-Time) Access
During a livesite, prefer involving the SRE team and having them make the production changes. However, for documentation purposes, these are the instructions to request JIT access. Refer to [OneNote](https://microsoft.sharepoint.com/teams/PaymentExperience/_layouts/15/Doc.aspx?sourcedoc=%7B171acb93-a045-434e-938d-ccefe55457ad%7D&action=edit&wd=target(PX%20Azurification.one%7Ca7020d78-b8b0-46e6-bc1d-b0d07d02ede6%2FJIT%20Access%7Cdbbba6a1-0867-4879-b673-789dc54cf9e2%2F)&share=IgGTyxoXRaBOQ5ONzO_lVFetAVuTzYY8wayvlF_mOXFb_MA) for original documentation.

- Use your SAW machine to open the [JIT portal](https://jitaccess.security.core.windows.net/WorkFlowTempAccess.aspx)
- Log in using pme.glb account. In the login page, select "sign in using an X.509 certificate" (make sure your YubiKey is loaded with certificates and inserted into SAW)
- Select the correct certificate in the pop-up window
- Select YubiKey as the smart card device (touch the copper part if the Yubikey option is not shown)
- Enter the pin of YubiKey in the next pop-up window then log in to JIT portal
- Submit a request of JIT access on JIT portal. Details to be filled
  - WorkItem Source: "other" or "IcM" 
  - Work Item ID: Task id (when choosing "other" as WorkItem source) or IcM id (when choosing "IcM" as WorkItem source).
  - Justification: Provide justification for PROD Access.
  - Resource Type: Select Portal from the dropdown list.
  - Subscription ID:Ex: PME INT Subscription ID(230ef3cc-8fdd-4f26-bf9c-10131b4080e5). Please find PX Azure PME subscription IDs below
  - Click on Validate & Add Resource 
  - Click on Submit 
  - Once the Request is submitted using above details, Approvers will get an email with the approval link and they need to review and approve the access from SAW device.

  |Subscription|Id|
  |----|----|
  |PME Geneva INT|86facc56-d6cb-484f-bc4e-9a7a7a848266 |
  |PME Geneva PROD|6c788ee2-6f07-48d2-b285-e56ececdcd99 |
  |PME INT|230ef3cc-8fdd-4f26-bf9c-10131b4080e5|
  |PME PROD|9b6168fd-7d68-47e1-9c71-e51828aa62c0|

## Footnotes
- [idweb](https://idweb/identitymanagement/default.aspx) seems to open only in Edge (and not in Chrome)

---
For questions/clarifications, email [author/s of this doc and PX support](mailto:kowshikpfte@microsoft.com?cc=PXSupport@microsoft.com&subject=Docs%20-%20operations/livesite-sop.md).
---