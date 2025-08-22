// <copyright file="PXTracingHandler.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXCommon
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Tracing;
    using Environments = Microsoft.Commerce.Payments.Common.Environments;

    public class PXTracingHandler : DelegatingHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PXTracingHandler" /> class.
        /// </summary>
        /// <param name="serviceName">The name of the service or client using this handler. This name is used for identification of the trace logs.</param>
        /// <param name="logError">A tracing action that will be executed if there is an error when sending or receiving a request.</param>
        /// <param name="logRequest">A tracing action that will be executed to log the request that's being sent. Default is no action. Do not use if the request could contain sensitive data.</param>
        /// <param name="logResponse">A tracing action that will be executed to log the response that's received. Default is no action. Do not use if the response could contain sensitive data.</param>
        public PXTracingHandler(
            string serviceName,
            Action<string, EventTraceActivity> logError = null,
            Action<string, string, EventTraceActivity> logRequest = null,
            Action<string, EventTraceActivity> logResponse = null)
            : base()
        {
            this.ServiceName = serviceName;
            this.LogError = logError ?? ((m, t) => PaymentsEventSource.Log.TracingHandlerTraceError(m, t));
            this.LogRequest = logRequest ?? ((a, m, t) => { });
            this.LogResponse = logResponse ?? ((m, t) => { });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PXTracingHandler" /> class.
        /// </summary>
        /// <param name="serviceName">The name of the service or client using this handler. This name is used for identification of the trace logs.</param>
        /// <param name="httpMessageHandler">An inner handler that is attached to this DelegatingHandler object.</param>
        /// <param name="logError">A tracing action that will be executed if there is an error when sending or receiving a request.</param>
        /// <param name="logRequest">A tracing action that will be executed to log the request that's being sent. Default is no action. Do not use if the request could contain sensitive data.</param>
        /// <param name="logResponse">A tracing action that will be executed to log the response that's received. Default is no action. Do not use if the response could contain sensitive data.</param>
        public PXTracingHandler(
            string serviceName,
            HttpMessageHandler httpMessageHandler,
            Action<string, EventTraceActivity> logError = null,
            Action<string, string, EventTraceActivity> logRequest = null,
            Action<string, EventTraceActivity> logResponse = null)
            : base(httpMessageHandler)
        {
            this.ServiceName = serviceName;
            this.LogError = logError ?? ((m, t) => PaymentsEventSource.Log.TracingHandlerTraceError(m, t));
            this.LogRequest = logRequest ?? ((a, m, t) => { });
            this.LogResponse = logResponse ?? ((m, t) => { });
        }

        public string ServiceName { get; set; }

        public Action<string, string, EventTraceActivity> LogRequest { get; set; }

        public Action<string, EventTraceActivity> LogResponse { get; set; }

        public Action<string, EventTraceActivity> LogError { get; set; }

        protected override sealed async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            DateTime startTime = DateTime.UtcNow;
            EventTraceActivity serverTraceActivity = request.GetRequestCorrelationId();

            using (new TraceCorrelationScope(serverTraceActivity))
            {
                this.PreSendOperation(request, serverTraceActivity);
                HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
                this.PostSendOperation(request, response, startTime, serverTraceActivity);

                // Ensure activity on logical call context is as expected.
                var serverTraceActivity2 = EventTraceActivity.Current;
                Debug.Assert(
                    serverTraceActivity.ActivityId.Equals(serverTraceActivity2.ActivityId),
                    string.Format(CultureInfo.InvariantCulture, "EventTraceActivity mismatch. Expected: '{0}', Found: '{1}'", serverTraceActivity.ActivityId, serverTraceActivity2.ActivityId));

                return response;
            }
        }

        private void PreSendOperation(HttpRequestMessage request, EventTraceActivity traceId)
        {
            try
            {
                string requestPayload = request.GetRequestPayload().Result;
                this.LogRequest(request.GetOperationName(), string.Format("Url:{0}, Payload:{1}", request.RequestUri.ToString(), requestPayload), traceId);
            }
            catch (Exception ex)
            {
                this.LogError(string.Format("Exception happened in presendaction. {0}", ex.Message), traceId);
            }
        }

        private void PostSendOperation(HttpRequestMessage request, HttpResponseMessage response, DateTime startTime, EventTraceActivity traceId)
        {
            try
            {
                StringBuilder sb = new StringBuilder(1000);
                sb.AppendLine("Response details:");
                sb.AppendLine("Status Code: " + response.StatusCode);

                if (response.Content != null)
                {
                    if (response.Content.Headers != null)
                    {
                        foreach (var header in response.Content.Headers)
                        {
                            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}: {1}", header.Key, header.GetSanitizeValueForLogging()));
                        }

                        if (response.Content.Headers.ContentLength != 0)
                        {
                            sb.AppendLine(response.Content.ReadAsStringAsync().Result);
                        }
                    }
                }

                // get operation name
                string operationName = request.GetOperationNameWithPendingOnInfo();
                string version = request.GetVersion();
                
                if (request.Properties != null)
                {
                    string callerName = request.GetRequestCallerName();
                    sb.AppendFormat("Caller: {0}", callerName ?? Constants.ClientNames.Unknown);
                }

                this.LogResponse(sb.ToString(), traceId);
            }
            catch (Exception ex)
            {
                this.LogError(string.Format("Exception happened in postsendaction. {0}", ex.Message), traceId);
            }
        }
    }
}
