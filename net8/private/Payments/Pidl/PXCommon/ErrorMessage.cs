// <copyright file="ErrorMessage.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PXCommon
{
    using Newtonsoft.Json;
    
    public class ErrorMessage
    {
        [JsonProperty(PropertyName = "ErrorCode")]
        public string ErrorCode { get; set; }

        [JsonProperty(PropertyName = "Retryable")]
        public bool Retryable { get; set; }

        [JsonProperty(PropertyName = "Message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "LocalizedMessage")]
        public string LocalizedMessage { get; set; }
    }
}