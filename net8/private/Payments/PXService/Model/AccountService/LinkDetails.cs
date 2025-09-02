// <copyright file="LinkDetails.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.AccountService
{
    using Newtonsoft.Json;

    /// <summary>
    /// Class representing the LinkDetails object
    /// </summary>
    public class LinkDetails
    {
        /// <summary>
        /// Gets or sets a value indicating the href object 
        /// </summary>
        [JsonProperty(PropertyName = "href")]
        public string Href { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the method object 
        /// </summary>
        [JsonProperty(PropertyName = "method")]
        public string Method { get; set; }
    }
}