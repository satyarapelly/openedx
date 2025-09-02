// <copyright file="NoOpRequestTrackingCounter.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Monitoring
{
    public class NoOpRequestTrackingCounter : IRequestTrackingCounter
    {
        public void RecordTotal()
        {
            // no-op
        }

        public void RecordSuccess()
        {
            // no-op
        }

        public void RecordFailure()
        {
            // no-op
        }

        public void RecordLatency(ulong milliseconds)
        {
            // no-op
        }

        public void RecordTotal(string instanceName)
        {
            // no-op
        }

        public void RecordSuccess(string instanceName)
        {
            // no-op
        }

        public void RecordFailure(string instanceName)
        {
            // no-op
        }

        public void RecordLatency(ulong milliseconds, string instanceName)
        {
            // no-op
        }
    }
}
