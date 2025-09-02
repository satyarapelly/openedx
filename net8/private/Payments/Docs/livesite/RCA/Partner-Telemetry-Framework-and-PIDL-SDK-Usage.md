# Partner Telemetry Framework and PIDL SDK Usage

| **Service/App Name** | **Partner Name** | **Service/App's**<br><br>**Telemetry Framework** | **Using React/JS** | **Using Bundle/JS file directly** |
| --- | --- | --- | --- | --- |
| Azure Ibiza | azure | Ibiza custom | <br> | JS File directly |
| Office Signup | commercialstores | 1DS | <br> | Webpack bundle |
| Office Manage | commercialstores | Aria | <br> | Webpack bundle |
| Office Checkout | commercialstores | Aria | <br> | Webpack bundle |

Aria framework is recommending to use 1DS SDK: [Download SDK | Aria - Event Analytics](https://www.aria.ms/developers/downloads/legacy-downloads/)

| **Service/App Name**  | **Partner Name**  | **POC** | **Service/App's** **Telemetry Framework**  | **Using React/JS**  | **Using Bundle/JS file directly**  | **Are they blocking on file download?** | **Comments** | **Delta between client and server logs**<br><br> |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Azure Ibiza  | azure  | Amber Bhargava | Ibiza custom  |  JS | JS File directly  | Earlier they were. They fixed it. | Azure applied fix to load PIDLSDK even if 1DS is not loaded. Not followed up since. | <br><br>| percentage | HasOnlyServiceTelemetry | TotalTelemetry |<br>| --- | --- | --- |<br>| 16.8437 | 24783 | 147135 |<br><br><br> |
| Office Signup  | commercialstores  | <br> | 1DS  |   | Webpack bundle  | <br> | <br> | <br> |
| Office Manage  | commercialstores  | Prashanth/Isabel | Aria  |   | Webpack bundle  | <br> | <br> | <br> |
| Office Checkout  | commercialstores  | Marco | Aria  |  React | Webpack bundle  | No | <br> | <br> |
| amcweb | amcweb | Lovish |  1DS | React | Webpack bundle | <br> | <br> | <br> |
| ~~amcweb~~ | ~~amcweb~~ | ~~Lovish~~ |  ~~1DS~~ | ~~JS~~ |  ~~NA~~ | ~~NA~~ | ~~<span style="background-color:lime">No JS version</span>~~ | <br> |
| Bing Ads | bing | <br> |   | JS |   | <br> | No follow up yet. | <br><br>| percentage | HasOnlyServiceTelemetry | TotalTelemetry |<br>| --- | --- | --- |<br>| 23.4836 | 1386 | 5902 |<br><br><br> |
| UniversalWebStore.Cart | Cart | Ryan Teel/Vicente | 1DS  | React |   | No | <br> | <br> |
| UniversalWebStore.Cart | cart | Ryan Teel/Vicente | JSLL | JS (Ember) | JS File | No | Facing errors related to 1DS so no telemetry/limited telemetry for PIDL<br><br><br>Athul is investigating 1DS load issues in cart portal | <br><br>| percentage | HasOnlyServiceTelemetry | TotalTelemetry |<br>| --- | --- | --- |<br>| 14.3114 | 12431 | 86861 |<br><br><br> |
| UniversalWebStore (BuyNow) | webblends | Ryan Teel/Vicente | 1DS | React | <br> | No | <br> | <br> |
| UniversalWebStore<br><br>(BuyNow) | webblends | Ryan Teel/Vicente | JSLL | JS (Ember) | <br> | No | Facing errors related to 1DS so no telemetry/limited telemetry for PIDL. Need to confirm from webblends team | <br> |
| XboxCom | webblends | Ayisha/Mustafa Zubair |  1DS | React | Webpack bundle | No | <br> | <br> |
| Store Microsoft Com | Cart | Ryan Teel/Vicente | <br> | React | Webpack bundle | No | <br> | <br> |
| Store Microsoft Com  | Cart | Ryan Teel/Vicente |   | JS (Ember) |  JS File | No | Facing errors related to 1DS so no telemetry/limited telemetry for PIDL | <br> |
| MSEG | mseg | <br> |   | React |   | <br> | <br> | <br> |
| North Star | northstarweb | Jorge |   | React | Webpack bundle | No | <br> | <br> |
| storewebsdk | officeoobe | <br> |   | React |   | <br> | <br> | <br> |
| Office OOBE  | officeoobe | <br> |   | JS |   | <br> | <br> | <br> |
| Partner Center | payin | <br> |   | JS |   | <br> | <br> | <br> |
| Setup Office | setupoffice | <br> |   | JS |   | <br> | Ireland team task with investigation | <br><br>| percentage | HasOnlyServiceTelemetry | TotalTelemetry |<br>| --- | --- | --- |<br>| 62.1725 | 28710 | 46178 |<br><br><br> |
| SMB OOBE | smboobe | <br> |   | React |   | <br> | <br> | <br> |
| OfficeCheckout | webblends | <br> |   | React |   | <br> | It is not on webblends and getting migrated to omex,  | <br> |
| OfficeDime | webblends | Ayisha/Mustafa Zubair |  1DS | React | Webpack bundle | No | <br> | <br> |
| OfficeCom | webblends\_inline/webblends | Ayisha/Mustafa Zubair |  1DS | React | Webpack bundle | No | <br> | <br> |
| Office COM | webblends\_inline/webblends | Ayisha/Mustafa Zubair |  1DS | React | Webpack bundle | No | <br> | <br> |
| MinecraftNet | webblends | Ayisha/Mustafa Zubair |  1DS | React | Webpack bundle  | No | <br> | <br> |
| PCGamingApp | webblends | Ayisha/Mustafa Zubair |  1DS | Ember | Webpack bundle | No | Facing errors related to 1DS so no telemetry/limited telemetry for PIDL | <br> |
| universalstoreinapp | webblends | Ayisha/Mustafa Zubair |  1DS | Ember | Webpack bundle | No | Facing errors related to 1DS so no telemetry/limited telemetry for PIDL | <br> |
| UniversalStoreNativeClient | webblends | Ayisha/Mustafa Zubair |  1DS | Ember | Webpack bundle | No | Facing errors related to 1DS so no telemetry/limited telemetry for PIDL | <br> |
| UniversalXboxApp | webblends | Ayisha/Mustafa Zubair |  1DS | Ember | Webpack bundle | No | Facing errors related to 1DS so no telemetry/limited telemetry for PIDL | <br> |
| xCloud | webblends | Ayisha/Mustafa Zubair |  1DS | React | Webpack bundle | No | <br> | <br> |
| Clipchamp | webblends | Ayisha/Mustafa Zubair |  1DS | React | Webpack bundle | No | <br> | <br> |
| SurfaceApp  | webblends | Ayisha/Mustafa Zubair |  1DS | React | Webpack bundle | No | <br> | <br> |
| StoreApp | webblends | Ayisha/Mustafa Zubair | 1DS | React | Webpack bundle | No | <br> | <br> |
| Mercury | xbox | Jakov Smelkin/<br><br>Logan Apple |  1DS | React | Webpack bundle | No | <br> | <br> |
| Mercury | cart | Ryan Teel/Vicente | JSLL | JS (Ember) | <br> | No | Facing errors related to 1DS so no telemetry/limited telemetry for PIDL | <br> |
| Saturn | xbox/ | Jakov Smelkin/<br><br>Logan Apple |  1DS | React | Webpack bundle | No | <br> | <br> |
|  XBOX | xbox | Jakov Smelkin/<br><br>Logan Apple |  1DS | React | Webpack bundle | No | <br> | <br> |
| XBOX Web  | xboxweb | Jakov Smelkin/<br><br>Logan Apple |  1DS | React | Webpack bundle | No | <br> | <br> |
| Webblends XBOX | webblendsxbox | <br> |   | JS |   | <br> | <br> | <br> | <br><br>| percentage | HasOnlyServiceTelemetry | TotalTelemetry |<br>| --- | --- | --- |<br>| 20.6216 | 491 | 2381 |<br><br><br> |

xboxsettings

xboxsubs

amcxbox

storify

Oxowebdirect

Consumersupport -&gt; CMAT team.

Commercialsupport -&gt; CMAT team.

Payin -&gt; Partner Center

Setupofficesdx