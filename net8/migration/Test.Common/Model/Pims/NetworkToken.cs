namespace Tests.Common.Model.Pims
{
    using Newtonsoft.Json;

    public class NetworkToken
    {
        [JsonProperty("networkTokenId")]
        public string Id { get; set; }

        [JsonProperty("networkTokenUsage")]
        public string Usage { get; set; }
    }
}
