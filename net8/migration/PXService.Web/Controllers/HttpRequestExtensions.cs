namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;

    internal static class HttpRequestExtensions
    {
        public static HttpResponseMessage CreateResponse(this HttpRequest request, HttpStatusCode statusCode)
        {
            return new HttpResponseMessage(statusCode);
        }

        public static HttpResponseMessage CreateResponse<T>(this HttpRequest request, T value)
        {
            return request.CreateResponse(HttpStatusCode.OK, value);
        }

        public static HttpResponseMessage CreateResponse(this HttpRequest request, HttpStatusCode statusCode, object value, string mediaType = "application/json")
        {
            var response = new HttpResponseMessage(statusCode);
            if (value != null)
            {
                var json = JsonConvert.SerializeObject(value);
                response.Content = new StringContent(json, Encoding.UTF8, mediaType);
            }
            return response;
        }

        public static HttpResponseMessage CreateErrorResponse(this HttpRequest request, HttpStatusCode statusCode, object error)
        {
            return request.CreateResponse(statusCode, error);
        }

        public static IEnumerable<KeyValuePair<string, string>> GetQueryNameValuePairs(this HttpRequest request)
        {
            return request.Query.Select(q => new KeyValuePair<string, string>(q.Key, q.Value));
        }

        public static bool TryGetQueryParameterValue(this HttpRequest request, string parameterName, out string value, string pattern = null)
        {
            if (request.Query.TryGetValue(parameterName, out var values))
            {
                value = values.FirstOrDefault();
                return true;
            }

            value = null;
            return false;
        }

        public static async Task<string> GetRequestPayload(this HttpRequest request)
        {
            if (request.ContentLength == null || request.ContentLength == 0)
            {
                return string.Empty;
            }

            request.EnableBuffering();
            request.Body.Position = 0;
            using var reader = new StreamReader(request.Body, leaveOpen: true);
            var payload = await reader.ReadToEndAsync();
            request.Body.Position = 0;
            return payload;
        }
    }
}
