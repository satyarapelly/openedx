// <copyright file="SecretStoreException.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tools.InstallCertificates
{
    using System;

    /// <summary>
    /// This exception class is thrown by Secret Store implementers when an operation fails
    /// </summary>
    [Serializable]
    public class SecretStoreException : Exception
    {
         /// <summary>
        /// Initializes a new instance of the <see cref="SecretStoreException"/> class.
        /// </summary>
        /// <param name="message">exception message</param>
        public SecretStoreException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecretStoreException"/> class.
        /// </summary>
        /// <param name="message">exception message </param>
        /// <param name="innerException">inner exception</param>
        public SecretStoreException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
