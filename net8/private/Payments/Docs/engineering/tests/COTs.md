# COTs

## Target Audience
PX Engineering

## Overview
COT tests are really unit tests, sometimes at a unit level, but for PX service we test at API level. We leverage [System.Web.Http.SelfHost.HttpSelfHostServer](https://docs.microsoft.com/en-us/previous-versions/aspnet/hh835627(v=vs.108)) to host the entire PX API in memory, this allows to test specific scenarios in a larger spectrum. In this document, we will focus on PX Service.

## Instructions
1. Setup services.
    - Refer to [devbox setup instructions](../../development/devbox-setup.md)
2. How to Run COTs: 
    - Pick test setting: In VS2022 Test->Configure Run Settings->Select Solution Wide runsettings File -> .\SC.CSPayments.PX\private\Payments\Tests\COT.PXService\RunSettings\PXService.OneBox.runsettings
    - Run test cases in COT.PXService 