// <copyright file="PaymentOchestratorCheckoutRequestsController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Controllers
{
    using System.Net.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService;
    using Newtonsoft.Json;
    using Constants = Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Constants;

    public class PaymentOchestratorCheckoutRequestsController : EmulatorBaseController
    {
        public PaymentOchestratorCheckoutRequestsController() : base(Constants.TestScenarioManagers.PaymentOchestrator, Constants.DefaultTestScenarios.POEmulator)
        {
        }

        [HttpPost]
        public HttpResponseMessage AttachAddress([FromQuery]string checkoutId, [FromBody] object address, [FromQuery] string type)
        {
            return this.GetResponse(Constants.PaymentOchestratorApiName.AttachAddress);
        }

        [HttpPost]
        public HttpResponseMessage PRAttachAddress([FromQuery]string paymentRequestId, [FromQuery] string type)
        {
            return this.GetResponse(Constants.PaymentOchestratorApiName.PRAttachAddress);
        }

        [HttpPost]
        public HttpResponseMessage AttachProfile([FromQuery] string checkoutId, [FromBody] object address)
        {
            return this.GetResponse(Constants.PaymentOchestratorApiName.AttachProfile);
        }

        [HttpPost]
        public HttpResponseMessage PRAttachProfile([FromQuery] string paymentRequestId, [FromBody] object address)
        {
            return this.GetResponse(Constants.PaymentOchestratorApiName.PRAttachProfile);
        }

        [HttpPost]
        public HttpResponseMessage AttachPaymentInstruments([FromQuery] string checkoutId, [FromBody] object pi)
        {
            return this.GetResponse(Constants.PaymentOchestratorApiName.AttachPaymentInstruments);
        }

        [HttpPost]
        public HttpResponseMessage PRAttachPaymentInstruments([FromQuery] string paymentRequestId, [FromBody] object pi)
        {
            return this.GetResponse(Constants.PaymentOchestratorApiName.PRAttachPaymentInstruments);
        }

        [HttpPost]
        public HttpResponseMessage Confirm([FromQuery] string checkoutId, [FromBody] object confirmpayload)
        {
            return this.GetResponse(Constants.PaymentOchestratorApiName.Confirm);
        }

        [HttpPost]
        public HttpResponseMessage PRConfirm([FromQuery] string paymentRequestId, [FromBody] object confirmpayload)
        {
            return this.GetResponse(Constants.PaymentOchestratorApiName.PRConfirm);
        }

        [HttpGet]
        public HttpResponseMessage ClientAction([FromQuery] string checkoutId)
        {
            var response = this.GetResponse(Constants.PaymentOchestratorApiName.GetClientAction);
            var responsePayload = response.Content.ReadAsStringAsync().Result;

            // TODO: Remove the direct dependency on the PXService models in the emulator which makes the
            // build and starting time too long. Instead use the PlaceholderReplacements to add the necessary data
            CheckoutRequestClientActions clientAction = JsonConvert.DeserializeObject<CheckoutRequestClientActions>(responsePayload);
            if (clientAction != null)
            {
                clientAction.CheckoutRequestId = checkoutId;
                responsePayload = JsonConvert.SerializeObject(clientAction);
            }

            response.Content = new StringContent(responsePayload, System.Text.Encoding.UTF8, Test.Common.Constants.HeaderValues.JsonContent);
            return response;
        }

        [HttpGet]
        public HttpResponseMessage GetClientActions([FromQuery] string paymentRequestId)
        {
            var response = this.GetResponse(Constants.PaymentOchestratorApiName.GetClientActions);
            this.PlaceholderReplacements[Constants.Placeholders.PaymentRequestId] = paymentRequestId;

            return this.ReplacePlaceholders(response);
        }
    }
}