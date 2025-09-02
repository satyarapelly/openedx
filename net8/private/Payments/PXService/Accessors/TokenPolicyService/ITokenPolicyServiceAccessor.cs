// <copyright file="ITokenPolicyServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.TokenPolicyService
{
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.PXService.Accessors.TokenPolicyService.DataModel;

    public interface ITokenPolicyServiceAccessor
    {
        Task<TokenPolicyDescription> GetTokenDescriptionAsync(string puid, string tokenValue, string market, string language, string clientIP, EventTraceActivity traceActivityId);
    }
}