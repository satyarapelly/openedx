// <copyright file="EligibilityResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.IssuerService
{
    using Newtonsoft.Json;

    public class EligibilityResponse
    {
        [JsonProperty(PropertyName = "eligibleToApply")]
        public bool EligibleToApply { get; set; }

        [JsonProperty(PropertyName = "prescreened")]
        public bool Prescreened { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string ApplicationStatus { get; set; }
    }
}