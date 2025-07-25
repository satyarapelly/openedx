namespace Microsoft.Commerce.Payments.PXCommon
{
    using System;

    public interface IAuthenticationLogger
    {
        void LogMiseTokenValidationResult(
            MiseTokenValidationResult result,
            long latency,
            Exception? exception,
            string incomingRequestId);
    }
}
