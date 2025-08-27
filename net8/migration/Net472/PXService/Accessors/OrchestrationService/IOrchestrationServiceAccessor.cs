// <copyright file="IOrchestrationServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.OrchestrationService
{
    using System.Threading.Tasks;
    using Tracing;

    public interface IOrchestrationServiceAccessor
    {
        /// <summary>
        /// Task to remove payment instrument.
        /// </summary>
        /// <param name="paymentInstrumentId">The payment instrument to be removed.</param>
        /// <param name="traceActivityId">Trace activity Id.</param>
        /// <returns>No content.</returns>
        Task RemovePaymentInstrument(string paymentInstrumentId, EventTraceActivity traceActivityId);

        /// <summary>
        /// Replace target PI with source PI.
        /// </summary>
        /// <param name="sourcePaymentInstrumentId">Source PIID.</param>
        /// <param name="targetPaymentInstrumentId">Target PIID.</param>
        /// <param name="paymentSessionId">Payment Session Id.</param>
        /// <param name="traceActivityId">Trace Activity Id.</param>
        /// <returns>No content.</returns>
        Task ReplacePaymentInstrument(string sourcePaymentInstrumentId, string targetPaymentInstrumentId, string paymentSessionId, EventTraceActivity traceActivityId);
    }
}
