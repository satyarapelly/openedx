using Newtonsoft.Json;

namespace ThirdParty.Model
{
    public class Card
    {
        [JsonProperty(PropertyName = "number")]
        public string Number{ get; set; }

        [JsonProperty(PropertyName = "expMonth")]
        public int ExpMonth { get; set; }

        [JsonProperty(PropertyName = "expYear")]
        public int ExpYear { get; set; }

        [JsonProperty(PropertyName = "cvv")]
        public string CVV { get; set; }
    }
}
