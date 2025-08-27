// <copyright file="ClientAction.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{
    using Newtonsoft.Json;

    public class ClientAction
    {
        [JsonProperty(PropertyName = "type")]
        public ClientActionType ActionType { get; set; }

        public PaymentInstrumentChallengeType ChallengeType { get; set; }

        public PaymentInstrument PaymentInstrument { get; set; }
    }
}