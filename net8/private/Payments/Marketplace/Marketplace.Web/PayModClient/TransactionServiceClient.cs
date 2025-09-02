// <copyright file="TransactionServiceClient.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace Marketplace.Web.PayModClient
{
    using Microsoft.Extensions.Configuration;
    using System;

    public class TransactionServiceClient : ServiceClient
    {
        private const string PayModINTClient = "dbceacd8-3c55-480d-b919-af59ff44042d";
        private const string PMETenantId = "975f013f-7f24-47e8-a7d3-abc4752bf346";
        private const string ApiVersionV1 = "2018-05-07";

        private TransactionServiceClient(ServiceClientSettings settings)
            : base(settings)
        {
        }

        public static TransactionServiceClient CreateInstance()
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
                ServiceEndpoint = "https://paymentstransactionservice.cp.microsoft-int.com/TransactionService/",
                AzureADAuth = true,
                AzureADClientId = PayModINTClient,
                AzureADTenantId = PMETenantId,
                AzureADResourceUrl = PayModINTClient,
                AzureADSecret = config.GetSection("DevSecretKey").Value
            };

            return new TransactionServiceClient(clientSettings);
        }
    }
}