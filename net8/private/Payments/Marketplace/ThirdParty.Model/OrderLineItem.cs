using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace ThirdParty.Model
{
    public enum FulfilmentState
    {
        [EnumMember(Value = "NotApplicable")]
        NotApplicable = 0,

        [EnumMember(Value = "Pending")]
        Pending,

        [EnumMember(Value = "Completed")]
        Completed
    }

    public class OrderLineItem
    {
        [JsonProperty(PropertyName = "product")]
        public Product Product { get; set; }

        [JsonProperty(PropertyName = "productSku")]
        public ProductSku Sku { get; set; }

        //[DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C0}")]
        [JsonProperty(PropertyName = "price")]
        public double Price { get; set; }

        [JsonProperty(PropertyName = "fulfilmentState")]
        public FulfilmentState State { get; set; }
    }
}
