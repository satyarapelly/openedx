// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace CIT.PXService.Tests
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class AnomalyDetectionTests : TestBase
    {
        [TestMethod]
        public async Task VerifyAccountIdExceedingCountAndFailRate()
        {
            // Arrange
            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi002-MC");

            string sampleSuccessContent = JsonConvert.SerializeObject(expectedPI);
            string sampleErrorContent = "{\"ErrorCode\":\"ValidationFailed\",\"Message\":\"The payment instrument cannot be validated.Please contact the payment processor for help.\",\"Details\":[]}";

            await TestBatchedRequests(
                new List<RequestBatch>()
                {
                    // CardTest detection wont start until there is atleast 100 data points
                    new RequestBatch()
                    {
                        AccountId = "WarmupAccount",
                        BatchSize = 100,
                        PimsResponseCode = HttpStatusCode.OK,
                        PimsResponseContent = sampleSuccessContent,
                        ExpectedPXResponseCode = HttpStatusCode.OK
                    },
                    new RequestBatch()
                    {
                        AccountId = "RateLimitTestAccount002",
                        BatchSize = 6,
                        PimsResponseCode = HttpStatusCode.BadRequest,
                        PimsResponseContent = sampleErrorContent,
                        ExpectedPXResponseCode = HttpStatusCode.BadRequest
                    },

                    // 6 Fail = Count of 6 requests meets/exceeds the threshold of 6
                    // 6 Fail = Fail rate of 100% meets/exceed the theshold of 85%
                    // At this point, any more requests from RateLimitTestAccount002 should be blocked
                    new RequestBatch()
                    {
                        AccountId = "RateLimitTestAccount002",
                        BatchSize = 2,
                        PimsResponseCode = HttpStatusCode.OK,
                        PimsResponseContent = sampleSuccessContent,
                        ExpectedPXResponseCode = HttpStatusCode.BadRequest
                    }
                });
        }

        // This test case can succeed when run individually because it expects a system failure.
        // When this test runs along with other tests WarmupAccount requests from other tests do not create a system failure.
        // [DataRow(true, true)]
        // [DataRow(false, false)]
        // [TestMethod]
        public async Task VerifyAccountIdExceedingCountAndFailRate_HighBaselineFailures(bool sendDisableBaselineCheckFlight, bool detectCardTesting)
        {
            // Arrange
            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi002-MC");

            string sampleSuccessContent = JsonConvert.SerializeObject(expectedPI);
            string sampleErrorContent = "{\"ErrorCode\":\"ValidationFailed\",\"Message\":\"The payment instrument cannot be validated.Please contact the payment processor for help.\",\"Details\":[]}";

            if (sendDisableBaselineCheckFlight)
            {
                PXFlightHandler.AddToEnabledFlights("PXRateLimitDisableBaselineCheck");
            }

            await TestBatchedRequests(
                new List<RequestBatch>()
                {
                    // CardTest detection wont start until there is atleast 100 data points
                    // Baseline of 100% failure, indicating a system failure
                    new RequestBatch()
                    {
                        AccountId = "WarmupAccount",
                        BatchSize = 100,
                        PimsResponseCode = HttpStatusCode.BadRequest,
                        PimsResponseContent = sampleErrorContent,
                        ExpectedPXResponseCode = HttpStatusCode.BadRequest
                    },
                    new RequestBatch()
                    {
                        AccountId = "RateLimitTestAccount002",
                        BatchSize = 6,
                        PimsResponseCode = HttpStatusCode.BadRequest,
                        PimsResponseContent = sampleErrorContent,
                        ExpectedPXResponseCode = HttpStatusCode.BadRequest
                    },

                    // 6 Fail = Count of 6 requests meets/exceeds the threshold of 6
                    // 6 Fail = Fail rate of 100% meets/exceed the theshold of 85%
                    // At this point, because of detection of system failure, any more requests from RateLimitTestAccount002 should not be blocked.
                    // But if PXRateLimitDisableBaselineCheck is sent, then system failure is ignored and requests from RateLimitTestAccount002 are blocked.
                    new RequestBatch()
                    {
                        AccountId = "RateLimitTestAccount002",
                        BatchSize = 2,
                        PimsResponseCode = HttpStatusCode.OK,
                        PimsResponseContent = sampleSuccessContent,
                        ExpectedPXResponseCode = detectCardTesting ? HttpStatusCode.BadRequest : HttpStatusCode.OK
                    }
                });
        }

        [TestMethod]
        public async Task VerifyAccountIdExceedingCountButNotFailRate()
        {
            // Arrange
            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi002-MC");

            string sampleSuccessContent = JsonConvert.SerializeObject(expectedPI);
            string sampleErrorContent = "{\"ErrorCode\":\"ValidationFailed\",\"Message\":\"The payment instrument cannot be validated.Please contact the payment processor for help.\",\"Details\":[]}";

            await TestBatchedRequests(
                new List<RequestBatch>()
                {
                    new RequestBatch()
                    {
                        // CardTest detection wont start until there is atleast 100 data points
                        AccountId = "WarmupAccount",
                        BatchSize = 100,
                        PimsResponseCode = HttpStatusCode.OK,
                        PimsResponseContent = sampleSuccessContent,
                        ExpectedPXResponseCode = HttpStatusCode.OK
                    },
                    new RequestBatch()
                    {
                        AccountId = "RateLimitTestAccount003",
                        BatchSize = 2,
                        PimsResponseCode = HttpStatusCode.OK,
                        PimsResponseContent = sampleSuccessContent,
                        ExpectedPXResponseCode = HttpStatusCode.OK
                    },
                    new RequestBatch()
                    {
                        AccountId = "RateLimitTestAccount003",
                        BatchSize = 8,
                        PimsResponseCode = HttpStatusCode.BadRequest,
                        PimsResponseContent = sampleErrorContent,
                        ExpectedPXResponseCode = HttpStatusCode.BadRequest
                    },

                    // 2 Pass and 8 Fail = Count of 10 requests exceeds the threshold of 6
                    // 2 Pass and 8 Fail = Fail rate of 80% does NOT exceed the theshold of 85%
                    // At this point RateLimitTestAccount003 should still NOT be blocked
                    new RequestBatch()
                    {
                        AccountId = "RateLimitTestAccount003",
                        BatchSize = 1,
                        PimsResponseCode = HttpStatusCode.OK,
                        PimsResponseContent = sampleSuccessContent,
                        ExpectedPXResponseCode = HttpStatusCode.OK
                    },
                });
        }

        [TestMethod]
        public async Task VerifyNotBlockedTestAccount()
        {
            // Arrange
            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi002-MC");

            string sampleSuccessContent = JsonConvert.SerializeObject(expectedPI);
            string sampleErrorContent = "{\"ErrorCode\":\"ValidationFailed\",\"Message\":\"The payment instrument cannot be validated.Please contact the payment processor for help.\",\"Details\":[]}";

            await TestBatchedRequests(
                new List<RequestBatch>()
                {
                    new RequestBatch()
                    {
                        // CardTest detection wont start until there is atleast 100 data points
                        AccountId = "WarmupAccount",
                        BatchSize = 100,
                        PimsResponseCode = HttpStatusCode.OK,
                        PimsResponseContent = sampleSuccessContent,
                        ExpectedPXResponseCode = HttpStatusCode.OK
                    },
                    new RequestBatch()
                    {
                        AccountId = "8e342cdc-771b-4b19-84a0-bef4c44911f7",
                        BatchSize = 10,
                        PimsResponseCode = HttpStatusCode.BadRequest,
                        PimsResponseContent = sampleErrorContent,
                        ExpectedPXResponseCode = HttpStatusCode.BadRequest
                    },

                    // 10 out of 10 Fail = Count of 10 requests exceeds the threshold of 6
                    // 10 out of 10 Fail = Fail rate of 100% exceeds the theshold of 85%
                    // Still, RateLimit won't block this account as this test account is included in the whitelist
                    new RequestBatch()
                    {
                        AccountId = "8e342cdc-771b-4b19-84a0-bef4c44911f7",
                        BatchSize = 1,
                        PimsResponseCode = HttpStatusCode.OK,
                        PimsResponseContent = sampleSuccessContent,
                        ExpectedPXResponseCode = HttpStatusCode.OK
                    },
                });
        }

        private static async Task TestBatchedRequests(List<RequestBatch> batches)
        {
            var emptyRequestBody = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "visa"
            };

            foreach (var batch in batches)
            {
                for (int i = 0; i < batch.BatchSize; i++)
                {
                    // Arrange
                    PXSettings.PimsService.ArrangeResponse(
                        content: batch.PimsResponseContent,
                        statusCode: batch.PimsResponseCode);

                    string url = string.Format("/v7.0/{0}/paymentInstrumentsEx?country=us&language=en-US&partner=webblends", batch.AccountId);

                    HttpResponseMessage result = await PXClient.PostAsync(
                        url,
                        new StringContent(
                            JsonConvert.SerializeObject(emptyRequestBody),
                            Encoding.UTF8,
                            PaymentConstants.HttpMimeTypes.JsonContentType));

                    Assert.AreEqual(batch.ExpectedPXResponseCode, result.StatusCode);

                    PXSettings.PimsService.ResetToDefaults();
                }
            }
        }
    }
}
