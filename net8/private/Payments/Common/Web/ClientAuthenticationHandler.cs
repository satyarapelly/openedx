// <copyright file="ClientAuthenticationHandler.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

using Microsoft.AspNetCore.Http;
using Microsoft.Commerce.Payments.Common.Authorization;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Microsoft.Commerce.Payments.Common.Web
{
    public class ClientAuthenticationHandler
    {
        private readonly RequestDelegate _next;
        private readonly ClientAuthenticationHandlerHelper _helper;

        private static readonly IIdentity NullIdentity = new GenericIdentity("NULL", "NULL");
        private static readonly IIdentity GroupIdentity = new GenericIdentity("GroupIdentity", "GroupIdentity");

        public ClientAuthenticationHandler(RequestDelegate next, ClientAuthenticationHandlerHelper helper = null)
        {
            _next = next;
            _helper = helper ?? new ClientAuthenticationHandlerHelper();
        }

        public enum WindowsUsersToRolesMappingOption
        {
            /// <summary>
            /// Don't try to map the windows users to known roles
            /// In this case the system will rely on AD SGs
            /// </summary>
            DoNotMapUsersToRoles,

            /// <summary>
            /// Map only known users for the rest the system will rely on AD SGs
            /// </summary>
            MapOnlyKnownUsersToRoles,

            /// <summary>
            /// Use only internal roles
            /// </summary>
            MapAllUsersToRoles,
        }

        public async Task InvokeAsync(HttpContext context)
        {
            IEnumerable<IIdentity> identities = new[] { context.User?.Identity ?? NullIdentity };
            var clientCert = await context.Connection.GetClientCertificateAsync();
            bool certificateUser = false;

            if (clientCert != null)
            {
                string subject = CertificateHelper.NormalizeDistinguishedName(clientCert.Subject);
                string issuer = CertificateHelper.NormalizeDistinguishedName(clientCert.Issuer);
                string thumbprint = CertificateHelper.NormalizeThumbprint(clientCert.Thumbprint);
                string subjectName = clientCert.SubjectName.Name;

                identities = _helper.MapCertificateToUsers(new CertificateDescription { Issuer = issuer, Subject = subject, Thumbprint = thumbprint, SubjectName = subjectName });
                certificateUser = true;
            }

            ClaimsPrincipal principal = null;

            if (identities == null || !identities.Any())
            {
                if (_helper.IsAdministrator(null))
                {
                    principal = new ClaimsPrincipal(new AdminPrincipal(NullIdentity));
                }
            }
            else
            {
                List<string> allRoles = new List<string>();
                bool isAdmin = false;
                IIdentity contributingIdentity = null;

                foreach (IIdentity identity in identities)
                {
                    if (_helper.IsAdministrator(identity))
                    {
                        principal = new ClaimsPrincipal(new AdminPrincipal(NullIdentity));
                        isAdmin = true;
                        break;
                    }

                    if (certificateUser || _helper.WindowsUsersToRolesMapping != WindowsUsersToRolesMappingOption.DoNotMapUsersToRoles)
                    {
                        var roles = _helper.MapUserToRoles(identity);
                        if (roles != null)
                        {
                            allRoles.AddRange(roles);
                            contributingIdentity = contributingIdentity ?? identity;
                        }
                    }
                }

                if (!isAdmin && (allRoles.Any() || certificateUser || _helper.WindowsUsersToRolesMapping == WindowsUsersToRolesMappingOption.MapAllUsersToRoles))
                {
                    contributingIdentity ??= identities.Count() == 1 ? identities.First() : NullIdentity;
                    principal = new ClaimsPrincipal(new GenericPrincipal(contributingIdentity, allRoles.ToArray()));
                }
            }

            if (principal != null)
            {
                context.User = principal;
                context.Items[PaymentConstants.Web.Properties.CallerName] = principal.Identity?.Name;
            }

            await _next(context);
        }

        private class AdminPrincipal : GenericPrincipal
        {
            public AdminPrincipal(IIdentity identity) : base(identity, null) { }

            public override bool IsInRole(string role) => !string.IsNullOrEmpty(role);
        }
    }

    public enum WindowsUsersToRolesMappingOption
    {
        /// <summary>
        /// Don't try to map the windows users to known roles
        /// In this case the system will rely on AD SGs
        /// </summary>
        DoNotMapUsersToRoles,

        /// <summary>
        /// Map only known users for the rest the system will rely on AD SGs
        /// </summary>
        MapOnlyKnownUsersToRoles,

        /// <summary>
        /// Use only internal roles
        /// </summary>
        MapAllUsersToRoles,
    }
}
