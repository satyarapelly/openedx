// <copyright file="PidlExecutionResult.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public enum PidlExecutionResultStatus
    {
        Passed,
        Failed
    }

    public class PidlExecutionResult
    {
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "status")]
        public PidlExecutionResultStatus Status { get; set; }

        [JsonProperty(PropertyName = "errorCode")]
        public string ErrorCode { get; set; }

        [JsonProperty(PropertyName = "errorMessage")]
        public string ErrorMessage { get; set; }
    }
}