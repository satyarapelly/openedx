// <copyright file="SellerMarketPlaceService.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace SelfHostedPXServiceCore.Mocks
{
    using Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks;
    using Test.Common;

    public class SellerMarketPlaceService : MockServiceWebRequestHandler
    {
        public SellerMarketPlaceService(SellerMarketPlaceServiceMockResponseProvider responseProvider, bool useArrangedResponses) : base(responseProvider, useArrangedResponses)
        {
        }
    }
}