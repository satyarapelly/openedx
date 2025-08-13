// <copyright file="IAuthenticationLogger.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXCommon
{
    using System;

    public interface IAuthenticationLogger
    {
        /// <summary>
        /// Log the authentication result
        /// </summary>
        /// <param name="result">result to log</param>
        /// <param name="latency">authentication latency </param>
        /// <param name="exception">exception to log</param>
        /// <param name="incomingRequestId">The unique id of incoming request and it can be CV if available</param>
        void LogMiseTokenValidationResult(MiseTokenValidationResult result, long latency, Exception exception, string incomingRequestId);

        /// <summary>
        /// Log authentication warning
        /// </summary>
        /// <param name="message">authentication message</param>
        /// <param name="exception">exception to log</param>
        /// <param name="incomingRequestId">The unique id of incoming request and it can be CV if available</param>
        void LogWarning(string message, Exception exception, string incomingRequestId);
    }
}
