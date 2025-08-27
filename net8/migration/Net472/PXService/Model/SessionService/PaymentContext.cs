// <copyright file="PaymentContext.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.SessionService
{
    using Newtonsoft.Json;

    public class PaymentContext
    {
        [JsonProperty(PropertyName = "piid")]
        public string Piid { get; set; }

        [JsonProperty(PropertyName = "partner")]
        public string Partner { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; } 

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }

        [JsonProperty(PropertyName = "hasPreOrder")]
        public bool HasPreOrder { get; set; }

        [JsonProperty(PropertyName = "isMOTO")]
        public bool IsMOTO { get; set; }
        
        [JsonProperty(PropertyName = "challengeScenario")]
        public string ChallengeScenario { get; set; }

        [JsonProperty(PropertyName = "purchaseOrderId")]
        public string PurchaseOrderId { get; set; }

        [JsonProperty(PropertyName = "preTax")]
        public decimal? Pretax { get; set; }

        [JsonProperty(PropertyName = "postTax")]
        public decimal? PostTax { get; set; }

        [JsonProperty(PropertyName = "rewardsPoints")]
        public decimal? RewardsPoints { get; set; }
    }
}