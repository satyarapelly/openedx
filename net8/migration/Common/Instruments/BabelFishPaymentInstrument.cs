// <copyright file="BabelFishPaymentInstrument.cs" company="Microsoft">Copyright (c) Microsoft 2017. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Instruments
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    public class BabelFishPaymentInstrument : PaymentInstrument
    {
        public BabelFishPaymentInstrument()
            : base(PaymentMethodRegistry.BabelFish)
        {
        }

        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public IDictionary<string, object> Details { get; set; }
    }
}
