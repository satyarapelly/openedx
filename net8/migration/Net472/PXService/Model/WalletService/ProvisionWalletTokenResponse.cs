// <copyright file="ProvisionWalletTokenResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.WalletService
{
    using Newtonsoft.Json;

    public class ProvisionWalletTokenResponse
    {
        [JsonProperty(PropertyName = "eci")]
        public string Eci { get; set; }

        [JsonProperty(PropertyName = "hasCryptogram")]
        public string HasCryptogram { get; set; }

        [JsonProperty(PropertyName = "walletMetadata")]
        public object WalletMetadata { get; set; }
    }
}