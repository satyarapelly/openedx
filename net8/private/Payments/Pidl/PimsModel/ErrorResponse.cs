// <copyright file="ErrorResponse.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PimsModel
{
    using Newtonsoft.Json;

    /// <summary>
    /// Error response object when we are returning 4xx and 5xx errors
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Gets or sets the error code 
        /// </summary>
        [JsonProperty(PropertyName = "ErrorCode")]
        public string ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets the error message
        /// </summary>
        [JsonProperty(PropertyName = "Message")]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the localized error message
        /// </summary>
        [JsonProperty(PropertyName = "LocalizedMessage")]
        public string LocalizedMessage { get; set; }
    }
}
