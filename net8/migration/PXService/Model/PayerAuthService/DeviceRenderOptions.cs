// <copyright file="DeviceRenderOptions.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PayerAuthService
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class DeviceRenderOptions
    {
        [JsonProperty(PropertyName = "sdk_interface")]
        public string SdkInterface { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        [JsonProperty(PropertyName = "sdk_ui_type")]
        public List<string> SdkUiType { get; set; }
    }
}