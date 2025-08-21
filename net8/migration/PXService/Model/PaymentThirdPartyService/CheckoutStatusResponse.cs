// <copyright file="CheckoutStatusResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentThirdPartyService
{
    using Newtonsoft.Json;

    public class CheckoutStatusResponse1
    {
        [JsonProperty(PropertyName = "checkoutStatus")]
        public string CheckoutStatus { get; set; }
    }
}