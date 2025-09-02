// <copyright file="DictinaryExtensions.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Helper
{
    using System.Collections.Generic;

    public static class DictinaryExtensions
    {
        public static bool TryGetValue<T>(this IDictionary<string, object> dictionary, string key, out T value)
        {
            object result;
            if (dictionary.TryGetValue(key, out result))
            {
                value = (T)result;
                return true;
            }
            else
            {
                value = default(T);
                return false;
            }
        }
    }
}
