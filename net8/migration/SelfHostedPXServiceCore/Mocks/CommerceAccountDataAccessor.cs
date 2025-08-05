// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2020. All rights reserved.</copyright>

namespace SelfHostedPXServiceCore.Mocks
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PXService;
    using Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel;
    using Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.Messages;
    using Microsoft.Commerce.Payments.Common.Tracing;
    
    public class CommerceAccountDataAccessor : ICommerceAccountDataAccessor
    {
        public static class BillingAccountId
        {
            public const string AzureBusinessAccount = "VFYAAAAAAAAAAAAB";
        }

        public Action<GetAccountInfoRequest> PreProcessGetAccountInfo { get; set; }

        public UpdateAccountRequest UpdateAccountRequest { get; set; }

        public CreateAccountResponse CreateAccount(CreateAccountRequest request, EventTraceActivity traceActivityId)
        {
            throw new NotImplementedException();
        }

        public GetAccountIdFromPaymentInstrumentInfoResponse GetAccountIdFromPaymentInstrumentInfo(GetAccountIdFromPaymentInstrumentInfoRequest request)
        {
            throw new NotImplementedException();
        }

        public GetAccountInfoResponse GetAccountInfo(GetAccountInfoRequest request, EventTraceActivity traceActivityId)
        {
            if (PreProcessGetAccountInfo != null)
            {
                PreProcessGetAccountInfo(request);
            }

            var accountInfoResponse = new GetAccountInfoResponse();
            accountInfoResponse.AccountList = new List<Account>();

            if (string.Equals(request.SearchCriteria?.AccountId, BillingAccountId.AzureBusinessAccount, StringComparison.OrdinalIgnoreCase))
            {
                accountInfoResponse.AccountList.Add(new PayinAccount()
                {
                    AccountID = BillingAccountId.AzureBusinessAccount,
                    AccountLevel = "Primary",
                    CountryCode = "us",
                    CustomerType = CustomerType.Business,
                    Status = AccountStatus.Active
                });
            }
            else if (!string.Equals(request.Requester.IdentityValue, "PuidOfNewUser", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(request.Requester.IdentityValue, "PuidWithNoLegacyBillableAccounts", StringComparison.OrdinalIgnoreCase))
            {
                accountInfoResponse.AccountList.Add(new Account()
                {
                    AccountID = "VFYAAAAAAAAAAAAA",
                    AccountLevel = "Primary",
                    CountryCode = "us",
                    Status = AccountStatus.Active
                });

                if (!string.Equals(request.Requester.IdentityValue, "PuidWithNoPayInAccounts", StringComparison.OrdinalIgnoreCase))
                {
                    accountInfoResponse.AccountList.Add(new PayinAccount()
                    {
                        AccountID = "VFYAAAAAAAAAAAAA",
                        AccountLevel = "Primary",
                        CountryCode = "us",
                        CustomerType = CustomerType.Personal,
                        Status = AccountStatus.Active
                    });
                }

                accountInfoResponse.AccountList.Add(new PayoutAccount()
                {
                    AccountID = "VFYAAAAAAAAAAAAA",
                    AccountLevel = "Primary",
                    CountryCode = "us",
                    Status = AccountStatus.Active
                });
            }

            return accountInfoResponse;
        }

        public UpdateAccountResponse UpdateAccount(UpdateAccountRequest request, EventTraceActivity traceActivityId)
        {
            UpdateAccountRequest = request;
            return new UpdateAccountResponse()
            {
                Account = request.Account
            };
        }
    }
}
