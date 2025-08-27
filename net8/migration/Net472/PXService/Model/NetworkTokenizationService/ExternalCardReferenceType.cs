// <copyright file="ExternalCardReferenceType.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.NetworkTokenizationService
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Indicates the external card reference type.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ExternalCardReferenceType
    {
        /// <summary>
        /// Unknown external card reference type.
        /// </summary>
        Unknown,

        /// <summary>
        /// Payment instrument ID from PIMS.
        /// </summary>
        PaymentInstrumentId,

        /// <summary>
        /// Other reference type.
        /// </summary>
        Other,
    }
}