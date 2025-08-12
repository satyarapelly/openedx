// <copyright file="WalletsHandler.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.Model.PXInternal;
    using Microsoft.Commerce.Payments.PXService.Model.WalletService;
    using Newtonsoft.Json;
    using static Microsoft.Commerce.Payments.PXService.V7.Constants;

    public class WalletsHandler
    {
        public static WalletConfig AdaptWalletResponseToPIDLConfig(
            ProviderDataResponse response,
            string client,
            string partner,
            HttpRequestMessage request,
            List<string> exposedFlightFeatures,
            EventTraceActivity traceActivityId,
            IList<PimsModel.V4.PaymentMethod> filteredPMs,
            string language = GlobalConstants.Defaults.Locale,
            bool isExpressCheckout = false,
            List<string> singleMarkets = null)
        {
            WalletConfig walletConfig = new WalletConfig
            {
                PIDLConfig = new PIDLConfig
                {
                    SelectResource = new SelectResource
                    {
                        Actions = new Actions
                        {
                            EwalletApayDefault = new List<string> { "PaymentInstrumentHandler.ClientSupported" },
                            EwalletGpayDefault = new List<string> { "PaymentInstrumentHandler.ClientSupported" }
                        }
                    },
                    HandlePaymentChallenge = new HandlePaymentChallenge
                    {
                        Actions = new Actions
                        {
                            EwalletApayDefault = new List<string> { "PaymentInstrumentHandler.CollectPaymentToken" },
                            EwalletGpayDefault = new List<string> { "PaymentInstrumentHandler.CollectPaymentToken" }
                        }
                    }
                },
                PaymentInstrumentHandlers = new List<PaymentInstrumentHandler>()
            };

            // Get non stored payment method id for google pay
            var gpayPiid = filteredPMs.ToList().Find(x => x.PaymentMethodType.ToLower() == V7.Constants.PaymentMethodType.GooglePay)?.Properties?.NonStoredPaymentMethodId;

            // Get non stored payment method id for Apple pay
            var apaypiid = filteredPMs.ToList().Find(x => x.PaymentMethodType.ToLower() == V7.Constants.PaymentMethodType.ApplePay)?.Properties?.NonStoredPaymentMethodId;

            GoogleProviderData gpayProvider = null;
            AppleProviderData apayProvider = null;
            List<DirectIntegrationData> directIntegrationData = response.DirectIntegrationData;
            Dictionary<string, List<string>> gpayCountrySupportedNetworks = null, apayCountrySupportedNetworks = null;
            foreach (var entry in directIntegrationData)
            {
                if (entry.PiType.Equals(WalletServiceConstants.ApplePay, System.StringComparison.OrdinalIgnoreCase))
                {
                    apayProvider = JsonConvert.DeserializeObject<AppleProviderData>(entry.ProviderData.ToString());
                    gpayCountrySupportedNetworks = entry.CountrySupportedNetworks;
                }
                else if (entry.PiType.Equals(WalletServiceConstants.GooglePay, System.StringComparison.OrdinalIgnoreCase))
                {
                    gpayProvider = JsonConvert.DeserializeObject<GoogleProviderData>(entry.ProviderData.ToString());
                    apayCountrySupportedNetworks = entry.CountrySupportedNetworks;
                }
            }

            if (gpayProvider != null)
            {
                GooglePaymentInstrumentHandler gpayInstrumentHandler = new GooglePaymentInstrumentHandler
                {
                    PaymentMethodFamily = PaymentMethodFamily.ewallet.ToString(),
                    Piid = gpayPiid,
                    PaymentMethodType = PaymentMethodType.GooglePay.ToString(),
                    AllowedAuthMethods = gpayProvider.AllowedMethods,
                    ProtocolVersion = gpayProvider.ProtocolVersion,
                    PublicKey = gpayProvider.PublicKey,
                    MerchantName = response.MerchantName,
                    PayLabel = PidlModel.V7.PidlModelHelper.GetLocalizedString(WalletConfigConstants.PayLabel, language),
                    ApiMajorVersion = gpayProvider.ApiMajorVersion,
                    ApiMinorVersion = gpayProvider.ApiMinorVersion,
                    PublicKeyVersion = gpayProvider.PublicKeyVersion,
                    AssuranceDetailsRequired = gpayProvider.AssuranceDetailsRequired,
                    IntegrationType = response.IntegrationType,
                    EnableGPayIframeForAllBrowsers = exposedFlightFeatures.Contains(Flighting.Features.PXEnableGPayIframeForAllBrowsers),
                    ClientSupported = new ClientSupported
                    {
                        SupportedBrowsers = new SupportedBrowsers
                        {
                            ChromeVersion = "100",
                            EdgeVersion = "100"
                        },
                        SupportedOS = new SupportedOS
                        {
                            IosVersion = "16.0",
                            AndroidVersion = "16.0",
                            WindowsVersion = "16.0"
                        },
                        AdditionalAPIsCheck = new List<string>() { "canMakePayment" }
                    },
                    AllowedAuthMethodsPerCountry = ConvertDictionaryValuesToUpperCase(gpayCountrySupportedNetworks),
                    MerchantId = gpayProvider.MerchantId,
                    EnableBillingAddress = true,
                    EnableEmail = true,
                    DisableGeoFencing = false,
                    SingleMarkets = singleMarkets,
                };

                // isExpressCheckout is true when expressCheckout component is used, such as candy crush checkout flow and webblends express checkout flow, and EnableEmail and EnableBillingAddress are true by default.
                // isExpressCheckout is false when expressCheckout component is not used, such as long form instance PI flow, and EnableEmail and EnableBillingAddress are false by default, but we can enable them by using PXWalletConfigEnableBillingAddress and PXWalletConfigEnableEmail flights.
                if (!isExpressCheckout)
                {
                    gpayInstrumentHandler.EnableBillingAddress = false;
                    gpayInstrumentHandler.EnableEmail = false;

                    if (exposedFlightFeatures.Contains(Flighting.Features.GPayApayInstancePI))
                    {
                        if (exposedFlightFeatures.Contains(Flighting.Features.PXWalletConfigEnableBillingAddress))
                        {
                            gpayInstrumentHandler.EnableBillingAddress = true;
                        }

                        if (exposedFlightFeatures.Contains(Flighting.Features.PXWalletConfigEnableEmail))
                        {
                            gpayInstrumentHandler.EnableEmail = true;
                        }
                    }
                }

                if (exposedFlightFeatures.Contains(Flighting.Features.PXWalletConfigAddDeviceSupportStatus))
                {
                    var browser = HttpRequestHelper.GetBrowser(request);
                    var os = HttpRequestHelper.GetOSFamily(request);
                    var deviceInfo = $"browser: {browser} os: {os}";

                    // Used as a disable for GooglePay
                    // will cover disable webblends: android edge, iphone edge and mac edge
                    if (exposedFlightFeatures.Contains(Flighting.Features.PXWalletConfigDisableGooglePay))
                    {
                        gpayInstrumentHandler.DeviceSupportedStatus = new DeviceSupportStatus
                        {
                            Result = false,
                            Reason = $"{WalletDeviceSupportedDebugMessages.ExcludedByFlight} {deviceInfo}"
                        };
                    }
                    else
                    {
                        gpayInstrumentHandler.DeviceSupportedStatus = GetDeviceSupportStatus(
                                WalletServiceConstants.GooglePay,
                                client,
                                request);
                    }
                }

                // To address the "fail to open windows" error caused by popup blockers on Android and Windows devices, we'll implement the following plan:
                // 1.For Android and Windows Devices: Use the native PaymentRequest API exclusively until the "fail to open windows" issue is resolved.
                // 2.For iOS and Mac Devices: Enable the fallback for webblends and can be expand to cart later
                if (exposedFlightFeatures.Contains(Flighting.Features.PXWalletConfigAddIframeFallbackSupported))
                {
                    gpayInstrumentHandler.IframeFallbackSupported = exposedFlightFeatures.Contains(Flighting.Features.PXWalletEnableGooglePayIframeFallback);
                }

                walletConfig.PaymentInstrumentHandlers.Add(gpayInstrumentHandler);
            }

            if (apayProvider != null)
            {
                ApplePaymentInstrumentHandler apayInstrumentHandler = new ApplePaymentInstrumentHandler
                {
                    PaymentMethodFamily = PaymentMethodFamily.ewallet.ToString(),
                    Piid = apaypiid,
                    PaymentMethodType = PaymentMethodType.ApplePay.ToString(),
                    PayLabel = PidlModel.V7.PidlModelHelper.GetLocalizedString(WalletConfigConstants.PayLabel, language),
                    MerchantIdentifier = apayProvider.MerchantIdentifier,
                    MerchantCapabilities = apayProvider.MerchantCapabilities,
                    DisplayName = WalletConfigConstants.DisplayName,
                    Initiative = WalletConfigConstants.Initiative,
                    InitiativeContext = WalletConfigConstants.InitiateContext,
                    ApplePayVersion = apayProvider.Version,
                    IntegrationType = response.IntegrationType,
                    ClientSupported = new ClientSupported
                    {
                        SupportedBrowsers = new SupportedBrowsers
                        {
                            SafariVersion = "16.1"
                        },
                        SupportedOS = new SupportedOS
                        {
                            IosVersion = "15.0"
                        },
                        PaymentProxyRequired = new PaymentProxy
                        {
                            SafariVersion = "16.5"
                        },
                        AdditionalAPIsCheck = new List<string>() { "canMakePaymentWithActiveCard" }
                    },
                    AllowedAuthMethodsPerCountry = ConvertDictionaryValuesToUpperCase(apayCountrySupportedNetworks),
                    EnableBillingAddress = true,
                    EnableEmail = true,
                    DisableGeoFencing = false,
                    SingleMarkets = singleMarkets,
                };

                if (!isExpressCheckout)
                {
                    apayInstrumentHandler.EnableBillingAddress = false;
                    apayInstrumentHandler.EnableEmail = false;

                    if (exposedFlightFeatures.Contains(Flighting.Features.GPayApayInstancePI))
                    {
                        if (exposedFlightFeatures.Contains(Flighting.Features.PXWalletConfigEnableBillingAddress))
                        {
                            apayInstrumentHandler.EnableBillingAddress = true;
                        }

                        if (exposedFlightFeatures.Contains(Flighting.Features.PXWalletConfigEnableEmail))
                        {
                            apayInstrumentHandler.EnableEmail = true;
                        }
                    }
                }

                if (exposedFlightFeatures.Contains(Flighting.Features.PXWalletConfigAddDeviceSupportStatus))
                {
                    try
                    {
                        // Used as disable for ApplePay unexcepted device support
                        // will cover webblends: disable edge mobile apple pay
                        if (exposedFlightFeatures.Contains(Flighting.Features.PXWalletConfigDisableApplePay))
                        {
                            apayInstrumentHandler.DeviceSupportedStatus = new DeviceSupportStatus
                            {
                                Result = false,
                                Reason = WalletDeviceSupportedDebugMessages.ExcludedByFlight
                            };
                        }
                        else
                        {
                            apayInstrumentHandler.DeviceSupportedStatus = GetDeviceSupportStatus(
                                WalletServiceConstants.ApplePay,
                                client,
                                request);
                        }
                    }
                    catch (Exception ex)
                    {
                        SllWebLogger.TracePXServiceException($"ClientInfo wrong format: {ex}", traceActivityId);
                    }
                }

                walletConfig.PaymentInstrumentHandlers.Add(apayInstrumentHandler);
            }

            return walletConfig;
        }

        private static Dictionary<string, List<string>> ConvertDictionaryValuesToUpperCase(Dictionary<string, List<string>> inputDictionary)
        {
            var outputDictionary = new Dictionary<string, List<string>>();

            foreach (KeyValuePair<string, List<string>> entry in inputDictionary)
            {
                List<string> outputList = entry.Value.ConvertAll(x => x.ToUpper());
                outputDictionary.Add(entry.Key, outputList);
            }

            return outputDictionary;
        }

        private static DeviceSupportStatus GetDeviceSupportStatus(
            string piType,
            string client,
            HttpRequestMessage request)
        {
            var browser = HttpRequestHelper.GetBrowser(request);
            var os = HttpRequestHelper.GetOSFamily(request);
            var deviceInfo = $"browser: {browser} os: {os}";

            if (!string.IsNullOrEmpty(client))
            {
                var clientInfo = JsonConvert.DeserializeObject<ClientInfo>(client);
                if (clientInfo.IsCrossOrigin
                    && piType == WalletServiceConstants.ApplePay
                    && HttpRequestHelper.GetBrowserMajorVer(request) < 17)
                {
                    return new DeviceSupportStatus
                    {
                        Result = false,
                        Reason = $"{WalletDeviceSupportedDebugMessages.IsCrossOrigin} {deviceInfo}"
                    };
                }
            }

            return new DeviceSupportStatus
            {
                Result = true,
                Reason = deviceInfo
            };
        }
    }
}