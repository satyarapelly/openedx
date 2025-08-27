// <copyright file="IdentityData.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.AccountService
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Class representing the identity data
    /// </summary>
    public class IdentityData
    {
        /// <summary>
        /// Gets or sets a value indicating the id of the identity 
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }
    }
}