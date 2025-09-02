// <copyright file="ServiceClient.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace Marketplace.Web.PayModClient
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using Newtonsoft.Json;
//    using TestContext = Example.TestContext;

    public class ServiceClient
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new SnakeCasingJsonContractResolver()
        };

        public const string HeaderTrackingId = "x-ms-tracking-id";
        public const string HeaderCorrelationId = "x-ms-correlation-id";
        public const string HeaderTest = "x-ms-test";
        public const string HeaderConfig = "x-ms-configs";
        public const string ApiVersion = "api-version";
        public const string AcceptHeader = "Accept";
        public const string AcceptLanguage = "Accept-Language";
        public const string HeaderFlight = "x-ms-flight";

        public ServiceClientSettings Settings;
        private MSALAuthenticationClient authenticationClient;

        public ServiceClient(ServiceClientSettings settings)
        {
            this.Settings = settings;

            if (this.Settings.ClientCertificate != null && this.Settings.AzureADAuth)
            {
                this.authenticationClient = new MSALAuthenticationClient(
                        string.Format(@"https://login.microsoftonline.com/{0}/", this.Settings.AzureADTenantId),
                        this.Settings.AzureADClientId,
                        this.Settings.AzureADResourceUrl,
                        new Lazy<X509Certificate2>(() => this.Settings.ClientCertificate, true));
            }
            else if (this.Settings.AzureADAuth && !string.IsNullOrEmpty(this.Settings.AzureADSecret))
            {
                this.authenticationClient = new MSALAuthenticationClient(
                        string.Format(@"https://login.microsoftonline.com/{0}/", this.Settings.AzureADTenantId),
                        this.Settings.AzureADClientId,
                        this.Settings.AzureADResourceUrl,
                        this.Settings.AzureADSecret);
            }
        }

        public static HttpStatusCode SendHttpRequest(HttpWebRequest request, out string content)
        {
            string contentType;
            WebHeaderCollection headers;
            return SendHttpRequest(request, out content, out contentType, out headers);
        }

        public static HttpStatusCode SendHttpRequest(HttpWebRequest request, out string content, out string responseContentType, out WebHeaderCollection headers)
        {
            content = null;
            headers = null;

            Trace.TraceInformation("Request details:");
            Trace.TraceInformation("  Method: {0}, Url: {1}", request.Method, request.RequestUri);

            foreach (string headerName in request.Headers.Keys)
            {
                Trace.TraceInformation("Request Header: {0}:{1}", headerName, request.Headers[headerName]);
            }

            HttpWebResponse httpResponse = null;
            try
            {
                try
                {
                    httpResponse = (HttpWebResponse)request.GetResponse();
                }
                catch (WebException responseException)
                {
                    Trace.TraceInformation("Response exception: {0}", responseException);

                    if (responseException.Response == null)
                    {
                        throw;
                    }
                    else
                    {
                        httpResponse = (HttpWebResponse)responseException.Response;
                    }
                }

                Trace.TraceInformation("Response details:");
                Trace.TraceInformation("  Status Code: {0}", httpResponse.StatusCode);
                Trace.TraceInformation("  Content Type: {0}", httpResponse.ContentType);
                Trace.TraceInformation("  Content Length: {0}", httpResponse.ContentLength);
                if (httpResponse.Headers != null)
                {
                    string enabledFlight = httpResponse.GetResponseHeader("x-ms-enabled-flights");
                    Trace.TraceInformation("  EnabledFlights: {0}", enabledFlight);
                }

                responseContentType = httpResponse.ContentType;

                if (httpResponse.ContentLength != 0)
                {
                    string contentString;
                    using (Stream responseStream = httpResponse.GetResponseStream())
                    {
                        contentString = new StreamReader(responseStream).ReadToEnd();
                    }

                    Trace.TraceInformation("  Content: {0}", contentString);
                    content = contentString;
                }

                headers = httpResponse.Headers;
                return httpResponse.StatusCode;
            }
            finally
            {
                if (httpResponse != null)
                {
                    httpResponse.Dispose();
                    httpResponse = null;
                }
            }
        }

        public HttpStatusCode SendRequest(string url, string method, TestContext context, dynamic[] requestInput, out dynamic responseOutput, List<ConfigurationContext> configs = null)
        {
            return this.SendRequest<dynamic>(url, method, context, requestInput, out responseOutput, configs);
        }

        public HttpStatusCode SendRequest(string url, string method, TestContext context, dynamic requestInput, out dynamic responseOutput, List<ConfigurationContext> configs = null)
        {
            return this.SendRequest<dynamic>(url, method, context, requestInput, out responseOutput, configs);
        }

        public HttpStatusCode SendRequest<T>(string url, string method, TestContext context, object requestInput, out T responseOutput, List<ConfigurationContext> configs = null) where T : class
        {
            return SendRequest<T>(url, method, context, requestInput, null, out responseOutput, configs);
        }

        public HttpStatusCode SendRequestWithTrackingId<T>(string url, string method, TestContext context, Guid trackingId, object requestInput, out T responseOutput, List<ConfigurationContext> configs = null) where T : class
        {
            return SendRequest<T>(url, method, context, requestInput, trackingId, out responseOutput, configs);
        }

        public HttpStatusCode SendRequest<T>(string url, string method, TestContext context, object requestInput, Guid? trackingId, out T responseOutput, List<ConfigurationContext> configs = null) where T : class
        {
            Dictionary<string, string> header = new Dictionary<string, string>();
            return this.SendRequest<T>(url, method, context, requestInput, trackingId, header, out responseOutput, configs);
        }

        public HttpStatusCode SendRequest<T>(string url, string method, TestContext context, object requestInput, Guid? trackingId, out T responseOutput, out WebHeaderCollection responseHeaders, List<ConfigurationContext> configs = null) where T : class
        {
            string contentType;
            string requestContent = null;
            if (requestInput != null)
            {
                contentType = this.GetContentType();
                requestContent = this.SerializeObject(requestInput, contentType);
            }

            return this.SendRequest<T>(url, method, context, requestContent, trackingId, out responseOutput, out responseHeaders, configs);
        }

        public HttpStatusCode SendRequest<T>(string url, string method, TestContext context, object requestInput, Guid? trackingId, Dictionary<string, string> header, out T responseOutput, List<ConfigurationContext> configs = null) where T : class
        {
            string contentType;
            string requestContent = null;
            if (requestInput != null)
            {
                contentType = this.GetContentType();
                requestContent = this.SerializeObject(requestInput, contentType);
            }

            if (!string.IsNullOrEmpty(this.Settings.ServiceApiVersion))
            {
                header[ApiVersion] = this.Settings.ServiceApiVersion;
            }

            return this.SendRequest<T>(url, method, context, requestContent, trackingId, header, out responseOutput, configs);
        }

        public HttpStatusCode SendRequest<T>(string url, string method, TestContext context, string requestContent, Guid? trackingId, Dictionary<string, string> requestHeaders, out T responseOutput, List<ConfigurationContext> configs = null) where T : class
        {
            WebHeaderCollection responseHeader;
            return this.SendRequest<T>(url, method, context, requestContent, trackingId, requestHeaders, out responseOutput, out responseHeader, configs);
        }

        public HttpStatusCode SendRequest<T>(string url, string method, TestContext context, string requestContent, Guid? trackingId, Dictionary<string, string> requestHeaders, out T responseOutput, out WebHeaderCollection responseHeader, List<ConfigurationContext> configs = null) where T : class
        {
            responseOutput = null;
            responseHeader = null;

            string responseContent;
            HttpStatusCode responseCode = HttpStatusCode.NotImplemented;
            HttpWebRequest request = this.CreateRequest(url);
            request.Method = method;
            if (trackingId.HasValue)
            {
                request.Headers.Add(HeaderTrackingId, trackingId.Value.ToString());
            }
            else
            {
                trackingId = Guid.NewGuid();
                request.Headers.Add(HeaderTrackingId, trackingId.Value.ToString());
            }

            //request.Headers.Add(HeaderCorrelationId, traceActivityId.ActivityId.ToString());

            if (requestHeaders != null)
            {
                foreach (string header in requestHeaders.Keys)
                {
                    request.Headers.Add(header, requestHeaders[header]);
                }
            }

            request.Accept = this.GetAcceptHeader();

            if (context != null)
            {
                request.Headers.Add(HeaderTest, JsonConvert.SerializeObject(context));
            }

            if (configs != null)
            {
                request.Headers.Add(HeaderConfig, JsonConvert.SerializeObject(configs));
            }

//            request.Headers.Add(PaymentConstants.HttpHeaders.CV, new EventTraceActivity().GetCorrelationVectorValue());

            string contentType;
            if (!string.IsNullOrEmpty(requestContent))
            {
                contentType = this.GetContentType();
                Trace.TraceInformation("Request content: {0}", requestContent);
                byte[] data = Encoding.UTF8.GetBytes(requestContent);
                request.ContentLength = data.Length;
                request.ContentType = contentType;
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(data, 0, data.Length);
                    requestStream.Flush();
                }
            }
            else
            {
                request.ContentLength = 0;
            }

            responseCode = SendHttpRequest(request, out responseContent, out contentType, out responseHeader);
            if (responseCode == HttpStatusCode.Created
                || responseCode == HttpStatusCode.OK
                || responseCode == HttpStatusCode.Accepted
                || responseCode == HttpStatusCode.BadGateway
                || responseCode == HttpStatusCode.GatewayTimeout
                || responseCode == HttpStatusCode.Conflict)
            {
                responseOutput = this.Deserialize<T>(responseContent, contentType);
            }
            else if (responseCode == HttpStatusCode.InternalServerError || responseCode == HttpStatusCode.BadRequest || responseCode == HttpStatusCode.PaymentRequired)
            {
                Trace.TraceWarning(string.Format("Error response with httpStatusCode '{0}': {1}", responseCode, responseContent));
                responseOutput = responseContent as T;
            }

            return responseCode;
        }

        public HttpStatusCode SendRequest<T>(string url, string method, TestContext context, string requestContent, Guid? trackingId, out T responseOutput, out WebHeaderCollection responseHeaders, List<ConfigurationContext> configs = null) where T : class
        {
            responseOutput = null;
            string responseContent;
            HttpStatusCode responseCode = HttpStatusCode.NotImplemented;
            HttpWebRequest request = this.CreateRequest(url);
            request.Method = method;
            if (trackingId.HasValue)
            {
                request.Headers.Add(HeaderTrackingId, trackingId.Value.ToString());
            }
            else
            {
                request.Headers.Add(HeaderTrackingId, Guid.NewGuid().ToString());
            }

            //request.Headers.Add(HeaderCorrelationId, traceActivityId.ActivityId.ToString());
            if (!string.IsNullOrEmpty(this.Settings.ServiceApiVersion))
            {
                request.Headers.Add(ApiVersion, this.Settings.ServiceApiVersion);
            }

            request.Accept = this.GetAcceptHeader();

            if (context != null)
            {
                request.Headers.Add(HeaderTest, JsonConvert.SerializeObject(context));
            }

            if (configs != null)
            {
                request.Headers.Add(HeaderConfig, JsonConvert.SerializeObject(configs));
            }

            string contentType;
            if (!string.IsNullOrEmpty(requestContent))
            {
                contentType = this.GetContentType();
                Trace.TraceInformation("Request content: {0}", requestContent);
                byte[] data = Encoding.UTF8.GetBytes(requestContent);
                request.ContentLength = data.Length;
                request.ContentType = contentType;
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(data, 0, data.Length);
                    requestStream.Flush();
                }
            }
            else
            {
                request.ContentLength = 0;
            }

            responseCode = SendHttpRequest(request, out responseContent, out contentType, out responseHeaders);
            if (responseCode == HttpStatusCode.Created || responseCode == HttpStatusCode.OK || responseCode == HttpStatusCode.Accepted || responseCode == HttpStatusCode.BadGateway || responseCode == HttpStatusCode.GatewayTimeout)
            {
                responseOutput = this.Deserialize<T>(responseContent, contentType);
            }
            else if (responseCode == HttpStatusCode.InternalServerError || responseCode == HttpStatusCode.PaymentRequired)
            {
                Trace.TraceWarning("Internal Server error encountered " + responseContent);
                responseOutput = responseContent as T;
            }

            return responseCode;
        }

        protected virtual string SerializeObject(object requestInput, string contentType)
        {
            if (contentType.ToLower().Contains("application/json"))
            {
                return JsonConvert.SerializeObject(requestInput, SerializerSettings);
            }
//            else if (contentType.ToLower().Contains("application/xml") || contentType.ToLower().Contains("text/xml"))
  //          {
                /*
                using (Stream stream = new MemoryStream())
                {
                    using (StreamContent content = new StreamContent(stream))
                    {
                        new XmlMediaTypeFormatter() { UseXmlSerializer = true }.WriteToStreamAsync(requestInput.GetType(), requestInput, stream, content, null).Wait();
                        stream.Position = 0;
                        return content.ReadAsStringAsync().Result;
                    }
                }
                */
    //        }
            else
            {
                throw new NotSupportedException(string.Format("{0} is not a supported content type", contentType));
            }
        }

        protected virtual T Deserialize<T>(string value, string contentType)
        {
            if (contentType.ToLower().Contains("application/json"))
            {
                return JsonConvert.DeserializeObject<T>(value, SerializerSettings);
            }
            else if (string.IsNullOrEmpty(value))
            {
                // The response content is empty when successfully remove PI. Add this to avoid hitting the following "else".
                return default(T);
            }
            else
            {
                throw new NotSupportedException(string.Format("{0} is not a supported content type", contentType));
            }
        }

        protected virtual string GetContentType()
        {
            return "application/json";
        }

        protected virtual string GetAcceptHeader()
        {
            return "application/json";
        }

        public HttpWebRequest CreateRequest(string pathAndQuery)
        {
            if (!string.IsNullOrEmpty(pathAndQuery) && pathAndQuery.StartsWith("/"))
            {
                pathAndQuery = pathAndQuery.Substring(1);
            }

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri(this.Settings.ServiceEndpoint + pathAndQuery));

            string aadToken = null;

            if (this.Settings.AzureADAuth)
            {
                aadToken = this.authenticationClient.GetDefaultTokenAsync(true).Result;
                request.Headers.Add("Authorization", aadToken);
            }
            else if (this.Settings.ClientCertificate != null)
            {
                request.ClientCertificates.Add(this.Settings.ClientCertificate);
            }

            if (aadToken != null)
            {
                Trace.TraceInformation("Create request with aad token from appId {0}", this.Settings.AzureADClientId);
            }
            else
            {
                Trace.TraceInformation("Create request with client certificate: {0}", this.Settings.ClientCertificate == null ? "Null" : this.Settings.ClientCertificate.Subject);
            }

            request.Credentials = CredentialCache.DefaultCredentials;

            return request;
        }
    }
}
