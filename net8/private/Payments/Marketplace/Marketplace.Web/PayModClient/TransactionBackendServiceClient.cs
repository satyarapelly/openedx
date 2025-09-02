// <copyright file="TransactionServiceClient.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace Marketplace.Web.PayModClient
{
    using Microsoft.Extensions.Configuration;
    using System;

    public class TransactionBackendServiceClient : ServiceClient
    {
        private const string PayModINTClient = "dbceacd8-3c55-480d-b919-af59ff44042d";
        private const string PMETenantId = "975f013f-7f24-47e8-a7d3-abc4752bf346";
        private const string ApiVersionV1 = "2014-04-14";

        private TransactionBackendServiceClient(ServiceClientSettings settings)
            : base(settings)
        {
        }

        public static TransactionBackendServiceClient CreateInstance()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile(
                    "appsettings.json",
                    optional: false,
                    reloadOnChange: true)
                .AddUserSecrets<TransactionServiceClient>()
                .Build();

            var clientSettings = new ServiceClientSettings(ApiVersionV1)
            {
                ServiceEndpoint = "https://transactionbackend-int-WestUS2.azurewebsites.net/TransactionBackendService/",
                AzureADAuth = true,
                AzureADClientId = PayModINTClient,
                AzureADTenantId = PMETenantId,
                AzureADResourceUrl = PayModINTClient,
                AzureADSecret = config.GetSection("DevSecretKey").Value
            };

            return new TransactionBackendServiceClient(clientSettings);
        }
    }
}