// <copyright file="ServiceClientSettings.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace Marketplace.Web.PayModClient
{
    using System;
    using System.Security.Cryptography.X509Certificates;

    public class ServiceClientSettings
    {
        public ServiceClientSettings(string apiVersion)
            : this()
        {
            this.ServiceApiVersion = apiVersion;
        }

        public ServiceClientSettings()
        {
            this.MaximumRequestRetryCount = 5;
            this.RetryDelay = TimeSpan.FromSeconds(2);
        }

        public string ServiceEndpoint { get; set; }

        public int MaximumRequestRetryCount { get; set; }

        public TimeSpan RetryDelay { get; set; }

        public string ServiceApiVersion { get; set; }

        public X509Certificate2 ClientCertificate { get; set; }

        public bool AzureADAuth { get; set; }

        public string AzureADTenantId { get; set; }

        public string AzureADClientId { get; set; }

        public string AzureADResourceUrl { get; set; }

        public string AzureADSecret { get; set; }
    }
}