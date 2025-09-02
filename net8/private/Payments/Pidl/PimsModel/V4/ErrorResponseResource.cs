// <copyright file="ErrorResponseResource.cs" company="Microsoft">Copyright (c) Microsoft 2019. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    /// <summary>
    /// Error response object when we are returning 4xx and 5xx errors
    /// </summary>
    public class ErrorResponseResource
    {
        /// <summary>
        /// Gets or sets the error code 
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets the error message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the localized error message
        /// </summary>
        public string LocalizedMessage { get; set; }
    }
}
