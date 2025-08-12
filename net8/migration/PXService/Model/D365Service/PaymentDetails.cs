// <copyright file="PaymentDetails.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.D365Service
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class PaymentDetails
    {
        [JsonProperty("remitBy", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string RemitBy { get; set; }

        [JsonProperty("payments", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public IList<Payment> Payments { get; set; }
    }
}