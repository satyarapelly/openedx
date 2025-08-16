// <copyright file="PaymentInstrumentAction.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    public class PaymentInstrumentAction
    {
        public PaymentInstrumentAction(string actionName, string displayText)
        {
            this.ActionName = actionName;
            this.DisplayText = displayText;
        }

        [JsonProperty(PropertyName = "actionName")]
        public string ActionName { get; set; }

        [JsonProperty(PropertyName = "displayText")]
        public string DisplayText { get; set; }
    }
}
