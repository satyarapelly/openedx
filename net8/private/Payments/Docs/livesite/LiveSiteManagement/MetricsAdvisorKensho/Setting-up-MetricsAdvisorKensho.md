# Setting up MetricsAdvisor/Kensho

We are currently using MetricsAdvisor (https://metricsadvisor.azurewebsites.net/)

Here are the steps we follow to setup a new data feed and set monitoring and alerts on its metrics

1. Add a data feed by giving the connection string and query details. We can copy the connection string for our Kusto cluster from any of the existing data feeds.
2. Define the dimensions and measures from the result set of the query.
3. Once the feed is added, make sure to add other admins and viewers.
4. Select the required metrics from the feed and add/edit detection and alerting configurations

![](/images/livesite/1-8deaf05730af4d27b212738b4c120653.png)

![](/images/livesite/1-34d2b2b2643b465bb1d2e08d86d3b980.png)

![](/images/livesite/1-7f8ac17268594029b6be8b9cf87ee43a.png)

![](/images/livesite/1-e322092781d44744bfcad7f45d9c9242.png)

![](/images/livesite/1-33989dacfc354e8e9c129840e14ac047.png)