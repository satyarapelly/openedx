// <copyright file="CommerceAccountCustomer.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.AccountService
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Class representing the MCA's customer
    /// </summary>
    public class CommerceAccountCustomer
    {
        /// <summary>
        /// Gets or sets a value indicating the id.
        /// </summary>
        [JsonProperty(PropertyName = "id")]

        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the customer type. Organization For Organization and Company. Account for MCA.
        /// </summary>
        [JsonProperty(PropertyName = "customer_type")]
        public string CustomerType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the sub type. Tenant for Organization. Company for Company. Customer_Commerce for Customer Bootstrapping. 
        /// </summary>
        [JsonProperty(PropertyName = "customer_subtype")]
        public string CustomerSubtype { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the identity of the account's owner. AADTenant Identity for Organization. Jarvis Internal Identity for Company and MCA.
        /// </summary>
        [JsonProperty(PropertyName = "identity")]
        public Identity Identity { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether its a test type.
        /// </summary>
        [JsonProperty(PropertyName = "is_test")]
        public bool IsTest { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the object type.
        /// </summary>
        [JsonProperty(PropertyName = "object_type")]
        public string ObjectType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the resource status.
        /// </summary>
        [JsonProperty(PropertyName = "resource_status")]
        public string ResourceStatus { get; set; }
    }
}