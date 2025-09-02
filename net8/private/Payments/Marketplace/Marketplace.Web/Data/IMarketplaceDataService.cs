using System.Collections.Generic;
using System.Threading.Tasks;
using ThirdParty.Model;

namespace Marketplace.Data
{
    public interface IMarketplaceDataService
    {
        Task InitializeAsync();
        Task<IEnumerable<CatalogItem>> ListCatalogItemsAsync();
        Task<CatalogItem> GetCatalogItemByIdAsync(string id);
        Task<Order> GetOrderInCartAsync();
        Task<Order> GetOrderByIdAsync(string id);
        Task<IEnumerable<Order>> ListOrdersAsync();
        Task<Order> AddOrderAsync(Order order);
        Task<Order> UpdateOrderAsync(Order order);
        Task<Customer> GetCustomerByIdAsync(string Id);
        Task<Customer> UpdateCustomerAsync(Customer customer);
        Task<IEnumerable<Customer>> ListCustomersAsync();
        Task DeleteOrderAsync(string id);




    }
}
