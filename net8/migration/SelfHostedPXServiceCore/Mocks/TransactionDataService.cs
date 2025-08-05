// <copyright file="TransactionDataService.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace SelfHostedPXServiceCore.Mocks
{
    using Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks;
    using Test.Common;

    public class TransactionDataService : MockServiceWebRequestHandler
    {
        public readonly TransactionDataServiceMockResponseProvider ResponseProvider;

        public TransactionDataService(TransactionDataServiceMockResponseProvider responseProvider, bool useArrangedResponses) : base(responseProvider, useArrangedResponses)
        {
            ResponseProvider = responseProvider;
        }
    }
}