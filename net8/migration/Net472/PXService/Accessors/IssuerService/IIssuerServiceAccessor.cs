// <copyright file="IIssuerServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PXService
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.PXService.Model.IssuerService;

    public interface IIssuerServiceAccessor
    {
        Task<ApplyResponse> Apply(string customerPuid, ApplyRequest applyRequest);

        Task<List<Application>> ApplicationDetails(string accountId, string cardProduct, string sessionId);

        Task<EligibilityResponse> Eligibility(string customerPuid, string cardProduct);

        Task<InitializeResponse> Initialize(InitializeRequest applyRequest);
    }
}