namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks
{
    using System.Net.Http;
    using System.Threading.Tasks;

    public interface IMockResponseProvider
    {
        void ResetDefaults();
        Task<HttpResponseMessage?> GetMatchedMockResponse(HttpRequestMessage request);
    }
}
