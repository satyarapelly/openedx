// <copyright file="PaymentSessionDescriptionsController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PXCommon;
    using Model;
    using Newtonsoft.Json;

    public class PaymentSessionDescriptionsController : ProxyController
    {
        /// <summary>
        /// Returns a PaymentSession PIDL for the given PaymentSessionData.
        /// </summary>
        /// <param name="accountId">User's account id</param>
        /// <param name="paymentSessionData">The context to create PaymentSession</param>
        /// <returns>Returns a purchase context PIDL for the given piid</returns>
        [HttpGet]
        public async Task<List<PIDLResource>> Get([FromUri]string accountId, [FromUri]string paymentSessionData)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            try
            {
                string tid = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.AadInfo.Tid);
                PaymentSessionData paymentSessionDataObj = JsonConvert.DeserializeObject<PaymentSessionData>(paymentSessionData);
                this.Request.AddPartnerProperty(paymentSessionDataObj?.Partner?.ToLower());

                PaymentSessionsHandler paymentSessionsHandler = await this.GetVersionBasedPaymentSessionsHandler(traceActivityId);
                PaymentSession paymentSession = await paymentSessionsHandler.CreatePaymentSession(
                    accountId: accountId,
                    paymentSessionData: paymentSessionDataObj,
                    deviceChannel: PXService.Model.ThreeDSExternalService.DeviceChannel.Browser,
                    exposedFlightFeatures: this.ExposedFlightFeatures,
                    emailAddress: await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.EmailAddress),
                    traceActivityId: traceActivityId,
                    testContext: HttpRequestHelper.GetTestHeader(),
                    isMotoAuthorized: this.Request.GetRequestHeader(GlobalConstants.HeaderValues.IsMotoHeader),
                    tid: tid);

                List<PIDLResource> pidlResource = PIDLResourceFactory.GetPaymentSessionPidl(paymentSession);
                return pidlResource;
            }
            catch (Exception ex)
            {
                throw new ValidationException(ErrorCode.InvalidRequestData, "paymentSessionData is invalid" + ex.Message);
            }
        }
    }
}