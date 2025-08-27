// <copyright file="ServerSettingResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using Newtonsoft.Json;

    public class ServerSettingResponse
    {
        [JsonProperty(PropertyName = "usePidlUI")]
        public bool UsePidlUI { get; set; }
    }
}