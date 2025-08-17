// <copyright file="ISecretStore.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tools.InstallCertificates
{
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Tracing;

    public interface ISecretStore
    {
        Task<byte[]> ReadFileBytes(string fileName, EventTraceActivity traceActivityId);

        Task<string> ReadFileAsText(string fileName, EventTraceActivity traceActivityId);

        Task<string> GetValue(string componentName, string key, EventTraceActivity traceActivityId);
    }
}
