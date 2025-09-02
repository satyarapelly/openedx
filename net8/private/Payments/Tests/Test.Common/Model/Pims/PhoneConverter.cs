// <copyright file="PhoneConverter.cs" company="Microsoft">Copyright (c) Microsoft 2019. All rights reserved.</copyright>

namespace Tests.Common.Model.Pims
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Ignores Read / Write of Phone property if it is not of type string. Phone property can be an object (with areaCode, localNumber, extension, country) 
    /// in the case of Legacy Invoice PI, the whole Phone object can be ignored in PX flows.
    /// </summary>
    public class PhoneConverter : JsonConverter
    {
        public PhoneConverter()
        {
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (objectType != typeof(string))
            {
                throw new JsonSerializationException(string.Format("PhoneConverter cannot deserialize '{0}' values", objectType.Name));
            }

            if (reader.TokenType == JsonToken.StartObject)
            {
                // Eat up the phone object that was sent as part of Legacy Invoice
                Newtonsoft.Json.Linq.JObject.Load(reader);

                return null;
            }
            else if (reader.TokenType == JsonToken.String)
            {
                return reader.Value;
            }
            else
            {
                return null;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value != null && value is string)
            {
                writer.WriteValue(value.ToString());
            }
        }
    }
}
