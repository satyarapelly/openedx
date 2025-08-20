// <copyright file="INetworkTokenizationServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.NetworkTokenizationService
{
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXService.Model.NetworkTokenizationService;
    using Microsoft.Commerce.Tracing;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface INetworkTokenizationServiceAccessor
    {
        Task<NetworkTokenizationServiceResponse> GetNetworkTokens(string puid, string deviceId, EventTraceActivity traceActivityId, List<string> exposedFlightFeatures);

        Task<GetTokenMetadataResponse> RequestToken(string puid, string deviceId, EventTraceActivity traceActivityId, List<string> exposedFlightFeatures, string email, string country, string language, PaymentInstrument paymentInstrument);

        Task<GetTokenizationEligibilityResponse> Tokenizable(string puid, string deviceId, EventTraceActivity traceActivityId, List<string> exposedFlightFeatures, string email, string bankIdentificationNumber, string cardProviderName, string networkTokenUsage);

        Task<GetCredentialsResponse> FetchCredentials(string ntid, string puid, string deviceId, EventTraceActivity traceActivityId, List<string> exposedFlightFeatures, string merchantURL, string storedProfile, string email);

        Task<ListTokenMetadataResponse> ListTokensWithExternalCardReference(string puid, string deviceId, EventTraceActivity traceActivityId, List<string> exposedFlightFeatures, string externalCardReference, string email);

        Task<RequestDeviceBindingResponse> RequestDeviceBinding(string ntid, string puid, string deviceId, EventTraceActivity traceActivityId, List<string> exposedFlightFeatures, string externalCardReference, string email, object sessionContext, object browserData);

        Task<RequestChallengeResponse> RequestChallenge(string ntid, string challengeId, string challengeMethodId, string puid, EventTraceActivity traceActivityId, List<string> exposedFlightFeatures, string email);

        Task<object> ValidateChallenge(string ntid, string challengeId, string challengeMethodId, string puid, EventTraceActivity traceActivityId, List<string> exposedFlightFeatures, string otp, string email);

        Task<PasskeyMandateResponse> SetMandates(string ntid, string puid, EventTraceActivity traceActivityId, List<string> exposedFlightFeatures, object appInstance, AssuranceData assuranceData, List<Mandate> mandates, string dfSessionId, string email);

        Task<PasskeyOperationResponse> PasskeyAuthenticate(string ntid, int authenticationAmount, string currencyCode, string puid, string deviceId, EventTraceActivity traceActivityId, List<string> exposedFlightFeatures, object sessionContext, object browserData, string applicationUrl, string merchantName, string email);

        Task<PasskeyOperationResponse> PasskeySetup(string ntid, int authenticationAmount, string currencyCode, string puid, string deviceId, EventTraceActivity traceActivityId, List<string> exposedFlightFeatures, object sessionContext, object browserData, string applicationUrl, string merchantName, string email);
    }
}
