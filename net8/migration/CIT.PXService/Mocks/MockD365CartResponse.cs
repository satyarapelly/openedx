namespace CIT.PXService.Mocks
{
    using Newtonsoft.Json;
    using Microsoft.Commerce.Payments.PXService.Model.D365Service;
    using System.Collections.Generic;

    public class MockD365CartResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("carts")]
        public List<Cart> Carts { get; set; }
    }
}
