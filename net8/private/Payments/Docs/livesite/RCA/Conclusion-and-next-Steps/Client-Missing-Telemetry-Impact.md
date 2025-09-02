# Client Missing Telemetry Impact

**Usage of client side logs**

- Alerting for any live-site based on reliability drop.
    - Reliability based on system error.
        - If this issue is because of some service side errors. These should also be there on service.
        - If client cannot reach service, then it will only be on the client side.
- Troubleshooting any CRI or bugs reported by customers/partners.
    - Partners logs could be used for this too.
- Scenario health is monitored for major partners (XBOX, Webblends, CommercialStores,  Azure)
    - The main reason this was done from client side telemetry is that this gives a picture from end customer perspective (like client does not care if retries are done to make the scenario successful).
    - When it was designed, it was an informed decision that client telemetry is lossy and we might not get 100% accurate data.

**For a certain partner, if all client telemetry is missing**

We don’t have visibility in any client side issue. They can range from

1. Client not able to make a service call to get PIDL document
2. After getting PIDL document not able to render appropriately
3. PIDL document not able to submit input to PX (or other services)
4. Client not responding correctly to partner.

For all of these to detect, an alert could be configured on completion rate (not reaching terminal state).

**For a certain partner, if lot of (&gt;10%) client telemetry is missing**

We will not be able to do client side data analysis for those customers whom telemetry is missing.

One way to find regression in telemetry loss is to create a baseline and monitor the baseline.

We can have regular alerts for client side logs.

**For a certain partner, if some (&lt;10%) client telemetry is missing**

We will not be able to do client side data analysis for those customers whom telemetry is missing.

One way to find regression in telemetry loss is to create a baseline and monitor the baseline.

We can have regular alerts for client side logs.