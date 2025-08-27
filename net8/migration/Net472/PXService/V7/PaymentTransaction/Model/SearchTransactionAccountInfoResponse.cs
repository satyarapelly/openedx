// <copyright file="SearchTransactionAccountInfoResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7.PaymentTransaction.Model
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class SearchTransactionAccountInfoResponse
    {
        [JsonProperty(PropertyName = "result")]
        public IEnumerable<SearchTransactionAccountinfoByPI> Result { get; set; }
    }
}