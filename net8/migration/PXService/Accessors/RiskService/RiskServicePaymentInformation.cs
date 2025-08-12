// <copyright file="RiskServicePaymentInformation.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.RiskService.V7
{
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Newtonsoft.Json;

    public class RiskServicePaymentInformation
    {
        public RiskServicePaymentInformation(PaymentInstrument paymentInstrument)
            : this(paymentInstrument.PaymentMethod)
        {
        }

        public RiskServicePaymentInformation(PaymentMethod paymentMethod)
            : this(paymentMethod.PaymentMethodFamily, paymentMethod.PaymentMethodType)
        {
        }

        [JsonConstructor]
        public RiskServicePaymentInformation(string family, string type)
        {
            this.Id = null;
            this.PaymentMethodFamily = family;
            this.PaymentMethodType = type;
        }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "paymentMethodType")]
        public string PaymentMethodType { get; set; }

        [JsonProperty(PropertyName = "paymentMethodFamily")]
        public string PaymentMethodFamily { get; set; }

        [JsonProperty(PropertyName = "allowed")]
        public bool Allowed { get; set; }

        public override bool Equals(object obj)
        {
            RiskServicePaymentInformation rspiObj = obj as RiskServicePaymentInformation;
            if (rspiObj == null)
            {
                return false;
            }
            else
            {
                return this.PaymentMethodFamily.Equals(rspiObj.PaymentMethodFamily) && this.PaymentMethodType.Equals(rspiObj.PaymentMethodType);
            }
        }

        public override int GetHashCode()
        {
            return (this.PaymentMethodFamily + this.PaymentMethodType).GetHashCode();
        }
    }
}