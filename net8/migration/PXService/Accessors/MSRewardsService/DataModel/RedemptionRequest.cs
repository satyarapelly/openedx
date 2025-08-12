// <copyright file="RedemptionRequest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>
// Sourced these classes from : https://msasg.visualstudio.com/rewards/_git/M.Rewards_Platform?path=/Models/Models/RequestModels.cs&version=GBcontainers&_a=contents

namespace Microsoft.Commerce.Payments.PXService.Accessors.MSRewardsService.DataModel
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text.Json.Serialization;
    using Newtonsoft.Json;

    public class RedemptionRequest
    {
        [JsonProperty("id")]
        public string OrderId { get; set; }

        [JsonProperty("attributes")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public IDictionary<string, string> OrderAttributes { get; set; }

        [JsonProperty("headers")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public IDictionary<string, string> RequestHeaders { get; set; }

        [JsonProperty("risk_context")]
        public RiskOrderContext RiskContext { get; set; }

        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("child_redemption")]
        public RedemptionRequest ChildRedemptionRequest { get; set; }

        [JsonProperty("item")]
        public string CatalogItem { get; set; }

        [JsonProperty("variable_redemption_request")]
        public VariableRedemptionItemRequestDetails VariableRedemptionRequest { get; set; }

        [JsonProperty("sms_chg_details")]
        public ForceTriggerSmsChallengeDetails ForceTriggerSmsChallengeDetails { get; set; }

        [JsonProperty("trigger_captcha_logic")]
        public bool TriggerCaptchaLogic { get; set; }

        [JsonProperty("phone_number_on_chg_first")]
        public bool PhoneNumberOnChallengeFirst { get; set; }

        [JsonProperty("phone_number_on_verification")]
        public bool IsPhoneNumberOnVerificationCodeRequest { get; set; }
    }
}