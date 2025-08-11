// <copyright file="IRequestTrackingCounter.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Monitoring
{
    /// <summary>
    /// Interface for a Performance Counter that tracks a request in the system
    /// </summary>
    public interface IRequestTrackingCounter
    {
        /// <summary>
        /// Records Total Requests
        /// </summary>
        void RecordTotal();

        /// <summary>
        /// Records Successful Requests
        /// </summary>
        void RecordSuccess();

        /// <summary>
        /// Records Failed Requests
        /// </summary>
        void RecordFailure();

        /// <summary>
        /// Records Latency of request
        /// </summary>
        /// <param name="milliseconds">elapsed time in milliseconds for this request</param>
        void RecordLatency(ulong milliseconds);

        /// <summary>
        /// Records Total Requests for a specific instance
        /// </summary>
        /// <param name="instanceName">instance name</param>
        void RecordTotal(string instanceName);

        /// <summary>
        /// Records Successful Requests for a specific instance
        /// </summary>
        /// <param name="instanceName">instance name</param>
        void RecordSuccess(string instanceName);

        /// <summary>
        /// Records Failed Requests for a specific instance
        /// </summary>
        /// <param name="instanceName">instance name</param>
        void RecordFailure(string instanceName);

        /// <summary>
        /// Records Latency of request for a specific instance
        /// </summary>
        /// <param name="milliseconds">elapsed time in milliseconds for this request</param>
        /// <param name="instanceName">instance name</param>
        void RecordLatency(ulong milliseconds, string instanceName);
    }
}
