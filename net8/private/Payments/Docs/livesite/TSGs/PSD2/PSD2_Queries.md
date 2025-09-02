# PSD2 Queries

PSD2 calls by CV

RequestTelemetry
| where TIMESTAMP between (datetime(2023-06-12 00:00:00Z)..datetime(2023-06-13 20:07:00Z))
| where cV startswith "odxHYSqVfL35ut6FEWZynY"
| where data_baseData_operationName in ("PayerAuthService-POST-Authenticate", "PayerAuthService-POST-GetThreeDSMethodURL", "MasterCardAuthenticateRequest"," PayerAuthService-POST-CompleteChallenge")
| project TIMESTAMP, cV, data_baseData_targetUri, data_baseData_operationName, data_RequestDetails, data_ResponseDetails


PSD2 calls by sessionId
let sessionId = "Z10074C8INV2ee8a76fb-2420-4d99-bd95-2b70f1842a24";
RequestTelemetry
| where TIMESTAMP between (datetime(2023-06-12 00:00:00Z)..datetime(2023-06-13 20:07:00Z))
| where (data_RequestDetails contains sessionId
    or data_ResponseDetails contains sessionId
    or data_baseData_targetUri contains sessionId)
| where data_baseData_operationName in ("PayerAuthService-POST-Authenticate", "PayerAuthService-POST-GetThreeDSMethodURL", "MasterCardAuthenticateRequest","PayerAuthService-POST-CompleteChallenge")
| project TIMESTAMP, cV, data_baseData_targetUri, data_baseData_operationName, data_RequestDetails, data_ResponseDetails


Number of users impacted by Safetynet transactions which failed

    • Safetynet catches transactions which have a failure from a downstream external dependent service and attempts to execute the charge, even though authentication was not completed.  Banks can still reject the charge for not having authentication completed and this query lets you know the amount of impacted users

// safetynet caught, transaction denied auth required
let start = todatetime("2023-04-11 16:00:00"); let end = todatetime("2023-04-11 23:00:00"); let piids = PSD2PayerAuthCompleteChallengeEvents(start, end, false) | where payerAuthError != "" | distinct piid; let ARExcludePiids = PSD2PayerAuthAuthenticateEvents(start, end, false) | where payerAuthError != "" | distinct piid; TransactionDetails | where Timestamp between (start .. end)   and StatusDetailsCode == "AuthenticationRequired"    and (PaymentInstrumentId in (piids) or PaymentInstrumentId in (ARExcludePiids)) | summarize arg_max(todatetime(Timestamp), *) by PaymentInstrumentId | summarize piCount=count()


Safetynet transaction failure impact by currency in USD

```
// Impact in USD
// safetynet caught, transaction denied auth required, currency conversion as of 04/15/2023
let start = todatetime("2023-04-11 16:00:00"); let end = todatetime("2023-04-11 23:00:00"); let piids = PSD2PayerAuthCompleteChallengeEvents(start, end, false) | where payerAuthError != "" | distinct piid; let ARExcludePiids = PSD2PayerAuthAuthenticateEvents(start, end, false) | where payerAuthError != "" | distinct piid; TransactionDetails | where Timestamp between (start .. end)   and StatusDetailsCode == "AuthenticationRequired"    and (PaymentInstrumentId in (piids) or PaymentInstrumentId in (ARExcludePiids)) | summarize arg_max(todatetime(Timestamp), *) by PaymentInstrumentId | summarize piCount=count(), amount = sum(Amount) by currency = Currency | summarize EUR = sumif(todouble(amount), currency == "EUR"), GBP = sumif(todouble(amount), currency == "GBP"), BRL = sumif(todouble(amount), currency == "BRL"), PLN = sumif(todouble(amount), currency == "PLN"), USD = sumif(todouble(amount), currency == "USD"), HUF = sumif(todouble(amount), currency == "HUF"), CAD = sumif(todouble(amount), currency == "CAD"), SEK = sumif(todouble(amount), currency == "SEK"), CZK = sumif(todouble(amount), currency == "CZK"), ARS = sumif(todouble(amount), currency == "ARS"), INR = sumif(todouble(amount), currency == "INR"), DKK = sumif(todouble(amount), currency == "DKK"), TRY = sumif(todouble(amount), currency == "TRY"), NOK = sumif(todouble(amount), currency == "NOK"), MXN = sumif(todouble(amount), currency == "MXN"), THB = sumif(todouble(amount), currency == "THB"), SAR = sumif(todouble(amount), currency == "SAR"), CHF = sumif(todouble(amount), currency == "CHF"), ZAR = sumif(todouble(amount), currency == "ZAR"), RON = sumif(todouble(amount), currency == "RON"), UAH = sumif(todouble(amount), currency == "UAH"), BHD = sumif(todouble(amount), currency == "BHD"), JPY = sumif(todouble(amount), currency == "JPY"), BGN = sumif(todouble(amount), currency == "BGN"), COP = sumif(todouble(amount), currency == "COP"), ALL = sumif(todouble(amount), currency == "ALL"), TTD = sumif(todouble(amount), currency == "TTD"), ISK = sumif(todouble(amount), currency == "ISK"), AUD = sumif(todouble(amount), currency == "AUD") | extend TotalInUSD = round((GBP * 1.24) + (EUR * 1.1) + (DKK * 0.15) + (BRL * 0.2) + (PLN * 0.24) + USD + (HUF * 0.0029) + (CAD * 0.75) +      (SEK * 0.097) + (CZK * 0.047) + (ARS * 0.0046) + (INR * 0.012) + (TRY * 0.052) + (NOK * 0.096) + (MXN * 0.055) + (THB * 0.029) + (SAR * 0.27) +      (CHF * 1.12) + (ZAR * 0.055) + (RON * 0.22) + (UAH * 0.027) + (BHD * 2.65) + (JPY * 0.0075) + (BGN * 0.56) + (COP * 0.00023) + (ALL * 0.0097) +      (TTD * 0.15) + (AUD * 0.67) + (ISK * 0.0073), 2)
```
