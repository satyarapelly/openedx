// <copyright file="PaymentThirdPartyPaymentAndCheckoutsController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Controllers
{
    using System.Net.Http;
    using System.Web.Http;
    using Constants = Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Constants;

    public class PaymentThirdPartyPaymentAndCheckoutsController : EmulatorBaseController
    {
        public PaymentThirdPartyPaymentAndCheckoutsController() : base(Constants.TestScenarioManagers.PaymentThirdParty, Constants.DefaultTestScenarios.PaymentThirdPartyEmulatorPayment)
        {
        }

        [HttpGet]
        public HttpResponseMessage GetPaymentRequest([FromUri] string paymentProviderId, [FromUri] string paymentRequestId)
        {
            return this.GetResponse(Constants.PaymentThirdPartyApiName.GetPaymentRequest);
        }

        [HttpGet]
        public HttpResponseMessage GetCheckout([FromUri] string paymentProviderId, [FromUri]string checkoutId)
        {
            return this.GetResponse(Constants.PaymentThirdPartyApiName.GetCheckout);
        }
    }
}