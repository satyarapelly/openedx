// <copyright file="OrderPaymentInstrument.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PurchaseService
{
    using Newtonsoft.Json;

    public class OrderPaymentInstrument
    {
        [JsonProperty("paymentInstrumentId")]
        public string PaymentInstrumentId { get; set; }

        [JsonProperty("paymentInstrumentType")]
        public string PaymentInstrumentType { get; set; }

        [JsonProperty("chargedAmount")]
        public double ChargedAmount { get; set; }
    }
}