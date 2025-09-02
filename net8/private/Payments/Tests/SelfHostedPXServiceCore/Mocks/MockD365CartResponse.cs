namespace SelfHostedPXServiceCore.Mocks
{
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PXService.Model.D365Service;
    using Newtonsoft.Json;

    public class MockD365CartResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("carts")]
        public List<Cart> Carts { get; set; }
    }
}
