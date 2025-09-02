// <copyright file="HalResource.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Web
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    /// <summary>
    /// HAL JSON Message, you can refer to http://tools.ietf.org/html/draft-kelly-json-hal for more details about HAL JSON
    /// This class is tied to the transaction service interface. If new changes to this class cause break on interface 
    /// starting from V2. A new class need to be forked for a new interface version.
    /// </summary>
    public class HalResource
    {
        private readonly Dictionary<string, RestLink> links;
        private string typeName;
        private string version;

        public HalResource(string typeName, string version)
        {
            this.links = new Dictionary<string, RestLink>();
            this.typeName = typeName;
            this.version = version;
        }

        /// <summary>
        /// Gets HAL links
        /// </summary>
        [JsonProperty(PropertyName = "links")]
        [SuppressMessage("Microsoft.Performance", "CA1811", Justification = "This is a field used by JSON to generate HAL response")]
        public Dictionary<string, RestLink> Links
        {
            get
            {
                return this.links;
            }
        }

        /// <summary>
        /// Gets REST object type name
        /// </summary>
        [JsonProperty("type", Required = Required.Always)]
        [SuppressMessage("Microsoft.Performance", "CA1811", Justification = "This is a field used by JSON to generate HAL response")]
        public string TypeName
        {
            get
            {
                return this.typeName;
            }
        }

        /// <summary>
        /// Gets REST response version
        /// </summary>
        [JsonProperty(PropertyName = "version")]
        [SuppressMessage("Microsoft.Performance", "CA1811", Justification = "This is a private fields used by JSON to generate HAL response")]
        public string Version
        {
            get
            {
                return this.version;
            }
        }

        protected void AddLink(string name, RestLink link)
        {
            this.links.Add(name, link);
        }
    }
}
