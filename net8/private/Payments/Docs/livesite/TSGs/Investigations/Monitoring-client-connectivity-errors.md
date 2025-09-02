# Monitoring client connectivity errors

Here are some sample queries to monitor client connectivity errors

`GetPidlSdkReliabilityMetrics(datetime("2022-10-17"), datetime("2022-10-19"), 1h, "addorupdatepi", "*", "*")`

`| project datetime_add('hour', -7, Timestamp), OperationName, TotalStarted, TotalFailed, TotalConnectivityErrors, ConnectivityErrorRate, UniqueIPsWithConnectivityErrors, UniqueUsersWithConnectivityErrors`

`| render timechart`

```
PidlSdkTelemetryEvents
```

`| where IngestTime between (datetime("2022-10-17") .. datetime("2022-10-19"))`

`and name == "Ms.Webi.OutgoingRequest"`

`and OSRTargetUri startswith "`https://paymentinstruments.mp.microsoft.com/`"`

`| summarize count(), percentiles(OSRLatencyMS, 5, 25, 50, 90) by bin(datetime_add('hour', -7, IngestTime), 1h), OSRSucceeded`

`| render timechart`