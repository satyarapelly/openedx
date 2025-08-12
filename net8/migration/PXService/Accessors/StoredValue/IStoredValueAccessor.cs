// <copyright file="IStoredValueAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Model.StoredValueService;

    public interface IStoredValueAccessor
    {
        Task<IList<StoredValueFundingCatalog>> GetStoredValueFundingCatalog(string currency, EventTraceActivity traceActivityId);

        Task<FundStoredValueTransaction> FundStoredValue(
            string amount,
            string country,
            string currency,
            string piid,
            string puid,
            string legacyAccountId,
            string greenId,
            string ipAddress,
            string userAgent,
            EventTraceActivity traceActivityId,
            string description = "");

        Task<FundStoredValueTransaction> CheckFundStoredValue(string legacyAccountId, string referenceId, EventTraceActivity traceActivityId);
    }
}
