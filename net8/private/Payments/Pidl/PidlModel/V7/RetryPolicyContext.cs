// <copyright file="RetryPolicyContext.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using Newtonsoft.Json;

    public class RetryPolicyContext
    {
        [JsonProperty(Order = 0, PropertyName = "maxRetryCount")]
        public int MaxRetryCount { get; set; }
    }
}
