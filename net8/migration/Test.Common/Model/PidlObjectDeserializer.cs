// <copyright file="PidlObjectDeserializer.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Pidl;

    public class PidlObjectDeserializer : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(object);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jToken = JToken.Load(reader);
            object target = jToken;

            if (jToken.Type == JTokenType.Array)
            {
                var jArray = jToken as JArray;
                var firstElement = jArray[0] as JObject;
                if (firstElement != null && firstElement.ContainsKey("identity") && firstElement["identity"] != null)
                {
                    target = new List<PIDLResource>();
                }
            }
            else if (jToken.Type == JTokenType.Object)
            {
                var jObject = jToken as JObject;
                if (jObject.ContainsKey("propertyType") && jObject["propertyType"] != null)
                {
                    target = new PropertyDescription();
                }
                else if (jObject.ContainsKey("identity") && jObject["identity"] != null)
                {
                    target = new PIDLResource();
                }
            }

            if (target != jToken)
            {
                serializer.Populate(jToken.CreateReader(), target);
            }

            return target;
        }
    }
}