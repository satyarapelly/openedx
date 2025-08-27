// <copyright file="ICatalogServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Model.CatalogService;
    using Tracing;

    public interface ICatalogServiceAccessor
    {
        Task<Catalog> GetProducts(List<string> productIds, string market, string language, string fieldsTemplate, string actionFilter, EventTraceActivity traceActivityId);

        Task<List<string>> GetSingleMarkets(EventTraceActivity traceActivityId);
    }
}
