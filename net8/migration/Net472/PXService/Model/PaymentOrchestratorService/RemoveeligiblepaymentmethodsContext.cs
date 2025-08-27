// <copyright file="RemoveEligiblePaymentMethodsContext.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{
    using Newtonsoft.Json;

    public class RemoveEligiblePaymentMethodsContext
    {
        [JsonProperty(PropertyName = "PIID")]
        public string PIID { get; set; }
    }
}