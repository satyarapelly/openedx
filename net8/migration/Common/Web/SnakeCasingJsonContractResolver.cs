// <copyright file="SnakeCasingJsonContractResolver.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Web
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Provides serialization functionality that meets the Commerce Platform specification.
    /// </summary>
    public class SnakeCasingJsonContractResolver : DefaultContractResolver
    {
        private const string LinksPropertyName = "links";
        private const string ObjectTypePropertyName = "object_type";
        private const string ContractVersionPropertyName = "contract_version";

        private static readonly Type DateTimeType = typeof(DateTime);
        private static readonly DateTime DefaultDateTime = default(DateTime);

        private static readonly EnumConverter StaticEnumConverter = new EnumConverter();
        private static readonly ByteArrayConverter StaticByteArrayConverter = new ByteArrayConverter();

        public SnakeCasingJsonContractResolver()
        {
            this.PropertyNameSerializationMap = new Dictionary<string, string>();
        }

        public bool UseDefaultDictionaryPropertyNameResolution { get; set; }

        public Dictionary<string, string> PropertyNameSerializationMap { get; private set; }
        
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            JsonPropertyAttribute propertyAttribute = member.GetCustomAttribute<JsonPropertyAttribute>();
            if (propertyAttribute != null)
            {
                property.PropertyName = propertyAttribute.PropertyName;
            }
            else
            {
                property.PropertyName = this.PropertyNameResolver(property.PropertyName);
            }

            if (property.PropertyType == DateTimeType)
            {
                property.DefaultValue = DefaultDateTime;
                property.DefaultValueHandling = DefaultValueHandling.Ignore;
            }
            else if (StaticEnumConverter.CanConvert(property.PropertyType))
            {
                property.Converter = StaticEnumConverter;
            }
            else if (StaticByteArrayConverter.CanConvert(property.PropertyType))
            {
                property.Converter = StaticByteArrayConverter;
            }

            switch (property.PropertyName)
            {
                case LinksPropertyName:
                    property.Order = 4;
                    break;
                case ObjectTypePropertyName:
                    property.Order = 1;
                    break;
                case ContractVersionPropertyName:
                    property.Order = 2;
                    break;
                default:
                    property.Order = 3;
                    break;
            }

            return property;
        }

        protected override JsonDictionaryContract CreateDictionaryContract(Type objectType)
        {
            JsonDictionaryContract dictionaryContract = base.CreateDictionaryContract(objectType);

            if (!this.UseDefaultDictionaryPropertyNameResolution)
            {
                dictionaryContract.DictionaryKeyResolver = this.PropertyNameResolver;
            }

            return dictionaryContract;
        }        

        private static string ToJsonCase(string enumValue)
        {
            if (string.IsNullOrWhiteSpace(enumValue))
            {
                throw new ArgumentException("Argument can not be null or whitespace.", "enumValue");
            }

            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(char.ToLower(enumValue[0]));

            bool lastWasUpper = true;

            for (int i = 1; i < enumValue.Length; i++)
            {
                bool isUpper = char.IsUpper(enumValue[i]);
                if (!lastWasUpper && isUpper)
                {
                    stringBuilder.Append("_");
                }

                char toAppend = isUpper ?
                    char.ToLower(enumValue[i]) : enumValue[i];

                stringBuilder.Append(toAppend);

                lastWasUpper = isUpper;
            }

            return stringBuilder.ToString();
        }

        private static string ToPascalCase(string jsonValue)
        {
            if (string.IsNullOrWhiteSpace(jsonValue))
            {
                throw new ArgumentException("Argument can not be null or whitespace.", "jsonValue");
            }

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

        private string PropertyNameResolver(string name)
        {
            string mappedName;
            if (!this.PropertyNameSerializationMap.TryGetValue(name, out mappedName))
            {
                mappedName = ToJsonCase(name);
            }

            return mappedName;
        }

        private class ByteArrayConverter : JsonConverter
        {
            private static readonly Type ByteArrayType = typeof(byte[]);

            public ByteArrayConverter()
            {
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == ByteArrayType;
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.String)
                {
                    return Convert.FromBase64String(reader.Value.ToString());
                }
                else
                {
                    throw new JsonSerializationException(string.Format("The JSON token type '{0}' can not be deserialized as an {1}.", reader.TokenType, "byte array"));
                }
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                byte[] bytes = value as byte[];
                if (bytes != null)
                {
                    writer.WriteValue(Convert.ToBase64String(bytes));
                }
            }
        }

        private class EnumConverter : JsonConverter
        {
            public EnumConverter()
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
                    string message = string.Format("The encoding '{0}' is not supported.", objectType.Name);
                    throw new JsonSerializationException(message);
                }

                if (reader.TokenType == JsonToken.String)
                {
                    return Enum.Parse(objectType, SnakeCasingJsonContractResolver.ToPascalCase(reader.Value.ToString()), true);
                }
                else if (reader.TokenType == JsonToken.Integer)
                {
                    return Enum.ToObject(objectType, reader.Value);
                }
                else
                {
                    string message = string.Format("The JSON token type '{0}' can not be deserialized as an {1}.", reader.TokenType, "enum");
                    throw new JsonSerializationException(message);
                }
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                if (value != null)
                {
                    writer.WriteValue(SnakeCasingJsonContractResolver.ToJsonCase(value.ToString()));
                }
            }
        }
    }
}