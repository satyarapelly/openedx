// <copyright file="ISessionServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System.Threading.Tasks;
    using Tracing;

    public interface ISessionServiceAccessor
    {
        Task<string> GenerateId(string sessionType, EventTraceActivity traceActivityId);

        Task<T> GetSessionResourceData<T>(string sessionId, EventTraceActivity traceActivityId);

        Task CreateSessionFromData<T>(string sessionId, T sessionData, EventTraceActivity traceActivityId);

        Task UpdateSessionResourceData<T>(string sessionId, T newSessionData, EventTraceActivity traceActivityId);
    }
}