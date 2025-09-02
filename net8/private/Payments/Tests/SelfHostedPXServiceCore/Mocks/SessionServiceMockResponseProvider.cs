namespace SelfHostedPXServiceCore.Mocks
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Test.Common;

    public class SessionServiceMockResponseProvider : IMockResponseProvider
    {
        public Dictionary<string, string> SessionStore { get; set; }
        
        public SessionServiceMockResponseProvider()
        {
            SessionStore = new Dictionary<string, string>();
        }

        public void ResetDefaults()
        {
            SessionStore.Clear();
        }

        public async Task<HttpResponseMessage> GetMatchedMockResponse(HttpRequestMessage request)
        {
            // Request urls of session service:
            // POST sessionservice/sessions/{0}
            // PUT sessionservice/sessions/{0}
            // GET sessionservice/sessions/{0}?sessionType={1}
            string responseContent = string.Empty;
            string reqUrl = request.RequestUri.ToString();
            int startIndex = reqUrl.IndexOf("sessions/") + 9;
            int endIndex = reqUrl.IndexOf("?sessionType=");
            int length = endIndex == -1 ? reqUrl.Length - startIndex : endIndex - startIndex;
            string sessionId = reqUrl.Substring(startIndex, length);

            if (request.Method == HttpMethod.Post || request.Method == HttpMethod.Put)
            {
                responseContent = await request.Content.ReadAsStringAsync();
                SessionStore[sessionId] = responseContent;
            }
            else if (request.Method == HttpMethod.Get)
            {
                responseContent = SessionStore[sessionId];
            }

            if (string.IsNullOrEmpty(responseContent))
            {
                return null;
            }

            return await Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    responseContent,
                    System.Text.Encoding.UTF8,
                    "application/json")
            });
        }
    }
}