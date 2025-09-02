// <copyright file="PaymentChallengeStatus.cs" company="Microsoft">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace Tests.Common.Model.PX
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
    }
}
