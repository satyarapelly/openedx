// <copyright file="GoogleProviderData.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.WalletService
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class GoogleProviderData
    {
        [JsonProperty(PropertyName = "allowedMethods")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public List<string> AllowedMethods { get; set; }

        [JsonProperty(PropertyName = "assuranceDetailsRequired")]
        public bool AssuranceDetailsRequired { get; set; }

        [JsonProperty(PropertyName = "protocolVersion")]
        public string ProtocolVersion { get; set; }

        [JsonProperty(PropertyName = "publicKey")]
        public string PublicKey { get; set; }

        [JsonProperty(PropertyName = "publicKeyVersion")]
        public string PublicKeyVersion { get; set; }

        [JsonProperty(PropertyName = "apiMajorVersion")]
        public string ApiMajorVersion { get; set; }

        [JsonProperty(PropertyName = "apiMinorVersion")]
        public string ApiMinorVersion { get; set; }

        [JsonProperty(PropertyName = "merchantId")]
        public string MerchantId { get; set; }
    }
}