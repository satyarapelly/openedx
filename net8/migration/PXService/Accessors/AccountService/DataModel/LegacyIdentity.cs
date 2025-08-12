// <copyright file="LegacyIdentity.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.AccountService.DataModel
{
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class LegacyIdentity
    {
        [SuppressMessage("Microsoft.Naming", "CA1721", Justification = "Json Property name")]
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }
    }
}