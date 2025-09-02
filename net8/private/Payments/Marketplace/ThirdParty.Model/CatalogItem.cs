using System.Collections.Generic;
using Newtonsoft.Json;

namespace ThirdParty.Model
{
    public class CatalogItem : BaseItem
    {
        [JsonProperty(PropertyName = "product")]
        public Product Product { get; set; }

        [JsonProperty(PropertyName = "skus")]
        public IList<ProductSku> Skus { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
