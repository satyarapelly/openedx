# Add PI - CreditCard

## Target audience
Developers and PMs contributing code or docs to PX

## Overview
This doc contains details about the Add Credit Card Scenario, that PIDL/PX supports.
Partners use this scenario in purchase and manage account flows.
In the purchase or signup flow, when user wants to add a new credit card, this flow will be exercised.
Similarly, when user wants to add a new credit card while managing payment methods in partner portals, this flow will be exercised.

List of test cases to cover this scenario can be found [here](https://microsoft.visualstudio.com/OSGS/_testPlans/define?planId=36222018&suiteId=36534450)


## How various partners integrate our components for this scenario

### 1) Azure Signup Portal  

#### Steps to see the flow from partner portal:
```
1) Have a MSA account ready to login. You can create a new MSA account at https://outlook.live.com/owa/?nlp=1&signup=1
2) Open browser and navigate to 'signup.azure.com'
3) Login with msa account and follow the steps to complete the signup process
```
Demo video can be found [here](https://bing.com)

#### Integration notes
```
- What pidlsdk view is being used?
    jquery view

- What parameters are being used to initialize pidlsdk? 

    pidl.docDisplayControl(userContextIn, returnOptions, eventHandler, displayOptions, telemetryOptions, callbacksIn, mockFunctions)
        
        userContextIn: {additionalHeaders: {x-ms-flight: 'showAVSSuggestions'}, authToken: "Bearer ...", userId: "me"}
        returnOptions: {callbacks: {failure: ƒ (t), gohome: ƒ (), success: ƒ (t)}}
        eventHandler: ƒ (t,n){return e.pidlEventListener(t,n)}
        displayOptions: {containerId: "pidlPiList"}
        telemetryOptions: {contextData:{partnerName: "Azure",scenarioId: "commerce.signup.azuresignup",sessionId: "e79eef38-a15f-4945-bb36-866e02cfc732",}}


- What different pidlsdk APIs are being used in the flow? What input parameters are being used to call those APIs?
    
    addResource(pidlDocInfo, prefillData, options)

        pidlDocInfo: {resourceType: "paymentInstrument", parameters: 
            {billableAccountId: "n9r3IMQAAAAAAAAA",classicProduct: "azureClassic",country: "US",family: "credit_card",language: "en",partner: "Azure",type: "visa,mc,amex,discover,jcb"}}
        prefillData: {address_line1: '1 Microsoft Way', city: 'Redmond', region: 'wa', postal_code: '98052-8300'}
        options: {riskData: {greenId: 'e79eef38-a15f-4945-bb36-866e02cfc732'}}


- Is submit block rendered by PIDLSDK?
    Yes

- Are there any other customizations/changes observed in the PIDL rendered DOM, like hiding an element or changing its attributes,..etc?
    None
```

#### Steps to see the flow from test portal:
- Please refer to the ADO Test case [here](https://microsoft.visualstudio.com/OSGS/_workitems/edit/36360725)


### 2) Office Signup Portal

#### Steps to see the flow from partner portal:
```
1) Open browser and navigate to 'https://signup.microsoft.com/create-account/signup?products=cfq7ttc0k62t:0001'
2) Follow the steps to create a new account and complete the signup process
```
Demo video can be found [here](https://bing.com)

#### Integration notes
```
- What pidlsdk view is being used?
    React OfficFabric view

- What parameters are being used to initialize pidlsdk? 

    pidl.docDisplayControl(userContextIn, returnOptions, eventHandler, displayOptions, telemetryOptions, callbacksIn, mockFunctions)

- What different pidlsdk APIs are being used in the flow? What input parameters are being used to call those APIs?
    addResource(pidlDocInfo, prefillData, options)

- Is submit block rendered by PIDLSDK?
    Yes

- Are there any other customizations/changes observed in the PIDL rendered DOM, like hiding an element or changing its attributes,..etc?
    None
```

#### Steps to see the flow from test portal:
- Please refer to the ADO Test case [here](https://microsoft.visualstudio.com/OSGS/_workitems/edit/36360725)

### 3) Microsoft Store Portal

#### Steps to see the flow from partner portal:
```
1) Have a MSA account ready to login. You can create a new MSA account at https://outlook.live.com/owa/?nlp=1&signup=1
2) Open browser and navigate to 'https://www.microsoft.com/en-us/store/b/sale?rtc=1'
3) Try to purchase a product and add a credit card in the flow
```
Demo video can be found [here](https://bing.com)

#### Integration notes
```
- What pidlsdk view is being used?
    React Classic view

- What parameters are being used to initialize pidlsdk? 

    pidl.docDisplayControl(userContextIn, returnOptions, eventHandler, displayOptions, telemetryOptions, callbacksIn, mockFunctions)


- What different pidlsdk APIs are being used in the flow? What input parameters are being used to call those APIs?
    addResource(pidlDocInfo, prefillData, options)

- Is submit block rendered by PIDLSDK?
    Yes

- Are there any other customizations/changes observed in the PIDL rendered DOM, like hiding an element or changing its attributes,..etc?
    None
```

#### Steps to see the flow from test portal:
- Please refer to the ADO Test case [here](https://microsoft.visualstudio.com/OSGS/_workitems/edit/36360725)


## Scenarios
### Add Credit Card Flows in India Market with 3DS
In 'India' market, during Add credit card, we show bank's challenge screen. Here are different ways that we show the challenge. Partners pick one of these flows when they integrate with payments

#### **Inline (redirection in same browser tab)**
1. Present "Add a credit or debit card" entry form via /paymentMethodDescriptions
1. When "Next" button is clicked, present "Check your info" page with "Save" button for user to verify information entered on previous form.
1. When "Save" button is clicked, present bank challenge page inline (in same browser tab)
1. Redirect to Ru (Success) or Rx (Error) url (in same browser tab) depending on challenge outcome.
1. The Success url will have some query parameters appended: picvRequired, pendingOn, type, family, id (piid)
  
#### **Non-Inline (Redirection in new browser tab)**
1. Present "Add a credit or debit card" entry form via /paymentMethodDescriptions
1. When "Next" button is clicked, present "Verify Your Card" page with "Open Bank's website" button.
1. When "Open bank's website" button is clicked:
    1. the "Did you successfully verify your card?" page will be presented in the current browser tab.
    1. the bank's website will be open in a new tab.
1. New tab is redirected to Ru (Success) or Rx (Error) url depending on challenge outcome
1. Original browser tab with "Did you successfully verify your card?" page will be polling for a success or failure from the bank challenge.
1. The user can also click the "Yes I'm done with bank verification" button to check for a success or failure manually.
1. Once a completed challenge is detected, the pidlsdk will give a success with the pi data.

  
#### **iFrame**
1. Will be activated over Inline or Non-Inline depending on a boolean we receive from PayMod (`is_full_page_redirect`)
1. Present "Add a credit or debit card" entry form via /paymentMethodDescriptions
1. When "Save" button is clicked, present bank challenge page in an iframe
1. When bank challenge is complete, present (in original browser tab outside of iframe) "Did you successfully verify your card?" page will be polling for a success or failure from the bank challenge.
1. The user can also click the "Yes I'm done with bank verification" button to check for a success or failure manually.
1. Once a completed challenge is detected, the pidlsdk will give a success with the pi data.  
  
#### Partner's flow for IN market
|Partner Name|Purchase Flow|
|---|---|
|amcweb|Inline|
|cart|Inline (but has a screen after Initial Credit Card Entry Form)|
|webblends|Non-Inline|
|webblends_inline|Inline|

---
For questions/clarifications, email [author/s of this doc and PX support](mailto:ravikm@microsoft.com?cc=PXSupport@microsoft.com&subject=Docs%20-%20scenarios/PI-add-credit-card.md).

---