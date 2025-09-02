// <copyright file="RiskEligibilityRequest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.RiskService
{
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXService.RiskService.V7;
    using Newtonsoft.Json;

    public class RiskEligibilityRequest
    {
        public RiskEligibilityRequest(string client, string puid, string tid, string oid, string ipAddress, string locale, string deviceType, IList<PaymentMethod> paymentMethods)
        {
            RiskEligibilityAccountDetails riskEligibilityAccountDetails = new RiskEligibilityAccountDetails()
            {
                Id = puid,
                IdNameSpace = string.Empty,
                TenantId = tid,
                ObjectId = oid
            };

            this.InitializeRiskEligibilityEventDetails(client, ipAddress, locale, deviceType, paymentMethods, riskEligibilityAccountDetails);
        }

        public RiskEligibilityRequest(string client, string puid, string tid, string oid, string idNameSpace, string commerceRootId, string orgId, string ipAddress, string locale, string deviceType, IList<PaymentMethod> paymentMethods)
        {
            RiskEligibilityAccountDetails riskEligibilityAccountDetails = new RiskEligibilityAccountDetails()
            {
                Id = puid,
                IdNameSpace = idNameSpace,
                TenantId = tid,
                ObjectId = oid,
                AccountId = commerceRootId,
                ExternalUserId = orgId,
            };

            this.InitializeRiskEligibilityEventDetails(client, ipAddress, locale, deviceType, paymentMethods, riskEligibilityAccountDetails);
        }

        [JsonProperty(PropertyName = "event_type")]
        public string EventType { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:Collection properties should be read only", Justification = "Needs to be writeable so the JSON serializer can run")]
        [JsonProperty(PropertyName = "event_details")]
        public RiskEligibilityEventDetails EventDetails { get; set; }

        private void InitializeRiskEligibilityEventDetails(string client, string ipAddress, string locale, string deviceType, IList<PaymentMethod> paymentMethods, RiskEligibilityAccountDetails riskEligibilityAccountDetails)
        {
            List<RiskServiceRequestPaymentInstrument> riskServiceRequestPaymentInstruments = new List<RiskServiceRequestPaymentInstrument>();
            foreach (PaymentMethod pm in paymentMethods)
            {
                RiskServiceRequestPaymentInstrument riskServiceRequestPaymentInstrument = new RiskServiceRequestPaymentInstrument()
                {
                    PaymentInstrumentFamily = pm.PaymentMethodFamily,
                    PaymentInstrumentType = pm.PaymentMethodType
                };
                riskServiceRequestPaymentInstruments.Add(riskServiceRequestPaymentInstrument);
            }

            var deviceDetails = new DeviceDetails
            {
                IpAddress = ipAddress,
                DeviceType = deviceType,
                Locale = locale,
            };

            RiskEligibilityEventDetails riskEligibilityEventDetails = new RiskEligibilityEventDetails()
            {
                AccountDetails = riskEligibilityAccountDetails,
                DeviceDetails = deviceDetails,
                PaymentInstrumentType = riskServiceRequestPaymentInstruments,
                Client = client,
            };

            this.EventType = "pi_type_customer_eligibility";
            this.EventDetails = riskEligibilityEventDetails;
        }
    }
}