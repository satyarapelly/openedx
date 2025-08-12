// <copyright file="RefundDetails.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.D365Service
{
    using System;
    using Newtonsoft.Json;

    public class RefundDetails : ChargeDetails
    {
        [JsonProperty("refundedDate", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public DateTimeOffset RefundedDate { get; set; }

        [JsonProperty("refundedReason", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string RefundedReason { get; set; }
    }
}