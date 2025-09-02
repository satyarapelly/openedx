// <copyright file="HalCollectionResource.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Web
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    /// <summary>
    /// HAL Collection JSON Message
    /// This class is tied to the transaction service interface. If new changes to this class cause break on interface 
    /// starting from V2. A new class need to be forked for a new interface version.
    /// </summary>
    public class HalCollectionResource : HalResource
    {
        private List<HalResource> responseCollection;

        protected HalCollectionResource(string typeName, string version, IEnumerable<HalResource> responseObjects)
            : base(typeName, version)
        {
            if (responseObjects == null)
            {
                this.responseCollection = new List<HalResource>();
            }
            else
            {
                this.responseCollection = new List<HalResource>(responseObjects);
            }
        }

        protected HalCollectionResource(string typeName, string version)
            : this(typeName, version, null)
        {
        }

        /// <summary>
        /// Gets the collection of response objects
        /// </summary>
        [JsonProperty("_", Required = Required.Always)]
        [SuppressMessage("Microsoft.Performance", "CA1811", Justification = "This is a field used by JSON to generate HAL response")]
        public IEnumerable<HalResource> Collection
        {
            get
            {
                return this.responseCollection;
            }
        }

        protected void AddObject(HalResource responseObject)
        {
            this.responseCollection.Add(responseObject);
        }
    }
}
