// <copyright file="ObjectInfo.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public abstract class ObjectInfo
    {
        private Dictionary<string, string> parameters;

        public ObjectInfo()
        {
        }

        public ObjectInfo(ObjectInfo template)
        {
            this.ResourceType = template.ResourceType;
            this.SetParameters(template.Parameters);
        }

        public ObjectInfo(string resourceType, string language, string country, string partner)
        {
            this.ResourceType = resourceType;
            this.SetParameters(language, country, partner);
        }

        [JsonProperty(PropertyName = "resourceType")]
        public string ResourceType { get; set; }

        [JsonProperty(PropertyName = "parameters")]
        public Dictionary<string, string> Parameters
        {
            get
            {
                return this.parameters;
            }
        }

        public void SetParameters(Dictionary<string, string> parametersIn)
        {
            if (parametersIn != null)
            {
                this.parameters = new Dictionary<string, string>(parametersIn);
            }
        }

        public void SetParameters(string language, string country, string partner)
        {
            this.parameters = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(language))
            {
                this.parameters.Add(Constants.QueryParamFields.Language, language);
            }

            if (!string.IsNullOrEmpty(country))
            {
                this.parameters.Add(Constants.QueryParamFields.Country, country);
            }

            if (!string.IsNullOrEmpty(partner))
            {
                this.parameters.Add(Constants.QueryParamFields.Partner, partner);
            }
        }
    }
}