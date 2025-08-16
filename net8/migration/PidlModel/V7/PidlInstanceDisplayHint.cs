// <copyright file="PidlInstanceDisplayHint.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Class for describing a Pidl Instance DisplayHint.
    /// This desplay hint used to trigger pidl resource by user action like when user select the select pm options. 
    /// </summary>
    public sealed class PidlInstanceDisplayHint : ContentDisplayHint
    {
        public PidlInstanceDisplayHint()
        {
        }

        public PidlInstanceDisplayHint(PidlInstanceDisplayHint template, Dictionary<string, string> contextTable)
            : base(template, contextTable)
        {
        }

        /// <summary>
        /// Gets or sets the PIDL instance key from the payment methods selection operation key. Sample values (credit_cart.visa, ewallet.paypal, list_pi)
        /// </summary>
        [JsonProperty(PropertyName = "pidlInstance")]
        public string PidlInstance { get; set; }

        /// <summary>
        /// Gets or sets the trigger submit order sample value (beforeBase)
        /// </summary>
        [JsonProperty(PropertyName = "triggerSubmitOrder")]
        public string TriggerSubmitOrder { get; set; }

        protected override string GetDisplayType()
        {
            return HintType.PidlInstance.ToString().ToLower();
        }
    }
}