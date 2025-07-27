using System;
namespace System.Web
{
    public class HttpRequest
    {
        public HttpRequest(Uri url)
        {
            Url = url;
        }

        public Uri Url { get; set; }
    }

    public class HttpContext
    {
        public static HttpContext? Current { get; set; }

        public HttpRequest Request { get; }

        public HttpContext(HttpRequest request)
        {
            Request = request;
        }
    }
}
