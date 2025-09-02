using Newtonsoft.Json;
using System.Collections.Generic;

namespace ThirdParty.Model
{
    public class Customer : BaseItem
    {
        public Customer()
        {
            this.PaymentInstruments = new List<PaymentInstrument>();
        }

        [JsonProperty(PropertyName = "firstName")]
        public string FirstName{ get; set; }

        [JsonProperty(PropertyName = "lastName")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "phone")]
        public string Phone { get; set; }

        [JsonProperty(PropertyName = "address")]
        public Address Address { get; set; }

        [JsonProperty(PropertyName = "processorId")]
        public string ProcessorId { get; set; }

        [JsonProperty(PropertyName = "processorObject")]
        public object ProcessorObject { get; set; }

        [JsonProperty(PropertyName = "paymentInstruments")]
        public List<PaymentInstrument> PaymentInstruments { get; set; }

        [JsonProperty(PropertyName = "_etag")]
        public string Etag { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
