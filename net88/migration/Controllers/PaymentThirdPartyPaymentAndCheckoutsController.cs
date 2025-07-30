// <copyright file="PaymentThirdPartyPaymentAndCheckoutsController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Controllers
{
    using System.Net.Http;
    using Microsoft.AspNetCore.Mvc;
    using Constants = Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Constants;

    public class PaymentThirdPartyPaymentAndCheckoutsController : EmulatorBaseController
    {
        public PaymentThirdPartyPaymentAndCheckoutsController() : base(Constants.TestScenarioManagers.PaymentThirdParty, Constants.DefaultTestScenarios.PaymentThirdPartyEmulatorPayment)
        {
        }

        [HttpGet]
        public HttpResponseMessage GetPaymentRequest([FromQuery] string paymentProviderId, [FromQuery] string paymentRequestId)
        {
            return this.GetResponse(Constants.PaymentThirdPartyApiName.GetPaymentRequest);
        }

        [HttpGet]
        public HttpResponseMessage GetCheckout([FromQuery] string paymentProviderId, [FromQuery]string checkoutId)
        {
            return this.GetResponse(Constants.PaymentThirdPartyApiName.GetCheckout);
        }
    }
}