// <copyright file="WalletConfig.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PXInternal
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    /// <summary>
    /// This is a model used by PXService internally to adapt WalletService's payload to format required by PIDL.
    /// </summary>
    public class WalletConfig
    {
        public PIDLConfig PIDLConfig { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public List<PaymentInstrumentHandler> PaymentInstrumentHandlers { get; set; }
    }

    [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Need multiple classes here")]
    public class PIDLConfig
    {
        [JsonProperty("SelectResource.PaymentInstrument")]
        public SelectResource SelectResource { get; set; }

        public HandlePaymentChallenge HandlePaymentChallenge { get; set; }
    }

    public class ClientInfo
    {
        [JsonProperty("isCrossOrigin")]
        public bool IsCrossOrigin { get; set; }
    }

    public class SelectResource
    {
        [JsonProperty("actions")]
        public Actions Actions { get; set; }
    }

    public class HandlePaymentChallenge
    {
        [JsonProperty("actions")]
        public Actions Actions { get; set; }
    }

    public class Actions
    {
        [JsonProperty("ewallet.applepay.default")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public List<string> EwalletApayDefault { get; set; }

        [JsonProperty("ewallet.googlepay.default")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public List<string> EwalletGpayDefault { get; set; }
    }

    public class DeviceSupportStatus
    {
        [JsonProperty("result")]
        public bool Result { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }
    }

    public class PaymentInstrumentHandler
    {
        [JsonProperty("paymentMethodFamily")]
        public string PaymentMethodFamily { get; set; }

        [JsonProperty("paymentMethodType")]
        public string PaymentMethodType { get; set; }

        [JsonProperty("piid")]
        public string Piid { get; set; }

        [JsonProperty("payLabel")]
        public string PayLabel { get; set; }

        [JsonProperty("integrationType")]
        public string IntegrationType { get; set; }

        [JsonProperty("allowedAuthMethodsPerCountry")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public Dictionary<string, List<string>> AllowedAuthMethodsPerCountry { get; set; }

        [JsonProperty("clientSupported")]
        public ClientSupported ClientSupported { get; set; }

        [JsonProperty("deviceSupportStatus")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public DeviceSupportStatus DeviceSupportedStatus { get; set; }

        [JsonProperty("iframeFallbackSupported")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public bool? IframeFallbackSupported { get; set; }

        [JsonProperty("enableBillingAddress")]
        public bool EnableBillingAddress { get; set; }

        [JsonProperty("enableEmail")]
        public bool EnableEmail { get; set; }

        [JsonProperty("disableGeoFencing")]
        public bool DisableGeoFencing { get; set; }

        [JsonProperty("singleMarkets")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public List<string> SingleMarkets { get; set; }
    }

    public class GooglePaymentInstrumentHandler : PaymentInstrumentHandler
    {
        [JsonProperty("allowedAuthMethods")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public List<string> AllowedAuthMethods { get; set; }

        [JsonProperty("protocolVersion")]
        public string ProtocolVersion { get; set; }

        [JsonProperty("publicKey")]
        public string PublicKey { get; set; }

        [JsonProperty("merchantName")]
        public string MerchantName { get; set; }

        [JsonProperty("apiMajorVersion")]
        public string ApiMajorVersion { get; set; }

        [JsonProperty("apiMinorVersion")]
        public string ApiMinorVersion { get; set; }

        [JsonProperty("assuranceDetailsRequired")]
        public bool AssuranceDetailsRequired { get; set; }

        [JsonProperty("publicKeyVersion")]
        public string PublicKeyVersion { get; set; }

        [JsonProperty("enableGPayIframeForAllBrowsers")]
        public bool EnableGPayIframeForAllBrowsers { get; set; }

        [JsonProperty("merchantId")]
        public string MerchantId { get; set; }
    }

    public class ApplePaymentInstrumentHandler : PaymentInstrumentHandler
    {
        [JsonProperty("merchantCapabilities")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public List<string> MerchantCapabilities { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("initiative")]
        public string Initiative { get; set; }

        [JsonProperty("initiativeContext")]
        public string InitiativeContext { get; set; }

        [JsonProperty("merchantIdentifier")]
        public string MerchantIdentifier { get; set; }

        [JsonProperty("applePayVersion")]
        public string ApplePayVersion { get; set; }
    }

    public class AllowedAuthMethodsPerCountry
    {
        [JsonProperty("us")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public List<string> Us { get; set; }

        [JsonProperty("ca")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public List<string> Ca { get; set; }
    }

    public class ClientSupported
    {
        [JsonProperty("supportedBrowsers")]
        public SupportedBrowsers SupportedBrowsers { get; set; }

        [JsonProperty("supportedOS")]
        public SupportedOS SupportedOS { get; set; }

        [JsonProperty("additionalAPIsCheck")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public List<string> AdditionalAPIsCheck { get; set; }

        [JsonProperty("paymentProxyRequired")]
        public PaymentProxy PaymentProxyRequired { get; set; }
    }

    public class SupportedBrowsers
    {
        [JsonProperty("chrome")]
        public string ChromeVersion { get; set; }

        [JsonProperty("safari")]
        public string SafariVersion { get; set; }

        [JsonProperty("edge")]
        public string EdgeVersion { get; set; }
    }

    public class SupportedOS
    {
        [JsonProperty("ios")]
        public string IosVersion { get; set; }

        [JsonProperty("android")]
        public string AndroidVersion { get; set; }

        [JsonProperty("windows")]
        public string WindowsVersion { get; set; }
    }

    public class PaymentProxy
    {
        [JsonProperty("safari")]
        public string SafariVersion { get; set; }
    }
}