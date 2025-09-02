// <copyright file="ChallengeScenario.cs" company="Microsoft">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace Tests.Common.Model.PX
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ChallengeScenario
    {
        PaymentTransaction,
        RecurringTransaction,
        AddCard
    }
}
