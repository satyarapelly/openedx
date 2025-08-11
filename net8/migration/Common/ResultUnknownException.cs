// <copyright file="ResultUnknownException.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common
{
    using System;

    /// <summary>
    /// This exception is thrown by the providers when an operation's outcome is unknown because of a failure.
    /// </summary>
    [Serializable]
    public class ResultUnknownException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResultUnknownException"/> class.
        /// </summary>
        /// <param name="message">exception message</param>
        public ResultUnknownException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultUnknownException"/> class.
        /// </summary>
        /// <param name="message">exception message </param>
        /// <param name="innerException">inner exception</param>
        public ResultUnknownException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
