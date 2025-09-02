// <copyright file="CertificateException.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Authorization
{
    using System;

    /// <summary>
    /// This exception class is thrown by certificate auth.
    /// </summary>
    [Serializable]
    public class CertificateException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateException"/> class.
        /// </summary>
        /// <param name="message">exception message</param>
        public CertificateException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateException"/> class.
        /// </summary>
        /// <param name="message">exception message </param>
        /// <param name="innerException">inner exception</param>
        public CertificateException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}