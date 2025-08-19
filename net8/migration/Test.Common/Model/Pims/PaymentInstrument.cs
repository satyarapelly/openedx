// <copyright file="PaymentInstrument.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model.Pims
{
    using System;
    using Microsoft.Commerce.Payments.PXCommon;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class PaymentInstrument : Microsoft.Commerce.Payments.PXCommon.RestResource
    {
        [JsonProperty(PropertyName = "id")]
        public string PaymentInstrumentId { get; set; }

        [JsonProperty(PropertyName = "accountId")]
        public string PaymentInstrumentAccountId { get; set; }

        [JsonProperty(PropertyName = "paymentMethod")]
        public PaymentMethod PaymentMethod { get; set; }

        [JsonProperty(PropertyName = "status")]
        [JsonConverter(typeof(StringEnumConverter))]
        public PaymentInstrumentStatus Status { get; set; }

        [JsonProperty(PropertyName = "creationDateTime", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime? CreationTime { get; set; }

        [JsonProperty(PropertyName = "lastUpdatedDateTime", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime? LastUpdatedTime { get; set; }

        [JsonProperty(PropertyName = "details")]
        public PaymentInstrumentDetails PaymentInstrumentDetails { get; set; }

        [JsonProperty(PropertyName = "clientAction")]
        public ClientAction ClientAction { get; set; }

        public object GetShallowCopyObj()
        {
            return this.MemberwiseClone();
        }
    }
}