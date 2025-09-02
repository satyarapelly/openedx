// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2020. All rights reserved.</copyright>

namespace SelfHostedPXServiceCore.Mocks
{
    using Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks;
    using Test.Common;

    public class StoredValueService : MockServiceWebRequestHandler
    {
        public StoredValueService(StoredValueServiceMockResponseProvider responseProvider, bool useArrangedResponses) : base(responseProvider, useArrangedResponses)
        {
        }
    }
}
