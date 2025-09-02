// <copyright company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class EventContext
    {
        public EventContext()
        {
        }

        public EventContext(string href, string method, bool silent)
        {
            this.Href = href;
            this.Method = method;
            this.Silent = silent;
        }

        [JsonProperty(PropertyName = "href")]
        public string Href { get; set; }

        [JsonProperty(PropertyName = "method")]
        public string Method { get; set; }

        [JsonProperty(PropertyName = "silent")]
        public bool Silent { get; set; }

        [JsonProperty(PropertyName = "explicit")]
        public bool Explicit { get; set; }

        [JsonProperty(PropertyName = "resourceType")]
        public string ResourceType { get; set; }

        [JsonProperty(PropertyName = "payload")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public Dictionary<string, string> Payload { get; set; }
    }
}