// <copyright file="FraudDetectionService.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace SelfHostedPXServiceCore.Mocks
{
    using Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks;
    using Test.Common;

    public class FraudDetectionService : MockServiceWebRequestHandler
    {
        public readonly FraudDetectionMockResponseProvider ResponseProvider;

        public FraudDetectionService(FraudDetectionMockResponseProvider responseProvider, bool useArrangedResponses) : base(responseProvider, useArrangedResponses)
        {
            ResponseProvider = responseProvider;
        }
    }
}
