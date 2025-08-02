// <copyright file="GetAccountIdFromPaymentInstrumentInfoResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.Messages
{
    using System.Collections.Generic;

    public class GetAccountIdFromPaymentInstrumentInfoResponse : AbstractResponse
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Legacy code moved from PCS. Needed for serialization")]
        public IList<string> AccountIdList { get; set; }
    }
}
