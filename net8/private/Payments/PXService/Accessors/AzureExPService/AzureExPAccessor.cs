namespace Microsoft.Commerce.Payments.PXService
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Authentication;
    using Microsoft.Commerce.Payments.Authentication.AzureActiveDirectoryCaller;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using static Microsoft.Commerce.Payments.PXCommon.Flighting;

    /// <summary>
    /// Lightweight stub implementation of IAzureExPAccessor used for build purposes.
    /// </summary>
    public class AzureExPAccessor : IAzureExPAccessor
    {
        public AzureExPAccessor(string expBlobUrl, IAzureActiveDirectoryTokenLoader tokenLoader, HttpMessageHandler messageHandler, bool enableTestHook = false)
        {
            // No initialization required for stub.
        }

        /// <inheritdoc />
        public Task<FeatureConfig> GetExposableFeatures(Dictionary<string, string> flightContext, EventTraceActivity traceActivityId)
        {
            return Task.FromResult(new FeatureConfig());
        }

        /// <inheritdoc />
        public void StopPolling()
        {
            // Stub has no background work to stop.
        }

        /// <inheritdoc />
        public bool InitializeVariantAssignmentProvider(byte[] blobContent) => false;
    }
}
