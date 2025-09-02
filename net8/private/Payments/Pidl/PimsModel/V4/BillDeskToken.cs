// <copyright file="BillDeskToken.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    using Newtonsoft.Json;

    public class BillDeskToken : TokenInfo
    {
        [JsonProperty(PropertyName = "cardAccountId")]
        public string CardAccountId { get; set; }
    }
}
