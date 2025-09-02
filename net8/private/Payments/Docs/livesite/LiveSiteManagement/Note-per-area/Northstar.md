# Northstar

## Overview

Manage-payments (aka NorthStar, aka Payment Options) is an application that encapsulates end-to-end experiences for payment management: listing PI, adding, editing, replacing and removing PIs.

Differently to the PIDL SDK, which is a collection of discrete components to manage PIs, manage-payment is more akin to an app to manage PIs end-to-end, similar to a partner storefront.

It internally uses the PIDL SDK for the discrete operations for add, update, replace, etc. but it also has UI of its own and makes service calls.

We release manage-payments as a React component that the AMC partner consumes in their payment options page.

## Portals

AMC hosts the manage-payment component in Prod: http://account.microsoft.com/billing/payments/

We host new builds first in our internal Azure site for testing: https://mspayment-ppe-centralus.azurewebsites.net/

## Dashboard

The engineering dashboard shows a general view of the status of the service calls Manage-Payments do, both through PIDL SDK and on its own.

https://xpert.microsoft.com/osg/views/ManagePayments%20Dashboard/7d04950c-dff7-4411-b6d5-f2fa53adc5a4

The business dashboard shows the completion rates (aka funnels) and other business related information but this dashboard is not normally used for live site troubleshooting because there is a delay of several hours for data to show up.

https://msit.powerbi.com/groups/me/reports/f70c904f-4828-40f1-b74a-afed635a0202

## Querying

## Manage-payments uses 1DS for logging and the data is available in Kusto and Xpert.

For Kusto, this is the connection information:

Cluster: https://pst.kusto.windows.net

Database: PROD

For Xpert, this a [sample query](https://xpert.microsoft.com/osg/Search?display=Grid&amp;searchQuery=%40name%20%3D%3D%20%22Microsoft.Commerce.Payments.ManagePayments.PageEvent%22%0Aselect%20%40eventName%2C%20%40%22eventProperties.healthy%22%2C%20%40country%2C%20%40market%2C%20%40cV%2C%20%40sessionId%0Awhere%20%40eventName%20%3D%3D%20%22HealthOnInitialize%22%20AND%20%40%22eventProperties.healthy%22%20%3D%3D%20false&amp;source=Environment%3DPROD%3B):

## Health reporting

The Manage-payments components makes 2 main service calls when loaded, it calls ListPI and GetTransactions to get the necessary data to render the homepage, these calls occur in parallel and if any of them fail, we will report a bad health via two events Health and HealthOnInitialize.

HealthOnInitialize is what AMC monitors and they may report low reliability based on it. This is the first event to check to get a sense of the general load failures of the component

Here is how to see a graph of the daily health failures in the last 30 days:`// HealthOnInitialize failure graph`

```
ManagePaymentsTelemetryEvents
```

`| where EventTime >= ago(30d)`

`  and EventName == "HealthOnInitialize"`

`  and EventProperties.healthy == false`

`| project EventTime`

`| summarize Count = count() by bin(EventTime, 1d)`

`| render timechart`

And these show a graph for the GetTransactions and ListPI failures:

```
// ListPI failures
ManagePaymentsTelemetryEvents
```

`| where EventTime >= ago(30d)`

`  and EventName == "ListPI_fetchFailed"`

`| project EventTime, CV, AMC_CV, EventProperties.error.code, EventProperties.error.message, EventProperties`

`| summarize Count = count() by bin(EventTime, 1d)`

`| render timechart`

```
// GetTransactions failures
ManagePaymentsTelemetryEvents
```

`| where EventTime >= ago(30d)`

`  and EventName == "GetTransactions_fetchFailed"`

`| project EventTime, CV, AMC_CV, EventProperties.error.code, EventProperties.error.message, EventProperties`

`| summarize Count = count() by bin(EventTime, 1d)`

`| render timechart`

Remove the `summarize` and `render` lines for a grid view of the errors.

## CV Search

The telemetry has 2 CVs, one that is tracked by the manage-payment component and another that is passed as a parameter by AMC.

The one that manage-payments tracks is unique per page load, the one that AMC passes is unique per session. This means that you can see the AMC CV stay constant for a given user on a given session even if they reload the page on the browser, while the manage-payments CV will change with each load, this allows you to track page reloads that a user does to retry after load errors.

This query searches for a specific AMC CV. Note that if the user reloaded the page you will see the CV column (not  AMC\_CV) changing after each page reload.

```
// Search by AMC CV
ManagePaymentsTelemetryEvents
```

`| where EventTime >= ago(30d) and EventTime <= ago(1d)`

`| project EventTime, CV, AMC_CV, EventName, EventProperties`

`| where AMC_CV startswith "3dzWsCzowES0hSgS"`

## Page-load measurement

This query measure the time it takes from the time the component start initializing to the time all data is ready and the page is loaded and ready for the user to interact with.

```
// Measure NorthStar page load time
```

`let startTime = ago(30d);`

`let endTime = now();`

```
ManagePaymentsTelemetryEvents
```

`| where EventTime >= startTime and EventTime <= endTime`

`  and EventName == "HealthOnInitialize"`

`  and EventProperties.healthy == true`

`  | extend endEventTime = EventTime`

`  | project endEventTime, CV, endEventName = EventName, endBaseCv = GetCVBase(CV), endBaseAmcCv = GetCVBase(AMC_CV)`

`| join kind=innerunique (`

`  ManagePaymentsTelemetryEvents`

`  | where EventName == "GetTransactions_startFetch"`

`    and EventTime >= startTime and EventTime <= endTime`

`  | project startEventTime = EventTime, CV, startEventName = EventName, startBaseCv = GetCVBase(CV), startBaseAmcCv = GetCVBase(AMC_CV)`

`  ) on $left.endBaseCv == $right.startBaseCv and $left.endBaseAmcCv == $right.startBaseAmcCv`

`  | where startEventTime <= endEventTime`

`  | extend loadTime = (endEventTime - startEventTime)`

`  | extend loadTimeInSeconds = bin(loadTime / 1s, 1)`

`  | project startEventName, endEventName, startEventTime, endEventTime, startBaseCv, endBaseCv, startBaseAmcCv, endBaseAmcCv, loadTime, loadTimeInSeconds, CV`

```
//  | where loadTimeInSeconds > 30 // Finds only the loads that took more than 30 secs
```

`| summarize Count = count() by loadTimeInSeconds`

```
//| summarize Count = count() by bin(startEventTime, 1d) // Groups the count by day for a better rendering
//| render timechart  // Shows a graph of the load time over time.
```

Note you can uncomment some of the lines above to filter the data further or get a graph.