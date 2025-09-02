// <copyright file="ValidationParameters.cs" company="Microsoft">Copyright (c) Microsoft  All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.TransactionService
{
    using Microsoft.Commerce.Payments.Common.Transaction;
    using Newtonsoft.Json;

    /// <summary>
    /// Request resource for validate
    /// </summary>
    public class ValidationParameters
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationParameters" /> class
        /// </summary>
        public ValidationParameters()
        {
        }

        /// <summary>
        /// Gets or sets the country code that where the request is originally from
        /// </summary>
        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the amount that where the request is originally from
        /// </summary>
        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the currency for this transaction
        /// </summary>
        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        /// <summary>
        /// Gets or sets the store that where the request is originally from
        /// </summary>
        [JsonProperty(PropertyName = "store")]
        public string Store { get; set; }

        /// <summary>
        /// Gets or sets the payment instrument Id
        /// </summary>
        [JsonProperty(PropertyName = "payment_instrument")]
        public string PaymentInstrument { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this transaction is categorized as commercial transaction.
        /// </summary>
        [JsonProperty(PropertyName = "commercial_transaction")]
        public bool CommercialTransaction { get; set; }

        /// <summary>
        /// Gets or sets the session id
        /// </summary>
        [JsonProperty(PropertyName = "session_id")]
        public string SessionId { get; set; }

        /// <summary>
        /// Gets or sets the authentication data for downstream processor, like FDC processor
        /// </summary>
        [JsonProperty(PropertyName = "authentication_data")]
        public AuthenticationData AuthenticationData { get; set; }

        /// <summary>
        /// Gets or sets the risk token
        /// </summary>
        [JsonProperty(PropertyName = "risk_token")]
        public string RiskToken { get; set; }

        /// <summary>
        /// Gets or sets the device type
        /// </summary>
        [JsonProperty(PropertyName = "device_type")]
        public string DeviceType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not this validation is an update instead of a new payment instrument
        /// </summary>
        [JsonProperty(PropertyName = "update_validation")]
        public bool UpdateValidation { get; set; }

        /// <summary>
        /// Gets or sets the customer's IP Address
        /// </summary>
        [JsonProperty("ip_address")]
        public string IpAddress { get; set; }

        /// <summary>
        /// Gets or sets the customer's User Agent
        /// </summary>
        [JsonProperty("user_agent")]
        public string UserAgent { get; set; }
    }
}