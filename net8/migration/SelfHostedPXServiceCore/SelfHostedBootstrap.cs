namespace SelfHostedPXServiceCore
{
    /// <summary>
    /// Backwards-compatible bootstrap API that delegates to <see cref="SelfHostedPxService"/>.
    /// </summary>
    public static class SelfHostedBootstrap
    {
        /// <summary>
        /// Spin up the PX service in-memory via <see cref="SelfHostedPxService"/>.
        /// </summary>
        public static SelfHostedPxService StartInMemory(string? baseUrl, bool useSelfHostedDependencies, bool useArrangedResponses)
        {
            return SelfHostedPxService.StartInMemory(baseUrl, useSelfHostedDependencies, useArrangedResponses);
        }
    }
}
