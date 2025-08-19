// <copyright file="PurchaseController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Controllers
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using Microsoft.AspNetCore.Mvc;
    using FromUri = Microsoft.AspNetCore.Mvc.FromQueryAttribute;
    using Common.Transaction;
    using Common.Web;
    using Test.Common;
    using Constants = Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Constants;
    using Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Extensions;

    public class PurchaseController : EmulatorBaseController
    {
        private static int orderStatusPurchasePollingCounter = 0;

        public PurchaseController() : base(Constants.TestScenarioManagers.Purchase)
        {
        }

        [HttpGet]
        public HttpResponseMessage ListSub([FromUri] string userId)
        {
            return this.GetResponse(Constants.PurchaseApiName.ListSub);
        }

        [HttpGet]
        [HttpPost]
        public HttpResponseMessage ListOrder([FromUri] string userId)
        {
            return this.GetResponse(Constants.PurchaseApiName.ListOrder);
        }

        [HttpGet]
        [HttpPut]
        public HttpResponseMessage GetOrder([FromUri] string userId, [FromUri] string orderId)
        {
            var response = this.GetResponse(Constants.PurchaseApiName.GetOrder);
            this.PlaceholderReplacements[Constants.Placeholders.ShipToAddressId] = orderId;

            return this.ReplacePlaceholders(response);
        }

        [HttpGet]
        public HttpResponseMessage GetSub([FromUri] string userId, [FromUri] string recurrenceId)
        {
            var response = this.GetResponse(Constants.PurchaseApiName.GetSub);
            this.PlaceholderReplacements[Constants.Placeholders.Id] = recurrenceId;

            return this.ReplacePlaceholders(response);
        }

        [HttpGet]
        public HttpResponseMessage CheckPi([FromUri] string userId, [FromUri] string paymentinstrumentid)
        {
            return this.GetResponse(Constants.PurchaseApiName.CheckPi);
        }

        protected override HttpResponseMessage GetResponse(string apiName)
        {
            TestContext testContext = null;
            if (this.Request.TryGetTestContext(out testContext))
            {
                if (testContext != null &&
                    testContext.ScenariosContain(Constants.TestScenarios.PXPurchasefdRedeemcsvSuccess))
                {
                    return TestScenarioManager.GetResponse(apiName, testContext);
                }

                if (testContext != null &&
                    testContext.ScenariosContain(Constants.TestScenarios.PXPurchasefdConfirmPaymentPolling) &&
                    Interlocked.CompareExchange(ref orderStatusPurchasePollingCounter, 0, 5) < 5)
                {
                    Interlocked.Increment(ref orderStatusPurchasePollingCounter); // Increase counter for polling
                    string pollpendingResponse = "{\"orderState\":\"pending\"}";
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(pollpendingResponse, Encoding.UTF8, Test.Common.Constants.HeaderValues.JsonContent)
                    };
                }

                Interlocked.Exchange(ref orderStatusPurchasePollingCounter, 0); // Reset when polling is finished.
                return TestScenarioManager.GetResponse(apiName, testContext);
            }
            else
            {
                // Return payload from default scenario
                testContext = new TestContext("PXDependencyEmulator.PurchaseEmulator", DateTime.UtcNow, Constants.TestScenarios.PXPurchasefdListtrxSuccess);
                return TestScenarioManager.GetResponse(apiName, testContext);
            }
        }
    }
}