// <copyright file="StoredValuePaymentInstrument.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Instruments
{
    using Newtonsoft.Json;

    public class StoredValuePaymentInstrument : PaymentInstrument
    {
        public StoredValuePaymentInstrument()
            : base(PaymentMethodRegistry.StoredValue)
        {
        }

        [JsonProperty(PropertyName = "stored_value_account_owner_id")]
        public long StoredValueAccountOwnerId { get; set; }

        [JsonProperty(PropertyName = "stored_value_account_id")]
        public long StoredValueAccountId { get; set; }

        [JsonProperty(PropertyName = "chargeable_payment_instrument_id")]
        public int ChargeablePaymentInstrumentId { get; set; }
    }
}
