namespace CIT.PXService.Mocks
{
    using Newtonsoft.Json;
    using Microsoft.Commerce.Payments.PXService.Model.D365Service;

    public class MockD365OrdersResponse
    {
        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("pagedResponse")]
        public PagedResponse<Order> PagedResponse { get; set; }
    }
}
