// <copyright file="NetworkTokenInfo.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.NetworkTokenizationService
{
    /// <summary>
    /// A set of informational fields about the network token.
    /// </summary>
    public class NetworkTokenInfo
    {
        /// <summary>
        /// Gets or sets the status of the network token produced by this tokenization.
        /// </summary>
        public NetworkTokenStatus NetworkTokenStatus { get; set; }

        /// <summary>
        /// Gets or sets an expiration date of the network token.
        /// </summary>
        public CardExpirationDate ExpirationDate { get; set; }

        /// <summary>
        /// Gets or sets last 4 digits of the network token's number.
        /// </summary>
        public string LastFourDigits { get; set; }
    }
}
