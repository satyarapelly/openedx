// <copyright file="RetryPolicy.cs" company="Microsoft">Copyright (c) Microsoft 2016. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using Newtonsoft.Json;

    public enum RetryPolicyType
    {
        // This is the default policy if nothing is specified
        unlimitedRetry,

        // If this policy is set, the context object should specify maxRetryCount
        limitedRetry,

        // Errors will be treated as terminating errors
        noRetry
    }

    public class RetryPolicy
    {
        [JsonProperty(Order = 0, PropertyName = "type")]
        [JsonConverter(typeof(CamelCaseStringEnumConverter))]
        public RetryPolicyType RetryPolicyType { get; set; }

        [JsonProperty(Order = 1, PropertyName = "context")]
        public RetryPolicyContext Context { get; set; }
    }
}
