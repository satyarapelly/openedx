using System.Collections.Generic;
using System.Net.Http;

namespace System.Net.Http
{
    public static class HttpRequestMessageExtensions
    {
        private static readonly HttpRequestOptionsKey<IDictionary<string, object?>> PropertiesKey =
            new("PX.Properties");

        public static IDictionary<string, object?> GetProperties(this HttpRequestMessage request)
        {
            if (!request.Options.TryGetValue(PropertiesKey, out var properties))
            {
                properties = new Dictionary<string, object?>();
                request.Options.Set(PropertiesKey, properties);
            }

            return properties;
        }
    }
}
