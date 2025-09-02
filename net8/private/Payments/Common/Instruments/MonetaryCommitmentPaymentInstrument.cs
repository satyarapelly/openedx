// <copyright file="MonetaryCommitmentPaymentInstrument.cs" company="Microsoft">Copyright (c) Microsoft 2017. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Instruments
{
    using Newtonsoft.Json;

    public class MonetaryCommitmentPaymentInstrument : PaymentInstrument
    {
        public MonetaryCommitmentPaymentInstrument()
            : base(PaymentMethodRegistry.MonetaryCommitment)
        {
        }

        [JsonProperty("balanceId")]
        public string BalanceId { get; set; }

        [JsonProperty("unitType")]
        public string UnitType { get; set; }
    }
}
