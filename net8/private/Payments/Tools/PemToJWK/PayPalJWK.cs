// <copyright file="PayPalJWK.cs" company="Microsoft">Copyright (c) Microsoft 2018. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.Tools.PemToJWK
{
    using System;
    using Newtonsoft.Json;

    public class PayPalJWK
    {
        [JsonProperty(PropertyName = "kty")]
        public string KeyType { get; private set; }

        [JsonProperty(PropertyName = "extractable")]
        public bool IsExtractable { get; private set; }

        [JsonProperty(PropertyName = "n")]
        public string Modulus { get; private set; }

        [JsonProperty(PropertyName = "e")]
        public string Exponent { get; private set; }

        public PayPalJWK(PayPalCrypto p)
        {
            this.KeyType = "RSA";
            this.IsExtractable = true;
            this.Modulus = Convert.ToBase64String(p.rsaParameters.Modulus);
            this.Exponent = Convert.ToBase64String(p.rsaParameters.Exponent);
        }
    }
}
