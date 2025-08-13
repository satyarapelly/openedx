// <copyright file ="MiseTokenValidationResult.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXCommon
{
    using System;
    using System.Security.Claims;

    public class MiseTokenValidationResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether validation is successful or not.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets errorCode if validation fails.
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets the message indicates the validation fails or succeeds.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the cloud instance where the token is validated successfully.
        /// </summary>
        public string CloudInstance { get; set; }

        /// <summary>
        /// Gets or sets the latency.
        /// </summary>
        public int Latency { get; set; }

        /// <summary>
        /// Gets or sets application id
        /// </summary>
        public string ApplicationId { get; set; }

        /// <summary>
        /// Gets or sets token principal
        /// </summary>
        public ClaimsPrincipal TokenPrincipal { get; set; }

        /// <summary>
        /// Gets or sets the exception when validation failed.
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Set Success and TokenPrincipal if the result is successful.
        /// </summary>
        /// <param name="tokenPrincipal">The token principal.</param>
        /// <param name="cloudInstance">The cloud instance where the token is validated successfully.</param>
        /// <param name="message">The message indicates why the authentication succeed.</param>
        public void Succeed(ClaimsPrincipal tokenPrincipal, string cloudInstance = null, string message = null)
        {
            this.Success = true;
            this.CloudInstance = cloudInstance ?? string.Empty;
            this.TokenPrincipal = tokenPrincipal;
            this.Message = message ?? "MISE + SAL Token validation is successful.";
        }

        /// <summary>
        /// Set Success, ErrorCode and ErrorDetails if the result is failed.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="message">The message indicates why the authentication failed.</param>
        /// <param name="ex">The exception when token authentication failed</param>
        public void Failed(string errorCode, string message, Exception ex = null)
        {
            this.Success = false;
            this.ErrorCode = errorCode;
            this.Message = message;
            this.Exception = ex;
        }
    }
}
