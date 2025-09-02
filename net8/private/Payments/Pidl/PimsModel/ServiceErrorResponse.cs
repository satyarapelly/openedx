// <copyright file="ServiceErrorResponse.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PimsModel
{
    using Newtonsoft.Json;

    public class ServiceErrorResponse : ErrorResponse
    {
        [JsonProperty(PropertyName = "CorrelationId")]
        public string CorrelationId { get; set; }
    }
}
