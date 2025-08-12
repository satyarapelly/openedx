// <copyright file="SessionDataModel.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.ChallengeManagementService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using Newtonsoft.Json;

    public class SessionDataModel
    {
        [JsonProperty(PropertyName = "accountId")]
        public string AccountId { get; set; }

        [JsonProperty(PropertyName = "cardNumber")]
        public string CardNumber { get; set; }

        [JsonProperty(PropertyName = "partner")]
        public string Partner { get; set; }

        [JsonProperty(PropertyName = "language")]
        public string Language { get; set; }

        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }

        [JsonProperty(PropertyName = "operation")]
        public string Operation { get; set; }

        [JsonProperty(PropertyName = "family")]
        public string Family { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string CardType { get; set; }

        [JsonProperty(PropertyName = "challengeRequired")]
        public bool ChallengeRequired { get; set; }

        [JsonProperty(PropertyName = "challengeCompleted")]
        public bool ChallengeCompleted { get; set; }

        [JsonProperty(PropertyName = "challengeRetries")]
        public int ChallengeRetries { get; set; }

        [JsonProperty(PropertyName = "pidlSdkVersion")]
        public string PidlSdkVersion { get; set; }
    }
}