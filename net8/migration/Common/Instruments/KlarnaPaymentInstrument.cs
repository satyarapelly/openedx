// <copyright file="KlarnaPaymentInstrument.cs" company="Microsoft">Copyright (c) Microsoft 2017. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Instruments
{
    using Newtonsoft.Json;

    public class KlarnaPaymentInstrument : PaymentInstrument
    {
        public KlarnaPaymentInstrument()
            : base(PaymentMethodRegistry.Klarna)
        {
        }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("gender")]
        public string Gender { get; set; }

        [JsonProperty("dateOfBirth")]
        public string DateOfBirth { get; set; }

        [JsonProperty("telephoneNumber")]
        public string TelephoneNumber { get; set; }

        [JsonProperty("nationalIdentificationNumber")]
        public string NationalIdentificationNumber { get; set; }

        [JsonProperty("shopperEmail")]
        public string ShopperEmail { get; set; }

        [JsonProperty("street")]
        public string Street { get; set; }

        [JsonProperty("houseNumberOrName")]
        public string HouseNumberOrName { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("postalCode")]
        public string PostalCode { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }
    }
}
