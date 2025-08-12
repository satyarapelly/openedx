// <copyright file="TokenCategory.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.TokenPolicyService.DataModel
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum TokenCategory
    {
        /// <summary>
        /// The token category could not be determined
        /// </summary>
        Unknown,

        /// <summary>
        /// The token is a Legacy Currency Stored Value token
        /// that does not have offers represented in Catalog.
        /// NOTE: Any modern CSV tokens that do have offers in Catalog
        /// will have a TokenCategory of "Other", not "Csv".
        /// </summary>
        Csv,

        /// <summary>
        /// A legacy token for the Xbox 360 ecosystem
        /// </summary>
        Xbox360,

        /// <summary>
        /// A Skype token
        /// </summary>
        Skype,

        /// <summary>
        /// Another type of token.
        /// These include tokens for modern Xbox, Apps, Perpetuals, 
        /// Modern CSV (has offers in Catalog), Subscriptions, etc.
        /// </summary>
        Other
    }
}