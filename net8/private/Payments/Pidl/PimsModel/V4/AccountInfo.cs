// <copyright file="AccountInfo.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    using Newtonsoft.Json;

    public class AccountInfo
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
    }
}