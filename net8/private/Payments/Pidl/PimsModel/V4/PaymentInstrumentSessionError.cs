// <copyright file="PaymentInstrumentSessionError.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2019. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class PaymentInstrumentSessionError
    {
        [JsonProperty(PropertyName = "ErrorCode")]
        public string ErrorCode { get; set; }

        [JsonProperty(PropertyName = "Message")]
        public string Message { get; set; }

        // TODO, check deserialization of List<string>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed for serialization purpose.")]
        [JsonProperty(PropertyName = "Targets")]
        public List<string> Targets { get; set; }
    }
}