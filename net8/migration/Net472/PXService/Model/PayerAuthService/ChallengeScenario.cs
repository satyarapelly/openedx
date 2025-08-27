// <copyright file="ChallengeScenario.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PayerAuthService
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