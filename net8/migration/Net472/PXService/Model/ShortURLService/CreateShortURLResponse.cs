// <copyright file="CreateShortURLResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PXService.Model.ShortURLService
{
    using System;
    using Newtonsoft.Json;

    public class CreateShortURLResponse
    {
        public CreateShortURLResponse(string id, string uri, DateTime expirationTime)
        {
            this.Id = id;
            this.Uri = uri;
            this.ExpirationTime = expirationTime;
        }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "uri")]
        public string Uri { get; set; }

        [JsonProperty(PropertyName = "expirationTime")]
        public DateTime ExpirationTime { get; set; }
    }
}