// <copyright file="ServerErrorCode.cs" company="Microsoft">Copyright (c) Microsoft 2016. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using Newtonsoft.Json;

    public class ServerErrorCode
    {
        [JsonProperty(Order = 0, PropertyName = "ErrorMessage")]
        public string ErrorMessage { get; set; }

        [JsonProperty(Order = 1, PropertyName = "Target")]
        public string Target { get; set; }

        [JsonProperty(Order = 2, PropertyName = "retryPolicy")]
        public RetryPolicy RetryPolicy { get; set; }
    }
}
