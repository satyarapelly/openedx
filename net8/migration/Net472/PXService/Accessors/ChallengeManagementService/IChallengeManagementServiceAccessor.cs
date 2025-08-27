// <copyright file="IChallengeManagementServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.ChallengeManagementService
{
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.PXService.Model.ChallengeManagementService;
    using Microsoft.Commerce.Tracing;
    using static Microsoft.Commerce.Payments.PXService.Model.ChallengeManagementService.SessionEnumDefinition;

    public interface IChallengeManagementServiceAccessor
    {
        Task<string> CreateChallenge(string sessionId, EventTraceActivity traceActivityId, string language, int riskScore, string challengeProvider);

        Task<ChallengeStatusResult> GetChallengeStatus(string sessionId, EventTraceActivity traceActivityId);

        Task<object> SubmitChallenge();

        Task<SessionBusinessModel> CreateChallengeSession(string sessionData, EventTraceActivity traceActivityId);

        Task<SessionBusinessModel> UpdateChallengeSession(SessionBusinessModel sessionRequest, EventTraceActivity traceActivityId);

        Task<SessionBusinessModel> GetChallengeSession(string sessionId, EventTraceActivity traceActivityId);
    }
}
