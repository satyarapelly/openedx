// <copyright file="CardExpirationDate.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.NetworkTokenizationService
{
    /// <summary>
    /// Expiration date of the card.
    /// </summary>
    public class CardExpirationDate
    {
        /// <summary>
        /// Gets or sets the year of expiration date.
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Gets or sets the month of expiration date.
        /// </summary>
        public int Month { get; set; }
    }
}