// <copyright file="CustomerChallengeAttestationData.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.TransactionDataService.DataModel
{
    using Newtonsoft.Json;

    public class CustomerChallengeAttestationData
    {
        public CustomerChallengeAttestationData(bool authenticationVerified)
        {
            this.ChallengeType = "3ds2";
            this.AuthenticationVerified = authenticationVerified;
            this.Source = "SafetyNet";
        }

        [JsonProperty(PropertyName = "challengeType")]
        public string ChallengeType { get; set; }

        [JsonProperty(PropertyName = "authenticationVerified")]
        public bool AuthenticationVerified { get; set; }

        [JsonProperty(PropertyName = "source")]
        public string Source { get; set; }
    }
}