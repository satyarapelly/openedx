// <copyright file="Helper.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlFactory
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using PXCommon;

    public static class Helper
    {
        private static readonly Dictionary<string, string> LanguageMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "zh", "zh-CHS" },
            { "zh-HANS", "zh-CHS" },
            { "zh-CN", "zh-CHS" },
            { "zh-HANS-CN", "zh-CHS" },
            { "zh-SG", "zh-CHS" },
            { "zh-HANS-SG", "zh-CHS" },
            { "zh-HANT", "zh-CHT" },
            { "zh-HK", "zh-CHT" },
            { "zh-HANT-HK", "zh-CHT" },
            { "zh-MO", "zh-CHT" },
            { "zh-HANT-MO", "zh-CHT" },
            { "zh-TW", "zh-CHT" },
            { "zh-HANT-TW", "zh-CHT" },
            { "ar-er", "ar" },
            { "ar-km", "ar" },
            { "ar-dj", "ar" },
            { "ru-kz", "ru" }
        };

        // Should be set by startup code if using ASP.NET Core
        public static string WebRootPath { get; set; }

        public static string GetFullPath(string relativePath)
        {
            string retVal = string.Empty;
            if (WebHostingUtility.IsApplicationSelfHosted())
            {
                string locationBeforeShadowCopy = typeof(Microsoft.Commerce.Payments.PidlFactory.Helper).Assembly.CodeBase;
                Uri urilocation = new Uri(locationBeforeShadowCopy);
                UriBuilder uri = new UriBuilder(urilocation);
                string locationWithoutUriPrefixes = Uri.UnescapeDataString(uri.Path);
                string dir = Path.GetDirectoryName(locationWithoutUriPrefixes);
                retVal = Path.Combine(dir, relativePath);
            }
            else if (!string.IsNullOrEmpty(WebRootPath))
            {
                return Path.Combine(WebRootPath, GlobalConstants.FolderNames.WebAppData, relativePath);
            }
            else
            {
                throw new InvalidOperationException("WebRootPath must be set when not self-hosted.");
            }

            return retVal;
        }

        public static string TryToLower(string parameter)
        {
            return parameter?.ToLowerInvariant();
        }

        public static CultureInfo GetCultureInfo(string language)
        {
            if (language != null && LanguageMapping.TryGetValue(language, out var mapped))
            {
                language = mapped;
            }

            try
            {
                return new CultureInfo(language);
            }
            catch
            {
                return new CultureInfo(GlobalConstants.Defaults.Locale);
            }
        }
    }
}
