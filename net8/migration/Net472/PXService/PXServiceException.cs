// <copyright file="PXServiceException.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class PXServiceException : Exception
    {
        public PXServiceException(string message, string errorCode)
            : base(message)
        {
            this.ErrorCode = errorCode;
        }

        public PXServiceException(string message, string errorCode, Exception innerException)
            : base(message, innerException)
        {
            this.ErrorCode = errorCode;
        }

        public string ErrorCode { get; set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            base.GetObjectData(info, context);
        }
    }
}