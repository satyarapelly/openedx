// <copyright file="IPayerAuthServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using PayerAuth = Model.PayerAuthService;

    public interface IPayerAuthServiceAccessor
    {
        // PayerAuth.V3 APIs
        Task<PayerAuth.PaymentSessionResponse> CreatePaymentSessionId(PayerAuth.PaymentSessionData paymentSessionData, EventTraceActivity traceActivityId);

        Task<PayerAuth.AuthenticationResponse> Authenticate(PayerAuth.AuthenticationRequest authRequest, EventTraceActivity traceActivityId);

        Task<PayerAuth.AuthenticationResponse> AuthenticateThreeDSOne(PayerAuth.AuthenticationRequest authRequest, EventTraceActivity traceActivityId);

        Task<PayerAuth.CompletionResponse> CompleteChallenge(PayerAuth.CompletionRequest completionRequest, EventTraceActivity traceActivityId);

        Task<PayerAuth.ThreeDSMethodData> Get3DSMethodURL(PayerAuth.PaymentSession paymentSession, EventTraceActivity traceActivityId);
    }
}