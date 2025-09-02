// <copyright file="FingerPrintMethodData.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    using Newtonsoft.Json;

    public class FingerPrintMethodData
    {
        [JsonProperty(PropertyName = "ThreeDSServerTransactionId", Required = Required.Default)]
        public string ThreeDSServerTransactionId { get; set; }

        [JsonProperty(PropertyName = "AcsStartProtocolVersion", Required = Required.Default)]
        public string AcsStartProtocolVersion { get; set; }

        [JsonProperty(PropertyName = "AcsEndProtocolVersion", Required = Required.Default)]
        public string AcsEndProtocolVersion { get; set; }

        [JsonProperty(PropertyName = "ThreeDSMethodUrl", Required = Required.Default)]
        public string ThreeDSMethodUrl { get; set; }

        [JsonProperty(PropertyName = "DSStartProtocolVersion", Required = Required.Default)]
        public string DSStartProtocolVersion { get; set; }

        [JsonProperty(PropertyName = "DSEndProtocolVersion", Required = Required.Default)]
        public string DSEndProtocolVersion { get; set; }
    }
}
