// <copyright file="TokenDescriptionRequestIdentifierType.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.TokenPolicyService.DataModel
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// The identifier types supported for a token description request.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TokenDescriptionRequestIdentifierType
    {
        /// <summary>
        /// A token code, e.g. a 5x5
        /// </summary>
        TokenCode,
    }
}