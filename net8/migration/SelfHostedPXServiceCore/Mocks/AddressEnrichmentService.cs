// <copyright file="AddressEnrichmentService.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace SelfHostedPXServiceCore.Mocks
{
    using Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks;
    using Test.Common;

    public class AddressEnrichmentService : MockServiceWebRequestHandler
    {
        public AddressEnrichmentService(AddressEnrichmentServiceMockResponseProvider responseProvider, bool useArrangedResponses) : base(responseProvider, useArrangedResponses)
        {
        }
    }
}
