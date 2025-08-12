// <copyright file="SuccessResponsePayload.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using Newtonsoft.Json;

    public class SuccessResponsePayload
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "operationType")]
        public string OperationType { get; set; }

        [JsonProperty(PropertyName = "response")]
        public object Response { get; set; }

        [JsonProperty(PropertyName = "additionalData")]
        public object AdditionalData { get; set; }

        [JsonProperty(PropertyName = "cv")]
        public string Cv { get; set; }

        [JsonProperty(PropertyName = "secondaryResponses")]
        public object SecondaryResponses { get; set; }
    }
}