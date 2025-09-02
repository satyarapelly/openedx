# Add PI

Sample Example :-  

[Incident 513257326](https://icmcdn.akamaized.net/imp/v3/incidents/details/513257326/home) : [M365][NOAM] (IUnable to add new card)
               
[Incident 511051682](https://icmcdn.akamaized.net/imp/v3/incidents/details/511051682/home) : M365 || Rhipe || APAC || 2406020060000056001 || Unable to replace payment method

[Incident 502962302](https://icmcdn.akamaized.net/imp/v3/incidents/details/502962302/home) : [M365][APAC]|"Check your address" error occurs when cx is trying to checking out
 
**Below is the list of query to check for Add pi  CRI scenarios**

Query :-

• <u>Usage-----If issue from PX end and have either Account id or Tenant Id in icm</u>

    • RequestTelemetry
     | where TIMESTAMP > ago(30d)
     | where (name == "Microsoft.Commerce.Tracing.Sll.PXServiceIncomingOperation" or name == "Microsoft.Commerce.Tracing.Sll.PXServiceOutgoingOperation")
     | extend res = parse_json(data_ResponseDetails)
     | extend error = res['ErrorCode']
     | where data_baseData_targetUri contains "a754339f-c678-4469-9cad-3cb7fdb862e2" //(Tenant Id Or Account Id)
     | project res['ErrorCode'],TIMESTAMP,cV, name, data_baseData_callerName,  data_baseData_operationName,data_correlationId , data_baseData_protocolStatusCode,data_baseData_dependencyName,data_baseData_dependencyOperationName, data_baseData_latencyMs, data_baseData_targetUri, data_RequestDetails,data_ResponseDetails , data_ResponseHeader,data_RequestHeader, data_AccountId, data_Partner, data_Country,data_faultDetail
 
<u>• Usage----if we require details of more than one month old with Tenant id in ICM</u>

    • Events
     //| where Timestamp > ago(10d)
     | where around(Timestamp,datetime(2024-05-17T15:21:55.170344Z),1d)
     //| where * contains "e47bcc42-28bb-42fa-9151-ec615be5940c"   //(Tenant Id Or Account Id)
     //| where CV startswith "448B+DvJPK4MskX96sIUYQ"
 
<u>•  Usage----To check the transaction status with PaymentId</u>

    • TransactionDetails
     | where PaymentId == "Z10093CQ0NNR57e91441-5437-4c1a-957d-f227081c19b8"
    
<u>•  Usage---check details of add pi failure using Tenant Id or User Id</u>

        PIMSAddCreditCardEventsV2
        | extend jsonObject = parse_json(RequestDetails)
        | extend tenantid = tostring(jsonObject.callerInfomation.tenantId)
        | extend EvaluationDetails = parse_json(EvaluationDetails)
        | extend modelFraudRulesDecision = EvaluationDetails.modelFraudRulesDecision
        | extend modelFraudRulesResultsDetails=EvaluationDetails.modelFraudRulesResults
        | extend modelFraudRulesResults = modelFraudRulesResultsDetails
        //| where AccountId == "0127ff04-6873-4208-8c59-970fc4ed3c17"
        | where tenantid == "bef1fff7-d06b-424b-bb58-e9d439223fd9"
        | project-reorder Timestamp,PaymentInstrumentId,AccountId, IpAddress,ErrorCode,ResponseType,TokenAuthResponse,RiskResponse,modelFraudRulesDecision,EvaluationDetails,CV
        
    
**Note:**

    • Transfer to "Commerce Risk Customer Engagement/Commerce Risk Customer Engagement."-->for Risk Decline
    • Before transfer change to icm to SEV 4 

[Incident 528151729](https://icmcdn.akamaized.net/imp/v3/incidents/details/528151729/home) : [M365][EMEA] Unable to add DD--->Reference for long bank name
