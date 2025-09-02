# Building Pipeline
#### Target Audience:
* PX Engineering
#### Overview:
Build pipeline of PX service is migrating from CDPx to Onebranch. Both CDPx and OneBranch pipelines are working for PX. Building time is faster for OneBranch pipeline. CDPx build pipeline will be deprecated sometime in 2022.
### CDPx Build Pipeline Details
|Type|Name|Duration|Corresponding Release Pipelines|
|--|--|--|--|
|Official|[SC.csPayments.PX-Windows-Official-engineering](https://microsoft.visualstudio.com/Universal%20Store/_build?definitionId=56494)|41m to 52m|[Engineering region release pipeline](https://microsoft.visualstudio.com/Universal%20Store/_release?definitionId=11820&view=mine&_a=releases); [Feature region release pipeline](https://microsoft.visualstudio.com/Universal%20Store/_release?definitionId=12999&view=mine&_a=releases)|
|Official|[SC.CSPayments.PX-Windows-Official-master](https://microsoft.visualstudio.com/Universal%20Store/_build?definitionId=56711)|41m to 61m|[Main regions release pipeline](https://microsoft.visualstudio.com/Universal%20Store/_release?definitionId=10488&view=mine&_a=releases)|
|Pull Request|[SC.CSPayments.PX-Windows-PullRequest-master](https://microsoft.visualstudio.com/Universal%20Store/_build?definitionId=56712)|46m to [81m]([SC.CSPayments.PX-Windows-PullRequest-engineering](https://microsoft.visualstudio.com/Universal%20Store/_build?definitionId=56438))|N/A|
|Pull Request|[SC.CSPayments.PX-Windows-PullRequest-engineering](https://microsoft.visualstudio.com/Universal%20Store/_build?definitionId=56438)|35m to 48m|N/A|
### OneBranch Build Pipeline Details
|Type|Name|Duration|Corresponding Release Pipelines|
|--|--|--|--|
|Official|[SC.CSPayments.PX-OneBranch-master-Official](https://microsoft.visualstudio.com/Universal%20Store/_build?definitionId=72510)|20m to 26m|[Main regions release pipeline](https://microsoft.visualstudio.com/Universal%20Store/_release?definitionId=15255&view=mine&_a=releases); [Engineering region release pipeline](https://microsoft.visualstudio.com/Universal%20Store/_release?definitionId=14891&view=mine&_a=releases); [Feature region release pipeline](https://microsoft.visualstudio.com/Universal%20Store/_release?definitionId=15256&view=mine&_a=releases)|
|Pull Request|[SC.CSPayments.PX-PullRequest](https://microsoft.visualstudio.com/Universal%20Store/_build?definitionId=72504)|19m to 25m|N/A|
