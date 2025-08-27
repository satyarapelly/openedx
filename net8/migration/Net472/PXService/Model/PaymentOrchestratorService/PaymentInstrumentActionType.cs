// <copyright file="PaymentInstrumentActionType.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Type of the action on a payment instrument. 
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PaymentInstrumentActionType
    {
        /// <summary>
        /// No action
        /// </summary>
        None,

        /// <summary>
        /// Vault the payment instrument
        /// </summary>
        VaultOnSuccess
    }
}