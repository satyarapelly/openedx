// <copyright file="PayerAuthService.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace SelfHostedPXServiceCore.Mocks
{
    using Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks;
    using Test.Common;

    public class PayerAuthService : MockServiceWebRequestHandler
    {
        public readonly PayerAuthServiceMockResponseProvider ResponseProvider;

        public PayerAuthService(PayerAuthServiceMockResponseProvider responseProvider, bool useArrangedResponses) : base(responseProvider, useArrangedResponses)
        {
            ResponseProvider = responseProvider;
        }
    }
}
