// <copyright file="InitializeRequest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PXService.Model.IssuerService
{
    using Newtonsoft.Json;

    public class InitializeRequest
    {
        [JsonProperty(PropertyName = "cardProduct")]
        public string Card { get; set; }

        [JsonProperty(PropertyName = "channel")]
        public string Channel { get; set; }

        [JsonProperty(PropertyName = "referrerId")]
        public string ReferrerId { get; set; }

        [JsonProperty(PropertyName = "market")]
        public string Market { get; set; }

        [JsonProperty(PropertyName = "subchannel")]
        public string Subchannel { get; set; }
    }
}