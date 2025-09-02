// <copyright file="PaymentInstrument.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Instruments
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the base payment instrument
    /// </summary>
    public abstract class PaymentInstrument
    {
        protected PaymentInstrument(PaymentMethod paymentMethod)
            : this(paymentMethod, new Dictionary<string, string>())
        {
        }

        protected PaymentInstrument(PaymentMethod paymentMethod, Dictionary<string, string> properties)
        {
            this.PaymentMethod = paymentMethod;
            this.Properties = properties == null ? new Dictionary<string, string>() : properties;
        }

        /// <summary>
        /// Gets the payment method from transaction service wrapper.
        /// </summary>
        public PaymentMethod PaymentMethod { get; private set; }

        /// <summary>
        /// Gets or sets the payment_method_type from PIMS
        /// </summary>
        public string PaymentMethodType { get; set; }

        /// <summary>
        /// Gets or sets the payment_method_family from PIMS
        /// </summary>
        public string PaymentMethodFamily { get; set; }

        [JsonProperty(PropertyName = "status")]
        public PaymentInstrumentStatus Status { get; set; }

        public PaymentInstrumentOwnerType OwnerType { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is the legacy representation of ownerId and is not serialized")]
        public byte[] OwnerId { get; set; }

        public BanTypes BanType { get; set; }

        public int BanReasonCode { get; set; }

        public string Id { get; set; }

        public DateTime InsertedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }

        public long RegisteredPaymentMethodId { get; set; }

        public Dictionary<string, string> Properties { get; private set; }

        /// <summary>
        /// Gets the sub type from transaction service wrapper.
        /// </summary>
        public virtual string SubType
        {
            get
            {
                return null;
            }
        }
    }
}
