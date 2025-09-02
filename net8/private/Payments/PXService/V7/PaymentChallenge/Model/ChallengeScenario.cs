// <copyright file="ChallengeScenario.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model
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