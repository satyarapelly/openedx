// <copyright file="WalletService.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace SelfHostedPXServiceCore.Mocks
{
    using Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks;
    using Test.Common;

    public class WalletService : MockServiceWebRequestHandler
    {
        public readonly WalletServiceMockResponseProvider ResponseProvider;

        public WalletService(WalletServiceMockResponseProvider responseProvider, bool useArrangedResponses) : base(responseProvider, useArrangedResponses)
        {
            ResponseProvider = responseProvider;
        }
    }
}