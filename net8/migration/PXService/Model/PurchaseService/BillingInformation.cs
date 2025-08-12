// <copyright file="BillingInformation.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PurchaseService
{
    using Newtonsoft.Json;

    public class BillingInformation
    {
        [JsonProperty("billingRecordId")]
        public string BillingRecordId { get; set; }

        [JsonProperty("billingRecordVersion")]
        public int BillingRecordVersion { get; set; }

        [JsonProperty("csvTopOffPaymentInstrumentId")]
        public string CsvTopOffPaymentInstrumentId { get; set; }

        [JsonProperty("paymentInstrumentId")]
        public string PaymentInstrumentId { get; set; }

        [JsonProperty("paymentInstrumentType")]
        public string PaymentInstrumentType { get; set; }

        [JsonProperty("sessionId")]
        public string SessionId { get; set; }
    }
}