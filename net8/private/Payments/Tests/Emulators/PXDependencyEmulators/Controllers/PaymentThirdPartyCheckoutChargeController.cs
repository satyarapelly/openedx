// <copyright file="PaymentThirdPartyCheckoutChargeController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Controllers
{
    using System.Net.Http;
    using Microsoft.AspNetCore.Mvc;
using FromUri = Microsoft.AspNetCore.Mvc.FromQueryAttribute;
    using Constants = Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Constants;

    public class PaymentThirdPartyCheckoutChargeController : EmulatorBaseController
    {
        public PaymentThirdPartyCheckoutChargeController() : base(Constants.TestScenarioManagers.PaymentThirdParty, Constants.DefaultTestScenarios.PaymentThirdPartyEmulatorCheckout)
        {
        }

        [HttpPost]
        public HttpResponseMessage Charge(
            [FromUri] string paymentProviderId, 
            [FromUri] string checkoutId, 
            [FromBody] object chargeRequest)
        {
            return this.GetResponse(Constants.PaymentThirdPartyApiName.Charge);
        }

        [HttpGet]
        public HttpResponseMessage Status(
            [FromUri] string paymentProviderId,
            [FromUri] string checkoutId)
        {
            return this.GetResponse(Constants.PaymentThirdPartyApiName.Status);
        }

        [HttpGet]
        public HttpResponseMessage Completed(
            [FromUri] string paymentProviderId,
            [FromUri] string checkoutId)
        {
            return this.GetResponse(Constants.PaymentThirdPartyApiName.Completed);
        }
    }
}