# Existing data feeds and Alerts

**PX**

We have the following 2 data feeds from **PX** telemetry data

1. **PXMetrics\_30MinsLookback** ([Metrics Advisor](https://metricsadvisor.azurewebsites.net/data-feed/3b64d913-8d6b-48a2-bd77-eb444bf3fb9d))
    1. This feed runs every 5 mins, and when it runs, it gets the data for the last 30 mins.
    2. We have the following alerts configured for this feed
        1. On **Reliability** metric, **PXReliabilityThresholdAlertSev2**.
            1. This alert will be triggered when Reliability dropped below 99.9 % (for nsm, below 99%) and unique users affected are above 10 (for Azure or CommercialStores) or above 100 (for all other partners)

1. **PXMetrics\_24HoursLookback** ([Metrics Advisor](https://metricsadvisor.azurewebsites.net/data-feed/73e5bbe7-a3b9-4a8a-8eae-3b3c3982bb7d))
    1. This feed runs every 12 hours, and when it runs, it gets the data for the last 24 hours.
    2. We have the following alerts configured for this feed
        1. On **Reliability** metric, **PXReliabilitySmartDetectionAlertSev4**.
            1. This alert will be triggered when Reliability dropped below smart threshold and unique users affected are above 100
        2. On **SuccessRate** metric, **PXSuccessRateThresholdSev3.**
            1. This alert will be triggered when SuccessRate dropped below 1 and unique users succeeded is below 1

**PIDLSDK**

We have the following 2 data feeds for **PIDLSDK** telemetry data

1. **PIDLSDKMetrics\_30MinsLookback** ([Metrics Advisor](https://metricsadvisor.azurewebsites.net/data-feed/15a9467e-daea-4dce-a06b-1a68ef52903d))
    1. This feed runs every 5 mins, and when it runs, it gets the data for the last 30 mins.
    2. We have the following alerts configured for this feed
        1. On **Reliability** metric, **PIDLSDKRelibilityThresholdAlertSev2**.
            1. This alert will be triggered when Reliability dropped below 99.9 % (for nsm, below 99%) and unique users affected are above 10 (for Azure or CommercialStores) or above 100 (for all other partners)

1. **PIDLSDKMetrics\_24HoursLookback** ([Metrics Advisor](https://metricsadvisor.azurewebsites.net/data-feed/f77f3ee9-fcc8-451a-a42e-f2373e6feaf9?sourceId=455&amp;notificationId=67287e6b-65bc-4459-a754-98555f340637&amp;alertType=Anomaly))
    1. This feed runs every 12 hours, and when it runs, it get the data for the last 24 hours.
    2. We have the following alerts configured for this feed
        1. On **ReliabilityInclUnknownErrors** metric, **PIDLSDKReliabilityThresholdAlertSev3**.
            1. This alert will be triggered when Reliability including unknown errors dropped below 99.9 for addorupdatepi scneario for 'Azure', 'CommercialStores', 'Webblends' and 'Xbox'. This is targeted for scenario health scenarios.
        2. On **ReliabilityInclUnknownErrors** metric,  **PIDLSDKReliabilitySmartDetectionAlertSev4.**
            1. This alert will be triggered when Reliability including unknown errors dropped below smart threshold and unique users affected are above 100

**Note** : We can see all the alert configurations at one place ([Metrics Advisor](https://metricsadvisor.azurewebsites.net/hook-setting)) and we can pause an alert and start an alert from it

![](/images/livesite/1-776c75794a9143d0951febaad6acbf8d.png)

**PXReliabilityThresholdAlertSev2**

![](/images/livesite/1-0fa9c6c1af954b34b542ab81b112b2a3.png)

**PXReliabilitySmartDetectionAlertSev4**

![](/images/livesite/1-e6fd98a417a8441b9d73cc82fdea27c9.png)

**PXSuccessRateThresholdSev3**

![](/images/livesite/1-2dacc1ec26e0426f94ce78ef8cce87df.png)

**PIDLSDKRelibilityThresholdAlertSev2**

![](/images/livesite/1-df4d38016cc74137abd110195a9b5af7.png)

**PIDLSDKReliabilityThresholdAlertSev3**

![](/images/livesite/1-2c02967e73844a35aa0571ba5d2a758d.png)

**PIDLSDKReliabilitySmartDetectionAlertSev4**

![](/images/livesite/1-ac2ee497b893440fa7dbe2e3cd137f1c.png)

![](/images/livesite/1-330288f8187340c7a59829be236b9c73.png)

**PXUserErrorRateThresholdAlertSev3 ( this is currently paused as it is causing lot of noise)**