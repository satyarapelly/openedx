namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators
{
    using Common.Transaction;
    using System.Net.Http;

    public interface IScenarioManager
    {
        HttpResponseMessage GetMockResponse(string apiName);
        HttpResponseMessage GetMockResponse(string apiName, TestContext testContext);
    }
}
