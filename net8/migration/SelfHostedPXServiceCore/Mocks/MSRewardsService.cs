// <copyright file="MSRewardsService.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace SelfHostedPXServiceCore.Mocks
{
    using Test.Common;

    public class MSRewardsService : MockServiceWebRequestHandler
    {
        public MSRewardsService(MSRewardsServiceMockResponseProvider responseProvider, bool useArrangedResponses) : base(responseProvider, useArrangedResponses)
        {
        }
    }
}