namespace SelfHostedPXServiceCore.Mocks
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using Test.Common;

    public class OrchestrationServiceMockResponseProvider : IMockResponseProvider
    {
        public void ResetDefaults()
        {
        }

        public Task<HttpResponseMessage> GetMatchedMockResponse(HttpRequestMessage httpRequestMessage)
        {
            return null;
        }
    }
}