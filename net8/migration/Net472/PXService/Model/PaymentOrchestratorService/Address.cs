// <copyright file="Address.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{
    using System.ComponentModel.DataAnnotations;

    public class Address
    {
        /// <summary>
        /// Gets or sets the country code for the product or service being sold. The format is ISO 3166 (e.g. US, UK, DE).
        /// </summary>
        [Required(ErrorMessage = "{0} is a mandatory field")]
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the state or province.
        /// </summary>
        [Required(ErrorMessage = "{0} is a mandatory field")]
        public string Region { get; set; }

        /// <summary>
        /// Gets or sets the district.
        /// </summary>
        public string District { get; set; }

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        [Required(ErrorMessage = "{0} is a mandatory field")]
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the street address.
        /// </summary>
        [Required(ErrorMessage = "{0} is a mandatory field")]
        public string AddressLine1 { get; set; }

        /// <summary>
        /// Gets or sets additional address information.
        /// </summary>
        public string AddressLine2 { get; set; }

        /// <summary>
        /// Gets or sets extended direction to the address.
        /// </summary>
        public string AddressLine3 { get; set; }

        /// <summary>
        /// Gets or sets the postal code or zip code.
        /// </summary>
        public string PostalCode { get; set; }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the full name.
        /// </summary>
        public string CorrespondenceName { get; set; }

        /// <summary>
        /// Gets or sets the phone number belong to the address.
        /// </summary>
        public string PhoneNumber { get; set; }
    }
}