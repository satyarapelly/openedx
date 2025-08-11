// <copyright file="DetailedTraceHandler.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Web
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Tracing;
    using Tracing;

    public class DetailedTraceHandler : BasicTraceHandler
    {
        public DetailedTraceHandler(Func<HttpRequestMessage, HttpResponseMessage, string, long, string, EventTraceActivity, EventTraceActivity, Task> apiDetailTrace)
            : base(DetailedRequestStarted, DetailedRequestCompleted, apiDetailTrace)
        {
        }

        public DetailedTraceHandler(
            Func<HttpRequestMessage, HttpResponseMessage, string, long, string, EventTraceActivity, EventTraceActivity, Task> apiDetailTrace,
            HttpMessageHandler innerHandler)
            : base(DetailedRequestStarted, DetailedRequestCompleted, apiDetailTrace, innerHandler)
        {
        }

        private static void DetailedRequestStarted(HttpRequestMessage request, string payload, string operationName, EventTraceActivity requestTraceId, EventTraceActivity serverTraceId)
        {
            PaymentsEventSource.Log.RequestStarted(TraceBuilderHelper.BuildTraceMessage(request, payload), request.Method.Method, request.RequestUri.PathAndQuery, operationName, requestTraceId, serverTraceId);
        }

        private static void DetailedRequestCompleted(HttpResponseMessage response, string payload, string operationName, long latency, EventTraceActivity requestTraceId, EventTraceActivity serverTraceId)
        {
            PaymentsEventSource.Log.RequestCompleted(TraceBuilderHelper.BuildTraceMessage(response, payload), (int)response.StatusCode, operationName, latency, requestTraceId, serverTraceId);
        }
    }
}
