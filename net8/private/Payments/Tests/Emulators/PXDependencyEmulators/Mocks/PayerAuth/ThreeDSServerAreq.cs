// <copyright file="ThreeDSServerAreq.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks
{
    using Newtonsoft.Json;
    using PXService.Model.PayerAuthService;

    public class ThreeDSServerAreq
    {
        [JsonProperty(PropertyName = "threeDSRequestorAuthenticationInd")]
        public string ThreeDSRequestorAuthenticationInd { get; set; }

        [JsonProperty(PropertyName = "threeDSRequestorChallengeInd")]
        public string ThreeDSRequestorChallengeInd { get; set; }

        [JsonProperty(PropertyName = "threeDSRequestorID")]
        public string ThreeDSRequestorID { get; set; }

        [JsonProperty(PropertyName = "threeDSRequestorName")]
        public string ThreeDSRequestorName { get; set; }

        [JsonProperty(PropertyName = "threeDSRequestorURL")]
        public string ThreeDSRequestorURL { get; set; }

        [JsonProperty(PropertyName = "acquirerBIN")]
        public string AcquirerBIN { get; set; }

        [JsonProperty(PropertyName = "acquirerMerchantID")]
        public string AcquirerMerchantID { get; set; }

        [JsonProperty(PropertyName = "threeDSCompInd")]
        public string ThreeDSCompInd { get; set; }

        [JsonProperty(PropertyName = "browserJavaEnabled")]
        public string BrowserJavaEnabled { get; set; }

        [JsonProperty(PropertyName = "sdkAppID")]
        public string SdkAppID { get; set; }

        [JsonProperty(PropertyName = "sdkEncData")]
        public string SdkEncData { get; set; }

        [JsonProperty(PropertyName = "sdkEphemPubKey")]
        public EphemPublicKey SdkEphemPubKey { get; set; }

        [JsonProperty(PropertyName = "sdkMaxTimeout")]
        public string SdkMaxTimeout { get; set; }

        [JsonProperty(PropertyName = "sdkReferenceNumber")]
        public string SdkReferenceNumber { get; set; }

        [JsonProperty(PropertyName = "sdkTransID")]
        public string SdkTransID { get; set; }

        [JsonProperty(PropertyName = "cardExpiryDate")]
        public string CardExpiryDate { get; set; }

        [JsonProperty(PropertyName = "encryptedData")]
        public string EncryptedData { get; set; }

        [JsonProperty(PropertyName = "billAddrCity")]
        public string BillAddrCity { get; set; }

        [JsonProperty(PropertyName = "billAddrCountry")]
        public string BillAddrCountry { get; set; }

        [JsonProperty(PropertyName = "billAddrLine1")]
        public string BillAddrLine1 { get; set; }

        [JsonProperty(PropertyName = "billAddrPostCode")]
        public string BillAddrPostCode { get; set; }

        [JsonProperty(PropertyName = "billAddrState")]
        public string BillAddrState { get; set; }

        [JsonProperty(PropertyName = "deviceChannel")]
        public string DeviceChannel { get; set; }

        [JsonProperty(PropertyName = "deviceRenderOptions")]
        public dynamic DeviceRenderOptions { get; set; }

        [JsonProperty(PropertyName = "Mcc")]
        public string Mcc { get; set; }

        [JsonProperty(PropertyName = "MerchantCountryCode")]
        public string MerchantCountryCode { get; set; }

        [JsonProperty(PropertyName = "MerchantName")]
        public string MerchantName { get; set; }

        [JsonProperty(PropertyName = "MessageCategory")]
        public string MessageCategory { get; set; }

        [JsonProperty(PropertyName = "purchaseAmount")]
        public string PurchaseAmount { get; set; }

        [JsonProperty(PropertyName = "purchaseCurrency")]
        public string PurchaseCurrency { get; set; }

        [JsonProperty(PropertyName = "purchaseExponent")]
        public string PurchaseExponent { get; set; }

        [JsonProperty(PropertyName = "purchaseDate")]
        public string PurchaseDate { get; set; }

        [JsonProperty(PropertyName = "transType")]
        public string TransType { get; set; }

        [JsonProperty(PropertyName = "generateCReq")]
        public string GenerateCReq { get; set; }

        [JsonProperty(PropertyName = "threeDSServerTransID")]
        public string ThreeDSServerTransID { get; set; }

        [JsonProperty(PropertyName = "notificationURL")]
        public string NotificationURL { get; set; }

        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }

        [JsonProperty(PropertyName = "partner")]
        public string Partner { get; set; }

        [JsonProperty(PropertyName = "messageVersion")]
        public string MessageVersion { get; set; }
    }
}