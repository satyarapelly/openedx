namespace SelfHostedPXServiceCore.Mocks
{
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Test.Common;

    public class AzureExPServiceMockResponseProvider : IMockResponseProvider
    {
        public void ResetDefaults()
        {
        }

        public async Task<HttpResponseMessage> GetMatchedMockResponse(HttpRequestMessage httpRequestMessage)
        {
            string responseContent = string.Empty;
            HttpStatusCode statusCode = HttpStatusCode.OK;

            return await Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(
                    responseContent,
                    System.Text.Encoding.UTF8,
                    "application/octet-stream")
            });
        }
    }
}