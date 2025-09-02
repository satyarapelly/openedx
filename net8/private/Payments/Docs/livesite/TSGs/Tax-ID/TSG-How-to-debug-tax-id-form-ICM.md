# TSG: How to debug tax id form ICM

<span style="background-color:white">Here is an example to load tax ID in india</span>
1. <span style="background-color:white">Use&#160;</span><span style="background-color:white">AuthGenUI to generate live token in PROD environment</span>
2. <span style="background-color:white">Load PIDLSDK React Portal by&#160;</span>[link](https://pidlsdktestportal.azurewebsites.net/PidlReact.html?init=%7b%22elementFactoryName%22:%22officeFabric%22%2c%22environment%22:%22prod%22%2c%22market%22:%22in%22%2c%22partner%22:%22commercialstores%22%2c%22resourceType%22:%22TaxId%22%2c%22taxIdType%22:%22commercial_tax_id%22%7d)<span style="background-color:white">&#160;and put your live token into Authorization Token text box</span>
3. <span style="background-color:white">Click &quot;send&quot; button</span>
4. <span style="background-color:white">The taxid form should be shown</span>
5. <span style="background-color:white">Put &quot;NYIlr1344V&quot; into PAN ID text box</span>
![](/images/livesite/1-6e410bf5570241c59948f670f7c30229.png)6. <span style="background-color:white">Open developer tools and network tab</span>
7. <span style="background-color:white">Setup Fiddler trace</span>
    - <span style="background-color:white">Place the following</span> post\_taxid\_hapi<span style="background-color:white">.dat file into your fiddler response folder e.g.</span> E:\Docs\fiddler\_response\post\_taxid\_hapi.dat
    - Adding the following fiddler rule
        https://commerceapi.office.net/v1/my-org/taxids
        E:\Docs\fiddler\_response\post\_taxid\_hapi.dat
[post\_taxid\_hapi.dat](/images/livesite/post_taxid_hapi-1-f3a6d32b38f847e9bbed9a8a88a76f0c.dat)
8. <span style="background-color:white">Click Save button in the form</span>

<span style="background-color:white">In the AutoResponder tab, adding the following</span>
[post\_taxid\_hapi.dat](/images/livesite/post_taxid_hapi-1-f3a6d32b38f847e9bbed9a8a88a76f0c.dat)