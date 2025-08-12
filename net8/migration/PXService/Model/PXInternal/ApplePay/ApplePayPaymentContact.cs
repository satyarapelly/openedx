// <copyright file="ApplePayPaymentContact.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PXInternal
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// This is a model used by PXService internally to extract apple pay payment token
    /// </summary>
    public class ApplePayPaymentContact
    {
        public ApplePayPaymentContact()
        {
        }

        /// <summary>
        /// Gets or sets phoneNumber - A phone number for the contact.
        /// </summary>
        [JsonProperty(PropertyName = "phoneNumber")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets emailAddress - An email address for the contact.
        /// </summary>
        [JsonProperty(PropertyName = "emailAddress")]
        public string EmailAddress { get; set; }

        /// <summary>
        /// Gets or sets givenName - The contact’s given name.
        /// </summary>
        [JsonProperty(PropertyName = "givenName")]
        public string GivenName { get; set; }

        /// <summary>
        /// Gets or sets familyName - The contact’s family name.
        /// </summary>
        [JsonProperty(PropertyName = "familyName")]
        public string FamilyName { get; set; }

        /// <summary>
        /// Gets or sets phoneticGivenName - The phonetic spelling of the contact’s given name.
        /// </summary>
        [JsonProperty(PropertyName = "phoneticGivenName")]
        public string PhoneticGivenName { get; set; }

        /// <summary>
        /// Gets or sets phoneticFamilyName - The phonetic spelling of the contact’s family name.
        /// </summary>
        [JsonProperty(PropertyName = "phoneticFamilyName")]
        public string PhoneticFamilyName { get; set; }

        /// <summary>
        /// Gets or sets addressLines - The street portion of the address for the contact.
        /// </summary>
        [JsonProperty(PropertyName = "addressLines")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set for serialization")]
        public List<string> AddressLines { get; set; }

        /// <summary>
        /// Gets or sets subLocality - Additional information associated with the location, typically defined at the city or town level (such as district or neighborhood), in a postal address.
        /// </summary>
        [JsonProperty(PropertyName = "subLocality")]
        public string SubLocality { get; set; }

        /// <summary>
        /// Gets or sets locality - The city for the contact.
        /// </summary>
        [JsonProperty(PropertyName = "locality")]
        public string Locality { get; set; }

        /// <summary>
        /// Gets or sets postalCode - The zip code or postal code, where applicable, for the contact.
        /// </summary>
        [JsonProperty(PropertyName = "postalCode")]
        public string PostalCode { get; set; }

        /// <summary>
        /// Gets or sets subAdministrativeArea - The subadministrative area (such as a county or other region) in a postal address.
        /// </summary>
        [JsonProperty(PropertyName = "subAdministrativeArea")]
        public string SubAdministrativeArea { get; set; }

        /// <summary>
        /// Gets or sets administrativeArea - The state for the contact.
        /// </summary>
        [JsonProperty(PropertyName = "administrativeArea")]
        public string AdministrativeArea { get; set; }

        /// <summary>
        /// Gets or sets country - The name of the country or region for the contact.
        /// </summary>
        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets countryCode - The contact’s two-letter ISO 3166 country code.
        /// </summary>
        [JsonProperty(PropertyName = "countryCode")]
        public string CountryCode { get; set; }
    }
}