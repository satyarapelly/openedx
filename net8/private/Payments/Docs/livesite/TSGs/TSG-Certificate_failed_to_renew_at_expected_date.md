# TSG: Certificate failed to renew at expected date

1. In Azure Key Vault Service, some certificates are configured for renewal after certain time. But somehow it has failed to renew an Incident will get created automatically.

2. The body of the incident should indicate the following data items:
    · The subscription ID

    · The service ID

    · The vault name

    · The certificate name

    · The expiration date of the certificate

    · The expected renewal date for the certificate

    · A failure reason

    · A failure message

    · Debugging information

The failure reason will give a general category of the failure. The failure message will provide details of the problem with the certificate renewal request.

3. It can be failed due to various reason which you can find in the below TSG 
TSG: [TSG: Certificate failed to renew at expected date | website (eng.ms)](https://eng.ms/docs/products/onecert-certificates-key-vault-and-dsms/key-vault-dsms/autorotationandecr/certfailedrenewalTSG)

4. It has been determined that the reason is an error in the certificate configuration that needs to be fixed by the owner of the certificate.

5. For renewing that certificate, we need to create one task for service change. For reference,
[Service Change 41785654: Manually renew PX "sslppecertificate" & "paymentexperience-cp-microsoft-int-com" certs to test OneCert configuration - Boards (visualstudio.com)](https://microsoft.visualstudio.com/OSGS/_workitems/edit/41785654)

6. Deployment Approver (DA) field in the task needs to be only updated by Approver and not by us (email/ping Kowshik to provide approval).

7. After the DA is approved, we need to drop an email to SRE team at PayTaskReq@microsoft.com by looping PXOnCall in CC and ask them to follow service change task and help us to renew the certs using following details present in the task itself:

    a. PME subscription:

    b. KV name:

    c. Certificate name:

    for eg. “paymentexperience-cp-microsoft-int-com”, “sslppecertificate”

8. Once the changes have been done by the SRE team, please follow the service change task and monitor the reliability dashboards to check if there is a drop in reliability due to recent changes, if there is a drop in reliability, work with SRE to revert.

9. For reference: 
    1. [Incident 341320012](https://portal.microsofticm.com/imp/v3/incidents/details/341320012/home) : [Publisher-Prod] Azure Key Vault--Key Vault Certificate Failed to Renew for: wus/px-kv-prod/paymentexperience-cp-microsoft-int-com.
    2. [Incident 341522065](https://portal.microsofticm.com/imp/v3/incidents/details/341522065/home) : [Publisher-Prod] Azure Key Vault--Key Vault Certificate Failed to Renew for: wus/px-kv-prod/sslppecertificate.

10. For Reference related links are: 
    1. [OneCert Customer Guide | OneCert Customer Guide (eng.ms)](https://eng.ms/docs/products/onecert-certificates-key-vault-and-dsms/onecert-customer-guide/docs)

    2. This incident was transfered to the impacted service team based on ServiceTree metadata, more information can be found at https://aka.ms/ASMAlertsMetadata.
