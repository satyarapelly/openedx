// <copyright file="EnumJsonConverter.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Web
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;

    /// <summary>
    /// Formats the Enum values to the format that we follow across commerce.
    /// </summary>
    public class EnumJsonConverter : JsonConverter
    {
        public EnumJsonConverter()
        {   
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsEnum;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (!objectType.IsEnum)
            {
                throw new JsonSerializationException(string.Format("EnumJsonConverter cannot deserialize '{0}' values", objectType.Name));
            }

            if (reader.TokenType == JsonToken.String)
            {
                return Enum.Parse(objectType, JscriptToPascalCase(reader.Value.ToString()));
            }
            else if (reader.TokenType == JsonToken.Integer)
            {
                return Enum.ToObject(objectType, reader.Value);
            }
            else
            {
                throw new JsonSerializationException(string.Format("EnumJsonConverter cannot deserialize '{0}' values", reader.TokenType));
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value != null)
            {
                writer.WriteValue(PascalToJscriptCase(value.ToString()));
            }
        }

        private static string PascalToJscriptCase(string enumValue)
        {
            Debug.Assert(enumValue != null && enumValue.Length > 0, "enumValue should be not null and at least one letter long");

            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(char.ToLower(enumValue[0]));

            for (int i = 1; i < enumValue.Length; i++)
            {
                if (char.IsUpper(enumValue[i]))
                {
                       stringBuilder.Append("_");
                       stringBuilder.Append(char.ToLower(enumValue[i]));
                }
                else
                {
                    stringBuilder.Append(enumValue[i]);
                }
            }

            return stringBuilder.ToString();
        }

        private static string JscriptToPascalCase(string jsonValue)
        {
            Debug.Assert(jsonValue != null && jsonValue.Length > 0, "jsonValue should be not null and at least one letter long");

            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(char.ToUpper(jsonValue[0]));
            for (int i = 1; i < jsonValue.Length; i++)
            {
                if (jsonValue[i] == '_')
                {  
                    stringBuilder.Append(char.ToUpper(jsonValue[++i]));
                }
                else
                {
                    stringBuilder.Append(jsonValue[i]);
                }
            }

            return stringBuilder.ToString();
        }
    }
}
