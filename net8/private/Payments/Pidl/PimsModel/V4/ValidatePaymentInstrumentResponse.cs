// <copyright file="ValidatePaymentInstrumentResponse.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2020. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    using Newtonsoft.Json;

    public class ValidatePaymentInstrumentResponse
    {
        [JsonProperty(PropertyName = "result")]
        public string Result { get; set; }
    }
}
