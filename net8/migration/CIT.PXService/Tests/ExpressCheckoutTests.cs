// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2022. All rights reserved.</copyright>

namespace CIT.PXService.Tests
{
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXService.Model.IssuerService;
    using Microsoft.Commerce.Payments.PXService.V7;
    using Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Runtime.Remoting.Messaging;
    using System.Text;
    using System.Threading.Tasks;

    [TestClass]
    public class ExpressCheckoutTests : TestBase
    {
        [DataRow("cw_gpay_cc80ac8e-3e33-40f3-9fed-6efb5be47762", "googlepay", true, "{\"apiVersionMinor\":0,\"apiVersion\":2,\"paymentMethodData\":{\"description\":\"Visa •••• 1234\",\"token\":\"<PCE Token which get from /getToken/apay>\",\"type\":\"CARD\",\"info\":{\"cardNetwork\":\"VISA\",\"cardDetails\":\"1234\",\"billingAddress\":{\"phoneNumber\":\"+1 111-111-1111\",\"address3\":\"\",\"sortingCode\":\"\",\"address2\":\"\",\"countryCode\":\"US\",\"address1\":\"1 Microsoft Way\",\"postalCode\":\"98052\",\"name\":\"First Last\",\"locality\":\"Redmond\",\"administrativeArea\":\"WA\"}}},\"shippingOptionData\":{\"id\":\"shipping-001\"},\"shippingAddress\":{\"address3\":\"\",\"sortingCode\":\"\",\"address2\":\"\",\"countryCode\":\"US\",\"address1\":\"1 Microsoft Way\",\"postalCode\":\"98052-7073\",\"name\":\"First Last\",\"locality\":\"REDMOND\",\"administrativeArea\":\"WA\"},\"email\":\"mstest@gmail.com\"}")]
        [DataRow("cw_apay_9f8b4e00-edf0-4b22-8feb-37c152e5875b", "applepay", true, "{\"billingContact\":{\"addressLines\":[\"1 way microsoft\"],\"administrativeArea\":\"WA\",\"country\":\"United States\",\"countryCode\":\"US\",\"familyName\":\"LastName\",\"givenName\":\"FirstName\",\"locality\":\"redmond\",\"postalCode\":\"98052\",\"subAdministrativeArea\":\"\",\"subLocality\":\"\"},\"shippingContact\":{\"addressLines\":[\"1 microsoft way\"],\"administrativeArea\":\"WA\",\"country\":\"United States\",\"countryCode\":\"US\",\"emailAddress\":\"mstest@gmail.com\",\"familyName\":\"LastName\",\"givenName\":\"FirstName\",\"locality\":\"redmond\",\"phoneNumber\":\"1234567890\",\"postalCode\":\"98052-8300\",\"subAdministrativeArea\":\"\",\"subLocality\":\"\"},\"token\":{\"token\":\"<PCE Token which get from /getToken/apay>\",\"paymentMethod\":{\"displayName\":\"Visa 1234\",\"network\":\"Visa\",\"type\":\"credit\"},\"transactionIdentifier\":\"8db6cbfe1b35798839f8c14075393f4533b62fa0783c70022d5c32dea4eab141\"}}")]
        [DataRow("cw_gpay_cc80ac8e-3e33-40f3-9fed-6efb5be47762", "googlepay", false, "{\"apiVersionMinor\":0,\"apiVersion\":2,\"paymentMethodData\":{\"description\":\"Visa •••• 1234\",\"token\":\"<PCE Token which get from /getToken/apay>\",\"type\":\"CARD\",\"info\":{\"cardNetwork\":\"VISA\",\"cardDetails\":\"1234\",\"billingAddress\":{\"phoneNumber\":\"+1 111-111-1111\",\"address3\":\"\",\"sortingCode\":\"\",\"address2\":\"\",\"countryCode\":\"US\",\"address1\":\"1 Microsoft Way\",\"postalCode\":\"98052\",\"name\":\"First Last\",\"locality\":\"Redmond\",\"administrativeArea\":\"WA\"}}},\"shippingOptionData\":{\"id\":\"shipping-001\"},\"shippingAddress\":{\"address3\":\"\",\"sortingCode\":\"\",\"address2\":\"\",\"countryCode\":\"US\",\"address1\":\"1 Microsoft Way\",\"postalCode\":\"98052-7073\",\"name\":\"First Last\",\"locality\":\"REDMOND\",\"administrativeArea\":\"WA\"},\"email\":\"mstest@gmail.com\"}")]
        [DataRow("cw_apay_9f8b4e00-edf0-4b22-8feb-37c152e5875b", "applepay", false, "{\"billingContact\":{\"addressLines\":[\"1 way microsoft\"],\"administrativeArea\":\"WA\",\"country\":\"United States\",\"countryCode\":\"US\",\"familyName\":\"LastName\",\"givenName\":\"FirstName\",\"locality\":\"redmond\",\"postalCode\":\"98052\",\"subAdministrativeArea\":\"\",\"subLocality\":\"\"},\"shippingContact\":{\"addressLines\":[\"1 microsoft way\"],\"administrativeArea\":\"WA\",\"country\":\"United States\",\"countryCode\":\"US\",\"emailAddress\":\"mstest@gmail.com\",\"familyName\":\"LastName\",\"givenName\":\"FirstName\",\"locality\":\"redmond\",\"phoneNumber\":\"1234567890\",\"postalCode\":\"98052-8300\",\"subAdministrativeArea\":\"\",\"subLocality\":\"\"},\"token\":{\"token\":\"<PCE Token which get from /getToken/apay>\",\"paymentMethod\":{\"displayName\":\"Visa 1234\",\"network\":\"Visa\",\"type\":\"credit\"},\"transactionIdentifier\":\"8db6cbfe1b35798839f8c14075393f4533b62fa0783c70022d5c32dea4eab141\"}}")]
        [TestMethod]
        public async Task ExpressCheckoutConfirmTests(string piid, string paymentMethodType, bool isCompleteProfile, string expressCheckoutPaymentData)
        {
            // Arrange
            string requestUrl = string.Format("/v7.0/testaccountid/expressCheckout/confirm");

            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account013", piid);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            var postAddressResponse = new
            {
                addressId = "addressId",
                address_line1 = "addressLine1",
                address_line2 = "addressLine2",
                city = "city",
                stateOrProvince = "stateOrProvince",
                postalCode = "postalCode",
                country = "country",
            };

            PXSettings.AccountsService.ArrangeResponse(
                JsonConvert.SerializeObject(postAddressResponse),
                HttpStatusCode.OK,
                new HttpMethod("Post"));

            AccountProfilesV3<AccountConsumerProfileV3> userProfiles = new AccountProfilesV3<AccountConsumerProfileV3>();
            userProfiles.UserProfiles = new List<AccountConsumerProfileV3>();

            AccountConsumerProfileV3 userProfile = new AccountConsumerProfileV3()
            {
                FirstName = "Test",
                LastName = "Test111",
                ProfileType = "consumer",
                EmailAddress = "test@test.test"
            };

            if (!isCompleteProfile)
            {
                userProfile.FirstName = null;
                userProfile.LastName = null;
                userProfile.EmailAddress = null;
            }

            userProfiles.UserProfiles.Add(userProfile);

            PXSettings.AccountsService.ArrangeResponse(
                JsonConvert.SerializeObject(userProfiles),
                HttpStatusCode.OK,
                new HttpMethod("Get"));

            PXSettings.AccountsService.ArrangeResponse(
                JsonConvert.SerializeObject(userProfiles),
                HttpStatusCode.OK,
                new HttpMethod("Patch"));

            bool updateProfileCalled = false;
            PXSettings.AccountsService.PreProcess = (accountServiceRequest) =>
            {
                if (accountServiceRequest.RequestUri.AbsolutePath.Contains($"/profiles") && accountServiceRequest.Method.Method == "PATCH")
                {
                    updateProfileCalled = true;
                }
            };

            bool isWalletAttachPaymentTypeUsed = false;
            PXSettings.PimsService.PreProcess = async (pimsServiceRequest) =>
            {
                string requestContent = await pimsServiceRequest.Content.ReadAsStringAsync();
                isWalletAttachPaymentTypeUsed = requestContent.Contains("\"AttachmentType\":\"Wallet\"");
            };

            var payload = new
            {
                paymentMethodType = paymentMethodType,
                checkoutCountry = "US",
                expressCheckoutPaymentData = expressCheckoutPaymentData,
            };

            HttpResponseMessage result = await SendRequestPXService(requestUrl, HttpMethod.Post, new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType), null);

            string responseContent = await result.Content.ReadAsStringAsync();
            ExpressCheckoutResult expressCheckoutResult = JsonConvert.DeserializeObject<ExpressCheckoutResult>(responseContent);

            // Assert
            if (!isCompleteProfile)
            {
                Assert.IsTrue(updateProfileCalled, "update profile should be called");
            }
            else
            {
                Assert.IsFalse(updateProfileCalled, "update profile should not be called");
            }

            Assert.IsNotNull(responseContent);
            Assert.IsNotNull(expressCheckoutResult.Pi, "result should contain a pi object");
            Assert.IsNotNull(expressCheckoutResult.BillingAddress, "result should contain a biilingAddress object");

            Assert.IsTrue(isWalletAttachPaymentTypeUsed, "Wallet Attach Payment Type is not used for post PI of googlepay or apple pay in confirm");
        }

        [DataRow("cw_gpay_cc80ac8e-3e33-40f3-9fed-6efb5be47762", "googlepay", true, "{\"apiVersionMinor\":0,\"apiVersion\":2,\"paymentMethodData\":{\"description\":\"Visa •••• 1234\",\"token\":\"<PCE Token which get from /getToken/apay>\",\"type\":\"CARD\",\"info\":{\"cardNetwork\":\"VISA\",\"cardDetails\":\"1234\",\"billingAddress\":{\"phoneNumber\":\"+1 111-111-1111\",\"address3\":\"\",\"sortingCode\":\"\",\"address2\":\"\",\"countryCode\":\"US\",\"address1\":\"1 Microsoft Way\",\"postalCode\":\"98052\",\"name\":\"First Last\",\"locality\":\"Redmond\",\"administrativeArea\":\"WA\"}}},\"shippingOptionData\":{\"id\":\"shipping-001\"},\"shippingAddress\":{\"address3\":\"\",\"sortingCode\":\"\",\"address2\":\"\",\"countryCode\":\"US\",\"address1\":\"1 Microsoft Way\",\"postalCode\":\"98052-7073\",\"name\":\"First Last\",\"locality\":\"REDMOND\",\"administrativeArea\":\"WA\"},\"email\":\"mstest@gmail.com\"}")]
        [DataRow("cw_apay_9f8b4e00-edf0-4b22-8feb-37c152e5875b", "applepay", true, "{\"billingContact\":{\"addressLines\":[\"1 way microsoft\"],\"administrativeArea\":\"WA\",\"country\":\"United States\",\"countryCode\":\"US\",\"familyName\":\"LastName\",\"givenName\":\"FirstName\",\"locality\":\"redmond\",\"postalCode\":\"98052\",\"subAdministrativeArea\":\"\",\"subLocality\":\"\"},\"shippingContact\":{\"addressLines\":[\"1 microsoft way\"],\"administrativeArea\":\"WA\",\"country\":\"United States\",\"countryCode\":\"US\",\"emailAddress\":\"mstest@gmail.com\",\"familyName\":\"LastName\",\"givenName\":\"FirstName\",\"locality\":\"redmond\",\"phoneNumber\":\"1234567890\",\"postalCode\":\"98052-8300\",\"subAdministrativeArea\":\"\",\"subLocality\":\"\"},\"token\":{\"token\":\"<PCE Token which get from /getToken/apay>\",\"paymentMethod\":{\"displayName\":\"Visa 1234\",\"network\":\"Visa\",\"type\":\"credit\"},\"transactionIdentifier\":\"8db6cbfe1b35798839f8c14075393f4533b62fa0783c70022d5c32dea4eab141\"}}")]
        [DataRow("cw_gpay_cc80ac8e-3e33-40f3-9fed-6efb5be47762", "googlepay", false, "{\"apiVersionMinor\":0,\"apiVersion\":2,\"paymentMethodData\":{\"description\":\"Visa •••• 1234\",\"token\":\"<PCE Token which get from /getToken/apay>\",\"type\":\"CARD\",\"info\":{\"cardNetwork\":\"VISA\",\"cardDetails\":\"1234\",\"billingAddress\":{\"phoneNumber\":\"+1 111-111-1111\",\"address3\":\"\",\"sortingCode\":\"\",\"address2\":\"\",\"countryCode\":\"US\",\"address1\":\"1 Microsoft Way\",\"postalCode\":\"98052\",\"name\":\"First Last\",\"locality\":\"Redmond\",\"administrativeArea\":\"WA\"}}},\"shippingOptionData\":{\"id\":\"shipping-001\"},\"shippingAddress\":{\"address3\":\"\",\"sortingCode\":\"\",\"address2\":\"\",\"countryCode\":\"US\",\"address1\":\"1 Microsoft Way\",\"postalCode\":\"98052-7073\",\"name\":\"First Last\",\"locality\":\"REDMOND\",\"administrativeArea\":\"WA\"},\"email\":\"mstest@gmail.com\"}")]
        [DataRow("cw_apay_9f8b4e00-edf0-4b22-8feb-37c152e5875b", "applepay", false, "{\"billingContact\":{\"addressLines\":[\"1 way microsoft\"],\"administrativeArea\":\"WA\",\"country\":\"United States\",\"countryCode\":\"US\",\"familyName\":\"LastName\",\"givenName\":\"FirstName\",\"locality\":\"redmond\",\"postalCode\":\"98052\",\"subAdministrativeArea\":\"\",\"subLocality\":\"\"},\"shippingContact\":{\"addressLines\":[\"1 microsoft way\"],\"administrativeArea\":\"WA\",\"country\":\"United States\",\"countryCode\":\"US\",\"emailAddress\":\"mstest@gmail.com\",\"familyName\":\"LastName\",\"givenName\":\"FirstName\",\"locality\":\"redmond\",\"phoneNumber\":\"1234567890\",\"postalCode\":\"98052-8300\",\"subAdministrativeArea\":\"\",\"subLocality\":\"\"},\"token\":{\"token\":\"<PCE Token which get from /getToken/apay>\",\"paymentMethod\":{\"displayName\":\"Visa 1234\",\"network\":\"Visa\",\"type\":\"credit\"},\"transactionIdentifier\":\"8db6cbfe1b35798839f8c14075393f4533b62fa0783c70022d5c32dea4eab141\"}}")]
        [TestMethod]
        public async Task ExpressCheckoutConfirmTest_InvalidAddress(string piid, string paymentMethodType, bool isCompleteProfile, string expressCheckoutPaymentData)
        {
            // Arrange
            string requestUrl = string.Format("/v7.0/testaccountid/expressCheckout/confirm");

            var postAddressResponse = new
            {
                addressId = "addressId",
                address_line1 = "addressLine1",
                address_line2 = "addressLine2",
                city = "city",
                stateOrProvince = "stateOrProvince",
                postalCode = "postalCode",
                country = "country",
            };

            PXSettings.AccountsService.ArrangeResponse(
                JsonConvert.SerializeObject(postAddressResponse),
                HttpStatusCode.InternalServerError,
                new HttpMethod("Post"));

            var payload = new
            {
                paymentMethodType = paymentMethodType,
                checkoutCountry = "US",
                expressCheckoutPaymentData = expressCheckoutPaymentData,
            };

            HttpResponseMessage result = await SendRequestPXService(requestUrl, HttpMethod.Post, new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType), null);

            string responseContent = await result.Content.ReadAsStringAsync();

            Assert.IsTrue(responseContent.Contains("InvalidAddress"), "result should contain a InvalidAddress error code");
        }

        [DataRow("cw_gpay_cc80ac8e-3e33-40f3-9fed-6efb5be47762", "googlepay", "{\"apiVersionMinor\":0,\"apiVersion\":2,\"paymentMethodData\":{\"description\":\"Visa •••• 1234\",\"token\":\"<PCE Token which get from /getToken/apay>\",\"type\":\"CARD\",\"info\":{\"cardNetwork\":\"VISA\",\"cardDetails\":\"1234\",\"billingAddress\":{\"phoneNumber\":\"+1 111-111-1111\",\"address3\":\"\",\"sortingCode\":\"\",\"address2\":\"\",\"countryCode\":\"US\",\"address1\":\"1 Microsoft Way\",\"postalCode\":\"98052\",\"name\":\"First Last\",\"locality\":\"Redmond\",\"administrativeArea\":\"WA\"}}},\"shippingOptionData\":{\"id\":\"shipping-001\"},\"shippingAddress\":{\"address3\":\"\",\"sortingCode\":\"\",\"address2\":\"\",\"countryCode\":\"US\",\"address1\":\"1 Microsoft Way\",\"postalCode\":\"98052-7073\",\"name\":\"First Last\",\"locality\":\"REDMOND\",\"administrativeArea\":\"WA\"},\"email\":\"mstest@gmail.com\"}")]
        [DataRow("cw_apay_9f8b4e00-edf0-4b22-8feb-37c152e5875b", "applepay", "{\"billingContact\":{\"addressLines\":[\"1 way microsoft\"],\"administrativeArea\":\"WA\",\"country\":\"United States\",\"countryCode\":\"US\",\"familyName\":\"LastName\",\"givenName\":\"FirstName\",\"locality\":\"redmond\",\"postalCode\":\"98052\",\"subAdministrativeArea\":\"\",\"subLocality\":\"\"},\"shippingContact\":{\"addressLines\":[\"1 microsoft way\"],\"administrativeArea\":\"WA\",\"country\":\"United States\",\"countryCode\":\"US\",\"emailAddress\":\"mstest@gmail.com\",\"familyName\":\"LastName\",\"givenName\":\"FirstName\",\"locality\":\"redmond\",\"phoneNumber\":\"1234567890\",\"postalCode\":\"98052-8300\",\"subAdministrativeArea\":\"\",\"subLocality\":\"\"},\"token\":{\"token\":\"<PCE Token which get from /getToken/apay>\",\"paymentMethod\":{\"displayName\":\"Visa 1234\",\"network\":\"Visa\",\"type\":\"credit\"},\"transactionIdentifier\":\"8db6cbfe1b35798839f8c14075393f4533b62fa0783c70022d5c32dea4eab141\"}}")]
        [TestMethod]
        public async Task ExpressCheckoutConfirmTest_InvalidProfile(string piid, string paymentMethodType, string expressCheckoutPaymentData)
        {
            // Arrange
            string requestUrl = string.Format("/v7.0/testaccountid/expressCheckout/confirm");

            var postAddressResponse = new
            {
                addressId = "addressId",
                address_line1 = "addressLine1",
                address_line2 = "addressLine2",
                city = "city",
                stateOrProvince = "stateOrProvince",
                postalCode = "postalCode",
                country = "country",
            };

            PXSettings.AccountsService.ArrangeResponse(
                JsonConvert.SerializeObject(postAddressResponse),
                HttpStatusCode.OK,
                new HttpMethod("Post"));

            AccountProfilesV3<AccountConsumerProfileV3> userProfiles = new AccountProfilesV3<AccountConsumerProfileV3>();
            userProfiles.UserProfiles = new List<AccountConsumerProfileV3>();

            AccountConsumerProfileV3 userProfile = new AccountConsumerProfileV3()
            {
                FirstName = null,
                LastName = null,
                ProfileType = "consumer",
                EmailAddress = null
            };

            userProfiles.UserProfiles.Add(userProfile);

            PXSettings.AccountsService.ArrangeResponse(
                JsonConvert.SerializeObject(userProfiles),
                HttpStatusCode.OK,
                new HttpMethod("Get"));

            PXSettings.AccountsService.ArrangeResponse(
                JsonConvert.SerializeObject(userProfiles),
                HttpStatusCode.InternalServerError,
                new HttpMethod("Patch"));

            var payload = new
            {
                paymentMethodType = paymentMethodType,
                checkoutCountry = "US",
                expressCheckoutPaymentData = expressCheckoutPaymentData,
            };

            HttpResponseMessage result = await SendRequestPXService(requestUrl, HttpMethod.Post, new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType), null);

            string responseContent = await result.Content.ReadAsStringAsync();

            Assert.IsTrue(responseContent.Contains("InvalidProfile"), "result should contain a InvalidProfile error code");
        }

        [DataRow("cw_gpay_cc80ac8e-3e33-40f3-9fed-6efb5be47762", "googlepay", true, "{\"apiVersionMinor\":0,\"apiVersion\":2,\"paymentMethodData\":{\"description\":\"Visa •••• 1234\",\"token\":\"<PCE Token which get from /getToken/apay>\",\"type\":\"CARD\",\"info\":{\"cardNetwork\":\"VISA\",\"cardDetails\":\"1234\",\"billingAddress\":{\"phoneNumber\":\"+1 111-111-1111\",\"address3\":\"\",\"sortingCode\":\"\",\"address2\":\"\",\"countryCode\":\"US\",\"address1\":\"1 Microsoft Way\",\"postalCode\":\"98052\",\"name\":\"First Last\",\"locality\":\"Redmond\",\"administrativeArea\":\"WA\"}}},\"shippingOptionData\":{\"id\":\"shipping-001\"},\"shippingAddress\":{\"address3\":\"\",\"sortingCode\":\"\",\"address2\":\"\",\"countryCode\":\"US\",\"address1\":\"1 Microsoft Way\",\"postalCode\":\"98052-7073\",\"name\":\"First Last\",\"locality\":\"REDMOND\",\"administrativeArea\":\"WA\"},\"email\":\"mstest@gmail.com\"}")]
        [DataRow("cw_apay_9f8b4e00-edf0-4b22-8feb-37c152e5875b", "applepay", true, "{\"billingContact\":{\"addressLines\":[\"1 way microsoft\"],\"administrativeArea\":\"WA\",\"country\":\"United States\",\"countryCode\":\"US\",\"familyName\":\"LastName\",\"givenName\":\"FirstName\",\"locality\":\"redmond\",\"postalCode\":\"98052\",\"subAdministrativeArea\":\"\",\"subLocality\":\"\"},\"shippingContact\":{\"addressLines\":[\"1 microsoft way\"],\"administrativeArea\":\"WA\",\"country\":\"United States\",\"countryCode\":\"US\",\"emailAddress\":\"mstest@gmail.com\",\"familyName\":\"LastName\",\"givenName\":\"FirstName\",\"locality\":\"redmond\",\"phoneNumber\":\"1234567890\",\"postalCode\":\"98052-8300\",\"subAdministrativeArea\":\"\",\"subLocality\":\"\"},\"token\":{\"token\":\"<PCE Token which get from /getToken/apay>\",\"paymentMethod\":{\"displayName\":\"Visa 1234\",\"network\":\"Visa\",\"type\":\"credit\"},\"transactionIdentifier\":\"8db6cbfe1b35798839f8c14075393f4533b62fa0783c70022d5c32dea4eab141\"}}")]
        [DataRow("cw_gpay_cc80ac8e-3e33-40f3-9fed-6efb5be47762", "googlepay", false, "{\"apiVersionMinor\":0,\"apiVersion\":2,\"paymentMethodData\":{\"description\":\"Visa •••• 1234\",\"token\":\"<PCE Token which get from /getToken/apay>\",\"type\":\"CARD\",\"info\":{\"cardNetwork\":\"VISA\",\"cardDetails\":\"1234\",\"billingAddress\":{\"phoneNumber\":\"+1 111-111-1111\",\"address3\":\"\",\"sortingCode\":\"\",\"address2\":\"\",\"countryCode\":\"US\",\"address1\":\"1 Microsoft Way\",\"postalCode\":\"98052\",\"name\":\"First Last\",\"locality\":\"Redmond\",\"administrativeArea\":\"WA\"}}},\"shippingOptionData\":{\"id\":\"shipping-001\"},\"shippingAddress\":{\"address3\":\"\",\"sortingCode\":\"\",\"address2\":\"\",\"countryCode\":\"US\",\"address1\":\"1 Microsoft Way\",\"postalCode\":\"98052-7073\",\"name\":\"First Last\",\"locality\":\"REDMOND\",\"administrativeArea\":\"WA\"},\"email\":\"mstest@gmail.com\"}")]
        [DataRow("cw_apay_9f8b4e00-edf0-4b22-8feb-37c152e5875b", "applepay", false, "{\"billingContact\":{\"addressLines\":[\"1 way microsoft\"],\"administrativeArea\":\"WA\",\"country\":\"United States\",\"countryCode\":\"US\",\"familyName\":\"LastName\",\"givenName\":\"FirstName\",\"locality\":\"redmond\",\"postalCode\":\"98052\",\"subAdministrativeArea\":\"\",\"subLocality\":\"\"},\"shippingContact\":{\"addressLines\":[\"1 microsoft way\"],\"administrativeArea\":\"WA\",\"country\":\"United States\",\"countryCode\":\"US\",\"emailAddress\":\"mstest@gmail.com\",\"familyName\":\"LastName\",\"givenName\":\"FirstName\",\"locality\":\"redmond\",\"phoneNumber\":\"1234567890\",\"postalCode\":\"98052-8300\",\"subAdministrativeArea\":\"\",\"subLocality\":\"\"},\"token\":{\"token\":\"<PCE Token which get from /getToken/apay>\",\"paymentMethod\":{\"displayName\":\"Visa 1234\",\"network\":\"Visa\",\"type\":\"credit\"},\"transactionIdentifier\":\"8db6cbfe1b35798839f8c14075393f4533b62fa0783c70022d5c32dea4eab141\"}}")]
        [TestMethod]
        public async Task ExpressCheckoutConfirmTests_InvalidPaymentInstrument(string piid, string paymentMethodType, bool isCompleteProfile, string expressCheckoutPaymentData)
        {
            // Arrange
            string requestUrl = string.Format("/v7.0/testaccountid/expressCheckout/confirm");

            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account013", piid);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI), HttpStatusCode.InternalServerError);

            var postAddressResponse = new
            {
                addressId = "addressId",
                address_line1 = "addressLine1",
                address_line2 = "addressLine2",
                city = "city",
                stateOrProvince = "stateOrProvince",
                postalCode = "postalCode",
                country = "country",
            };

            PXSettings.AccountsService.ArrangeResponse(
                JsonConvert.SerializeObject(postAddressResponse),
                HttpStatusCode.OK,
                new HttpMethod("Post"));

            AccountProfilesV3<AccountConsumerProfileV3> userProfiles = new AccountProfilesV3<AccountConsumerProfileV3>();
            userProfiles.UserProfiles = new List<AccountConsumerProfileV3>();

            AccountConsumerProfileV3 userProfile = new AccountConsumerProfileV3()
            {
                FirstName = "Test",
                LastName = "Test111",
                ProfileType = "consumer",
                EmailAddress = "test@test.test"
            };

            if (!isCompleteProfile)
            {
                userProfile.FirstName = null;
                userProfile.LastName = null;
                userProfile.EmailAddress = null;
            }

            userProfiles.UserProfiles.Add(userProfile);

            PXSettings.AccountsService.ArrangeResponse(
                JsonConvert.SerializeObject(userProfiles),
                HttpStatusCode.OK,
                new HttpMethod("Get"));

            PXSettings.AccountsService.ArrangeResponse(
                JsonConvert.SerializeObject(userProfiles),
                HttpStatusCode.OK,
                new HttpMethod("Patch"));

            var payload = new
            {
                paymentMethodType = paymentMethodType,
                checkoutCountry = "US",
                expressCheckoutPaymentData = expressCheckoutPaymentData,
            };

            HttpResponseMessage result = await SendRequestPXService(requestUrl, HttpMethod.Post, new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType), null);

            string responseContent = await result.Content.ReadAsStringAsync();

            Assert.IsTrue(responseContent.Contains("InvalidPaymentInstrument"), "result should contain a InvalidAddress error code");
        }
    }
}
