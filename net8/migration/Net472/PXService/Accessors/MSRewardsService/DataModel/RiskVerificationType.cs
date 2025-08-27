// <copyright file="RiskVerificationType.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>
// Sourced these classes from : https://msasg.visualstudio.com/rewards/_git/M.Rewards_Platform?path=/Models/Models/RequestModels.cs&version=GBcontainers&_a=contents

namespace Microsoft.Commerce.Payments.PXService.Accessors.MSRewardsService.DataModel
{
    using System.Text.Json.Serialization;
    using Newtonsoft.Json.Converters;

    // https://msasg.visualstudio.com/rewards/_git/M.Rewards_Platform?path=/Models/Models/RiskModels.cs&_a=contents&version=GBcontainers
    // IMPORTANT!!! We must keep the explicit enum values unchanged. Even if we add new enum values in the middle,
    // we must not change the existing values. We cannot reclaim the value when any of these enum values are deleted.
    // It is because these will be serialized into storage. If values change, the deserialization will be off.
    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
    [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
    public enum RiskVerificationType
    {
        /// <summary>
        /// this shouldn't happen
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// send a SMS text message
        /// </summary>
        SMS = 1,

        /// <summary>
        /// call the user
        /// </summary>
        Call = 2,
    }
}