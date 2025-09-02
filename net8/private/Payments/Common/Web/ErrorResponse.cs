// <copyright file="ErrorResponse.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Web
{
    /// <summary>
    /// Error response object when we are returning 4xx and 5xx errors
    /// </summary>
    public class ErrorResponse
    {
        public ErrorResponse()
        {
        }

        public ErrorResponse(string errorCode, string message, string localizedMessage)
        {
            this.ErrorCode = errorCode;
            this.Message = message;
            this.LocalizedMessage = localizedMessage;
        }

        public ErrorResponse(string errorCode, string message)
            : this(errorCode, message, null)
        {
        }

        public ErrorResponse(ErrorCode errorCode, string message, string localizedMessage)
            : this(errorCode.ToString(), message, localizedMessage)
        {
        }

        public ErrorResponse(ErrorCode errorCode, string message)
            : this(errorCode.ToString(), message)
        {
        }

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
