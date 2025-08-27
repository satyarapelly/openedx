// <copyright file="PaymentInstrumentUsage.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum PaymentInstrumentUsage
    {
        /// <summary>
        /// The payment instrument is the primary choice for transactions.
        /// Only one primary payment instrument is allowed.
        /// </summary>
        PrimaryPaymentInstrument,
    }
}