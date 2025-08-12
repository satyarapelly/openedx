// <copyright file="ApiError.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.D365Service
{
    using Newtonsoft.Json;

    public class ApiError
    {
        [JsonProperty("code", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Code { get; set; }

        [JsonProperty("message", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Message { get; set; }

        [JsonProperty("innerError", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public ApiError InnerError { get; set; }
    }
}