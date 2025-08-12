// <copyright file="Mandate.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.NetworkTokenizationService
{
    /// <summary>
    /// Represents a mandate.
    /// </summary>
    public class Mandate
    {
        /// <summary>
        /// Gets or sets the mandate identifier.
        /// </summary>
        public string MandateId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the merchant name.
        /// </summary>
        public string MerchantName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the merchant category.
        /// </summary>
        public string MerchantCategory { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the merchant category code.
        /// </summary>
        public string MerchantCategoryCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the authentication amount.
        /// </summary>
        public decimal AuthenticationAmount { get; set; }

        /// <summary>
        /// Gets or sets the currency code.
        /// </summary>
        public string CurrencyCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the effective until date. In UNIX timestamp.
        /// </summary>
        public long EffectiveUntil { get; set; }
    }
}