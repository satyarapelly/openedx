// <copyright file="ITokenizationServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.TokenizationService
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Tracing;

    public interface ITokenizationServiceAccessor
    {
        Task<string> GetEncryptionKey(EventTraceActivity traceActivityId, List<string> exposedFlightFeatures = null);

        Dictionary<string, string> GetTokenizationServiceUrls();
    }
}