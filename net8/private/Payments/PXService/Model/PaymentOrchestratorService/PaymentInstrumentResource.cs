// <copyright file="PaymentInstrumentResource.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class PaymentInstrumentResource
    {
        [JsonProperty(Order = 1, PropertyName = "id")]
        public string PaymentInstrumentId { get; set; }

        [JsonProperty(Order = 2, PropertyName = "accountId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string AccountId { get; set; }

        [JsonProperty(Order = 4, PropertyName = "paymentMethod")]
        public PaymentMethod PaymentMethod { get; set; }

        /// <summary>
        /// Gets or sets the payment instrument status
        /// </summary>
        [JsonProperty(Order = 3, PropertyName = "status")]
        [JsonConverter(typeof(StringEnumConverter))]
        public PaymentInstrumentStatus Status { get; set; }

        [JsonProperty(Order = 6, PropertyName = "creationDateTime", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime? CreationTime { get; set; }

        [JsonProperty(Order = 7, PropertyName = "lastUpdatedDateTime", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime? LastUpdatedTime { get; set; }

        [JsonProperty(Order = 5, PropertyName = "details")]
        [SuppressMessage("Microsoft.Performance", "CA1811", Justification = "Keep private set for Json deserialization")]
        public PIDetails PaymentInstrumentDetails { get; set; }

        [JsonProperty(Order = 11, PropertyName = "chargeable", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool? Chargeable { get; set; }
    }
}