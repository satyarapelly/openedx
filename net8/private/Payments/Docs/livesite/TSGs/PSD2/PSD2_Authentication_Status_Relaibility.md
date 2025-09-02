# PSD2 Authentication Status Reliability

**What is this query?**

This query tracks the reliability of the PaymentSessionsController-GET-AuthenticationStatus api.
Partners call this api to find out whether a PSD2 session is verified or not. 

**What is the impact?**

Calling partners might not know if a session is verified or not

**How do I get user impact numbers?**

Use below query to find the impact numbers:
```
GetPXFailureDetails(ago(30m), now(), "paymentsessionscontroller-get-authenticationstatus", "*", "*", "*", "*",  "*", "systemerror")
```
To get Unique users experiencing the error:
```
GetPXFailureDetails(ago(30m), now(), "paymentsessionscontroller-get-authenticationstatus", "*", "*", "*", "*",  "*", "systemerror") 
| summarize dcount(AccountId) by OperationName, PartnerName
```

2) Using the above details, analyze the impact and engage the correct team as mentioned at  [livesite-sop.md - Repos (visualstudio.com)](https://microsoft.visualstudio.com/Universal%20Store/_git/SC.CSPayments.PX?path=/private/Payments/Docs/operations/livesite-sop.md&_a=preview) 
   
    a. If the issue is within PX service, see if the issue is limited to specific region. If so, we can either engage SRE oncall to take the impacted region offline, or get 'Traffic Manager Contributor' access to our PROD subscription using JIT and take the region offline. Our PROD Azure Traffic Manager is at [Azure Portal](https://login.microsoftonline.com/mspmecloud.onmicrosoft.com/oauth2/v2.0/authorize?redirect_uri=https%3A%2F%2Fportal.azure.com%2Fsignin%2Findex%2F&response_type=code%20id_token&scope=https%3A%2F%2Fmanagement.core.windows.net%2F%2Fuser_impersonation%20openid%20email%20profile&state=OpenIdConnect.AuthenticationProperties%3Dfh-4kcDQX3MloWkQRnAi5sZJheRCTW-CSHmwgF0y3AWKeg7PeQogQIAnAxe9dCSVQCQu7d6CBxeDyx0jSBNHcCaLL95zJHErPKts5TF-oJYh9NcmZBY_AmdF_won4WRP8sBrHHyBVy1E4hyNq36aK1OOVp4nmxN5GTQF_-PqGOFXtupbgoPxrpcdsyQEWdpmnd-qQA8RK54MMo8_ZvcMrlw76S_Va3v34SF78UiGEpZp0TRER88nG_Ziudfcujd4ApgAuV_0IFLLftXin3lEYmClDqFfrqCvnoKolbQjvPzHo3YSNKIh5qr1Ho01vm0N9-gCxARYrveEdFnzUbn42bQTVTpOJyOo2Vnd2IcIwLszNbSX8ZX0rAZrHg8IrMgPd8tgBRHW5Qlj6CFQTkEFQjuYZvvNaNITw4uYzc8dMOwjRoNuuM2YHMKKp8k4ppzeF44BsSMAvt28YBRbmtjlNsNzkJoLy6hUqfr0lsPrf08Bt2lKAquyCFRxrDO-fHrASiH9pUGqZK9BE3cLFYzS0g&response_mode=form_post&nonce=638653215680923268.NjYwMGFlOTYtNGJmMy00N2IyLWFkMjYtZDNjZTE0MTY3MTJjODExN2YzYmMtODgyMS00Nzg0LWI0MTMtNjJiZjA4YWI4MmNi&client_id=c44b4083-3bb0-49c1-b47d-974e53cbdf3c&site_id=501430&client-request-id=05302819-62c5-49ff-93f7-1c15267e98ba&x-client-SKU=ID_NET472&x-client-ver=7.5.0.0). You need to use PME account and Yubi Key to login.
    b. If the issue is caused by any recent flight changes, see if we need to turn off those flights. Refer to [feature-flighting.md - Repos (visualstudio.com)](https://microsoft.visualstudio.com/Universal%20Store/_git/SC.CSPayments.PX?path=/private/Payments/Docs/operations/feature-flighting.md&_a=preview) for flight config management
    c. If the issue is within PX service and not specific to a region and not specific to any flight config, engage Eng Manager Oncall and see if any recent deployment needs to be rolled back.

Dev Owners: Selvanr, Kolbychien,anushrimarar,rtalluri
