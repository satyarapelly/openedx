// <copyright file="Card.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PXCommon;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class Card : RestResource
    {
        [JsonProperty(PropertyName = "id")]
        public string CardId { get; set; }

        [JsonProperty(PropertyName = "customer_id")]
        public string CustomerId { get; set; }

        [JsonProperty(PropertyName = "object_type")]
        public string ObjectType { get; set; }

        [JsonProperty(PropertyName = "account_holder_name")]
        public string CardAccountHolderName { get; set; }

        [JsonProperty(PropertyName = "expiry_year")]
        public string ExpiryYear { get; set; }

        [JsonProperty(PropertyName = "expiry_month")]
        public string ExpiryMonth { get; set; }

        [JsonProperty(PropertyName = "last_four_digits")]
        public string LastFourDigits { get; set; }

        [JsonProperty(PropertyName = "bin_number")]
        public string BinNumber { get; set; }

        [JsonProperty(PropertyName = "card_type")]
        public string CardType { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed for serialization purpose.")]
        [JsonProperty(PropertyName = "brands")]
        public List<string> Brands { get; set; }

        [JsonProperty(PropertyName = "address")]
        public AddressInfo Address { get; set; }

        [JsonProperty(PropertyName = "removed")]
        public bool Removed { get; set; }

        [JsonProperty(PropertyName = "card_number_id")]
        public string CardNumberId { get; set; }

        [JsonProperty(PropertyName = "display_name")]
        public string DisplayName { get; set; }
    }
}