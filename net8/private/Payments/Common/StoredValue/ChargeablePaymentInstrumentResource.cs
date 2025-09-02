// <copyright file="chargeablePaymentInstrumentResource.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.StoredValue
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// ChargeablePaymentInstrumentResource is a copy of stored value core resource
    /// </summary>
    public class ChargeablePaymentInstrumentResource
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("tax_required")]
        public bool TaxRequired { get; set; }
    }
}