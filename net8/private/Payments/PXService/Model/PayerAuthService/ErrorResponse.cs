// <copyright file="ErrorResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PayerAuthService
{
    using Newtonsoft.Json;

    public class ErrorResponse
    {
        [JsonProperty(PropertyName = "error_code")]
        public string ErrorCode { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "error_source")]
        public string ErrorSource { get; set; }
    }
}