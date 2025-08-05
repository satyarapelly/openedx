namespace SelfHostedPXServiceCore.Mocks
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Commerce.Payments.PXService.Model.D365Service;
    using Newtonsoft.Json;

    public class MockD365CheckPiResult
    {
        [JsonProperty("paymentInstrumentId")]
        public string PaymentInstrumentId { get; set; }

        [JsonProperty("orderIds")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public List<string> OrderIds { get; set; }

        public PaymentInstrumentCheckResponse GetPaymentInstrumentCheckResponse()
        {
            return new PaymentInstrumentCheckResponse
            {
                PendingOrderIds = this.OrderIds
            };
        }
    }
}
