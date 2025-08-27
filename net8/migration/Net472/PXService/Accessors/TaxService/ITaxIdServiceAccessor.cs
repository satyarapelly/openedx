// <copyright file="ITaxIdServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.PXService.V7;
    using Microsoft.Commerce.Tracing;

    public interface ITaxIdServiceAccessor
    {
        Task<object[]> GetTaxIds(string accountId, EventTraceActivity traceActivityId);

        Task<TaxData[]> GetTaxIdsByProfileTypeAndCountryWithState(string accountId, string profileType, string country, EventTraceActivity traceActivityId);
    }
}