// <copyright file="ResourceInfo.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    public class ResourceInfo : ObjectInfo
    {
        public ResourceInfo()
        {
        }

        public ResourceInfo(ResourceInfo template)
            : base(template)
        {
            this.Id = template.Id;
        }

        public ResourceInfo(string resourceType, string id, string language, string country, string partner)
            : base(resourceType, language, country, partner)
        {
            this.Id = id;
        }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "sessionId")]
        public string SessionId { get; set; }
    }
}
