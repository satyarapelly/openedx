// <copyright file="LongStringConverter.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Web
{
    using System;
    using System.Globalization;
    using Newtonsoft.Json;

    public class LongStringConverter : JsonConverter<long>
    {
        public override long ReadJson(JsonReader reader, Type objectType, long existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            string s = reader.Value as string;

            return long.Parse(s, CultureInfo.InvariantCulture);
        }

        public override void WriteJson(JsonWriter writer, long value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString(CultureInfo.InvariantCulture));
        }
    }
}
