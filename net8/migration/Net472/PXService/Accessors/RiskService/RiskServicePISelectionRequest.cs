// <copyright file="RiskServicePISelectionRequest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.RiskService.V7
{
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Newtonsoft.Json;

    public class RiskServicePISelectionRequest
    {
        public RiskServicePISelectionRequest(string puid, string client, string purchaseApiVersion, string orderId, string sessionId, IList<RiskServicePaymentInformation> paymentInfo)
            : this(puid, client, purchaseApiVersion, orderId, sessionId)
        {
            this.PaymentInfo = paymentInfo;
        }

        public RiskServicePISelectionRequest(string puid, string client, string purchaseApiVersion, string orderId, string sessionId, IList<PaymentMethod> paymentMethods)
            : this(puid, client, purchaseApiVersion, orderId, sessionId)
        {
            this.PaymentInfo = new List<RiskServicePaymentInformation>();
            foreach (PaymentMethod pm in paymentMethods)
            {
                RiskServicePaymentInformation rspi = new RiskServicePaymentInformation(pm);
                this.PaymentInfo.Add(rspi);
            }
        }

        public RiskServicePISelectionRequest(string puid, string client, string purchaseApiVersion, string orderId, string sessionId, IList<PaymentInstrument> paymentInstruments)
            : this(puid, client, purchaseApiVersion, orderId, sessionId)
        {
            ISet<RiskServicePaymentInformation> rspis = new HashSet<RiskServicePaymentInformation>();
            foreach (PaymentInstrument pi in paymentInstruments)
            {
                RiskServicePaymentInformation rspi = new RiskServicePaymentInformation(pi);
                rspis.Add(rspi);
            }

            this.PaymentInfo = new List<RiskServicePaymentInformation>(rspis);
        }

        private RiskServicePISelectionRequest(string puid, string client, string purchaseApiVersion, string orderId, string sessionId)
        {
            this.Puid = puid;
            this.Client = client;
            this.PurchaseApiVersion = purchaseApiVersion;
            this.OrderId = orderId;
            this.SessionId = sessionId;
        }

        [JsonProperty(PropertyName = "puid")]
        public string Puid { get; set; }

        [JsonProperty(PropertyName = "client")]
        public string Client { get; set; }

        [JsonProperty(PropertyName = "purchase_api_version")]
        public string PurchaseApiVersion { get; set; }

        [JsonProperty(PropertyName = "order_id")]
        public string OrderId { get; set; }

        [JsonProperty(PropertyName = "session_id")]
        public string SessionId { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:Collection properties should be read only", Justification = "Needs to be writeable so the JSON serializer can run")]
        [JsonProperty(PropertyName = "payment_info")]
        public IList<RiskServicePaymentInformation> PaymentInfo { get; set; }
    }
}