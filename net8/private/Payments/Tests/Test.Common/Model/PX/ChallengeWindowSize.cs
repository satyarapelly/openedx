// <copyright file="ChallengeWindowSize.cs" company="Microsoft">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace Tests.Common.Model.PX
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ChallengeWindowSize
    {
        // Width = "250px", Height = "400px"
        [EnumMember(Value = "01")]
        One,

        // Width = "390px", Height = "400px"
        [EnumMember(Value = "02")]
        Two,

        // Width = "500px", Height = "600px"
        [EnumMember(Value = "03")]
        Three,

        // Width = "600px", Height = "400px"
        [EnumMember(Value = "04")]
        Four,

        // Width = "100%", Height = "100%"
        [EnumMember(Value = "05")]
        Five
    }
}
