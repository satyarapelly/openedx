// <copyright file="ListCardsResponse.cs" company="Microsoft">Copyright (c) Microsoft 2017. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class ListCardsResponse 
    {
        /// <summary>
        /// Gets or sets a value indicating the list of items object.
        /// </summary>
        [JsonProperty(PropertyName = "items")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needs to be set for serialization purpose")]
        public List<Card> Items { get; set; }
    }
}