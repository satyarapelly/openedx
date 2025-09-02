# Getting started with the pidl-react-enrichment project

## Target audience
Developers and PMs contributing code to the manage-payments project.

## Overview
This document gives details about the project, source code and dev setup, build, test and release processes.

## Project
The *pidl-react-enrichment* components are used by the the [AMC website](https://account.microsoft.com/) in their [Services and Subscriptions](https://account.microsoft.com/services) page.

The two main functions inside the *Services and Subscriptions* page are PayNow and Change PI.

### PayNow
PayNow deals with subscriptions that are *in-dunning*. These are subscriptions that have expired recently due to either the customer didn't opt for auto-renewal or the payment instrument in their account is expired, has no enough funds or is no longer valid otherwise.
The customer is able to see the payment instrument associated with the subscription and have an option to *Pay now* which means either selecting another PI existing in their account, updating the PI associated to the subscription or adding a PI.

### Change PI
For those subcriptions that are still active, we display the PI that is associated with each subscription and offer the customer an option to change the PI, either by selecting a different one already in their account, updating it or adding a new PI.

### Components
The pidl-react-enrichment project consist of 4 components:
* AddPaymentInstrument
* EditPaymentInstrument
* SelectPaymentInstrument
* ValidateAddress

These components are wrappers around the PIDL SDK components: *AddResource*, *UpdateResource*, *SelectResourceType*, *SelectResource* and *ValidateAddress*.

#### AddPaymentInstrument
AddPaymentInstrument wraps two PIDL SDK components: *SelectResourceType* and *AddResource* into a single UI.
It provides built-in styling for these components as well as the *Edit Address* functionality that hides the address fields when the component is loaded and shows them if the user click the "Edit address" button.

#### EditPaymentsInstrument
EditPaymentsInstrument wraps the PIDL SDK component *UpdateResource*.
It provides styling and also the *Edit Address* functionality.

#### SelectPaymentInstrument
SelectPaymentInstrument wraps the PIDL SDK component *SelectResource*.
It provides styling.

#### ValidateAddress
ValidateAddress wraps the PIDL SDK component *ValidateAddress*.
It provides styling.

## Source code structure
All the four components are located in the project folder:

* **pay.pidl.sdk\components\react\pidl-react-enrichment**:  

Two additional folders are relevant as depencies although you may need to update them less frequently when developing for pidl-react-enrichment:

* **pay.pidl.sdk\components\react\pidl-react**:  
Contains the core components for the Add, Edit, Select PI and Validate Address that pidl-react-enrichment wraps.

* **pay.pidl.sdk\components\react\pidl-fluent-ui**:  
This is an element factory that pidl-react accepts to render basic UI elements like textboxes, buttons, etc. using the standard look-and-feel defined by [Microsoft Fluent UI library](https://developer.microsoft.com/en-us/fluentui/).

## Dev box setup
Please refer to the [PIDL SDK dev setup](./pidlsdk-setup.md) process.   
If you have setup already your PIDL SDK source code clone, you don't have to do it again. You can use the same clone you created for PIDL SDK.

## Build
This component is build as part of the PIDL SDK's [Integration](https://microsoft.visualstudio.com/Universal%20Store/_build?definitionId=20658) pipeline and published as the **pidl-react-enrichment** package in the [Universal.Store.NPM](https://microsoft.visualstudio.com/Universal%20Store/_artifacts/feed/Universal.Store.NPM/Npm/@cspayments%2Fpidl-react-enrichment/overview) repository.

## Testing
These components are tested only on the [PIDL React test portal](https://pidlsdktestportal.azurewebsites.net/PidlReact.html).  
Test cases are available in [Azure Test Suite](https://microsoft.visualstudio.com/OSGS/_testPlans/define?planId=33790634&suiteId=33790682).

Note you can put a subscription in-dunning for testing purposes by [changing the expiration date](https://microsoft.sharepoint.com/teams/PaymentsNorthStar/_layouts/15/Doc.aspx?sourcedoc={6d36bf28-e75e-43b9-be7e-584e61495493}&action=edit&wd=target%28AMC%20React%20Status.one%7C4d1701fe-3b1a-4b7a-88b2-78d0106d4726%2FSteps%20to%20put%20user%20in%20dunning%7C7ce57d72-22fb-43e0-a65a-931ddef104e3%2F%29&wdorigin=703).

## Release
Once a new version has been published to the Universal.Store.NPM repository, the AMC team can be notified to upgrade to the latest version.