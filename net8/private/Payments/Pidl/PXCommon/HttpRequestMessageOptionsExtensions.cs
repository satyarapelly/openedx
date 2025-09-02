using System.Collections.Generic;
using System.Net.Http;

namespace Microsoft.Commerce.Payments.PXCommon
{
    public static class HttpRequestMessageOptionsExtensions
    {
        private static readonly HttpRequestOptionsKey<IDictionary<string, object?>> PropertiesKey = new("_Properties");

        private static IDictionary<string, object?> GetPropertyBag(this HttpRequestMessage request)
        {
            if (!request.Options.TryGetValue(PropertiesKey, out var bag))
            {
                bag = new Dictionary<string, object?>();
                request.Options.Set(PropertiesKey, bag);
            }
            return bag;
        }

        public static void SetProperty(this HttpRequestMessage request, string key, object? value)
        {
            request.GetPropertyBag()[key] = value;
        }

        public static void AddProperty(this HttpRequestMessage request, string key, object? value)
        {
            request.GetPropertyBag().Add(key, value);
        }

        public static bool ContainsProperty(this HttpRequestMessage request, string key)
        {
            return request.GetPropertyBag().ContainsKey(key);
        }

        public static T? GetProperty<T>(this HttpRequestMessage request, string key)
        {
            var bag = request.GetPropertyBag();
            return bag.TryGetValue(key, out var value) ? (T?)value : default;
        }

        public static bool TryGetProperty<T>(this HttpRequestMessage request, string key, out T? value)
        {
            var bag = request.GetPropertyBag();
            if (bag.TryGetValue(key, out var obj) && obj is T t)
            {
                value = t;
                return true;
            }

            value = default;
            return false;
        }
    }
}
