// <copyright file="BasicTraceHandler.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Web
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Tracing;

    public class BasicTraceHandler : DelegatingHandler
    {
        public BasicTraceHandler(
            Action<HttpRequestMessage, string, string, EventTraceActivity, EventTraceActivity> traceRequest,
            Action<HttpResponseMessage, string, string, long, EventTraceActivity, EventTraceActivity> traceResponse,
            Func<HttpRequestMessage, HttpResponseMessage, string, long, string, EventTraceActivity, EventTraceActivity, Task> apiDetailTrace)
        {
            this.TraceRequest = traceRequest;
            this.TraceResponse = traceResponse;
            this.ApiDetailTrace = apiDetailTrace;
        }

        public BasicTraceHandler(
            Action<HttpRequestMessage, string, string, EventTraceActivity, EventTraceActivity> traceRequest,
            Action<HttpResponseMessage, string, string, long, EventTraceActivity, EventTraceActivity> traceResponse,
            Func<HttpRequestMessage, HttpResponseMessage, string, long, string, EventTraceActivity, EventTraceActivity, Task> apiDetailTrace,
            HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
            this.TraceRequest = traceRequest;
            this.TraceResponse = traceResponse;
            this.ApiDetailTrace = apiDetailTrace;
        }

        public BasicTraceHandler()
        {
        }

        public BasicTraceHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        public Action<HttpRequestMessage, string, string, EventTraceActivity, EventTraceActivity> TraceRequest
        {
            get;
            private set;
        }

        public Action<HttpResponseMessage, string, string, long, EventTraceActivity, EventTraceActivity> TraceResponse
        {
            get;
            private set;
        }

        public Func<HttpRequestMessage, HttpResponseMessage, string, long, string, EventTraceActivity, EventTraceActivity, Task> ApiDetailTrace
        {
            get;
            private set;
        }

        protected override sealed async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            EventTraceActivity serverTraceId = null;
            object result;
            if (request.Properties.TryGetValue(PaymentConstants.Web.Properties.ServerTraceId, out result))
            {
                serverTraceId = (EventTraceActivity)result;
            }
            else
            {
                serverTraceId = request.GetRequestCorrelationId();
            }

            EventTraceActivity requestTraceId = null;
            if (request.Properties.TryGetValue(PaymentConstants.Web.Properties.ClientTraceId, out result))
            {
                requestTraceId = (EventTraceActivity)result;
            }
            else
            {
                // should never be here.
                requestTraceId = new EventTraceActivity(Guid.NewGuid()) { CorrelationVectorV4 = serverTraceId.CorrelationVectorV4 };
            }

            string operationName = string.Format("{0}-{1}", request.Method, request.RequestUri.AbsolutePath.Trim('/'));
            request.Properties.Add(PaymentConstants.Web.Properties.OperationName, operationName);

            string trackingId = request.GetTrackingId();

            // Need set the request content before processing.
            await request.GetRequestPayload();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            if (this.TraceRequest != null)
            {
                this.TraceRequest(request, string.Empty, operationName, requestTraceId, serverTraceId);
            }

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
            response.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId, requestTraceId.ActivityId.ToString());
            stopwatch.Stop();

            if (this.TraceResponse != null)
            {
                this.TraceResponse(response, string.Empty, operationName, stopwatch.ElapsedMilliseconds, requestTraceId, serverTraceId);
            }

            if (this.ApiDetailTrace != null)
            {
                Stopwatch traceOperationStopwatch = new Stopwatch();
                await this.ApiDetailTrace(request, response, operationName, stopwatch.ElapsedMilliseconds, string.Empty, requestTraceId, serverTraceId);
                traceOperationStopwatch.Stop();
            }

            return response;
        }
    }
}
