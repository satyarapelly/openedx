// <copyright file="PartnerHints.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
    using Newtonsoft.Json;

    /// <summary>
    /// PartnerHints is a child of ActionContext, contains context the partner would need for completing the next PIDL action
    /// i.e. the next PIDL action needs to be displayed as a popup (as in Paypal CIB to MIB), so "placement" will be equal to "popup"
    /// </summary>
    public class PartnerHints
    {
        public PartnerHints()
        {
        }

        [JsonProperty(PropertyName = "placement")]
        public string Placement { get; set; }

        [JsonProperty(PropertyName = "submitMethod")]
        public string SubmitMethod { get; set; }
    }
}