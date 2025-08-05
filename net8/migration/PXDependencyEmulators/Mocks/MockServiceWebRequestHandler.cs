// <copyright file="MockServiceWebRequestHandler.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Generic HTTP handler used by the self-hosted PX service to mock dependent services.
    /// It delegates to an <see cref="IMockResponseProvider"/> for arranged responses and
    /// falls back to the default <see cref="HttpClientHandler"/> when no match is found.
    /// </summary>
    public class MockServiceWebRequestHandler : HttpClientHandler
    {
        private readonly IMockResponseProvider responseProvider;
        private readonly bool useArrangedResponses;

        public MockServiceWebRequestHandler(IMockResponseProvider responseProvider, bool useArrangedResponses)
        {
            this.responseProvider = responseProvider;
            this.useArrangedResponses = useArrangedResponses;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (useArrangedResponses)
            {
                var arrangedResponse = await responseProvider.GetMatchedMockResponse(request);
                if (arrangedResponse != null)
                {
                    return arrangedResponse;
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
