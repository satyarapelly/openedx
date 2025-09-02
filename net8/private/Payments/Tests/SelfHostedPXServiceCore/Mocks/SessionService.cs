// <copyright file="SessionService.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace SelfHostedPXServiceCore.Mocks
{
    using Test.Common;
    using Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks;

    public class SessionService : MockServiceWebRequestHandler
    {
        public readonly SessionServiceMockResponseProvider ResponseProvider;

        public SessionService(SessionServiceMockResponseProvider responseProvider, bool useArrangedResponses) : base(responseProvider, useArrangedResponses)
        {
            ResponseProvider = responseProvider;
        }
    }
}
