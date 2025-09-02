// <copyright file="ShortURLDBAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.ShortURLDB
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using global::Azure.Identity;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.Model.ShortURLDB;

    public class ShortURLDBAccessor : IShortURLDBAccessor
    {
        private Container codeContainer;
        private string charList = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private string dbName = "ShortUrlDB";
        private string containerName = "Codes";
        private string endpointDomainName;

        public ShortURLDBAccessor(string account, string endpointDomainName)
        {
            this.endpointDomainName = endpointDomainName;

            CosmosClientOptions cosmosClientOptions = new CosmosClientOptions()
            {
                ConnectionMode = ConnectionMode.Gateway,
                ApplicationName = "PXService"
            };

            CosmosClient client;
            if (account.Equals("https://localhost:8081"))
            {
                cosmosClientOptions.HttpClientFactory = () =>
                {
                    HttpMessageHandler httpMessageHandler = new HttpClientHandler()
                    {
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };

                    return new HttpClient(httpMessageHandler);
                };

                // Replace placeholder_key with the Primary Key found in your Cosmos DB Emulator
                client = new CosmosClient(account, "placeholder_key", cosmosClientOptions);
            }
            else
            {
                // Can possibly use whatever MI is assigned to the app service
                DefaultAzureCredential credential = new DefaultAzureCredential();
                client = new CosmosClient(account, credential, cosmosClientOptions);
            }

            this.codeContainer = client.GetContainer(this.dbName, this.containerName);

            this.Initialize(client);
        }

        public async Task<CreateResponse> Create(CreateRequest request)
        {
            string bigURL = request.URL;

            if (!IsValidUri(bigURL))
            {
                Exception ex = new ArgumentException("Invalid URL format.");

                // hack commenting as part of merge conflicts.
                SllWebLogger.DatabaseActionResult(
                    false,
                    this.dbName,
                    this.containerName,
                    Constants.ShortURLDBAction.Create,
                     ex);

                throw ex;
            }

            string code = string.Empty;
            CodeEntry codeEntry = null;
            int attempts = 0;
            int mins = (int)((request.TTLMinutes == null) ? PXCommon.Constants.ShortURL.TTLMinutes : request.TTLMinutes);
            var expireTime = DateTime.Now.AddMinutes(mins);

            try
            {
                while (attempts < 12)
                {
                    code = this.GenerateCode(Constants.ShortURL.CodeLength);
                    attempts++;
                    codeEntry = new CodeEntry
                    {
                        Code = code,
                        URL = bigURL,
                        Hits = 0,
                        RequestedTTLMinutes = mins,
                        CreationTime = DateTime.Now,
                        ExpireTime = expireTime,
                        MarkedForDeletion = false,
                        TTL = Constants.ShortURL.PurgeIntervalDays * 24 * 60 * 60
                    };
                    if (await this.CheckAndAddCodeEntryAsync(codeEntry))
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                SllWebLogger.DatabaseActionResult(
                    false,
                    this.dbName,
                    this.containerName,
                    Constants.ShortURLDBAction.Create,
                     ex);
            }

            if (attempts >= 12 || codeEntry == null)
            {
                Exception ex = new Exception("Reached maximum db retries.");
                SllWebLogger.DatabaseActionResult(
                    false,
                    this.dbName,
                    this.containerName,
                    Constants.ShortURLDBAction.Create,
                     ex);

                throw ex;
            }

            UriBuilder shortUrl = new UriBuilder("https", this.endpointDomainName, -1, code);
            SllWebLogger.DatabaseActionResult(
                true,
                this.dbName,
                this.containerName,
                Constants.ShortURLDBAction.Create);
            return new CreateResponse() { Uri = shortUrl.Uri, ExpirationTime = expireTime, Code = code };
        }

        public async Task<bool> CheckAndAddCodeEntryAsync(CodeEntry codeEntry)
        {
            try
            {
                await this.codeContainer.CreateItemAsync<CodeEntry>(codeEntry, new PartitionKey(codeEntry.Code));
                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                return false;
            }
        }

        public async Task<CodeEntry> GetCodeEntryAsync(string code)
        {
            try
            {
                ItemResponse<CodeEntry> response = await this.codeContainer.ReadItemAsync<CodeEntry>(code, new PartitionKey(code));
                SllWebLogger.DatabaseActionResult(
                    true,
                    this.dbName,
                    this.containerName,
                    Constants.ShortURLDBAction.Read);
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                SllWebLogger.DatabaseActionResult(
                    false,
                    this.dbName,
                    this.containerName,
                    Constants.ShortURLDBAction.Read,
                     ex);

                return null;
            }
        }

        public async Task UpdateCodeEntryAsync(CodeEntry codeEntry)
        {
            try 
            { 
                await this.codeContainer.UpsertItemAsync<CodeEntry>(codeEntry, new PartitionKey(codeEntry.Code));
                SllWebLogger.DatabaseActionResult(
                    true,
                    this.dbName,
                    this.containerName,
                    Constants.ShortURLDBAction.Update);
            }
            catch (Exception ex)
            {
                SllWebLogger.DatabaseActionResult(
                    false,
                    this.dbName,
                    this.containerName,
                    Constants.ShortURLDBAction.Update,
                     ex);
            }
        }

        private static bool IsValidUri(string uri)
        {
            return Uri.TryCreate(uri, UriKind.Absolute, out Uri validatedUri);
        }

        private async void Initialize(CosmosClient client)
        {
            DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(this.dbName);

            ContainerProperties props = database.Database.DefineContainer(name: this.containerName, partitionKeyPath: "/id")
                .WithUniqueKey().Path("/code").Path("/tenantId").Attach().Build();

            props.DefaultTimeToLive = 30 * 24 * 60 * 60;  // (30 days by default)
            await database.Database.CreateContainerIfNotExistsAsync(props);
        }

        private string GenerateCode(int length = 6)
        {
            int codeLength = length;
            char[] clistarr = this.charList.ToCharArray();

            Random rnd = new Random();
            long number = (long)(rnd.NextDouble() * (long)Math.Pow(clistarr.Length, codeLength));
            string result = string.Empty;
            int i = codeLength;
            while (i != 0)
            {
                result = clistarr[number % clistarr.Length] + result;
                number /= clistarr.Length;
                i--;
            }

            return result;
        }
    }
}