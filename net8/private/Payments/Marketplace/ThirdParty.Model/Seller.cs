using Newtonsoft.Json;

namespace ThirdParty.Model
{
    public class Seller : BaseItem
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "logo")]
        public string Logo { get; set; }

        [JsonProperty(PropertyName = "accountId")]
        public string AccountId { get; set; }

        [JsonProperty(PropertyName = "account")]
        public object Account { get; set; }
    }
}
