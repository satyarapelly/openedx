// <copyright file="SearchTransactionAccountinfoByPI.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7.PaymentTransaction.Model
{
    using Newtonsoft.Json;

    public class SearchTransactionAccountinfoByPI
    {
        [JsonProperty(PropertyName = "id")]
        public string PaymentInstrumentId { get; set; }

        [JsonProperty(PropertyName = "accountId")]
        public string PaymentInstrumentAccountId { get; set; }
    }
}