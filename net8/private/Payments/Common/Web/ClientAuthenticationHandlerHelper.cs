// <copyright file="ClientAuthenticationHandlerHelper.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Web
{
    using System;
    using System.Collections.Generic;
    using System.Security.Principal;
    using Payments.Common.Authorization;

    public class ClientAuthenticationHandlerHelper
    {
        private readonly Func<CertificateDescription, IEnumerable<IIdentity>> mapCertificateToUsers;
        private readonly Func<IIdentity, IEnumerable<string>> mapUserToRoles;

        public ClientAuthenticationHandlerHelper() : this(ClientAuthenticationHandler.WindowsUsersToRolesMappingOption.DoNotMapUsersToRoles, null, null)
        {
        }

        public ClientAuthenticationHandlerHelper(
            ClientAuthenticationHandler.WindowsUsersToRolesMappingOption windowsUsersToRolesMapping,
            Func<CertificateDescription, IIdentity[]> mapCertificateToUsers,
            Func<IIdentity, string[]> mapUserToRoles)
        {
            this.WindowsUsersToRolesMapping = windowsUsersToRolesMapping;
            this.mapCertificateToUsers = mapCertificateToUsers;
            this.mapUserToRoles = mapUserToRoles;
        }

        public ClientAuthenticationHandler.WindowsUsersToRolesMappingOption WindowsUsersToRolesMapping { get; protected set; }

        public virtual IEnumerable<string> MapUserToRoles(IIdentity identity)
        {
            if (this.mapUserToRoles != null)
            {
                return this.mapUserToRoles(identity);
            }

            return null;
        }

        public virtual IEnumerable<IIdentity> MapCertificateToUsers(CertificateDescription certificateDescription)
        {
            if (this.mapCertificateToUsers != null)
            {
                return this.mapCertificateToUsers(certificateDescription);
            }

            return null;
        }

        public virtual bool IsAdministrator(IIdentity identity)
        {
            return false;
        }
    }
}