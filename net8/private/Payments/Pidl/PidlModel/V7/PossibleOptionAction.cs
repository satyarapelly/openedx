// <copyright file="PossibleOptionAction.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class PossibleOptionAction
    {
        public PossibleOptionAction(string displayText, DisplayHintAction pidlAction)
        {
            this.DisplayText = displayText;
            this.PidlAction = pidlAction;
        }

        [JsonProperty(PropertyName = "displayText")]
        public string DisplayText { get; set; }

        [JsonProperty(PropertyName = "pidlAction")]
        public DisplayHintAction PidlAction { get; set; }
    }
}
