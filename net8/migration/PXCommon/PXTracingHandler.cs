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
    using Environments = Microsoft.Commerce.Payments.Common.Environments;

    public class PXTracingHandler : DelegatingHandler
    {
        private static readonly HttpRequestOptionsKey<EventTraceActivity> TraceActivityKey = new("PXTraceActivity");

        private readonly Action<string, string, EventTraceActivity> logRequest;
        private readonly Action<string, EventTraceActivity> logResponse;
        private readonly Action<string, EventTraceActivity> logError;

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
            this.logError = logError ?? ((m, t) => { });
            this.logRequest = logRequest ?? ((a, m, t) => { });
            this.logResponse = logResponse ?? ((m, t) => { });
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
            this.logError = logError ?? ((m, t) => { });
            this.logRequest = logRequest ?? ((a, m, t) => { });
            this.logResponse = logResponse ?? ((m, t) => { });
        }

        public string ServiceName { get; }

        protected sealed override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return this.InvokeAsync(request, cancellationToken);
        }

        protected virtual async Task<HttpResponseMessage> InvokeAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!request.Options.TryGetValue(TraceActivityKey, out var serverTraceActivity))
            {
                serverTraceActivity = request.GetRequestCorrelationId();
                request.Options.Set(TraceActivityKey, serverTraceActivity);
            }

            using (new TraceCorrelationScope(serverTraceActivity))
            {
                await this.PreSendOperation(request, serverTraceActivity).ConfigureAwait(false);
                HttpResponseMessage response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
                await this.PostSendOperation(request, response, serverTraceActivity).ConfigureAwait(false);

                // Ensure activity on logical call context is as expected.
                var serverTraceActivity2 = EventTraceActivity.Current;
                Debug.Assert(
                    serverTraceActivity.ActivityId.Equals(serverTraceActivity2.ActivityId),
                    string.Format(CultureInfo.InvariantCulture, "EventTraceActivity mismatch. Expected: '{0}', Found: '{1}'", serverTraceActivity.ActivityId, serverTraceActivity2.ActivityId));

                return response;
            }
        }

        private async Task PreSendOperation(HttpRequestMessage request, EventTraceActivity traceId)
        {
            try
            {
                string requestPayload = await request.GetRequestPayload().ConfigureAwait(false);
                this.logRequest(request.GetOperationName(), string.Format(CultureInfo.InvariantCulture, "Url:{0}, Payload:{1}", request.RequestUri.ToString(), requestPayload), traceId);
            }
            catch (Exception ex)
            {
                this.logError(string.Format("Exception happened in presendaction. {0}", ex.Message), traceId);
            }
        }

        private async Task PostSendOperation(HttpRequestMessage request, HttpResponseMessage response, EventTraceActivity traceId)
        {
            try
            {
                StringBuilder sb = new StringBuilder(1000);
                sb.AppendLine("Response details:");
                sb.AppendLine("Status Code: " + response.StatusCode);

                var content = response.Content;
                if (content != null && content.Headers != null)
                {
                    foreach (var header in content.Headers)
                    {
                        sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}: {1}", header.Key, header.GetSanitizeValueForLogging()));
                    }

                    if (content.Headers.ContentLength != 0)
                    {
                        try
                        {
                            sb.AppendLine(await content.ReadAsStringAsync().ConfigureAwait(false));
                        }
                        catch (ObjectDisposedException)
                        {
                            this.logError("Response content disposed before it could be read", traceId);
                        }
                    }
                }

                string callerName = request.GetRequestCallerName();
                sb.AppendFormat("Caller: {0}", callerName ?? Constants.ClientNames.Unknown);

                this.logResponse(sb.ToString(), traceId);
            }
            catch (Exception ex)
            {
                this.logError(string.Format("Exception happened in postsendaction. {0}", ex.Message), traceId);
            }
        }
    }
}