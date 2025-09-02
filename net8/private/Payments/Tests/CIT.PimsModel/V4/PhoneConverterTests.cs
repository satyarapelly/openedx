// <copyright file="PhoneConverterTests.cs" company="Microsoft">Copyright (c) Microsoft 2019. All rights reserved.</copyright>

namespace CIT.PimsModel.V4
{
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class PhoneConverterTests
    {
        const string InvoiceWithPhone = "{\"id\":\"QnqyEAAAAAANAACA\",\"accountId\":\"d7042f79-cb62-4163-ba36-3d4d1549b526\",\"status\":\"active\",\"paymentMethod\":{\"paymentMethodFamily\":\"virtual\",\"paymentMethodType\":\"legacy_invoice\",\"display\":{\"name\":\"Invoice\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/invoice.jpg\",\"logos\":[{\"mimeType\":\"image/jpeg\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/invoice.jpg\"}]},\"properties\":{\"offlineRecurring\":true,\"userManaged\":true,\"soldToAddressRequired\":true,\"supportedOperations\":[],\"taxable\":true,\"providerRemittable\":false,\"moBillingIdentityUrl\":\"\",\"riskyPaymentMethod\":false,\"fundsAvailabilityWindow\":0,\"multipleLineItemsSupported\":true,\"splitPaymentSupported\":false,\"purchaseWaitTime\":0}},\"details\":{\"accountHolderName\":\"Sample Recipient\",\"sapCustomerNumber\":\"d7777f81-f\",\"companyPONumber\":\"Sample PO Number\",\"address\":{\"address_line1\":\"1 Microsoft Way\",\"city\":\"redmond\",\"region\":\"WA\",\"postal_code\":\"98052\",\"country\":\"US\"},\"phone\":{\"areaCode\":\"123\",\"localNumber\":\"123123\",\"country\":\"US\"},\"hashIdentity\":\"989200278056\"},\"creationDateTime\":\"2019-03-29T00:47:10.05\",\"lastUpdatedDateTime\":\"2019-03-29T00:47:17.99\"}";
        const string InvoiceWithoutPhone = "{\"id\":\"QnqyEAAAAAANAACA\",\"accountId\":\"d7042f79-cb62-4163-ba36-3d4d1549b526\",\"status\":\"active\",\"paymentMethod\":{\"paymentMethodFamily\":\"virtual\",\"paymentMethodType\":\"legacy_invoice\",\"display\":{\"name\":\"Invoice\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/invoice.jpg\",\"logos\":[{\"mimeType\":\"image/jpeg\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/invoice.jpg\"}]},\"properties\":{\"offlineRecurring\":true,\"userManaged\":true,\"soldToAddressRequired\":true,\"supportedOperations\":[],\"taxable\":true,\"providerRemittable\":false,\"moBillingIdentityUrl\":\"\",\"riskyPaymentMethod\":false,\"fundsAvailabilityWindow\":0,\"multipleLineItemsSupported\":true,\"splitPaymentSupported\":false,\"purchaseWaitTime\":0}},\"details\":{\"accountHolderName\":\"Sample Recipient\",\"sapCustomerNumber\":\"d7777f81-f\",\"companyPONumber\":\"Sample PO Number\",\"address\":{\"address_line1\":\"1 Microsoft Way\",\"city\":\"redmond\",\"region\":\"WA\",\"postal_code\":\"98052\",\"country\":\"US\"},\"hashIdentity\":\"989200278056\"},\"creationDateTime\":\"2019-03-29T00:47:10.05\",\"lastUpdatedDateTime\":\"2019-03-29T00:47:17.99\"}";
        const string UnionPayDebitCardWithPhone = "{\"id\":\"5olOlwEAAAABAACA\",\"accountId\":\"9a9f7653-256f-41b3-8b25-9379fb759597\",\"status\":\"pending\",\"paymentMethod\":{\"paymentMethodFamily\":\"credit_card\",\"paymentMethodType\":\"unionpay_debitcard\",\"display\":{\"name\":\"银...(5)\",\"logo\":\"https://pmservices.cp.microsoft.com/staticresourceservice/images/v3/logo_unionpay.png\",\"logos\":[{\"mimeType\":\"image/png\",\"url\":\"https://pmservices.cp.microsoft.com/staticresourceservice/images/v3/logo_unionpay.png\"},{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft.com/staticresourceservice/images/v4/logo_unionpay.svg\"}]},\"properties\":{\"offlineRecurring\":true,\"userManaged\":true,\"soldToAddressRequired\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\"],\"taxable\":false,\"providerRemittable\":false,\"moBillingIdentityUrl\":\"\",\"riskyPaymentMethod\":false,\"authWindow\":10080,\"fundsAvailabilityWindow\":0,\"multipleLineItemsSupported\":true,\"splitPaymentSupported\":true,\"purchaseWaitTime\":0},\"exclusionTags\":[\"LegacySubscriptions\"]},\"details\":{\"phone\":\"18747187471\",\"pendingOn\":\"Sms\"}}";
        const string UnionPayDebitCardWithoutPhone = "{\"id\":\"5olOlwEAAAABAACA\",\"accountId\":\"9a9f7653-256f-41b3-8b25-9379fb759597\",\"status\":\"pending\",\"paymentMethod\":{\"paymentMethodFamily\":\"credit_card\",\"paymentMethodType\":\"unionpay_debitcard\",\"display\":{\"name\":\"银...(5)\",\"logo\":\"https://pmservices.cp.microsoft.com/staticresourceservice/images/v3/logo_unionpay.png\",\"logos\":[{\"mimeType\":\"image/png\",\"url\":\"https://pmservices.cp.microsoft.com/staticresourceservice/images/v3/logo_unionpay.png\"},{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft.com/staticresourceservice/images/v4/logo_unionpay.svg\"}]},\"properties\":{\"offlineRecurring\":true,\"userManaged\":true,\"soldToAddressRequired\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\"],\"taxable\":false,\"providerRemittable\":false,\"moBillingIdentityUrl\":\"\",\"riskyPaymentMethod\":false,\"authWindow\":10080,\"fundsAvailabilityWindow\":0,\"multipleLineItemsSupported\":true,\"splitPaymentSupported\":true,\"purchaseWaitTime\":0},\"exclusionTags\":[\"LegacySubscriptions\"]},\"details\":{\"pendingOn\":\"Sms\"}}";
        const string UnionPayCreditCardSmsResumeErrorWithPhone = "{\"id\":\"BHupZgEAAAABAACA\",\"accountId\":\"b2497b81-ddd1-4fe8-b663-e167bf8818b2\",\"status\":\"pending\",\"paymentMethod\":{\"paymentMethodFamily\":\"credit_card\",\"paymentMethodType\":\"unionpay_creditcard\",\"display\":{\"name\":\"U...(20)\",\"logo\":\"https://pmservices.cp.microsoft.com/staticresourceservice/images/v3/logo_unionpay.png\",\"logos\":[{\"mimeType\":\"image/png\",\"url\":\"https://pmservices.cp.microsoft.com/staticresourceservice/images/v3/logo_unionpay.png\"},{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft.com/staticresourceservice/images/v4/logo_unionpay.svg\"}]},\"properties\":{\"offlineRecurring\":true,\"userManaged\":true,\"soldToAddressRequired\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\"],\"taxable\":false,\"providerRemittable\":false,\"moBillingIdentityUrl\":\"\",\"riskyPaymentMethod\":false,\"authWindow\":10080,\"fundsAvailabilityWindow\":0,\"multipleLineItemsSupported\":true,\"splitPaymentSupported\":true,\"purchaseWaitTime\":0},\"exclusionTags\":[\"LegacySubscriptions\"]},\"details\":{\"accountHolderName\":\"吴...(2)\",\"expiryYear\":\"MASKED\",\"expiryMonth\":\"MASKED\",\"lastFourDigits\":\"6940\",\"phone\":\"15034057999\",\"exportable\":false,\"cardType\":\"unknown\",\"pendingOn\":\"Sms\"},\"creationDateTime\":\"2017-06-12T18:17:58.713\",\"lastUpdatedDateTime\":\"2017-06-12T18:17:58.713\"}";

        [DataRow(InvoiceWithPhone, null, "4258828080")]
        [DataRow(InvoiceWithPhone, null, "")]
        [DataRow(InvoiceWithPhone, null, "4258828080")]
        [DataRow(InvoiceWithoutPhone, null, "4258828080")]
        [DataRow(UnionPayDebitCardWithPhone, "18747187471", null)]
        [DataRow(UnionPayDebitCardWithPhone, "18747187471", "")]
        [DataRow(UnionPayDebitCardWithPhone, "18747187471", "4258828080")]
        [DataRow(UnionPayDebitCardWithoutPhone, null, "4258828080")]
        [DataRow(UnionPayCreditCardSmsResumeErrorWithPhone, "15034057999", "4258828080")]
        [DataTestMethod]
        public void PhoneConverterTests_Valid(string serializedPi, string expectedPhone, string updatePhone)
        {   
            var pi = JsonConvert.DeserializeObject<PaymentInstrument>(serializedPi, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            Assert.AreEqual(expectedPhone, pi.PaymentInstrumentDetails.Phone, "The deserialized value of phone '{0}' is not as expected value : '{1}'", pi.PaymentInstrumentDetails.Phone, expectedPhone);

            pi.PaymentInstrumentDetails.Phone = updatePhone;
            serializedPi = JsonConvert.SerializeObject(pi, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            if (updatePhone == null)
            {
                Assert.IsFalse(serializedPi.Contains("\"phone\""), "Serialized Payment Instrument should not contain the updated phone field");
            }
            else if (updatePhone == string.Empty)
            {
                Assert.IsTrue(serializedPi.Contains("\"phone\":\"\""), $"Serialized Payment Instrument should contain the updated phone");
            }
            else
            {
                Assert.IsTrue(serializedPi.Contains($"\"phone\":\"{updatePhone}\""), "Serialized Payment Instrument should contain the updated phone");
            }
        }
    }
}
