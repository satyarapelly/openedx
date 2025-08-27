// <copyright file="IAnomalyDetectionAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using Tracing;

    public interface IAnomalyDetectionAccessor
    {
        /// <summary>
        /// Check whether given accountId is malicious or not
        /// </summary>
        /// <param name="accountId">account Id</param>
        /// <param name="traceActivityId">request trace id</param>
        /// <returns>true if given accounId</returns>
        bool IsMaliciousAccountId(string accountId, EventTraceActivity traceActivityId);

        /// <summary>
        /// Check whether given clientIP is malicious or not
        /// </summary>
        /// <param name="clientIP">client IP</param>
        /// <param name="traceActivityId">request trace id</param>
        /// <returns>true if given clientIP is malicious</returns>
        bool IsMaliciousClientIP(string clientIP, EventTraceActivity traceActivityId);

        /// <summary>
        /// Initialize Anomaly detection results. Used as test hook
        /// </summary>
        /// <param name="accountIdBlobContent">accountId blob content</param>
        /// <param name="clientIPBlobContent">clientIP blob content</param>
        /// <returns>true if successfully initialized</returns>
        bool InitializeAnomalyDetectionResults(byte[] accountIdBlobContent, byte[] clientIPBlobContent);
    }
}
