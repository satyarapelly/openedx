// <copyright file="Payment.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.D365Service
{
    using Newtonsoft.Json;

    public class Payment
    {
        [JsonProperty("paymentInstrumentId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string PaymentInstrumentId { get; set; }

        [JsonProperty("totalAmount", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public decimal TotalAmount { get; set; }

        [JsonProperty("isPrimary", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public bool IsPrimary { get; set; }
    }
}