namespace Test.Common
{
    using System.Net;
    using System.Net.Http;
    using System.Text.RegularExpressions;

    public class ConditionalResponse
    {
        public HttpMethod Method { get; set; }

        public string UrlPattern { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public string Content { get; set; }

        public bool IsMatch(HttpRequestMessage request)
        {
            if (Method != null && request.Method != Method)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(UrlPattern))
            {
                Regex urlRegex = new Regex(UrlPattern);
                if (!urlRegex.IsMatch(request.RequestUri.ToString()))
                {
                    return false;
                }
            }

            return true;
        }
    }
}