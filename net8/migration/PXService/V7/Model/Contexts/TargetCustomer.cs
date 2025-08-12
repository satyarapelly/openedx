// <copyright file="TargetCustomer.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7.Contexts
{
    using Newtonsoft.Json;

    /// <summary>
    /// Model for target customer.
    /// </summary>
    [JsonObject]
    public class TargetCustomer
    {
        [JsonProperty(PropertyName = "customerType")]
        public string CustomerType { get; set; }

        [JsonProperty(PropertyName = "customerId")]
        public string CustomerId { get; set; }
    }
}
