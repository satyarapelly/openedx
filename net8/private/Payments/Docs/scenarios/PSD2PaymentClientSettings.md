# PSD2 Settings versions - PaymentClientSettings.json file

## Overview
Certificates for the various payment types, which are recorded in the PaymentClientSettings.json file, expire after a few years and it is our responsibility to update them with renewed versions before the expiration date. 

All new changes should be made in a new versions folder (Pidl > PXService > App_Data > PSD2_Config), using the PaymentClientSettings.json file from the last appropirate version folder and only changing the fields that need to be upgraded. 

## Useage

New versions of PaymentClientSettings.json are added to the PSD2_Config folder based on new requirements. Examples include 

- New card certificates
- Upgrades keys 
- New deviceDataParameters requirements given by EmVCO
- New url endpoints that need to be hit by the 3ds2-sdk

For every new json file, there must be a version number bump

3DSSDK makes the following GET call to get the latest version based on the highest available flight: 
`GET /v6.0/settings/Microsoft.Payments.Client/sdk3.1-V23/`

Validates correct/highest version via the `/authenticate` or `/createAndAuthenticate` call, on the `ValidateSettingsVersion()` function

Hits PX Settings controller to get the appropriate PaymentClientSettings.json file

## Flighting

All versions are handled by a flight : **PXPSD2SettingVersionV[version#]**

    ie. PXPSD2SettingVersionV23

The flight is controlled on the Azure Exp level, usually advancing in the following the manner: 

- Day 1: 1% 
- Day 2: 5%
- Day 3: 25%
- Day 4: 100%

Use the following queries to ensure that the new versions are being found: 

```
// -------------------------- Check if new settings flight is being passed to all possible calls---------------------------

RequestTelemetry
| where TIMESTAMP > ago(5h) // update time
| project data_ResponseHeader, todatetime(TIMESTAMP)
| summarize V22Flight_only = countif(data_ResponseHeader has "PXPSD2SettingVersionV22" and data_ResponseHeader !has "PXPSD2SettingVersionV23"),
    V23Flight_only = countif(data_ResponseHeader has "PXPSD2SettingVersionV23" and data_ResponseHeader !has "PXPSD2SettingVersionV22"),
    BothFlights = countif(data_ResponseHeader has "PXPSD2SettingVersionV22" and data_ResponseHeader has "PXPSD2SettingVersionV23") by bin (TIMESTAMP, 30m) // update time
| render timechart with (ycolumns=V22Flight_only, V23Flight_only, BothFlights) 

// -------------------------- Check if challenges are completing with the new flight + the new delta to the PyamentClientSettings.json ---------------------------
let startTime = ago(4d);
let endtime = ago(1m);
PSD2AppAuthenticateEvents(startTime, endtime, true) | join kind=inner PSD2PayerAuthAuthenticateEvents(startTime, endtime, true) on sessionId | join kind=inner PSD2PayerAuthCompleteChallengeEvents(startTime, endtime) on sessionId
| where settingsVersion in ("V23") // change version
| where channel == "app_based" and paymentMethodType == "visa" // change the PMT to the delta of the new file
| where transStatus  == "C"
//| project settingsVersion, httpStatus, transStatus, sessionId;
| summarize totalChallenges = count() by transStatus1, bin(todatetime(transTime), 6h)
| render timechart with (ycolumns=totalChallenges)
```
** Note: **  

transStatus > coming from PSD2PayerAuthAuthenticateEvents, shows if a challenge *needs to be* initiated
transStatus1 > coming from PSD2PayerAuthCompleteChallengeEvents, shows the result of the rendered challenge


Read more about the specific columns here: https://docs.3dsecure.io/3dsv2/specification_220.html#attr-ARes-transStatus


## File breakdown
- certs-V#
    - Contains the DsCertificates.json file, which includes the directory root server certificates, commonly known as dsCertificates, of the Payment Method Type. 
    - the dsCertificates are listed in array format under the property "caRootCertificates". 
    - The following These certificates will be used and validated in PaymentSessionHandler.cs and PaymentPSD2CertificateValidator. 
- Sdk2.2-V#
    - Contains a PaymentClientSettings.json file, which will be accessed in the latest SDK update
- Sdk2-V# and V#
    - Contains a PaymentClientSettings.json file, which is used for backwards compatibility for partners who have not upgraded to sdk2.2

As we upgrade the sdk versions, new folders will contain the latest files

### Example PR https://microsoft.visualstudio.com/Universal%20Store/_git/SC.CSPayments.PX/pullrequest/11072963

## Contacts: 
Kolby Chien

Anushri Marar

Selvan Ramasamy
