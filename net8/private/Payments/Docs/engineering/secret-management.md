# Secret Management

## Target audience
PX Engineering

## Prerequisites
N/A

## Overview
This doc covers secret management of PX Service.

## PX certificate location
|Enviroment|KV name|Subscription
|--|--|--|
|INT|[PX-KV-INT](https://portal.azure.com/#@mspmecloud.onmicrosoft.com/resource/subscriptions/230ef3cc-8fdd-4f26-bf9c-10131b4080e5/resourceGroups/PX-Infra-INT-CentralUS/providers/Microsoft.KeyVault/vaults/PX-KV-INT/overview)|[STORECORE-PST-PX-INT](https://portal.azure.com/#@mspmecloud.onmicrosoft.com/resource/subscriptions/230ef3cc-8fdd-4f26-bf9c-10131b4080e5/overview)
|INT|[PX-TestKV-INT](https://portal.azure.com/#@mspmecloud.onmicrosoft.com/resource/subscriptions/230ef3cc-8fdd-4f26-bf9c-10131b4080e5/resourceGroups/PX-Infra-INT-CentralUS/providers/Microsoft.KeyVault/vaults/PX-TestKV-INT/overview)|[STORECORE-PST-PX-INT](https://portal.azure.com/#@mspmecloud.onmicrosoft.com/resource/subscriptions/230ef3cc-8fdd-4f26-bf9c-10131b4080e5/overview)
|PPE/PROD|[PX-KV-PROD](https://portal.azure.com/#@mspmecloud.onmicrosoft.com/resource/subscriptions/9b6168fd-7d68-47e1-9c71-e51828aa62c0/resourceGroups/PX-Infra-PROD-WestUS/providers/Microsoft.KeyVault/vaults/PX-KV-PROD/overview)|[STORECORE-PST-PX-PROD](https://portal.azure.com/#@mspmecloud.onmicrosoft.com/resource/subscriptions/9b6168fd-7d68-47e1-9c71-e51828aa62c0/overview)
|PPE/PROD|[PX-TestKV-PROD](https://portal.azure.com/#@mspmecloud.onmicrosoft.com/resource/subscriptions/9b6168fd-7d68-47e1-9c71-e51828aa62c0/resourceGroups/PX-Infra-PROD-WestUS/providers/Microsoft.KeyVault/vaults/PX-TestKV-PROD/overview)|[STORECORE-PST-PX-PROD](https://portal.azure.com/#@mspmecloud.onmicrosoft.com/resource/subscriptions/9b6168fd-7d68-47e1-9c71-e51828aa62c0/overview)

## PX certificate inventory
[This excel sheet](https://microsoft.sharepoint.com/:x:/t/PaymentExperience/EWWx6SVhf0dBobRPHGuT7UAB3DlDwbke1ZK5pbHBi9kJPQ?e=8QtU57) includes information about current INT/PPE/PROD PX certificates. 


## ECR drill and certificate renewal
[ECRDrill.md](../operations/ECRDrill.md) describes the process for ECR drill and certificaite renewal.

## Read next 
N/A

---
For questions/clarifications, email [author/s of this doc and PX support](mailto:wwei@microsoft.com?cc=PXSupport@microsoft.com&subject=Docs%20-%20development/secret-management.md).
<!--
- Replace "kowshikpfte" with the document owner's alias
- Replace "development/doc-template.md" with the actual file name
-->

---