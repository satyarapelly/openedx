# PSD2 Reliability Rate

What is this query?

This query tracks how many users are starting PSD2 challenges for purchases and completing them in a graceful manner

What is the impact?

Users are starting PSD2 challenges but unable to complete them.  This will block them from completing purchases.

How do I get user impact numbers?

Use below query to and for the partners sending the alert, get the number from the "UniqueUsersAffectedCount" column
```
let endTime = ago(1m);
let startTime = ago(30m);
PSD2PayerAuthAuthenticationEvents
| join kind=leftouter PSD2PayerAuthCompletionChallengeEvents on sessionId
| where transTime > startTime and transTime < endTime and transStatus == "C"
| summarize arg_max(transTime, *) by accountId
| summarize xboxTotalCount = dcountif(accountId, channel == "app_based"),
    xboxCompletedCount = dcountif(accountId, isempty(sessionId1) == false and channel == "app_based"),
    xboxUsersNotCompletedCount = dcountif(accountId, isempty(sessionId1) and channel == "app_based"),
    webTotalCount = dcountif(accountId, channel == "browser"),
    webCompletedCount = dcountif(accountId, isempty(sessionId1) == false and channel == "browser"),
    webUsersNotCompletedCount = dcountif(accountId, isempty(sessionId1) and channel == "browser")
    by partner
| extend UserPassRate = iif(xboxTotalCount > 0, round(((xboxCompletedCount * 1.0) / xboxTotalCount) * 100, 2), round(((webCompletedCount * 1.0) / webTotalCount) * 100, 2))
| extend UniqueUsersAffectedCount = iff(xboxUsersNotCompletedCount > 0, xboxUsersNotCompletedCount, webUsersNotCompletedCount)
| extend TotalUniqueUserAttemptsCount = iff(xboxTotalCount > 0, xboxTotalCount, webTotalCount)
| project TotalUniqueUserAttemptsCount, UniqueUsersAffectedCount, UserPassRate, partner
```

**Resolution**

    1. Check to see if there is a bridge with StoreCore-PST-DCS team
        a. ICM Team: StoreCore-PST-DCS
        b. If there is an existing bridge, merge bridges and work with Store-PST-DCS on call for resolution
        c. If StoreCore-PST-DCS does not have a bridge, likely an issue in PIDL/PX
    2. Was there a PX flight that was recently enabled that matches with the drop in reliability rate?
        a. If there was flight recently turned on for PSD2, revert that flight and monitor
    3. Was there a PX deployment that week that included PSD2 changes?
        a. Investigate if impact time matches with PX deployment time.
            i. Rollback PX deployment if impact start time matches
    4. Was there a PIDL deployment that week by a storefront partner?
        a. If only one group of partners has reduced reliability and there were no flight or PX changes, then issue lies with storefront partner.
            i. Partner ICM teams
                1. Webblends, cart, oxooobe
                    a. Store-Purchase Experience Incident Manager
                2. Oxowebdirect, oxodime
                    a. ESCALATE HERE – OMEX (yes this is the icm team name)
                3. Commercialstores
                    a. ??
                4. Azure
                    a. ??
                5. xbet
                    a. Xbox Storefront & Commerce Experiences PE Team 
                    b. XBET Services Point Engineer
                6. XboxSubs
                    a. CXT IcM Blue Crew
