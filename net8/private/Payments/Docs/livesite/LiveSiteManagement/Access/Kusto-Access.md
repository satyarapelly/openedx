# Kusto Access

All the PX and PIDL SDK log is stored in PST Kusto cluster:

## Please follow the step below to get PST Kusto cluster:

1. <span style="background-color:#e8ebfa">Click on the link to which Kusto cluster you are needing access to:</span>
2. **<span style="background-color:#e8ebfa">PST Kusto Access (Prod)</span>**<span style="background-color:#e8ebfa">:&#160;</span>[Link](https://myaccess/identityiq/ui/rest/redirect?rp1=/accessRequest/accessRequest.jsf&amp;rp2=accessRequest/review?role=PST+Kusto+Access&amp;autoSubmit=true)<span style="background-color:#e8ebfa">&#160;-&#160;Provides access to pst.kusto.windows.net, PROD Database (all the PX/PIDL log are in the Prod Kusto cluster)</span>
3. **<span style="background-color:#e8ebfa">PSTTest Kusto Access (Non-Prod)</span>**<span style="background-color:#e8ebfa">:&#160;</span>[Link](https://myaccess/identityiq/ui/rest/redirect?rp1=/accessRequest/accessRequest.jsf&amp;rp2=accessRequest/review?role=PSTTest+Kusto+Access&amp;autoSubmit=true)<span style="background-color:#e8ebfa">&#160;-&#160;Provides access to testpst.westus2.kusto.windows.net, INT &amp; Test databases(no need for PX or PIDLSDK log)</span>
4. <span style="background-color:#e8ebfa">Click Submit</span>
5. <span style="background-color:#e8ebfa">Select your account, and one of the follow two roles and a justification on why you are needing access:</span>
6. **<span style="background-color:#e8ebfa">Viewer</span>**<span style="background-color:#e8ebfa">: Kusto Database Viewer Role - Can read all data and metadata of any database.</span>
7. **<span style="background-color:#e8ebfa">User</span>**<span style="background-color:#e8ebfa">: Kusto Database User Role -&#160;Can read all data and metadata of the database. Additionally, can create tables and become the table admin for those tables, and create functions in the database.&#160; This role will be limited to Payments Engineering team members</span>
8. <span style="background-color:#e8ebfa">Click next</span>
9. <span style="background-color:#e8ebfa">Click Submit Request</span>

## Setup Kusto Client

Refer section "Access Kusto" in [Observability (pxdocs.azurewebsites.net)](https://pxdocs.azurewebsites.net/operations/observability.html)

![](/images/livesite/1-18241833e7a90f453dce635d02f4ae88.png)1. Refer section "Sample PX Queries" and "PIDLSDK Queries" in [Observability (pxdocs.azurewebsites.net)](https://pxdocs.azurewebsites.net/operations/observability.html) to setup Kusto Client to verify our Kusto access is setup correctly