using Newtonsoft.Json;

namespace ThirdParty.Model
{
    public enum SellerRepresentativeType
    {
        Owner,
        Proprietor,
        Employee
    }

    public class SellerRepresentative
    {
        [JsonProperty(PropertyName = "personId")]
        public string personId { get; set; }

        [JsonProperty(PropertyName = "representativeType")]
        public string representativeType { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string title { get; set; }

        [JsonProperty(PropertyName = "percentOwnership")]
        public decimal PercentOwnership { get; set; }
    }
}
