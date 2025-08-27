// <copyright file="Currency.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentThirdPartyService
{
    using Newtonsoft.Json;

    public class Currency
    {
        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }
    }
}