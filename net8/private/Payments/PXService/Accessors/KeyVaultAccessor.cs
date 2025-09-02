// <copyright file="KeyVaultAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using global::Azure.Identity;
    using global::Azure.Security.KeyVault.Secrets;

    public class KeyVaultAccessor : IKeyVaultAccessor, IDisposable
    {
        private readonly SecretClient secretClient;
        private readonly string vaultName;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyVaultAccessor" /> class using SecretClient class with ClientCertificateCredentials
        /// using the passed in parameters
        /// </summary>
        /// <param name="vaultName">Key vault name</param>
        /// <param name="tenantId">Tenant Id</param>
        /// <param name="clientId">client app id</param>
        /// <param name="authCert">secret app key</param>
        public KeyVaultAccessor(string vaultName, string tenantId, string clientId, X509Certificate2 authCert)
        {
            this.vaultName = vaultName;
            var uri = new Uri(string.Format("https://{0}.vault.azure.net", this.vaultName));
            var credentials = new ClientCertificateCredential(tenantId, clientId, authCert, new ClientCertificateCredentialOptions { SendCertificateChain = true });

            this.secretClient = new SecretClient(uri, credentials);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyVaultAccessor" /> class using SecretClient class with ManagedIdentityCredentials
        /// using the passed in parameters
        /// </summary>
        /// <param name="vaultName">Key vault name</param>
        /// <param name="clientId">client app id</param>
        public KeyVaultAccessor(string vaultName, string clientId)
        {
            this.vaultName = vaultName;
            var uri = new Uri(string.Format("https://{0}.vault.azure.net", this.vaultName));
            var credentials = new ManagedIdentityCredential(clientId);

            this.secretClient = new SecretClient(uri, credentials);
        }

        public async Task<string> GetSecretAsync(string name)
        {
            KeyVaultSecret secret = await this.secretClient.GetSecretAsync(name);
            return secret.Value;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}