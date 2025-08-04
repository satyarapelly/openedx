namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System;
    using System.Net;
    using System.Net.Http;

    public class HttpResponseException : Exception
    {
        public HttpResponseMessage Response { get; }

        public HttpResponseException(HttpResponseMessage response)
        {
            this.Response = response;
        }

        public HttpResponseException(HttpStatusCode statusCode)
        {
            this.Response = new HttpResponseMessage(statusCode);
        }
    }
}
