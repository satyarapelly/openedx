# JIT Access

## Pre req: Refer[JIT Access Prerequisite](JIT-Access-Prerequisite.md)page

## Steps to request JIT access:

1. Log in to JIT Access Portal.
    Use your SAW machine to open
    [https://jitaccess.security.core.windows.net/WorkFlowTempAccess.aspx](https://nam06.safelinks.protection.outlook.com/?url=https%3A%2F%2Fjitaccess.security.core.windows.net%2FWorkFlowTempAccess.aspx&amp;data=02%7C01%7CYuanji.Wang%40microsoft.com%7C54bf1d9ed8ae4a4a9db708d6fb2b78f1%7C72f988bf86f141af91ab2d7cd011db47%7C1%7C0%7C636972561460332706&amp;sdata=idjoZx0VFgmIi91nKK0fcwG1O08gjv5qsQVj6kiJfKw%3D&amp;reserved=0)
    Log in using pme.glb account. In the login page, select "sign in using an X.509 certificate" (make sure your YubiKey is loaded with certificates and inserted into SAW):
<!-- Processing ImageNode failed -->
    <span style="background-color:white">Select the correct certificate in the pop-up window:</span>
<!-- Processing ImageNode failed -->    <span style="background-color:white">Trouble shoot: If the above pop-up window didn&#39;t show, please use a new incognito window or use another Browser to retry.</span>

    <span style="background-color:white">Select YubiKey as the smart card device (touch the copper part if the Yubikey option is not shown):</span>
![](/images/livesite/1-22475439940446eb84a900aea5c3138a.png)
    <span style="background-color:white">Enter the pin of YubiKey in the next pop-up window then log in to JIT portal:</span>
<!-- Processing ImageNode failed -->
2. <span style="background-color:white">Submit a request of JIT access on JIT portal. Details to be filled:</span>
    1. <span style="background-color:white">WorkItem Source: &quot;other&quot; or &quot;IcM&quot;</span>
    2. <span style="background-color:white">Work Item ID: Task id (when choosing &quot;other&quot; as WorkItem source) or IcM id (when choosing &quot;IcM&quot; as WorkItem source).</span><span style="background-color:white">&#160;</span>
    3. <span style="background-color:white">Justification: Provide justification for PROD Access.</span><span style="background-color:white">&#160;</span>
    4. <span style="background-color:white">Resource Type: Select Portal from the dropdown list.</span><span style="background-color:white">&#160;</span>
    5. <span style="background-color:white">Subscription ID:&#160;</span>**<span style="background-color:white">Ex</span>**<span style="background-color:white">: PME INT Subscription ID(</span>230ef3cc-8fdd-4f26-bf9c-10131b4080e5<span style="background-color:white">)</span><span style="background-color:white">&#160;. Please find PX Azure PME subscription IDs below.</span>
<!-- Processing ImageNode failed -->
3. <span style="background-color:white">Click on</span> Validate & Add Resource
4. Click on Submit
    Once the Request is submitted using above details, Approvers will get an email with the approval link and they need to review and approve the access from SAW device.

<!-- Processing ImageNode failed -->
<!-- Processing ImageNode failed -->

<!-- Processing ImageNode failed -->
<!-- Processing ImageNode failed -->

***<span style="background-color:silver;text-decoration:underline">Powershell Commands to request access:</span>***

pushd `\\reddog\Builds\branches\git_security_jit_master_latest\RDTools\WAPD\JITShell`

import-module .\jitshell

New-JITRequest -env product -src IcM -wid <span style="background-color:yellow">&lt;Replace with ICM Ticket Number&gt;</span> -Justification "JIT Access" -rtype Portals -subId <span style="background-color:yellow">&lt;Replace with Subscription ID&gt;</span> -AccessLevel Administrator -ver 2015-09-07.1.0

Trouble shootings:

Troubleshooting JIT Elevation Failures:  [Troubleshooting JIT Evevation Failures](https://microsoft.sharepoint.com/:w:/r/teams/AzureSecurityCompliance/Security/_layouts/15/Doc.aspx?sourcedoc=%7BDBB1593A-CA98-4075-BFAA-8C234164819D%7D&amp;file=DebuggingMyRejectedJITRequests.docx&amp;action=default&amp;mobileredirect=true&amp;cid=dd066151-0d77-4c71-a1d4-76d6c93d11f4)

[Azure JIT for Payment Services](onenote:https://microsoft.sharepoint.com/teams/pims/Shared%20Documents/PIMS/OnCall.one#Azure%20JIT%20for%20Payment%20Services&amp;section-id={7FD11A78-8DD6-46CD-BF0D-910629C7B032}&amp;page-id={9009E36F-3A98-4225-A66D-5FE54FA8423F}&amp;end)

PX Azure PME subscription IDs

| <br> | Subscription  | Subscription ID |
| --- | --- | --- |
| 1 | PME Geneva INT | 86facc56-d6cb-484f-bc4e-9a7a7a848266 |
| 2 | PME Geneva PROD | 6c788ee2-6f07-48d2-b285-e56ececdcd99 |
| 3 | PME INT | 230ef3cc-8fdd-4f26-bf9c-10131b4080e5 |
| 4 | PME PROD | 9b6168fd-7d68-47e1-9c71-e51828aa62c0 |