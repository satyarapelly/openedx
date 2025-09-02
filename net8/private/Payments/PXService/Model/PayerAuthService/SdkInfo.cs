// <copyright file="SdkInfo.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PayerAuthService
{
    using Newtonsoft.Json;
    using PayerAuthService;

    public class SdkInfo
    {
        [JsonProperty(PropertyName = "sdk_app_id")]
        public string SdkAppID { get; set; }

        [JsonProperty(PropertyName = "sdk_encrypted_data")]
        public string SdkEncryptedData { get; set; }

        [JsonProperty(PropertyName = "sdk_ephemeral_public_key")]
        public EphemPublicKey SdkEphemeralPublicKey { get; set; }

        [JsonProperty(PropertyName = "sdk_maximum_timeout")]
        public string SdkMaximumTimeout { get; set; }

        [JsonProperty(PropertyName = "sdk_reference_number")]
        public string SdkReferenceNumber { get; set; }

        [JsonProperty(PropertyName = "sdk_trans_id")]
        public string SdkTransID { get; set; }

        [JsonProperty(PropertyName = "device_render_option")]
        public DeviceRenderOptions DeviceRenderOption { get; set; }
    }
}