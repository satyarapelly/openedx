// <copyright file="MobileCarrierBillingPaymentInstrument.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Instruments
{
    public class MobileCarrierBillingPaymentInstrument : PaymentInstrument
    {
        public MobileCarrierBillingPaymentInstrument()
            : base(PaymentMethodRegistry.MobileCarrierBilling)
        {
        }

        // TODO, Remove after billing integration.
        public string Puid { get; set; }

        public string IccId { get; set; }

        // TODO, Remove after billing integration.
        public string MobileNumber { get; set; }

        public string MobileOperatorId { get; set; }

        public string DeviceIds { get; set; }
    }
}
