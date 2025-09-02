// <copyright file="ChallengeCancelIndicator.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PayerAuthService
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ChallengeCancelIndicator
    {
        [SuppressMessage("Microsoft.Naming", "CA1726", Justification = "We needed to use cancelled status")]
        [EnumMember(Value = "01")]
        CancelledByCardHolder,

        [SuppressMessage("Microsoft.Naming", "CA1726", Justification = "We needed to use cancelled status")]
        [EnumMember(Value = "02")]
        CancelledByRequestor,

        [EnumMember(Value = "03")]
        TransactionAbandoned,

        [EnumMember(Value = "04")]
        TransactionTimedOut,

        [EnumMember(Value = "05")]
        TransactionCReqTimedOut,

        [EnumMember(Value = "06")]
        TransactionError,

        [EnumMember(Value = "07")]
        Unknown,
    }
}