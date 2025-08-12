// <copyright file="CustomerIdentityData.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.AccountService.DataModel
{
    using Newtonsoft.Json;

    public class CustomerIdentityData
    {
        [JsonProperty(PropertyName = "puid")]
        public string PUID { get; set; }
    }
}