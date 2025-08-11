// <copyright file="AuthenticationData.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Transaction
{
    using Newtonsoft.Json;

    public class AuthenticationData
    {
        [JsonProperty(PropertyName = "challenge_type")]
        public ChallengeType ChallengeType { get; set; }

        public string Data { get; set; }

        public AuthenticationData Clone()
        {
            return new AuthenticationData
            {
                ChallengeType = this.ChallengeType,
                Data = this.Data,
            };
        }
    }
}
