# ICMs (Sev 1/2)

- Primary on-call joins the bridge and works on troubleshooting/mitigating/resolving the ICM.
    - <span style="background-color:yellow">Please make sure your phone call is unmuted - Test out our phone is correctly setup</span>
        - <span style="background-color:yellow">Primary oncall -&gt;Secondary oncall -&gt; EIM -&gt; Secondary EIM -&gt; Management</span>
    - <span style="background-color:yellow">Answer the phone call and acknowledge the incidence</span>
    - <span style="background-color:yellow">If you can&#39;t figure out the reason within 10 mins, please engage the 2</span><sup style="background-color:yellow">nd</sup> <span style="background-color:yellow">oncall</span>
        - <span style="background-color:yellow">The first 2 counts in the fundamental meeting</span>
            - <span style="background-color:yellow">TTD: How soon the issue was detected</span>
            - <span style="background-color:yellow">TTM: How soon mitigate the call (</span><span style="background-color:yellow">TTE: How soon join the call</span><span style="background-color:yellow">)</span>
    - <span style="background-color:yellow">Engage EIM right way if it is a real Sev2/Sev1 issue</span>
        - <span style="background-color:yellow">Sev2 and above requires EIM communication email to Marcel</span>
        - <span style="background-color:yellow">EIM helps engaging teams</span>
    - <span style="background-color:yellow">If you try to join a bridge and the OSOC team does not join, it might be because the sev was caused by an OSOC outage, for which an email might have been sent. Keep an eye on your incoming messages to see if an outage is related to the sev.</span>
- Here are some documentation to look at
    - [IcM Ticket Guide (pxdocs.azurewebsites.net)](https://pxdocs.azurewebsites.net/operations/icm-ticket-guide.html)
    - https://pxdocs.azurewebsites.net/operations/livesite-sop.html
    - https://pxdocs.azurewebsites.net/operations/observability.html
    - [OnCall DRI Handbook-v3.published.pdf](https://microsoft.sharepoint.com/teams/DRICulturev-Team/Shared%20Documents/Forms/AllItems.aspx?id=%2Fteams%2FDRICulturev%2DTeam%2FShared%20Documents%2FGeneral%2FPublished%20Handbook%2FOnCall%20DRI%20Handbook%2Dv3%2Epublished%2Epdf&amp;parent=%2Fteams%2FDRICulturev%2DTeam%2FShared%20Documents%2FGeneral%2FPublished%20Handbook&amp;p=true)
- Walk through an incident
    - You will receive txt message, email and call
    - Acknowledge in txt message, call or ICM (Refer: Figure I for acknowledge button)
    - Join the bridge
        - If the bridge isn't created or osoc doesn't join bridge, send email to osoc@microsoft.com to engage
    - Look the email and incident to determine whether we need assistance
        - External issue: [Incident-327136639](https://portal.microsofticm.com/imp/v3/incidents/details/327136639/home)
            - Ask whether there is a outage ongoing, if so join the main bridge
            - If not: engage the external team [Livesite SOP (pxdocs.azurewebsites.net)](https://pxdocs.azurewebsites.net/operations/livesite-sop.html)
        - Machine/network issue: [Incident 325492353](https://portal.microsofticm.com/imp/v3/incidents/details/325492353/home)
    - What is the impact?
        - Service side: [PX Len Reliability Dashboard](https://lens.msftcloudes.com/#/dashboard/5dc09589-664c-4f9e-b41e-02bb73b36eab?temp=0&amp;isSample=false&amp;_g=%28ws:e377c266-06a6-4858-9d50-4e24c8d098ed%29)
        - Client side: [PX MetricsAdvisor Alerts - TSG](onenote:#PX%20MetricsAdvisor%20Alerts%20-%20TSG&amp;section-id={7299DEF3-4AB2-4959-A423-5742D6772C8A}&amp;page-id={D3447F71-050B-4B0D-A79F-01479D82B220}&amp;end&amp;base-path=https://microsoft.sharepoint.com/teams/PaymentExperience/SiteAssets/Payment%20Experience/LiveSite/Live-Site-Management.one)
            <span style="background-color:#fffffe">GetPidlSdkFailureDetails(ago(1h), now(), &quot;*&quot;, &quot;*&quot;, &quot;*&quot;, &quot;systemerror&quot;)</span>
            - POST PI with retry:  [8/24/2022](onenote:..\Run-The-Business\Weekly%20Meeting.one#8\24\2022&amp;section-id={664DAC9D-0C43-4865-8813-904B1389E702}&amp;page-id={C8059CE5-9B57-43BD-A2D5-A29B1A254C1C}&amp;end&amp;base-path=https://microsoft.sharepoint.com/teams/PaymentExperience/SiteAssets/Payment%20Experience)
        - More details on the impact
            - API
            - # customer
            - Partners (portal)

    Figure I: if the incident acknowledge, the button in yellow should be grey out.
![](/images/livesite/1-af223af742c34605825d2e9284b61d8d.png)
    Figure II: Mitigate issue due to External issue (outside PX/PIDLSDK)
![](/images/livesite/1-79598a96e6b44ac1b2741512a25fc6f1.png)

[Incident 325492353](https://portal.microsofticm.com/imp/v3/incidents/details/325492353/home) : PX PROD Overall Reliability

<span style="background-color:#fffffe">| summarize count() by PartnerName, PidlSdkOperationType</span>