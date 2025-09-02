// <copyright file="IFraudDetectionServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.FraudDetectionService
{
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.PXService.Model.FraudDetectionService;

    /// <summary>
    /// Provides functionality to access FraudDetectionService
    /// </summary>
    public interface IFraudDetectionServiceAccessor
    {
        /// <summary>
        /// Bot detection 
        /// </summary>
        /// <param name="requestId">Request Id, e.g. payment request or checkout request id</param> 
        /// <param name="traceActivityId">Trace Activity Id</param> 
        /// <returns>Evaluation Result</returns>
        Task<EvaluationResult> BotDetection(string requestId, EventTraceActivity traceActivityId);

        /// <summary>
        /// Bot detection Confirmation
        /// </summary>
        /// <param name="requestId">Request Id, e.g. payment request or checkout request id</param> 
        /// <param name="traceActivityId">Trace Activity Id</param> 
        /// <param name="isChallengeResolved">Challenge Resolution Result</param> 
        /// <returns>Evaluation Result</returns>
        Task<EvaluationResult> BotDetectionConfirmation(string requestId, EventTraceActivity traceActivityId, bool isChallengeResolved);
    }
}