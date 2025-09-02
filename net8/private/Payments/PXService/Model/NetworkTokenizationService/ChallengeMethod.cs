// <copyright file="ChallengeMethod.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.NetworkTokenizationService
{
    /// <summary>
    /// Represents a single challenge method returned by the provider.
    /// </summary>
    public class ChallengeMethod
    {
        /// <summary>
        /// Gets or sets the challenge method type.
        /// </summary>
        public ChallengeMethodType ChallengeMethodType { get; set; }

        /// <summary>
        /// Gets or sets the value associated with the indicated challenge method type.
        /// </summary>
        public string ChallengeValue { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for this challenge. This is used in future challenge requests.
        /// </summary>
        public string ChallengeMethodId { get; set; }
    }
}