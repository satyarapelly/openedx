// <copyright file="NetworkTokenizationService.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace SelfHostedPXServiceCore.Mocks
{
    using Test.Common;

    public class NetworkTokenizationService : MockServiceWebRequestHandler
    {
        public NetworkTokenizationService(NetworkTokenizationServiceMockResponseProvider responseProvider, bool useArrangedResponses) : base(responseProvider, useArrangedResponses)
        {
        }
    }
}
