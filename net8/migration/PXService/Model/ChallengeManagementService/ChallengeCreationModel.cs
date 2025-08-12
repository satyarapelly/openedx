// <copyright file="ChallengeCreationModel.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.ChallengeManagementService
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Web;
    using Newtonsoft.Json;
    using static Microsoft.Commerce.Payments.PXService.Model.ChallengeManagementService.ChallengeEnumDefinition;

    public class ChallengeCreationModel
    {
        [Required(ErrorMessage = "{0} is a mandatory field")]
        [MaxLength(36, ErrorMessage = "The {0} can not have more than {1} characters")]
        [JsonProperty(PropertyName = "session_id")]
        public string SessionId { get; set; }

        [Required(ErrorMessage = "{0} is a mandatory field")]
        [JsonProperty(PropertyName = "risk_score")]
        public int? RiskScore { get; set; }

        [JsonProperty(PropertyName = "audio")]
        public bool? Audio { get; set; }

        [Required(ErrorMessage = "{0} is a mandatory field")]
        [EnumDataType(typeof(ChallengeRequestor))]
        [JsonProperty(PropertyName = "challenge_requestor_name")]
        public string ChallengeRequestorName { get; set; }

        [MaxLength(36, ErrorMessage = "The {0} can not have more than {1} characters")]
        [EnumDataType(typeof(ChallengeProvider))]
        [JsonProperty(PropertyName = "challenge_provider_name")]
        public string ChallengeProviderName { get; set; }

        [JsonProperty(PropertyName = "max_attempts")]
        public long? MaxAttempts { get; set; }

        [JsonProperty(PropertyName = "session_length")]
        public long? SessionLength { get; set; }
    }
}