//---------------------------------------------------------------------
// <copyright>
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Azure.Core.Diagnostics;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace COT.PXService
{
    /// <summary>
    /// Helper class to retrieve secrets from Azure Key Vault
    /// </summary>
    /// <remarks>
    /// TODO: add some (optional) caching to prevent unneccessary calls to AKV
    /// </remarks>
    public class KeyVaultAccessor : IDisposable
    {
        private readonly SecretClient secretClient;
        private readonly string vaultName;

        /// <summary>
        /// Initializes a new instance of the SecretClient class with ClientSecretCredentials
        /// using the passed in parameters
        /// </summary>
        /// <param name="vaultName">Key vault name</param>
        /// <param name="tenantId">Tenant Id</param>
        /// <param name="appId">client app id</param>
        /// <param name="secret">secret app key</param>
        public KeyVaultAccessor(string vaultName, string tenantId, string appId, string secret)
        {
            this.vaultName = vaultName;
            var uri = new Uri(string.Format("https://{0}.vault.azure.net", this.vaultName));

            // listener and options will enable logging during COT run in release pipeline
            // for debugging purposes. Once these are working properly we can remove the listener and options.
            AzureEventSourceListener listener = AzureEventSourceListener.CreateConsoleLogger();
            ClientSecretCredentialOptions options = new ClientSecretCredentialOptions()
            {
                Diagnostics =
                {
                    IsAccountIdentifierLoggingEnabled = true,
                    IsLoggingContentEnabled = true,
                    IsLoggingEnabled = true,
                    IsTelemetryEnabled = true,
                }
            };

            var credentials = new ClientSecretCredential(tenantId, appId, secret, options);

            this.secretClient = new SecretClient(uri, credentials);
        }

        /// <summary>
        /// Retrieves a certificate stored in AKV.
        /// </summary>
        /// <param name="name">name of certificate</param>
        /// <returns>X509 certificate</returns>
        public async Task<X509Certificate2> GetCertificateAsync(string name)
        {
            KeyVaultSecret secret = await this.secretClient.GetSecretAsync(name);
            byte[] privateKeyBytes = Convert.FromBase64String(secret.Value);
            var cert = new X509Certificate2(privateKeyBytes, (string)null, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);

            return cert;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}