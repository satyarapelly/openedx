// <copyright file="PaymentMethodFamily.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model.Pims
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a family of PaymentMethods.  e.g. Credit cards.
    /// </summary>
    public class PaymentMethodFamily : PaymentMethodBase
    {
        // PaymentMethods that belong to a PaymentMethodFamily will not be initialized
        // at config-load time because it may vary based on the country parameter.  Hence,
        // it is determined at request-time (by considering the country parameter
        // of the request).
        private List<PaymentMethod> paymentMethods;

        public PaymentMethodFamily() : base()
        {
        }

        public PaymentMethodFamily(PaymentMethodFamily template)
            : base(template)
        {
            if (null != template.PaymentMethods)
            {
                this.paymentMethods = new List<PaymentMethod>(template.PaymentMethods);
            }
        }

        [JsonProperty(Order = 0, PropertyName = "payment_methods")]
        public List<PaymentMethod> PaymentMethods
        {
            get
            {
                return this.paymentMethods;
            }
        }

        public void AddPaymentMethod(PaymentMethod paymentMethodToAdd)
        {
            if (this.paymentMethods == null)
            {
                this.paymentMethods = new List<PaymentMethod>();
            }

            this.paymentMethods.Add(paymentMethodToAdd);
        }
    }
}