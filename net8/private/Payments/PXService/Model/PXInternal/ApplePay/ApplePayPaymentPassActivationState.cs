// <copyright file="ApplePayPaymentPassActivationState.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PXInternal
{
    using System.Text.Json.Serialization;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// This is a model used by PXService internally to extract apple pay payment data
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ApplePayPaymentPassActivationState
    {
        // Active and ready to be used for payment.
        activated,

        // Not active but may be activated by the issuer.
        requiresActivation,

        // Not ready for use but activation is in progress.
        activating,

        // Not active and can't be activated.
        suspended,

        // Not active because the issuer has disabled the account associated with the device.
        deactivated
    }
}