// <copyright file="UserInfo.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using Newtonsoft.Json;

    public class UserInfo
    {
        public UserInfo()
        {
            // Default this to the type PUID
            this.UserIdType = "PUID";
        }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "userIdType")]
        public string UserIdType { get; set; }
    }
}