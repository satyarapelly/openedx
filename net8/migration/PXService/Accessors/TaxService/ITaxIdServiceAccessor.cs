// <copyright file="ITaxIdServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.PXService.V7;

    public interface ITaxIdServiceAccessor
    {
        Task<object[]> GetTaxIds(string accountId, EventTraceActivity traceActivityId);

        Task<TaxData[]> GetTaxIdsByProfileTypeAndCountryWithState(string accountId, string profileType, string country, EventTraceActivity traceActivityId);
    }
}