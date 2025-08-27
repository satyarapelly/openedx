// <copyright file="TransactionResource.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.TransactionService
{
    using Microsoft.Commerce.Payments.Common.Transaction;
    using Newtonsoft.Json;

    // The return object of the TransactionService Validate call
    public class TransactionResource
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "payment_instrument")]
        public string PaymentInstrument { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "amount_received")]
        public decimal AmountReceived { get; set; }

        [JsonProperty(PropertyName = "target_amount")]
        public decimal TargetAmount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }

        [JsonProperty(PropertyName = "merchant_reference_number")]
        public string MerchantReferenceNumber { get; set; }

        [JsonProperty(PropertyName = "merchant_id")]
        public string MerchantId { get; set; }

        [JsonProperty(PropertyName = "type")]
        public TransactionType TransactionType { get; set; }

        [JsonProperty(PropertyName = "status")]
        public TransactionStatus Status { get; set; }

        [JsonProperty(PropertyName = "status_details")]
        public TransactionStatusDetail StatusDetails { get; set; }

        [JsonProperty(PropertyName = "external_reference")]
        public ExternalReference ExternalReference { get; set; }

        [JsonProperty(PropertyName = "redirect_url")]
        public string RedirectUrl { get; set; }
    }
}
