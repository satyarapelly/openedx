// <copyright file="PIDLException.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory
{
    using System;

    public class PIDLException : Exception
    {
        public PIDLException(string message, string errorCode)
            : base(message)
        {
            // Store custom error code in base.Data dictionary
            this.Data[GlobalConstants.ExceptionDataKeys.PIDLErrorCode] = errorCode;
        }

        // Hides base virtual Data to make it non-virtual in this class
        public new System.Collections.IDictionary Data => base.Data;
    }
}
