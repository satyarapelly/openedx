// <copyright file="IAzureExPAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Tracing;
    using static Microsoft.Commerce.Payments.PXCommon.Flighting;

    public interface IAzureExPAccessor
    {
        /// <summary>
        /// Get AzureExP FeatureConfig for the given context
        /// </summary>
        /// <param name="flightContext">request context</param>
        /// <param name="traceActivityId">request trace id</param>
        /// <returns>FeatureConfig for the given flight context</returns>
        Task<FeatureConfig> GetExposableFeatures(Dictionary<string, string> flightContext, EventTraceActivity traceActivityId);

        /// <summary>
        /// Stops AzureExP blob polling
        /// </summary>
        void StopPolling();

        /// <summary>
        /// Initialize variant assignment provider. Used as test hook
        /// </summary>
        /// <param name="blobContent">blob content</param>
        /// <returns>true if successfully initialized</returns>
        bool InitializeVariantAssignmentProvider(byte[] blobContent);
    }
}
