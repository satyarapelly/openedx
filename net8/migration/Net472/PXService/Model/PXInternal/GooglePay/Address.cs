// <copyright file="Address.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PXInternal
{
    using Newtonsoft.Json;

    /// <summary>
    /// This is a model used by PXService internally to extract google pay payment data
    /// </summary>
    public class Address
    {
        public Address()
        {
        }

        /// <summary>
        /// Gets or sets name - The full name of the addressee
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets postalCode - The postal or ZIP code.
        /// </summary>
        [JsonProperty(PropertyName = "postalCode")]
        public string PostalCode { get; set; }

        /// <summary>
        /// Gets or sets countryCode - ISO 3166-1 alpha-2 country code.
        /// </summary>
        [JsonProperty(PropertyName = "countryCode")]
        public string CountryCode { get; set; }

        /// <summary>
        /// Gets or sets phoneNumber - A telephone number, if phoneNumberRequired is set to true in the PaymentDataRequest.
        /// </summary>
        [JsonProperty(PropertyName = "phoneNumber")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets address1 - The first line of the address.
        /// </summary>
        [JsonProperty(PropertyName = "address1")]
        public string Address1 { get; set; }

        /// <summary>
        /// Gets or sets address2 - The second line of the address.
        /// </summary>
        [JsonProperty(PropertyName = "address2")]
        public string Address2 { get; set; }

        /// <summary>
        /// Gets or sets address3 - The third line of the addr
        /// </summary>
        [JsonProperty(PropertyName = "address3")]
        public string Address3 { get; set; }

        /// <summary>
        /// Gets or sets locality - City, town, neighborhood, or suburb.
        /// </summary>
        [JsonProperty(PropertyName = "locality")]
        public string Locality { get; set; }

        /// <summary>
        /// Gets or sets administrativeArea - A country subdivision, such as a state or province.
        /// </summary>
        [JsonProperty(PropertyName = "administrativeArea")]
        public string AdministrativeArea { get; set; }

        /// <summary>
        /// Gets or sets sortingCode - The sorting code.
        /// </summary>
        [JsonProperty(PropertyName = "sortingCode")]
        public string SortingCode { get; set; }
    }
}