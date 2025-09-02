// <copyright file="PimsServiceDelegatingHandler.cs" company="Microsoft">Copyright (c) Microsoft 2017. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks
{
    using Test.Common;

    public class PimsServiceDelegatingHandler : MockServiceDelegatingHandler
    {
        public PimsServiceDelegatingHandler(PimsMockResponseProvider pimsMockResponseProvider, bool useArrangedResponses) : base(pimsMockResponseProvider, useArrangedResponses)
        {
        }
    }
}