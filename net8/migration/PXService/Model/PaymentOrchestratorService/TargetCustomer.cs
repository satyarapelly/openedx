// <copyright file="TargetCustomer.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{
    using Newtonsoft.Json;

    /// <summary>
    ///     Model the information of the target customer.
    /// </summary>
    [JsonObject]
    public class TargetCustomer
    {
        [JsonProperty(PropertyName = "customerType")]
        public string CustomerType { get; set; }

        [JsonProperty(PropertyName = "customerId")]
        public string CustomerId { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "firstName")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "lastName")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "oid")]
        public string Oid { get; set; }

        [JsonProperty(PropertyName = "tid")]
        public string Tid { get; set; }

        [JsonProperty(PropertyName = "puid")]
        public string Puid { get; set; }

        [JsonProperty(PropertyName = "orgPuid")]
        public string OrgPuid { get; set; }

        [JsonProperty(PropertyName = "uniqueName")]
        public string UniqueName { get; set; }

        [JsonProperty(PropertyName = "bingAdsId")]
        public string BingAdsId { get; set; }

        [JsonProperty(PropertyName = "commerceRootId")]
        public string CommerceRootId { get; set; }

        [JsonProperty(PropertyName = "onBehalfOf")]
        public bool OnBehalfOf { get; set; }
    }
}