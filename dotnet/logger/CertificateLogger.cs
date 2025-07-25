namespace Microsoft.Commerce.Payments.PXCommon
{
    using System;
    using Microsoft.Commerce.Tracing;

    /// <summary>
    /// Logs certificate operations using <see cref="SllWebLogger"/>.
    /// </summary>
    public class CertificateLogger : IAuthenticationLogger
    {
        public void LogMiseTokenValidationResult(
            MiseTokenValidationResult result,
            long latency,
            Exception? exception,
            string incomingRequestId)
        {
            string message = $"Certificate operation {(result.Success ? "succeeded" : "failed")} in {latency}ms";
            if (exception != null)
            {
                message += $": {exception}";
            }

            SllWebLogger.TracePXServiceException(message, new EventTraceActivity(incomingRequestId));
        }
    }
}
