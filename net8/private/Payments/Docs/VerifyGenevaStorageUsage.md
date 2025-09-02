### Steps to idenitfy the storage accounts used by Geneva

1. Navigate to the Geneva Logs Account Settings. Go to the **Resources** section.
![Geneva Logs Account Settings](..../../../docs/images/geneva-storage/log-acc-settings.png)

2. Click on the **"Subscription Health Dashboard"**. It'll land in the Jarvis Dashbaord page.
3. Go to **HealthMetrics-Resource** chart. Click on the **Settings** icon and make sure that the **MdsAccount** filter contians the correct MdsAccount name.
![Geneva Storage Dashboard](..../../../docs/images/geneva-storage/geneva-storage-dash.png)

4. The names displayed in the chart should match the storage account names you see  in the Azure portal subscription:
![Geneva Storage Dashboard](..../../../docs/images/geneva-storage/subscription-storage-accounts.png)

5. Any extra storage accounts we see in Azure portal but not in the Jarvis dahsboard can be safely deleted.

6. These steps are in addition to all the other verification steps, like seeing no activity on the storage account and the capacity etc.