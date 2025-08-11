// <copyright file="CvvValidationCode.cs" company="Microsoft">Copyright (c) Microsoft 2017. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Transaction
{
    using Microsoft.Commerce.Payments.Common.Web;
    using Newtonsoft.Json;

    /// <summary>
    /// Normailzed CVV valid result code
    /// </summary>
    [JsonConverter(typeof(EnumJsonConverter))]
    public enum CvvValidationCode
    {
        /// <summary>
        /// The default status when nothing is known.
        /// </summary>
        None,

        /// <summary>
        /// The cvv is matched.
        /// </summary>
        Matched,

        /// <summary>
        /// The cvv is not matched.
        /// </summary>
        Unmatched,

        /// <summary>
        /// The cvv is not processed.
        /// </summary>
        NotChecked,

        /// <summary>
        /// Issuer has not certified for CVV2 or Issuer has not provided Visa with the CVV2 encryption keys.
        /// </summary>
        IssuerNotCertified,

        /// <summary>
        /// no CVV2 number was entered.
        /// </summary>
        NoCvvProvided,
    }
}