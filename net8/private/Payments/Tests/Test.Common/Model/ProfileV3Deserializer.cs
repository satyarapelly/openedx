// <copyright file="ProfileV3Deserializer.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model
{
    using System;
    using Accounts;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class ProfileV3Deserializer : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ProfileV3);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var profileObject = JObject.Load(reader);
            ProfileV3 profile = CreateNewProfile(profileObject);
            serializer.Populate(profileObject.CreateReader(), profile);
            return profile;
        }

        private static ProfileV3 CreateNewProfile(JObject profileObject)
        {
            switch (profileObject["type"].ToString())
            {
                case "employee":
                    return new EmployeeProfileV3();
                case "organization":
                    return new OrganizationProfileV3();
                case "consumer":
                    return new ConsumerProfileV3();
                default:
                    return null;
            }
        }
    }
}