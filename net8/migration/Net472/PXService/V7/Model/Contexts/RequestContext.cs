// <copyright file="RequestContext.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7.Contexts
{
    using System.Linq;
    using Newtonsoft.Json;    

    /// <summary>
    /// Represents the context of a request in the payment service.
    /// </summary>
    public class RequestContext
    {
        /// <summary>
        /// Gets or sets the tenant ID.
        /// </summary>
        [JsonProperty(PropertyName = "tenantId")]
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets the tenant customer ID.
        /// </summary>
        [JsonProperty(PropertyName = "tenantCustomerId")]
        public string TenantCustomerId { get; set; }

        /// <summary>
        /// Gets or sets the request ID.
        /// </summary>
        [JsonProperty(PropertyName = "requestId")]
        public string RequestId { get; set; }

        /// <summary>
        /// Gets or sets the internal account ID.
        /// </summary>
        [JsonProperty(PropertyName = "paymentAccountId")]
        public string PaymentAccountId { get; set; }

        public static string GetRequestType(RequestContext requestContext)
        {
            return requestContext?.RequestId?.Split('_')?.FirstOrDefault();
        }

        public static string GetRequestType(string requestId)
        {
            return requestId?.Split('_')?.FirstOrDefault();
        }
    }
}