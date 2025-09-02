using Newtonsoft.Json;

namespace ThirdParty.Model
{
    public class BaseItem
    {
        private string id;

        [JsonProperty(PropertyName = "id")]
        public string Id
        {
            get => id;
            set
            {
                id = value;
                BucketId = id.Substring(0, 2);
            }
        }

        [JsonProperty(PropertyName = "bucketId")]
        public string BucketId { get; private set; }
    }
}
