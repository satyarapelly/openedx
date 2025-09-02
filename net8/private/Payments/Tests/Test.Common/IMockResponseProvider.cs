namespace Test.Common
{
    using System.Net.Http;
    using System.Threading.Tasks;

    public interface IMockResponseProvider
    {
        void ResetDefaults();

        Task<HttpResponseMessage> GetMatchedMockResponse(HttpRequestMessage httpRequestMessage);
    }
}