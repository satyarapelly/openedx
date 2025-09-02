// <copyright file="PaymentInstrumentDetails.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentThirdPartyService
{
    using Newtonsoft.Json;

    public class PaymentInstrumentDetails
    {
        [JsonProperty(PropertyName = "accountHolderName")]
        public string CardHolderName { get; set; }

        [JsonProperty(PropertyName = "accountToken")]
        public string TokenizedAccountNumber { get; set; }

        [JsonProperty(PropertyName = "cvvToken")]
        public string TokenizedCvv { get; set; }

        [JsonProperty(PropertyName = "expiryYear")]
        public string ExpiryYear { get; set; }

        [JsonProperty(PropertyName = "expiryMonth")]
        public string ExpiryMonth { get; set; }

        [JsonProperty(PropertyName = "address")]
        public Address Address { get; set; }
    }
}