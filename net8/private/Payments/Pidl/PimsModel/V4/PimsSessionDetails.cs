// <copyright file="PimsSessionDetails.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2019. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    using System.Net;
    using Newtonsoft.Json;

    public class PimsSessionDetails
    {
        [JsonProperty(PropertyName = "paymentInstrumentId")]
        public string PaymentInstrumentId { get; set; }

        [JsonProperty(PropertyName = "statusCode")]
        public HttpStatusCode? StatusCode { get; set; }

        [JsonProperty(PropertyName = "error")]
        public InstrumentManagementServiceErrorResponse SessionError { get; set; }
    }
}
