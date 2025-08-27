// <copyright file="TokenGenerationLogger.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.Authentication
{
    using System;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Tracing;

    public class TokenGenerationLogger : Payments.Authentication.Tracing.IAuthenticationLogger
    {
        public void LogAzureActiveDirectoryTokenExpiring(string message, string traceId)
        {
            SllWebLogger.TracePXServiceException(
                string.Format("Warning in token generation - token expiring, message: {0}", message), new EventTraceActivity(new Guid(traceId)));
        }

        public void LogAzureActiveDirectoryTokenLoaderException(string message, Exception exception, string traceId)
        {
            SllWebLogger.TracePXServiceException(
                string.Format("Warning in token generation - token loader exception, message: {0}, exception: {1}", message, exception?.ToString() ?? string.Empty), new EventTraceActivity(new Guid(traceId)));
        }

        public void LogAzureActiveDirectoryTokenLoaderFetchedToken(string resource, string clientId, bool autoRefresh, double autoRefreshInMin, DateTime curTimeUTC, string tokenFrom, DateTimeOffset expiresOn, string traceId)
        {
            SllWebLogger.TraceServerMessage(
                                "TokenGeneration",
                                traceId,
                                null,
                                $"Fetched token using resource: {resource} from: {tokenFrom} using clientId: {clientId} at {curTimeUTC}. Token expires at {expiresOn}.",
                                Diagnostics.Tracing.EventLevel.Informational);
        }

        public void LogExpiredAzureActiveDirectoryTokenReturned(string message, string traceId)
        {
            SllWebLogger.TraceServerMessage(
                                "TokenGeneration",
                                traceId,
                                null,
                                message,
                                Diagnostics.Tracing.EventLevel.Informational);
        }

        public void LogManagedIndentityTokenClientGetAadTokenFailed(string resource, string clientId, int latency, Exception exception, string traceId)
        {
            SllWebLogger.TraceTokenGenerationResult(false, resource, clientId, latency, traceId, exception);
        }

        public void LogManagedIndentityTokenClientGetAadTokenSuccess(string resource, string clientId, DateTimeOffset expiresOn, int latency, string traceId)
        {
            SllWebLogger.TraceTokenGenerationResult(true, resource, clientId, latency, traceId, null, expiresOn.ToString());
        }

        public void LogMSALAppTokenClientGetAadTokenFailed(string resource, string clientId, int latency, Exception exception, string traceId)
        {
            SllWebLogger.TraceTokenGenerationResult(false, resource, clientId, latency, traceId, exception);
        }

        public void LogMSALAppTokenClientGetAadTokenSuccess(string resource, string clientId, DateTimeOffset expiresOn, int latency, string traceId)
        {
            SllWebLogger.TraceTokenGenerationResult(true, resource, clientId, latency, traceId, null, expiresOn.ToString());
        }
    }
}