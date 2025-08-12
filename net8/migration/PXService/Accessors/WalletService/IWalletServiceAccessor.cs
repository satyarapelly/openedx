// <copyright file="IWalletServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.PXService.Model.WalletService;

    public interface IWalletServiceAccessor
    {
        Task<ProviderDataResponse> GetProviderData(EventTraceActivity traceActivityId, List<string> exposedFlightFeatures);
        
        Task<string> SetupProviderSession(SetupProviderSessionIncomingPayload providerSessionPayload, EventTraceActivity traceActivityId);

        Task<ProvisionWalletTokenResponse> Provision(string sessionId, string accountId, ProvisionWalletTokenIncomingPayload providerSessionPayload, EventTraceActivity traceActivityId);

        Task<ValidateDataResponse> Validate(string sessionId, string accountId, ValidateIncomingPayload validatePayload, EventTraceActivity traceActivityId);
    }
}