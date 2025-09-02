// <copyright file="PaymentSessionData.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;
    using PXService.Model.ThreeDSExternalService;

    public class PaymentSessionData
    {
        [JsonProperty(PropertyName = "piid", Required = Required.Always)]
        public string PaymentInstrumentId { get; set; }

        [JsonProperty(PropertyName = "language", Required = Required.Always)]
        public string Language { get; set; }

        [JsonProperty(PropertyName = "partner", Required = Required.Always)]
        public string Partner { get; set; }

        [JsonProperty(PropertyName = "piCid")]
        public string PaymentInstrumentAccountId { get; set; }

        [JsonProperty(PropertyName = "amount", Required = Required.Always)]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "currency", Required = Required.Always)]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "country", Required = Required.Always)]
        public string Country { get; set; }

        [JsonProperty(PropertyName = "hasPreOrder")]
        public bool HasPreOrder { get; set; }

        // This is the ID created by OMS for OMS Legacy purchases. Its also called OCPTenantId.
        [JsonProperty(PropertyName = "caid")]
        public string CommercialAccountId { get; set; }

        [JsonProperty(PropertyName = "isLegacy")]
        public bool IsLegacy { get; set; }

        [JsonProperty(PropertyName = "isMOTO")]
        public bool IsMOTO { get; set; }

        [JsonProperty(PropertyName = "challengeScenario")]
        public ChallengeScenario ChallengeScenario { get; set; }

        [JsonProperty(PropertyName = "challengeWindowSize")]
        public ChallengeWindowSize ChallengeWindowSize { get; set; }

        [JsonProperty(PropertyName = GlobalConstants.QueryParamNames.BillableAccountId)]
        public string BillableAccountId { get; set; }

        [JsonProperty(PropertyName = GlobalConstants.QueryParamNames.ClassicProduct)]
        public string ClassicProduct { get; set; }

        [JsonProperty(PropertyName = "purchaseOrderId")]
        public string PurchaseOrderId { get; set; }

        [JsonProperty(PropertyName = "paymentMethodType")]
        public string PaymentMethodType { get; set; }

        [JsonProperty(PropertyName = "redeemRewards")]
        public bool RedeemRewards { get; set; }

        [JsonProperty(PropertyName = "rewardsPoints")]
        public int RewardsPoints { get; set; }

        [JsonProperty(PropertyName = "authorizationGroups")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public List<AuthorizationGroup> AuthorizationGroups { get; set; }
    }
}
