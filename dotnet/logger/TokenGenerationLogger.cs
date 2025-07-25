namespace Microsoft.Commerce.Payments.PXCommon
{
    using System;
    using Microsoft.Commerce.Tracing;

    /// <summary>
    /// Logs token generation attempts using <see cref="SllWebLogger"/>.
    /// </summary>
    public class TokenGenerationLogger : IAuthenticationLogger
    {
        public void LogMiseTokenValidationResult(
            MiseTokenValidationResult result,
            long latency,
            Exception? exception,
            string incomingRequestId)
        {
            string message = $"Token generation {(result.Success ? "succeeded" : "failed")} in {latency}ms";
            if (exception != null)
            {
                message += $": {exception}";
            }

            SllWebLogger.TracePXServiceException(message, new EventTraceActivity(incomingRequestId));
        }
    }
}
