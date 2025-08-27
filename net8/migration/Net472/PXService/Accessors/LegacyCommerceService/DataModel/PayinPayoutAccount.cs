// <copyright file="PayinPayoutAccount.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel
{
    public class PayinPayoutAccount
    {
        public string AccountID
        {
            get;
            set;
        }

        public PayinAccount PayinAccount
        {
            get;
            set;
        }

        public PayoutAccount PayoutAccount
        {
            get;
            set;
        }
    }
}
