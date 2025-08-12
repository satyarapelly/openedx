// <copyright file="PaymentChallengeStatus.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model
{
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum PaymentChallengeStatus
    {
        Unknown,
        Succeeded,
        ByPassed,
        Failed,
        [SuppressMessage("Microsoft.Naming", "CA1726", Justification = "We needed to use cancelled status")]
        Cancelled,
        TimedOut,
        NotApplicable,
        InternalServerError
    }
}