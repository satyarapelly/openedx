// <copyright file="ProviderToken.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    using Newtonsoft.Json;

    public class ProviderToken
    {
        [JsonProperty(PropertyName = "billDeskToken")]
        public BillDeskToken BillDeskToken { get; set; }
    }
}
