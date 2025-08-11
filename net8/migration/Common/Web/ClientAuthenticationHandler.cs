// <copyright file="ClientAuthenticationHandler.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Web
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using Authorization;

    public class ClientAuthenticationHandler : DelegatingHandler
    {
        private static readonly IIdentity NullIdentity = new GenericIdentity("NULL", "NULL");

        private static readonly IIdentity GroupIdentity = new GenericIdentity("GroupIdentity", "GroupIdentity");

        private readonly ClientAuthenticationHandlerHelper helper;

        public ClientAuthenticationHandler(ClientAuthenticationHandlerHelper helper)
        {
            this.helper = helper;
            if (helper == null)
            {
                this.helper = new ClientAuthenticationHandlerHelper();
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

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            IEnumerable<IIdentity> identities = new[] { Thread.CurrentPrincipal.Identity };
            X509Certificate2 cert = request.GetClientCertificate();
            bool certificateUser = false;
            if (cert != null)
            {
                string subject = CertificateHelper.NormalizeDistinguishedName(cert.Subject);
                string issuer = CertificateHelper.NormalizeDistinguishedName(cert.Issuer);
                string thumbprint = CertificateHelper.NormalizeThumbprint(cert.Thumbprint);
                string subjectName = cert.SubjectName.Name;

                identities = this.helper.MapCertificateToUsers(new CertificateDescription { Issuer = issuer, Subject = subject, Thumbprint = thumbprint, SubjectName = subjectName });
                certificateUser = true;
            }

            if (identities == null || !identities.Any())
            {
                if (this.helper.IsAdministrator(null))
                {
                    Thread.CurrentPrincipal = new AdminPrincial(NullIdentity);
                }
            }
            else
            {
                List<string> allRoles = new List<string>();
                bool admin = false;
                IIdentity contributingIdentity = null;
                foreach (IIdentity identity in identities)
                {
                    if (this.helper.IsAdministrator(identity))
                    {
                        // Replace any existing identity by default admin identity
                        Thread.CurrentPrincipal = new AdminPrincial(NullIdentity);
                        admin = true;
                        break;
                    }

                    if (certificateUser || this.helper.WindowsUsersToRolesMapping != WindowsUsersToRolesMappingOption.DoNotMapUsersToRoles)
                    {
                        IEnumerable<string> roles = this.helper.MapUserToRoles(identity);

                        if (roles != null)
                        {
                            allRoles.AddRange(roles);
                            contributingIdentity = contributingIdentity == null ? identity : GroupIdentity;
                        }
                    }
                }

                if (!admin && (allRoles.Any() || certificateUser || this.helper.WindowsUsersToRolesMapping == WindowsUsersToRolesMappingOption.MapAllUsersToRoles))
                {
                    if (identities.Count() == 1)
                    {
                        contributingIdentity = identities.First();
                    }
                    else if (contributingIdentity == null)
                    {
                        contributingIdentity = NullIdentity;
                    }

                    GenericPrincipal principal = new GenericPrincipal(contributingIdentity, allRoles.ToArray());
                    Thread.CurrentPrincipal = principal;
                }
            }

            // This have to be done because of ASP.net limitation documented at: http://aspnetwebstack.codeplex.com/workitem/264
            if (HttpContext.Current != null)
            {
                HttpContext.Current.User = Thread.CurrentPrincipal;
            }

            if (Thread.CurrentPrincipal != null && Thread.CurrentPrincipal.Identity != null)
            {
                request.Properties[PaymentConstants.Web.Properties.CallerName] = Thread.CurrentPrincipal.Identity.Name;
            }

            return base.SendAsync(request, cancellationToken);
        }

        private class AdminPrincial : GenericPrincipal
        {
            public AdminPrincial(IIdentity identity)
                : base(identity, null)
            {
            }

            public override bool IsInRole(string role)
            {
                if (role != null)
                {
                    return true;
                }

                return false;
            }
        }
    }
}