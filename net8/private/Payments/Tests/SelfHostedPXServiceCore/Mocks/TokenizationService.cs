// <copyright file="TokenizationService.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace SelfHostedPXServiceCore.Mocks
{
    using Test.Common;
    using Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks;

    public class TokenizationService : MockServiceWebRequestHandler
    {
        public TokenizationService(TokenizationServiceMockResponseProvider responseProvider, bool useArrangedResponses) : base(responseProvider, useArrangedResponses)
        {
        }
    }
}