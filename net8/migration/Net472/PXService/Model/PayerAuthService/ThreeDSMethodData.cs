// <copyright file="ThreeDSMethodData.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PayerAuthService
{
    using Newtonsoft.Json;

    public class ThreeDSMethodData
    {
        [JsonProperty(PropertyName = "three_ds_method_url")]
        public string ThreeDSMethodURL { get; set; }

        [JsonProperty(PropertyName = "three_ds_server_trans_id")]
        public string ThreeDSServerTransID { get; set; }
    }
}