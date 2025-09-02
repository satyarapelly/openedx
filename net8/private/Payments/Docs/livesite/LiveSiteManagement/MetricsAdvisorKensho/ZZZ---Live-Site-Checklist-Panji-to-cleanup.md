# ZZZ - Live Site Checklist [Panji to cleanup]

1. [List of IcM for our Team](https://icm.ad.msft.net/imp/v3/incidents/search/advanced?sl=jncvezf0ym2) - Ack Incident and see if multiple incidents exist

2. If we already have a corresponding PXService alert, we can ignore the next steps listed here and focus on the PXService alert.

3. [PIDLSDK Dashboard](https://xpert.microsoft.com/osg/views/PidlSdkReport/054ab90f-a70d-49f8-b3e6-fdd5dcae0fd0) - In the dashboard look at 'Terminal Errors' Section and see the error trends for the triggered alert

4. [Search PIDLSDK Events](https://xpert.microsoft.com/osg/Search?display=Logs&amp;searchQuery=%40eventName%3D%3D%22failure%22&amp;source=Environment%3DPROD%3BModernClient%3DRealtime%3BVEFProvider%3DMicrosoft.Commerce%3BVEFProvider%3DPayments%3BVEFProvider%3DPidlSdk%3BVEFTopic%3DPageEvent%3B) - Search for the failures and update the query based on the triggered alert

5. Payments Ops / SRE
6. | Service | IcM Team Name |
| --- | --- |
| Accounts | [StoreCore-PST-SRE](https://icm.ad.msft.net/imp/v3/oncall/current?serviceId=23443&amp;teamIds=45248&amp;scheduleType=current&amp;shiftType=current&amp;viewType=1&amp;gridViewStartDate=2019-11-25T05:45:13.330Z&amp;gridViewEndDate=2020-05-21T07:00:00.000Z&amp;gridViewSelectedDateRangeType=9) |

Pidlsdk Usage Dash

https://xpert.microsoft.com/osg/views/PidlSdkUsage/881deb75-923c-4d74-bddc-3bd1df43a27e