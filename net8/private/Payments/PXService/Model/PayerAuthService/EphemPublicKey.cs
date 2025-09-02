// <copyright file="EphemPublicKey.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PayerAuthService
{
    using Newtonsoft.Json;
    
    public class EphemPublicKey
    {
        [JsonProperty(PropertyName = "kty", Required = Required.Always)]
        public string Kty { get; set; }

        [JsonProperty(PropertyName = "crv", Required = Required.Always)]
        public string Crv { get; set; }

        [JsonProperty(PropertyName = "x", Required = Required.Always)]
        public string X { get; set; }

        [JsonProperty(PropertyName = "y", Required = Required.Always)]
        public string Y { get; set; }
    }
}