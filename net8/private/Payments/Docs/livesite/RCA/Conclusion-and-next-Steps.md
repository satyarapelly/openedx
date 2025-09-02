# Conclusion and next Steps

- We need to document
    - Guidance to setup of PIDL SDK client side telemetry
    - Impact of missing client side telemetry.
- We also need to understand
    - If there is any other way to package 1DS files to make it easier for telemetry to be included.
    - How different partners are using PIDL SDK (react/JS, bundle/files)?
    - Do they have risk of blocking any flows if PIDL SDK telemetry is broken?
- There is some client telemetry loss for all partners and for most of them it is &lt;10%.
- For some partners, the client telemetry loss is high &gt;10% and for few partners, it is very high (setup.office.com has telemetry loss of 60+%). We need follow-up on these cases to understand root cause.
    - 07/22: The referrer URL has only domain name. Ravi is following up with setup office to root cause.
- For some partners, there is 100% telemetry loss. This is most likely they are not using 1DS files in their system. We need follow up on it.
    - We need to figure out a way for how to get information about such partners and get it done earlier.
    - Is there any other way, we can have these partners not miss client telemetry?
    - XBOX in React Native has decided to not use are sending telemetry in a different way as they didn’t want to have 1DS dependencies. Need to understand if this could be another way for other partners too?
        - We are getting telemetry from XBOX telemetry data and ingesting into our Kusto cluster. Need to confirm with Selvan.
- There is some telemetry loss in PX service as compared to PIFD. This seems to be some kind of bug which needs to be investigated.
    - 07/22: Kowshik started to look at it.
- Some other follow-ups
    - Understand from Commercial Stores team why in some cases comercialstores partner telemetry is 100% not there? They are using React and the bundle should always have 1DS file.

Other notes:

- Understanding what could be the cause for setupoffice.com?
    - Probably talking to them
    - Probably also looking at the path in the referrer URL