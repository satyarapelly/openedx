// <copyright file="D365Service.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace SelfHostedPXServiceCore.Mocks
{
    using Test.Common;
    using Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks;

    public class D365Service : MockServiceWebRequestHandler
    {
        public D365Service(D365ServiceMockResponseProvider responseProvider, bool useArrangedResponses) : base(responseProvider, useArrangedResponses)
        {
        }
    }
}