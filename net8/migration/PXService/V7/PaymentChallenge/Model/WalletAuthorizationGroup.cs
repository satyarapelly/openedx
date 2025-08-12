// <copyright file="WalletAuthorizationGroup.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model
{
    using Newtonsoft.Json;

    public class WalletAuthorizationGroup
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "totalAmount")]
        public decimal TotalAmount { get; set; }
    }
}