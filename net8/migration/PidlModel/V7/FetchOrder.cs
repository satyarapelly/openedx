// <copyright file="FetchOrder.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using Newtonsoft.Json;

    public class FetchOrder
    {
        public FetchOrder()
        {
        }

        public FetchOrder(FetchOrder template)
        {
            this.Retry = template.Retry;
            this.Endpoint = template.Endpoint;
            this.UseSecondaryPayload = template.UseSecondaryPayload;
            this.XHRConfig = new XHRConfig(template.XHRConfig);
        }

        public FetchOrder(int retry, string endpoint, bool useSecondaryPayload, XHRConfig xhrConfig)
        {
            this.Retry = retry;
            this.Endpoint = endpoint;
            this.UseSecondaryPayload = useSecondaryPayload;
            this.XHRConfig = xhrConfig;
        }

        [JsonProperty(Order = 0, PropertyName = "retry")]
        public int Retry { get; set; }

        [JsonProperty(Order = 1, PropertyName = "endpoint")]
        public string Endpoint { get; set; }

        [JsonProperty(Order = 2, PropertyName = "useSecondaryPayload")]
        public bool UseSecondaryPayload { get; set; }

        [JsonProperty(Order = 3, PropertyName = "xhrConfig")]
        public XHRConfig XHRConfig { get; set; }
    }
}
