// <copyright file="HeaderMasker.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class HeaderMasker
    {
        private static readonly string[] SensitiveHeaders = 
        {
            "X-S2S-Proxy-Token",
            "Authorization",
            "x-pay-token",
            "x-ms-auth-internal-token" // Used only by /probe caller
        };

        public static string GetSanitizeValueForLogging(this KeyValuePair<string, IEnumerable<string>> header)
        {
            if (header.Value == null)
            {
                return null;
            }

            if (header.Key == null)
            {
                return header.Value.ToString();
            }

            if (SensitiveHeaders.Contains(header.Key, StringComparer.InvariantCultureIgnoreCase))
            {
                return string.Join(",", header.Value.Select((s) => s.Length > 0 ? string.Format("{0}...({1})", s[0], s.Length) : "x...(0)"));
            }
            else
            {
                return string.Join(",", header.Value);
            }
        }
    }
}
