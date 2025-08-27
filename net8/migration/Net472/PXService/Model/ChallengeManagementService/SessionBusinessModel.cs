// <copyright file="SessionBusinessModel.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.ChallengeManagementService
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;
    using static Microsoft.Commerce.Payments.PXService.Model.ChallengeManagementService.SessionEnumDefinition;

    public class SessionBusinessModel
    {
        [MaxLength(36, ErrorMessage = "The {0} can not have more than {1} characters")]
        [JsonProperty(PropertyName = "session_id")]
        public string SessionId { get; set; }

        [Required(ErrorMessage = "{0} is a mandatory field")]
        [EnumDataType(typeof(SessionType))]
        [JsonProperty(PropertyName = "session_type")]
        public string SessionType { get; set; }

        [EnumDataType(typeof(SessionStatus))]
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [MaxLength(36, ErrorMessage = "The {0} can not have more than {1} characters")]
        [JsonProperty(PropertyName = "parent_session_id")]
        public string ParentSessionId { get; set; }

        [JsonProperty(PropertyName = "child_sessions")]
        public List<SessionBusinessModel> ChildSessions { get; }

        [JsonProperty(PropertyName = "session_data_hash")]
        public string SessionDataHash { get; set; }

        [JsonProperty(PropertyName = "session_data")]
        public string SessionData { get; set; }

        [Range(1, 150, ErrorMessage = "The session length should be between 0 and 150 min")]
        [JsonProperty(PropertyName = "session_length")]
        public int? SessionLength { get; set; }

        [JsonProperty(PropertyName = "session_sliding_expiration")]
        public bool? SessionSlidingExpiration { get; set; }

        [Range(typeof(DateTime), "01/01/1900", "01/01/2999", ErrorMessage = "Valid dates for the Property {0} between {1} and {2}")]
        [JsonProperty(PropertyName = "session_expires_at")]
        public DateTime? SessionExpiresAt { get; set; }

        [MaxLength(100, ErrorMessage = "The {0} can not have more than {1} characters")]
        [JsonProperty(PropertyName = "created_by")]
        public string CreatedBy { get; set; }

        [MaxLength(100, ErrorMessage = "The {0} can not have more than {1} characters")]
        [JsonProperty(PropertyName = "updated_by")]
        public string UpdatedBy { get; set; }

        [Range(typeof(DateTime), "01/01/1900", "01/01/2999", ErrorMessage = "Valid dates for the Property {0} between {1} and {2}")]
        [JsonProperty(PropertyName = "created_date")]
        public DateTime? CreatedDate { get; set; }

        [Range(typeof(DateTime), "01/01/1900", "01/01/2999", ErrorMessage = "Valid dates for the Property {0} between {1} and {2}")]
        [JsonProperty(PropertyName = "updated_date")]
        public DateTime? UpdatedDate { get; set; }
    }
}