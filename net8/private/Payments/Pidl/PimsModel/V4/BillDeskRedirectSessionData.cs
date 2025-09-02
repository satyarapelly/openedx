// <copyright file="BillDeskRedirectSessionData.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    using Newtonsoft.Json;

    public class BillDeskRedirectSessionData
    {
        [JsonProperty(PropertyName = "IsFullPageRedirect")]
        public bool IsFullPageRedirect { get; set; }
    }
}
