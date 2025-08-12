// <copyright file="PaymentRequestStatus.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum PaymentRequestStatus
    {
        PendingClientAction,
        PendingInitialTransaction,
        PendingProcessing,
        PendingNextTransaction,
        Completed,
        Abandoned
    }
}