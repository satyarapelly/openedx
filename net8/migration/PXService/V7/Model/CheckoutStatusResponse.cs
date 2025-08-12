// <copyright file="CheckoutStatusResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Microsoft.Commerce.Payments.PXService.V7
{
    public class CheckoutStatusResponse1
    {
        [JsonProperty(PropertyName = "checkoutStatus")]
        public string CheckoutStatus { get; set; }
    }
}