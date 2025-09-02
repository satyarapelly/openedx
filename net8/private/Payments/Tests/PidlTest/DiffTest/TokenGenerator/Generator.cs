// <copyright file="Generator.cs" company="Microsoft">Copyright (c) Microsoft 2016. All rights reserved.</copyright>

namespace PidlTest
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Identity.Client;

    public static class Generator
    {
        private static readonly string ClientId = "YOUR_CLIENT_ID";
        private static readonly string TenantId = "YOUR_TENANT_ID";
        private static readonly string Authority = $"https://login.microsoftonline.com/{TenantId}";
        private static readonly string[] Scopes = new[] { "user.read" };

        public static async Task<string> GenerateAsync(string environment, string userName, string password)
        {
            var app = PublicClientApplicationBuilder.Create(ClientId)
                .WithAuthority(Authority)
                .WithRedirectUri("http://localhost")
                .Build();

            var securePassword = new System.Security.SecureString();
            foreach (char c in password)
            {
                securePassword.AppendChar(c);
            }

            try
            {
                var result = await app.AcquireTokenByUsernamePassword(Scopes, userName, securePassword)
                                      .ExecuteAsync();

                return result.AccessToken;
            }
            catch (MsalUiRequiredException ex)
            {
                throw new InvalidOperationException("Interactive login required or MFA enforced. Use interactive flow.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Token acquisition failed.", ex);
            }
        }
    }
}

