// <copyright file="SessionDataResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.WalletService
{
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class SessionDataResponse
    {  
        [JsonProperty(PropertyName = "sessionData")]
        public SessionData SessionData { get; set; }  
    }

    [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Need multiple classes here")]
    public class SessionData
    {
        [JsonProperty(PropertyName = "sessionData")]
        public string EpochTimestamp { get; set; }

        [JsonProperty(PropertyName = "expiresAt")]
        public string ExpiresAt { get; set; }

        [JsonProperty(PropertyName = "merchantSessionIdentifier")]
        public string MerchantSessionIdentifier { get; set; }

        [JsonProperty(PropertyName = "nonce")]
        public string Nonce { get; set; }

        [JsonProperty(PropertyName = "merchantIdentifier")]
        public string MerchantIdentifier { get; set; }

        [JsonProperty(PropertyName = "domainName")]
        public string DomainName { get; set; }

        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "signature")]
        public string Signature { get; set; }

        [JsonProperty(PropertyName = "operationalAnalyticsIdentifier")]
        public string OperationalAnalyticsIdentifier { get; set; }

        [JsonProperty(PropertyName = "retries")]
        public string Retries { get; set; }

        [JsonProperty(PropertyName = "pspId")]
        public string PspId { get; set; }
    }
}
