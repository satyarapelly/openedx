// <copyright file="BillingInformation.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.D365Service
{
    using Newtonsoft.Json;

    public class BillingInformation
    {
        [JsonProperty("soldToAddressId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string SoldToAddressId { get; set; }
    }
}