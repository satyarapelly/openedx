// <copyright file="AuthenticationLogger.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.Authentication
{
    using System;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Tracing;

    public class AuthenticationLogger : IAuthenticationLogger
    {
        void IAuthenticationLogger.LogMiseTokenValidationResult(
            MiseTokenValidationResult result,
            long latency,
            Exception exception,
            string incomingRequestId)
        {
            SllWebLogger.TraceMISETokenValidationResult(
                result.Success,
                result.ApplicationId,
                result.ErrorCode,
                result.CloudInstance,
                result.Message,
                latency,
                incomingRequestId,
                exception);
        }

        void IAuthenticationLogger.LogWarning(string message, Exception exception, string incomingRequestId)
        {
            // Todo: fix the broken between incomingRequestId and exception table
            SllWebLogger.TracePXServiceException(
                string.Format("Warning in authentication, message: {0}, exception: {1}", message, exception?.ToString() ?? string.Empty), new EventTraceActivity());
        }
    }
}