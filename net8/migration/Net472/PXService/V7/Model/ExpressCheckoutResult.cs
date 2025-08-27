// <copyright file="ExpressCheckoutResult.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Newtonsoft.Json;

    public class ExpressCheckoutResult
    {
        [JsonProperty(PropertyName = "pi")]
        public PaymentInstrument Pi { get; set; }

        [JsonProperty(PropertyName = "billingAddress")]
        public AddressInfoV3 BillingAddress { get; set; }
    }
}