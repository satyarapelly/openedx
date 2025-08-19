// <copyright file="BillingInformation.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Tests.Common.Model.PurchaseService
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
    }
}