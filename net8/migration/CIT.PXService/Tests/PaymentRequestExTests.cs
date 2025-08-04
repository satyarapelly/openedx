// <copyright file="PaymentRequestExTests.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace CIT.PXService.Tests
{
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService;
    using global::Tests.Common.Model.Pidl;

    [TestClass]
    public class PaymentRequestExTests : TestBase
    {
        [TestInitialize]
        public void Startup()
        {
            PXSettings.AddressEnrichmentService.Responses.Clear();
            PXSettings.AccountsService.Responses.Clear();
            PXSettings.PimsService.Responses.Clear();
            PXSettings.TokenPolicyService.Responses.Clear();
            PXSettings.PurchaseService.Responses.Clear();
            PXSettings.PartnerSettingsService.Responses.Clear();
            PXSettings.OrchestrationService.Responses.Clear();
            PXSettings.IssuerService.Responses.Clear();
            PXSettings.CatalogService.Responses.Clear();

            PXSettings.AddressEnrichmentService.ResetToDefaults();
            PXSettings.AccountsService.ResetToDefaults();
            PXSettings.PimsService.ResetToDefaults();
            PXSettings.TokenPolicyService.ResetToDefaults();
            PXSettings.PurchaseService.ResetToDefaults();
            PXSettings.PartnerSettingsService.ResetToDefaults();
            PXSettings.OrchestrationService.ResetToDefaults();
            PXSettings.IssuerService.ResetToDefaults();
            PXSettings.CatalogService.ResetToDefaults();
            PXSettings.PaymentOrchestratorService.ResetToDefaults();
        }

        [TestMethod]
        public async Task AttachChallengeDataTest()
        {
            // Arrange
            string requestId = "pr_39c93cc0-e855-42bc-8aca-183a572e14bc";
            string url = string.Format("/v7.0/PaymentClient/PaymentRequestsEx/{0}/attachChallengeData", requestId);

            var initialPaymentRequest = new PaymentRequest
            {
                PaymentRequestId = requestId,
                Status = PaymentRequestStatus.PendingClientAction,
                PaymentInstruments = new List<PaymentInstrument>
                {
                    new PaymentInstrument
                    {
                        PaymentInstrumentId = "Account001-Pi001-Visa",
                        PaymentMethodType = Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService.PaymentMethodType.Visa,
                        Usage = PaymentInstrumentUsage.PrimaryPaymentInstrument
                    }
                }
            };

            var paymentRequest = new PaymentRequest
            {
                PaymentRequestId = requestId,
                Status = PaymentRequestStatus.PendingInitialTransaction,
            };

            PXSettings.PaymentOrchestratorService.ArrangeResponse(JsonConvert.SerializeObject(initialPaymentRequest), HttpStatusCode.OK, HttpMethod.Get);
            PXSettings.PaymentOrchestratorService.ArrangeResponse(JsonConvert.SerializeObject(paymentRequest), HttpStatusCode.OK, HttpMethod.Post);

            var payload = new
            {
                cvvToken = "123",
                piId = "Account001-Pi001-Visa"
            };

            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl(url)),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            // Act
            var response = await PXClient.SendAsync(request);

            // Assert
            string result = await response.Content.ReadAsStringAsync();
            JObject jsonObject = JObject.Parse(result);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task RemoveEligiblePaymentmethodsTest()
        {
            // Arrange
            string requestId = "pr_39c93cc0-e855-42bc-8aca-183a572e14bc";
            string url = string.Format("/v7.0/PaymentClient/PaymentRequestsEx/{0}/removeEligiblePaymentMethods", requestId);

            var payload = new
            {
                piid = "Account001-Pi001-Visa"
            };

            var initialPaymentRequest = new PaymentRequest
            {
                PaymentRequestId = requestId,
                Status = PaymentRequestStatus.PendingClientAction,
                PaymentInstruments = new List<PaymentInstrument>
                {
                    new PaymentInstrument
                    {
                        PaymentInstrumentId = "Account001-Pi001-Visa",
                        PaymentMethodType = Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService.PaymentMethodType.Visa,
                        Usage = PaymentInstrumentUsage.PrimaryPaymentInstrument
                    }
                }
            };

            PXSettings.PaymentOrchestratorService.ArrangeResponse(JsonConvert.SerializeObject(initialPaymentRequest), HttpStatusCode.OK, HttpMethod.Post);

            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl(url)),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            // Act
            var response = await PXClient.SendAsync(request);

            // Assert
            string result = await response.Content.ReadAsStringAsync();
            PIDLResource pidl = ReadSinglePidlResourceFromJson(result);

            // Assert
            Assert.IsNotNull(pidl.ClientAction, "Client action missing");
            Assert.AreEqual(global::Tests.Common.Model.Pidl.ClientActionType.ReturnContext, pidl.ClientAction.ActionType);
            Assert.IsNotNull(pidl.ClientAction.Context, "Client action context missing");
            Assert.IsTrue(pidl.ClientAction.Context.ToString().Contains("refresh"), "Client action context missing refresh action type");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
