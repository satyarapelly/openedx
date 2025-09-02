// <copyright file="TriggerEventContext.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using Newtonsoft.Json;

    public class TriggerEventContext
    {
        public TriggerEventContext(string name)
        {
            this.Name = name;
        }

        public TriggerEventContext(string name, object parameters)
        {
            this.Name = name;
            this.Params = parameters;
        }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "params")]
        public object Params { get; set; }
    }
}
