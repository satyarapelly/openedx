// <copyright file="PaymentSessionDescriptionsController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model;
    using Newtonsoft.Json;

    [ApiController]
    [Route("api/[controller]")]
    public class PaymentSessionDescriptionsController : ProxyController
    {
        /// <summary>
        /// Returns a PaymentSession PIDL for the given PaymentSessionData.
        /// </summary>
        /// <group>PaymentSessionDescriptions</group>
        /// <verb>GET</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/PaymentSessionDescriptions</url>
        /// <param name="accountId">User's account id</param>
        /// <param name="paymentSessionData">the context to create PaymentSession Object</param>
        /// <response code="200">List&lt;PIDLResource&gt;</response>
        /// <returns>Returns a PaymentSession PIDL for the given PaymentSessionData</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822", Justification = "Needs to be an instance method for Route action selection")]
        [HttpGet]
        public async Task<List<PIDLResource>> Get(string accountId, string paymentSessionData)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            PaymentSessionData paymentSessionDataObj = JsonConvert.DeserializeObject<PaymentSessionData>(paymentSessionData);

            string tid = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.AadInfo.Tid);
            string userId = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.Puid);
            bool isGuestUser = GuestAccountHelper.IsGuestAccount(this.Request);

            // Use Partner Settings if enabled for the partner
            PaymentExperienceSetting setting = this.GetPaymentExperienceSetting(Constants.Component.HandlePaymentChallenge);
            this.EnableFlightingsInPartnerSetting(setting, paymentSessionDataObj.Country);

            PaymentChallenge.PaymentSessionsHandler paymentSessionsHandler = await this.GetVersionBasedPaymentSessionsHandler(traceActivityId);
            PaymentSession paymentSession = await paymentSessionsHandler.CreatePaymentSession(
                accountId: accountId, 
                paymentSessionData: paymentSessionDataObj, 
                deviceChannel: PXService.Model.ThreeDSExternalService.DeviceChannel.Browser,
                exposedFlightFeatures: this.ExposedFlightFeatures,
                emailAddress: await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.EmailAddress),
                traceActivityId: traceActivityId,
                testContext: HttpRequestHelper.GetTestHeader(this.Request),
                isMotoAuthorized: this.Request.GetRequestHeader(GlobalConstants.HeaderValues.IsMotoHeader),
                tid: tid,
                userId: userId,
                isGuestUser: isGuestUser,
                setting: setting);

            List<PIDLResource> pidlResource = PIDLResourceFactory.GetPaymentSessionPidl(paymentSession);
            return pidlResource;
        }
    }
}