// <copyright file="Xbox360TokenDescription.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.TokenPolicyService.DataModel
{
    using Newtonsoft.Json;

    public class Xbox360TokenDescription
    {
        /// <summary>
        /// Gets or sets the reduced title.
        /// </summary>
        [JsonProperty(PropertyName = "reducedTitle")]
        public string ReducedTitle { get; set; }
    }
}