// <copyright file="CustomerChallengeAttestationRequest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.TransactionDataService.DataModel
{
    using Newtonsoft.Json;

    public class CustomerChallengeAttestationRequest
    {
        public CustomerChallengeAttestationRequest(bool authenticationVerified)
        {
            this.Schema = "customer_challenge_attestation";
            this.Version = "1.0.0";
            this.Data = new CustomerChallengeAttestationData(authenticationVerified);
        }

        [JsonProperty(PropertyName = "schema")]
        public string Schema { get; set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "data")]
        public CustomerChallengeAttestationData Data { get; set; }
    }
}