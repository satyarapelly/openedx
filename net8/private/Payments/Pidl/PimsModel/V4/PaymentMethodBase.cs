// <copyright file="PaymentMethodBase.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    using Newtonsoft.Json;

    /// <summary>
    /// This is the base class for PaymentMethod and PaymentMethodFamily classes since payment-methods
    /// shown to the end user could be a mix of PaymentMethod/s and PaymentMethodFamily/ies
    /// </summary>
    public class PaymentMethodBase
    {
        public PaymentMethodBase()
        {
            this.Display = new PaymentInstrumentDisplayDetails
            {
                Name = string.Empty,
                Logo = string.Empty
            };
        }

        public PaymentMethodBase(PaymentMethodBase template)
        {
            this.PaymentMethodFamily = template.PaymentMethodFamily;
            this.Display = new PaymentInstrumentDisplayDetails
            {
                Name = template.Display.Name,
                Logo = template.Display.Logo
            };
        }

        [JsonProperty(PropertyName = "paymentMethodFamily")]
        public string PaymentMethodFamily { get; set; }

        [JsonProperty(PropertyName = "display")]
        public PaymentInstrumentDisplayDetails Display { get; set; }

        public string AdditionalDisplayText { get; set; }
    }
}