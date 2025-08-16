// <copyright file="PidlIdentity.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    public class PidlIdentity
    {
        public PidlIdentity()
        {
        }

        public PidlIdentity(PidlIdentity template)
            : this(template.DescriptionType, template.Operation, template.Country, template.ResourceId)
        {
        }

        public PidlIdentity(string descriptionType, string operation, string country, string resourceId)
        {
            this.DescriptionType = descriptionType;
            this.Operation = operation;
            this.Country = country;
            this.ResourceId = resourceId;
        }

        [JsonProperty(PropertyName = "description_type")]
        public string DescriptionType { get; set; }

        [JsonProperty(PropertyName = "operation")]
        public string Operation { get; set; }

        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }

        [JsonProperty(PropertyName = "resource_id")]
        public string ResourceId { get; set; }
    }
}
