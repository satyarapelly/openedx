namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Extensions
{
    using Common.Transaction;
    using Microsoft.Commerce.Payments.Common.Testing;
    using System.Net.Http;

    public static class TestScenarioManagerExtensions
    {
        public static HttpResponseMessage GetResponse(this IScenarioManager manager, string apiName)
        {
            return manager.GetMockResponse(apiName);
        }

        public static HttpResponseMessage GetResponse(this IScenarioManager manager, string apiName, TestContext testContext)
        {
            return manager.GetMockResponse(apiName, testContext);
        }
    }
}
