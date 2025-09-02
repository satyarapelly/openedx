// <copyright file="PidlPayload.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXCommon;
    using Newtonsoft.Json;

    public class PidlPayload
    {
        private readonly List<PaymentInstrument> paymentInstruments;
        private readonly PidlInfo pidlInfo;

        public PidlPayload(List<PaymentInstrument> paymentInstruments, PidlInfo pidlInfo)
        {
            this.paymentInstruments = paymentInstruments;
            this.pidlInfo = pidlInfo;
        }

        [JsonProperty(Order = 0, PropertyName = "paymentInstruments")]
        public List<PaymentInstrument> PaymentInstruments
        {
            get
            {
                return this.paymentInstruments;
            }
        }

        [JsonProperty(Order = 1, PropertyName = "pidlInfo")]
        public PidlInfo PidlInfo
        {
            get
            {
                return this.pidlInfo;
            }
        }
    }
}
