# Known issue - low reliability on getWalletConfig

<u>**Known issues**</u>

If you see service error as below, please forward ICM to team investigate
IcM On-call : [Payment Services - ICM Tenant/ StoreCore-PST-TX](https://portal.microsofticm.com/imp/v3/oncall/current?serviceId=23443&teamIds=75758)

**Error:** System.Net.Http.HttpRequestException: A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond. (tokenization.cp.microsoft.com:443)

**Details:**

<u>**What happens**</u>

Since 11/15 9:00 AM, we received more traffic from microsoft.com due to thanksgiving wave 1 traffic. We start seeing wallet service starts having connection issues to tokenization service. The RPS is roughly ~1.  Wallet service reliability drops ~99.5% and high latency.

**Error:** System.Net.Http.HttpRequestException: A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond. (tokenization.cp.microsoft.com:443)
 
<u>**What impact**</u>

Since PIDLSDK client side has retry and fallback, the error shouldn't impact purchase. We are ok to hold the fix after thanksgiving.
 
<u>**Next steps**</u>
Ashish commits to put a cache in wallet service after lockdown to improve getWalletConfig reliability. 

<u>**Sample ICM:**</u> 

[Incident 442606542](https://portal.microsofticm.com/imp/v3/incidents/incident/442606542/summary) : Reliability of PIFD PX API (Hourly 99.75%|50|20k, Daily 99.8%|150|400k) Role: PIFDWebApp - GET_getWalletConfig_v7.0_{}---High latency(Wallet Service)
