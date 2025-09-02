// <copyright file="CertificateDescription.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Authorization
{
    /// <summary>
    /// The certificate object for validation 
    /// </summary>
    public class CertificateDescription
    {
        /// <summary>
        /// Gets or sets the certificate subject
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the certificate issuer
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// Gets or sets the file name for installing the certificate
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the certificate subject name
        /// </summary>
        public string SubjectName { get; set; }

        /// <summary>
        /// Gets or sets the certificate thumbprint
        /// </summary>
        public string Thumbprint { get; set; }

        /// <summary>
        /// Gets or sets the certificate friendly name
        /// </summary>
        public string FriendlyName { get; set; }
    }
}
