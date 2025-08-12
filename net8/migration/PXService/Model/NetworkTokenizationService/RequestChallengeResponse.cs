// <copyright file="RequestChallengeResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.NetworkTokenizationService
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Represents a RequestChellenge API call response.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class RequestChallengeResponse
    {
        /// <summary>
        /// Gets or sets the number of times the consumer can request a new challenge for a token.
        /// After the number of attempts is exceeded, the ability to challenge the customer is disabled for this token.
        /// </summary>
        public int MaxChallengeAttempts { get; set; }

        /// <summary>
        /// Gets or sets the number of retries a consumer can try to validate a single challenge with OTP before they must request a new one.
        /// </summary>
        public int MaxValidationAttempts { get; set; }

        /// <summary>
        /// Gets or sets the timeout for challenge validation: the challenge validation will fail if the consumer submits OTP after this period. This result is in minutes.
        /// </summary>
        public int ChallengeTimeout { get; set; }
    }
}
