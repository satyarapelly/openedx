// <copyright file="FailedOperationException.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common
{
    using System;

    /// <summary>
    /// This exception class is thrown by providers when an operation fails and it is not sure of the result of the operation
    /// </summary>
    [Serializable]
    public class FailedOperationException : Exception
    {   
        /// <summary>
        /// Initializes a new instance of the <see cref="FailedOperationException"/> class.
        /// </summary>
        /// <param name="message">exception message</param>
        public FailedOperationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FailedOperationException"/> class.
        /// </summary>
        /// <param name="message">exception message </param>
        /// <param name="innerException">inner exception</param>
        public FailedOperationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
