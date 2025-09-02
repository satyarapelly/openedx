using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using ThirdParty.Model;

namespace Marketplace.Data
{
    public class MarketplaceCosmosDb : IMarketplaceDataService
    {
        // The Cosmos client instance
        private CosmosClient cosmosClient;

        // The database we will create
        private Database db;

        // The container we will create.
        private Container catalog;
        private Container customers;
        private Container orders;

        public MarketplaceCosmosDb(IConfiguration config)
        {
            // Create a new instance of the Cosmos Client
            var cs = config.GetSection("MarketplaceDb").Value;
            cosmosClient = new CosmosClient(
                connectionString: cs
            );
        }

        public async Task InitializeAsync()
        {
            // Create a new database
            db = await cosmosClient.CreateDatabaseIfNotExistsAsync(
                id: Constants.MarketplaceDbName);

            Console.WriteLine("Created database: {0}\n", db.Id);

            // Create a new container
            catalog = await db.CreateContainerIfNotExistsAsync(
                Constants.ContainerNames.Catalog,
                "/bucketId");

            customers = await db.CreateContainerIfNotExistsAsync(
                Constants.ContainerNames.Customers,
                "/bucketId");

            orders = await db.CreateContainerIfNotExistsAsync(
                Constants.ContainerNames.Orders, 
                "/bucketId");
        }

        public async Task<IEnumerable<CatalogItem>> ListCatalogItemsAsync()
        {
            var sqlQueryText = "SELECT * FROM c";

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<CatalogItem> queryResultSetIterator = catalog.GetItemQueryIterator<CatalogItem>(
                queryDefinition);

            List<CatalogItem> catalogItems = new List<CatalogItem>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<CatalogItem> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (CatalogItem c in currentResultSet)
                {
                    catalogItems.Add(c);
                }
            }

            return catalogItems;
        }

        public async Task<CatalogItem> GetCatalogItemByIdAsync(string id)
        {
            var sqlQueryText = string.Format("SELECT * FROM c WHERE c.id = '{0}'", id);

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<CatalogItem> queryResultSetIterator = catalog.GetItemQueryIterator<CatalogItem>(
                queryDefinition);

            FeedResponse<CatalogItem> currentResultSet = await queryResultSetIterator.ReadNextAsync();
            return currentResultSet.FirstOrDefault();
        }

        public async Task<IEnumerable<Order>> ListOrdersAsync()
        {
            var sqlQueryText = string.Format("SELECT * FROM o");

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            var queryIterator = orders.GetItemQueryIterator<Order>(
                queryDefinition);

            var resultSet = await queryIterator.ReadNextAsync();
            return resultSet.ToList<Order>();
        }

        public async Task<Order> GetOrderInCartAsync()
        {
            QueryDefinition queryDefinition = new QueryDefinition(
                "SELECT * FROM o WHERE o.state = 'InCart'");
            var queryIterator = this.orders.GetItemQueryIterator<Order>(
                queryDefinition);

            var resultSet = await queryIterator.ReadNextAsync();
            var inCartOrders = resultSet.ToList<Order>();

            return inCartOrders.OrderByDescending(o => o.CartData).FirstOrDefault();
        }

        public async Task<Order> GetOrderByIdAsync(string id)
        {
            ItemResponse<Order> response = await orders.ReadItemAsync<Order>(
                id: id,
                partitionKey: new PartitionKey(id.Substring(0, 2)));

            return response.Resource;
        }

        public async Task DeleteOrderAsync(string id)
        {
            await orders.DeleteItemAsync<Order>(
                id: id,
                partitionKey: new PartitionKey(id.Substring(0, 2)));
        }

        public async Task<Order> AddOrderAsync(Order order)
        {
            ItemResponse<Order> response = await orders.CreateItemAsync<Order>(
                order,
                new PartitionKey(order.BucketId));

            return response.Resource;
        }

        public async Task<Order> UpdateOrderAsync(Order order)
        {
            ItemResponse<Order> response = await orders.ReplaceItemAsync<Order>(
                item: order,
                id: order.Id,
                partitionKey: new PartitionKey(order.BucketId),
                requestOptions: new ItemRequestOptions()
                {
                    IfMatchEtag = order.Etag
                });

            return response.Resource;
        }

        public async Task<Customer> GetCustomerByIdAsync(string id)
        {
            var sqlQueryText = string.Format("SELECT * FROM c WHERE c.id = '{0}'", id);

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            var queryIterator = customers.GetItemQueryIterator<Customer>(
                queryDefinition);

            var resultSet = await queryIterator.ReadNextAsync();
            return resultSet.FirstOrDefault();
        }

        public async Task<Customer> UpdateCustomerAsync(Customer customer)
        {
            ItemResponse<Customer> response = await customers.ReplaceItemAsync<Customer>(
                item: customer,
                id: customer.Id,
                partitionKey: new PartitionKey(customer.BucketId),
                requestOptions: new ItemRequestOptions()
                {
                    IfMatchEtag = customer.Etag
                });

            return response.Resource;
        }

        public async Task<IEnumerable<Customer>> ListCustomersAsync()
        {
            var sqlQueryText = string.Format("SELECT * FROM o");

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            var queryIterator = customers.GetItemQueryIterator<Customer>(
                queryDefinition);

            var resultSet = await queryIterator.ReadNextAsync();
            return resultSet.ToList<Customer>();
        }
    }
}
