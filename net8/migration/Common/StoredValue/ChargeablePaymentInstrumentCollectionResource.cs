// <copyright file="chargeablePaymentInstrumentCollectionResource.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.StoredValue
{
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.Common.Web;
    using Newtonsoft.Json;

    /// <summary>
    /// ChargeablePaymentInstrumentCollectionResource is a copy of stored value core resource
    /// </summary>
    public class ChargeablePaymentInstrumentCollectionResource
    {
        private List<ChargeablePaymentInstrumentResource> chargeablePaymentInstruments;

        public ChargeablePaymentInstrumentCollectionResource()
        {
            this.chargeablePaymentInstruments = new List<ChargeablePaymentInstrumentResource>();
        }

        [JsonProperty("chargeable_payment_instruments")]
        public List<ChargeablePaymentInstrumentResource> ChargeablePaymentInstruments 
        {
            get
            {
                return this.chargeablePaymentInstruments;
            }
        }
    }
}