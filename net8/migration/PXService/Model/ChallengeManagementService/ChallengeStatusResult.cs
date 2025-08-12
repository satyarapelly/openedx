// <copyright file="ChallengeStatusResult.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.ChallengeManagementService
{
    using Newtonsoft.Json;

    public class ChallengeStatusResult
    {
        [JsonProperty(PropertyName = "passed")]
        public bool Passed { get; set; }
    }
}