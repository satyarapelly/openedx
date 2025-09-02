// <copyright file="LocalizationRepository.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Pidl.Localization
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Resources;
    using System.Text;

    public class LocalizationRepository
    {
        private const string DefaultCultureName = "en-US";

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

        private static readonly LocalizationRepository InstanceField = new LocalizationRepository();
        private ResourceManager resourceManager = null;
        private Dictionary<string, string> defaultResourseSetReverseLookup;
        private Dictionary<string, string> defaultResourseSetReverseLookupCultureIgnoreCase;

        private LocalizationRepository()
        {
            this.ResourceManager = new ResourceManager("Microsoft.Commerce.Payments.PidlLocalizationRepository.Resources.LocalizedResources", Assembly.GetExecutingAssembly());
        }

        public static LocalizationRepository Instance
        {
            get
            {
                return InstanceField;
            }
        }

        private ResourceManager ResourceManager
        {
            get
            {
                return this.resourceManager;
            }

            set
            {
                this.resourceManager = value;
                this.defaultResourseSetReverseLookup = new Dictionary<string, string>(StringComparer.CurrentCulture);
                this.defaultResourseSetReverseLookupCultureIgnoreCase = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
                ResourceSet defaultResourceSet = this.resourceManager.GetResourceSet(new CultureInfo(string.Empty), true, false);
                foreach (DictionaryEntry element in defaultResourceSet)
                {
                    this.defaultResourseSetReverseLookup[element.Value.ToString()] = element.Key.ToString();
                    this.defaultResourseSetReverseLookupCultureIgnoreCase[element.Value.ToString()] = element.Key.ToString();
                }
            }
        }

        public string GetLocalizedString(string value, string language)
        {
            CultureInfo cultureInfo = GetCultureInfo(language);
            return this.GetLocalizedString(value, cultureInfo);
        }

        public string GetLocalizedString(string value, CultureInfo culture)
        {
            string retVal;
            string resourceKey = null;

            this.defaultResourseSetReverseLookup.TryGetValue(value, out resourceKey);
            if (resourceKey == null)
            {
                this.defaultResourseSetReverseLookupCultureIgnoreCase.TryGetValue(value, out resourceKey);
            }

            retVal = resourceKey == null ? value : this.ResourceManager.GetString(resourceKey, culture) ?? value;
            return retVal;
        }

        private static CultureInfo GetCultureInfo(string language)
        {
            CultureInfo retVal;

            if (language != null && LanguageMapping.ContainsKey(language))
            {
                language = LanguageMapping[language];
            }

            try
            {
                retVal = new CultureInfo(language);
            }
            catch
            {
                // Fallback to the default culture if the specified language was not initialized correctly
                retVal = new CultureInfo(DefaultCultureName);
            }

            return retVal;
        }
    }
}
