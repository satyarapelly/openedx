// <copyright file="Price.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentThirdPartyService
{
    using Microsoft.Commerce.Payments.Common.Web;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public class Price
    {
        [JsonConverter(typeof(LongStringConverter))]
        [JsonProperty(PropertyName = "amount")]
        public long Amount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public Currency Currency { get; set; }
    }
}