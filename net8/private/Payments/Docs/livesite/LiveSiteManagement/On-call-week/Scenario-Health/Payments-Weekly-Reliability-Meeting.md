# Payments Weekly Reliability Meeting

PX oncall has to present scenario health report

1. Goto 'aka.ms/scenariohealth' ([Scenario Health (microsoft.com)](https://scenariohealth.microsoft.com/?engineeringManager=mdeller))
2. You can see scenario group as 'Pay' and it has 19 scenarios onboard
3. Click on that 'Pay' scenario group to see the filtered list of scenarios for Payments team.
4. Here is the breakdown of those **19 payment scenarios**:
    1. PX team scenarios : **Add or Update Payment Method UI** scenarios
        1. Add or Update Payment Method UI (Azure Direct) - This is for 'Azure' partner
        2. Add or Update Payment Method UI (Office Commercial) - This is for 'CommercialStores' partner
        3. Add or Update Payment Method UI (Store) - This is for 'Webblends' partner
        4. Add or Update Payment Method UI (Xbox One) - This is for 'Xbox' partner
    2. Selvan/PX team scenarios : **Handle payment challenge UI** scenarios
        1. Handle payment challenge UI (Azure Direct) - This is for 'Azure' partner
        2. Handle payment challenge UI (Office Commercial) - This is for 'CommercialStores' partner
        3. Handle payment challenge UI (Store) - This is for 'Webblends' partner
    3. Paymod/Transactions team scenario :
        1. **Customer can pay** (modern stack) - Please follow up with '**paytranoncall@microsoft.com**' for any dips in this scenario
    4. **PIMS** scenarios:  '**pimsoncalltask@microsoft.com**' for any dips in these scenarios
        1. Get payment method details (there are 6 different partner specific scenarios under it)
        2. List payment method (there are 5 different partner specific scenarios under it)
5. For any dip in the scenario, ideally we should have a corresponding ICM. If not, we need to tune our monitoring and alert system to auto detect these issues.
    If we have ICM incident, we need to link it by clicking **'Link IcM Items**' and search by giving ICM number and add it.

![](/images/livesite/1-e2275c4233154af592c016c9cfba90ae.png)
![](/images/livesite/1-6b87d608bdab417a99ff28efd4df2dc7.png)
6. In the 'GAP' analysis section in the scenario health portal, if there is any outage for our services that we can't map to any existing scenarios, we need to onboard the scenario by working with 'ScenarioHealth' team (peterco@microsoft.com, shriramn@microsoft.com)

**Query behind**:

`let dateTime = datetime(2022-07-31);`

`let startTime = datetime_add('hour', 7, dateTime);`

`let endTime = datetime_add('day', 1, startTime);`

`let scenarioName = "addorupdatepi";`

`let partnerName = "commercialstores";` **`//partner name`**

`cluster('pst.kusto.windows.net').database('Prod').GetPidlSdkReliabilityMetrics(startTime, endTime, time(null), scenarioName, partnerName, "*",  true)`

`| project Scenario="commerce.payments.addorupdatepaymentmethodUIstore", Partner = PartnerName, DataDateTime = dateTime`

`, Total = TotalStarted, Success = TotalStarted - TotalSystemErrors - TotalUnknownErrors, Failure = TotalSystemErrors + TotalUnknownErrors, ReliabilityInclUnknownErrors`

If there is a requirement to filter/change some error conditions, please update this function

cluster('pst.kusto.windows.net').database('Prod').**GetPidlSdkReliabilityMetrics**

If there is any change in errorcodes required, please update this function.

cluster('pst.kusto.windows.net').database('Prod').**GetPidlSdkErrorCategory**

**Refer to** [Existing PIDLSDK Reliability functions](Payments-Weekly-Reliability-Meeting.md)

**Query behind**:

`let dateTime = datetime(2022-07-31);`

`let startTime = datetime_add('hour', 7, dateTime);`

`let endTime = datetime_add('day', 1, startTime);`

`let scenarioName = "challenge.handlepaymentchallenge";`

`let partnerName = "webblends";` **`//partner name`**

`cluster('pst.kusto.windows.net').database('Prod').GetPidlSdkReliabilityMetrics(startTime, endTime, time(null), scenarioName, partnerName, "*",  true)`

`| project Scenario="commerce.payments.handlepaymentchallengeUIstore", Partner = PartnerName, DataDateTime = dateTime`

`, Total = TotalStarted, Success = TotalStarted - TotalSystemErrors - TotalUnknownErrors, Failure = TotalSystemErrors + TotalUnknownErrors, ReliabilityInclUnknownErrors`

**Sample query for investigating the failures:**

`let dateTime = datetime(2022-09-24);`

`let startTime = datetime_add('hour', 7, dateTime);`

`let endTime = datetime_add('day', 1, startTime);`

`let scenarioName = "challenge.handlepaymentchallenge";`

`let partnerName = "commercialstores"; //partner name`

`GetPidlSdkFailureDetails(startTime, endTime, scenarioName, partnerName, "*", "SystemError")`

![](/images/livesite/1-49a953351795476b833b1167f37edbf8.png)![](/images/livesite/1-d2a29278309d45f18d9ffe9956c856ad.png)![](/images/livesite/1-5616471171d9420dab154de690c2eeb2.png)

Existing PIDLSDK Reliability functions

![](/images/livesite/1-33c540e85eb64255bf783086472509c1.png)