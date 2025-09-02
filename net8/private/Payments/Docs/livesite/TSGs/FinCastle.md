# FinCastle

    • PX Kusto queries for volume / success rate / errors:


Web Partner:

        ○ Xbox Support Services\CXT IcM Green Crew (Can't engage directly, need to go through OSOC)


PX Kusto queries for volume / success rate / errors:

```
////
//// OVERALL QUERY VOLUME, BY CHANNEL/REFERRERID
////
RequestTelemetry
| where TIMESTAMP > ago(7d)
| where data_baseData_operationName == "PaymentMethodDescriptionsController-GET"
| where data_IsTest != "True"
| where data_Partner == "xboxweb"
| where data_PidlOperation == "apply"
| where SourceNamespace == "paymentexperiencelogsprod"
| extend channel = tolower(parse_url(data_baseData_targetUri).['Query Parameters']['channel'])
| extend referrerId = tolower(parse_url(data_baseData_targetUri).['Query Parameters']['referrerId'])
| summarize count() by channel, referrerId
| order by count_ desc


RequestTelemetry
| where TIMESTAMP > ago(7d)
| where data_baseData_operationName == "PaymentInstrumentsExController-POST"
| where data_IsTest != "True"
| where data_Partner == "xboxweb"
| where SourceNamespace == "paymentexperiencelogsprod"
| extend cardProduct = tolower(parse_json(data_RequestDetails).['cardProduct'])
| extend channel = tolower(parse_json(data_RequestDetails).['channel'])
| extend referrerId = tolower(parse_json(data_RequestDetails).['referrerId'])
| where cardProduct == "xboxcreditcard"
| summarize count() by channel, referrerId
| order by count_ desc 


////
//// SUCCESS RATE QUERIES
////
RequestTelemetry
| where TIMESTAMP > ago(7d)
| where data_baseData_operationName == "PaymentMethodDescriptionsController-GET"
| where data_IsTest != "True"
| where data_Partner == "xboxweb"
| where data_PidlOperation == "apply"
| where SourceNamespace == "paymentexperiencelogsprod"
| extend EventTime = bin(TIMESTAMP, 1h)
| summarize SuccessfulCount=todouble(countif(data_baseData_protocolStatusCode == 200)), TotalCount=todouble(count()) by EventTime
| project EventTime, SuccessRate=(SuccessfulCount / TotalCount) * 100
| render timechart


RequestTelemetry
| where TIMESTAMP > ago(7d)
| where data_baseData_operationName == "PaymentInstrumentsExController-POST"
| where data_IsTest != "True"
| where data_Partner == "xboxweb"
| where SourceNamespace == "paymentexperiencelogsprod"
| extend cardProduct = tolower(parse_json(data_RequestDetails).['cardProduct'])
| where cardProduct == "xboxcreditcard"
| extend EventTime = bin(TIMESTAMP, 1h)
| summarize SuccessfulCount=todouble(countif(data_baseData_protocolStatusCode == 200)), TotalCount=todouble(count()) by EventTime
| project EventTime, SuccessRate=(SuccessfulCount / TotalCount) * 100
| render timechart


////
//// ERROR QUERIES
////
RequestTelemetry
| where TIMESTAMP > ago(7d)
| where data_baseData_operationName == "PaymentMethodDescriptionsController-GET"
| where data_IsTest != "True"
| where data_Partner == "xboxweb"
| where data_PidlOperation == "apply"
| where SourceNamespace == "paymentexperiencelogsprod"
| where data_baseData_protocolStatusCode != 200
| project TIMESTAMP, cV, data_baseData_protocolStatusCode, data_ErrorCode, data_ErrorMessage, data_RequestHeader, data_RequestData, data_RequestDetails, data_ResponseHeader, data_ResponseDetails


RequestTelemetry
| where TIMESTAMP > ago(7d)
| where data_baseData_operationName == "PaymentInstrumentsExController-POST"
| where data_IsTest != "True"
| where data_Partner == "xboxweb"
| where SourceNamespace == "paymentexperiencelogsprod"
| extend cardProduct = tolower(parse_json(data_RequestDetails).['cardProduct'])
| where cardProduct == "xboxcreditcard"
| where data_baseData_protocolStatusCode != 200
| project TIMESTAMP, cV, data_baseData_protocolStatusCode, data_ErrorCode, data_ErrorMessage, data_RequestHeader, data_RequestData, data_RequestDetails, data_ResponseHeader, data_ResponseDetails


//
// Console QR code 
//
let endTime = ago(1m);
let startTime = ago(72h); // 9am 6/12 
let targetPartner = "storify";
let binSize = 1h;
RequestTelemetry
| where TIMESTAMP between (startTime .. endTime) // change time
| where data_baseData_operationName contains "PaymentMethodDescriptionsController-GET"
| extend scenario = tolower(parse_url(data_baseData_targetUri).['Query Parameters']['scenario'])
| extend partner = tolower(parse_url(data_baseData_targetUri).['Query Parameters']["partner"])
| extend country = tolower(parse_url(data_baseData_targetUri).['Query Parameters']["country"])
| where partner in ("storify", "xboxcardapp")
| where country == "us"
| where scenario == "xboxcobrandedcard"
| project data_baseData_targetUri, TIMESTAMP
| summarize addTrial = dcount(data_baseData_targetUri)  by bin(todatetime(TIMESTAMP), binSize)
| union 
(
RequestTelemetry
| where TIMESTAMP between (startTime .. endTime) // change time
| where data_baseData_operationName contains "PaymentInstrumentsExController-GET-GetModernPI"
| extend scenario = tolower(parse_url(data_baseData_targetUri).['Query Parameters']['scenario'])
| extend partner = tolower(parse_url(data_baseData_targetUri).['Query Parameters']["partner"])
| extend country = tolower(parse_url(data_baseData_targetUri).['Query Parameters']["country"])
| where partner in ("storify", "xboxcardapp")
| where country == "us"
| where scenario == "xboxcobrandedcard"
| extend statusResult = tolower(parse_json(data_ResponseDetails).["status"])
| project data_baseData_targetUri, TIMESTAMP, statusResult
| summarize activePis = dcountif(data_baseData_targetUri, statusResult=="active"), 
pendingPis = dcountif(data_baseData_targetUri, statusResult=="pending"),
declinedPis = dcountif(data_baseData_targetUri, statusResult=="declined"),
unknownPis = dcountif(data_baseData_targetUri, statusResult=="unknown"),
cancelledPis = dcountif(data_baseData_targetUri, statusResult=="cancelled")  by bin(todatetime(TIMESTAMP), binSize)
)
| summarize applyAttempts = sum(addTrial), 
    successfulApply = sum(activePis), 
    declined = sum(declinedPis), 
    pending = sum(pendingPis),
    unknown = sum(unknownPis),
    cancelled = sum(cancelledPis),
    userAbandonedFlow = sum(addTrial)-sum(activePis)-sum(declinedPis)-sum(unknownPis)-sum(pendingPis)-sum(cancelledPis),
    conversionRatePercent = round(todouble(((sum(activePis)+sum(pendingPis))/todouble(sum(addTrial)))),2),
    abandonedPercent = round ( todouble ((sum(addTrial)-sum(activePis)-sum(declinedPis)-sum(pendingPis)-sum(unknownPis)) / todouble(sum(addTrial))), 2)
    by bin(todatetime(TIMESTAMP), binSize)
| render timechart; 


//
// Apply on console initialize requires two calls, one GET, then an immediate POST right after
//
let endTime = ago(1m);
let startTime = ago(3d);
let binSize = 1h;
RequestTelemetry
| where TIMESTAMP between (startTime .. endTime)
| where data_baseData_operationName contains "PaymentMethodDescriptionsController-GET"
| extend scenario = tolower(parse_url(data_baseData_targetUri).['Query Parameters']['scenario'])
| extend partner = tolower(parse_url(data_baseData_targetUri).['Query Parameters']["partner"])
| extend country = tolower(parse_url(data_baseData_targetUri).['Query Parameters']["country"])
| extend family = tolower(parse_url(data_baseData_targetUri).['Query Parameters']["family"])
| extend type = tolower(parse_url(data_baseData_targetUri).['Query Parameters']["type"])
| where partner in ("storify", "xboxcardapp")
| where country == "us"
| where scenario == "xboxcobrandedcard"
| summarize getCount = count() by data_baseData_protocolStatusCode, bin(TIMESTAMP, binSize)
| union(
    RequestTelemetry
    | where TIMESTAMP between (startTime .. endTime)
    | where data_baseData_operationName contains "PaymentInstrumentsExController-POST"
    | extend partner = tolower(parse_url(data_baseData_targetUri).['Query Parameters']["partner"])
    | extend country = tolower(parse_url(data_baseData_targetUri).['Query Parameters']["country"])
    | extend operation = tolower(parse_url(data_baseData_targetUri).['Query Parameters']["operation"])
    | where partner in ("storify", "xboxcardapp")
    | where country == "us"
    | where operation == "apply"
    | summarize postCount = count() by data_baseData_protocolStatusCode, bin(TIMESTAMP, binSize)
)
| render timechart 

// Console eligibility errors
let endTime = ago(1m);
let startTime = ago(7d);
let binSize = 1h;
RequestTelemetry
| where TIMESTAMP between (startTime .. endTime)
| where data_baseData_operationName contains "PaymentMethodDescriptionsController-GET"
| extend scenario = tolower(parse_url(data_baseData_targetUri).['Query Parameters']['scenario'])
| extend partner = tolower(parse_url(data_baseData_targetUri).['Query Parameters']["partner"])
| extend country = tolower(parse_url(data_baseData_targetUri).['Query Parameters']["country"])
| where partner in ("storify", "xboxcardapp")
| where country == "us"
| where scenario == "xboxcobrandedcard"
| summarize applySuccess = dcountif(data_baseData_targetUri, data_ResponseDetails contains("xboxCardApplyCardAlreadyIssuedHeading")), 
    applyError = dcountif(data_baseData_targetUri, data_ResponseDetails contains("xboxCardApplyInternalErrorHeading")), 
    applyPending = dcountif(data_baseData_targetUri, data_ResponseDetails contains("xboxCoBrandedCardApplyPendingHeading")), 
    applyIneligible = dcountif(data_baseData_targetUri, data_ResponseDetails contains("xboxCardApplyNotEligibleHeading"))
    by bin(TIMESTAMP, binSize)
| render timechart
```