// <copyright file="ITransactionDataServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.TransactionDataService
{
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.PXService.Accessors.TransactionDataService.DataModel;
    using Microsoft.Commerce.Tracing;

    public interface ITransactionDataServiceAccessor
    {
        Task<string> GenerateDataId(EventTraceActivity traceActivityId);

        Task<string> UpdateCustomerChallengeAttestation(string accountId, string sessionId, bool authenticationVerified, EventTraceActivity traceActivityId);
    }
}