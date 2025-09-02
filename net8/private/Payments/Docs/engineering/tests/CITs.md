# CITs

## Target Audience
PX Engineering

## Overview
CIT tests are really unit tests, sometimes at a unit level, but for PX service we test at API level. We leverage [System.Web.Http.SelfHost.HttpSelfHostServer](https://docs.microsoft.com/en-us/previous-versions/aspnet/hh835627(v=vs.108)) to host the entire PX API in memory, this allows to test specific scenarios in a larger spectrum. In this document, we will focus on PX Service.

## Architecture
The selfhost ability is encapsulated generically in the [HostableService](https://microsoft.visualstudio.com/DefaultCollection/Universal%20Store/_git/SC.CSPayments.PX?path=/private/Payments/Tests/SelfHostedPXServiceCore/Mocks/HostableService.cs) class, this allows that any emulator can be selfhosted with minimal code changes. The [SelfHostedPxService](https://microsoft.visualstudio.com/DefaultCollection/Universal%20Store/_git/SC.CSPayments.PX?path=/private/Payments/Tests/SelfHostedPXServiceCore/SelfHostedPxService.cs) class inherits from HostableService, however, it handles setup logic specific for PX Service, such as selfhosting all its emulator dependencies.

![](../../images/HostableService.PNG)
classDiagram
    HostableService <|-- SelfHostedPxService
    HostableService : +List<int> PreRegisteredPorts
    HostableService : +string Port
    HostableService : +Uri BaseUri
    HostableService : +HttpSelfHostConfiguration SelfHostConfiguration
    HostableService : +HttpSelfHostServer SelfHostServer
    HostableService: +Dispose()
    HostableService: +GetAvailablePort()
    class SelfHostedPxService {
      +HostableService PxHostableService
      +Dictionary<Type, HostableService> SelfHostedDependencies
      +PXServiceSettings PXSettings
      +PXServiceHandler PXHandler
      +PXServiceCorsHandler PXCorsHandler
      +PXServiceFlightHandler PXFlightHandler
      +ResetDependencies()
      +GetPXServiceUrl()
      +GetRequest()
    }
    
We selfhost PX service when running the PX Service CITs and when running the DiffTest against selfhost. This requires different mock handlers for each case. The handler logic is encapsulated in [MockServiceHandler](https://microsoft.visualstudio.com/DefaultCollection/Universal%20Store/_git/SC.CSPayments.PX?path=/private/Payments/Tests/Test.Common/MockServiceHandler.cs), but explicit implementations are [MockServiceWebRequestHandler](https://microsoft.visualstudio.com/DefaultCollection/Universal%20Store/_git/SC.CSPayments.PX?path=/private/Payments/Tests/Test.Common/MockServiceWebRequestHandler.cs) and [MockServiceDelegatingHandler](https://microsoft.visualstudio.com/DefaultCollection/Universal%20Store/_git/SC.CSPayments.PX?path=/private/Payments/Tests/Test.Common/MockServiceDelegatingHandler.cs).

- MockServiceWebRequestHandlers have explicit implementation used when running as SelfHoste and gets used in CITs and diff tests.
- MockServiceDelegatingHandlers are used when the service is running in IIS and gets used in COTs.
- IMockResponseProvider interface to abstract logic behind mocks.

![](../../images/MockServiceHandler.PNG)
classDiagram
    class MockServiceHandler{
      +List<string> Requests
      +List<ConditionalResponse> Responses
      +Action<HttpRequestMessage> PreProcess
      +Action<HttpResponseMessage> PostProcess
      +ResetToDefaults()
      +ArrangeResponse()
      +SendAsync()
    }
    class MockServiceDelegatingHandler{
      -MockServiceHandler mockServiceHandler
      +List<string> Requests
      +List<ConditionalResponse> Responses
      +Action<HttpRequestMessage> PreProcess
      +Action<HttpResponseMessage> PostProcess
      +ResetToDefaults()
      +ArrangeResponse()
      +SendAsync()
    }
      
![](../../images/MockServiceWebRequestHandler.PNG)
classDiagram
  MockServiceWebRequestHandler <|-- PimsService
  MockServiceWebRequestHandler : -MockServiceHandler mockServiceHandler
  MockServiceWebRequestHandler : +List<string> Requests
  MockServiceWebRequestHandler : +List<ConditionalResponse> Responses
  MockServiceWebRequestHandler : +Action<HttpRequestMessage> PreProcess
  MockServiceWebRequestHandler : +Action<HttpResponseMessage> PostProcess
  MockServiceWebRequestHandler : +ResetToDefaults()
  MockServiceWebRequestHandler : +ArrangeResponse()
  MockServiceWebRequestHandler : +SendAsync()
  class PimsService {
    +PimsMockResponseProvider ResponseProvider
  }

![](../../images/IMockResponseProvider.PNG)
classDiagram
    class IMockResponseProvider{
      +ask<HttpResponseMessage> GetMatchedMockResponse(HttpRequestMessage httpRequestMessage)
      +ResetDefaults()
    }


## Mocking https calls
There are 3 ways to mock http calls, but only 2 prefered ways.
- For CITs
  - Prefer using `ArrangeResponse` on its respective service. Example: `PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));`
  - Leverage `PreProcess` and `PostProcess` to assert.
  - Avoid using `ResponseProviders`, example:
    - `Assert.IsTrue(PXSettings.PimsService.ResponseProvider.IsBillingAccountIdSet == expectedValue);`
    - `var piFromPims = PimsMockResponseProvider.GetPaymentInstrument("Account001", piId);`
- For COTs
  - Prefer using controllers that inherit from EmulatorControler and drive mocks via TestScenarios and flights.
  - TODO: Merge `MockResponseProviders` and `TestScenarios`, pending on team discussion.
  
---
For questions/clarifications, email [author/s of this doc and PX support](mailto:holugo@microsoft.com?cc=PXSupport@microsoft.com&subject=Docs%20-%20engineering/tests/cits.md).

---