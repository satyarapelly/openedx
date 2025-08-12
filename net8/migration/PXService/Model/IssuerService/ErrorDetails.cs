// <copyright file="ErrorDetails.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PXService.Model.IssuerService
{
    using Newtonsoft.Json;

    public class ErrorDetails
    {
        [JsonProperty(PropertyName = "errorCode")]
        public string ErrorCode { get; set; }

        [JsonProperty(PropertyName = "errorTitle")]
        public string ErrorTitle { get; set; }

        [JsonProperty(PropertyName = "errorDetail")]
        public string ErrorDetail { get; set; }
    }
}