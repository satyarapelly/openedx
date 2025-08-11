// <copyright file="DirectDebitPaymentInstrumentSetupInfo.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Instruments
{
    using System;

    public class DirectDebitPaymentInstrumentSetupInfo
    {
        // Original TrackgingGuid of the add payment instrument step
        public Guid TrackingId { get; set; }

        // Redirect Url for customer to do Docusign Verification
        public string DocuSignRedirectUrl { get; set; }

        // PaymentMethodType information used in Callback Api of Redirection Service
        public string PaymentMethodType { get; set; }

        // BillableAccountId is used for PIMS call legacy UpdatePI for confirming the PICV challenge
        public string BillableAccountId { get; set; }

        // RequesterIdentityType is used for PIMS call legacy UpdatePI for confirming the PICV challenge
        public string RequesterIdentityType { get; set; }

        // RequesterIdentityValue is used for PIMS call legacy UpdatePI for confirming the PICV challenge
        public string RequesterIdentityValue { get; set; }

        // CommerceAccountId is used for building the RDS callback URL
        public string CommerceAccountId { get; set; }

        // PaymentInstrumentId is used for building the RDS callback URL
        public string PaymentInstrumentId { get; set; }
    }
}
