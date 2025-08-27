// <copyright file="ITransactionServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.PXService.Model.TransactionService;
    using Microsoft.Commerce.Tracing;

    public interface ITransactionServiceAccessor
    {
        Task<PaymentResource> CreatePaymentObject(string accountId, EventTraceActivity traceActivityId);

        Task<TransactionResource> ValidateCvv(string accountId, string paymentId, ValidationParameters validationParameters, EventTraceActivity traceActivityId);
    }
}