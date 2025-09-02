// <copyright file="CounterScope.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Monitoring
{
    using System;
    using System.Diagnostics;

    public class CounterScope : IDisposable
    {
        private readonly string counterInstanceName;
        private Action<string> operationSuccess;
        private Action<string> operationFailure;
        private Action<ulong, string> operationLatency;
        private Stopwatch latencyStopWatch;
        private bool completedSuccessfully;

        public CounterScope(
            Action<string> operationStarted,
            Action<string> operationSuccess,
            Action<string> operationFailure,
            Action<ulong, string> operationLatency)
            : this(operationStarted, operationSuccess, operationFailure, operationLatency, string.Empty)
        {
        }

        public CounterScope(
            Action<string> operationStarted,
            Action<string> operationSuccess,
            Action<string> operationFailure,
            Action<ulong, string> operationLatency,
            string counterInstanceName)
        {
            Debug.Assert(operationStarted != null, "The operation started action is required.");
            Debug.Assert(operationSuccess != null, "The operation success action is required.");
            Debug.Assert(operationFailure != null, "The operation failure action is required.");
            Debug.Assert(operationLatency != null, "The operation latency action is required.");

            this.operationSuccess = operationSuccess;
            this.operationFailure = operationFailure;
            this.operationLatency = operationLatency;
            this.counterInstanceName = counterInstanceName;

            operationStarted(counterInstanceName);

            this.latencyStopWatch = Stopwatch.StartNew();
        }

        /// <summary>
        /// Mark the operation as being successful.  If this is not called before the
        /// CounterScope is disposed, the operation will be considered a failure.  
        /// </summary>
        /// <remarks>
        /// It is assumed this will be called from the same thread on which the dispose
        /// occurs.  If that is not the case, Dispose might see the wrong value of
        /// completedSuccessfully.
        /// </remarks>
        public void Success()
        {
            this.completedSuccessfully = true;
        }

        /// <summary>
        /// Marks the end of the counter scope including calculation of latency and
        /// reporting of success if the operation was successful.
        /// </summary>
        public void Dispose()
        {
            // FxCop suggests adding this so that future derived classes with
            // finalizers don't have to reimplement IDisposable.
            GC.SuppressFinalize(this);
            this.latencyStopWatch.Stop();
            this.operationLatency((ulong)this.latencyStopWatch.ElapsedMilliseconds, this.counterInstanceName);
            if (this.completedSuccessfully)
            {
                this.operationSuccess(this.counterInstanceName);
            }
            else
            {
                this.operationFailure(this.counterInstanceName);
            }
        }
    }
}
