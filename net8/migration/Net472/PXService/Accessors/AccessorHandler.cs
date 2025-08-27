// <copyright file="AccessorHandler.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors
{
    using System.Net;
    using System.Net.Http;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Tracing;

    public class AccessorHandler
    {
        public static void HandleEmptyErrorResponses(HttpResponseMessage response, string actionName, EventTraceActivity traceActivityId, string serviceName)
        {
            string emptyResponseMessage = $"Failed response from {serviceName} while calling {actionName}. reasonPhrase: {response.ReasonPhrase}";
            HttpStatusCode statusCode = response.StatusCode;

            if (statusCode == HttpStatusCode.Unauthorized)
            {
                throw TraceCore.TraceException(traceActivityId, new FailedOperationException("Unauthorized access - 401. " + emptyResponseMessage));
            }
            else if (statusCode == HttpStatusCode.Forbidden)
            {
                throw TraceCore.TraceException(traceActivityId, new FailedOperationException("Forbidden access - 403. " + emptyResponseMessage));
            }
            else if (statusCode == HttpStatusCode.NotFound)
            {
                throw TraceCore.TraceException(traceActivityId, new FailedOperationException("Resource not found - 404. " + emptyResponseMessage));
            }
            else if (statusCode == HttpStatusCode.InternalServerError)
            {
                throw TraceCore.TraceException(traceActivityId, new FailedOperationException("Internal server error - 500. " + emptyResponseMessage));
            }
            else if (statusCode == HttpStatusCode.BadGateway)
            {
                throw TraceCore.TraceException(traceActivityId, new FailedOperationException("Bad gateway - 502. " + emptyResponseMessage));
            }
            else if (statusCode == HttpStatusCode.ServiceUnavailable)
            {
                throw TraceCore.TraceException(traceActivityId, new FailedOperationException("Service unavailable - 503. " + emptyResponseMessage));
            }
        }
    }
}