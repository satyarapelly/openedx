// <copyright file="PartnerSettingsService.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace SelfHostedPXServiceCore.Mocks
{
    using Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks;
    using Test.Common;

    public class PartnerSettingsService : MockServiceWebRequestHandler
    {
        public readonly PartnerSettingsServiceMockResponseProvider ResponseProvider;

        public PartnerSettingsService(PartnerSettingsServiceMockResponseProvider responseProvider, bool useArrangedResponses) : base(responseProvider, useArrangedResponses)
        {
            ResponseProvider = responseProvider;
        }
    }
}