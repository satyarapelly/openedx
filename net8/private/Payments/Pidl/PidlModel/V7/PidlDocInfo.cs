// <copyright file="PidlDocInfo.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    public class PidlDocInfo : ObjectInfo
    {
        public PidlDocInfo()
        {
        }

        public PidlDocInfo(PidlDocInfo template)
            : base(template)
        {
        }

        public PidlDocInfo(string resourceType, string language, string country, string partner, string type = null, string family = null, string scenario = null)
            : base(resourceType, language, country, partner)
        {
            if (!string.IsNullOrEmpty(type))
            {
                this.Parameters.Add(Constants.QueryParamFields.Type, type);
            }

            if (!string.IsNullOrEmpty(family))
            {
                this.Parameters.Add(Constants.QueryParamFields.Family, family);
            }

            if (!string.IsNullOrEmpty(scenario))
            {
                this.Parameters.Add(Constants.QueryParamFields.Scenario, scenario);
            }
        }

        [JsonProperty(PropertyName = "anonymousPidl")]
        public bool AnonymousPidl { get; set; }
    }
}
