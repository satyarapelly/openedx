// <copyright file="MockServiceDelegatingHandler.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Test.Common;

    /// <summary>
    /// A delegating handler that allows tests to supply arranged responses for outbound
    /// requests. If the <see cref="IMockResponseProvider"/> does not supply a response the
    /// request is forwarded to the next handler in the pipeline.
    /// </summary>
    public class MockServiceDelegatingHandler : DelegatingHandler
    {
        private readonly IMockResponseProvider responseProvider;
        private readonly bool useArrangedResponses;

        public MockServiceDelegatingHandler(IMockResponseProvider responseProvider, bool useArrangedResponses)
            : this(responseProvider, useArrangedResponses, new HttpClientHandler())
        {
        }

        public MockServiceDelegatingHandler(IMockResponseProvider responseProvider, bool useArrangedResponses, HttpMessageHandler innerHandler)
            : base(innerHandler)
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