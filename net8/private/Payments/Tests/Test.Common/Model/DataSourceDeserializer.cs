// <copyright file="DataSourceDeserializer.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Pidl;

    public class DataSourceDeserializer : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DataSource);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var dataSourceJObject = JObject.Load(reader);
            DataSource dataSource = JsonConvert.DeserializeObject<DataSource>(dataSourceJObject.ToString());
            return dataSource;
        }
    }
}