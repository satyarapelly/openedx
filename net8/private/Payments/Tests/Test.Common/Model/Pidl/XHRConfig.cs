// <copyright file="XHRConfig.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
    using Newtonsoft.Json;

    public class XHRConfig
    {
        public XHRConfig()
        {
        }

        public XHRConfig(XHRConfig template)
        {
            this.GetRequestTimeout = template.GetRequestTimeout;
            this.PostRequestTimeout = template.PostRequestTimeout;
        }

        public XHRConfig(int getRequestTimeout, int postRequestTimeout)
        {
            this.GetRequestTimeout = getRequestTimeout;
            this.PostRequestTimeout = postRequestTimeout;
        }

        [JsonProperty(Order = 0, PropertyName = "getRequestTimeout")]
        public int GetRequestTimeout { get; set; }

        [JsonProperty(Order = 1, PropertyName = "postRequestTimeout")]
        public int PostRequestTimeout { get; set; }
    }
}