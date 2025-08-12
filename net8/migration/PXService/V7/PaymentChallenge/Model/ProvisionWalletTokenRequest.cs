// <copyright file="ProvisionWalletTokenRequest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.WalletService
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model;
    using Newtonsoft.Json;

    public class ProvisionWalletTokenRequest
    {
        [JsonProperty(PropertyName = "sessionId")]
        public string SessionId { get; set; }

        [JsonProperty(PropertyName = "accountId")]
        public string AccountId { get; set; }

        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }

        [JsonProperty(PropertyName = "piFamily")]
        public string PiFamily { get; set; }

        [JsonProperty(PropertyName = "piType")]
        public string PiType { get; set; }

        [JsonProperty(PropertyName = "integrationType")]
        public string IntegrationType { get; set; }

        [JsonProperty(PropertyName = "tokenReference")]
        public string TokenReference { get; set; }

        [JsonProperty(PropertyName = "transactionType")]
        public string TransactionType { get; set; }

        [JsonProperty(PropertyName = "authorizationGroups")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public List<WalletAuthorizationGroup> AuthorizationGroups { get; set; }
    }
}