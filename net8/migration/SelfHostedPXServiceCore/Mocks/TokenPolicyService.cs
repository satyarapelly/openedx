// <copyright file="TokenPolicyService.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace SelfHostedPXServiceCore.Mocks
{
    using Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks;
    using Test.Common;

    public class TokenPolicyService : MockServiceWebRequestHandler
    {
        public readonly TokenPolicyServiceMockResponseProvider ResponseProvider;

        public TokenPolicyService(TokenPolicyServiceMockResponseProvider responseProvider, bool useArrangedResponses) : base(responseProvider, useArrangedResponses)
        {
        }
    }
}