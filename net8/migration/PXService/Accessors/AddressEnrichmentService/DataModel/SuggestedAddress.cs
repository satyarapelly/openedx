// <copyright file="SuggestedAddress.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.AddressEnrichmentService.DataModel
{
    using Newtonsoft.Json;
    using PimsModel.V4;

    public class SuggestedAddress
    {
        [JsonProperty(PropertyName = "mailability_score")]
        public string MailabilityScore { get; set; }

        [JsonProperty(PropertyName = "result_percentage")]
        public string ResultPercentage { get; set; }

        [JsonProperty(PropertyName = "address_type")]
        public string AddressType { get; set; }

        [JsonProperty(PropertyName = "address")]
        public Address Address { get; set; }

        public PXAddressV3Info ToPXAddressV3Info(string addressId, PXAddressV3Info userEntered)
        {
            return new PXAddressV3Info
            {
                Id = PidlFactory.GlobalConstants.SuggestedAddressesIds.Suggested + addressId,
                AddressLine1 = this.Address.AddressLine1,
                AddressLine2 = this.Address.AddressLine2,
                AddressLine3 = this.Address.AddressLine3,
                City = this.Address.City,
                Region = this.Address.Region,
                Country = this.Address.Country,
                PostalCode = this.Address.PostalCode,
                FirstName = userEntered.FirstName,
                LastName = userEntered.LastName,
                PhoneNumber = userEntered.PhoneNumber,
                DefaultBillingAddress = userEntered.DefaultBillingAddress,
                DefaultShippingAddress = userEntered.DefaultShippingAddress,
            };
        }

        public AddressInfoV3 ToAddressInfoV3(AddressInfoV3 userEntered)
        {
            return new AddressInfoV3
            {
                AddressLine1 = this.Address.AddressLine1,
                AddressLine2 = this.Address.AddressLine2,
                AddressLine3 = this.Address.AddressLine3,
                City = this.Address.City,
                Region = this.Address.Region,
                Country = this.Address.Country,
                PostalCode = this.Address.PostalCode,
                FirstName = userEntered.FirstName,
                LastName = userEntered.LastName,
                PhoneNumber = userEntered.PhoneNumber,
            };
        }
    }
}