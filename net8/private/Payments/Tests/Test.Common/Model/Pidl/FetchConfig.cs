// <copyright file="FetchConfig.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    public class FetchConfig
    {
        public FetchConfig()
        {
        }

        public FetchConfig(FetchConfig template)
        {
            this.InitialRetryTimeout = template.InitialRetryTimeout;
            this.RetryTimeoutMultiplier = template.RetryTimeoutMultiplier;
            this.RetryableErrorCodes = template.RetryableErrorCodes;
            this.FetchOrder = template.FetchOrder;
        }

        public FetchConfig(
            int initialRetryTimeout,
            double retryTimeoutMultiplier,
            List<int> retryableErrorCodes,
            List<FetchOrder> fetchOrder)
        {
            this.InitialRetryTimeout = initialRetryTimeout;
            this.RetryTimeoutMultiplier = retryTimeoutMultiplier;
            this.RetryableErrorCodes = retryableErrorCodes;
            this.FetchOrder = fetchOrder;
        }

        [JsonProperty(Order = 0, PropertyName = "initialRetryTimeout")]
        public int InitialRetryTimeout { get; set; }

        [JsonProperty(Order = 1, PropertyName = "retryTimeoutMultiplier")]
        public double RetryTimeoutMultiplier { get; set; }

        [JsonProperty(Order = 2, PropertyName = "retryableErrorCodes")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public List<int> RetryableErrorCodes { get; set; }

        [JsonProperty(Order = 3, PropertyName = "maxServerErrorRetryCount")]
        public int MaxServerErrorRetryCount { get; set; }

        [JsonProperty(Order = 4, PropertyName = "fetchOrder")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public List<FetchOrder> FetchOrder { get; set; }
    }
}