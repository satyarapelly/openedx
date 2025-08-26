// <copyright file="PaymentChallengeTests.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace CIT.PXService.Tests
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.PXService;
    using Microsoft.Commerce.Payments.PXService.Accessors.TransactionDataService;
    using Microsoft.Commerce.Payments.PXService.Model.PurchaseService;
    using Microsoft.Commerce.Payments.PXService.Model.ThreeDSExternalService;
    using Microsoft.Commerce.Payments.PXService.V7;
    using Microsoft.Commerce.Tracing;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;
    using PIMSModel = Microsoft.Commerce.Payments.PimsModel.V4;

    [TestClass]
    public class PaymentChallengeTests
    {
        private const string PifdBaseUrl = "https://pifdbaseurl";
        private const string SessionId = "sessionId";
        private Mock<IPayerAuthServiceAccessor> mockPayerAuthServiceAccessor;
        private Mock<IPIMSAccessor> mockPimsAccessor;
        private Mock<ISessionServiceAccessor> mockSessionServiceAccessor;
        private Mock<IAccountServiceAccessor> mockAccountServiceAccessor;
        private Mock<IPurchaseServiceAccessor> mockPurchaseServiceAccessor;
        private Mock<ITransactionServiceAccessor> mockTransactionServiceAccessor;
        private Mock<ITransactionDataServiceAccessor> mockTransactionDataServiceAccessor;

        [TestInitialize]
        public void TestInitialize()
        {
            mockPayerAuthServiceAccessor = new Mock<IPayerAuthServiceAccessor>();
            mockPimsAccessor = new Mock<IPIMSAccessor>();
            mockSessionServiceAccessor = new Mock<ISessionServiceAccessor>();
            mockAccountServiceAccessor = new Mock<IAccountServiceAccessor>();
            mockPurchaseServiceAccessor = new Mock<IPurchaseServiceAccessor>();
            mockTransactionServiceAccessor = new Mock<ITransactionServiceAccessor>();
            mockTransactionDataServiceAccessor = new Mock<ITransactionDataServiceAccessor>();

            mockPayerAuthServiceAccessor.Setup(x => x.CreatePaymentSessionId(
                It.IsAny<Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.PaymentSessionData>(),
                It.IsAny<EventTraceActivity>()))
                .Returns(Task.FromResult(new Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.PaymentSessionResponse()
                {
                    PaymentSessionId = Guid.NewGuid().ToString()
                }));

            var sessionData = new Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession
            {
                Id = SessionId
            };

            mockSessionServiceAccessor
                .Setup(x => x.GetSessionResourceData<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(It.IsAny<string>(), It.IsAny<EventTraceActivity>()))
                .ReturnsAsync(sessionData);

            mockPurchaseServiceAccessor.Setup(x => x.GetOrder(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<EventTraceActivity>()))
                .Returns(Task.FromResult(JsonConvert.DeserializeObject<Order>("{\"billingInformation\":{\"billingRecordId\":\"string\",\"billingRecordVersion\":0,\"challengeCompletedKind\":\"None\",\"conversionType\":\"RetainExisting\",\"csvTopOffPaymentInstrumentId\":\"string\",\"paymentInstrumentId\":\"string\",\"paymentInstrumentOwner\":{\"identityType\":\"string\",\"identityValue\":\"string\",\"organization\":{\"identityType\":\"string\",\"identityValue\":\"string\"}},\"paymentInstrumentType\":\"None\",\"riskSessionId\":\"string\",\"secondaryPaymentInstrumentId\":\"string\",\"sessionId\":\"string\",\"soldToAddressId\":\"string\",\"taxInfoReference\":\"00000000-0000-0000-0000-000000000000\",\"tokenizedPaymentInstrumentData\":\"string\"},\"bundlePrices\":[{\"instanceId\":\"string\",\"listPrice\":0,\"retailPrice\":299.99}],\"clientContext\":{\"client\":\"string\",\"deviceFamily\":\"string\",\"deviceId\":\"string\",\"deviceType\":\"string\",\"role\":\"string\",\"roleOverride\":\"string\"},\"createdTime\":\"2020-05-15T12:02:37.135Z\",\"currencyCode\":\"string\",\"displayTotalFeeAmount\":0,\"feesOrder\":{\"fees\":[{\"descriptiveCode\":\"string\",\"taxDetails\":[{\"jurisdiction\":\"string\",\"taxAmount\":15.5,\"taxType\":\"string\"}],\"displayTotalAmount\":0,\"totalAmount\":299.99,\"totalTaxAmount\":0,\"unitaryAmount\":0}]},\"friendlyName\":\"string\",\"isInManualReview\":true,\"isPIRequired\":true,\"RequiresPIValidation\":\"string\",\"isUpdatePIAllowed\":true,\"language\":\"string\",\"manualReviewRiskDecision\":\"string\",\"market\":\"string\",\"omniChannelFulfillmentId\":\"string\",\"omniChannelReservationId\":\"string\",\"orderAdditionalMetadata\":\"string\",\"orderId\":\"00000000-0000-0000-0000-000000000000\",\"orderLineItems\":[{\"agent\":{\"identityType\":\"string\",\"identityValue\":\"string\",\"organization\":{\"identityType\":\"string\",\"identityValue\":\"string\"}},\"authorizationPendingReasonCode\":\"string\",\"availabilityId\":\"string\",\"beneficiary\":{\"identityType\":\"string\",\"identityValue\":\"string\",\"organization\":{\"identityType\":\"string\",\"identityValue\":\"string\"}},\"billingState\":\"string\",\"bundleInformation\":{\"componentGroupId\":\"string\",\"instanceId\":\"string\",\"slotType\":\"Undefined\"},\"campaignId\":\"string\",\"currencyCode\":\"string\",\"deliveryCostInformation\":{\"billingStateCode\":\"string\",\"billingStateDescription\":\"Product 123\",\"priceAmount\":0,\"taxAmount\":0,\"refundedPriceAmount\":0,\"refundedTaxAmount\":0},\"deliveryMethodId\":\"string\",\"description\":\"string\",\"devOfferId\":\"string\",\"distributorId\":\"string\",\"feesLineItem\":{\"fees\":[{\"descriptiveCode\":\"string\",\"taxDetails\":[{\"jurisdiction\":\"string\",\"taxAmount\":0,\"taxType\":\"string\"}],\"displayTotalAmount\":0,\"totalAmount\":0,\"totalTaxAmount\":0,\"unitaryAmount\":0}]},\"fulfillmentData\":\"string\",\"fulfillmentDate\":\"2020-05-15T12:02:37.135Z\",\"fulfillmentSessionId\":\"string\",\"fulfillmentState\":\"string\",\"giftee\":{\"recipientId\":\"string\",\"recipientType\":\"string\"},\"isPIRequired\":true,\"RequiresPIValidation\":\"string\",\"isShippingAddressRequired\":true,\"isTaxIncluded\":true,\"lineItemId\":\"string\",\"listPrice\":0,\"optionalCampaignId\":\"string\",\"originalListPrice\":0,\"fulfillmentStates\":[{\"authorizationStateReasonCode\":\"string\",\"cancelable\":true,\"canceled\":true,\"cancelReason\":\"string\",\"fulfillmentQuantityId\":\"string\",\"fulfillmentReturnId\":\"string\",\"fulfillmentReturnType\":\"None\",\"fulfillmentState\":\"None\",\"refundJustifications\":[{\"comment\":\"string\",\"reasonCode\":\"string\",\"riskDecision\":\"string\",\"riskReasonCode\":\"string\",\"userRole\":\"string\"}],\"tokenIdentifier\":\"string\",\"token5x5Value\":\"string\"}],\"billingStates\":[{\"startIndex\":0,\"endIndex\":0,\"billingState\":\"None\",\"billingStateCode\":\"string\",\"billingStateDescription\":\"string\",\"totalChargedAmount\":0,\"totalRefundedAmount\":0,\"totalRefundedTaxAmount\":0,\"totalRefundedFeeAmount\":0,\"totalRefundedFeeTaxAmount\":0}],\"fulfillmentBillingStates\":[{\"billingState\":\"None\",\"count\":0,\"fulfillmentState\":\"None\"}],\"parentalApprovalState\":\"string\",\"promoCode\":{\"assetId\":\"string\",\"assetSource\":\"string\",\"isUnlimitedRedemption\":true,\"name\":\"string\",\"redemptionExpiryDate\":\"2020-05-15T12:02:37.135Z\",\"redemptionStartDate\":\"2020-05-15T12:02:37.135Z\"},\"purchaseRestriction\":{\"askToBuySetting\":\"string\",\"purchaseApprovalState\":\"string\",\"isChild\":true,\"isInFamily\":true},\"payments\":[{\"paymentInstrumentId\":\"string\",\"paymentInstrumentType\":\"string\",\"chargedAmount\":0}],\"productId\":\"product_123\",\"productReturnCategorization\":\"string\",\"productType\":\"string\",\"quantity\":1,\"reservationInformation\":{\"deliverByDates\":{\"additionalProp1\":\"string\",\"additionalProp2\":\"string\",\"additionalProp3\":\"string\"}},\"retailPrice\":299.99,\"shipFromId\":\"string\",\"shipFromAddressId\":\"string\",\"shipToAddressId\":\"string\",\"skuId\":\"30-X9383-94\",\"taxAmount\":15.5,\"taxType\":\"string\",\"title\":\"string\",\"tokenIdentifier\":\"string\",\"token5x5Value\":\"string\",\"totalAmount\":299.99,\"recurrenceId\":\"string\",\"seatBlockId\":\"string\",\"seatBlockInformation\":{\"isClaimable\":true,\"managedBy\":{\"identityType\":\"string\",\"identityValue\":\"string\",\"organization\":{\"identityType\":\"string\",\"identityValue\":\"string\"}},\"seatBlockType\":\"Organization\"},\"legacyPurchaseId\":\"string\"},{\"agent\":{\"identityType\":\"string\",\"identityValue\":\"string\",\"organization\":{\"identityType\":\"string\",\"identityValue\":\"string\"}},\"authorizationPendingReasonCode\":\"string\",\"availabilityId\":\"string\",\"beneficiary\":{\"identityType\":\"string\",\"identityValue\":\"string\",\"organization\":{\"identityType\":\"string\",\"identityValue\":\"string\"}},\"billingState\":\"string\",\"bundleInformation\":{\"componentGroupId\":\"string\",\"instanceId\":\"string\",\"slotType\":\"Undefined\"},\"campaignId\":\"string\",\"currencyCode\":\"string\",\"deliveryCostInformation\":{\"billingStateCode\":\"string\",\"billingStateDescription\":\"Product Second\",\"priceAmount\":0,\"taxAmount\":0,\"refundedPriceAmount\":0,\"refundedTaxAmount\":0},\"deliveryMethodId\":\"string\",\"description\":\"string\",\"devOfferId\":\"string\",\"distributorId\":\"string\",\"feesLineItem\":{\"fees\":[{\"descriptiveCode\":\"string\",\"taxDetails\":[{\"jurisdiction\":\"string\",\"taxAmount\":0,\"taxType\":\"string\"}],\"displayTotalAmount\":0,\"totalAmount\":0,\"totalTaxAmount\":0,\"unitaryAmount\":0}]},\"fulfillmentData\":\"string\",\"fulfillmentDate\":\"2020-05-15T12:02:37.135Z\",\"fulfillmentSessionId\":\"string\",\"fulfillmentState\":\"string\",\"giftee\":{\"recipientId\":\"string\",\"recipientType\":\"string\"},\"isPIRequired\":true,\"RequiresPIValidation\":\"string\",\"isShippingAddressRequired\":true,\"isTaxIncluded\":true,\"lineItemId\":\"string\",\"listPrice\":0,\"optionalCampaignId\":\"string\",\"originalListPrice\":0,\"fulfillmentStates\":[{\"authorizationStateReasonCode\":\"string\",\"cancelable\":true,\"canceled\":true,\"cancelReason\":\"string\",\"fulfillmentQuantityId\":\"string\",\"fulfillmentReturnId\":\"string\",\"fulfillmentReturnType\":\"None\",\"fulfillmentState\":\"None\",\"refundJustifications\":[{\"comment\":\"string\",\"reasonCode\":\"string\",\"riskDecision\":\"string\",\"riskReasonCode\":\"string\",\"userRole\":\"string\"}],\"tokenIdentifier\":\"string\",\"token5x5Value\":\"string\"}],\"billingStates\":[{\"startIndex\":0,\"endIndex\":0,\"billingState\":\"None\",\"billingStateCode\":\"string\",\"billingStateDescription\":\"string\",\"totalChargedAmount\":0,\"totalRefundedAmount\":0,\"totalRefundedTaxAmount\":0,\"totalRefundedFeeAmount\":0,\"totalRefundedFeeTaxAmount\":0}],\"fulfillmentBillingStates\":[{\"billingState\":\"None\",\"count\":0,\"fulfillmentState\":\"None\"}],\"parentalApprovalState\":\"string\",\"promoCode\":{\"assetId\":\"string\",\"assetSource\":\"string\",\"isUnlimitedRedemption\":true,\"name\":\"string\",\"redemptionExpiryDate\":\"2020-05-15T12:02:37.135Z\",\"redemptionStartDate\":\"2020-05-15T12:02:37.135Z\"},\"purchaseRestriction\":{\"askToBuySetting\":\"string\",\"purchaseApprovalState\":\"string\",\"isChild\":true,\"isInFamily\":true},\"payments\":[{\"paymentInstrumentId\":\"string\",\"paymentInstrumentType\":\"string\",\"chargedAmount\":0}],\"productId\":\"product_123\",\"productReturnCategorization\":\"string\",\"productType\":\"string\",\"quantity\":1,\"reservationInformation\":{\"deliverByDates\":{\"additionalProp1\":\"string\",\"additionalProp2\":\"string\",\"additionalProp3\":\"string\"}},\"retailPrice\":499.99,\"shipFromId\":\"string\",\"shipFromAddressId\":\"string\",\"shipToAddressId\":\"string\",\"skuId\":\"30-X9383-96\",\"taxAmount\":25.5,\"taxType\":\"string\",\"title\":\"string\",\"tokenIdentifier\":\"string\",\"token5x5Value\":\"string\",\"totalAmount\":499.99,\"recurrenceId\":\"string\",\"seatBlockId\":\"string\",\"seatBlockInformation\":{\"isClaimable\":true,\"managedBy\":{\"identityType\":\"string\",\"identityValue\":\"string\",\"organization\":{\"identityType\":\"string\",\"identityValue\":\"string\"}},\"seatBlockType\":\"Organization\"},\"legacyPurchaseId\":\"string\"}],\"orderPlacedDate\":\"2020-05-15T12:02:37.135Z\",\"orderRefundedDate\":\"2020-05-15T12:02:37.135Z\",\"orderState\":\"string\",\"orderValidityEndTime\":\"2020-05-15T12:02:37.135Z\",\"orderValidityStartTime\":\"2020-05-15T12:02:37.135Z\",\"partnerPurchaseReceipt\":{\"productFamily\":\"string\",\"productIdentifier\":\"string\",\"purchaseOrderId\":\"string\",\"purchaseReceipt\":\"string\",\"purchaseSource\":\"string\",\"transactionDate\":\"2020-05-15T12:02:37.135Z\"},\"promoCodes\":[\"string\"],\"purchaser\":{\"identityType\":\"string\",\"identityValue\":\"string\",\"organization\":{\"identityType\":\"string\",\"identityValue\":\"string\"}},\"riskSessionId\":\"string\",\"shortOrderId\":\"string\",\"stateReasonCode\":\"string\",\"taxDetails\":[{\"jurisdiction\":\"string\",\"taxAmount\":0,\"taxType\":\"string\"}],\"testScenarios\":\"string\",\"totalAmount\":0,\"totalAmountBeforeTax\":0,\"totalAmusementTax\":0,\"totalChargedToCsvTopOffPI\":0,\"totalDeliveryPriceAmount\":0,\"totalDeliveryPriceTaxAmount\":0,\"totalFeeAmount\":0,\"totalItemAmount\":0,\"totalSalesTax\":0,\"totalTaxAmount\":0,\"version\":\"string\"}")));

            var shippingAddress = new PIMSModel.AddressInfoV3
            {
                FirstName = "John",
                LastName = "Doe",
                AddressLine1 = "One Microsoft Way",
                AddressLine2 = "Address Line 2",
                AddressLine3 = "Address Line 3",
                City = "redmond",
                Region = "WA",
                PostalCode = "98052",
                Country = "USA"
            };

            mockAccountServiceAccessor.Setup(x => x.GetAddress<PIMSModel.AddressInfoV3>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<EventTraceActivity>()))
                .Returns(Task.FromResult(shippingAddress));
        }

        [TestMethod]
        public void ThreeDSUtil_Base64Url_Encoding_Decoding()
        {
            ChallengeRequest sampleCReq = new ChallengeRequest
            {
                ThreeDSServerTransID = "5d0cefae-7154-44c7-9391-f6945ca04d0d",
                AcsTransID = "ec05157f-11b8-4dca-9c1e-b98a3ac53eda",
                ChallengeWindowSize = ChallengeWindowSize.Two,
                MessageType = "CReq",
                MessageVersion = "2.1.0"
            };
            ChallengeRequest decodedCReq = null;
            string base64Url = string.Empty;
            string expectedBase64Url = "eyJ0aHJlZURTU2VydmVyVHJhbnNJRCI6IjVkMGNlZmFlLTcxNTQtNDRjNy05MzkxLWY2OTQ1Y2EwNGQwZCIsImFjc1RyYW5zSUQiOiJlYzA1MTU3Zi0xMWI4LTRkY2EtOWMxZS1iOThhM2FjNTNlZGEiLCJjaGFsbGVuZ2VXaW5kb3dTaXplIjoiMDIiLCJtZXNzYWdlVHlwZSI6IkNSZXEiLCJtZXNzYWdlVmVyc2lvbiI6IjIuMS4wIn0";

            // Check encoding
            try
            {
                base64Url = ThreeDSUtils.EncodeUrl(ThreeDSUtils.EncodeObjectToBase64(sampleCReq));

                Assert.AreEqual(expectedBase64Url, base64Url, "base64Url does not match expected value");
            }
            catch
            {
                Assert.Fail("Encoding function fails with error");
            }

            // Check decoding
            try
            {
                string decodedString = ThreeDSUtils.DecodeBase64(ThreeDSUtils.DecodeUrl(base64Url));

                decodedCReq = JsonConvert.DeserializeObject<ChallengeRequest>(decodedString);
            }
            catch
            {
                Assert.Fail("Decoding function fails with error");
            }

            Assert.AreEqual(sampleCReq.ThreeDSServerTransID, decodedCReq.ThreeDSServerTransID);
            Assert.AreEqual(sampleCReq.AcsTransID, decodedCReq.AcsTransID);
            Assert.AreEqual(sampleCReq.ChallengeWindowSize, decodedCReq.ChallengeWindowSize);
            Assert.AreEqual(sampleCReq.MessageType, decodedCReq.MessageType);
            Assert.AreEqual(sampleCReq.MessageVersion, decodedCReq.MessageVersion);
        }
    }
}