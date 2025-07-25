namespace Microsoft.Commerce.Payments.PXCommon
{
    using System;
    using Microsoft.Commerce.Tracing;

    /// <summary>
    /// Simple implementation of <see cref="IAuthenticationLogger"/> that writes
    /// validation results to <see cref="SllWebLogger"/>.
    /// </summary>
    public class AuthenticationLogger : IAuthenticationLogger
    {
        public void LogMiseTokenValidationResult(
            MiseTokenValidationResult result,
            long latency,
            Exception? exception,
            string incomingRequestId)
        {
            string message = $"Token validation {(result.Success ? "succeeded" : "failed")} in {latency}ms";
            if (exception != null)
            {
                message += $": {exception}";
            }

            SllWebLogger.TracePXServiceException(message, new EventTraceActivity(incomingRequestId));
        }
    }
}
