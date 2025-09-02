// <copyright file="AttachmentType.cs" company="Microsoft Corporation">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// AttachmentType for payment instrument.
    /// Wallet indicates customer information is required and pi will be added under the customer as wallet pi.
    /// Standalone indicates customner information is not required and will be ignored if provided.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AttachmentType
    {
        Wallet = 0,
        Standalone = 1,
    }
}
