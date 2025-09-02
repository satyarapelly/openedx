# JIT Access Prerequisite

## Prerequisite:

1. A SAW is need for getting JIT access. Please follow the instruction below to acquire and set up your SAW and join silo if needed:
    https://strikecommunity.azurewebsites.net/articles/673/onboard-as-a-new-cai-user.html
2. Request a PME account and YubiKey following the instruction below on your SAW machine:
    [https://msazure.visualstudio.com/AzureWiki/\_wiki/wikis/AzureWiki.wiki/29758/Account-Creation-and-YubiKeys](https://msazure.visualstudio.com/AzureWiki/_wiki/wikis/AzureWiki.wiki/29758/Account-Creation-and-YubiKeys)
    Under user account click create request, then select pme as domain name and enter your alias and fill out the form as shown below:
<!-- Processing ImageNode failed -->
3. Enroll certificates for Yubikey: https://microsoft.sharepoint.com/teams/CDOCIDM/SitePages/YubiKey-Management.aspx
4. Join PME domain security groups to get read access on the subscriptions and to request JIT access for subscription:
    <span style="background-color:white">Once you got your PME account created, using</span> https://oneidentity.core.windows.net<span style="background-color:white">&#160;from you SAW machine please join below PME Security Groups&#160;</span>**<span style="background-color:white">based on your role</span>**<span style="background-color:white">. Make sure to login to</span> https://oneidentity.core.windows.net<span style="background-color:white">&#160;with your PME account and YubiKey cert/pin.</span>
    <span style="background-color:white">&#160;</span>
    <span style="background-color:white">&#183;</span><span style="background-color:white">&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;</span><span style="background-color:white">&#160;</span>**<span style="background-color:white;text-decoration:underline">PME Domain Security Groups are:-</span>**

![](/images/livesite/1-cc1b647633a2097419ff09b61824a791.png)
![](/images/livesite/1-db15f209725703712d9268c252b5f6a3.png)
5. <span style="background-color:white">Add your alias in the text box and click add Members</span>
6. <span style="background-color:white">Click Modify</span>

![](/images/livesite/1-3f4b837834fe0ca21a7d6ee92b542f09.png)
![](/images/livesite/1-ee1d0e7ff0440a4c3849fb405ed25de8.png)
    <span style="background-color:white">&#160;</span>
    <span style="background-color:white;text-decoration:underline">Note that</span><span style="background-color:white">&#160;everyone member of “pme.gbl.msidentity.com\</span>**<span style="background-color:white">UST-PST-DevJITAccess</span>**<span style="background-color:white">” and “pme.gbl.msidentity.com\</span>**<span style="background-color:white">UST-PST-SREJITAccess</span>**<span style="background-color:white">” will get ‘</span>**<span style="background-color:white">Reader’ access to all PME Azure Subscriptions by default</span>**<span style="background-color:white">.</span>

7. Join the corresponding IDWEB group (http://aka.ms/idweb):
    Dev's need to join IDWEB group PaymentsDev-JITAccess to get read access on the subscriptions and to request JIT access for subscription.
    SRE's need to join IDWEB group PaymentsSRE-JITAccess to get read access on the subscriptions and to request JIT access for subscription.

8. Complete the CloudScreen Requirement: (Complete ASAP as this process can take up to 14 days)
    https://screening.microsoft.com/

<!-- Processing ImageNode failed -->

<span style="background-color:white">&#160;</span>

***<span style="background-color:white">Dev&#39;s JIT Access;</span>***

<span style="background-color:white">pme.gbl.msidentity.com\UST-PST-DevJITAccess</span>

![](/images/livesite/1-18ce1fac08be06c50c8a24c0d65ebbbf.png)
<span style="background-color:white">&#160;</span>

mccordmatt@microsoft.com

jorge.ledezma@microsoft.com

suchoudhury@microsoft.com

pawisesa@microsoft.com

subramanianh@microsoft.com

aaronvega@microsoft.com

anushrimarar@microsoft.com

rtalluri@microsoft.com

Mccordmatt,jorlede,suchoudhury,pawisesa,aaronvega, anushrimarar