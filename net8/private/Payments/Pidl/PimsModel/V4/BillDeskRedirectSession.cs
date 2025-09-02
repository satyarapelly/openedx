// <copyright file="BillDeskRedirectSession.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    using Newtonsoft.Json;

    public class BillDeskRedirectSession
    {
        [JsonProperty(PropertyName = "Data")]
        public string Data { get; set; }
    }
}
