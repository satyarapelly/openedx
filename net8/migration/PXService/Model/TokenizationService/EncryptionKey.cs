// <copyright file="EncryptionKey.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.TokenizationService
{    
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class EncryptionKey
    {
        [JsonProperty(PropertyName = "kid")]
        public string Kid { get; set; }

        [JsonProperty(PropertyName = "kty")]
        public string Kty { get; set; }        

        [JsonProperty(PropertyName = "key_ops")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public List<string> KeyOps { get; set; }

        [JsonProperty(PropertyName = "n")]
        public string Modulus { get; set; }

        [JsonProperty(PropertyName = "e")]
        public string PublicExponent { get; set; }

        [JsonProperty(PropertyName = "iat")]
        public long Iat { get; set; }

        [JsonProperty(PropertyName = "exp")]
        public long Exp { get; set; }

        [JsonProperty(PropertyName = "nbf")]
        public long Nbf { get; set; }

        [JsonProperty(PropertyName = "use")]
        public string Use { get; set; }

        [JsonProperty(PropertyName = "alg")]
        public string Algorithem { get; set; }
    }
}