// <copyright file="UserInformation.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Linq;
    using Microsoft.Commerce.Payments.Management.CertificateVerificationCore;

    /// <summary>
    /// The certificate object for validation 
    /// </summary>
    public class UserInformation : Common.Authorization.CertificateDescription
    {
        public string Role { get; set; }

        public string AllowedAccounts { get; set; }

        public string AllowedAuthenticatedPathTemplate { get; set; }

        public string AllowedUnAuthenticatedPaths { get; set; }

        public string PartnerName { get; set; }

        /// <summary>
        /// Gets or sets AAD application Id used by AAD token authentication
        /// </summary>
        public string ApplicationId { get; set; }

        /// <summary>
        /// Gets or sets Certificate verification rule used by certificate authentication
        /// </summary>
        public VerifyBySubjectIssuerThumbprint CertificateVerificationRule { get; set; }

        public static bool IsAuthorized(Uri request, UserInformation partner)
        {
            if (partner.Role == GlobalConstants.ClientRoles.Admin)
            {
                return true;
            }

            // when role is test, we want to limit the path and user account it can access
            string absolutePath = request.AbsolutePath;
            if (!string.IsNullOrEmpty(partner.AllowedAccounts))
            {
                string[] allowedAccounts = partner.AllowedAccounts.Split(',');
                var account = allowedAccounts
                    .Where(x => absolutePath.StartsWith(string.Format(partner.AllowedAuthenticatedPathTemplate, x), StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault();
                if (!string.IsNullOrEmpty(account))
                {
                    return true;
                }
            }

            if (!string.IsNullOrEmpty(partner.AllowedUnAuthenticatedPaths))
            {
                string[] allowedUnAuthenticatedPaths = partner.AllowedUnAuthenticatedPaths.Split(',');
                var path = allowedUnAuthenticatedPaths
                    .Where(x => absolutePath.StartsWith(x, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault();
                if (!string.IsNullOrEmpty(path))
                {
                    return true;
                }
            }

            return false;
        }
    }
}