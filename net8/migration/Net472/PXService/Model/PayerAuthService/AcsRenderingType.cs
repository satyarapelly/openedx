// <copyright file="AcsRenderingType.cs" company="Microsoft">Copyright (c) Microsoft 2020. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PayerAuthService
{
    using Newtonsoft.Json;

    public class AcsRenderingType
    {
        [JsonProperty(PropertyName = "acs_interface", Required = Required.Always)]
        public string AcsInterface { get; set; }

        [JsonProperty(PropertyName = "acs_ui_template", Required = Required.Always)]
        public string AcsUiTemplate { get; set; }
    }
}