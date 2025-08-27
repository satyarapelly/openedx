// <copyright file="Links.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.AccountService
{
    using Newtonsoft.Json;

    /// <summary>
    /// Class representing the links in a given object
    /// </summary>
    public class Links
    {
        /// <summary>
        /// Gets or sets a value indicating the self link
        /// </summary>
        [JsonProperty(PropertyName = "self")]
        public LinkDetails Self { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the profiles link
        /// </summary>
        [JsonProperty(PropertyName = "profiles")]
        public LinkDetails Profiles { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the addresses link
        /// </summary>
        [JsonProperty(PropertyName = "addresses")]
        public LinkDetails Addresses { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the delete link
        /// </summary>
        [JsonProperty(PropertyName = "delete")]
        public LinkDetails Delete { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the update link
        /// </summary>
        [JsonProperty(PropertyName = "update")]
        public LinkDetails Update { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the add link
        /// </summary>
        [JsonProperty(PropertyName = "add")]
        public LinkDetails Add { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the snapshot link
        /// </summary>
        [JsonProperty(PropertyName = "snapshot")]
        public LinkDetails Snapshot { get; set; }
    }
}