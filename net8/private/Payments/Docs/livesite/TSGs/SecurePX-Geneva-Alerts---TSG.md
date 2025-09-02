# SecurePX Geneva Alerts - TSG

# SecurePX Service Overview

## What is SecurePX Service:
SecurePX service is a backend API which serves the contents (securefield.html and app.js) for the secure fields in the PidlSDK. The secure field feature is currently being flighted in the northstarweb partner for the add credit card flow. The flight being used to enable this feature is "PXEnableSecureFieldAddCreditCard".

## Explanation of the Impact:
If the SecurePX service fails or experiences issues, it could disrupt the add credit card flow for users for the partner for which secure field functionality has been enabled, potentially leading to a loss of transactions and customer dissatisfaction.

## Severity Levels:
- **Sev0**: Entire platform globally offline, directly impacting customers.
- **Sev1**: Multi-Region, multi-service, or major service degradation issue.
- **Sev2**: Partial workflow or use case issue impacting service(s) or customer experience.
- **Sev3**: Urgent or high business impact with no SLA impact.
- **Sev4**: Not urgent, no SLA impact such as routine maintenance.

## SLA Definition for Incidents:
- **Sev1/2**: Mitigation goal within 60 minutes (TTM < 60).
- **Sev3/4**: No specific SLA impact.

## Resolving ICMs:
To check the metrics related to the health of the SecurePX service, the following dashboard can be used:
- **SecurePX Service Dashboard** - (https://amg-paymentexperience-d2c9cydkephxfwgq.wus3.grafana.azure.com/d/ab34b552-b89e-49f3-bd5d-3b280c5520bd/securepx-service-dashboard?orgId=1 )

## Investigation Steps:
1. Access the SecurePX Service Dashboard to review current metrics and identify anomalies.
2. Analyze logs within the last 24 hours to pinpoint any errors or unusual patterns.
3. Verify if the flight "PXEnableSecureFieldAddCreditCard" is active and functioning correctly.

## Root Cause Analysis (RCA):
Once the issue is identified, document the root cause to understand the underlying problem and prevent future occurrences.

## Fast Mitigation Techniques:
- Stop the flight "PXEnableSecureFieldAddCreditCard" to halt traffic to the SecurePX service temporarily.
- Consider failover to a backup service if available.

## Validation Steps:
1. Confirm that the flight "PXEnableSecureFieldAddCreditCard" is stopped.
2. Monitor the SecurePX Service Dashboard to ensure metrics return to normal levels.
3. Conduct a test transaction to verify the add credit card flow is operational.

## Escalation Contact or Point-of-Contact (POC):
For further assistance, contact the SecurePX service team at pxoncall@microsoft.com.

## Acronyms:
- **API**: Application Programming Interface
- **ICM**: Incident Management
- **TTM**: Time to Mitigate

Note: In case of issues with the service and uncertainty on how to resolve them, the flight "PXEnableSecureFieldAddCreditCard" can be stopped, which should stop sending traffic to the SecurePX service.

### Airgap / Sovereign Cloud Deployments
To find deployments to Sovereign Clouds (USNat, USSec, etc.) for this service, navigate to Azure Bridge and filter by the service's name.

## Public Cloud Deployments

Below are links to the EV2 rollouts in the last 14 days for the service "Payment Experience Service" (ServiceTreeID: e50abb8e-e976-4311-b12b-85156f4abc0e) service groups:

| Service Group                               | Classic EV2 Portal             | New EV2 Portal                 |
|---------------------------------------------|--------------------------------|--------------------------------|
| microsoft.staticresources                   | [Link](https://ev2portal.azure.net/#/?servicegroups=microsoft.staticresources&daterange=14&rolloutinfra=Prod)    | [Link](https://ra.ev2portal.azure.net/#/PROD/e50abb8e-e976-4311-b12b-85156f4abc0e/microsoft.staticresources?dateRange=14)    |
| securepx                                    | [Link](https://ev2portal.azure.net/#/?servicegroups=securepx&daterange=14&rolloutinfra=Prod)    | [Link](https://ra.ev2portal.azure.net/#/PROD/e50abb8e-e976-4311-b12b-85156f4abc0e/securepx?dateRange=14)    |
| microsoft.cfs.pc.sellermarketplace.prod     | [Link](https://ev2portal.azure.net/#/?servicegroups=microsoft.cfs.pc.sellermarketplace.prod&daterange=14&rolloutinfra=Prod)    | [Link](https://ra.ev2portal.azure.net/#/PROD/e50abb8e-e976-4311-b12b-85156f4abc0e/microsoft.cfs.pc.sellermarketplace.prod?dateRange=14)    |
| 3ppmarketplace                              | [Link](https://ev2portal.azure.net/#/?servicegroups=3ppmarketplace&daterange=14&rolloutinfra=Prod)    | [Link](https://ra.ev2portal.azure.net/#/PROD/e50abb8e-e976-4311-b12b-85156f4abc0e/3ppmarketplace?dateRange=14)    |
| securepxservice                             | [Link](https://ev2portal.azure.net/#/?servicegroups=securepxservice&daterange=14&rolloutinfra=Prod)    | [Link](https://ra.ev2portal.azure.net/#/PROD/e50abb8e-e976-4311-b12b-85156f4abc0e/securepxservice?dateRange=14)    |
| aad first party git                         | [Link](https://ev2portal.azure.net/#/?servicegroups=aad%20first%20party%20git&daterange=14&rolloutinfra=Prod)    | [Link](https://ra.ev2portal.azure.net/#/PROD/e50abb8e-e976-4311-b12b-85156f4abc0e/aad%20first%20party%20git?dateRange=14)    |
| pss                                         | [Link](https://ev2portal.azure.net/#/?servicegroups=pss&daterange=14&rolloutinfra=Prod)    | [Link](https://ra.ev2portal.azure.net/#/PROD/e50abb8e-e976-4311-b12b-85156f4abc0e/pss?dateRange=14)    |
| microsoft.cfs.pc.px.prod                    | [Link](https://ev2portal.azure.net/#/?servicegroups=microsoft.cfs.pc.px.prod&daterange=14&rolloutinfra=Prod)    | [Link](https://ra.ev2portal.azure.net/#/PROD/e50abb8e-e976-4311-b12b-85156f4abc0e/microsoft.cfs.pc.px.prod?dateRange=14)    |
| microsoft.cfs.pc.staticresources.prod       | [Link](https://ev2portal.azure.net/#/?servicegroups=microsoft.cfs.pc.staticresources.prod&daterange=14&rolloutinfra=Prod)    | [Link](https://ra.ev2portal.azure.net/#/PROD/e50abb8e-e976-4311-b12b-85156f4abc0e/microsoft.cfs.pc.staticresources.prod?dateRange=14)    |
| px                                          | [Link](https://ev2portal.azure.net/#/?servicegroups=px&daterange=14&rolloutinfra=Prod)    | [Link](https://ra.ev2portal.azure.net/#/PROD/e50abb8e-e976-4311-b12b-85156f4abc0e/px?dateRange=14)    |