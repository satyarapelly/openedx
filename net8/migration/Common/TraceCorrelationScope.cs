//-----------------------------------------------------------------------
// <copyright file="TraceCorrelationScope.cs" company="Microsoft">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Commerce.Payments.PXCommon
{
    using System;
    using System.Runtime.Remoting.Messaging;
    using Microsoft.Commerce.Payments.Common.Tracing;

    /// <summary>
    /// Push a <see cref="T:EventTraceActivity"/> onto the logical call context and pops it when exiting its scope.
    /// </summary>
    public class TraceCorrelationScope : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TraceCorrelationScope"/> class.
        /// Pushes the given <see cref="T:EventTraceActivity"/> onto the logical call context.
        /// Within this scope the activity is available through <see cref="EventTraceActivity.Current"/>.
        /// </summary>
        /// <param name="eventTraceActivity">The event trace activity.</param>
        public TraceCorrelationScope(EventTraceActivity eventTraceActivity)
        {
            CallContext.LogicalSetData(EventTraceActivity.Name, eventTraceActivity);
        }

        /// <summary>
        /// Empties the <see cref="T:EventTraceActivity"/> data slot on the logical call context.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            CallContext.FreeNamedDataSlot(EventTraceActivity.Name);
        }
    }
}
