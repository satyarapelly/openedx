// <copyright file="Generate3DS2ChallengePIDLResource.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.V7.Contexts;
    using Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model;
    using Microsoft.Commerce.Tracing;
    
    public delegate Task<PIDLResource> Generate3DS2ChallengePIDLResource(
        PaymentSessionData paymentSessionData,
        RequestContext requestContext,
        EventTraceActivity traceActivityId,
        PaymentExperienceSetting setting);
}