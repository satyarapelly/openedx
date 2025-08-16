// <copyright file="ButtonDisplayHint.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Class for describing a Button DisplayHint
    /// </summary>
    public sealed class ButtonDisplayHint : ContentDisplayHint
    {
        public ButtonDisplayHint()
        { 
        }

        public ButtonDisplayHint(ButtonDisplayHint template, Dictionary<string, string> contextTable)
            : base(template, contextTable)
        {
        }

        [JsonProperty(PropertyName = "tooltipText")]
        public string TooltipText
        {
            get;
            set;
        }

        protected override string GetDisplayType()
        {
            return HintType.Button.ToString().ToLower();
        }
    }
}