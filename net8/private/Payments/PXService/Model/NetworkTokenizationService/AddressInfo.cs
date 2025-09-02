// <copyright file="AddressInfo.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.NetworkTokenizationService
{
    /// <summary>
    /// Customer address information.
    /// </summary>
    public class AddressInfo
    {
        /// <summary>
        /// Gets or sets the unit number.
        /// </summary>
        public string UnitNumber { get; set; }

        /// <summary>
        /// Gets or sets the address line 1.
        /// </summary>
        public string Street { get; set; }

        /// <summary>
        /// Gets or sets the address line 2.
        /// </summary>
        public string StreetLine2 { get; set; }

        /// <summary>
        /// Gets or sets the address line 3.
        /// </summary>
        public string StreetLine3 { get; set; }

        /// <summary>
        /// Gets or sets the city name.
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the postal code.
        /// </summary>
        public string PostalCode { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the country code.
        /// </summary>
        public string CountryCode { get; set; }
    }
}