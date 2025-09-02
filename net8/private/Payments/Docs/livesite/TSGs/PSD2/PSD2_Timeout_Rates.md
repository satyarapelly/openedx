# PSD2 Timeout Rates

**• Query to get number of affected users**
    let start = todatetime("2023-09-24 00:00:00");
    let end = todatetime("2023-09-25 00:00:00");
    let piids =
    PSD2PayerAuthCompleteChallengeEvents(start, end, false)
    | where transStatus in ("N")
    | where transStatusReason in ("14","81")
    | where challengeCancel in ("04","05")
    | distinct piid;
    TransactionDetails
    | where Timestamp between (start .. end)
      and TransactionStatus in ("declined")
       and (PaymentInstrumentId in (piids))
    | summarize arg_max(todatetime(Timestamp), *) by PaymentInstrumentId
| summarize piCount=count() by TransactionStatus, Country

    • This is a "SafetyNet" scenario, which means that even if the user times out, we still attempt to complete the charge.  If below Sev 2 threshold, can mitigate issue; but investigation is needed.

**• What to do if Sev 2?**

○ Storecore-PST-TX on call can help with identifying issues with connection to Mastercard
   
 ○ Was there a PX deployment recently with PSD2 changes and that is causing the issue?

 ○ See if rise in error matches with PX deployment timeline

○ Revert deployment 
[How to roll back](onenote:Live-Site-Management.one#How%20to%20roll%20back&section-id={8b6ab7aa-5f2d-4ea6-a6a6-305995c8710e}&page-id={12580ce3-6e96-4187-877a-2d1c13f9b6f2}&end)
[(Web view)](https://microsoft.sharepoint.com/teams/PaymentExperience/_layouts/15/Doc.aspx?sourcedoc={171acb93-a045-434e-938d-ccefe55457ad}&action=edit&wd=target%28LiveSite%2FLive-Site-Management.one%7C8b6ab7aa-5f2d-4ea6-a6a6-305995c8710e%2FHow%20to%20roll%20back%7C12580ce3-6e96-4187-877a-2d1c13f9b6f2%2F%29&wdorigin=703&wdpreservelink=1)

**• Is this a transient error?**

    ○ If the error fires on the graph with "(empty)" partner, there's most likely an issue in the PSD2 ecosystem as this is checking timeout rates across every partner.
        ○ Was there a PX deployment recently with PSD2 changes?
        ○ If not, there is likely an issue downstream with Mastercard or maybe PayerAuth.  
    ○ If the incident only fires for a single partner (e.x. commercialstores), it may relate to a storefront change
        ▪ If issue lasts for multiple hours, escalate to PX on call so they can get in contact with storefront
            ▪ Webblends - 
        ▪ In Metrics Advisor, look at the TimeoutCount dropdown to see how many timeouts we have.
            ▪ If less than 10, can mitigate issue
        ▪ Example
            ▪ CommericialStores partner had 100% time out rate issue caused by them enabling a content security policy on hosting iframe.  This led to bank challenges not loading correctly

**• Query to get timeout count in Kusto**

    ○ let startTime = ago(1d);
    let endTime = ago(1m);
    PSD2PayerAuthCompleteChallengeEvents(startTime, endTime, false)
    | summarize TimeoutCount = countif(transStatus in ("R","N","U") and transStatusReason in ("14","81") and challengeCancel in ("04","05","")),
        TotalChallengeCount = count() by partner
    | extend TimeoutRate = round((TimeoutCount)*1.0/TotalChallengeCount*100,2)
    
**• Query to get timeout errors in Kusto**
    let startTime = ago(1h);
    let endTime = ago(1m);
    PSD2PayerAuthCompleteChallengeEvents(startTime, endTime, false)
    | where transStatus in ("R","N","U") and transStatusReason in ("14","81") and challengeCancel in ("04","05","")
    
    
Use Queries here to get information for specific sessions [PSD2 Queries](onenote:#PSD2%20Queries&section-id={9d4e1697-e8e8-4a70-bbc2-717d69b04a3b}&page-id={ffd9a908-775b-426b-90f1-92d41ef3e8c0}&end)
[(Web view)](https://microsoft.sharepoint.com/teams/PaymentExperience/_layouts/15/Doc.aspx?sourcedoc={171acb93-a045-434e-938d-ccefe55457ad}&action=edit&wd=target%28LiveSite%2FTSGs.one%7C9d4e1697-e8e8-4a70-bbc2-717d69b04a3b%2FPSD2%20Queries%7Cffd9a908-775b-426b-90f1-92d41ef3e8c0%2F%29&wdorigin=703&wdpreservelink=1)

