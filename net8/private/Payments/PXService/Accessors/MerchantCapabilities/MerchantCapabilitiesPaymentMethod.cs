// <copyright file="MerchantCapabilitiesPaymentMethod.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.MerchantCapabilitiesService.V7
{
    using Newtonsoft.Json;

    public class MerchantCapabilitiesPaymentMethod
    {
        [JsonProperty(PropertyName = "family")]
        public string PaymentMethodFamily { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string PaymentMethodType { get; set; }
    }
}