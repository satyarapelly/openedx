// <copyright file="Purchaser.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PurchaseService
{
    using Newtonsoft.Json;

    public class Purchaser
    {
        [JsonProperty("identityType")]
        public string IdentityType { get; set; }

        [JsonProperty("identityValue")]
        public string IdentityValue { get; set; }
    }
}