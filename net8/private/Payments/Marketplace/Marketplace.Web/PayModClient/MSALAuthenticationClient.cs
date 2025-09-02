// <copyright file="MSALAuthenticationClient.cs" company="Microsoft">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace Marketplace.Web.PayModClient
{
    using System;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using Microsoft.Identity.Client;

    public class MSALAuthenticationClient
    {
        private const string DefaultScopeName = ".default";
        private readonly IConfidentialClientApplication aadTokenGetterClient;
        private readonly string resourceId;

        public MSALAuthenticationClient(string authority, string clientId, string resourceId, Lazy<X509Certificate2> certificate)
        {
            this.aadTokenGetterClient = ConfidentialClientApplicationBuilder.Create(clientId)
                                                                            .WithAuthority(new Uri(authority))
                                                                            .WithCertificate(certificate.Value)
                                                                            .Build();
            this.resourceId = resourceId;
        }

        public MSALAuthenticationClient(string authority, string clientId, string resourceId, string secret)
        {
            this.aadTokenGetterClient = ConfidentialClientApplicationBuilder.Create(clientId)
                                                                            .WithAuthority(new Uri(authority))
                                                                            .WithClientSecret(secret)
                                                                            .Build();
            this.resourceId = resourceId;
        }

        public async Task<string> GetTokenAsync(string resource, bool sendX5c)
        {
            var authenticationResult = await this.aadTokenGetterClient.AcquireTokenForClient(new[] { this.resourceId + "/" + DefaultScopeName })
                                            .WithSendX5C(true) // WithSendX5c is required here to support Subject Name and Issuer Authentication for aad cert autorotation
                                            .ExecuteAsync();
            return authenticationResult.CreateAuthorizationHeader();
        }

        public async Task<string> GetDefaultTokenAsync(bool sendX5c = false)
        {
            return await this.GetTokenAsync(this.resourceId, sendX5c);
        }
    }
}
