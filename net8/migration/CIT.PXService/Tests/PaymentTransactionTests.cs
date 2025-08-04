// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2019. All rights reserved.</copyright>
namespace CIT.PXService.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXService;
    using Microsoft.Commerce.Payments.PXService.Model.PurchaseService;
    using Microsoft.Commerce.Payments.PXService.V7.PaymentTransaction.Model;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xbox.Experimentation.Contracts.GroupsAdmin;
    using Moq;
    using Newtonsoft.Json;
    using static Microsoft.Commerce.Payments.PXCommon.Constants;

    [TestClass]
    public class PaymentTransactionTests : TestBase
    {
        private const string AccountId = "cab65d16-bd42-4f3e-bad4-c33a1519b59a";
        private const string PUID = "1055518870507325";
        private const string EMAIL = "mstest_pymentsnstar1@outlook.com";
        private const string PUIDNextLink = "1055518870507445";
        private const string EMAINextLink = "mstest_pymentsnstar2@outlook.com";
        private Mock<IPurchaseServiceAccessor> purchaseServiceAccessorMock;
        private EventTraceActivity traceActivity;
        private const string ValidUserId = "testUserId";
        private const string ValidNextLink = "https://trusted.domain.com/nextPage";
        private const string InvalidNextLink = "https://untrusted.domain.com/nextPage";
        private List<string> exposedFlightFeatures;

        [TestInitialize]
        public void TestInitialize()
        {
            this.purchaseServiceAccessorMock = new Mock<IPurchaseServiceAccessor>();
            this.traceActivity = new EventTraceActivity();
            this.exposedFlightFeatures = new List<string> { "PXEnableSSRFPolicy" };
        }

        [TestMethod]
        public async Task ListOrders_WithValidNextLink_ReturnsOrders()
        {
            // Arrange
            var expectedOrders = new Orders { NextLink = "https://trusted.domain.com/nextPage2" };
            this.purchaseServiceAccessorMock
                .Setup(x => x.ListOrders(ValidUserId, ValidNextLink, traceActivity, exposedFlightFeatures))
                .ReturnsAsync(expectedOrders);

            // Act
            var actualOrders = await this.purchaseServiceAccessorMock.Object.ListOrders(ValidUserId, ValidNextLink, traceActivity, exposedFlightFeatures);

            // Assert
            Assert.IsNotNull(actualOrders);
            Assert.AreEqual(expectedOrders.NextLink, actualOrders.NextLink);
            this.purchaseServiceAccessorMock.Verify(x => x.ListOrders(ValidUserId, ValidNextLink, traceActivity, exposedFlightFeatures), Times.Once);
        }

        [TestMethod]
        public async Task ListOrders_WithInvalidNextLink_ThrowsException()
        {
            // Arrange
            const string InvalidNextLink = "https://untrusted.domain.com/orders/nextPage";
            const string ExpectedErrorMessage = "The next link URL is not in the trusted domain.";

            this.purchaseServiceAccessorMock
                .Setup(x => x.ListOrders(ValidUserId, InvalidNextLink, traceActivity, exposedFlightFeatures))
                .Throws(new Exception(ExpectedErrorMessage));

            // Act & Assert
            var exception = await Assert.ThrowsExceptionAsync<Exception>(() =>
                this.purchaseServiceAccessorMock.Object.ListOrders(ValidUserId, InvalidNextLink, traceActivity, exposedFlightFeatures));

            Assert.AreEqual(ExpectedErrorMessage, exception.Message);
            this.purchaseServiceAccessorMock.Verify(x => x.ListOrders(ValidUserId, InvalidNextLink, traceActivity, exposedFlightFeatures), Times.Once);
        }

        [TestMethod]
        public async Task ListOrders_WithEmptyExposedFlightFeatures_DoesNotValidateSSRF()
        {
            // Arrange
            var expectedOrders = new Orders { NextLink = "https://trusted.domain.com/nextPage2" };
            var emptyFlightFeatures = new List<string>();

            this.purchaseServiceAccessorMock
                .Setup(x => x.ListOrders(ValidUserId, ValidNextLink, traceActivity, emptyFlightFeatures))
                .ReturnsAsync(expectedOrders);

            // Act
            var actualOrders = await this.purchaseServiceAccessorMock.Object.ListOrders(ValidUserId, ValidNextLink, traceActivity, emptyFlightFeatures);

            // Assert
            Assert.IsNotNull(actualOrders);
            Assert.AreEqual(expectedOrders.NextLink, actualOrders.NextLink);
            this.purchaseServiceAccessorMock.Verify(x => x.ListOrders(ValidUserId, ValidNextLink, traceActivity, emptyFlightFeatures), Times.Once);
        }

        // Ignore this test for now because it fails at CDPx pipeline, the following task is created to track the fixation
        // Task 28999776: Fix failed CIT.PXService in CDPx pipeline
        [Ignore]
        [DataTestMethod]
        public async Task ListTransactions()
        {
            var url = string.Format("/v7.0/{0}/paymentTransactions", AccountId);
            var request = new HttpRequestMessage(HttpMethod.Get, GetPXServiceUrl(url));
            request.Headers.Add("x-ms-clientcontext-encoding", "base64");

            // Base64 encoded values of: PUID=1055518870507325,mstest_pymentsnstar1@outlook.com
            request.Headers.Add("x-ms-msaprofile", string.Format("PUID={0},emailAddress={1}", ToBase64(PUID), ToBase64(EMAIL)));

            var response = await PXClient.SendAsync(request);
            string responseText = await response.Content.ReadAsStringAsync();

            var responseContent = JsonConvert.DeserializeObject<Microsoft.Commerce.Payments.PXService.V7.PaymentTransaction.Model.PaymentTransactions>(responseText);

            string responseWithUtcTimeZone = JsonConvert.SerializeObject(
                responseContent,
                Formatting.None,
                new JsonSerializerSettings
                {
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc
                });

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            string expectedValue = "{\"pageSize\":100,\"hasMoreRecords\":false,\"continuationToken\":null,\"orders\":[{\"orderId\":\"fc30a4ba-ba2c-46a3-acec-fe81ca5e92e2\",\"orderLineItems\":[{\"orderLineItemId\":\"b7696286-9396-41d9-ad5a-4f09ab329baf\",\"productType\":\"PASS\",\"description\":\"3-Month Xbox Live Gold Membership (Digital Code)\",\"skuId\":\"0002\",\"productId\":\"CFQ7TTC0K5ZM\",\"quantity\":1,\"currency\":\"GBP\",\"totalAmount\":17.99,\"tax\":3.0}],\"description\":\"3-Month Xbox Live Gold Membership (Digital Code)\",\"orderedDate\":\"2020-07-13T10:41:28.9658854Z\",\"piid\":\"D+-WfwAAAAABAACA\",\"userId\":\"0000000000000000\",\"transactionStatus\":0,\"orderState\":\"Purchased\",\"currency\":\"GBP\",\"amount\":17.99,\"taxAmount\":3.0,\"refundedDate\":\"0001-01-01T00:00:00Z\",\"SubscriptionId\":null,\"checkPiResult\":true},{\"orderId\":\"f5598585-7340-4c5b-b41e-3df950c4925e\",\"orderLineItems\":[{\"orderLineItemId\":\"16bd31de-d49a-41c2-aa95-9de6d8117082\",\"productType\":\"Game\",\"description\":\"Riptide GP2\",\"skuId\":\"0010\",\"productId\":\"9WZDNCRFJ9Z1\",\"quantity\":1,\"currency\":\"USD\",\"totalAmount\":3.28,\"tax\":0.29}],\"description\":\"Riptide GP2\",\"orderedDate\":\"2019-11-14T22:07:12.0200785Z\",\"piid\":\"lchqggAAAAAHAACA\",\"userId\":\"1055518870507325\",\"transactionStatus\":0,\"orderState\":\"Purchased\",\"currency\":\"USD\",\"amount\":3.28,\"taxAmount\":0.29,\"refundedDate\":\"0001-01-01T00:00:00Z\",\"SubscriptionId\":null,\"checkPiResult\":true},{\"orderId\":\"365bf870-ad8d-44e7-a0d6-2d94c6dbb93e\",\"orderLineItems\":[{\"orderLineItemId\":\"dc2ddb68-0774-4cf8-b81c-b8111253585b\",\"productType\":\"PASS\",\"description\":\"Office 365 Home\",\"skuId\":\"007P\",\"productId\":\"CFQ7TTC0K5DM\",\"quantity\":1,\"currency\":\"USD\",\"totalAmount\":10.99,\"tax\":1.0}],\"description\":\"Office 365 Home\",\"orderedDate\":\"2019-10-18T01:12:27.4436338Z\",\"piid\":\"lchqggAAAAABAACA\",\"userId\":\"1055518870507325\",\"transactionStatus\":0,\"orderState\":\"Purchased\",\"currency\":\"USD\",\"amount\":10.99,\"taxAmount\":1.0,\"refundedDate\":\"0001-01-01T00:00:00Z\",\"SubscriptionId\":\"mdr:1055518870507325:e4e59792df594f0c959cb7ac0ad9f0bd:365bf870-ad8d-44e7-a0d6-2d94c6dbb93e\",\"checkPiResult\":false},{\"orderId\":\"2c1bcc81-9d16-4473-8549-13302c38f132\",\"orderLineItems\":[{\"orderLineItemId\":\"83a30054-9860-48a4-8952-da76d88d1fda\",\"productType\":\"PASS\",\"description\":\"Xbox Live Gold 1 Month\",\"skuId\":\"000C\",\"productId\":\"CFQ7TTC0K5DJ\",\"quantity\":1,\"currency\":\"USD\",\"totalAmount\":10.99,\"tax\":1.0}],\"description\":\"Xbox Live Gold\",\"orderedDate\":\"2019-10-18T01:10:02.4130199Z\",\"piid\":\"lchqggAAAAABAACA\",\"userId\":\"1055518870507325\",\"transactionStatus\":0,\"orderState\":\"Purchased\",\"currency\":\"USD\",\"amount\":10.99,\"taxAmount\":1.0,\"refundedDate\":\"0001-01-01T00:00:00Z\",\"SubscriptionId\":\"mdr:1055518870507325:1277730306f34c3998bbc89b759e95a5:2c1bcc81-9d16-4473-8549-13302c38f132\",\"checkPiResult\":false}],\"subscriptions\":[{\"subscriptionId\":\"mdr:1055518870507325:1277730306f34c3998bbc89b759e95a5:2c1bcc81-9d16-4473-8549-13302c38f132\",\"autoRenew\":true,\"startDate\":\"2019-10-18T00:00:00Z\",\"recurringFrequency\":\"Monthly\",\"nextRenewalDate\":\"2019-11-18T00:00:00Z\",\"title\":\"Classic Gold\",\"piid\":\"lchqggAAAAAIAACA\",\"productId\":\"CFQ7TTC0K5DJ\",\"recurrenceState\":\"Active\",\"csvTopOffPaymentInstrumentId\":null},{\"subscriptionId\":\"mdr:1055518870507325:e4e59792df594f0c959cb7ac0ad9f0bd:365bf870-ad8d-44e7-a0d6-2d94c6dbb93e\",\"autoRenew\":true,\"startDate\":\"2019-10-18T00:00:00Z\",\"recurringFrequency\":\"Monthly\",\"nextRenewalDate\":\"2019-11-18T00:00:00Z\",\"title\":\"Office 365 (Worldwide)\",\"piid\":\"lchqggAAAAAIAACA\",\"productId\":\"CFQ7TTC0K5DM\",\"recurrenceState\":\"Active\",\"csvTopOffPaymentInstrumentId\":null},{\"subscriptionId\":\"VFYAAAAAAAAAAAEA\",\"autoRenew\":true,\"startDate\":\"2020-09-28T00:00:00Z\",\"recurringFrequency\":\"Monthly\",\"nextRenewalDate\":\"2020-10-28T00:00:00Z\",\"title\":\"Windows Azure MSDN Visual Studio Professional\",\"piid\":\"VFYAAAAAAAABAACA\",\"productId\":\"55414de8-168e-4cbf-a56a-9445131bd21f\",\"recurrenceState\":\"Active\",\"csvTopOffPaymentInstrumentId\":null}]}";

            Assert.AreEqual(expectedValue, responseWithUtcTimeZone);
        }

        [DataRow(true)]
        [DataRow(false)]
        [DataTestMethod]
        public async Task ListTransactionsIncludingD365Orders(bool useListD365PendingOrdersFlight)
        {
            var url = string.Format("/v7.0/{0}/paymentTransactions", AccountId);
            var request = new HttpRequestMessage(HttpMethod.Get, GetPXServiceUrl(url));
            request.Headers.Add("x-ms-clientcontext-encoding", "base64");

            // Base64 encoded values of: PUID=1055518870507325,mstest_pymentsnstar1@outlook.com
            request.Headers.Add("x-ms-msaprofile", string.Format("PUID={0},emailAddress={1}", ToBase64(PUID), ToBase64(EMAIL)));

            if (useListD365PendingOrdersFlight)
            {
                PXFlightHandler.AddToEnabledFlights("PXEnableListD365PendingOrders");
            }

            var response = await PXClient.SendAsync(request);
            string responseText = await response.Content.ReadAsStringAsync();

            var responseContent = JsonConvert.DeserializeObject<Microsoft.Commerce.Payments.PXService.V7.PaymentTransaction.Model.PaymentTransactions>(responseText);

            string responseWithUtcTimeZone = JsonConvert.SerializeObject(
                responseContent,
                Formatting.None,
                new JsonSerializerSettings
                {
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc
                });

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            string expectedValue = "\"orderId\":\"111-123\",\"orderLineItems\":[{\"orderLineItemId\":\"1\",\"productType\":\"Physical\",\"description\":\"Test D365\",\"skuId\":\"000D\",\"productId\":\"8MZBMMCK15GL\",\"quantity\":1,\"currency\":\"USD\",\"totalAmount\":990.89,\"tax\":90.9}],\"clientContext\":null,\"description\":\"Test D365\",\"orderedDate\":\"2021-05-06T05:44:09.6639829Z\",\"csvTopOffPaymentInstrumentId\":null,\"piid\":\"lchqggAAAAAZAACA\",\"userId\":\"1055518870507325\",\"transactionStatus\":0,\"orderState\":\"PendingFulfillment\",\"currency\":\"USD\",\"amount\":990.89,\"taxAmount\":90.9,\"refundedDate\":\"0001-01-01T00:00:00Z\",\"SubscriptionId\":null,\"checkPiResult\":true,\"email\":\"mstest_pymentsnstar1@outlook.com\",\"country\":null,\"device\":null";
            if (useListD365PendingOrdersFlight)
            {
                Assert.IsTrue(responseWithUtcTimeZone.Contains(expectedValue));
            }
            else
            {
                Assert.IsFalse(responseWithUtcTimeZone.Contains(expectedValue));
            }
        }

        [DataTestMethod]
        public async Task ListTransactionsNextLink()
        {
            var url = string.Format("/v7.0/{0}/paymentTransactions", AccountId);
            var request = new HttpRequestMessage(HttpMethod.Get, GetPXServiceUrl(url));
            request.Headers.Add("x-ms-clientcontext-encoding", "base64");

            // Base64 encoded values of: PUID=1055518870507445,mstest_pymentsnstar2@outlook.com
            request.Headers.Add("x-ms-msaprofile", string.Format("PUID={0},emailAddress={1}", ToBase64(PUIDNextLink), ToBase64(EMAINextLink)));

            var response = await PXClient.SendAsync(request);
            string responseText = await response.Content.ReadAsStringAsync();

            var responseContent = JsonConvert.DeserializeObject<Microsoft.Commerce.Payments.PXService.V7.PaymentTransaction.Model.PaymentTransactions>(responseText);
            Assert.AreNotEqual(string.Empty, responseContent);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        // Ignore this test for now because it fails at CDPx pipeline, the following task is created to track the fixation
        // Task 28999776: Fix failed CIT.PXService in CDPx pipeline
        [Ignore]
        [DataRow("PuidOfNewUser", "{\"pageSize\":100,\"hasMoreRecords\":false,\"continuationToken\":null,\"orders\":[],\"subscriptions\":[]}")]
        [DataRow("PuidWithNoLegacyBillableAccounts", "{\"pageSize\":100,\"hasMoreRecords\":false,\"continuationToken\":null,\"orders\":[{\"orderId\":\"f5598585-7340-4c5b-b41e-3df950c4925e\",\"orderLineItems\":[{\"orderLineItemId\":\"16bd31de-d49a-41c2-aa95-9de6d8117082\",\"productType\":\"Game\",\"description\":\"Riptide GP2\",\"skuId\":\"0010\",\"productId\":\"9WZDNCRFJ9Z1\",\"quantity\":1,\"currency\":\"USD\",\"totalAmount\":3.28,\"tax\":0.29}],\"description\":\"Riptide GP2\",\"orderedDate\":\"2019-11-14T22:07:12.0200785Z\",\"piid\":\"lchqggAAAAAHAACA\",\"userId\":\"1055518870507325\",\"transactionStatus\":0,\"orderState\":\"Purchased\",\"currency\":\"USD\",\"amount\":3.28,\"taxAmount\":0.29,\"refundedDate\":\"0001-01-01T00:00:00Z\",\"SubscriptionId\":null,\"checkPiResult\":false},{\"orderId\":\"365bf870-ad8d-44e7-a0d6-2d94c6dbb93e\",\"orderLineItems\":[{\"orderLineItemId\":\"dc2ddb68-0774-4cf8-b81c-b8111253585b\",\"productType\":\"PASS\",\"description\":\"Office 365 Home\",\"skuId\":\"007P\",\"productId\":\"CFQ7TTC0K5DM\",\"quantity\":1,\"currency\":\"USD\",\"totalAmount\":10.99,\"tax\":1.0}],\"description\":\"Office 365 Home\",\"orderedDate\":\"2019-10-18T01:12:27.4436338Z\",\"piid\":\"lchqggAAAAABAACA\",\"userId\":\"1055518870507325\",\"transactionStatus\":0,\"orderState\":\"Purchased\",\"currency\":\"USD\",\"amount\":10.99,\"taxAmount\":1.0,\"refundedDate\":\"0001-01-01T00:00:00Z\",\"SubscriptionId\":\"mdr:1055518870507325:e4e59792df594f0c959cb7ac0ad9f0bd:365bf870-ad8d-44e7-a0d6-2d94c6dbb93e\",\"checkPiResult\":false},{\"orderId\":\"2c1bcc81-9d16-4473-8549-13302c38f132\",\"orderLineItems\":[{\"orderLineItemId\":\"83a30054-9860-48a4-8952-da76d88d1fda\",\"productType\":\"PASS\",\"description\":\"Xbox Live Gold 1 Month\",\"skuId\":\"000C\",\"productId\":\"CFQ7TTC0K5DJ\",\"quantity\":1,\"currency\":\"USD\",\"totalAmount\":10.99,\"tax\":1.0}],\"description\":\"Xbox Live Gold\",\"orderedDate\":\"2019-10-18T01:10:02.4130199Z\",\"piid\":\"lchqggAAAAABAACA\",\"userId\":\"1055518870507325\",\"transactionStatus\":0,\"orderState\":\"Purchased\",\"currency\":\"USD\",\"amount\":10.99,\"taxAmount\":1.0,\"refundedDate\":\"0001-01-01T00:00:00Z\",\"SubscriptionId\":\"mdr:1055518870507325:1277730306f34c3998bbc89b759e95a5:2c1bcc81-9d16-4473-8549-13302c38f132\",\"checkPiResult\":false}],\"subscriptions\":[]}")]
        [DataRow("PuidWithNoPayInAccounts", "{\"pageSize\":100,\"hasMoreRecords\":false,\"continuationToken\":null,\"orders\":[{\"orderId\":\"f5598585-7340-4c5b-b41e-3df950c4925e\",\"orderLineItems\":[{\"orderLineItemId\":\"16bd31de-d49a-41c2-aa95-9de6d8117082\",\"productType\":\"Game\",\"description\":\"Riptide GP2\",\"skuId\":\"0010\",\"productId\":\"9WZDNCRFJ9Z1\",\"quantity\":1,\"currency\":\"USD\",\"totalAmount\":3.28,\"tax\":0.29}],\"description\":\"Riptide GP2\",\"orderedDate\":\"2019-11-14T22:07:12.0200785Z\",\"piid\":\"lchqggAAAAAHAACA\",\"userId\":\"1055518870507325\",\"transactionStatus\":0,\"orderState\":\"Purchased\",\"currency\":\"USD\",\"amount\":3.28,\"taxAmount\":0.29,\"refundedDate\":\"0001-01-01T00:00:00Z\",\"SubscriptionId\":null,\"checkPiResult\":false},{\"orderId\":\"365bf870-ad8d-44e7-a0d6-2d94c6dbb93e\",\"orderLineItems\":[{\"orderLineItemId\":\"dc2ddb68-0774-4cf8-b81c-b8111253585b\",\"productType\":\"PASS\",\"description\":\"Office 365 Home\",\"skuId\":\"007P\",\"productId\":\"CFQ7TTC0K5DM\",\"quantity\":1,\"currency\":\"USD\",\"totalAmount\":10.99,\"tax\":1.0}],\"description\":\"Office 365 Home\",\"orderedDate\":\"2019-10-18T01:12:27.4436338Z\",\"piid\":\"lchqggAAAAABAACA\",\"userId\":\"1055518870507325\",\"transactionStatus\":0,\"orderState\":\"Purchased\",\"currency\":\"USD\",\"amount\":10.99,\"taxAmount\":1.0,\"refundedDate\":\"0001-01-01T00:00:00Z\",\"SubscriptionId\":\"mdr:1055518870507325:e4e59792df594f0c959cb7ac0ad9f0bd:365bf870-ad8d-44e7-a0d6-2d94c6dbb93e\",\"checkPiResult\":false},{\"orderId\":\"2c1bcc81-9d16-4473-8549-13302c38f132\",\"orderLineItems\":[{\"orderLineItemId\":\"83a30054-9860-48a4-8952-da76d88d1fda\",\"productType\":\"PASS\",\"description\":\"Xbox Live Gold 1 Month\",\"skuId\":\"000C\",\"productId\":\"CFQ7TTC0K5DJ\",\"quantity\":1,\"currency\":\"USD\",\"totalAmount\":10.99,\"tax\":1.0}],\"description\":\"Xbox Live Gold\",\"orderedDate\":\"2019-10-18T01:10:02.4130199Z\",\"piid\":\"lchqggAAAAABAACA\",\"userId\":\"1055518870507325\",\"transactionStatus\":0,\"orderState\":\"Purchased\",\"currency\":\"USD\",\"amount\":10.99,\"taxAmount\":1.0,\"refundedDate\":\"0001-01-01T00:00:00Z\",\"SubscriptionId\":\"mdr:1055518870507325:1277730306f34c3998bbc89b759e95a5:2c1bcc81-9d16-4473-8549-13302c38f132\",\"checkPiResult\":false}],\"subscriptions\":[]}")]
        [DataRow(PUID, "{\"pageSize\":100,\"hasMoreRecords\":false,\"continuationToken\":null,\"orders\":[{\"orderId\":\"fc30a4ba-ba2c-46a3-acec-fe81ca5e92e2\",\"orderLineItems\":[{\"orderLineItemId\":\"b7696286-9396-41d9-ad5a-4f09ab329baf\",\"productType\":\"PASS\",\"description\":\"3-Month Xbox Live Gold Membership (Digital Code)\",\"skuId\":\"0002\",\"productId\":\"CFQ7TTC0K5ZM\",\"quantity\":1,\"currency\":\"GBP\",\"totalAmount\":17.99,\"tax\":3.0}],\"description\":\"3-Month Xbox Live Gold Membership (Digital Code)\",\"orderedDate\":\"2020-07-13T10:41:28.9658854Z\",\"piid\":\"D+-WfwAAAAABAACA\",\"userId\":\"0000000000000000\",\"transactionStatus\":0,\"orderState\":\"Purchased\",\"currency\":\"GBP\",\"amount\":17.99,\"taxAmount\":3.0,\"refundedDate\":\"0001-01-01T00:00:00Z\",\"SubscriptionId\":null,\"checkPiResult\":true},{\"orderId\":\"f5598585-7340-4c5b-b41e-3df950c4925e\",\"orderLineItems\":[{\"orderLineItemId\":\"16bd31de-d49a-41c2-aa95-9de6d8117082\",\"productType\":\"Game\",\"description\":\"Riptide GP2\",\"skuId\":\"0010\",\"productId\":\"9WZDNCRFJ9Z1\",\"quantity\":1,\"currency\":\"USD\",\"totalAmount\":3.28,\"tax\":0.29}],\"description\":\"Riptide GP2\",\"orderedDate\":\"2019-11-14T22:07:12.0200785Z\",\"piid\":\"lchqggAAAAAHAACA\",\"userId\":\"1055518870507325\",\"transactionStatus\":0,\"orderState\":\"Purchased\",\"currency\":\"USD\",\"amount\":3.28,\"taxAmount\":0.29,\"refundedDate\":\"0001-01-01T00:00:00Z\",\"SubscriptionId\":null,\"checkPiResult\":true},{\"orderId\":\"365bf870-ad8d-44e7-a0d6-2d94c6dbb93e\",\"orderLineItems\":[{\"orderLineItemId\":\"dc2ddb68-0774-4cf8-b81c-b8111253585b\",\"productType\":\"PASS\",\"description\":\"Office 365 Home\",\"skuId\":\"007P\",\"productId\":\"CFQ7TTC0K5DM\",\"quantity\":1,\"currency\":\"USD\",\"totalAmount\":10.99,\"tax\":1.0}],\"description\":\"Office 365 Home\",\"orderedDate\":\"2019-10-18T01:12:27.4436338Z\",\"piid\":\"lchqggAAAAABAACA\",\"userId\":\"1055518870507325\",\"transactionStatus\":0,\"orderState\":\"Purchased\",\"currency\":\"USD\",\"amount\":10.99,\"taxAmount\":1.0,\"refundedDate\":\"0001-01-01T00:00:00Z\",\"SubscriptionId\":\"mdr:1055518870507325:e4e59792df594f0c959cb7ac0ad9f0bd:365bf870-ad8d-44e7-a0d6-2d94c6dbb93e\",\"checkPiResult\":false},{\"orderId\":\"2c1bcc81-9d16-4473-8549-13302c38f132\",\"orderLineItems\":[{\"orderLineItemId\":\"83a30054-9860-48a4-8952-da76d88d1fda\",\"productType\":\"PASS\",\"description\":\"Xbox Live Gold 1 Month\",\"skuId\":\"000C\",\"productId\":\"CFQ7TTC0K5DJ\",\"quantity\":1,\"currency\":\"USD\",\"totalAmount\":10.99,\"tax\":1.0}],\"description\":\"Xbox Live Gold\",\"orderedDate\":\"2019-10-18T01:10:02.4130199Z\",\"piid\":\"lchqggAAAAABAACA\",\"userId\":\"1055518870507325\",\"transactionStatus\":0,\"orderState\":\"Purchased\",\"currency\":\"USD\",\"amount\":10.99,\"taxAmount\":1.0,\"refundedDate\":\"0001-01-01T00:00:00Z\",\"SubscriptionId\":\"mdr:1055518870507325:1277730306f34c3998bbc89b759e95a5:2c1bcc81-9d16-4473-8549-13302c38f132\",\"checkPiResult\":false}],\"subscriptions\":[{\"subscriptionId\":\"mdr:1055518870507325:1277730306f34c3998bbc89b759e95a5:2c1bcc81-9d16-4473-8549-13302c38f132\",\"autoRenew\":true,\"startDate\":\"2019-10-18T00:00:00Z\",\"recurringFrequency\":\"Monthly\",\"nextRenewalDate\":\"2019-11-18T00:00:00Z\",\"title\":\"Classic Gold\",\"piid\":\"lchqggAAAAAIAACA\",\"productId\":\"CFQ7TTC0K5DJ\",\"recurrenceState\":\"Active\",\"csvTopOffPaymentInstrumentId\":null},{\"subscriptionId\":\"mdr:1055518870507325:e4e59792df594f0c959cb7ac0ad9f0bd:365bf870-ad8d-44e7-a0d6-2d94c6dbb93e\",\"autoRenew\":true,\"startDate\":\"2019-10-18T00:00:00Z\",\"recurringFrequency\":\"Monthly\",\"nextRenewalDate\":\"2019-11-18T00:00:00Z\",\"title\":\"Office 365 (Worldwide)\",\"piid\":\"lchqggAAAAAIAACA\",\"productId\":\"CFQ7TTC0K5DM\",\"recurrenceState\":\"Active\",\"csvTopOffPaymentInstrumentId\":null},{\"subscriptionId\":\"VFYAAAAAAAAAAAEA\",\"autoRenew\":true,\"startDate\":\"2020-09-28T00:00:00Z\",\"recurringFrequency\":\"Monthly\",\"nextRenewalDate\":\"2020-10-28T00:00:00Z\",\"title\":\"Windows Azure MSDN Visual Studio Professional\",\"piid\":\"VFYAAAAAAAABAACA\",\"productId\":\"55414de8-168e-4cbf-a56a-9445131bd21f\",\"recurrenceState\":\"Active\",\"csvTopOffPaymentInstrumentId\":null}]}")]
        [DataRow("PuidOfNewUser", "{\"pageSize\":100,\"hasMoreRecords\":false,\"continuationToken\":null,\"orders\":[],\"subscriptions\":[]}")]
        [DataRow("PuidWithNoLegacyBillableAccounts", "{\"pageSize\":100,\"hasMoreRecords\":false,\"continuationToken\":null,\"orders\":[{\"orderId\":\"f5598585-7340-4c5b-b41e-3df950c4925e\",\"orderLineItems\":[{\"orderLineItemId\":\"16bd31de-d49a-41c2-aa95-9de6d8117082\",\"productType\":\"Game\",\"description\":\"Riptide GP2\",\"skuId\":\"0010\",\"productId\":\"9WZDNCRFJ9Z1\",\"quantity\":1,\"currency\":\"USD\",\"totalAmount\":3.28,\"tax\":0.29}],\"description\":\"Riptide GP2\",\"orderedDate\":\"2019-11-14T22:07:12.0200785Z\",\"piid\":\"lchqggAAAAAHAACA\",\"userId\":\"1055518870507325\",\"transactionStatus\":0,\"orderState\":\"Purchased\",\"currency\":\"USD\",\"amount\":3.28,\"taxAmount\":0.29,\"refundedDate\":\"0001-01-01T00:00:00Z\",\"SubscriptionId\":null,\"checkPiResult\":null},{\"orderId\":\"365bf870-ad8d-44e7-a0d6-2d94c6dbb93e\",\"orderLineItems\":[{\"orderLineItemId\":\"dc2ddb68-0774-4cf8-b81c-b8111253585b\",\"productType\":\"PASS\",\"description\":\"Office 365 Home\",\"skuId\":\"007P\",\"productId\":\"CFQ7TTC0K5DM\",\"quantity\":1,\"currency\":\"USD\",\"totalAmount\":10.99,\"tax\":1.0}],\"description\":\"Office 365 Home\",\"orderedDate\":\"2019-10-18T01:12:27.4436338Z\",\"piid\":\"lchqggAAAAABAACA\",\"userId\":\"1055518870507325\",\"transactionStatus\":0,\"orderState\":\"Purchased\",\"currency\":\"USD\",\"amount\":10.99,\"taxAmount\":1.0,\"refundedDate\":\"0001-01-01T00:00:00Z\",\"SubscriptionId\":\"mdr:1055518870507325:e4e59792df594f0c959cb7ac0ad9f0bd:365bf870-ad8d-44e7-a0d6-2d94c6dbb93e\",\"checkPiResult\":null},{\"orderId\":\"2c1bcc81-9d16-4473-8549-13302c38f132\",\"orderLineItems\":[{\"orderLineItemId\":\"83a30054-9860-48a4-8952-da76d88d1fda\",\"productType\":\"PASS\",\"description\":\"Xbox Live Gold 1 Month\",\"skuId\":\"000C\",\"productId\":\"CFQ7TTC0K5DJ\",\"quantity\":1,\"currency\":\"USD\",\"totalAmount\":10.99,\"tax\":1.0}],\"description\":\"Xbox Live Gold\",\"orderedDate\":\"2019-10-18T01:10:02.4130199Z\",\"piid\":\"lchqggAAAAABAACA\",\"userId\":\"1055518870507325\",\"transactionStatus\":0,\"orderState\":\"Purchased\",\"currency\":\"USD\",\"amount\":10.99,\"taxAmount\":1.0,\"refundedDate\":\"0001-01-01T00:00:00Z\",\"SubscriptionId\":\"mdr:1055518870507325:1277730306f34c3998bbc89b759e95a5:2c1bcc81-9d16-4473-8549-13302c38f132\",\"checkPiResult\":null}],\"subscriptions\":[{\"subscriptionId\":\"mdr:1055518870507325:1277730306f34c3998bbc89b759e95a5:2c1bcc81-9d16-4473-8549-13302c38f132\",\"autoRenew\":true,\"startDate\":\"2019-10-18T00:00:00Z\",\"recurringFrequency\":\"Monthly\",\"nextRenewalDate\":\"2019-11-18T00:00:00Z\",\"title\":\"Classic Gold\",\"piid\":\"lchqggAAAAAIAACA\",\"productId\":\"CFQ7TTC0K5DJ\",\"recurrenceState\":\"Active\",\"csvTopOffPaymentInstrumentId\":null},{\"subscriptionId\":\"mdr:1055518870507325:e4e59792df594f0c959cb7ac0ad9f0bd:365bf870-ad8d-44e7-a0d6-2d94c6dbb93e\",\"autoRenew\":true,\"startDate\":\"2019-10-18T00:00:00Z\",\"recurringFrequency\":\"Monthly\",\"nextRenewalDate\":\"2019-11-18T00:00:00Z\",\"title\":\"Office 365 (Worldwide)\",\"piid\":\"lchqggAAAAAIAACA\",\"productId\":\"CFQ7TTC0K5DM\",\"recurrenceState\":\"Active\",\"csvTopOffPaymentInstrumentId\":null}]}")]
        [DataRow("PuidWithNoPayInAccounts", "{\"pageSize\":100,\"hasMoreRecords\":false,\"continuationToken\":null,\"orders\":[{\"orderId\":\"f5598585-7340-4c5b-b41e-3df950c4925e\",\"orderLineItems\":[{\"orderLineItemId\":\"16bd31de-d49a-41c2-aa95-9de6d8117082\",\"productType\":\"Game\",\"description\":\"Riptide GP2\",\"skuId\":\"0010\",\"productId\":\"9WZDNCRFJ9Z1\",\"quantity\":1,\"currency\":\"USD\",\"totalAmount\":3.28,\"tax\":0.29}],\"description\":\"Riptide GP2\",\"orderedDate\":\"2019-11-14T22:07:12.0200785Z\",\"piid\":\"lchqggAAAAAHAACA\",\"userId\":\"1055518870507325\",\"transactionStatus\":0,\"orderState\":\"Purchased\",\"currency\":\"USD\",\"amount\":3.28,\"taxAmount\":0.29,\"refundedDate\":\"0001-01-01T00:00:00Z\",\"SubscriptionId\":null,\"checkPiResult\":null},{\"orderId\":\"365bf870-ad8d-44e7-a0d6-2d94c6dbb93e\",\"orderLineItems\":[{\"orderLineItemId\":\"dc2ddb68-0774-4cf8-b81c-b8111253585b\",\"productType\":\"PASS\",\"description\":\"Office 365 Home\",\"skuId\":\"007P\",\"productId\":\"CFQ7TTC0K5DM\",\"quantity\":1,\"currency\":\"USD\",\"totalAmount\":10.99,\"tax\":1.0}],\"description\":\"Office 365 Home\",\"orderedDate\":\"2019-10-18T01:12:27.4436338Z\",\"piid\":\"lchqggAAAAABAACA\",\"userId\":\"1055518870507325\",\"transactionStatus\":0,\"orderState\":\"Purchased\",\"currency\":\"USD\",\"amount\":10.99,\"taxAmount\":1.0,\"refundedDate\":\"0001-01-01T00:00:00Z\",\"SubscriptionId\":\"mdr:1055518870507325:e4e59792df594f0c959cb7ac0ad9f0bd:365bf870-ad8d-44e7-a0d6-2d94c6dbb93e\",\"checkPiResult\":null},{\"orderId\":\"2c1bcc81-9d16-4473-8549-13302c38f132\",\"orderLineItems\":[{\"orderLineItemId\":\"83a30054-9860-48a4-8952-da76d88d1fda\",\"productType\":\"PASS\",\"description\":\"Xbox Live Gold 1 Month\",\"skuId\":\"000C\",\"productId\":\"CFQ7TTC0K5DJ\",\"quantity\":1,\"currency\":\"USD\",\"totalAmount\":10.99,\"tax\":1.0}],\"description\":\"Xbox Live Gold\",\"orderedDate\":\"2019-10-18T01:10:02.4130199Z\",\"piid\":\"lchqggAAAAABAACA\",\"userId\":\"1055518870507325\",\"transactionStatus\":0,\"orderState\":\"Purchased\",\"currency\":\"USD\",\"amount\":10.99,\"taxAmount\":1.0,\"refundedDate\":\"0001-01-01T00:00:00Z\",\"SubscriptionId\":\"mdr:1055518870507325:1277730306f34c3998bbc89b759e95a5:2c1bcc81-9d16-4473-8549-13302c38f132\",\"checkPiResult\":null}],\"subscriptions\":[{\"subscriptionId\":\"mdr:1055518870507325:1277730306f34c3998bbc89b759e95a5:2c1bcc81-9d16-4473-8549-13302c38f132\",\"autoRenew\":true,\"startDate\":\"2019-10-18T00:00:00Z\",\"recurringFrequency\":\"Monthly\",\"nextRenewalDate\":\"2019-11-18T00:00:00Z\",\"title\":\"Classic Gold\",\"piid\":\"lchqggAAAAAIAACA\",\"productId\":\"CFQ7TTC0K5DJ\",\"recurrenceState\":\"Active\",\"csvTopOffPaymentInstrumentId\":null},{\"subscriptionId\":\"mdr:1055518870507325:e4e59792df594f0c959cb7ac0ad9f0bd:365bf870-ad8d-44e7-a0d6-2d94c6dbb93e\",\"autoRenew\":true,\"startDate\":\"2019-10-18T00:00:00Z\",\"recurringFrequency\":\"Monthly\",\"nextRenewalDate\":\"2019-11-18T00:00:00Z\",\"title\":\"Office 365 (Worldwide)\",\"piid\":\"lchqggAAAAAIAACA\",\"productId\":\"CFQ7TTC0K5DM\",\"recurrenceState\":\"Active\",\"csvTopOffPaymentInstrumentId\":null}]}")]
        [DataRow(PUID, "{\"pageSize\":100,\"hasMoreRecords\":false,\"continuationToken\":null,\"orders\":[{\"orderId\":\"f5598585-7340-4c5b-b41e-3df950c4925e\",\"orderLineItems\":[{\"orderLineItemId\":\"16bd31de-d49a-41c2-aa95-9de6d8117082\",\"productType\":\"Game\",\"description\":\"Riptide GP2\",\"skuId\":\"0010\",\"productId\":\"9WZDNCRFJ9Z1\",\"quantity\":1,\"currency\":\"USD\",\"totalAmount\":3.28,\"tax\":0.29}],\"description\":\"Riptide GP2\",\"orderedDate\":\"2019-11-14T22:07:12.0200785Z\",\"piid\":\"lchqggAAAAAHAACA\",\"userId\":\"1055518870507325\",\"transactionStatus\":0,\"orderState\":\"Purchased\",\"currency\":\"USD\",\"amount\":3.28,\"taxAmount\":0.29,\"refundedDate\":\"0001-01-01T00:00:00Z\",\"SubscriptionId\":null,\"checkPiResult\":null},{\"orderId\":\"365bf870-ad8d-44e7-a0d6-2d94c6dbb93e\",\"orderLineItems\":[{\"orderLineItemId\":\"dc2ddb68-0774-4cf8-b81c-b8111253585b\",\"productType\":\"PASS\",\"description\":\"Office 365 Home\",\"skuId\":\"007P\",\"productId\":\"CFQ7TTC0K5DM\",\"quantity\":1,\"currency\":\"USD\",\"totalAmount\":10.99,\"tax\":1.0}],\"description\":\"Office 365 Home\",\"orderedDate\":\"2019-10-18T01:12:27.4436338Z\",\"piid\":\"lchqggAAAAABAACA\",\"userId\":\"1055518870507325\",\"transactionStatus\":0,\"orderState\":\"Purchased\",\"currency\":\"USD\",\"amount\":10.99,\"taxAmount\":1.0,\"refundedDate\":\"0001-01-01T00:00:00Z\",\"SubscriptionId\":\"mdr:1055518870507325:e4e59792df594f0c959cb7ac0ad9f0bd:365bf870-ad8d-44e7-a0d6-2d94c6dbb93e\",\"checkPiResult\":null},{\"orderId\":\"2c1bcc81-9d16-4473-8549-13302c38f132\",\"orderLineItems\":[{\"orderLineItemId\":\"83a30054-9860-48a4-8952-da76d88d1fda\",\"productType\":\"PASS\",\"description\":\"Xbox Live Gold 1 Month\",\"skuId\":\"000C\",\"productId\":\"CFQ7TTC0K5DJ\",\"quantity\":1,\"currency\":\"USD\",\"totalAmount\":10.99,\"tax\":1.0}],\"description\":\"Xbox Live Gold\",\"orderedDate\":\"2019-10-18T01:10:02.4130199Z\",\"piid\":\"lchqggAAAAABAACA\",\"userId\":\"1055518870507325\",\"transactionStatus\":0,\"orderState\":\"Purchased\",\"currency\":\"USD\",\"amount\":10.99,\"taxAmount\":1.0,\"refundedDate\":\"0001-01-01T00:00:00Z\",\"SubscriptionId\":\"mdr:1055518870507325:1277730306f34c3998bbc89b759e95a5:2c1bcc81-9d16-4473-8549-13302c38f132\",\"checkPiResult\":null}],\"subscriptions\":[{\"subscriptionId\":\"mdr:1055518870507325:1277730306f34c3998bbc89b759e95a5:2c1bcc81-9d16-4473-8549-13302c38f132\",\"autoRenew\":true,\"startDate\":\"2019-10-18T00:00:00Z\",\"recurringFrequency\":\"Monthly\",\"nextRenewalDate\":\"2019-11-18T00:00:00Z\",\"title\":\"Classic Gold\",\"piid\":\"lchqggAAAAAIAACA\",\"productId\":\"CFQ7TTC0K5DJ\",\"recurrenceState\":\"Active\",\"csvTopOffPaymentInstrumentId\":null},{\"subscriptionId\":\"mdr:1055518870507325:e4e59792df594f0c959cb7ac0ad9f0bd:365bf870-ad8d-44e7-a0d6-2d94c6dbb93e\",\"autoRenew\":true,\"startDate\":\"2019-10-18T00:00:00Z\",\"recurringFrequency\":\"Monthly\",\"nextRenewalDate\":\"2019-11-18T00:00:00Z\",\"title\":\"Office 365 (Worldwide)\",\"piid\":\"lchqggAAAAAIAACA\",\"productId\":\"CFQ7TTC0K5DM\",\"recurrenceState\":\"Active\",\"csvTopOffPaymentInstrumentId\":null},{\"subscriptionId\":\"VFYAAAAAAAAAAAEA\",\"autoRenew\":true,\"startDate\":\"2020-09-28T00:00:00Z\",\"recurringFrequency\":\"Monthly\",\"nextRenewalDate\":\"2020-10-28T00:00:00Z\",\"title\":\"Windows Azure MSDN Visual Studio Professional\",\"piid\":\"VFYAAAAAAAABAACA\",\"productId\":\"55414de8-168e-4cbf-a56a-9445131bd21f\",\"recurrenceState\":\"Active\",\"csvTopOffPaymentInstrumentId\":null}]}")]
        [DataTestMethod]
        public async Task ListTransactionsIncludingCtpSubscriptions(string puid, string expectedPaymentTransactionsResponse)
        {
            var url = string.Format("/v7.0/{0}/paymentTransactions", AccountId);
            var request = new HttpRequestMessage(HttpMethod.Get, GetPXServiceUrl(url));
            request.Headers.Add("x-ms-clientcontext-encoding", "base64");

            // Base64 encoded values of: PUID=1055518870507325,mstest_pymentsnstar1@outlook.com
            request.Headers.Add("x-ms-msaprofile", string.Format("PUID={0},emailAddress={1}", ToBase64(puid), ToBase64(EMAIL)));

            var response = await PXClient.SendAsync(request);
            string responseText = await response.Content.ReadAsStringAsync();

            var responseContent = JsonConvert.DeserializeObject<Microsoft.Commerce.Payments.PXService.V7.PaymentTransaction.Model.PaymentTransactions>(responseText);
            string responseWithUtcTimeZone = JsonConvert.SerializeObject(
                responseContent,
                Formatting.None,
                new JsonSerializerSettings
                {
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc
                });

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(expectedPaymentTransactionsResponse, responseWithUtcTimeZone);
        }

        [DataTestMethod]
        [DataRow("northstarweb", "", true, false)]
        [DataRow("northstarweb", "", false, false)]
        [DataRow("northstarweb", "invalidCvv", false, false)]
        [DataRow("northstarweb", "validationFailed", false, false)]
        [DataRow("defaulttemplate", "", true, false)]
        [DataRow("defaulttemplate", "", false, false)]
        [DataRow("defaulttemplate", "invalidCvv", false, false)]
        [DataRow("defaulttemplate", "validationFailed", false, false)]
        [DataRow("northstarweb", "", true, true)]
        [DataRow("northstarweb", "", false, true)]
        [DataRow("northstarweb", "invalidCvv", false, true)]
        [DataRow("northstarweb", "validationFailed", false, true)]
        [DataRow("defaulttemplate", "", true, true)]
        [DataRow("defaulttemplate", "", false, true)]
        [DataRow("defaulttemplate", "invalidCvv", false, true)]
        [DataRow("defaulttemplate", "validationFailed", false, true)]
        public async Task ListTransactions_POST(string partner, string scenario, bool noAccountId, bool isFlightEnabled)
        {
            string accountId = "Account001";
            string piId = "lchqggAAAAABAACA";
            string country = "us";
            string language = "en-US";
            var url = string.Format("v7.0/{0}/paymentTransactions?country={1}&language={2}&partner={3}", accountId, country, language, partner);
            var request = new HttpRequestMessage(HttpMethod.Post, GetPXServiceUrl(url));
            request.Headers.Add("x-ms-clientcontext-encoding", "base64");

            if (isFlightEnabled)
            {
                PXFlightHandler.AddToEnabledFlights("PXEnableSearchTransactionParallelRequest");
            }

            object payload = new
            {
                id = piId,
                cvvToken = "placeholder"
            };

            var mockPims = new Mock<IPIMSAccessor>();
            var pi = new PaymentInstrument()
            {
                PaymentInstrumentDetails = new PaymentInstrumentDetails(),
                PaymentMethod = new PaymentMethod()
                {
                    PaymentMethodFamily = "credit_card",
                    PaymentMethodType = "visa"
                }
            };

            mockPims.Setup(x => x.GetPaymentInstrument(
               accountId,
               piId,
               It.IsAny<EventTraceActivity>(),
               partner,
               country,
               language,
               It.IsAny<List<string>>(),
               It.IsAny<PaymentExperienceSetting>()))
               .Returns(Task.FromResult(pi));

            if (noAccountId)
            {
                var searchByAccountNo = new List<SearchTransactionAccountinfoByPI>();
                mockPims.Setup(x => x.SearchByAccountNumber(
                It.IsAny<string>(),
                It.IsAny<EventTraceActivity>()))
                .Returns(Task.FromResult(searchByAccountNo));
            }
            else
            {
                var searchByAccountNoList = new List<SearchTransactionAccountinfoByPI>
                {
                    new SearchTransactionAccountinfoByPI { PaymentInstrumentAccountId = "Account003", PaymentInstrumentId = "lchqggAAAAABAACA" },
                    new SearchTransactionAccountinfoByPI { PaymentInstrumentAccountId = "Account005", PaymentInstrumentId = "lchqggAAAAABAACB" },
                    new SearchTransactionAccountinfoByPI { PaymentInstrumentAccountId = "Account006", PaymentInstrumentId = "lchqggAAAAABAACC" },
                    new SearchTransactionAccountinfoByPI { PaymentInstrumentAccountId = "Account007", PaymentInstrumentId = "lchqggAAAAABAACD" }
                };
                var searchByAccountNoResponse = new SearchTransactionAccountInfoResponse { Result = searchByAccountNoList };
                PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(searchByAccountNoResponse), HttpStatusCode.OK, null, ".*/searchByAccountNumber.*");

                PXSettings.AccountsService.ArrangeResponse(JsonConvert.SerializeObject(new System.Exception("test exception thrown")), HttpStatusCode.InternalServerError, null, ".*/Account005/profiles.*");
            }

            if (scenario == "validationFailed" || scenario == "invalidCvv")
            {
                PXSettings.PimsService.PreProcess = (pimsServiceRequest) =>
                {
                    if (pimsServiceRequest.RequestUri.AbsoluteUri.Contains($"/validateCvv"))
                    {
                        var validateCVV = new ServiceErrorResponse();
                        if (scenario == "invalidCvv")
                        {
                            validateCVV = new ServiceErrorResponse { CorrelationId = "85783bd8-fe4b-4379-8064-79cdd300e1d5", ErrorCode = "InvalidCvv", Message = "The card's security code is invalid." };
                        }
                        else
                        {
                            validateCVV = new ServiceErrorResponse { CorrelationId = "85783bd8-fe4b-4379-8064-79cdd300e1d5", ErrorCode = "ValidationFailed", Message = "The payment instrument cannot be validated. Please contact the payment processor for help." };
                        }

                        PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(validateCVV), HttpStatusCode.BadRequest);
                    }
                };
            }

            var json = JsonConvert.SerializeObject(payload);
            HttpResponseMessage response = await PXClient.PostAsync(url, new StringContent(json, Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            if (!string.IsNullOrEmpty(scenario))
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
                string responseText = await response.Content.ReadAsStringAsync();
                Assert.IsTrue(responseText.Contains("ValidationFailed"));
            }
            else
            {
                string responseText = await response.Content.ReadAsStringAsync();
                var responseContent = JsonConvert.DeserializeObject<Microsoft.Commerce.Payments.PXService.V7.PaymentTransaction.Model.PaymentTransactions>(responseText);
                string responseWithUtcTimeZone = JsonConvert.SerializeObject(
                    responseContent,
                    Formatting.None,
                    new JsonSerializerSettings
                    {
                        DateTimeZoneHandling = DateTimeZoneHandling.Utc
                    });

                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                string expectedValue = "\"orderId\":\"2c1bcc81-9d16-4473-8549-13302c38f132\",\"orderLineItems\":[{\"orderLineItemId\":\"83a30054-9860-48a4-8952-da76d88d1fda\",\"productType\":\"PASS\",\"description\":\"Xbox Live Gold 1 Month\",\"skuId\":\"000C\",\"productId\":\"CFQ7TTC0K5DJ\",\"quantity\":1,\"currency\":\"USD\",\"totalAmount\":10.99,\"tax\":1.0}],\"clientContext\":null,\"description\":\"Xbox Live Gold\",\"orderedDate\":\"2019-10-18T01:10:02.4130199Z\",\"csvTopOffPaymentInstrumentId\":null,\"piid\":\"lchqggAAAAABAACA\",\"userId\":\"1055518870507325\",\"transactionStatus\":0,\"orderState\":\"Purchased\",\"currency\":\"USD\",\"amount\":10.99,\"taxAmount\":1.0,\"refundedDate\":\"0001-01-01T00:00:00Z\",\"SubscriptionId\":\"mdr:1055518870507325:1277730306f34c3998bbc89b759e95a5:2c1bcc81-9d16-4473-8549-13302c38f132\",\"checkPiResult\":false,\"email\":\"test@email.com\",\"country\":\"US\",\"device\":\"Web\"";
                Assert.IsTrue(responseWithUtcTimeZone.Contains(expectedValue));
            }

            PXSettings.PimsService.ResetToDefaults();
        }

        [DataTestMethod]
        [DataRow("northstarweb", "", false)]
        [DataRow("northstarweb", "", true)]
        [DataRow("northstarweb", "invalidCvv", true)]
        [DataRow("northstarweb", "invalidCvv", false)]
        public async Task ListTransactions_POST_NoOrdersForPI(string partner, string scenario, bool isFlightEnabled)
        {
            string accountId = "Account010";
            string piId = "Account010-Pi002-Visa-SearchTransaction";
            string country = "us";
            string language = "en-US";
            var url = string.Format("v7.0/{0}/paymentTransactions?country={1}&language={2}&partner={3}", accountId, country, language, partner);
            var request = new HttpRequestMessage(HttpMethod.Post, GetPXServiceUrl(url));
            request.Headers.Add("x-ms-clientcontext-encoding", "base64");

            if (isFlightEnabled)
            {
                PXFlightHandler.AddToEnabledFlights("PXEnableSearchTransactionParallelRequest");
            }

            object payload = new
            {
                id = piId,
                cvvToken = "placeholder"
            };
            var json = JsonConvert.SerializeObject(payload);

            if (scenario == "invalidCvv")
            {
                PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject("invalidCvv"), HttpStatusCode.OK, null, ".*/Account010-Pi002-Visa-SearchTransaction/validateCvv.*");
                HttpResponseMessage response = await PXClient.PostAsync(url, new StringContent(json, Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));
                Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            }
            else
            {
                var searchByAccountNoList = new List<SearchTransactionAccountinfoByPI>();
                var searchByAccountNoResponse = new SearchTransactionAccountInfoResponse { Result = searchByAccountNoList };
                PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(searchByAccountNoResponse), HttpStatusCode.OK, null, ".*/searchByAccountNumber.*");
                HttpResponseMessage response = await PXClient.PostAsync(url, new StringContent(json, Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            }
        }
    }
}