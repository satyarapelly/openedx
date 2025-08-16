// <copyright file="Constants.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System;
    using System.Collections.Generic;

    internal static class Constants
    {
        private static HashSet<string> descriptionTypesWithDirectName = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase)
        {
            DescriptionTypes.TaxIdDescription,
        };

        internal static HashSet<string> DescriptionTypesWithDirectName
        {
            get
            {
                return Constants.descriptionTypesWithDirectName;
            }
        }

        public static class DescriptionTypes
        {
            public const string PaymentMethodDescription = "paymentMethod";
            public const string AddressDescription = "address";
            public const string ChallengeDescription = "challenge";
            public const string ProfileDescription = "profile";
            public const string DigitizationDescription = "digitization";
            public const string MiscellaneousDescription = "data";
            public const string TaxIdDescription = "taxId";
        }

        public static class PropertyErrorConstants
        {
            public const string RequiredFieldEmpty = "required_field_empty";
            public const string RequiredFieldEmptyMessage = "The required field is null or empty";
        }

        internal static class DescriptionIdentityFields
        {
            public const string DescriptionType = "description_type";
            public const string Family = "family";
            public const string Type = "type";
            public const string Country = "country";
            public const string Locale = "locale";
            public const string Step = "step";
            public const string Scenario = "scenario";
            public const string CountryCode = "country_code";
        }

        internal static class QueryParamFields
        {
            public const string Language = "language";
            public const string Country = "country";
            public const string Partner = "partner";
            public const string Type = "type";
            public const string Family = "family";
            public const string Scenario = "scenario";
            public const string PageId = "pageId";
        }

        internal static class ConfigSpecialStrings
        {
            public const string CollectionNamePrefix = "{}";
            public const string CollectionDelimiter = ";";
            public const string NameValueDelimiter = "=";
            public const string CountryId = "()CountryId";
            public const string PartnerName = "()PartnerName";
        }

        internal static class ScenarioContextsFields
        {
            internal const string ResourceType = "resourceType";
            internal const string TerminatingErrorHandling = "terminatingErrorHandling";
        }

        internal static class ResourceTypes
        {
            internal const string Primary = "primary"; // default
            internal const string Secondary = "secondary";
        }

        internal static class TerminatingErrorHandlingMethods
        {
            internal const string Throw = "throw"; // default
            internal const string Ignore = "ignore";
        }

        internal static class DisplayHintIds
        {
            public const string PidlContainer = "pidlContainer";
        }

        internal static class PidlPropertyNames
        {
            public const string DataDescription = "data_description";
        }

        internal static class ElementTypes
        {
            public const string ButtonList = "buttonList";
            public const string Textbox = "textbox";
            public const string Dropdown = "dropdown";
        }
    }
}