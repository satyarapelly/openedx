// <copyright file="Address.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.AddressEnrichmentService.DataModel
{
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Newtonsoft.Json;

    public class Address
    {
        public Address()
        {
        }

        public Address(AddressInfoV3 address)
        {
            this.AddressLine1 = address.AddressLine1;
            this.AddressLine2 = address.AddressLine2;
            this.AddressLine3 = address.AddressLine3;
            this.City = address.City;
            this.Region = address.Region;
            this.PostalCode = address.PostalCode;
            this.Country = address.Country;
        }

        public Address(PXAddressV3Info address)
        {
            this.AddressLine1 = address.AddressLine1;
            this.AddressLine2 = address.AddressLine2;
            this.AddressLine3 = address.AddressLine3;
            this.City = address.City;
            this.Region = address.Region;
            this.PostalCode = address.PostalCode;
            this.Country = address.Country;
        }

        [JsonProperty("address_line1")]
        public string AddressLine1 { get; set; }

        [JsonProperty("address_line2")]
        public string AddressLine2 { get; set; }

        [JsonProperty("address_line3")]
        public string AddressLine3 { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("postal_code")]
        public string PostalCode { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        public PXAddressV3Info ToPXAddressV3Info(string addressId, PXAddressV3Info userEntered)
        {
            return new PXAddressV3Info
            {
                Id = PidlFactory.GlobalConstants.SuggestedAddressesIds.Suggested + addressId,
                AddressLine1 = this.AddressLine1,
                AddressLine2 = this.AddressLine2,
                AddressLine3 = this.AddressLine3,
                City = this.City,
                Region = this.Region,
                Country = this.Country,
                PostalCode = this.PostalCode,
                FirstName = userEntered.FirstName,
                LastName = userEntered.LastName,
                PhoneNumber = userEntered.PhoneNumber,
                DefaultBillingAddress = userEntered.DefaultBillingAddress,
                DefaultShippingAddress = userEntered.DefaultShippingAddress,
            };
        }
    }
}