// <copyright file="CheckoutStatusResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

using Newtonsoft.Json;

namespace Microsoft.Commerce.Payments.PXService.V7
{
    public class CheckoutStatusResponse
    {
        [JsonProperty(PropertyName = "checkoutStatus")]
        public string CheckoutStatus { get; set; }
    }
}