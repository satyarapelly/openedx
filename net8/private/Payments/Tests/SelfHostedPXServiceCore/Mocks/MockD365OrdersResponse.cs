namespace SelfHostedPXServiceCore.Mocks
{
    using Microsoft.Commerce.Payments.PXService.Model.D365Service;
    using Newtonsoft.Json;

    public class MockD365OrdersResponse
    {
        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("pagedResponse")]
        public PagedResponse<Order> PagedResponse { get; set; }
    }
}
