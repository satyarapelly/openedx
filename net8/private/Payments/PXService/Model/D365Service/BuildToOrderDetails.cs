// <copyright file="BuildToOrderDetails.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.D365Service
{
    using System;
    using Newtonsoft.Json;

    public class BuildToOrderDetails
    {
        [JsonProperty("buildToOrderImageUrl", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public Uri BuildToOrderImageUrl { get; set; }

        [JsonProperty("engraving", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string Engraving { get; set; }

        [JsonProperty("referenceId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string ReferenceId { get; set; }
    }
}