// <copyright file="ReplaceRequest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.OrchestrationService.DataModel
{
    using Newtonsoft.Json;

    /// <summary>
    /// Request object for replace operation.
    /// </summary>
    public class ReplaceRequest
    {
        /// <summary>
        /// Gets or sets Target payment instrument id.
        /// </summary>
        [JsonProperty(PropertyName = "targetPaymentInstrumentId")]
        public string TargetPaymentInstrumentId { get; set; }

        /// <summary>
        /// Gets or sets the current session.
        /// The session id for PSD2.
        /// </summary>
        [JsonProperty(PropertyName = "paymentSessionId")]
        public string PaymentSessionId { get; set; }
    }
}