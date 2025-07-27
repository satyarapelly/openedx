// <copyright file="ExpressCheckoutException.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// This exception is thrown when the PimsSession has status other than Success or InProgress
    /// </summary>
    [Serializable]
    public class ExpressCheckoutException : Exception
    {
        private const string ServiceErrorCodeName = "Error";

        public ExpressCheckoutException(ServiceErrorResponse error)
            : base(error?.Message)
        {
            this.Error = error;
            this.ErrorCode = error.ErrorCode;
        }

        /// <summary>
        /// Gets or sets the error 
        /// </summary>    
        public ServiceErrorResponse Error { get; set; }

        /// <summary>
        /// Gets or sets the error code 
        /// </summary>        
        public string ErrorCode { get; set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            base.GetObjectData(info, context);
            if (this.Error != null)
            {
                info.AddValue(
                    ServiceErrorCodeName,
                    string.Format("ErrorCode: {0} Message: {1} Source: {2}", this.Error.ErrorCode, this.Error.Message, this.Error.Source ?? string.Empty));
            }
            else
            {
                info.AddValue(ServiceErrorCodeName, string.Empty);
            }
        }
    }
}