# Glossary

## Target audience
Developers and PMs contributing code or docs to PX

### AKV
#### Azure Key Vault

PX service relies on [AKV](https://docs.microsoft.com/en-us/azure/key-vault/general/overview) to store secrets like certificates private keys.

### AAD
#### Azure Active Directory

PX relies on [AAD](https://azure.microsoft.com/en-us/services/active-directory/#overview) to authenticate callers by validating AAD tokens in incoming requests. It also relies on 
AAD to create tokens when making outgoing calls to other services that accept AAD tokens.

### AMC
#### Account Microsoft Com
Refers to the website https://account.microsoft.com which is the portal for customers to manage their Microsoft account.  
Also used to refer to the team that owns the website.

### ATM
#### Azure Traffic Manager

[ATM](https://docs.microsoft.com/en-us/azure/traffic-manager/traffic-manager-overview) distributes traffic from PIFD service to various Azure regions where PX service is deployed.

### SC
#### Service Change

Its a type of record in ADO used to track PROD changes like deployment and corresponding approvals.

Example: 
[Service Change 36789476](https://microsoft.visualstudio.com/OSGS/_workitems/edit/36789476): Automated 
Compliance Record: Payment Experience Service

### ECR
#### Emergency Certificate Rotation

If a certificate is compromised, services need to quickly rotate such certificates in production.  ECR 
describes that situation.  ECR drill is an exercise to ensure that ECR capability is ready for a real 
emergency.

### MSN Account
#### Microsoft Network Account
Also known as Microsoft account.  
Refers to accounts with emails ending commonly in @hotmail.com, @live.com, @outlook.com, etc.

### PIFD
#### Payment Instrument Front Door

This is a front door service that authenticates user tokens (e.g. MSA / AAD) and make calls to mid-tier and back-end services.

### PX
#### Payment Experience

Depending on the context, this could either mean the [team](https://microsoft.sharepoint.com/teams/PaymentExperience) responsible for Payment Experiences across Microsoft or the implementations 
(PX Service, PIDLSDK, MSPayments, etc.) currently in use.

### OSOC
#### [One Store Ops Center](https://microsoft.sharepoint.com/teams/OSOC1/SitePages/Home.aspx)

A team that is dedicated to manage outages and livesite issues. When we get a Sev-2 or higher incident, OSOC joins
the bridge and helps with communication, getting more teams engaged as needed to mitigate the issue etc. 

---
For questions/clarifications, email 
[author/s of this doc and PX support](mailto:kowshikpfte@microsoft.com?cc=PXSupport@microsoft.com&subject=Docs%20-%20glossary.md).
---