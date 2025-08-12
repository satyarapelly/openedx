// <copyright file="VariableRedemptionItemRequestDetails.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>
// Sourced these classes from : https://msasg.visualstudio.com/rewards/_git/M.Rewards_Platform?path=/Models/Models/RequestModels.cs&version=GBcontainers&_a=contents

namespace Microsoft.Commerce.Payments.PXService.Accessors.MSRewardsService.DataModel
{
    using System.Text.Json.Serialization;
    using Newtonsoft.Json;

    public class VariableRedemptionItemRequestDetails
    {
        [JsonProperty("variable_amount")]
        public int VariableAmount { get; set; }
    }
}