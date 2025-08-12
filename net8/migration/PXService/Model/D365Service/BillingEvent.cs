// <copyright file="BillingEvent.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.D365Service
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class BillingEvent
    {
        [JsonProperty("eventId", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string EventId { get; set; }

        [JsonProperty("eventType", Required = Required.Always)]
        public BillingEventType EventType { get; set; }

        [JsonProperty("paymentInstrumentId", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string PaymentInstrumentId { get; set; }

        [JsonProperty("totalAmount", Required = Required.Always)]
        public decimal? TotalAmount { get; set; }

        [JsonProperty("transactionDate", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? TransactionDate { get; set; }

        [JsonProperty("lineItemIds", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        public IList<string> LineItemIds { get; }
    }
}