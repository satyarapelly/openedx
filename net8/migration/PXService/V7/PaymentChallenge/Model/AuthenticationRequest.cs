// <copyright file="AuthenticationRequest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;
    using PXService.Model.PayerAuthService;

    public class AuthenticationRequest
    {
        [JsonProperty(PropertyName = "sdkAppID", Required = Required.Always)]
        public string SdkAppId { get; set; }

        [JsonProperty(PropertyName = "sdkEncData", Required = Required.Always)]
        public string SdkEncData { get; set; }

        [JsonProperty(PropertyName = "sdkEphemeralPublicKey", Required = Required.Always)]
        public EphemPublicKey SdkEphemPublicKey { get; set; }

        [JsonProperty(PropertyName = "sdkMaxTimeout", Required = Required.Always)]
        public string SdkMaxTimeout { get; set; }

        [JsonProperty(PropertyName = "sdkReferenceNumber", Required = Required.Always)]
        public string SdkReferenceNumber { get; set; }

        [JsonProperty(PropertyName = "sdkTransID", Required = Required.Always)]
        public string SdkTransID { get; set; }

        [JsonProperty(PropertyName = "sdkInterface", Required = Required.Always)]
        public string SdkInterface { get; set; }

        [JsonProperty(PropertyName = "sdkUiType", Required = Required.Always)]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public List<string> SdkUiType { get; set; }

        [JsonProperty(PropertyName = "settingsVersion")]
        public string SettingsVersion { get; set; }

        [JsonProperty(PropertyName = "settingsVersionTryCount")]
        public ushort SettingsVersionTryCount { get; set; }

        [JsonProperty(PropertyName = "sessionToken")]
        public string SessionToken { get; set; }

        [JsonProperty(PropertyName = "sdkClientVersion")]
        public string SDKClientVersion { get; set; }

        [JsonProperty(PropertyName = "language")]
        public string Language { get; set; }

        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }
        
        [JsonProperty(PropertyName = "partner")]
        public string Partner { get; set; }

        // Latest version of the PSD2 SDK, default to 2.2.0 if not in request
        [JsonProperty(PropertyName = "messageVersion")]
        public string MessageVersion { get; set; } = "2.2.0";
    }
}