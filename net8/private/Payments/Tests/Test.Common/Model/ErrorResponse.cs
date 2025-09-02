// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace Tests.Common.Model
{
    // Currently, clients of PXService rely on their own copy of the model (instead of relying on PXService's model).
    // So, these tests should have their own copy as well to detect any changes in the model.  This class is a copy
    // of Microsoft.Commerce.Payments.Common.Web.ErrorResponse
    public class ErrorResponse
    {
        /// <summary>
        /// Gets or sets the error code 
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets the error message
        /// </summary>
        public string Message { get; set; }
    }
}
