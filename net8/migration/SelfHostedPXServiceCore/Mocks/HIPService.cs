// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2022. All rights reserved.</copyright>

namespace SelfHostedPXServiceCore.Mocks
{
    using Test.Common;

    public class HIPService : MockServiceWebRequestHandler
    {
        public HIPService(HIPServiceMockResponseProvider responseProvider, bool useArrangedResponses) : base(responseProvider, useArrangedResponses)
        {
        }
    }
}
