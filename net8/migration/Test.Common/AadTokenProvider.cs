// <copyright file="AadTokenProvider.cs" company="Microsoft">Copyright (c) Microsoft 2017. All rights reserved.</copyright>

namespace Tests.Common
{
    using System;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using Microsoft.Identity.Client;
    using Microsoft.Identity.Client.AppConfig;

    public class AadTokenProvider
    {
        private readonly string resource;
        private readonly IConfidentialClientApplication confidentialClientApp;
        private readonly IManagedIdentityApplication managedIdentityApplication;

        public AadTokenProvider(
            string authority, 
            string clientAppId,
            string resource, 
            X509Certificate2 clientCert)
        {
            this.resource = resource;
            this.confidentialClientApp = ConfidentialClientApplicationBuilder
                .Create(clientAppId)
                .WithAuthority(new Uri(authority))
                .WithCertificate(clientCert)
                .Build();
        }

        public AadTokenProvider(
            string clientAppId,
            string resource)
        {
            this.resource = resource;
            this.managedIdentityApplication = ManagedIdentityApplicationBuilder
                .Create(ManagedIdentityId.WithUserAssignedClientId(clientAppId))
                .Build();
        }

        public async Task<string> AcquireToken()
        {
            AuthenticationResult authResult;

            if (this.managedIdentityApplication != null)
            {
                authResult = await this.managedIdentityApplication
                                .AcquireTokenForManagedIdentity(this.resource + "/.default")
                                .ExecuteAsync();
            }
            else
            {
                authResult = await this.confidentialClientApp
                                .AcquireTokenForClient(new[] { this.resource + "/.default" })
                                .WithSendX5C(true)
                                .ExecuteAsync();
            }

            return authResult.CreateAuthorizationHeader();
        }
    }
}
