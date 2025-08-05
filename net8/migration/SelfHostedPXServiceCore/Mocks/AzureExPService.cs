// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace SelfHostedPXServiceCore.Mocks
{
    using Test.Common;

    public class AzureExPService : MockServiceWebRequestHandler
    {
        public AzureExPService(AzureExPServiceMockResponseProvider responseProvider, bool useArrangedResponses) : base(responseProvider, useArrangedResponses)
        {
        }
    }
}
