// <copyright file="PaymentRequestsExController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7.PaymentClient
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Common.Web;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService;
    using static Microsoft.Commerce.Payments.PXService.V7.Constants;

    public class PaymentRequestsExController : ProxyController
    {
        /// <summary>
        /// Attach cahllenge data to payment request
        /// </summary>
        /// <group>PaymentRequestsEx</group>
        /// <verb>POST</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/paymentClient/paymentRequestsEx/{paymentRequestId}/attachChallengeData</url>
        /// <param name="paymentInstrument" required="true" cref="object" in="body">payment instrument object including piid</param>
        /// <param name="paymentRequestId" required="true" cref="string" in="path">payment request id</param>
        /// <response code="200">PaymentRequest object</response>
        /// <returns>PaymentRequest object</returns>
        [HttpPost]
        public async Task<HttpResponseMessage> AttachChallengeData(
            [FromBody] PIDLData paymentInstrument,
            string paymentRequestId)
        {
            const string CvvTokenPath = "cvvToken";
            const string PiIdPath = "piId";

            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();

            if (paymentInstrument == null)
            {
                throw TraceCore.TraceException(traceActivityId, new ValidationException(ErrorCode.InvalidRequestData, "PaymentInstrument information not found in the request"));
            }

            var cvvToken = paymentInstrument.TryGetPropertyValue(CvvTokenPath);
            var piId = paymentInstrument.TryGetPropertyValue(PiIdPath);

            if (string.IsNullOrWhiteSpace(cvvToken))
            {
                throw TraceCore.TraceException(traceActivityId, new ValidationException(ErrorCode.InvalidRequestData, "cvvToken data not found in the payload"));
            }

            if (string.IsNullOrWhiteSpace(piId))
            {
                throw TraceCore.TraceException(traceActivityId, new ValidationException(ErrorCode.InvalidRequestData, "piid data not found in the payload"));
            }

            var paymentRequestClientActions = await this.Settings.PaymentOrchestratorServiceAccessor.PaymentRequestAttachChallengeData(paymentRequestId, piId, PaymentInstrumentChallengeType.Cvv, cvvToken, traceActivityId);
            return this.Request.CreateResponse(new RequestStatusResponse { RequestId = paymentRequestClientActions.PaymentRequestId, Status = paymentRequestClientActions.Status });
        }

        /// <summary>
        /// Delete eligible payment instrument from payment request
        /// </summary>
        /// <group>PaymentRequestsEx</group>
        /// <verb>POST</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/paymentClient/paymentRequestsEx/{paymentRequestId}/removeEligiblePaymentMethods</url>
        /// <param name="paymentInstrument" required="true" cref="object" in="body">payment instrument object including piid</param>
        /// <param name="paymentRequestId" required="true" cref="string" in="path">payment request id</param>
        /// <response code="200">PaymentRequest object</response>
        /// <returns>PaymentRequest object</returns>
        [HttpPost]
        public async Task<HttpResponseMessage> RemoveEligiblePaymentmethods(
            [FromBody] PIDLData paymentInstrument,
            string paymentRequestId)
        {
            const string PiIdPath = "piid";

            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();

            if (paymentInstrument == null)
            {
                throw TraceCore.TraceException(traceActivityId, new ValidationException(ErrorCode.InvalidRequestData, "PaymentInstrument information not found in the request"));
            }

            var piId = paymentInstrument.TryGetPropertyValue(PiIdPath);

            if (string.IsNullOrWhiteSpace(piId))
            {
                throw TraceCore.TraceException(traceActivityId, new ValidationException(ErrorCode.InvalidRequestData, "piid data not found in the payload"));
            }

            var paymentRequestClientActions = await this.Settings.PaymentOrchestratorServiceAccessor.RemoveEligiblePaymentmethods(paymentRequestId, piId, traceActivityId);

            PidlModel.V7.ActionContext actionContext = new PidlModel.V7.ActionContext()
            {
                Action = ReturnContextClientActionTypes.Refresh,
                Id = paymentRequestClientActions.PaymentRequestId,
            };

            PidlModel.V7.PIDLResource pidlResource = new PidlModel.V7.PIDLResource()
            {
                ClientAction = new PXCommon.ClientAction(PXCommon.ClientActionType.ReturnContext, actionContext)
            };

            return this.Request.CreateResponse(pidlResource);
        }
    }
}