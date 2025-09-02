using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ThirdParty.Model
{
    public enum PaymentType 
    {
        [EnumMember(Value = "Immediate")]
        Immediate = 0,

        [EnumMember(Value = "Deferred")]
        Deferred = 1,

        [EnumMember(Value = "Recurring")]
        Recurring = 2
    }

    public class ProductSku : BaseItem
    {
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "image")]
        public string Image { get; set; }

        [JsonProperty(PropertyName = "paymentType")]
        public PaymentType PaymentType { get; set; }

        //[DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C0}")]
        [JsonProperty(PropertyName = "price")]
        public double Price { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
