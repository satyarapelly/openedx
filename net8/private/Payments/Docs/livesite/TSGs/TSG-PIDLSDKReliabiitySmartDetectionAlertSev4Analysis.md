# TSG:PIDLSDKReliabiitySmartDetectionAlertSev4 Analysis

Reference Icm:[Incident 388308259](https://icmcdn.akamaized.net/imp/v3/incidents/details/388308259/home) : [Incident Report][High] PIDLSDKReliabiitySmartDetectionAlertSev4 - 2023-05-09 12:00:00 (UTC) - <-- EMPTY -->|address.update.hapiv1soldtoorganization|commercialstores

Query:
```
let scenarioName = "*";
let partnerName = "commercialstores"; //partner name
GetPidlSdkFailureDetails(datetime(2023-05-09),now(), scenarioName, partnerName, "*", "*")
| where PidlSdkOperationType == "Update" 
| where PidlResourceName == "address.update.hapiv1soldtoorganization"
//| where ErrorCode == "resourceInstanceDownloadFailed_500_"
| summarize count() by ErrorCode, ErrorType//, bin(IngestTime, 1h)
| render timechart

    • PidlSdkOperationType -->Information given in Title (mostly add or update)
    • PidlResourceName-->Information given in details of icm as Operation name
    • partnerName->Information given in details of icm as Partner name
    • Use "*" if value of the parameter is unknown
```    


Find the ErrorCode with highest count and check the reason for Failure and connect with corresponding team

Note:
If there is spike only for small period of time  and down after some time ICM is good to be resolved
