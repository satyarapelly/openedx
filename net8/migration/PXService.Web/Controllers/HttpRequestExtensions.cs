namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System.Net;
    using System.Net.Http;
    using System.Text;
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
    }
}
