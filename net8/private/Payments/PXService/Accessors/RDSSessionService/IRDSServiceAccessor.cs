// <copyright file="IRDSServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Tracing;

    public interface IRDSServiceAccessor
    {
        Task<string> GetRDSSessionState(string rdsSessionId, EventTraceActivity traceActivityId);
    }
}