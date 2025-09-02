using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace ThirdParty.Model
{
    public enum PIType
    {
        [EnumMember(Value = "Card")]
        Card,

        [EnumMember(Value = "Invoice")]
        Invoice
    };

    public class PaymentInstrument : BaseItem
    {
        [JsonProperty(PropertyName = "Type")]
        public PIType Type { get; set; }

        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName{ get; set; }

        [JsonProperty(PropertyName = "displayImage")]
        public string DisplayImage { get; set; }

        [JsonProperty(PropertyName = "details")]
        public object Details { get; set; }

    }
}
