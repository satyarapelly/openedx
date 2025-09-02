# TSG: Debug Update Profile invalid header value for if-match

[Incident-332048412 Details - IcM (microsofticm.com)](https://portal.microsofticm.com/imp/v3/incidents/details/332048412/home)

**Expected behavior**: If-Match is mismatch between Get and PUT calls. More explanation on [if-match or etag](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/If-Match).

Following the steps below to repo issue or validate Prod works as expected.

1. Following the instruction here to create a test account [Get Access token for HAPI and Jarvis FD.docx](https://microsoft-my.sharepoint.com/:w:/p/jiefan/ESpc2nV4zQ9KoJN-vgUzYAABo1bN625YnX1A6ILR9qyeQg?e=UxBhsm)
2. Click edit in yellow to load update profile page
3. ![](/images/livesite/1-c3edbe4fc1130697298748947179ea10.png)

4. ![](/images/livesite/1-9b033723876d0ac014b7df2314617cce.png)

5. "If-Match" is getting from GET Patch Profile call
6. Click Save
7. ![](/images/livesite/1-8750f8aa09c30ee60e3ad24c14fa20de.png)
    Click "Use this Address"
8. The PATCH profile call successes
9. Double confirm  the GET Profile call response header if-match/etag matches Patch Profile call if-match/etag

As conclusion, the PIDL SDK works as expected.