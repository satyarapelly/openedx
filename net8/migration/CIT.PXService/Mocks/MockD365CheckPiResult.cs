namespace CIT.PXService.Mocks
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;
    using Microsoft.Commerce.Payments.PXService.Model.D365Service;

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
