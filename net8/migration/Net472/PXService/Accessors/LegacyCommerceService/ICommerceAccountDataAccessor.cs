// <copyright file="ICommerceAccountDataAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{    
    using Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.Messages;
    using Microsoft.Commerce.Tracing;

    public interface ICommerceAccountDataAccessor
    {
        GetAccountInfoResponse GetAccountInfo(GetAccountInfoRequest request, EventTraceActivity traceActivityId);
        CreateAccountResponse CreateAccount(CreateAccountRequest request, EventTraceActivity traceActivityId);
        UpdateAccountResponse UpdateAccount(UpdateAccountRequest request, EventTraceActivity traceActivityId);
        GetAccountIdFromPaymentInstrumentInfoResponse GetAccountIdFromPaymentInstrumentInfo(GetAccountIdFromPaymentInstrumentInfoRequest request);
    }
}