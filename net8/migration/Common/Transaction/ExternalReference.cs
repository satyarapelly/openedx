// <copyright file="ExternalReference.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Transaction
{
    using Newtonsoft.Json;

    public class ExternalReference
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "uri")]
        public string Uri { get; set; }

        public ExternalReference Clone()
        {
            return new ExternalReference
            {
                Id = this.Id,
                Uri = this.Uri,
            };
        }
    }
}