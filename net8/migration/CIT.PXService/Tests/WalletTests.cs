// <copyright file="WalletTests.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace CIT.PXService.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Metrics;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using CIT.PXService.TestData;
    using Microsoft.Commerce.Payments.Common.Transaction;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel;
    using Microsoft.Commerce.Payments.PXService.Model.PXInternal;
    using Microsoft.Commerce.Payments.PXService.Model.WalletService;
    using Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model;
    using Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks;
    using Microsoft.Diagnostics.Tracing.Parsers.AspNet;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using UAParser;
    using static CIT.PXService.TestData.Samples;

    [TestClass]
    public class WalletTests : TestBase
    {
        [TestMethod]
        public async Task GetWalletConfig()
        {
            string url = $"/v7.0/getWalletConfig";
            string expectedWalletResponse = "{\"merchantName\":\"Microsoft\",\"integrationType\":\"DIRECT\",\"directIntegrationData\":[{\"piFamily\":\"Ewallet\",\"piType\":\"ApplePay\",\"countrySupportedNetworks\":{\"us\":[\"visa\",\"mastercard\",\"discover\",\"amex\"],\"ca\":[\"visa\",\"mastercard\",\"discover\",\"amex\"]},\"providerData\":{\"merchantIdentifier\":\"merchant.com.microsoft.paymicrosoft.sandbox\",\"merchantCapabilities\":[\"supports3DS\",\"supportsEMV\"],\"version\":\"3\"}},{\"piFamily\":\"Ewallet\",\"piType\":\"GooglePay\",\"countrySupportedNetworks\":{\"us\":[\"visa\",\"mastercard\",\"discover\",\"amex\"],\"ca\":[\"visa\",\"mastercard\",\"discover\",\"amex\"]},\"providerData\":{\"allowedMethods\":[\"PAN_ONLY\",\"CRYPTOGRAM_3DS\"],\"assuranceDetailsRequired\":true,\"protocolVersion\":\"ECv2\",\"publicKey\":\"BDRvcFHptepUygYllpEmnHNb4rfG4oBU/sVcAVMn9I7Pu3wYpDpjhBeoF+i30iB2HUQGyZEUsodGdhphaD/j6rU=\",\"publicKeyVersion\":\"1\",\"apiMajorVersion\":2,\"apiMinorVersion\":0,\"merchantId\":\"BCR2DN4TZ244PH2A\"}}]}";
            string expectedPIMSResponse = "[{\"paymentMethodType\":\"applepay\",\"properties\":{\"offlineRecurring\":true,\"userManaged\":false,\"chargeThresholds\":null,\"redirectRequired\":null,\"soldToAddressRequired\":true,\"splitPaymentSupported\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"providerCountry\":null,\"nonStoredPaymentMethodId\":\"be4de87d-7e38-4b2d-8836-9237eb32848e\",\"isNonStoredPaymentMethod\":true},\"paymentMethodGroup\":\"ewallet\",\"groupDisplayName\":\"eWallet\",\"exclusionTags\":null,\"paymentMethodFamily\":\"ewallet\",\"display\":{\"name\":\"ApplePay\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_applepay.svg\",\"logos\":[{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_applepay.svg\"}]},\"AdditionalDisplayText\":null},{\"paymentMethodType\":\"googlepay\",\"properties\":{\"offlineRecurring\":true,\"userManaged\":false,\"chargeThresholds\":null,\"redirectRequired\":null,\"soldToAddressRequired\":true,\"splitPaymentSupported\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"providerCountry\":null,\"nonStoredPaymentMethodId\":\"cdc85313-9b57-4052-81fb-dea336132cbf\",\"isNonStoredPaymentMethod\":true},\"paymentMethodGroup\":\"ewallet\",\"groupDisplayName\":\"eWallet\",\"exclusionTags\":null,\"paymentMethodFamily\":\"ewallet\",\"display\":{\"name\":\"GooglePay\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_googlepay.svg\",\"logos\":[{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_googlepay.svg\"}]},\"AdditionalDisplayText\":null}]";
            string pxResponse = "{\"PIDLConfig\":{\"SelectResource.PaymentInstrument\":{\"actions\":{\"ewallet.applepay.default\":[\"PaymentInstrumentHandler.ClientSupported\"],\"ewallet.googlepay.default\":[\"PaymentInstrumentHandler.ClientSupported\"]}},\"HandlePaymentChallenge\":{\"actions\":{\"ewallet.applepay.default\":[\"PaymentInstrumentHandler.CollectPaymentToken\"],\"ewallet.googlepay.default\":[\"PaymentInstrumentHandler.CollectPaymentToken\"]}}},\"PaymentInstrumentHandlers\":[{\"allowedAuthMethods\":[\"PAN_ONLY\",\"CRYPTOGRAM_3DS\"],\"protocolVersion\":\"ECv2\",\"publicKey\":\"BDRvcFHptepUygYllpEmnHNb4rfG4oBU/sVcAVMn9I7Pu3wYpDpjhBeoF+i30iB2HUQGyZEUsodGdhphaD/j6rU=\",\"merchantName\":\"Microsoft\",\"apiMajorVersion\":\"2\",\"apiMinorVersion\":\"0\",\"assuranceDetailsRequired\":true,\"publicKeyVersion\":\"1\",\"enableGPayIframeForAllBrowsers\":false,\"merchantId\":\"BCR2DN4TZ244PH2A\",\"paymentMethodFamily\":\"ewallet\",\"paymentMethodType\":\"googlepay\",\"piid\":\"cdc85313-9b57-4052-81fb-dea336132cbf\",\"payLabel\":\"amount due plus applicable taxes\",\"integrationType\":\"DIRECT\",\"allowedAuthMethodsPerCountry\":{\"us\":[\"VISA\",\"MASTERCARD\",\"DISCOVER\",\"AMEX\"],\"ca\":[\"VISA\",\"MASTERCARD\",\"DISCOVER\",\"AMEX\"]},\"clientSupported\":{\"supportedBrowsers\":{\"chrome\":\"100\",\"edge\":\"100\"},\"supportedOS\":{\"ios\":\"16.0\",\"android\":\"16.0\",\"windows\":\"16.0\"},\"additionalAPIsCheck\":[\"canMakePayment\"]},\"enableBillingAddress\":false,\"enableEmail\":false,\"disableGeoFencing\":false,\"singleMarkets\":[\"AT\",\"BE\",\"BG\",\"CH\",\"CY\",\"CZ\",\"DE\",\"DK\",\"EE\",\"ES\",\"FI\",\"FR\",\"GB\",\"GR\",\"HR\",\"HU\",\"IE\",\"IS\",\"IT\",\"LI\",\"LT\",\"LU\",\"LV\",\"MT\",\"NL\",\"NO\",\"PL\",\"PT\",\"RO\",\"SE\",\"SI\",\"SK\"]},{\"merchantCapabilities\":[\"supports3DS\",\"supportsEMV\"],\"displayName\":\"Microsoft\",\"initiative\":\"Web\",\"initiativeContext\":\"mystore.example.com\",\"merchantIdentifier\":\"merchant.com.microsoft.paymicrosoft.sandbox\",\"applePayVersion\":\"3\",\"paymentMethodFamily\":\"ewallet\",\"paymentMethodType\":\"applepay\",\"piid\":\"be4de87d-7e38-4b2d-8836-9237eb32848e\",\"payLabel\":\"amount due plus applicable taxes\",\"integrationType\":\"DIRECT\",\"allowedAuthMethodsPerCountry\":{\"us\":[\"VISA\",\"MASTERCARD\",\"DISCOVER\",\"AMEX\"],\"ca\":[\"VISA\",\"MASTERCARD\",\"DISCOVER\",\"AMEX\"]},\"clientSupported\":{\"supportedBrowsers\":{\"safari\":\"16.1\"},\"supportedOS\":{\"ios\":\"15.0\"},\"additionalAPIsCheck\":[\"canMakePaymentWithActiveCard\"],\"paymentProxyRequired\":{\"safari\":\"16.5\"}},\"enableBillingAddress\":false,\"enableEmail\":false,\"disableGeoFencing\":false,\"singleMarkets\":[\"AT\",\"BE\",\"BG\",\"CH\",\"CY\",\"CZ\",\"DE\",\"DK\",\"EE\",\"ES\",\"FI\",\"FR\",\"GB\",\"GR\",\"HR\",\"HU\",\"IE\",\"IS\",\"IT\",\"LI\",\"LT\",\"LU\",\"LV\",\"MT\",\"NL\",\"NO\",\"PL\",\"PT\",\"RO\",\"SE\",\"SI\",\"SK\"]}]}";

            PXSettings.WalletService.ArrangeResponse(expectedWalletResponse);
            PXSettings.PimsService.ArrangeResponse(expectedPIMSResponse);

            PXSettings.WalletService.PreProcess = (walletRequest) =>
            {
                Uri requestUri = walletRequest.RequestUri;
                Assert.IsTrue(requestUri.PathAndQuery.Contains("/wallet"));
                Assert.IsNull(walletRequest.Content);
            };

            PXFlightHandler.AddToEnabledFlights("PXDisableGetWalletConfigCache");
            PXSettings.WalletService.PostProcess = async (walletResponse) =>
            {
                if (walletResponse != null)
                {
                    Assert.AreEqual(walletResponse.StatusCode, HttpStatusCode.OK);
                    Assert.IsNotNull(walletResponse.Content);
                    string responseContent = await walletResponse.Content.ReadAsStringAsync();
                    Assert.AreEqual(responseContent, expectedWalletResponse);
                }
            };

            await GetRequest(
                url,
                null,
                null,
                (responseCode, responseBody, responseHeaders) =>
                {
                    Assert.AreEqual(HttpStatusCode.OK, responseCode);
                    Assert.IsNotNull(responseBody);
                    Assert.AreEqual(pxResponse, responseBody, $"responseBody is {responseBody}, which is not expected");
                });

            PXSettings.WalletService.ResetToDefaults();
        }

        [DataRow(true)]
        [DataRow(false)]
        [TestMethod]
        public async Task GetWalletConfig_cache(bool disableCache)
        {
            string url = $"/v7.0/getWalletConfig";
            string expectedWalletResponse = "{\"merchantName\":\"Microsoft\",\"integrationType\":\"DIRECT\",\"directIntegrationData\":[{\"piFamily\":\"Ewallet\",\"piType\":\"ApplePay\",\"countrySupportedNetworks\":{\"us\":[\"visa\",\"mastercard\",\"discover\",\"amex\"],\"ca\":[\"visa\",\"mastercard\",\"discover\",\"amex\"],\"fr\":[\"visa\",\"mastercard\",\"amex\"],\"de\":[\"visa\",\"mastercard\",\"amex\"],\"it\":[\"visa\",\"mastercard\",\"amex\"],\"es\":[\"visa\",\"mastercard\",\"amex\"],\"gb\":[\"visa\",\"mastercard\",\"amex\"]},\"providerData\":{\"merchantIdentifier\":\"merchant.com.microsoft.paymicrosoft.sandbox\",\"merchantCapabilities\":[\"supports3DS\",\"supportsEMV\"],\"version\":\"MASKED(1)\"}},{\"piFamily\":\"Ewallet\",\"piType\":\"GooglePay\",\"countrySupportedNetworks\":{\"us\":[\"visa\",\"mastercard\",\"discover\",\"amex\"],\"ca\":[\"visa\",\"mastercard\",\"discover\",\"amex\"],\"fr\":[\"visa\",\"mastercard\",\"amex\"],\"de\":[\"visa\",\"mastercard\",\"amex\"],\"it\":[\"visa\",\"mastercard\",\"amex\"],\"es\":[\"visa\",\"mastercard\",\"amex\"],\"gb\":[\"visa\",\"mastercard\",\"amex\"]},\"providerData\":{\"allowedMethods\":[\"PAN_ONLY\",\"CRYPTOGRAM_3DS\"],\"assuranceDetailsRequired\":true,\"protocolVersion\":\"ECv2\",\"publicKey\":\"MASKED(88)\",\"publicKeyVersion\":\"01032025\",\"apiMajorVersion\":2,\"apiMinorVersion\":0,\"merchantId\":\"BCR2DN4TZ244PH2A\"}}]}";
            string expectedPIMSResponse = "[{\"paymentMethodType\":\"applepay\",\"properties\":{\"offlineRecurring\":true,\"userManaged\":false,\"chargeThresholds\":null,\"redirectRequired\":null,\"soldToAddressRequired\":true,\"splitPaymentSupported\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"providerCountry\":null,\"nonStoredPaymentMethodId\":\"be4de87d-7e38-4b2d-8836-9237eb32848e\",\"isNonStoredPaymentMethod\":true},\"paymentMethodGroup\":\"ewallet\",\"groupDisplayName\":\"eWallet\",\"exclusionTags\":null,\"paymentMethodFamily\":\"ewallet\",\"display\":{\"name\":\"ApplePay\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_applepay.svg\",\"logos\":[{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_applepay.svg\"}]},\"AdditionalDisplayText\":null},{\"paymentMethodType\":\"googlepay\",\"properties\":{\"offlineRecurring\":true,\"userManaged\":false,\"chargeThresholds\":null,\"redirectRequired\":null,\"soldToAddressRequired\":true,\"splitPaymentSupported\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"providerCountry\":null,\"nonStoredPaymentMethodId\":\"cdc85313-9b57-4052-81fb-dea336132cbf\",\"isNonStoredPaymentMethod\":true},\"paymentMethodGroup\":\"ewallet\",\"groupDisplayName\":\"eWallet\",\"exclusionTags\":null,\"paymentMethodFamily\":\"ewallet\",\"display\":{\"name\":\"GooglePay\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_googlepay.svg\",\"logos\":[{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_googlepay.svg\"}]},\"AdditionalDisplayText\":null}]";
            string pxResponse = "{\"PIDLConfig\":{\"SelectResource.PaymentInstrument\":{\"actions\":{\"ewallet.applepay.default\":[\"PaymentInstrumentHandler.ClientSupported\"],\"ewallet.googlepay.default\":[\"PaymentInstrumentHandler.ClientSupported\"]}},\"HandlePaymentChallenge\":{\"actions\":{\"ewallet.applepay.default\":[\"PaymentInstrumentHandler.CollectPaymentToken\"],\"ewallet.googlepay.default\":[\"PaymentInstrumentHandler.CollectPaymentToken\"]}}},\"PaymentInstrumentHandlers\":[{\"allowedAuthMethods\":[\"PAN_ONLY\",\"CRYPTOGRAM_3DS\"],\"protocolVersion\":\"ECv2\",\"publicKey\":\"MASKED(88)\",\"merchantName\":\"Microsoft\",\"apiMajorVersion\":\"2\",\"apiMinorVersion\":\"0\",\"assuranceDetailsRequired\":true,\"publicKeyVersion\":\"01032025\",\"enableGPayIframeForAllBrowsers\":false,\"merchantId\":\"BCR2DN4TZ244PH2A\",\"paymentMethodFamily\":\"ewallet\",\"paymentMethodType\":\"googlepay\",\"piid\":\"cdc85313-9b57-4052-81fb-dea336132cbf\",\"payLabel\":\"amount due plus applicable taxes\",\"integrationType\":\"DIRECT\",\"allowedAuthMethodsPerCountry\":{\"us\":[\"VISA\",\"MASTERCARD\",\"DISCOVER\",\"AMEX\"],\"ca\":[\"VISA\",\"MASTERCARD\",\"DISCOVER\",\"AMEX\"],\"fr\":[\"VISA\",\"MASTERCARD\",\"AMEX\"],\"de\":[\"VISA\",\"MASTERCARD\",\"AMEX\"],\"it\":[\"VISA\",\"MASTERCARD\",\"AMEX\"],\"es\":[\"VISA\",\"MASTERCARD\",\"AMEX\"],\"gb\":[\"VISA\",\"MASTERCARD\",\"AMEX\"]},\"clientSupported\":{\"supportedBrowsers\":{\"chrome\":\"100\",\"edge\":\"100\"},\"supportedOS\":{\"ios\":\"16.0\",\"android\":\"16.0\",\"windows\":\"16.0\"},\"additionalAPIsCheck\":[\"canMakePayment\"]},\"enableBillingAddress\":false,\"enableEmail\":false,\"disableGeoFencing\":false,\"singleMarkets\":[\"AT\",\"BE\",\"BG\",\"CH\",\"CY\",\"CZ\",\"DE\",\"DK\",\"EE\",\"ES\",\"FI\",\"FR\",\"GB\",\"GR\",\"HR\",\"HU\",\"IE\",\"IS\",\"IT\",\"LI\",\"LT\",\"LU\",\"LV\",\"MT\",\"NL\",\"NO\",\"PL\",\"PT\",\"RO\",\"SE\",\"SI\",\"SK\"]},{\"merchantCapabilities\":[\"supports3DS\",\"supportsEMV\"],\"displayName\":\"Microsoft\",\"initiative\":\"Web\",\"initiativeContext\":\"mystore.example.com\",\"merchantIdentifier\":\"merchant.com.microsoft.paymicrosoft.sandbox\",\"applePayVersion\":\"MASKED(1)\",\"paymentMethodFamily\":\"ewallet\",\"paymentMethodType\":\"applepay\",\"piid\":\"be4de87d-7e38-4b2d-8836-9237eb32848e\",\"payLabel\":\"amount due plus applicable taxes\",\"integrationType\":\"DIRECT\",\"allowedAuthMethodsPerCountry\":{\"us\":[\"VISA\",\"MASTERCARD\",\"DISCOVER\",\"AMEX\"],\"ca\":[\"VISA\",\"MASTERCARD\",\"DISCOVER\",\"AMEX\"],\"fr\":[\"VISA\",\"MASTERCARD\",\"AMEX\"],\"de\":[\"VISA\",\"MASTERCARD\",\"AMEX\"],\"it\":[\"VISA\",\"MASTERCARD\",\"AMEX\"],\"es\":[\"VISA\",\"MASTERCARD\",\"AMEX\"],\"gb\":[\"VISA\",\"MASTERCARD\",\"AMEX\"]},\"clientSupported\":{\"supportedBrowsers\":{\"safari\":\"16.1\"},\"supportedOS\":{\"ios\":\"15.0\"},\"additionalAPIsCheck\":[\"canMakePaymentWithActiveCard\"],\"paymentProxyRequired\":{\"safari\":\"16.5\"}},\"enableBillingAddress\":false,\"enableEmail\":false,\"disableGeoFencing\":false,\"singleMarkets\":[\"AT\",\"BE\",\"BG\",\"CH\",\"CY\",\"CZ\",\"DE\",\"DK\",\"EE\",\"ES\",\"FI\",\"FR\",\"GB\",\"GR\",\"HR\",\"HU\",\"IE\",\"IS\",\"IT\",\"LI\",\"LT\",\"LU\",\"LV\",\"MT\",\"NL\",\"NO\",\"PL\",\"PT\",\"RO\",\"SE\",\"SI\",\"SK\"]}]}";

            PXSettings.PimsService.ArrangeResponse(expectedPIMSResponse);
            if (disableCache)
            {
                PXFlightHandler.AddToEnabledFlights("PXDisableGetWalletConfigCache");
            }

            int walletServiceCalledCount = 0;
            PXSettings.WalletService.PreProcess = (walletRequest) =>
            {
                Uri requestUri = walletRequest.RequestUri;
                Assert.IsTrue(requestUri.PathAndQuery.Contains("/wallet"));
                Assert.IsNull(walletRequest.Content);
                walletServiceCalledCount += 1;
            };

            PXSettings.WalletService.PostProcess = async (walletResponse) =>
            {
                if (walletResponse != null)
                {
                    Assert.AreEqual(walletResponse.StatusCode, HttpStatusCode.OK);
                    Assert.IsNotNull(walletResponse.Content);
                    string responseContent = await walletResponse.Content.ReadAsStringAsync();
                    Assert.AreEqual(responseContent, expectedWalletResponse);
                }
            };

            await GetRequest(
                url,
                null,
                null,
                (responseCode, responseBody, responseHeaders) =>
                {
                    Assert.AreEqual(HttpStatusCode.OK, responseCode);
                    Assert.IsNotNull(responseBody);
                    Assert.AreEqual(pxResponse, responseBody, $"responseBody is {responseBody}, which is not expected");
                    if (disableCache)
                    {
                        Assert.AreEqual(1, walletServiceCalledCount);
                    }
                    else
                    {
                        Assert.IsTrue(walletServiceCalledCount <= 1);
                    }
                });

            // call again to test cache, wallet service should not be called again
            await GetRequest(
                url,
                null,
                null,
                (responseCode, responseBody, responseHeaders) =>
                {
                    Assert.AreEqual(HttpStatusCode.OK, responseCode);
                    Assert.IsNotNull(responseBody);
                    Assert.AreEqual(pxResponse, responseBody, $"responseBody is {responseBody}, which is not expected");
                    if (disableCache)
                    {
                        Assert.AreEqual(2, walletServiceCalledCount);
                    }
                    else
                    {
                        Assert.IsTrue(walletServiceCalledCount <= 1);
                    }
                });

            PXSettings.WalletService.ResetToDefaults();
        }

        [DataRow("partner=webblends&client=%7B%22isCrossOrigin%22:true%7D")]
        [DataRow("partner=webblends&client=%7B%22isCrossOrigin%22:true")]
        [DataTestMethod]
        public async Task GetWalletConfig_WithParams(string queries)
        {
            string url = $"/v7.0/getWalletConfig?{queries}";
            string expectedWalletResponse = "{\"merchantName\":\"Microsoft\",\"integrationType\":\"DIRECT\",\"directIntegrationData\":[{\"piFamily\":\"Ewallet\",\"piType\":\"ApplePay\",\"countrySupportedNetworks\":{\"us\":[\"visa\",\"mastercard\",\"discover\",\"amex\"],\"ca\":[\"visa\",\"mastercard\",\"discover\",\"amex\"]},\"providerData\":{\"merchantIdentifier\":\"merchant.com.microsoft.paymicrosoft.sandbox\",\"merchantCapabilities\":[\"supports3DS\",\"supportsEMV\"],\"version\":\"3\"}},{\"piFamily\":\"Ewallet\",\"piType\":\"GooglePay\",\"countrySupportedNetworks\":{\"us\":[\"visa\",\"mastercard\",\"discover\",\"amex\"],\"ca\":[\"visa\",\"mastercard\",\"discover\",\"amex\"]},\"providerData\":{\"allowedMethods\":[\"PAN_ONLY\",\"CRYPTOGRAM_3DS\"],\"assuranceDetailsRequired\":true,\"protocolVersion\":\"ECv2\",\"publicKey\":\"BDRvcFHptepUygYllpEmnHNb4rfG4oBU/sVcAVMn9I7Pu3wYpDpjhBeoF+i30iB2HUQGyZEUsodGdhphaD/j6rU=\",\"publicKeyVersion\":\"1\",\"apiMajorVersion\":2,\"apiMinorVersion\":0,\"merchantId\":\"BCR2DN4TZ244PH2A\"}}]}";
            string expectedPIMSResponse = "[{\"paymentMethodType\":\"applepay\",\"properties\":{\"offlineRecurring\":true,\"userManaged\":false,\"chargeThresholds\":null,\"redirectRequired\":null,\"soldToAddressRequired\":true,\"splitPaymentSupported\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"providerCountry\":null,\"nonStoredPaymentMethodId\":\"be4de87d-7e38-4b2d-8836-9237eb32848e\",\"isNonStoredPaymentMethod\":true},\"paymentMethodGroup\":\"ewallet\",\"groupDisplayName\":\"eWallet\",\"exclusionTags\":null,\"paymentMethodFamily\":\"ewallet\",\"display\":{\"name\":\"ApplePay\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_applepay.svg\",\"logos\":[{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_applepay.svg\"}]},\"AdditionalDisplayText\":null},{\"paymentMethodType\":\"googlepay\",\"properties\":{\"offlineRecurring\":true,\"userManaged\":false,\"chargeThresholds\":null,\"redirectRequired\":null,\"soldToAddressRequired\":true,\"splitPaymentSupported\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"providerCountry\":null,\"nonStoredPaymentMethodId\":\"cdc85313-9b57-4052-81fb-dea336132cbf\",\"isNonStoredPaymentMethod\":true},\"paymentMethodGroup\":\"ewallet\",\"groupDisplayName\":\"eWallet\",\"exclusionTags\":null,\"paymentMethodFamily\":\"ewallet\",\"display\":{\"name\":\"GooglePay\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_googlepay.svg\",\"logos\":[{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_googlepay.svg\"}]},\"AdditionalDisplayText\":null}]";
            string pxResponse = "{\"PIDLConfig\":{\"SelectResource.PaymentInstrument\":{\"actions\":{\"ewallet.applepay.default\":[\"PaymentInstrumentHandler.ClientSupported\"],\"ewallet.googlepay.default\":[\"PaymentInstrumentHandler.ClientSupported\"]}},\"HandlePaymentChallenge\":{\"actions\":{\"ewallet.applepay.default\":[\"PaymentInstrumentHandler.CollectPaymentToken\"],\"ewallet.googlepay.default\":[\"PaymentInstrumentHandler.CollectPaymentToken\"]}}},\"PaymentInstrumentHandlers\":[{\"allowedAuthMethods\":[\"PAN_ONLY\",\"CRYPTOGRAM_3DS\"],\"protocolVersion\":\"ECv2\",\"publicKey\":\"BDRvcFHptepUygYllpEmnHNb4rfG4oBU/sVcAVMn9I7Pu3wYpDpjhBeoF+i30iB2HUQGyZEUsodGdhphaD/j6rU=\",\"merchantName\":\"Microsoft\",\"apiMajorVersion\":\"2\",\"apiMinorVersion\":\"0\",\"assuranceDetailsRequired\":true,\"publicKeyVersion\":\"1\",\"enableGPayIframeForAllBrowsers\":false,\"merchantId\":\"BCR2DN4TZ244PH2A\",\"paymentMethodFamily\":\"ewallet\",\"paymentMethodType\":\"googlepay\",\"piid\":\"cdc85313-9b57-4052-81fb-dea336132cbf\",\"payLabel\":\"amount due plus applicable taxes\",\"integrationType\":\"DIRECT\",\"allowedAuthMethodsPerCountry\":{\"us\":[\"VISA\",\"MASTERCARD\",\"DISCOVER\",\"AMEX\"],\"ca\":[\"VISA\",\"MASTERCARD\",\"DISCOVER\",\"AMEX\"]},\"clientSupported\":{\"supportedBrowsers\":{\"chrome\":\"100\",\"edge\":\"100\"},\"supportedOS\":{\"ios\":\"16.0\",\"android\":\"16.0\",\"windows\":\"16.0\"},\"additionalAPIsCheck\":[\"canMakePayment\"]},\"enableBillingAddress\":false,\"enableEmail\":false,\"disableGeoFencing\":false,\"singleMarkets\":[\"AT\",\"BE\",\"BG\",\"CH\",\"CY\",\"CZ\",\"DE\",\"DK\",\"EE\",\"ES\",\"FI\",\"FR\",\"GB\",\"GR\",\"HR\",\"HU\",\"IE\",\"IS\",\"IT\",\"LI\",\"LT\",\"LU\",\"LV\",\"MT\",\"NL\",\"NO\",\"PL\",\"PT\",\"RO\",\"SE\",\"SI\",\"SK\"]},{\"merchantCapabilities\":[\"supports3DS\",\"supportsEMV\"],\"displayName\":\"Microsoft\",\"initiative\":\"Web\",\"initiativeContext\":\"mystore.example.com\",\"merchantIdentifier\":\"merchant.com.microsoft.paymicrosoft.sandbox\",\"applePayVersion\":\"3\",\"paymentMethodFamily\":\"ewallet\",\"paymentMethodType\":\"applepay\",\"piid\":\"be4de87d-7e38-4b2d-8836-9237eb32848e\",\"payLabel\":\"amount due plus applicable taxes\",\"integrationType\":\"DIRECT\",\"allowedAuthMethodsPerCountry\":{\"us\":[\"VISA\",\"MASTERCARD\",\"DISCOVER\",\"AMEX\"],\"ca\":[\"VISA\",\"MASTERCARD\",\"DISCOVER\",\"AMEX\"]},\"clientSupported\":{\"supportedBrowsers\":{\"safari\":\"16.1\"},\"supportedOS\":{\"ios\":\"15.0\"},\"additionalAPIsCheck\":[\"canMakePaymentWithActiveCard\"],\"paymentProxyRequired\":{\"safari\":\"16.5\"}},\"enableBillingAddress\":false,\"enableEmail\":false,\"disableGeoFencing\":false,\"singleMarkets\":[\"AT\",\"BE\",\"BG\",\"CH\",\"CY\",\"CZ\",\"DE\",\"DK\",\"EE\",\"ES\",\"FI\",\"FR\",\"GB\",\"GR\",\"HR\",\"HU\",\"IE\",\"IS\",\"IT\",\"LI\",\"LT\",\"LU\",\"LV\",\"MT\",\"NL\",\"NO\",\"PL\",\"PT\",\"RO\",\"SE\",\"SI\",\"SK\"]}]}";

            PXSettings.WalletService.ArrangeResponse(expectedWalletResponse);
            PXSettings.PimsService.ArrangeResponse(expectedPIMSResponse);
            PXFlightHandler.AddToEnabledFlights("PXDisableGetWalletConfigCache");

            PXSettings.WalletService.PreProcess = (walletRequest) =>
            {
                Uri requestUri = walletRequest.RequestUri;
                Assert.IsTrue(requestUri.PathAndQuery.Contains("/wallet"));
                Assert.IsNull(walletRequest.Content);
            };

            PXSettings.WalletService.PostProcess = async (walletResponse) =>
            {
                if (walletResponse != null)
                {
                    Assert.AreEqual(walletResponse.StatusCode, HttpStatusCode.OK);
                    Assert.IsNotNull(walletResponse.Content);
                    string responseContent = await walletResponse.Content.ReadAsStringAsync();
                    Assert.AreEqual(responseContent, expectedWalletResponse);
                }
            };

            await GetRequest(
                url,
                null,
                null,
                (responseCode, responseBody, responseHeaders) =>
                {
                    Assert.AreEqual(HttpStatusCode.OK, responseCode);
                    Assert.IsNotNull(responseBody);
                    Assert.AreEqual(pxResponse, responseBody, $"responseBody is {responseBody}, which is not expected");
                });

            PXSettings.WalletService.ResetToDefaults();
        }

        [DataRow("", "testPartner", "client=%7B%22isCrossOrigin%22:true%7D", false, true)]
        [DataRow("", "testPartner", "client=%7B%22isCrossOrigin%22:false%7D", true, true)]
        [DataRow("", "testPartner", "", true, true)]
        [DataRow("", "testPartner", "", true, true)]
        [DataTestMethod]
        public async Task GetWalletConfig_DeviceSupportForPayments(string userAgent, string partner, string queryParameters, bool expectedApayResult, bool expectedGpayResult)
        {
            List<string> flightList = new List<string>()
            {
                "PXWalletConfigAddDeviceSupportStatus",
                "PXDisableGetWalletConfigCache"
            };

            string url = $"/v7.0/getWalletConfig?{queryParameters}";
            string expectedWalletResponse = "{\"merchantName\":\"Microsoft\",\"integrationType\":\"DIRECT\",\"directIntegrationData\":[{\"piFamily\":\"Ewallet\",\"piType\":\"ApplePay\",\"countrySupportedNetworks\":{\"us\":[\"visa\",\"mastercard\",\"discover\",\"amex\"],\"ca\":[\"visa\",\"mastercard\",\"discover\",\"amex\"]},\"providerData\":{\"merchantIdentifier\":\"merchant.com.microsoft.paymicrosoft.sandbox\",\"merchantCapabilities\":[\"supports3DS\",\"supportsEMV\"],\"version\":\"3\"}},{\"piFamily\":\"Ewallet\",\"piType\":\"GooglePay\",\"countrySupportedNetworks\":{\"us\":[\"visa\",\"mastercard\",\"discover\",\"amex\"],\"ca\":[\"visa\",\"mastercard\",\"discover\",\"amex\"]},\"providerData\":{\"allowedMethods\":[\"PAN_ONLY\",\"CRYPTOGRAM_3DS\"],\"assuranceDetailsRequired\":true,\"protocolVersion\":\"ECv2\",\"publicKey\":\"BDRvcFHptepUygYllpEmnHNb4rfG4oBU/sVcAVMn9I7Pu3wYpDpjhBeoF+i30iB2HUQGyZEUsodGdhphaD/j6rU=\",\"publicKeyVersion\":\"1\",\"apiMajorVersion\":2,\"apiMinorVersion\":0,\"merchantId\":\"BCR2DN4TZ244PH2A\"}}]}";
            string expectedPIMSResponse = "[{\"paymentMethodType\":\"applepay\",\"properties\":{\"offlineRecurring\":true,\"userManaged\":false,\"chargeThresholds\":null,\"redirectRequired\":null,\"soldToAddressRequired\":true,\"splitPaymentSupported\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"providerCountry\":null,\"nonStoredPaymentMethodId\":\"be4de87d-7e38-4b2d-8836-9237eb32848e\",\"isNonStoredPaymentMethod\":true},\"paymentMethodGroup\":\"ewallet\",\"groupDisplayName\":\"eWallet\",\"exclusionTags\":null,\"paymentMethodFamily\":\"ewallet\",\"display\":{\"name\":\"ApplePay\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_applepay.svg\",\"logos\":[{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_applepay.svg\"}]},\"AdditionalDisplayText\":null},{\"paymentMethodType\":\"googlepay\",\"properties\":{\"offlineRecurring\":true,\"userManaged\":false,\"chargeThresholds\":null,\"redirectRequired\":null,\"soldToAddressRequired\":true,\"splitPaymentSupported\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"providerCountry\":null,\"nonStoredPaymentMethodId\":\"cdc85313-9b57-4052-81fb-dea336132cbf\",\"isNonStoredPaymentMethod\":true},\"paymentMethodGroup\":\"ewallet\",\"groupDisplayName\":\"eWallet\",\"exclusionTags\":null,\"paymentMethodFamily\":\"ewallet\",\"display\":{\"name\":\"GooglePay\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_googlepay.svg\",\"logos\":[{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_googlepay.svg\"}]},\"AdditionalDisplayText\":null}]";

            PXSettings.WalletService.ArrangeResponse(expectedWalletResponse);
            PXSettings.PimsService.ArrangeResponse(expectedPIMSResponse);

            var headers = new Dictionary<string, string>();
            headers.Add("x-ms-clientcontext-encoding", "base64");
            headers.Add("x-ms-deviceinfo", GetDeviceInfo(userAgent));

            await GetRequest(
                url,
                headers,
                flightList,
                (responseCode, responseBody, responseHeaders) =>
                {
                    Assert.AreEqual(HttpStatusCode.OK, responseCode, $"Unexpected responsecode {responseCode}");
                    Assert.IsNotNull(responseBody);

                    WalletConfig walletConfig = JsonConvert.DeserializeObject<WalletConfig>(responseBody);
                    Assert.IsNotNull(walletConfig, "wallet config is null");
                    Assert.IsNotNull(walletConfig.PaymentInstrumentHandlers, "payment instrument handlers is null");

                    var gpayHandler = walletConfig.PaymentInstrumentHandlers[0];
                    var aPayHandler = walletConfig.PaymentInstrumentHandlers[1];

                    Assert.IsNotNull(gpayHandler, "gpayhandler is null");
                    Assert.IsNotNull(aPayHandler, "apayhandler is null");
                    Assert.IsNotNull(gpayHandler.DisableGeoFencing);
                    Assert.IsFalse(gpayHandler.DisableGeoFencing, "DisableGeoFencing should be false for express checkout");
                    Assert.IsNotNull(gpayHandler.SingleMarkets);
                    Assert.IsTrue(gpayHandler.SingleMarkets.Count > 0, "SingleMarkets should not be empty for express checkout");
                    Assert.IsNotNull(aPayHandler.DisableGeoFencing);
                    Assert.IsFalse(aPayHandler.DisableGeoFencing, "DisableGeoFencing should be false for express checkout");
                    Assert.IsNotNull(aPayHandler.SingleMarkets);
                    Assert.IsTrue(aPayHandler.SingleMarkets.Count > 0, "SingleMarkets should not be empty for express checkout");

                    Assert.IsNotNull(gpayHandler.DeviceSupportedStatus, "gpay DeviceSupported is null");
                    Assert.IsNotNull(aPayHandler.DeviceSupportedStatus, "apay DeviceSupported is null");

                    Assert.AreEqual(gpayHandler.DeviceSupportedStatus.Result, expectedGpayResult, $"gpayhandler result should be {expectedGpayResult}");
                    Assert.AreEqual(aPayHandler.DeviceSupportedStatus.Result, expectedApayResult, $"apayhandler result should be {expectedApayResult}");
                });
        }

        [TestMethod]
        public async Task GetWalletConfig_GpayIframeflight()
        {
            string url = $"/v7.0/getWalletConfig";
            string expectedWalletResponse = "{\"merchantName\":\"Microsoft\",\"integrationType\":\"DIRECT\",\"directIntegrationData\":[{\"piFamily\":\"Ewallet\",\"piType\":\"ApplePay\",\"countrySupportedNetworks\":{\"us\":[\"visa\",\"mastercard\",\"discover\",\"amex\"],\"ca\":[\"visa\",\"mastercard\",\"discover\",\"amex\"]},\"providerData\":{\"merchantIdentifier\":\"merchant.com.microsoft.paymicrosoft.sandbox\",\"merchantCapabilities\":[\"supports3DS\",\"supportsEMV\"],\"version\":\"3\"}},{\"piFamily\":\"Ewallet\",\"piType\":\"GooglePay\",\"countrySupportedNetworks\":{\"us\":[\"visa\",\"mastercard\",\"discover\",\"amex\"],\"ca\":[\"visa\",\"mastercard\",\"discover\",\"amex\"]},\"providerData\":{\"allowedMethods\":[\"PAN_ONLY\",\"CRYPTOGRAM_3DS\"],\"assuranceDetailsRequired\":true,\"protocolVersion\":\"ECv2\",\"publicKey\":\"BDRvcFHptepUygYllpEmnHNb4rfG4oBU/sVcAVMn9I7Pu3wYpDpjhBeoF+i30iB2HUQGyZEUsodGdhphaD/j6rU=\",\"publicKeyVersion\":\"1\",\"apiMajorVersion\":2,\"apiMinorVersion\":0,\"merchantId\":\"BCR2DN4TZ244PH2A\"}}]}";
            string expectedPIMSResponse = "[{\"paymentMethodType\":\"applepay\",\"properties\":{\"offlineRecurring\":true,\"userManaged\":false,\"chargeThresholds\":null,\"redirectRequired\":null,\"soldToAddressRequired\":true,\"splitPaymentSupported\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"providerCountry\":null,\"nonStoredPaymentMethodId\":\"be4de87d-7e38-4b2d-8836-9237eb32848e\",\"isNonStoredPaymentMethod\":true},\"paymentMethodGroup\":\"ewallet\",\"groupDisplayName\":\"eWallet\",\"exclusionTags\":null,\"paymentMethodFamily\":\"ewallet\",\"display\":{\"name\":\"ApplePay\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_applepay.svg\",\"logos\":[{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_applepay.svg\"}]},\"AdditionalDisplayText\":null},{\"paymentMethodType\":\"googlepay\",\"properties\":{\"offlineRecurring\":true,\"userManaged\":false,\"chargeThresholds\":null,\"redirectRequired\":null,\"soldToAddressRequired\":true,\"splitPaymentSupported\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"providerCountry\":null,\"nonStoredPaymentMethodId\":\"cdc85313-9b57-4052-81fb-dea336132cbf\",\"isNonStoredPaymentMethod\":true},\"paymentMethodGroup\":\"ewallet\",\"groupDisplayName\":\"eWallet\",\"exclusionTags\":null,\"paymentMethodFamily\":\"ewallet\",\"display\":{\"name\":\"GooglePay\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_googlepay.svg\",\"logos\":[{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_googlepay.svg\"}]},\"AdditionalDisplayText\":null}]";
            string pxResponse = "{\"PIDLConfig\":{\"SelectResource.PaymentInstrument\":{\"actions\":{\"ewallet.applepay.default\":[\"PaymentInstrumentHandler.ClientSupported\"],\"ewallet.googlepay.default\":[\"PaymentInstrumentHandler.ClientSupported\"]}},\"HandlePaymentChallenge\":{\"actions\":{\"ewallet.applepay.default\":[\"PaymentInstrumentHandler.CollectPaymentToken\"],\"ewallet.googlepay.default\":[\"PaymentInstrumentHandler.CollectPaymentToken\"]}}},\"PaymentInstrumentHandlers\":[{\"allowedAuthMethods\":[\"PAN_ONLY\",\"CRYPTOGRAM_3DS\"],\"protocolVersion\":\"ECv2\",\"publicKey\":\"BDRvcFHptepUygYllpEmnHNb4rfG4oBU/sVcAVMn9I7Pu3wYpDpjhBeoF+i30iB2HUQGyZEUsodGdhphaD/j6rU=\",\"merchantName\":\"Microsoft\",\"apiMajorVersion\":\"2\",\"apiMinorVersion\":\"0\",\"assuranceDetailsRequired\":true,\"publicKeyVersion\":\"1\",\"enableGPayIframeForAllBrowsers\":true,\"merchantId\":\"BCR2DN4TZ244PH2A\",\"paymentMethodFamily\":\"ewallet\",\"paymentMethodType\":\"googlepay\",\"piid\":\"cdc85313-9b57-4052-81fb-dea336132cbf\",\"payLabel\":\"amount due plus applicable taxes\",\"integrationType\":\"DIRECT\",\"allowedAuthMethodsPerCountry\":{\"us\":[\"VISA\",\"MASTERCARD\",\"DISCOVER\",\"AMEX\"],\"ca\":[\"VISA\",\"MASTERCARD\",\"DISCOVER\",\"AMEX\"]},\"clientSupported\":{\"supportedBrowsers\":{\"chrome\":\"100\",\"edge\":\"100\"},\"supportedOS\":{\"ios\":\"16.0\",\"android\":\"16.0\",\"windows\":\"16.0\"},\"additionalAPIsCheck\":[\"canMakePayment\"]},\"enableBillingAddress\":false,\"enableEmail\":false,\"disableGeoFencing\":false,\"singleMarkets\":[\"AT\",\"BE\",\"BG\",\"CH\",\"CY\",\"CZ\",\"DE\",\"DK\",\"EE\",\"ES\",\"FI\",\"FR\",\"GB\",\"GR\",\"HR\",\"HU\",\"IE\",\"IS\",\"IT\",\"LI\",\"LT\",\"LU\",\"LV\",\"MT\",\"NL\",\"NO\",\"PL\",\"PT\",\"RO\",\"SE\",\"SI\",\"SK\"]},{\"merchantCapabilities\":[\"supports3DS\",\"supportsEMV\"],\"displayName\":\"Microsoft\",\"initiative\":\"Web\",\"initiativeContext\":\"mystore.example.com\",\"merchantIdentifier\":\"merchant.com.microsoft.paymicrosoft.sandbox\",\"applePayVersion\":\"3\",\"paymentMethodFamily\":\"ewallet\",\"paymentMethodType\":\"applepay\",\"piid\":\"be4de87d-7e38-4b2d-8836-9237eb32848e\",\"payLabel\":\"amount due plus applicable taxes\",\"integrationType\":\"DIRECT\",\"allowedAuthMethodsPerCountry\":{\"us\":[\"VISA\",\"MASTERCARD\",\"DISCOVER\",\"AMEX\"],\"ca\":[\"VISA\",\"MASTERCARD\",\"DISCOVER\",\"AMEX\"]},\"clientSupported\":{\"supportedBrowsers\":{\"safari\":\"16.1\"},\"supportedOS\":{\"ios\":\"15.0\"},\"additionalAPIsCheck\":[\"canMakePaymentWithActiveCard\"],\"paymentProxyRequired\":{\"safari\":\"16.5\"}},\"enableBillingAddress\":false,\"enableEmail\":false,\"disableGeoFencing\":false,\"singleMarkets\":[\"AT\",\"BE\",\"BG\",\"CH\",\"CY\",\"CZ\",\"DE\",\"DK\",\"EE\",\"ES\",\"FI\",\"FR\",\"GB\",\"GR\",\"HR\",\"HU\",\"IE\",\"IS\",\"IT\",\"LI\",\"LT\",\"LU\",\"LV\",\"MT\",\"NL\",\"NO\",\"PL\",\"PT\",\"RO\",\"SE\",\"SI\",\"SK\"]}]}";

            PXSettings.WalletService.ArrangeResponse(expectedWalletResponse);
            PXSettings.PimsService.ArrangeResponse(expectedPIMSResponse);
            PXFlightHandler.AddToEnabledFlights("PXDisableGetWalletConfigCache");

            PXSettings.WalletService.PreProcess = (walletRequest) =>
            {
                Uri requestUri = walletRequest.RequestUri;
                Assert.IsTrue(requestUri.PathAndQuery.Contains("/wallet"));
                Assert.IsNull(walletRequest.Content);
            };

            PXSettings.WalletService.PostProcess = async (walletResponse) =>
            {
                if (walletResponse != null)
                {
                    Assert.AreEqual(walletResponse.StatusCode, HttpStatusCode.OK);
                    Assert.IsNotNull(walletResponse.Content);
                    string responseContent = await walletResponse.Content.ReadAsStringAsync();
                    Assert.AreEqual(responseContent, expectedWalletResponse);
                }
            };

            await GetRequest(
                url,
                null,
                new List<string>() { "PXEnableGPayIframeForAllBrowsers", "PXDisableGetWalletConfigCache" },
                (responseCode, responseBody, responseHeaders) =>
                {
                    Assert.AreEqual(HttpStatusCode.OK, responseCode);
                    Assert.IsNotNull(responseBody);
                    Assert.AreEqual(pxResponse, responseBody, $"responseBody is {responseBody}, which is not expected");
                });

            PXSettings.WalletService.ResetToDefaults();
        }

        [TestMethod]
        public async Task GetWalletConfig_InstancePIflight()
        {
            string url = $"/v7.0/getWalletConfig";
            string expectedWalletResponse = "{\"merchantName\":\"Microsoft\",\"integrationType\":\"DIRECT\",\"directIntegrationData\":[{\"piFamily\":\"Ewallet\",\"piType\":\"ApplePay\",\"countrySupportedNetworks\":{\"us\":[\"visa\",\"mastercard\",\"discover\",\"amex\"],\"ca\":[\"visa\",\"mastercard\",\"discover\",\"amex\"]},\"providerData\":{\"merchantIdentifier\":\"merchant.com.microsoft.paymicrosoft.sandbox\",\"merchantCapabilities\":[\"supports3DS\",\"supportsEMV\"],\"version\":\"3\"}},{\"piFamily\":\"Ewallet\",\"piType\":\"GooglePay\",\"countrySupportedNetworks\":{\"us\":[\"visa\",\"mastercard\",\"discover\",\"amex\"],\"ca\":[\"visa\",\"mastercard\",\"discover\",\"amex\"]},\"providerData\":{\"allowedMethods\":[\"PAN_ONLY\",\"CRYPTOGRAM_3DS\"],\"assuranceDetailsRequired\":true,\"protocolVersion\":\"ECv2\",\"publicKey\":\"BDRvcFHptepUygYllpEmnHNb4rfG4oBU/sVcAVMn9I7Pu3wYpDpjhBeoF+i30iB2HUQGyZEUsodGdhphaD/j6rU=\",\"publicKeyVersion\":\"1\",\"apiMajorVersion\":2,\"apiMinorVersion\":0,\"merchantId\":\"BCR2DN4TZ244PH2A\"}}]}";
            string expectedPIMSResponse = "[{\"paymentMethodType\":\"applepay\",\"properties\":{\"offlineRecurring\":true,\"userManaged\":false,\"chargeThresholds\":null,\"redirectRequired\":null,\"soldToAddressRequired\":true,\"splitPaymentSupported\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"providerCountry\":null,\"nonStoredPaymentMethodId\":\"be4de87d-7e38-4b2d-8836-9237eb32848e\",\"isNonStoredPaymentMethod\":true},\"paymentMethodGroup\":\"ewallet\",\"groupDisplayName\":\"eWallet\",\"exclusionTags\":null,\"paymentMethodFamily\":\"ewallet\",\"display\":{\"name\":\"ApplePay\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_applepay.svg\",\"logos\":[{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_applepay.svg\"}]},\"AdditionalDisplayText\":null},{\"paymentMethodType\":\"googlepay\",\"properties\":{\"offlineRecurring\":true,\"userManaged\":false,\"chargeThresholds\":null,\"redirectRequired\":null,\"soldToAddressRequired\":true,\"splitPaymentSupported\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"providerCountry\":null,\"nonStoredPaymentMethodId\":\"cdc85313-9b57-4052-81fb-dea336132cbf\",\"isNonStoredPaymentMethod\":true},\"paymentMethodGroup\":\"ewallet\",\"groupDisplayName\":\"eWallet\",\"exclusionTags\":null,\"paymentMethodFamily\":\"ewallet\",\"display\":{\"name\":\"GooglePay\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_googlepay.svg\",\"logos\":[{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_googlepay.svg\"}]},\"AdditionalDisplayText\":null}]";
            string pxResponse = "{\"PIDLConfig\":{\"SelectResource.PaymentInstrument\":{\"actions\":{\"ewallet.applepay.default\":[\"PaymentInstrumentHandler.ClientSupported\"],\"ewallet.googlepay.default\":[\"PaymentInstrumentHandler.ClientSupported\"]}},\"HandlePaymentChallenge\":{\"actions\":{\"ewallet.applepay.default\":[\"PaymentInstrumentHandler.CollectPaymentToken\"],\"ewallet.googlepay.default\":[\"PaymentInstrumentHandler.CollectPaymentToken\"]}}},\"PaymentInstrumentHandlers\":[{\"allowedAuthMethods\":[\"PAN_ONLY\",\"CRYPTOGRAM_3DS\"],\"protocolVersion\":\"ECv2\",\"publicKey\":\"BDRvcFHptepUygYllpEmnHNb4rfG4oBU/sVcAVMn9I7Pu3wYpDpjhBeoF+i30iB2HUQGyZEUsodGdhphaD/j6rU=\",\"merchantName\":\"Microsoft\",\"apiMajorVersion\":\"2\",\"apiMinorVersion\":\"0\",\"assuranceDetailsRequired\":true,\"publicKeyVersion\":\"1\",\"enableGPayIframeForAllBrowsers\":false,\"merchantId\":\"BCR2DN4TZ244PH2A\",\"paymentMethodFamily\":\"ewallet\",\"paymentMethodType\":\"googlepay\",\"piid\":\"cdc85313-9b57-4052-81fb-dea336132cbf\",\"payLabel\":\"amount due plus applicable taxes\",\"integrationType\":\"DIRECT\",\"allowedAuthMethodsPerCountry\":{\"us\":[\"VISA\",\"MASTERCARD\",\"DISCOVER\",\"AMEX\"],\"ca\":[\"VISA\",\"MASTERCARD\",\"DISCOVER\",\"AMEX\"]},\"clientSupported\":{\"supportedBrowsers\":{\"chrome\":\"100\",\"edge\":\"100\"},\"supportedOS\":{\"ios\":\"16.0\",\"android\":\"16.0\",\"windows\":\"16.0\"},\"additionalAPIsCheck\":[\"canMakePayment\"]},\"enableBillingAddress\":false,\"enableEmail\":false,\"disableGeoFencing\":false,\"singleMarkets\":[\"AT\",\"BE\",\"BG\",\"CH\",\"CY\",\"CZ\",\"DE\",\"DK\",\"EE\",\"ES\",\"FI\",\"FR\",\"GB\",\"GR\",\"HR\",\"HU\",\"IE\",\"IS\",\"IT\",\"LI\",\"LT\",\"LU\",\"LV\",\"MT\",\"NL\",\"NO\",\"PL\",\"PT\",\"RO\",\"SE\",\"SI\",\"SK\"]},{\"merchantCapabilities\":[\"supports3DS\",\"supportsEMV\"],\"displayName\":\"Microsoft\",\"initiative\":\"Web\",\"initiativeContext\":\"mystore.example.com\",\"merchantIdentifier\":\"merchant.com.microsoft.paymicrosoft.sandbox\",\"applePayVersion\":\"3\",\"paymentMethodFamily\":\"ewallet\",\"paymentMethodType\":\"applepay\",\"piid\":\"be4de87d-7e38-4b2d-8836-9237eb32848e\",\"payLabel\":\"amount due plus applicable taxes\",\"integrationType\":\"DIRECT\",\"allowedAuthMethodsPerCountry\":{\"us\":[\"VISA\",\"MASTERCARD\",\"DISCOVER\",\"AMEX\"],\"ca\":[\"VISA\",\"MASTERCARD\",\"DISCOVER\",\"AMEX\"]},\"clientSupported\":{\"supportedBrowsers\":{\"safari\":\"16.1\"},\"supportedOS\":{\"ios\":\"15.0\"},\"additionalAPIsCheck\":[\"canMakePaymentWithActiveCard\"],\"paymentProxyRequired\":{\"safari\":\"16.5\"}},\"enableBillingAddress\":false,\"enableEmail\":false,\"disableGeoFencing\":false,\"singleMarkets\":[\"AT\",\"BE\",\"BG\",\"CH\",\"CY\",\"CZ\",\"DE\",\"DK\",\"EE\",\"ES\",\"FI\",\"FR\",\"GB\",\"GR\",\"HR\",\"HU\",\"IE\",\"IS\",\"IT\",\"LI\",\"LT\",\"LU\",\"LV\",\"MT\",\"NL\",\"NO\",\"PL\",\"PT\",\"RO\",\"SE\",\"SI\",\"SK\"]}]}";

            PXSettings.WalletService.ArrangeResponse(expectedWalletResponse);
            PXSettings.PimsService.ArrangeResponse(expectedPIMSResponse);

            PXSettings.WalletService.PreProcess = (walletRequest) =>
            {
                Uri requestUri = walletRequest.RequestUri;
                Assert.IsTrue(requestUri.PathAndQuery.Contains("/wallet"));
                Assert.IsNull(walletRequest.Content);
            };

            PXSettings.WalletService.PostProcess = async (walletResponse) =>
            {
                if (walletResponse != null)
                {
                    Assert.AreEqual(walletResponse.StatusCode, HttpStatusCode.OK);
                    Assert.IsNotNull(walletResponse.Content);
                    string responseContent = await walletResponse.Content.ReadAsStringAsync();
                    Assert.AreEqual(responseContent, expectedWalletResponse);
                }
            };

            await GetRequest(
                url,
                null,
                new List<string>() { "GPayApayInstancePI", "PXDisableGetWalletConfigCache" },
                (responseCode, responseBody, responseHeaders) =>
                {
                    Assert.AreEqual(HttpStatusCode.OK, responseCode);
                    Assert.IsNotNull(responseBody);
                    Assert.AreEqual(pxResponse, responseBody, $"responseBody is {responseBody}, which is not expected");
                });

            PXSettings.WalletService.ResetToDefaults();
        }

        [TestMethod]
        public async Task GetWalletConfig_InstancePI_PXWalletConfigEnableEmail_flight()
        {
            string url = $"/v7.0/getWalletConfig";
            string expectedWalletResponse = "{\"merchantName\":\"Microsoft\",\"integrationType\":\"DIRECT\",\"directIntegrationData\":[{\"piFamily\":\"Ewallet\",\"piType\":\"ApplePay\",\"countrySupportedNetworks\":{\"us\":[\"visa\",\"mastercard\",\"discover\",\"amex\"],\"ca\":[\"visa\",\"mastercard\",\"discover\",\"amex\"]},\"providerData\":{\"merchantIdentifier\":\"merchant.com.microsoft.paymicrosoft.sandbox\",\"merchantCapabilities\":[\"supports3DS\",\"supportsEMV\"],\"version\":\"3\"}},{\"piFamily\":\"Ewallet\",\"piType\":\"GooglePay\",\"countrySupportedNetworks\":{\"us\":[\"visa\",\"mastercard\",\"discover\",\"amex\"],\"ca\":[\"visa\",\"mastercard\",\"discover\",\"amex\"]},\"providerData\":{\"allowedMethods\":[\"PAN_ONLY\",\"CRYPTOGRAM_3DS\"],\"assuranceDetailsRequired\":true,\"protocolVersion\":\"ECv2\",\"publicKey\":\"BDRvcFHptepUygYllpEmnHNb4rfG4oBU/sVcAVMn9I7Pu3wYpDpjhBeoF+i30iB2HUQGyZEUsodGdhphaD/j6rU=\",\"publicKeyVersion\":\"1\",\"apiMajorVersion\":2,\"apiMinorVersion\":0,\"merchantId\":\"BCR2DN4TZ244PH2A\"}}]}";
            string expectedPIMSResponse = "[{\"paymentMethodType\":\"applepay\",\"properties\":{\"offlineRecurring\":true,\"userManaged\":false,\"chargeThresholds\":null,\"redirectRequired\":null,\"soldToAddressRequired\":true,\"splitPaymentSupported\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"providerCountry\":null,\"nonStoredPaymentMethodId\":\"be4de87d-7e38-4b2d-8836-9237eb32848e\",\"isNonStoredPaymentMethod\":true},\"paymentMethodGroup\":\"ewallet\",\"groupDisplayName\":\"eWallet\",\"exclusionTags\":null,\"paymentMethodFamily\":\"ewallet\",\"display\":{\"name\":\"ApplePay\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_applepay.svg\",\"logos\":[{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_applepay.svg\"}]},\"AdditionalDisplayText\":null},{\"paymentMethodType\":\"googlepay\",\"properties\":{\"offlineRecurring\":true,\"userManaged\":false,\"chargeThresholds\":null,\"redirectRequired\":null,\"soldToAddressRequired\":true,\"splitPaymentSupported\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"providerCountry\":null,\"nonStoredPaymentMethodId\":\"cdc85313-9b57-4052-81fb-dea336132cbf\",\"isNonStoredPaymentMethod\":true},\"paymentMethodGroup\":\"ewallet\",\"groupDisplayName\":\"eWallet\",\"exclusionTags\":null,\"paymentMethodFamily\":\"ewallet\",\"display\":{\"name\":\"GooglePay\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_googlepay.svg\",\"logos\":[{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_googlepay.svg\"}]},\"AdditionalDisplayText\":null}]";
            string pxResponse = "{\"PIDLConfig\":{\"SelectResource.PaymentInstrument\":{\"actions\":{\"ewallet.applepay.default\":[\"PaymentInstrumentHandler.ClientSupported\"],\"ewallet.googlepay.default\":[\"PaymentInstrumentHandler.ClientSupported\"]}},\"HandlePaymentChallenge\":{\"actions\":{\"ewallet.applepay.default\":[\"PaymentInstrumentHandler.CollectPaymentToken\"],\"ewallet.googlepay.default\":[\"PaymentInstrumentHandler.CollectPaymentToken\"]}}},\"PaymentInstrumentHandlers\":[{\"allowedAuthMethods\":[\"PAN_ONLY\",\"CRYPTOGRAM_3DS\"],\"protocolVersion\":\"ECv2\",\"publicKey\":\"BDRvcFHptepUygYllpEmnHNb4rfG4oBU/sVcAVMn9I7Pu3wYpDpjhBeoF+i30iB2HUQGyZEUsodGdhphaD/j6rU=\",\"merchantName\":\"Microsoft\",\"apiMajorVersion\":\"2\",\"apiMinorVersion\":\"0\",\"assuranceDetailsRequired\":true,\"publicKeyVersion\":\"1\",\"enableGPayIframeForAllBrowsers\":false,\"merchantId\":\"BCR2DN4TZ244PH2A\",\"paymentMethodFamily\":\"ewallet\",\"paymentMethodType\":\"googlepay\",\"piid\":\"cdc85313-9b57-4052-81fb-dea336132cbf\",\"payLabel\":\"amount due plus applicable taxes\",\"integrationType\":\"DIRECT\",\"allowedAuthMethodsPerCountry\":{\"us\":[\"VISA\",\"MASTERCARD\",\"DISCOVER\",\"AMEX\"],\"ca\":[\"VISA\",\"MASTERCARD\",\"DISCOVER\",\"AMEX\"]},\"clientSupported\":{\"supportedBrowsers\":{\"chrome\":\"100\",\"edge\":\"100\"},\"supportedOS\":{\"ios\":\"16.0\",\"android\":\"16.0\",\"windows\":\"16.0\"},\"additionalAPIsCheck\":[\"canMakePayment\"]},\"enableBillingAddress\":false,\"enableEmail\":true,\"disableGeoFencing\":false,\"singleMarkets\":[\"AT\",\"BE\",\"BG\",\"CH\",\"CY\",\"CZ\",\"DE\",\"DK\",\"EE\",\"ES\",\"FI\",\"FR\",\"GB\",\"GR\",\"HR\",\"HU\",\"IE\",\"IS\",\"IT\",\"LI\",\"LT\",\"LU\",\"LV\",\"MT\",\"NL\",\"NO\",\"PL\",\"PT\",\"RO\",\"SE\",\"SI\",\"SK\"]},{\"merchantCapabilities\":[\"supports3DS\",\"supportsEMV\"],\"displayName\":\"Microsoft\",\"initiative\":\"Web\",\"initiativeContext\":\"mystore.example.com\",\"merchantIdentifier\":\"merchant.com.microsoft.paymicrosoft.sandbox\",\"applePayVersion\":\"3\",\"paymentMethodFamily\":\"ewallet\",\"paymentMethodType\":\"applepay\",\"piid\":\"be4de87d-7e38-4b2d-8836-9237eb32848e\",\"payLabel\":\"amount due plus applicable taxes\",\"integrationType\":\"DIRECT\",\"allowedAuthMethodsPerCountry\":{\"us\":[\"VISA\",\"MASTERCARD\",\"DISCOVER\",\"AMEX\"],\"ca\":[\"VISA\",\"MASTERCARD\",\"DISCOVER\",\"AMEX\"]},\"clientSupported\":{\"supportedBrowsers\":{\"safari\":\"16.1\"},\"supportedOS\":{\"ios\":\"15.0\"},\"additionalAPIsCheck\":[\"canMakePaymentWithActiveCard\"],\"paymentProxyRequired\":{\"safari\":\"16.5\"}},\"enableBillingAddress\":false,\"enableEmail\":true,\"disableGeoFencing\":false,\"singleMarkets\":[\"AT\",\"BE\",\"BG\",\"CH\",\"CY\",\"CZ\",\"DE\",\"DK\",\"EE\",\"ES\",\"FI\",\"FR\",\"GB\",\"GR\",\"HR\",\"HU\",\"IE\",\"IS\",\"IT\",\"LI\",\"LT\",\"LU\",\"LV\",\"MT\",\"NL\",\"NO\",\"PL\",\"PT\",\"RO\",\"SE\",\"SI\",\"SK\"]}]}";

            PXSettings.WalletService.ArrangeResponse(expectedWalletResponse);
            PXSettings.PimsService.ArrangeResponse(expectedPIMSResponse);
            PXFlightHandler.AddToEnabledFlights("PXDisableGetWalletConfigCache");

            PXSettings.WalletService.PreProcess = (walletRequest) =>
            {
                Uri requestUri = walletRequest.RequestUri;
                Assert.IsTrue(requestUri.PathAndQuery.Contains("/wallet"));
                Assert.IsNull(walletRequest.Content);
            };

            PXSettings.WalletService.PostProcess = async (walletResponse) =>
            {
                if (walletResponse != null)
                {
                    Assert.AreEqual(walletResponse.StatusCode, HttpStatusCode.OK);
                    Assert.IsNotNull(walletResponse.Content);
                    string responseContent = await walletResponse.Content.ReadAsStringAsync();
                    Assert.AreEqual(responseContent, expectedWalletResponse);
                }
            };

            await GetRequest(
                url,
                null,
                new List<string>() { "GPayApayInstancePI", "PXWalletConfigEnableEmail", "PXDisableGetWalletConfigCache" },
                (responseCode, responseBody, responseHeaders) =>
                {
                    Assert.AreEqual(HttpStatusCode.OK, responseCode);
                    Assert.IsNotNull(responseBody);
                    Assert.AreEqual(pxResponse, responseBody, $"responseBody is {responseBody}, which is not expected");
                });

            PXSettings.WalletService.ResetToDefaults();
        }

        [TestMethod]
        public async Task GetWalletConfig_InstancePI_PXWalletConfigEnableBillingAddress_flight()
        {
            string url = $"/v7.0/getWalletConfig";
            string expectedWalletResponse = "{\"merchantName\":\"Microsoft\",\"integrationType\":\"DIRECT\",\"directIntegrationData\":[{\"piFamily\":\"Ewallet\",\"piType\":\"ApplePay\",\"countrySupportedNetworks\":{\"us\":[\"visa\",\"mastercard\",\"discover\",\"amex\"],\"ca\":[\"visa\",\"mastercard\",\"discover\",\"amex\"]},\"providerData\":{\"merchantIdentifier\":\"merchant.com.microsoft.paymicrosoft.sandbox\",\"merchantCapabilities\":[\"supports3DS\",\"supportsEMV\"],\"version\":\"3\"}},{\"piFamily\":\"Ewallet\",\"piType\":\"GooglePay\",\"countrySupportedNetworks\":{\"us\":[\"visa\",\"mastercard\",\"discover\",\"amex\"],\"ca\":[\"visa\",\"mastercard\",\"discover\",\"amex\"]},\"providerData\":{\"allowedMethods\":[\"PAN_ONLY\",\"CRYPTOGRAM_3DS\"],\"assuranceDetailsRequired\":true,\"protocolVersion\":\"ECv2\",\"publicKey\":\"BDRvcFHptepUygYllpEmnHNb4rfG4oBU/sVcAVMn9I7Pu3wYpDpjhBeoF+i30iB2HUQGyZEUsodGdhphaD/j6rU=\",\"publicKeyVersion\":\"1\",\"apiMajorVersion\":2,\"apiMinorVersion\":0,\"merchantId\":\"BCR2DN4TZ244PH2A\"}}]}";
            string expectedPIMSResponse = "[{\"paymentMethodType\":\"applepay\",\"properties\":{\"offlineRecurring\":true,\"userManaged\":false,\"chargeThresholds\":null,\"redirectRequired\":null,\"soldToAddressRequired\":true,\"splitPaymentSupported\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"providerCountry\":null,\"nonStoredPaymentMethodId\":\"be4de87d-7e38-4b2d-8836-9237eb32848e\",\"isNonStoredPaymentMethod\":true},\"paymentMethodGroup\":\"ewallet\",\"groupDisplayName\":\"eWallet\",\"exclusionTags\":null,\"paymentMethodFamily\":\"ewallet\",\"display\":{\"name\":\"ApplePay\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_applepay.svg\",\"logos\":[{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_applepay.svg\"}]},\"AdditionalDisplayText\":null},{\"paymentMethodType\":\"googlepay\",\"properties\":{\"offlineRecurring\":true,\"userManaged\":false,\"chargeThresholds\":null,\"redirectRequired\":null,\"soldToAddressRequired\":true,\"splitPaymentSupported\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"providerCountry\":null,\"nonStoredPaymentMethodId\":\"cdc85313-9b57-4052-81fb-dea336132cbf\",\"isNonStoredPaymentMethod\":true},\"paymentMethodGroup\":\"ewallet\",\"groupDisplayName\":\"eWallet\",\"exclusionTags\":null,\"paymentMethodFamily\":\"ewallet\",\"display\":{\"name\":\"GooglePay\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_googlepay.svg\",\"logos\":[{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_googlepay.svg\"}]},\"AdditionalDisplayText\":null}]";
            string pxResponse = "{\"PIDLConfig\":{\"SelectResource.PaymentInstrument\":{\"actions\":{\"ewallet.applepay.default\":[\"PaymentInstrumentHandler.ClientSupported\"],\"ewallet.googlepay.default\":[\"PaymentInstrumentHandler.ClientSupported\"]}},\"HandlePaymentChallenge\":{\"actions\":{\"ewallet.applepay.default\":[\"PaymentInstrumentHandler.CollectPaymentToken\"],\"ewallet.googlepay.default\":[\"PaymentInstrumentHandler.CollectPaymentToken\"]}}},\"PaymentInstrumentHandlers\":[{\"allowedAuthMethods\":[\"PAN_ONLY\",\"CRYPTOGRAM_3DS\"],\"protocolVersion\":\"ECv2\",\"publicKey\":\"BDRvcFHptepUygYllpEmnHNb4rfG4oBU/sVcAVMn9I7Pu3wYpDpjhBeoF+i30iB2HUQGyZEUsodGdhphaD/j6rU=\",\"merchantName\":\"Microsoft\",\"apiMajorVersion\":\"2\",\"apiMinorVersion\":\"0\",\"assuranceDetailsRequired\":true,\"publicKeyVersion\":\"1\",\"enableGPayIframeForAllBrowsers\":false,\"merchantId\":\"BCR2DN4TZ244PH2A\",\"paymentMethodFamily\":\"ewallet\",\"paymentMethodType\":\"googlepay\",\"piid\":\"cdc85313-9b57-4052-81fb-dea336132cbf\",\"payLabel\":\"amount due plus applicable taxes\",\"integrationType\":\"DIRECT\",\"allowedAuthMethodsPerCountry\":{\"us\":[\"VISA\",\"MASTERCARD\",\"DISCOVER\",\"AMEX\"],\"ca\":[\"VISA\",\"MASTERCARD\",\"DISCOVER\",\"AMEX\"]},\"clientSupported\":{\"supportedBrowsers\":{\"chrome\":\"100\",\"edge\":\"100\"},\"supportedOS\":{\"ios\":\"16.0\",\"android\":\"16.0\",\"windows\":\"16.0\"},\"additionalAPIsCheck\":[\"canMakePayment\"]},\"enableBillingAddress\":true,\"enableEmail\":false,\"disableGeoFencing\":false,\"singleMarkets\":[\"AT\",\"BE\",\"BG\",\"CH\",\"CY\",\"CZ\",\"DE\",\"DK\",\"EE\",\"ES\",\"FI\",\"FR\",\"GB\",\"GR\",\"HR\",\"HU\",\"IE\",\"IS\",\"IT\",\"LI\",\"LT\",\"LU\",\"LV\",\"MT\",\"NL\",\"NO\",\"PL\",\"PT\",\"RO\",\"SE\",\"SI\",\"SK\"]},{\"merchantCapabilities\":[\"supports3DS\",\"supportsEMV\"],\"displayName\":\"Microsoft\",\"initiative\":\"Web\",\"initiativeContext\":\"mystore.example.com\",\"merchantIdentifier\":\"merchant.com.microsoft.paymicrosoft.sandbox\",\"applePayVersion\":\"3\",\"paymentMethodFamily\":\"ewallet\",\"paymentMethodType\":\"applepay\",\"piid\":\"be4de87d-7e38-4b2d-8836-9237eb32848e\",\"payLabel\":\"amount due plus applicable taxes\",\"integrationType\":\"DIRECT\",\"allowedAuthMethodsPerCountry\":{\"us\":[\"VISA\",\"MASTERCARD\",\"DISCOVER\",\"AMEX\"],\"ca\":[\"VISA\",\"MASTERCARD\",\"DISCOVER\",\"AMEX\"]},\"clientSupported\":{\"supportedBrowsers\":{\"safari\":\"16.1\"},\"supportedOS\":{\"ios\":\"15.0\"},\"additionalAPIsCheck\":[\"canMakePaymentWithActiveCard\"],\"paymentProxyRequired\":{\"safari\":\"16.5\"}},\"enableBillingAddress\":true,\"enableEmail\":false,\"disableGeoFencing\":false,\"singleMarkets\":[\"AT\",\"BE\",\"BG\",\"CH\",\"CY\",\"CZ\",\"DE\",\"DK\",\"EE\",\"ES\",\"FI\",\"FR\",\"GB\",\"GR\",\"HR\",\"HU\",\"IE\",\"IS\",\"IT\",\"LI\",\"LT\",\"LU\",\"LV\",\"MT\",\"NL\",\"NO\",\"PL\",\"PT\",\"RO\",\"SE\",\"SI\",\"SK\"]}]}";

            PXSettings.WalletService.ArrangeResponse(expectedWalletResponse);
            PXSettings.PimsService.ArrangeResponse(expectedPIMSResponse);
            PXFlightHandler.AddToEnabledFlights("PXDisableGetWalletConfigCache");

            PXSettings.WalletService.PreProcess = (walletRequest) =>
            {
                Uri requestUri = walletRequest.RequestUri;
                Assert.IsTrue(requestUri.PathAndQuery.Contains("/wallet"));
                Assert.IsNull(walletRequest.Content);
            };

            PXSettings.WalletService.PostProcess = async (walletResponse) =>
            {
                if (walletResponse != null)
                {
                    Assert.AreEqual(walletResponse.StatusCode, HttpStatusCode.OK);
                    Assert.IsNotNull(walletResponse.Content);
                    string responseContent = await walletResponse.Content.ReadAsStringAsync();
                    Assert.AreEqual(responseContent, expectedWalletResponse);
                }
            };

            await GetRequest(
                url,
                null,
                new List<string>() { "GPayApayInstancePI", "PXWalletConfigEnableBillingAddress", "PXDisableGetWalletConfigCache" },
                (responseCode, responseBody, responseHeaders) =>
                {
                    Assert.AreEqual(HttpStatusCode.OK, responseCode);
                    Assert.IsNotNull(responseBody);
                    Assert.AreEqual(pxResponse, responseBody, $"responseBody is {responseBody}, which is not expected");
                });

            PXSettings.WalletService.ResetToDefaults();
        }

        [TestMethod]
        public async Task SetupProviderSession()
        {
            string url = $"v7.0/setupWalletProviderSession";
            string expectedWalletResponse = "{\"sessionData\":{\"sessionData\":null,\"expiresAt\":\"1694559490538\",\"merchantSessionIdentifier\":\"SSH56C2A31FCF8949C9AB4660B91DD3879A_2101F68F6980DFE07DEFE987B1CAF2961766C119C8FDCBB33566B1A97F33C9C3\",\"nonce\":\"92f989a8\",\"merchantIdentifier\":\"64F3879816DBE958F12FF0011022BD1EF2A84E1EC75C6319B8F8D30E648FFFAB\",\"domainName\":\"dotnet-googlepay.azurewebsites.net\",\"displayName\":\"MyStore\",\"signature\":\"308006092a864886f70d010702a0803080020101310d300b0609608648016503040201308006092a864886f70d0107010000a080308203e330820388a00302010202084c304149519d5436300a06082a8648ce3d040302307a312e302c06035504030c254170706c65204170706c69636174696f6e20496e746567726174696f6e204341202d20473331263024060355040b0c1d4170706c652043657274696669636174696f6e20417574686f7269747931133011060355040a0c0a4170706c6520496e632e310b3009060355040613025553301e170d3139303531383031333235375a170d3234303531363031333235375a305f3125302306035504030c1c6563632d736d702d62726f6b65722d7369676e5f5543342d50524f4431143012060355040b0c0b694f532053797374656d7331133011060355040a0c0a4170706c6520496e632e310b30090603550406130255533059301306072a8648ce3d020106082a8648ce3d03010703420004c21577edebd6c7b2218f68dd7090a1218dc7b0bd6f2c283d846095d94af4a5411b83420ed811f3407e83331f1c54c3f7eb3220d6bad5d4eff49289893e7c0f13a38202113082020d300c0603551d130101ff04023000301f0603551d2304183016801423f249c44f93e4ef27e6c4f6286c3fa2bbfd2e4b304506082b0601050507010104393037303506082b060105050730018629687474703a2f2f6f6373702e6170706c652e636f6d2f6f63737030342d6170706c65616963613330323082011d0603551d2004820114308201103082010c06092a864886f7636405013081fe3081c306082b060105050702023081b60c81b352656c69616e6365206f6e207468697320636572746966696361746520627920616e7920706172747920617373756d657320616363657074616e6365206f6620746865207468656e206170706c696361626c65207374616e64617264207465726d7320616e6420636f6e646974696f6e73206f66207573652c20636572746966696361746520706f6c69637920616e642063657274696669636174696f6e2070726163746963652073746174656d656e74732e303606082b06010505070201162a687474703a2f2f7777772e6170706c652e636f6d2f6365727469666963617465617574686f726974792f30340603551d1f042d302b3029a027a0258623687474703a2f2f63726c2e6170706c652e636f6d2f6170706c6561696361332e63726c301d0603551d0e041604149457db6fd57481868989762f7e578507e79b5824300e0603551d0f0101ff040403020780300f06092a864886f76364061d04020500300a06082a8648ce3d0403020349003046022100be09571fe71e1e735b55e5afacb4c72feb445f30185222c7251002b61ebd6f55022100d18b350a5dd6dd6eb1746035b11eb2ce87cfa3e6af6cbd8380890dc82cddaa63308202ee30820275a0030201020208496d2fbf3a98da97300a06082a8648ce3d0403023067311b301906035504030c124170706c6520526f6f74204341202d20473331263024060355040b0c1d4170706c652043657274696669636174696f6e20417574686f7269747931133011060355040a0c0a4170706c6520496e632e310b3009060355040613025553301e170d3134303530363233343633305a170d3239303530363233343633305a307a312e302c06035504030c254170706c65204170706c69636174696f6e20496e746567726174696f6e204341202d20473331263024060355040b0c1d4170706c652043657274696669636174696f6e20417574686f7269747931133011060355040a0c0a4170706c6520496e632e310b30090603550406130255533059301306072a8648ce3d020106082a8648ce3d03010703420004f017118419d76485d51a5e25810776e880a2efde7bae4de08dfc4b93e13356d5665b35ae22d097760d224e7bba08fd7617ce88cb76bb6670bec8e82984ff5445a381f73081f4304606082b06010505070101043a3038303606082b06010505073001862a687474703a2f2f6f6373702e6170706c652e636f6d2f6f63737030342d6170706c65726f6f7463616733301d0603551d0e0416041423f249c44f93e4ef27e6c4f6286c3fa2bbfd2e4b300f0603551d130101ff040530030101ff301f0603551d23041830168014bbb0dea15833889aa48a99debebdebafdacb24ab30370603551d1f0430302e302ca02aa0288626687474703a2f2f63726c2e6170706c652e636f6d2f6170706c65726f6f74636167332e63726c300e0603551d0f0101ff0404030201063010060a2a864886f7636406020e04020500300a06082a8648ce3d040302036700306402303acf7283511699b186fb35c356ca62bff417edd90f754da28ebef19c815e42b789f898f79b599f98d5410d8f9de9c2fe0230322dd54421b0a305776c5df3383b9067fd177c2c216d964fc6726982126f54f87a7d1b99cb9b0989216106990f09921d00003182018830820184020101308186307a312e302c06035504030c254170706c65204170706c69636174696f6e20496e746567726174696f6e204341202d20473331263024060355040b0c1d4170706c652043657274696669636174696f6e20417574686f7269747931133011060355040a0c0a4170706c6520496e632e310b300906035504061302555302084c304149519d5436300b0609608648016503040201a08193301806092a864886f70d010903310b06092a864886f70d010701301c06092a864886f70d010905310f170d3233303931323231353831305a302806092a864886f70d010934311b3019300b0609608648016503040201a10a06082a8648ce3d040302302f06092a864886f70d010904312204202bb7cf44918761eb3abcdbc4088eedef6f6a71326bc2078cb41bc4f4b440220e300a06082a8648ce3d0403020447304502203fe94dc8f3cd543af97baca2096bcb4fb42c0d0023af6778b928ecd1fd808e55022100ec2687178ab06fec6353099abf5c5afad1a0da3b0e452dac8a14d469e1c2e906000000000000\",\"operationalAnalyticsIdentifier\":\"MyStore:64F3879816DBE958F12FF0011022BD1EF2A84E1EC75C6319B8F8D30E648FFFAB\",\"retries\":\"0\",\"pspId\":\"64F3879816DBE958F12FF0011022BD1EF2A84E1EC75C6319B8F8D30E648FFFAB\"}}";
            var payload = new SetupProviderSessionIncomingPayload();
            payload.PiType = "ewallet";
            payload.PiFamily = "applepay";
            payload.WalletSessionData = new WalletSessionData
            {
                MerchantIdentifier = "merchant.com.microsoft.paymicrosoft.sandbox",
                DisplayName = "MyStore",
                Initiative = "web",
                InitiativeContext = "dotnet-googlepay.azurewebsites.net"
            };

            PXSettings.WalletService.ArrangeResponse(expectedWalletResponse);
            PXFlightHandler.AddToEnabledFlights("PXDisableGetWalletConfigCache");

            PXSettings.WalletService.PreProcess = (walletRequest) =>
            {
                Uri requestUri = walletRequest.RequestUri;
                Assert.IsTrue(requestUri.PathAndQuery.Contains("/wallet"));
                Assert.IsTrue(requestUri.PathAndQuery.Contains("/setupprovidersession"));
                Assert.IsNotNull(walletRequest.Content);
            };

            PXSettings.WalletService.PostProcess = async (walletResponse) =>
            {
                if (walletResponse != null)
                {
                    Assert.AreEqual(walletResponse.StatusCode, HttpStatusCode.OK);
                    Assert.IsNotNull(walletResponse.Content);
                    string responseContent = await walletResponse.Content.ReadAsStringAsync();
                    Assert.AreEqual(responseContent, expectedWalletResponse);
                }
            };

            dynamic response = await SendRequestPXService(
                url,
                System.Net.Http.HttpMethod.Post,
                payload);

            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // PX just passes the response from Wallet to PIDL in this case
            Assert.AreEqual(expectedWalletResponse, response.Content.Value);
            PXSettings.WalletService.ResetToDefaults();
        }

        [DataRow("applepay", "cw_apay_123", "{\"billingContact\":{\"addressLines\":[\"1 Microsoft Way\"],\"administrativeArea\":\"WA\",\"country\":\"United States\",\"countryCode\":\"US\",\"familyName\":\"LastName\",\"givenName\":\"FirstName\",\"locality\":\"redmond\",\"postalCode\":\"98052\",\"subAdministrativeArea\":\"\",\"subLocality\":\"\"},\"shippingContact\":{\"addressLines\":[\"1 microsoft way\"],\"administrativeArea\":\"WA\",\"country\":\"United States\",\"countryCode\":\"US\",\"emailAddress\":\"mstest@gmail.com\",\"familyName\":\"LastName\",\"givenName\":\"FirstName\",\"locality\":\"redmond\",\"phoneNumber\":\"1234567890\",\"postalCode\":\"98052-8300\",\"subAdministrativeArea\":\"\",\"subLocality\":\"\"},\"token\":{\"token\":\"<PCE Token which get from /getToken/apay>\",\"paymentMethod\":{\"displayName\":\"Visa 1234\",\"network\":\"Visa\",\"type\":\"credit\"},\"transactionIdentifier\":\"8db6cbfe1b35798839f8c14075393f4533b62fa0783c70022d5c32dea4eab141\"}}")]
        [DataRow("googlepay", "cw_gpay_123", "{\"apiVersionMinor\":0,\"apiVersion\":2,\"paymentMethodData\":{\"description\":\"Visa •••• 1234\",\"token\":\"<PCE Token which get from /getToken/apay>\",\"type\":\"CARD\",\"info\":{\"cardNetwork\":\"VISA\",\"cardDetails\":\"1234\",\"billingAddress\":{\"phoneNumber\":\"+1 111-111-1111\",\"address3\":\"\",\"sortingCode\":\"\",\"address2\":\"\",\"countryCode\":\"US\",\"address1\":\"1 Microsoft Way\",\"postalCode\":\"98052\",\"name\":\"First Last\",\"locality\":\"redmond\",\"administrativeArea\":\"WA\"}}},\"shippingOptionData\":{\"id\":\"shipping-001\"},\"shippingAddress\":{\"address3\":\"\",\"sortingCode\":\"\",\"address2\":\"\",\"countryCode\":\"US\",\"address1\":\"1 Microsoft Way\",\"postalCode\":\"98052\",\"name\":\"First Last\",\"locality\":\"redmond\",\"administrativeArea\":\"WA\"},\"email\":\"mstest@gmail.com\"}")]
        [DataRow("applepay", "123", "{\"billingContact\":{\"addressLines\":[\"1 Microsoft Way\"],\"administrativeArea\":\"WA\",\"country\":\"United States\",\"countryCode\":\"US\",\"familyName\":\"LastName\",\"givenName\":\"FirstName\",\"locality\":\"redmond\",\"postalCode\":\"98052\",\"subAdministrativeArea\":\"\",\"subLocality\":\"\"},\"shippingContact\":{\"addressLines\":[\"1 microsoft way\"],\"administrativeArea\":\"WA\",\"country\":\"United States\",\"countryCode\":\"US\",\"emailAddress\":\"mstest@gmail.com\",\"familyName\":\"LastName\",\"givenName\":\"FirstName\",\"locality\":\"redmond\",\"phoneNumber\":\"1234567890\",\"postalCode\":\"98052-8300\",\"subAdministrativeArea\":\"\",\"subLocality\":\"\"},\"token\":{\"token\":\"<PCE Token which get from /getToken/apay>\",\"paymentMethod\":{\"displayName\":\"Visa 1234\",\"network\":\"Visa\",\"type\":\"credit\"},\"transactionIdentifier\":\"8db6cbfe1b35798839f8c14075393f4533b62fa0783c70022d5c32dea4eab141\"}}")]
        [TestMethod]
        public async Task ProvisionWalletToken(string type, string piid, string paymentData)
        {
            string url = $"v7.0/1234/provisionWalletToken";
            string transactionDataResponse = "\"Z10005CDKAY52ffd47e0-e1ab-45c6-b693-cd019f02fc3c\"";
            string expectedWalletResponse = "{\"eci\":\"05\",\"hasCryptogram\":\"true\",\"walletMetadata\":{\"cardType\":\"Visa\",\"lastFourDigits\":\"1234\",\"expirationMonth\":\"12\",\"expirationYear\":\"2025\"}}";
            var payload = new ProvisionWalletTokenIncomingPayload();
            payload.PiType = type;
            payload.PiFamily = "ewallet";
            payload.TokenReference = "Z10005CDKAY52ffd47e0-e1ab-45c6-b693-cd019f02fc3c";
            payload.SessionData = new PaymentSessionData
            {
                Country = "us",
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                PaymentInstrumentId = piid,
                Language = "en-us",
                Partner = "cart",
                Amount = 10,
                Currency = "USD",
                AuthorizationGroups = new List<AuthorizationGroup> { new AuthorizationGroup() { Id = "12345", TotalAmount = 10, ItemTitles = new List<string>() { "mouse" } } }
            };

            payload.PaymentData = paymentData;

            PXSettings.TransactionDataService.ArrangeResponse(transactionDataResponse);
            PXSettings.WalletService.ArrangeResponse(expectedWalletResponse);
            PXFlightHandler.AddToEnabledFlights("PXDisableGetWalletConfigCache");

            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account013", "cw_gpay_cc80ac8e-3e33-40f3-9fed-6efb5be47762");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI), HttpStatusCode.OK, null, ".*/paymentInstruments.*");

            bool updatePICalled = false;
            Microsoft.Commerce.Payments.PidlFactory.V7.PIDLData capturedPI = null;
            PXSettings.PimsService.PreProcess = (pimsRequest) =>
            {
                Uri requestUri = pimsRequest.RequestUri;
                updatePICalled = requestUri.AbsoluteUri.Contains("/update");
                updatePICalled = requestUri.AbsoluteUri.Contains("country=us");
                updatePICalled = requestUri.AbsoluteUri.Contains("partner=cart");

                // Capture the payload to validate address fields
                if (updatePICalled && pimsRequest.Content != null)
                {
                    var content = pimsRequest.Content.ReadAsStringAsync().Result;
                    capturedPI = JsonConvert.DeserializeObject<Microsoft.Commerce.Payments.PidlFactory.V7.PIDLData>(content);
                }
            };

            bool provisionCalled = false;
            PXSettings.WalletService.PreProcess = (walletRequest) =>
            {
                provisionCalled = true;
                Uri requestUri = walletRequest.RequestUri;
                Assert.IsTrue(requestUri.PathAndQuery.Contains("/wallet"));
                Assert.IsTrue(requestUri.PathAndQuery.Contains("/provision"));
                Assert.IsNotNull(walletRequest.Content);
            };

            PXSettings.WalletService.PostProcess = async (walletResponse) =>
            {
                if (walletResponse != null)
                {
                    Assert.AreEqual(walletResponse.StatusCode, HttpStatusCode.OK);
                    Assert.IsNotNull(walletResponse.Content);
                    string responseContent = await walletResponse.Content.ReadAsStringAsync();
                    Assert.AreEqual(responseContent, expectedWalletResponse);
                }
            };

            dynamic response = await SendRequestPXService(
                url,
                System.Net.Http.HttpMethod.Post,
                payload);

            Assert.IsNotNull(response);
            if (!piid.Contains("cw_gpay") && !piid.Contains("cw_apay"))
            {
                Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model.PaymentSession session = response.Content.Value;
                Assert.AreEqual("Z10005CDKAY52ffd47e0-e1ab-45c6-b693-cd019f02fc3c", session.Id);
                Assert.IsFalse(session.IsChallengeRequired);
                Assert.IsTrue(!string.IsNullOrWhiteSpace(session.Signature));
                Assert.AreEqual(PaymentChallengeStatus.NotApplicable, session.ChallengeStatus);
                Assert.AreEqual(piid, session.PaymentInstrumentId);
                Assert.AreEqual("en-us", session.Language);
                Assert.AreEqual("cart", session.Partner);
                Assert.AreEqual(10, session.Amount);
                Assert.AreEqual("USD", session.Currency);
                Assert.AreEqual(ChallengeScenario.PaymentTransaction, session.ChallengeScenario);
                Assert.AreEqual("us", session.Country);
            }
            else
            {
                Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model.PaymentSessionData sessionData = response.Content.Value;
                string responseContent = await response.Content.ReadAsStringAsync();
                Assert.IsFalse(responseContent.Contains("Id:"));
                Assert.AreEqual(piid, sessionData.PaymentInstrumentId);
                Assert.AreEqual("en-us", sessionData.Language);
                Assert.AreEqual("cart", sessionData.Partner);
                Assert.AreEqual(10, sessionData.Amount);
                Assert.AreEqual("USD", sessionData.Currency);
                Assert.AreEqual(ChallengeScenario.PaymentTransaction, sessionData.ChallengeScenario);
                Assert.AreEqual("us", sessionData.Country);

                // Validate that when instance PI flow is used, the PI payload includes all address fields
                if (capturedPI != null && updatePICalled)
                {
                    // Deserialize the "details" JSON string into a Dictionary<string, object>
                    var detailsJson = capturedPI["details"]?.ToString();
                    var details = !string.IsNullOrEmpty(detailsJson)
                        ? JsonConvert.DeserializeObject<Dictionary<string, object>>(detailsJson)
                        : null;

                    Assert.IsNotNull(details, "PI details should not be null");

                    var addressJson = details["address"]?.ToString();
                    var address = !string.IsNullOrEmpty(addressJson)
                        ? JsonConvert.DeserializeObject<Dictionary<string, object>>(addressJson)
                        : null;

                    Assert.IsNotNull(address, "Address in PI details should not be null");

                    // Verify all address fields are present
                    Assert.IsTrue(address.ContainsKey("address_line1"), "Address should contain AddressLine1");
                    Assert.IsTrue(address.ContainsKey("address_line2"), "Address should contain AddressLine2");
                    Assert.IsTrue(address.ContainsKey("address_line3"), "Address should contain AddressLine3");
                    Assert.IsTrue(address.ContainsKey("city"), "Address should contain City");
                    Assert.IsTrue(address.ContainsKey("region"), "Address should contain Region");
                    Assert.IsTrue(address.ContainsKey("postal_code"), "Address should contain PostalCode");
                    Assert.IsTrue(address.ContainsKey("country"), "Address should contain Country");

                    // Verify the values match what was in the test data
                    Assert.AreEqual("1 Microsoft Way", address["address_line1"]);
                    Assert.AreEqual("redmond", address["city"]);
                    Assert.AreEqual("WA", address["region"]);
                    Assert.AreEqual("98052", address["postal_code"]);
                    Assert.AreEqual("US", address["country"]);
                }
            }

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsTrue(updatePICalled == (piid.Contains("cw_gpay") || piid.Contains("cw_apay")));
            Assert.IsTrue(provisionCalled == (!piid.Contains("cw_gpay") && !piid.Contains("cw_apay")));
            PXSettings.WalletService.ResetToDefaults();
        }

        [TestMethod]
        public async Task ProvisionWalletTokenWithoutAuthorizationGroup()
        {
            string url = $"v7.0/1234/provisionWalletToken";
            string transactionDataResponse = "\"Z10005CDKAY52ffd47e0-e1ab-45c6-b693-cd019f02fc3c\"";
            string expectedWalletResponse = "{\"eci\":\"05\",\"hasCryptogram\":\"true\",\"walletMetadata\":{\"cardType\":\"Visa\",\"lastFourDigits\":\"1234\",\"expirationMonth\":\"12\",\"expirationYear\":\"2025\"}}";
            var payload = new ProvisionWalletTokenIncomingPayload();
            payload.PiType = "ewallet";
            payload.PiFamily = "googlepay";
            payload.TokenReference = "Z10005CDKAY52ffd47e0-e1ab-45c6-b693-cd019f02fc3c";
            payload.SessionData = new PaymentSessionData
            {
                Country = "us",
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                PaymentInstrumentId = "123456123456",
                Language = "en-us",
                Partner = "cart",
                Amount = 10,
                Currency = "USD",
            };

            PXSettings.TransactionDataService.ArrangeResponse(transactionDataResponse);
            PXSettings.WalletService.ArrangeResponse(expectedWalletResponse);
            PXFlightHandler.AddToEnabledFlights("PXDisableGetWalletConfigCache");

            PXSettings.WalletService.PreProcess = (walletRequest) =>
            {
                Uri requestUri = walletRequest.RequestUri;
                Assert.IsTrue(requestUri.PathAndQuery.Contains("/wallet"));
                Assert.IsTrue(requestUri.PathAndQuery.Contains("/provision"));
                Assert.IsNotNull(walletRequest.Content);
            };

            PXSettings.WalletService.PostProcess = async (walletResponse) =>
            {
                if (walletResponse != null)
                {
                    Assert.AreEqual(walletResponse.StatusCode, HttpStatusCode.OK);
                    Assert.IsNotNull(walletResponse.Content);
                    string responseContent = await walletResponse.Content.ReadAsStringAsync();
                    Assert.AreEqual(responseContent, expectedWalletResponse);
                }
            };

            dynamic response = await SendRequestPXService(
                url,
                System.Net.Http.HttpMethod.Post,
                payload);
            Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model.PaymentSession session = response.Content.Value;
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("Z10005CDKAY52ffd47e0-e1ab-45c6-b693-cd019f02fc3c", session.Id);
            Assert.IsFalse(session.IsChallengeRequired);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(session.Signature));
            Assert.AreEqual(PaymentChallengeStatus.NotApplicable, session.ChallengeStatus);
            Assert.AreEqual("123456123456", session.PaymentInstrumentId);
            Assert.AreEqual("en-us", session.Language);
            Assert.AreEqual("cart", session.Partner);
            Assert.AreEqual(10, session.Amount);
            Assert.AreEqual("USD", session.Currency);
            Assert.AreEqual(ChallengeScenario.PaymentTransaction, session.ChallengeScenario);
            Assert.AreEqual("us", session.Country);
            PXSettings.WalletService.ResetToDefaults();
        }

        [TestMethod]
        public async Task ValidateDataWithApproveResponse()
        {
            string url = $"v7.0/1234/provisionWalletToken";
            string transactionDataResponse = "\"Z10005CDKAY52ffd47e0-e1ab-45c6-b693-cd019f02fc3c\"";
            string expectedValidateResponse = "{\"result\":\"Approved\"}";
            var payload = new ValidateIncomingPayload
            {
                PiType = "ewallet",
                PiFamily = "applepay",
                TokenReference = "Z10093CKM41Ifb7e9ca3-429b-44f7-af67-3eb8ba797e19",
                SessionData = new PaymentSessionData
                {
                    Country = "us",
                    ChallengeScenario = ChallengeScenario.PaymentTransaction,
                    PaymentInstrumentId = "123456123456",
                    Language = "en-us",
                    Partner = "cart",
                    Amount = 0,
                    Currency = "USD"
                },
            };

            PXSettings.TransactionDataService.ArrangeResponse(transactionDataResponse);
            PXSettings.WalletService.ArrangeResponse(expectedValidateResponse, HttpStatusCode.OK, HttpMethod.Post, "api/wallet/validate");
            PXFlightHandler.AddToEnabledFlights("PXDisableGetWalletConfigCache");

            PXSettings.WalletService.PreProcess = (walletRequest) =>
            {
                Uri requestUri = walletRequest.RequestUri;
                Assert.IsTrue(requestUri.PathAndQuery.Contains("/wallet"));
            };

            PXSettings.WalletService.PostProcess = async (walletResponse) =>
            {
                if (walletResponse != null)
                {
                    Assert.AreEqual(walletResponse.StatusCode, HttpStatusCode.OK);
                    Assert.IsNotNull(walletResponse.Content);
                    string responseContent = await walletResponse.Content.ReadAsStringAsync();
                }
            };

            // pass the flight to send request
            dynamic response = await SendRequestPXService(
                url,
                System.Net.Http.HttpMethod.Post,
                payload);
            Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model.PaymentSession session = response.Content.Value;
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("Z10005CDKAY52ffd47e0-e1ab-45c6-b693-cd019f02fc3c", session.Id);
            Assert.IsFalse(session.IsChallengeRequired);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(session.Signature));
            Assert.AreEqual(PaymentChallengeStatus.NotApplicable, session.ChallengeStatus);
            Assert.AreEqual("123456123456", session.PaymentInstrumentId);
            Assert.AreEqual("en-us", session.Language);
            Assert.AreEqual("cart", session.Partner);
            Assert.AreEqual(0, session.Amount);
            Assert.AreEqual("USD", session.Currency);
            Assert.AreEqual(ChallengeScenario.PaymentTransaction, session.ChallengeScenario);
            Assert.AreEqual("us", session.Country);
            PXSettings.WalletService.ResetToDefaults();
        }

        [TestMethod]
        public async Task ValidateDataWithRejectedResponse()
        {
            string url = $"v7.0/1234/provisionWalletToken";
            string transactionDataResponse = "\"Z10005CDKAY52ffd47e0-e1ab-45c6-b693-cd019f02fc3c\"";
            string expectedValidateResponse = "{\"result\":\"Rejected\"}";
            var payload = new ValidateIncomingPayload
            {
                PiType = "ewallet",
                PiFamily = "applepay",
                TokenReference = "Z10093CKM41Ifb7e9ca3-429b-44f7-af67-3eb8ba797e19",
                SessionData = new PaymentSessionData
                {
                    Country = "us",
                    ChallengeScenario = ChallengeScenario.PaymentTransaction,
                    PaymentInstrumentId = "123456123456",
                    Language = "en-us",
                    Partner = "cart",
                    Amount = 0,
                    Currency = "USD"
                },
            };

            PXSettings.TransactionDataService.ArrangeResponse(transactionDataResponse);
            PXSettings.WalletService.ArrangeResponse(expectedValidateResponse, HttpStatusCode.OK, HttpMethod.Post, "api/wallet/validate");
            PXFlightHandler.AddToEnabledFlights("PXDisableGetWalletConfigCache");

            PXSettings.WalletService.PreProcess = (walletRequest) =>
            {
                Uri requestUri = walletRequest.RequestUri;
                Assert.IsTrue(requestUri.PathAndQuery.Contains("/wallet"));
            };

            PXSettings.WalletService.PostProcess = async (walletResponse) =>
            {
                if (walletResponse != null)
                {
                    Assert.AreEqual(walletResponse.StatusCode, HttpStatusCode.OK);
                    Assert.IsNotNull(walletResponse.Content);
                    string responseContent = await walletResponse.Content.ReadAsStringAsync();
                }
            };

            // pass the flight to send request
            dynamic response = await SendRequestPXServiceWithFlightOverrides(
                url,
                System.Net.Http.HttpMethod.Post,
                payload,
                Flighting.Features.PXEnableValidateAPIForGPAP);
            Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model.PaymentSession session = response.Content.Value;
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("Z10005CDKAY52ffd47e0-e1ab-45c6-b693-cd019f02fc3c", session.Id);
            Assert.IsFalse(session.IsChallengeRequired);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(session.Signature));
            Assert.AreEqual(PaymentChallengeStatus.Failed, session.ChallengeStatus);
            Assert.AreEqual("123456123456", session.PaymentInstrumentId);
            Assert.AreEqual("en-us", session.Language);
            Assert.AreEqual("cart", session.Partner);
            Assert.AreEqual(0, session.Amount);
            Assert.AreEqual("USD", session.Currency);
            Assert.AreEqual(ChallengeScenario.PaymentTransaction, session.ChallengeScenario);
            Assert.AreEqual("us", session.Country);
            PXSettings.WalletService.ResetToDefaults();
        }

        [TestMethod]
        public async Task ValidateDataWithPositiveAmount()
        {
            string url = $"v7.0/1234/provisionWalletToken";
            string transactionDataResponse = "\"Z10005CDKAY52ffd47e0-e1ab-45c6-b693-cd019f02fc3c\"";
            string expectedValidateResponse = "{\"result\":\"Rejected\"}";
            var payload = new ValidateIncomingPayload
            {
                PiType = "ewallet",
                PiFamily = "applepay",
                TokenReference = "Z10093CKM41Ifb7e9ca3-429b-44f7-af67-3eb8ba797e19",
                SessionData = new PaymentSessionData
                {
                    Country = "us",
                    ChallengeScenario = ChallengeScenario.PaymentTransaction,
                    PaymentInstrumentId = "123456123456",
                    Language = "en-us",
                    Partner = "cart",
                    Amount = 10,
                    Currency = "USD"
                },
            };

            PXSettings.TransactionDataService.ArrangeResponse(transactionDataResponse);
            PXSettings.WalletService.ArrangeResponse(expectedValidateResponse, HttpStatusCode.OK, HttpMethod.Post, "api/wallet/validate");
            PXFlightHandler.AddToEnabledFlights("PXDisableGetWalletConfigCache");

            PXSettings.WalletService.PreProcess = (walletRequest) =>
            {
                Uri requestUri = walletRequest.RequestUri;
                Assert.IsTrue(requestUri.PathAndQuery.Contains("/wallet"));
            };

            PXSettings.WalletService.PostProcess = async (walletResponse) =>
            {
                if (walletResponse != null)
                {
                    Assert.AreEqual(walletResponse.StatusCode, HttpStatusCode.OK);
                    Assert.IsNotNull(walletResponse.Content);
                    string responseContent = await walletResponse.Content.ReadAsStringAsync();
                }
            };

            // pass the flight to send request
            dynamic response = await SendRequestPXServiceWithFlightOverrides(
                url,
                System.Net.Http.HttpMethod.Post,
                payload,
                Flighting.Features.PXEnableValidateAPIForGPAP);
            Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model.PaymentSession session = response.Content.Value;
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("Z10005CDKAY52ffd47e0-e1ab-45c6-b693-cd019f02fc3c", session.Id);
            Assert.IsFalse(session.IsChallengeRequired);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(session.Signature));

            // Even the mock validate API response is Rejected, which should cause the PaymentChallengeStatus to be Failed in the logic
            // The status is still NotApplicable since amount is not $0
            Assert.AreEqual(PaymentChallengeStatus.NotApplicable, session.ChallengeStatus);
            Assert.AreEqual("123456123456", session.PaymentInstrumentId);
            Assert.AreEqual("en-us", session.Language);
            Assert.AreEqual("cart", session.Partner);
            Assert.AreEqual(10, session.Amount);
            Assert.AreEqual("USD", session.Currency);
            Assert.AreEqual(ChallengeScenario.PaymentTransaction, session.ChallengeScenario);
            Assert.AreEqual("us", session.Country);
            PXSettings.WalletService.ResetToDefaults();
        }

        [DataRow("PXWalletConfigAddDeviceSupportStatus,PXWalletConfigDisableGooglePay,PXDisableGetWalletConfigCache", "Mozilla/5.0 (iPhone; CPU iPhone OS 17_1_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.1.1 Mobile/15E148 Safari/604.1", false, false, true)]
        [DataRow("PXWalletConfigAddDeviceSupportStatus,PXWalletConfigDisableApplePay,PXDisableGetWalletConfigCache", "Mozilla/5.0 (iPhone; CPU iPhone OS 17_1_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.1.1 Mobile/15E148 Safari/604.1", false, true, false)]
        [DataRow("PXWalletConfigAddDeviceSupportStatus,PXDisableGetWalletConfigCache", "Mozilla/5.0 (iPhone; CPU iPhone OS 17_1_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.1.1 Mobile/15E148 Safari/604.1", false, true, true)]
        [DataRow("PXWalletConfigAddDeviceSupportStatus,PXDisableGetWalletConfigCache", "Mozilla/5.0 (iPhone; CPU iPhone OS 17_1_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.1.1 Mobile/15E148 Safari/604.1", true, true, true)]
        [DataRow("PXWalletConfigAddDeviceSupportStatus,PXDisableGetWalletConfigCache", "Mozilla/5.0 (iPhone; CPU iPhone OS 16_1_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.1.1 Mobile/15E148 Safari/604.1", true, true, false)]
        [DataRow("PXWalletConfigAddDeviceSupportStatus,PXWalletConfigDisableApplePay,PXWalletConfigDisableGooglePay,PXDisableGetWalletConfigCache", "Mozilla/5.0 (iPhone; CPU iPhone OS 16_1_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.1.1 Mobile/15E148 Safari/604.1", true, false, false)]
        [DataTestMethod]
        public async Task ValidateWalletConfigExcludDevice(
            string flights,
            string userAgent,
            bool crossOrigin,
            bool expectedGpayDeviceStatus,
            bool expectApayDeviceStatus)
        {
            List<string> flightList = flights.Split(',').ToList<string>();
            string url = $"/v7.0/getWalletConfig?partner=testPartner&client=%7B%22isCrossOrigin%22:{crossOrigin.ToString().ToLower()}%7D";
            string expectedWalletResponse = "{\"merchantName\":\"Microsoft\",\"integrationType\":\"DIRECT\",\"directIntegrationData\":[{\"piFamily\":\"Ewallet\",\"piType\":\"ApplePay\",\"countrySupportedNetworks\":{\"us\":[\"visa\",\"mastercard\",\"discover\",\"amex\"],\"ca\":[\"visa\",\"mastercard\",\"discover\",\"amex\"]},\"providerData\":{\"merchantIdentifier\":\"merchant.com.microsoft.paymicrosoft.sandbox\",\"merchantCapabilities\":[\"supports3DS\",\"supportsEMV\"],\"version\":\"3\"}},{\"piFamily\":\"Ewallet\",\"piType\":\"GooglePay\",\"countrySupportedNetworks\":{\"us\":[\"visa\",\"mastercard\",\"discover\",\"amex\"],\"ca\":[\"visa\",\"mastercard\",\"discover\",\"amex\"]},\"providerData\":{\"allowedMethods\":[\"PAN_ONLY\",\"CRYPTOGRAM_3DS\"],\"assuranceDetailsRequired\":true,\"protocolVersion\":\"ECv2\",\"publicKey\":\"BDRvcFHptepUygYllpEmnHNb4rfG4oBU/sVcAVMn9I7Pu3wYpDpjhBeoF+i30iB2HUQGyZEUsodGdhphaD/j6rU=\",\"publicKeyVersion\":\"1\",\"apiMajorVersion\":2,\"apiMinorVersion\":0,\"merchantId\":\"BCR2DN4TZ244PH2A\"}}]}";
            string expectedPIMSResponse = "[{\"paymentMethodType\":\"applepay\",\"properties\":{\"offlineRecurring\":true,\"userManaged\":false,\"chargeThresholds\":null,\"redirectRequired\":null,\"soldToAddressRequired\":true,\"splitPaymentSupported\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"providerCountry\":null,\"nonStoredPaymentMethodId\":\"be4de87d-7e38-4b2d-8836-9237eb32848e\",\"isNonStoredPaymentMethod\":true},\"paymentMethodGroup\":\"ewallet\",\"groupDisplayName\":\"eWallet\",\"exclusionTags\":null,\"paymentMethodFamily\":\"ewallet\",\"display\":{\"name\":\"ApplePay\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_applepay.svg\",\"logos\":[{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_applepay.svg\"}]},\"AdditionalDisplayText\":null},{\"paymentMethodType\":\"googlepay\",\"properties\":{\"offlineRecurring\":true,\"userManaged\":false,\"chargeThresholds\":null,\"redirectRequired\":null,\"soldToAddressRequired\":true,\"splitPaymentSupported\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"providerCountry\":null,\"nonStoredPaymentMethodId\":\"cdc85313-9b57-4052-81fb-dea336132cbf\",\"isNonStoredPaymentMethod\":true},\"paymentMethodGroup\":\"ewallet\",\"groupDisplayName\":\"eWallet\",\"exclusionTags\":null,\"paymentMethodFamily\":\"ewallet\",\"display\":{\"name\":\"GooglePay\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_googlepay.svg\",\"logos\":[{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_googlepay.svg\"}]},\"AdditionalDisplayText\":null}]";

            PXSettings.WalletService.ArrangeResponse(expectedWalletResponse);
            PXSettings.PimsService.ArrangeResponse(expectedPIMSResponse);

            var headers = new Dictionary<string, string>();
            headers.Add("x-ms-clientcontext-encoding", "base64");
            headers.Add("x-ms-deviceinfo", GetDeviceInfo(userAgent));

            await GetRequest(
                url,
                headers,
                flightList,
                (responseCode, responseBody, responseHeaders) =>
                {
                    Assert.AreEqual(HttpStatusCode.OK, responseCode, $"Unexpected response code {responseCode}");
                    Assert.IsNotNull(responseBody);

                    WalletConfig walletConfig = JsonConvert.DeserializeObject<WalletConfig>(responseBody);
                    Assert.IsNotNull(walletConfig, "wallet config is null");
                    Assert.IsNotNull(walletConfig.PaymentInstrumentHandlers, "payment instrument handlers is null");

                    var gpayHandler = walletConfig.PaymentInstrumentHandlers[0];
                    var aPayHandler = walletConfig.PaymentInstrumentHandlers[1];

                    Assert.IsNotNull(gpayHandler, "gpayhandler is null");
                    Assert.IsNotNull(aPayHandler, "apayhandler is null");
                    Assert.IsNotNull(gpayHandler.DisableGeoFencing);
                    Assert.IsFalse(gpayHandler.DisableGeoFencing, "DisableGeoFencing should be false for express checkout");
                    Assert.IsNotNull(gpayHandler.SingleMarkets);
                    Assert.IsTrue(gpayHandler.SingleMarkets.Count > 0, "SingleMarkets should not be empty for express checkout");
                    Assert.IsNotNull(aPayHandler.DisableGeoFencing);
                    Assert.IsFalse(aPayHandler.DisableGeoFencing, "DisableGeoFencing should be false for express checkout");
                    Assert.IsNotNull(aPayHandler.SingleMarkets);
                    Assert.IsTrue(aPayHandler.SingleMarkets.Count > 0, "SingleMarkets should not be empty for express checkout");

                    Assert.AreEqual(expectedGpayDeviceStatus, gpayHandler.DeviceSupportedStatus.Result, $"gpay devicesupported status should be {expectedGpayDeviceStatus}");
                    Assert.AreEqual(expectApayDeviceStatus, aPayHandler.DeviceSupportedStatus.Result, $"apay devicesupported status should be {expectedGpayDeviceStatus}");
                });
        }

        [DataRow("ios", "safari,chrome,edge", "PXWalletConfigAddIframeFallbackSupported,PXWalletEnableGooglePayIframeFallback,PXDisableGetWalletConfigCache", true, true)]
        [DataRow("mac os x", "safari,chrome,edge", "PXWalletConfigAddIframeFallbackSupported,PXWalletEnableGooglePayIframeFallback,PXDisableGetWalletConfigCache", false, true)]
        [DataRow("android", "*", "PXWalletConfigAddIframeFallbackSupported,PXDisableGetWalletConfigCache", true, false)]
        [DataRow("windows", "*", "PXWalletConfigAddIframeFallbackSupported,PXDisableGetWalletConfigCache", false, false)]
        [DataTestMethod]
        public async Task ValidateGPayIframeFallback(
            string os,
            string browsers,
            string flights,
            bool crossOrigin,
            bool expectedIframeFallback)
        {
            List<string> flightList = flights.Split(',').ToList<string>();
            string url = $"/v7.0/getWalletConfig?partner=testPartner&client=%7B%22isCrossOrigin%22:{crossOrigin.ToString().ToLower()}%7D";
            string expectedWalletResponse = "{\"merchantName\":\"Microsoft\",\"integrationType\":\"DIRECT\",\"directIntegrationData\":[{\"piFamily\":\"Ewallet\",\"piType\":\"ApplePay\",\"countrySupportedNetworks\":{\"us\":[\"visa\",\"mastercard\",\"discover\",\"amex\"],\"ca\":[\"visa\",\"mastercard\",\"discover\",\"amex\"]},\"providerData\":{\"merchantIdentifier\":\"merchant.com.microsoft.paymicrosoft.sandbox\",\"merchantCapabilities\":[\"supports3DS\",\"supportsEMV\"],\"version\":\"3\"}},{\"piFamily\":\"Ewallet\",\"piType\":\"GooglePay\",\"countrySupportedNetworks\":{\"us\":[\"visa\",\"mastercard\",\"discover\",\"amex\"],\"ca\":[\"visa\",\"mastercard\",\"discover\",\"amex\"]},\"providerData\":{\"allowedMethods\":[\"PAN_ONLY\",\"CRYPTOGRAM_3DS\"],\"assuranceDetailsRequired\":true,\"protocolVersion\":\"ECv2\",\"publicKey\":\"BDRvcFHptepUygYllpEmnHNb4rfG4oBU/sVcAVMn9I7Pu3wYpDpjhBeoF+i30iB2HUQGyZEUsodGdhphaD/j6rU=\",\"publicKeyVersion\":\"1\",\"apiMajorVersion\":2,\"apiMinorVersion\":0,\"merchantId\":\"BCR2DN4TZ244PH2A\"}}]}";
            string expectedPIMSResponse = "[{\"paymentMethodType\":\"applepay\",\"properties\":{\"offlineRecurring\":true,\"userManaged\":false,\"chargeThresholds\":null,\"redirectRequired\":null,\"soldToAddressRequired\":true,\"splitPaymentSupported\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"providerCountry\":null,\"nonStoredPaymentMethodId\":\"be4de87d-7e38-4b2d-8836-9237eb32848e\",\"isNonStoredPaymentMethod\":true},\"paymentMethodGroup\":\"ewallet\",\"groupDisplayName\":\"eWallet\",\"exclusionTags\":null,\"paymentMethodFamily\":\"ewallet\",\"display\":{\"name\":\"ApplePay\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_applepay.svg\",\"logos\":[{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_applepay.svg\"}]},\"AdditionalDisplayText\":null},{\"paymentMethodType\":\"googlepay\",\"properties\":{\"offlineRecurring\":true,\"userManaged\":false,\"chargeThresholds\":null,\"redirectRequired\":null,\"soldToAddressRequired\":true,\"splitPaymentSupported\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"providerCountry\":null,\"nonStoredPaymentMethodId\":\"cdc85313-9b57-4052-81fb-dea336132cbf\",\"isNonStoredPaymentMethod\":true},\"paymentMethodGroup\":\"ewallet\",\"groupDisplayName\":\"eWallet\",\"exclusionTags\":null,\"paymentMethodFamily\":\"ewallet\",\"display\":{\"name\":\"GooglePay\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_googlepay.svg\",\"logos\":[{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_googlepay.svg\"}]},\"AdditionalDisplayText\":null}]";

            PXSettings.WalletService.ArrangeResponse(expectedWalletResponse);
            PXSettings.PimsService.ArrangeResponse(expectedPIMSResponse);

            await GetRequest(
                url,
                null,
                flightList,
                (responseCode, responseBody, responseHeaders) =>
                {
                    Assert.AreEqual(HttpStatusCode.OK, responseCode, $"Unexpected response code {responseCode}");
                    Assert.IsNotNull(responseBody);

                    WalletConfig walletConfig = JsonConvert.DeserializeObject<WalletConfig>(responseBody);
                    Assert.IsNotNull(walletConfig, "wallet config is null");
                    Assert.IsNotNull(walletConfig.PaymentInstrumentHandlers, "payment instrument handlers is null");

                    var gpayHandler = walletConfig.PaymentInstrumentHandlers[0];
                    var aPayHandler = walletConfig.PaymentInstrumentHandlers[1];

                    Assert.IsNotNull(gpayHandler, "gpayhandler is null");
                    Assert.IsNotNull(aPayHandler, "apayhandler is null");
                    Assert.IsNotNull(gpayHandler.DisableGeoFencing);
                    Assert.IsFalse(gpayHandler.DisableGeoFencing, "DisableGeoFencing should be false for express checkout");
                    Assert.IsNotNull(gpayHandler.SingleMarkets);
                    Assert.IsTrue(gpayHandler.SingleMarkets.Count > 0, "SingleMarkets should not be empty for express checkout");
                    Assert.IsNotNull(aPayHandler.DisableGeoFencing);
                    Assert.IsFalse(aPayHandler.DisableGeoFencing, "DisableGeoFencing should be false for express checkout");
                    Assert.IsNotNull(aPayHandler.SingleMarkets);
                    Assert.IsTrue(aPayHandler.SingleMarkets.Count > 0, "SingleMarkets should not be empty for express checkout");

                    Assert.AreEqual(expectedIframeFallback, gpayHandler.IframeFallbackSupported, $"gpay iframe fallback should be {expectedIframeFallback}");
                    Assert.IsNull(aPayHandler.IframeFallbackSupported);
                });
        }

        private static string GetDeviceInfo(string userAgent)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(userAgent);
            string encodedText = Convert.ToBase64String(plainTextBytes);

            // actual user Agent: "ipAddress=MTc2LjQ1LjkyLjE=,userAgent=TW96aWxsYS81LjAgKFdpbmRvd3MgTlQgMTAuMDsgV2luNjQ7IHg2NDsgcnY6MTIyLjApIEdlY2tvLzIwMTAwMTAxIEZpcmVmb3gvMTIyLjA=,deviceId=MASKED"
            return "ipAddress=MTc2LjQ1LjkyLjE=,deviceId=MASKED,userAgent=" + encodedText;
        }
    }
}