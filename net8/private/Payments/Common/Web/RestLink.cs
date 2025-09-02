// <copyright file="RestLink.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Web
{
    using Newtonsoft.Json;

    /// <summary>
    /// Link for REST resources
    /// This class is tied to the transaction service interface. If new changes to this class cause break on interface 
    /// starting from V2. A new class need to be forked for a new interface version.
    /// </summary>
    public class RestLink
    {
        /// <summary>
        /// Gets or sets Hyper Reference
        /// </summary>
        [JsonProperty(PropertyName = "href")]
        public string Href { get; set; }

        /// <summary>
        /// Gets or sets Payload
        /// </summary>
        [JsonProperty(PropertyName = "payload")]
        public object Payload { get; set; }

        /// <summary>
        /// Gets or sets the Http method
        /// </summary>
        [JsonProperty(PropertyName = "method")]
        public string Method { get; set; }
    }
}
