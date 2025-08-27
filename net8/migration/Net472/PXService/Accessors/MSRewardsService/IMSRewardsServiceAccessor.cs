// <copyright file="IMSRewardsServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.MSRewardsService
{
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.PXService.Accessors.MSRewardsService.DataModel;
    using Microsoft.Commerce.Tracing;

    public interface IMSRewardsServiceAccessor
    {
        Task<GetUserInfoResult> GetUserInfo(string userId, string country, EventTraceActivity traceActivityId);

        Task<RedemptionResult> RedeemRewards(string userId, string country, string partnerName, bool hasAnyPI, RedemptionRequest redemptionRequest, EventTraceActivity traceActivityId);
    }
}