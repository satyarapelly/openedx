// <copyright file="Constants.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Constants container, each set of constants will be grouped into a nested class
    /// </summary>
    internal static class Constants
    {
        private static readonly Dictionary<string, string> countriesToNotCapitalizeRegionNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { CountryCodes.Turkey, "TRProvinces" },
            { CountryCodes.Venezuela, "VEProvinces" }
        };

        private static List<string> amcWebReactScenarios = new List<string>() { Constants.ScenarioNames.PayNow, Constants.ScenarioNames.ProfileAddress, Constants.ScenarioNames.ChangePI };

        private static List<string> amcWebReactTradeAVSScenarios = new List<string>() { Constants.ScenarioNames.SuggestAddressesTradeAVS };

        private static List<string> possibleOperationTypes = new List<string>() { Constants.PidlOperationTypes.Add, Constants.PidlOperationTypes.Update, Constants.PidlOperationTypes.Replace, Constants.PidlOperationTypes.AddAdditional, Constants.PidlOperationTypes.RenderPidlPage, Constants.PidlOperationTypes.Delete, Constants.PidlOperationTypes.Apply, Constants.PidlOperationTypes.Offer, Constants.PidlOperationTypes.ExpressCheckout };

        private static List<string> partnersEnabledSinglePiDisplay = new List<string>() { Constants.PartnerNames.AmcWeb, Constants.PartnerNames.WindowsSettings };

        private static HashSet<string> partnersWithPageSplits = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Constants.PidlConfig.XboxPartnerName,
            Constants.PidlConfig.AmcXboxPartnerName
        };

        // TODO: Tax ID collection
        // Let profile and address form also leverage countriesEnabledTaxIdCheckbox to enable taxIdCheckbox
        // currently it is used by standalone taxID form only
        private static HashSet<string> countriesEnabledTaxIdCheckbox = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // existing 10 countries
            "tr",
            "no",
            "am",
            "by",
            "bd",
            "my",
            "cl",
            "mx",
            "id",
            "th",

            // 13 new countries using VAT ID as the label
            "bh",
            "cm",
            "ge",
            "gh",
            "is",
            "ke",
            "md",
            "ng",
            "om",
            "tj",
            "ua",
            "uz",
            "zw",

            // new countries not using VAT ID as the label
            "fj",
            "gt",
            "kh",
            "ph",
            "vn",
            "bb",
            "il",
            "kz",
            "la",
            "np",
            "sg",
            "ug",
            "ci",
            "gh",
            "sn",
            "zm",

            // enable 4 countries in prod to have tax id checkbox
            "ae",
            "bs",
            "co",
            "sa",
        };

        private static Dictionary<string, List<string>> countryToPartnerCombinationsForPXSkipGetPMCCOnly = new Dictionary<string, List<string>>()
        {
            { "br", new List<string>
                {
                    "cart",
                    "webblends",
                    "oxodime",
                    "oxowebdirect"
                }
            }
        };

        // Used to return more than one profile pidl
        // For now, it's a requirement for commercialstores for both org and legal profile update
        // enabled for all the countries above (countriesWithTaxIdCheckbox)
        private static List<string> multipleProfilesEnabledPartners = new List<string>()
        {
            PartnerNames.Commercialstores,
        };

        private static HashSet<string> partnersToEnableKlarnaCheckout = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Constants.PidlConfig.WebblendsPartnerName,
            Constants.PidlConfig.CartPartnerName,
            Constants.PidlConfig.OXOWebDirectPartnerName,
            Constants.PidlConfig.OXODIMEPartnerName
        };

        private static HashSet<string> countriesToEnableKlarnaCheckout = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "nl",
            "at",
            "de",
            "fi",
            "dk",
            "no",
            "se"
        };

        // Below are the countries that don't have region names to display.
        private static HashSet<string> countriesToExcludeRegionName = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "lv",
            "al",
            "dz",
            "ba",
            "ge",
            "bh",
            "pk",
            "kw",
            "ke",
            "eg",
            "ec",
            "sk",
            "jo",
            "mn",
            "ma",
            "om",
            "py",
            "lk",
            "tn"
        };

        private static HashSet<string> partnersToEnablePayPal2ndScreenRedirectButton = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Constants.PidlConfig.XboxPartnerName,
            Constants.PidlConfig.AmcXboxPartnerName
        };

        private static List<string> avsSuggestEnabledPartners = new List<string>()
        {
            PartnerNames.Cart,
            PartnerNames.OXOWebDirect,
            PartnerNames.OXODIME,
            PartnerNames.Webblends,
            PartnerNames.WebblendsInline,
            PartnerNames.Xbox,
            PartnerNames.OfficeOobe,
            PartnerNames.OXOOobe,
            PartnerNames.SmbOobe,
            PartnerNames.AmcWeb,
            PartnerNames.Mseg,
            PartnerNames.OneDrive,
            PartnerNames.StoreOffice,
            PartnerNames.Payin,
            PartnerNames.SetupOffice,
            PartnerNames.SetupOfficeSdx,
            PartnerNames.ConsumerSupport,
            PartnerNames.XboxWeb,
            PartnerNames.WindowsSettings
        };

        private static List<string> modalGroupIds = new List<string>()
        {
            "suggestBlockUserEntered",
            "suggestBlock",
            "suggestBlockUserEnteredV2",
            "suggestBlockV2",
        };

        private static HashSet<string> partnersToEnablePaypalRedirectOnTryAgain = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Constants.PidlConfig.WebblendsPartnerName,
            Constants.PidlConfig.OXODIMEPartnerName
        };

        public static List<string> AmcWebReactScenarios
        {
            get
            {
                return amcWebReactScenarios;
            }
        }

        public static List<string> AmcWebReactTradeAVSScenarios
        {
            get
            {
                return amcWebReactTradeAVSScenarios;
            }
        }

        public static List<string> PidlPossibleOperationsTypes
        {
            get
            {
                return possibleOperationTypes;
            }
        }

        public static List<string> PartnersEnabledSinglePiDisplay
        {
            get
            {
                return partnersEnabledSinglePiDisplay;
            }
        }

        public static HashSet<string> PartnersWithPageSplits
        {
            get
            {
                return partnersWithPageSplits;
            }
        }

        public static HashSet<string> PartnersToEnableKlarnaCheckout
        {
            get
            {
                return partnersToEnableKlarnaCheckout;
            }
        }

        public static HashSet<string> CountriesToEnableKlarnaCheckout
        {
            get
            {
                return countriesToEnableKlarnaCheckout;
            }
        }

        public static HashSet<string> AllCountriesEnabledTaxIdCheckbox
        {
            get
            {
                return countriesEnabledTaxIdCheckbox;
            }
        }

        public static Dictionary<string, List<string>> CountryToPartnerCombinationsForPXSkipGetPMCCOnly
        {
            get
            {
                return countryToPartnerCombinationsForPXSkipGetPMCCOnly;
            }
        }

        public static HashSet<string> PartnersToEnablePayPal2ndScreenRedirectButton
        {
            get
            {
                return partnersToEnablePayPal2ndScreenRedirectButton;
            }
        }

        public static Dictionary<string, string> CountriesToNotCapitalizeRegionNames
        {
            get
            {
                return countriesToNotCapitalizeRegionNames;
            }
        }

        public static HashSet<string> CountriesToExcludeRegionName
        {
            get
            {
                return countriesToExcludeRegionName;
            }
        }

        internal static List<string> AvsSuggestEnabledPartners
        {
            get
            {
                return avsSuggestEnabledPartners;
            }
        }

        internal static List<string> ModalGroupIds
        {
            get
            {
                return modalGroupIds;
            }
        }

        internal static HashSet<string> PartnersToEnablePaypalRedirectOnTryAgain
        {
            get
            {
                return partnersToEnablePaypalRedirectOnTryAgain;
            }
        }

        internal static List<string> MultipleProfilesEnabledPartners
        {
            get
            {
                return multipleProfilesEnabledPartners;
            }
        }

        internal static Dictionary<string, List<string>> ServiceSidePostAddressSupportedPartnersAndScenarios
        {
            get
            {
                return new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                {
                    { PartnerNames.AmcWeb, new List<string>() { ScenarioNames.ProfileAddress } },
                    { TemplateName.DefaultTemplate, new List<string>() { ScenarioNames.ProfileAddress } }
                };
            }
        }

        internal static Dictionary<string, List<string>> AddressTextDisplayIdToPropertyNameMappings
        {
            get
            {
                return new Dictionary<string, List<string>>
                {
                    { DisplayHintIds.AddressEnteredOnlyLine1TradeAVS, new List<string>() { "address_line1" } },
                    { DisplayHintIds.AddressEnteredOnlyLine2TradeAVS, new List<string>() { "address_line2" } },
                    { DisplayHintIds.AddressEnteredOnlyCityRegionTradeAVS, new List<string>() { "city", "region" } },
                    { DisplayHintIds.AddressEnteredOnlyPostalCodeTradeAVS, new List<string>() { "postal_code" } },
                };
            }
        }

        internal static Dictionary<string, string> DataSourceToAddressTypeMappings
        {
            get
            {
                return new Dictionary<string, string>
                {
                    { DataSource.Hapi, AddressTypes.HapiV1 },
                    { DataSource.JarvisProfileAddress, AddressTypes.ShippingV3 },
                    { DataSource.JarvisShippingV3, AddressTypes.ShippingV3 },
                    { DataSource.JarvisBilling, AddressTypes.Billing },
                    { DataSource.JarvisOrgAddress, AddressTypes.OrgAddress }
                };
            }
        }

        internal static Dictionary<string, string> HapiDisplayHintIdsMappings
        {
            get
            {
                return new Dictionary<string, string>
                {
                    { AddressProperty.FirstName, DisplayHintIds.HapiFirstName },
                    { AddressProperty.MiddleName, DisplayHintIds.HapiMiddleName },
                    { AddressProperty.LastName, DisplayHintIds.HapiLastName },
                    { AddressProperty.CompanyName, DisplayHintIds.HapiCompanyName },
                    { AddressProperty.Email, DisplayHintIds.HapiEmail },
                    { AddressProperty.PhoneNumber, DisplayHintIds.HapiPhoneNumber },
                    { GenericProperty.PageTitle, DisplayHintIds.HapiAddressHeading },
                    { GenericProperty.StarRequiredTextGroup, DisplayHintIds.StarRequiredTextGroup },
                    { GenericProperty.MicrosoftPrivacyTextGroup, DisplayHintIds.MicrosoftPrivacyTextGroup },
                    { AddressProperty.AddressLine1, DisplayHintIds.HapiAddressLine1 },
                    { AddressProperty.AddressLine2, DisplayHintIds.HapiAddressLine2 },
                    { AddressProperty.AddressLine3, DisplayHintIds.HapiAddressLine3 },
                    { AddressProperty.Region, DisplayHintIds.HapiRegion },
                    { AddressProperty.Country, DisplayHintIds.HapiCountry },
                };
            }
        }

        internal static Dictionary<string, string> SetIsSubmitGroupFalseMappings
        {
            get
            {
                return new Dictionary<string, string>
                {
                    { GenericProperty.AddressSuggestionUseThisAddressButtonWithSuggestions, DisplayHintIds.AddressUseEnteredGroup },
                    { GenericProperty.AddressSuggestionUseThisAddressButtonWithoutSuggestions, DisplayHintIds.AddressNextGroup },
                };
            }
        }

        internal static Dictionary<string, string> JarvisDisplayHintIdsMappings
        {
            get
            {
                return new Dictionary<string, string>
                {
                    { AddressProperty.FirstName, DisplayHintIds.AddressFirstNameOptional },
                    { AddressProperty.LastName, DisplayHintIds.AddressLastNameOptional },
                    { AddressProperty.Email, DisplayHintIds.AddressEmailOptional },
                    { AddressProperty.PhoneNumber, DisplayHintIds.AddressPhoneNumberOptional },
                    { AddressProperty.Country, DisplayHintIds.AddressCountry },
                };
            }
        }

        internal static Dictionary<string, string> JarvisOrgAddressDisplayHintIdsMappings
        {
            get
            {
                return new Dictionary<string, string>
                {
                    { AddressProperty.OrgEmail, DisplayHintIds.OrgAddressModernEmail },
                    { AddressProperty.OrgPhoneNumber, DisplayHintIds.OrgAddressModernPhoneNumber },
                };
            }
        }

        internal static Dictionary<string, string> ProfileEmployeeDisplayHints
        {
            get
            {
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { ProfileFieldSettingNames.CompanyName, Constants.ProfileHintIds.ProfileEmployeeCompanyNameProperty },
                    { ProfileFieldSettingNames.FirstName, Constants.ProfileHintIds.AddressFirstName },
                    { ProfileFieldSettingNames.LastName, Constants.ProfileHintIds.AddressLastName },
                };
            }
        }

        internal static Dictionary<string, string> ProfileOrganizationDisplayHints
        {
            get
            {
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { ProfileFieldSettingNames.CompanyName, Constants.ProfileHintIds.ProfileOrganizationCompanyNameProperty },
                    { ProfileFieldSettingNames.FirstName, Constants.ProfileHintIds.AddressFirstName },
                    { ProfileFieldSettingNames.LastName, Constants.ProfileHintIds.AddressLastName },
                };
            }
        }

        internal static Dictionary<string, List<string>> ConsumerTaxIdMappings
        {
            get
            {
                return new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                {
                    { DescriptionIdentityFields.Value, new List<string>() { DisplayHintIds.TaxCpfProperty, DisplayHintIds.TaxVatIdProperty } }
                };
            }
        }

        internal static bool IsVirtualLegacyInvoice(string paymentMethodFamily, string paymentMethodType)
        {
            return string.Equals(paymentMethodFamily, Constants.PaymentMethodFamilyNames.Virtual, StringComparison.OrdinalIgnoreCase)
                && string.Equals(paymentMethodType, Constants.PaymentMethodTypeNames.LegacyInvoice, StringComparison.OrdinalIgnoreCase);
        }

        public static class ProfileFieldSettingNames
        {
            public const string CompanyName = "companyName";
            public const string FirstName = "firstName";
            public const string LastName = "lastName";
        }

        public static class ProfileHintIds
        {
            public const string ProfileEmployeeCompanyNameProperty = "profileEmployeeCompanyNameProperty";
            public const string ProfileOrganizationCompanyNameProperty = "profileOrganizationCompanyNameProperty";
            public const string AddressFirstName = "addressFirstName";
            public const string AddressLastName = "addressLastName";
        }

        public static class AddressProperty
        {
            public const string FirstName = "firstName";
            public const string MiddleName = "middleName";
            public const string LastName = "lastName";
            public const string CompanyName = "companyName";
            public const string Email = "email";
            public const string PhoneNumber = "phoneNumber";
            public const string OrgEmail = "orgEmail";
            public const string OrgPhoneNumber = "orgPhoneNumber";
            public const string AddressLine1 = "addressLine1";
            public const string AddressLine2 = "addressLine2";
            public const string AddressLine3 = "addressLine3";
            public const string AddressCity = "addressCity";
            public const string Region = "region";
            public const string Country = "country";
            public const string AddressProvince = "addressProvince";
            public const string AddressPostalCode = "addressPostalCode";
        }

        public static class AddressDataDescriptionProperty
        {
            public const string Line1 = "line1";
            public const string AddressLine1 = "address_line1";
            public const string AddressLine1NoSpace = "addressLine1";
            public const string AddressLineUnderscore1 = "address_line_1";
            public const string AddressLine2 = "address_line2";
            public const string AddressLine3 = "address_line3";
            public const string AddressCity = "city";
            public const string Region = "region";
            public const string AddressPostalCode = "postal_code";
            public const string Country = "country";
            public const string HapiSuaLine1 = "hapi_sua_line_1";
            public const string ProfileAddressLine1DataBinding = "profile_address_line_1_databinding";
            public const string ProfileLegalEntityAddressLine1DataBinding = "profile_legalentity_address_line_1_databinding";
            public const string AddressBillingGroupAddressLine1 = "address_billingGroup_address_line_1";
            public const string HapiV1ModernAccountV20190531OrganizationAddressAddressLine1 = "hapiV1ModernAccountV20190531OrganizationAddress_addressLine1";
            public const string OrgAddressAddressLine1 = "orgAddress_addressLine1";
            public const string HapiV1ModernAccountV20190531OrganizationAddressBillToAddressLine1 = "hapiV1ModernAccountV20190531OrganizationAddressBillTo_addressLine1";
            public const string HapiV1ModernAccountV20190531IndividualAddressAddressLine1 = "hapiV1ModernAccountV20190531IndividualAddress_addressLine1";
            public const string HapiV1ModernAccountV20190531AddressAddressLine1 = "hapiV1ModernAccountV20190531Address_addressLine1";
            public const string HapiV1ModernAccountV20190531IndividualAddressBillToAddressLine1 = "hapiV1ModernAccountV20190531IndividualAddressBillTo_addressLine1";
        }

        public static class GenericProperty
        {
            public const string PageTitle = "pageTitle";
            public const string StarRequiredTextGroup = "starRequiredTextGroup";
            public const string MicrosoftPrivacyTextGroup = "microsoftPrivacyTextGroup";
            public const string AddressSuggestionUseThisAddressButtonWithSuggestions = "addressSuggestionUseThisAddressButtonWithSuggestions";
            public const string AddressSuggestionUseThisAddressButtonWithoutSuggestions = "addressSuggestionUseThisAddressButtonWithoutSuggestions";
        }

        public static class DataSource
        {
            public const string Hapi = "hapi";
            public const string Jarvis = "jarvis";
            public const string JarvisBilling = "jarvisBilling";
            public const string JarvisShippingV3 = "jarvisShippingV3";
            public const string JarvisShipping = "jarvisShipping";
            public const string JarvisOrgAddress = "jarvisOrgAddress";
            public const string JarvisProfileAddress = "jarvisprofileaddress";
            public const string ConsumerTaxId = "consumerTaxId";
        }

        public static class RetryCount
        {
            public const int MaxRetryCountOnNetworkError = 1;
            public const int MaxRetryCountOnOAuthError = 2;
            public const int MinRetryCountOnPicvChallenge = 1;
        }

        public static class PidlUrlConstants
        {
            public const string TransformationUrlSubPath = "/transformation";
            public const string ValidationUrlSubPath = "/validation?language=()Language";
            public const string StaticResourceServiceImagesV4 = "https://pmservices.cp.microsoft.com/staticresourceservice/images/v4";
            public const string XboxCardImage = "https://pmservices.cp.microsoft.com/staticresourceservice/images/v4/cardImage_XboxCoBrandedCardApply.png";
            public const string PIFDIntDomain = "pmservices.cp.microsoft-int.com";
            public const string StaticResourceServiceProdAFDEndpoint = "https://pmservices.cp.microsoft.com";
            public const string StaticResourceServiceIntAFDEndpoint = "https://pmservices.cp.microsoft-int.com";
            public const string StaticResourceServiceProdCDNEndpoint = "https://staticresources.payments.microsoft.com";
            public const string StaticResourceServiceIntCDNEndpoint = "https://staticresources.payments.microsoft-int.com";
            public const string EditPhoneMSAPath = "https://aka.ms/editPhone";
            public const string XboxCardApplyBackgroundImage = "https://pmservices.cp.microsoft.com/staticresourceservice/images/v4/background_XboxCoBrandedCardApply_{themePlaceholder}.png";
        }

        public static class PidlOperationTypes
        {
            public const string Add = "add";
            public const string AddAdditional = "add_additional";
            public const string Update = "update";
            public const string UpdatePatch = "update_patch";
            public const string UpdatePartial = "update_partial";
            public const string Select = "select";
            public const string Redeem = "redeem";
            public const string ConfirmRedeem = "confirmRedeem";
            public const string SelectInstance = "selectinstance";
            public const string SelectSingleInstance = "selectsingleinstance";
            public const string ValidateInstance = "validateinstance";
            public const string Purchase = "purchase";
            public const string ValidatePurchase = "validate";
            public const string Show = "show";
            public const string Search = "search";
            public const string SearchTransactions = "searchTransactions";
            public const string Replace = "replace";
            public const string FundStoredValue = "fundStoredValue";
            public const string ValidateAddress = "validate";
            public const string RenderPidlPage = "renderPidlPage";
            public const string Delete = "delete";
            public const string Apply = "apply";
            public const string HandlePaymentChallenge = "handlepaymentchallenge";
            public const string Offer = "offer";
            public const string EditPhoneNumber = "editPhoneNumber";
            public const string SelectChallengeType = "selectChallengeType";
            public const string ExpressCheckout = "expressCheckout";
        }

        public static class PaymentInstrumentLogos
        {
            private static Dictionary<string, string> logoUrls = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { PaymentMethodTypeNames.Visa, "/staticresourceservice/images/v4/logo_visa.svg" },
                { PaymentMethodTypeNames.Amex, "/staticresourceservice/images/v4/logo_amex.svg" },
                { PaymentMethodTypeNames.MasterCard, "/staticresourceservice/images/v4/logo_mc.svg" },
                { PaymentMethodTypeNames.Discover, "/staticresourceservice/images/v4/logo_discover.svg" },
                { PaymentMethodTypeNames.JapanCreditBureau, "/staticresourceservice/images/v4/logo_jcb.svg" },
                { PaymentMethodTypeNames.HiperCard, "/staticresourceservice/images/v4/logo_hipercard.svg" },
                { PaymentMethodTypeNames.Elo, "/staticresourceservice/images/v4/logo_elo.svg" },
                { PaymentMethodTypeNames.Verve, "/staticresourceservice/images/v4/v2_logo_verve.png" },
            };

            public static bool IsTypeSupported(string type)
            {
                if (string.IsNullOrEmpty(type))
                {
                    return false;
                }

                return logoUrls.ContainsKey(type);
            }

            public static string GetLogoUrl(string type)
            {
                if (string.IsNullOrEmpty(type))
                {
                    return null;
                }

                string logoUrl = null;
                logoUrls.TryGetValue(type, out logoUrl);
                if (logoUrl != null)
                {
                    logoUrl = (Common.Environments.Environment.IsProdOrPPEEnvironment ? PidlUrlConstants.StaticResourceServiceProdAFDEndpoint : PidlUrlConstants.StaticResourceServiceIntAFDEndpoint) + logoUrl;
                }

                return logoUrl;
            }
        }

        public static class PidlResourceIdentities
        {
            internal const string PaymentMethodSelectPidl = "selectpm";
            internal const string PaymentInstrumentSelectPidl = "selectpi";
            internal const string AddressGroupSelectPidl = "selectaddressgroup";
            internal const string AddressGroupSuggestionPidl = "shipping";
            internal const string PaymentInstrumentSelectNoPiPidl = "selectpinone";
            internal const string PaymentInstrumentBackupPidl = "selectpibackup";
            internal const string SinglePaymentInstrumentPidl = "selectsinglepi";
            internal const string DisplaySinglePaymentInstrumentPidl = "displaysinglepi";
            internal const string SinglePaymentInstrumentNoPiPidl = "selectsinglepinone";
            internal const string SinglePaymentInstrumentNoActivePiPidl = "selectsinglepinoactive";
            internal const string SingleBackupPidl = "selectsinglebackup";
            internal const string SingleCsvPidl = "selectsinglepicsv";
            internal const string SearchTransactions = "paymentInstrument.searchTransactions";
            internal const string List = "list";
            internal const string ListModern = "listmodern";
            internal const string ListPI = "listpi";
            internal const string ListMCPI = "listmcpi";
            internal const string ListMCPIModern = "listmcpimodern";
            internal const string ListAdditionalPI = "listadditionalpi";
            internal const string OrganizationDisableTax = "organizationDisableTax";
            internal const string LegalEntityDisableTax = "legalEntityDisableTax";
            internal const string VatId = "vat_id";
            internal const string VatIdDisableTax = "vat_idDisableTax";
            internal const string VatIdAdditionalData = "vat_idAdditionalData";
            internal const string HapiSUADisabledTax = "hapiServiceUsageAddressDisabledTax";
            internal const string PXV3 = "px_v3";
            internal const string PXV3Billing = "px_v3_billing";
            internal const string PXV3Shipping = "px_v3_shipping";
            internal const string UserEnteredAddress = "userEntered";
            internal const string AddressAVSSuggestions = "addressAVSSuggestions";
            internal const string AddressNoAVSSuggestions = "addressNoAVSSuggestions";
            internal const string AddressAVSSuggestionsV2 = "addressAVSSuggestionsV2";
            internal const string AddressNoAVSSuggestionsV2 = "addressNoAVSSuggestionsV2";
            internal const string UserEnteredAddressOnly = "userEnteredOnly";
            internal const string PaypalRedirectStaticPidl = "paypalredirectpidl";
            internal const string GenericRedirectStaticPidl = "genericredirectpidl";
            internal const string ThirdPartyPaymentSelectPM = "tppSelectpm";
            internal const string ItalyCodiceFiscale = "national_identification_number";
            internal const string EgyptNationalIdentificationNumber = "egypt_national_identification_number";
        }

        public static class TransformationType
        {
            public const string ToPhoneNumberE164 = "ToPhoneNumberE164";
            public const string IndiaStateFullNameToInitials = "IndiaStateFullNameToInitials";

            // VNext currently is libphonenumber-csharp 8.8.10
            public const string ToPhoneNumberE164VNext = "ToPhoneNumberE164VNext";
        }

        public static class TaxIdTypes
        {
            public const string Consumer = "consumer_tax_id";
            public const string Commercial = "commercial_tax_id";
        }

        public static class CountryCodes
        {
            public const string India = "in";
            public const string Brazil = "br";
            public const string Sweden = "se";
            public const string UnitedStates = "us";
            public const string Venezuela = "ve";
            public const string Turkey = "tr";
            public const string Italy = "it";
            public const string Egypt = "eg";
        }

        public static class IndiaStateMapping
        {
            private static Dictionary<string, string> stateMappingIndia = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
            {
                // State Mapping for India
                { "ANDAMAN AND NICOBAR ISLANDS", "AN" },
                { "ANDHRA PRADESH", "AP" },
                { "ARUNACHAL PRADESH", "AR" },
                { "ASSAM", "AS" },
                { "BIHAR", "BR" },
                { "CHANDIGARH", "CH" },
                { "CHHATTISGARH", "CT" },
                { "DADRA AND NAGAR HAVELI AND DAMAN AND DIU", "DN" },
                { "LADAKH", "LA" },
                { "DELHI", "DL" },
                { "NATIONAL CAPITAL TERRITORY OF DELHI", "DL" },
                { "GOA", "GA" },
                { "GUJARAT", "GJ" },
                { "HARYANA", "HR" },
                { "HIMACHAL PRADESH", "HP" },
                { "JAMMU AND KASHMIR", "JK" },
                { "JHARKHAND", "JH" },
                { "KARNATAKA", "KA" },
                { "KERALA", "KL" },
                { "LAKSHADWEEP", "LD" },
                { "MADHYA PRADESH", "MP" },
                { "MAHARASHTRA", "MH" },
                { "MANIPUR", "MN" },
                { "MEGHALAYA", "ML" },
                { "MIZORAM", "MZ" },
                { "NAGALAND", "NL" },
                { "ORISSA", "OR" },
                { "PUDUCHERRY", "PY" },
                { "PUNJAB", "PB" },
                { "RAJASTHAN", "RJ" },
                { "SIKKIM", "SK" },
                { "TAMIL NADU", "TN" },
                { "TELANGANA", "TG" },
                { "TRIPURA", "TR" },
                { "UTTAR PRADESH", "UP" },
                { "UTTARAKHAND", "UT" },
                { "WEST BENGAL", "WB" }
            };

            public static string GetMappingState(string fullNameState)
            {
                string stateInitials = null;
                stateMappingIndia.TryGetValue(fullNameState, out stateInitials);
                return stateInitials;
            }
        }

        public static class AVSStatus
        {
            private static Dictionary<string, string> avsStatusMessages = new Dictionary<string, string>()
            {
                { "VerifiedShippable", "Given address is valid and shippable." },
                { "Verified", "Given address is valid and verified." },
                { "PremisesPartial", "We can verify this location exists, but need more address info." },
                { "StreetPartial", "Enter your house or building number." },
                { "InteractionRequired", "The address you entered is different to the one recommended by the postal service." },
                { "Multiple", "Enter your house or building number." },
                { "None", "We can't validate this address.  If you're certain this is correct, ignore this." },
                { "Fallback", "Choose an option below." },
            };

            public static string GetAVSMessage(string status)
            {
                string message = null;
                if (!avsStatusMessages.TryGetValue(status, out message))
                {
                    avsStatusMessages.TryGetValue("Fallback", out message);
                }

                return message;
            }
        }

        public static class ValidationTypes
        {
            public const string Service = "service";
            public const string Regex = "regex";
            public const string Function = "function";
        }

        public static class ServerValidationType
        {
            public const string PhoneNumber = "phonenumber";
            public const string PhoneNumberVNext = "phonenumberVNext"; // VNext currently is libphonenumber-csharp 8.8.10
        }

        public static class TransformerNames
        {
            public const string PhoneNumberTransformer = "PhoneNumberTransformer";
        }

        public static class ValidatorNames
        {
            public const string PhoneNumberValidator = "PhoneNumberValidator";
            public const string BankCodeValidator = "BankCodeValidator";
        }

        public static class DataDescriptionFilePaths
        {
            public const string DomainDictionariesCSV = "V7/Config/DomainDictionaries.csv";
            public const string PaymentMethodFamiliesCSV = "V7/Config/PaymentMethodFamilies.csv";
            public const string PaymentMethodsCSV = "V7/Config/PaymentMethods.csv";
            public const string PaymentMethodsInCountriesCSV = "V7/Config/PaymentMethodsInCountries.csv";
            public const string TaxIdsInCountriesCSV = "V7/Config/TaxIdsInCountries.csv";
            public const string PIDLResourcesCSV = "V7/Config/PIDLResources.csv";
            public const string PropertyDescriptionsCSV = "V7/Config/PropertyDescriptions.csv";
            public const string PropertyValidationCSV = "V7/Config/PropertyValidation.csv";
            public const string PropertyTransformationCSV = "V7/Config/PropertyTransformation.csv";
            public const string PropertyDataProtectionsCSV = "V7/Config/PropertyDataProtections.csv";
            public const string ValidationChallengeTypesCSV = "V7/Config/ValidationChallengeTypes.csv";
        }

        public static class DataDescriptionOverrideFileNames
        {
            public const string PropertyDescriptionsCSV = "PropertyDescriptions.csv";
            public const string PropertyValidationCSV = "PropertyValidation.csv";
        }

        public static class DataSourcesFilePaths
        {
            public const string DataSourcesCSV = "V7/Config/DataSources.csv";
        }

        public static class SubmitLinksFilePaths
        {
            public const string SubmitLinksCSV = "V7/Config/SubmitLinks.csv";
        }

        public static class DisplayDescriptionFileNames
        {
            public const string PIDLResourcesDisplaySequencesCSV = "PIDLResourcesDisplaySequences.csv";
            public const string ContainerDisplayDescriptionsCSV = "ContainerDisplayHints.csv";
            public const string PropertyDisplayDescriptionsCSV = "PropertyDisplayHints.csv";
            public const string DisplaySequencesCSV = "DisplaySequences.csv";
            public const string PropertyErrorMessagesCSV = "PropertyErrorMessages.csv";
            public const string DisplayDescriptionTagsCSV = "DisplayDescriptionTags.csv";
            public const string DisplayDescriptionConditionalFieldsCSV = "DisplayDescriptionConditionalFields.csv";
            public const string DisplayStringSequencesCSV = "DisplayStringSequences.csv";
            public const string DisplayStringsCSV = "DisplayStrings.csv";
            public const string DisplayTransformationsCSV = "DisplayTransformations.csv";
            public const string DisplayDictionariesCSV = "DisplayDictionaries.csv";
        }

        public static class PidlConfig
        {
            public const string DisplayDescriptionFolderRootPath = "V7/Config/DisplayDescriptions/";
            public const string DefaultPartnerName = PartnerNames.DefaultPartner;
            public const string WebblendsPartnerName = PartnerNames.Webblends;
            public const string WebblendsInlinePartnerName = PartnerNames.WebblendsInline;
            public const string XboxPartnerName = PartnerNames.Xbox;
            public const string AmcXboxPartnerName = PartnerNames.AmcXbox;
            public const string DisplayDescriptionRootPageId = "rootPage";
            public const string FlightingFolderName = "Flights";
            public const string OXODIMEPartnerName = PartnerNames.OXODIME;
            public const string OXOWebDirectPartnerName = PartnerNames.OXOWebDirect;
            public const string CartPartnerName = PartnerNames.Cart;
        }

        public static class ClientSettingNames
        {
            // Setting for the PIDL SDK wait for all the secure fields in the form to initialize
            public const string PidlSdkWaitTimeForSecureFieldsInit = "pidlSdkWaitTimeForSecureFieldsInit";
        }

        public static class ClientSettings
        {
            // Number of milliseconds the PIDL SDK wait for all the secure fields in the form to initialize
            public const int PidlSdkWaitTimeForSecureFieldsInit = 10000;
        }

        public static class DescriptionTypes
        {
            public const string PaymentTokenDescription = "paymentToken";
            public const string PaymentMethodDescription = "paymentMethod";
            public const string PaymentInstrumentDescription = "paymentInstrument";
            public const string AddressDescription = "address";
            public const string OrderDescription = "order";
            public const string ChallengeDescription = "challenge";
            public const string ProfileDescription = "profile";
            public const string DigitizationDescription = "digitization";
            public const string MiscellaneousDescription = "data";
            public const string TaxIdDescription = "taxId";
            public const string CVV = "cvv";
            public const string TenantDescription = "tenant";
            public const string RewardsDescription = "rewards";
            public const string StaticDescription = "static";
            public const string PrerequisitesSuffix = "prerequisites";
            public const string BillingGroupDescription = "billingGroup";
            public const string AddressGroupDescription = "addressGroup";
            public const string FingerprintIFrameDescription = "fingerprintIFrame";
            public const string TimeoutFingerprintIFrameDescription = "timeoutFingerprintIFrame";
            public const string ThreeDSChallangeIFrameDescription = "threeDSChallengeIFrame";
            public const string ThreeDSAddPIIFrameDescription = "threeDSAddPIIFrameDescription";
            public const string AddressBillingV3 = "addressBillingV3";
            public const string AddressShippingV3 = "addressShippingV3";
            public const string TradeAVSModal = "TradeAVSModal";
            public const string ThirdPartyPaymentsCheckoutChallangeIFrame = "tppCheckoutChallengeIFrame";
            public const string CheckoutDescription = "checkout";
        }

        public static class ResourceTypes
        {
            public const string PaymentMethod = "paymentMethod";
            public const string PaymentInstrument = "paymentInstrument";
            public const string Address = "address";
            public const string Challenge = "challenge";
            public const string TaxId = "taxId";
            public const string Profile = "profile";
            public const string Rewards = "rewards";
        }

        public static class ChallengeDescriptionTypes
        {
            public const string TermsAndConditions = "termsAndConditions";
            public const string Cvv = "cvv";
            public const string Cvv3 = "cvv3";
            public const string Cvv4 = "cvv4";
            public const string Sms = "sms";
            public const string TokensSms = "tokensSms";
            public const string AlipayQrCode = "alipayQrCode";
            public const string ChallengeSelection = "challengeSelection";
            public const string ChallengeResolution = "challengeResolution";
            public const string ChallengeResolutionPhoneOffline = "challengeResolutionPhoneOffline";
            public const string ThreeDS = "threeds";
            public const string PaypalQrCodeXboxNative = "paypalQrCodeXboxNative";
            public const string PaypalQrCode = "paypalQrCode";
            public const string VenmoQrCode = "venmoQrCode";
            public const string KakaopayQrCode = "kakaopayQrCode";
            public const string GenericQrCode = "genericQrCode";
            public const string GlobalPIQrCode = "globalPIQrCode";
            public const string ThreeDSOneQrCode = "ThreeDSOneQrCode";
            public const string XboxCoBrandedCard = "xboxCoBrandedCard";
            public const string RewardsPhoneNumberQrCode = "rewardsPhoneNumberQrCode";
            public const string CreditCardQrCode = "creditCardQrCode";
        }

        public static class StaticDescriptionTypes
        {
            public const string Cc3DSRedirectPidl = "cc3DSRedirectPidl";
            public const string Cc3DSRedirectAndStatusCheckPidl = "cc3DSRedirectAndStatusCheckPidl";
            public const string Cc3DSStatusCheckPidl = "cc3DSStatusCheckPidl";
            public const string Cc3DSRedirectAndStatusCheckAddPIPidl = "cc3DSRedirectAndStatusCheckAddPIPidl";
            public const string Cc3DSStatusCheckAddPIPidl = "cc3DSStatusCheckAddPIPidl";
            public const string ThirdPartyPaymentsCheckoutErrorPidl = "tppCheckoutErrorPidl";
            public const string LegacyBillDesk3DSRedirectAndStatusCheckPidl = "legacyBillDesk3DSRedirectAndStatusCheckPidl";
            public const string LegacyBillDesk3DSStatusCheckPidl = "legacyBillDesk3DSStatusCheckPidl";
            public const string XboxCardNotEligibleErrorStaticPidl = "xboxCardNotEligibleErrorPidl";
            public const string XboxCardPendingErrorStaticPidl = "xboxCardPendingErrorPidl";
            public const string XboxCardApprovedErrorPidl = "xboxCardApprovedErrorPidl";
            public const string XboxCardInternalErrorStaticPidl = "xboxCardInternalErrorPidl";
            public const string XboxCardUpsellBuyNowPidl = "xboxCardUpsellBuyNowPidl";
            public const string SepaRedirectAndStatusCheckPidl = "sepaRedirectAndStatusCheckPidl";
        }

        public static class DigitizationDisplayLinkIds
        {
            public const string Next = "digitizationNext";
            public const string Cancel = "digitizationCancel";
            public const string Submit = "digitizationSubmit";
            public const string RevertChallenge = "digitizationChallengeRevert";
            public const string HaveCode = "challengeSelectionCodeLink";
        }

        public static class StyleHints
        {
            public const string ImageHeightSmall = "image-height-small";
            public const string AlignHorizontalCenter = "alignment-horizontal-center";
            public const string GapSmall = "gap-small";
            public const string WidthFill = "width-fill";
            public const string AlignverticalCenter = "align-vertical-center";
            public const string DirectionHorizontal = "direction-horizontal";
        }

        public static class DigitizationDisplayHintIds
        {
            public const string ChallengeResolutionText = "challengeResolutionText";
            public const string ChallengeResolutionPhoneOfflineText = "challengeResolutionPhoneOfflineText";
            public const string DigitizationCallBank = "digitizationCallBank";
            public const string ChallengeSelectionOption = "challengeSelectionOption";
            public const string TermsAndConditionsText = "termsAndConditionsText";
            public const string TermsAndConditionsWebView = "termsAndConditionsWebView";
            public const string DigitizationCardImage = "digitizationCardImage";
            public const string AccountHolderName = "accountHolderName";
            public const string LastFourDigits = "cardLastFourDigit";
        }

        public static class ImageDisplayHintIds
        {
            public const string XboxNativeSelectPMLogoTemplate = "{0}_logo";
            public const string XboxNativeSelectPMSubPageLogoTemplate = "{0}_logo_sub_page";
            public const string XboxNativeSelectPMSingleLogoTemplate = "single-{0}";
            public const string CardImage = "cardImage";
        }

        public static class DisplayHintTypes
        {
            public const string Image = "image";
            public const string Group = "group";
            public const string Heading = "heading";
            public const string SubHeading = "subheading";
            public const string Text = "text";
            public const string Button = "button";
            public const string Page = "page";
            public const string Hyperlink = "hyperlink";
            public const string TextGroup = "textgroup";
            public const string Logo = "logo";
            public const string Property = "property";
        }

        public static class PageDisplayHintIds
        {
            public const string AccountDetailsPageDisplayName = "AccountDetailsPage";
            public const string AccountSummaryPageDisplayName = "AccountSummaryPage";
            public const string NoAddressSuggestionsPage = "noAddressSuggestionsPage";
        }

        public static class GroupDisplayHintIds
        {
            public const string AlternativeSvgLogoWrapper = "alternativeSvgLogoWrapper_";
            public const string PaymentMethodOption = "paymentMethodOption";
            public const string PaymentOptionLogoGroup = "paymentOptionLogoGroup";
            public const string PaymentOptionLogoGroupCreditCards = "paymentOptionLogoGroupCreditCards";
            public const string PaymentOptionTextGroup = "paymentOptionTextGroup";
            public const string MultiplePaymentMethodLogosRowOneGroup = "multiplePaymentMethodLogosRowOneGroup";
            public const string MultiplePaymentMethodLogosRowTwoGroup = "multiplePaymentMethodLogosRowTwoGroup";
            public const string WarningIcon = "warning_icon";
            public const string PaymentOptionDisplayGroup = "paymentOptionDisplayGroup";
            public const string PaymentMethodColumnGroup = "paymentMethodColumnGroup";
            public const string PaymentInstrumentItemExpiredCCGroup = "paymentInstrumentItemExpiredCCGroup";
            public const string PaymentInstrumentItemStoredValueGroup = "paymentInstrumentItemStoredValueGroup";
            public const string PaymentOptionsGroup = "paymentOptionsGroup";
            public const string CardPointsTextWrapperGroup = "cardPointsTextWrapperGroup";
            public const string CardImageWrapperGroup = "cardImageWrapperGroup";
            public const string HapiFirstNameLastNameGroup = "hapiV1ModernAccountV20190531Address_firstAndLastNameGroup";
            public const string AddressOptionsGroup = "addressOptionsGroup";
            public const string AddressPostalCodeGroup = "addressPostalCodeGroup";
            public const string AddressStatePostalCodeGroup = "addressStatePostalCodeGroup";
            public const string AddressPostalCodeStateGroup = "addressPostalCodeStateGroup";
            public const string AddressProvincePostalCodeGroup = "addressProvincePostalCodeGroup";
            public const string AddressPostalCodeProvinceGroup = "addressPostalCodeProvinceGroup";
            public const string HapiV1ModernAccountV20190531AddressRegionAndPostalCodeGroup = "hapiV1ModernAccountV20190531Address_regionAndPostalCodeGroup";
            public const string HapiV1ModernAccountV20190531AddressRegionGroup = "hapiV1ModernAccountV20190531Address_regionGroup";
            public const string HapiV1ModernAccountV20190531AddressPostalCodeGroup = "hapiV1ModernAccountV20190531Address_postalCodeGroup";
            public const string HapiV1ModernAccountV20190531AddressPostalCodeAndRegionGroup = "hapiV1ModernAccountV20190531Address_postalCodeAndRegionGroup";
            public const string HapiV1ModernAccountV20190531IndividualAddressFirstAndLastNameGroup = "hapiV1ModernAccountV20190531IndividualAddress_firstAndLastNameGroup";
        }

        public static class LayoutOrientations
        {
            public const string Vertical = "vertical";
            public const string Inline = "inline";
        }

        public static class AutoSubmitIds
        {
            public const string XboxCoBrandedCardQrCode = "XboxCoBrandedCardQrCode";
        }

        public static class ButtonDisplayHintIds
        {
            public const string VerifySubmitButton = "verifySubmitButton";
            public const string NextButton = "nextButton";
            public const string PreviousButton = "previousButton";
            public const string SaveContinueButton = "saveContinueButton";
            public const string SaveButton = "saveButton";
            public const string ConfirmationButton = "confirmationButton";
            public const string DoneSubmitButton = "DoneSubmitButton";
            public const string AddButton = "addButton";
            public const string OkButton = "okButton";
            public const string AgreeAndContinueButton = "agreeAndContinueButton";
            public const string AgreeAndPayButton = "agreeAndPayButton";
            public const string VerifyCodeButton = "verifyCodeButton";
            public const string CancelButton = "cancelButton";
            public const string CancelBackButton = "cancelBackButton";
            public const string HiddenCancelBackButton = "hiddenCancelBackButton";
            public const string BackButton = "backButton";
            public const string PaypalCancelButton = "paypalCancelButton";
            public const string SubmitButton = "submitButton";
            public const string SearchSubmitButton = "searchSubmitButton";
            public const string SubmitButtonHidden = "submitButtonHidden";
            public const string SucessButton = "successButton";
            public const string SaveNextButton = "saveNextButton";
            public const string CsvRedeemVerifyBalanceButton = "csvRedeemVerifyBalanceButton";
            public const string CsvRedeemAddBalanceButton = "csvRedeemAddBalanceButton";
            public const string PaypalSaveNextButton = "paypalSaveNextButton";
            public const string SaveButtonHidden = "saveButtonHidden";
            public const string PaypalSignInButton = "paypalSignInButton";
            public const string PaypalYesButton = "paypalYesButton";
            public const string PaypalRedirectSubmitButton = "paypalRedirectSubmitButton";
            public const string VenmoYesButton = "venmoYesButton";
            public const string IdealYesButton = "idealYesButton";
            public const string IdealDoneButton = "idealDoneButton";
            public const string GenericYesButton = "genericYesButton";
            public const string AlipayContinueButton = "alipayContinueButton";
            public const string Cc3DSGoToBankButton = "cc3DSGoToBankButton";
            public const string UPIGoToBankButton = "upiGoToBankButton";
            public const string Cc3DSYesButton = "cc3DSYesButton";
            public const string UPIYesVerificationButton = "upiYesVerificationButton";
            public const string Cc3DSYesVerificationButton = "cc3DSYesVerificationButton";
            public const string BuyButton = "buyButton";
            public const string HiddenBuyButton = "hiddenBuyButton";
            public const string SuccessButtonHidden = "successButtonHidden";
            public const string UseButton = "useButtonPidlPayload";
            public const string UseButtonNext = "useButtonNext";
            public const string SendCodeButton = "sendCodeButton";
            public const string ContinueRedirectButton = "continueRedirectButton";
            public const string OkActionButton = "okActionButton";
            public const string ValidateButtonHidden = "validateButtonHidden";
            public const string VerifyPicvButton = "verifyPicvButton";
            public const string SaveWithValidationButton = "saveWithValidationButton";
            public const string ValidateThenSubmitButtonHidden = "validateThenSubmitButtonHidden";
            public const string ValidateThenSubmitButton = "validateThenSubmitButton";
            public const string ValidateThenSuccessWithPayloadButton = "validateThenSuccessWithPayloadButton";
            public const string ValidateThenSuccessWithPayloadButtonHidden = "validateThenSuccessWithPayloadButtonHidden";
            public const string AddressUseButton = "addressUseButton";
            public const string AddressNextButton = "addressNextButton";
            public const string UserEnteredButton = "userEnteredButton";
            public const string AddressBackButton = "addressBackButton";
            public const string PaypalQrCodeSignInButton = "PaypalQrCodeSignInButton";
            public const string VenmoSignInButton = "venmoSignInButton";
            public const string SaveAddressButton = "SaveAddressButton";
            public const string Cvv3DSSubmitButton = "cvv3DSSubmitButton";
            public const string PaySubmitButton = "paySubmitButton";
            public const string NextToPaypalButton = "nextButtonToPaypal";
            public const string BackButtonFromPaypal = "backButton";
            public const string LegacyBillDesk3DSYesButton = "legacyBillDesk3DSYesButton";
            public const string LegacyBillDesk3DSGoToBankButton = "legacyBillDesk3DSGoToBankButton";
            public const string ViewTermsButton = "viewTermsButton";
            public const string NextModernValidateButton = "nextModernValidateButton";
            public const string IndiaTokenConsentMessageHyperlink = "indiaTokenConsentMessageHyperlink";
            public const string SaveButtonSuccess = "saveButtonSuccess";
            public const string AddressChangeTradeAVSButton = "addressChangeTradeAVSButton";
            public const string AddressChangeButton = "addressChangeButton";
            public const string AddressChangeTradeAVSV2Button = "addressChangeTradeAVSV2Button";
            public const string SaveSecondScreenButton = "saveSecondScreenButton";
            public const string SepaGoToBankButton = "sepaGoToBankButton";
            public const string SepaStatusCheckYesButton = "sepaStatusCheckYesButton";
        }   

        public static class TextDisplayHintIds
        {
            public const string Expired = "expired--";
            public const string FixThisWayToPay = "fixThisWayToPay--";
            public const string PaymentOptionText = "paymentOptionText";
            public const string UseThisPaymentMethod = "useThisPaymentMethod--";
            public const string PlusMore = "plusMore";
            public const string AddressSuggestionMessage = "addressSuggestionMessage";
            public const string IndiaTokenConsentMessage = "indiaTokenConsentMessage";
            public const string CardPointsText = "cardPointsText";
            public const string CurrencyValueText = "currencyValueText";
            public const string CardPointsGroup = "cardPointsGroup";
            public const string CvvChallengeText = "cvvChallengeText";
            public const string AddressRecommandationMessage = "addressRecommandationMessage";
        }

        public static class DataSourceNames
        {
            public const string AddressResource = "addressResource";
        }

        public static class TextDisplayHintContents
        {
            public const string PlusMore = "+ more";
        }

        public static class HeadingDisplayHintIds
        {
            public const string AddCreditDebitHeading = "add_credit_debit_heading";
            public const string BillingAddressPageHeading = "billingAddressPageHeading";
        }

        public static class DisplayNames
        {
            public const string BillingAddress = "Billing address";
            public const string AllMandatoryFieldsText = "All fields are mandatory/required.";
            public const string Asterisk = "*";
        }

        public static class DisplayHintIds
        {
            public const string AddressNextGroup = "addressNextGroup";
            public const string AddressUseEnteredGroup = "addressUseEnteredGroup";
            public const string AddressCheckbox = "addressCheckbox";
            public const string AddressFirstName = "addressFirstName";
            public const string AddressMiddleName = "addressMiddleName";
            public const string AddressLastName = "addressLastName";
            public const string AddressFirstNameOptional = "addressFirstNameOptional";
            public const string AddressLastNameOptional = "addressLastNameOptional";
            public const string AddressEmailOptional = "emailAddressOptional";
            public const string AddressPhoneNumberOptional = "addressPhoneNumberOptional";
            public const string OrgAddressModernEmail = "orgAddressModern_email";
            public const string OrgAddressModernPhoneNumber = "orgAddressModern_phoneNumber";
            public const string NonSimMobiPhoneOperator = "non_sim_mobi_phone_operator";
            public const string PaymentSummaryText = "paymentSummaryText";
            public const string SmsChallengeText = "smsChallengeText";
            public const string SmsNewCodeLink = "smsNewCodeLink";
            public const string AlipayQrCodeChallengeImage = "alipayQrCodeChallengeImage";
            public const string AlipayQrCodeChallengeRedirectionLink = "alipayQrCodeChallengeRedirectionLink";
            public const string PicvRetryCount = "directDebitChallengeLine1";
            public const string PicvLastRetryCount = "directDebitChallengeLine2";
            public const string SepaRedirectButton = "sepaTryAgainButton";
            public const string SepaSuccessButton = "sepaYesButton";
            public const string NoProfileAddressText = "noProfileAddressText";
            public const string PaymentMethodSelect = "paymentMethod";
            public const string PaymentMethodTppSelect = "paymentMethodtpp";
            public const string PaymentMethodSelectPMGrouping = "paymentMethodPMGrouping";
            public const string PaymentMethodSelectDropdown = "paymentMethodDropdown";
            public const string PaymentInstrumentSearchId = "paymentInstrumentSearchTransactionsId";
            public const string PaymentInstrumentSearchTransactionsSubHeading = "paymentInstrumentSearchTransactionsSubHeading";
            public const string PaymentInstrumentSearchTransactionsCvv = "paymentInstrumentSearchTransactionsCvv";
            public const string SearchSubmitButtonGroup = "searchSubmitButtonGroup";
            public const string PaymentMethodSelectRadio = "paymentMethodRadio";
            public const string PaymentInstrumentSelect = "paymentInstrument";
            public const string BackupPaymentInstrumentSelect = "backupPaymentInstrument";
            public const string PaymentInstrumentSelectHeading = "paymentInstrumentSelectHeading";
            public const string AddNewAddressLink = "addNewAddressLink";
            public const string NewPaymentMethodLink = "newPaymentMethodLink";
            public const string NewPaymentStatementLink = "newPaymentStatementLink";
            public const string RedeemGiftCardLink = "redeemGiftCardLink";
            public const string SelectPaymentMethodLink = "selectPaymentMethodLink";
            public const string PidlContainer = "pidlContainer";
            public const string PaymentSelection = "paymentInstrumentSelection";
            public const string PaymentDisplay = "paymentInstrumentDisplay";
            public const string BackupPaymentSelection = "backupPaymentInstrumentSelection";
            public const string PaymentSelectionImage = "paymentSelectionImage";
            public const string BackupSelectionImage = "backupSelectionImage";
            public const string ChangeInstanceButton = "changeInstanceButton";
            public const string RemainingBalance = "remainingBalanceText";
            public const string BalanceText = "paymentInstrumentBalance";
            public const string BackupBalanceText = "backupPaymentInstrumentBalance";
            public const string AddNewCCButton = "addNewCreditCardLink";
            public const string PaymentInstrumentLogoBlock = "paymentInstrumentLogoBlock";
            public const string MicrosoftPrivacyTextGroup = "microsoftPrivacyTextGroup";
            public const string PaymentChangeSettingsTextGroup = "paymentChangeSettingsTextGroup";
            public const string CancelNextGroup = "cancelNextGroup";
            public const string CancelSaveGroup = "cancelSaveGroup";
            public const string SaveCancelGroup = "saveCancelGroup";
            public const string CancelGroup = "cancelGroup";
            public const string CancelAddGroup = "cancelAddGroup";
            public const string CancelBackGroup = "cancelBackGroup";
            public const string BackButton = "backButton";
            public const string BillingGroup = "billingGroup";
            public const string AddressGroup = "addressGroup";
            public const string BillingGroupListSIBillingGroupId = "billingGroupId";
            public const string PaymentInstrumentShowListPaymentInstrumentId = "paymentInstrumentId";
            public const string BillingGroupListAddBGHyperlinkId = "addNewBG";
            public const string BillingGroupListEditBillingDetailsHyperlinkId = "editBillingDetails";
            public const string ProfileEditLEHyperlinkId = "updateSoldToProfileLink";
            public const string IFramePageId = "IFramePage";
            public const string IFramePollStatusPageId = "IFramePollStatusPage";
            public const string ThreeDSFingerprintIFrameId = "ThreeDSFingerprintIFrame";
            public const string ThreeDSChallengeIFrameId = "ThreeDSChallengeIFrame";
            public const string VisaTokenIFrame = "visaTokenIFrame";
            public const string ThreeDSChallengePageName = "PaymentChallengePage";
            public const string ThreeDSIframeAddPIPageName = "ThreeDSIFrameAddPIPage";
            public const string ThreeDSIframeAddPIPollStatusPageName = "ThreeDSIFrameAddPIPollStatusPage";
            public const string ThreeDSTimeoutFingerprintIFrameId = "ThreeDSTimeoutFingerprintIFrame";
            public const string BillingGroupLightWeightAddNewPaymentInstrument = "billingGroupLightWeightAddNewPaymentInstrument";
            public const string AddressState = "addressState";
            public const string EditPaymentMethodLink = "editPaymentMethodLink";
            public const string SelectResourceNextLink = "selectResourceNextLink";
            public const string PaymentInstrumentShowPIChangeLink = "paymentInstrumentShowPIChangeLink";
            public const string XboxCoBrandedCardQrCodeHeading = "xboxCoBrandedCardQrCodeHeading";
            public const string XboxCoBrandedCardQrCodeSubheading = "xboxCoBrandedCardQrCodeSubheading";
            public const string XboxCoBrandedCardQrCodeText = "xboxCoBrandedCardQrCodeText";
            public const string XboxCoBrandedCardQrCodeBodyText = "xboxCoBrandedCardQrCodeBodyText";
            public const string XboxCoBrandedCardQrCodeDisclaimerText = "xboxCoBrandedCardQrCodeDisclaimerText";
            public const string XboxCoBrandedCardQrCodeImage = "xboxCoBrandedCardQrCodeImage";
            public const string XboxCoBrandedCardPageBackgroundImage = "xboxCoBrandedCardPageBackgroundImage";
            public const string XboxCoBrandedCardQrCodeRedirectButton = "xboxCoBrandedCardQrCodeRedirectButton";
            public const string XboxCoBrandedCardQrCodePage3 = "xboxCoBrandedCardQrCodePage3";
            public const string CreditCardQrCodeURLText = "creditCardQrCodeURLText";
            public const string CreditCardQrCodeImage = "creditCardQrCodeImage";
            public const string PaypalQrCodeChallengeImage = "paypalQrCodeImage";
            public const string PaypalQrCodeChallengeURLText = "paypalQrCodeURLText";
            public const string PaypalQrCodeRedirectButton = "paypalQrCodeRedirectButton";
            public const string PaypalQrCodeUseBrowserText = "paypalQrCodeUseBrowserText";
            public const string PaypalQrCodeChallengePage = "paypalQrCodeChallengePage";
            public const string PaypalQrCodeBackButton = "paypalQrCodeBackButton";
            public const string PaypalQrCodeChallengeLoginRedirectionLink = "paypalQrCodeChallengeLoginRedirectionLink";
            public const string PaypalPIShortUrl = "paypalPIShortUrl";
            public const string PaypalPIShortUrlInstruction = "paypalPIShortUrlInstruction";
            public const string PaypalPIShortUrlGroup = "paypalPIShortUrlGroup";
            public const string PaypalQrCodeImageAndURLGroup = "paypalQrCodeImageAndURLGroup";
            public const string PaypalQrCodeChallengePage3 = "paypalQrCodeChallengePage3";
            public const string VenmoShortUrl = "venmoShortUrl";
            public const string VenmoUrlInstructionText = "venmoUrlInstructionText";
            public const string VenmoURLGroup = "venmoURLGroup";
            public const string VenmoQrCodeImageAndURLGroup = "venmoQrCodeImageAndURLGroup";
            public const string GlobalPIQrCodeChallengeImage = "globalPIQrCodeImage";
            public const string GlobalPIQrCodeImageGroup = "globalPIQrCodeImageGroup";
            public const string GlobalPIQrCodeChallengeSecondImage = "globalPIQrCodeSecondImage";
            public const string GenericQrCodeChallengeImage = "genericQrCodeImage";
            public const string CCThreeDSQrCodeChallengeImage = "ccThreeDSQrCodeImage";
            public const string GoToBankButton = "goToBankButton";
            public const string CCThreeDSWebviewInstructionGroup = "ccThreeDSWebviewInstructionGroup";
            public const string GenericQrCodeChallengeURLText = "genericQrCodeURLText";
            public const string GenericQrCodeChallengePage = "genericQrCodeChallengePage";
            public const string VenmoQrCodeChallengeImage = "venmoQrCodeImage";
            public const string VenmoQrCodeChallengeURLText = "venmoQrCodeURLText";
            public const string VenmoQrCodeRedirectButton = "venmoQrCodeRedirectButton";
            public const string VenmoQrCodeUseBrowserText = "venmoQrCodeUseBrowserText";
            public const string VenmoQrCodeChallengePage = "venmoQrCodeChallengePage";
            public const string VenmoQrCodeBackButton = "venmoQrCodeBackButton";
            public const string PaymentInstrumentListPi = "paymentInstrumentListPi";
            public const string PaymentInstrument = "paymentInstrument";
            public const string PaymentInstrumentItemUpdateLink = "paymentInstrumentItemUpdateLink";
            public const string PaymentInstrumentItemReplaceLink = "paymentInstrumentItemReplaceLink";
            public const string PaymentInstrumentItemDeleteLink = "paymentInstrumentItemDeleteLink";
            public const string PaymentInstrumentItemCSVAddMoneyLink = "paymentInstrumentItemCSVAddMoneyLink";
            public const string PaymentInstrumentItemCSVRedeemLink = "paymentInstrumentItemCSVRedeemLink";
            public const string PaymentInstrumentItemCSVShopLink = "paymentInstrumentItemCSVShopLink";
            public const string PaymentInstrumentItemACHUpdateLink = "paymentInstrumentItemACHUpdateLink";
            public const string PaymentInstrumentItemACHViewMandateLink = "paymentInstrumentItemACHViewMandateLink";
            public const string PaymentInstrumentItemACHDeleteLink = "paymentInstrumentItemDeleteLink";
            public const string PaymentInstrumentItemPaypalDeleteLink = "paymentInstrumentItemPaypalDeleteLink";
            public const string PaymentInstrumentItemNSMDeleteLink = "paymentInstrumentItemNSMDeleteLink";
            public const string PaymentInstrumentListEligiblePi = "paymentInstrumentListEligiblePi";
            public const string FundStoredValueWithBitcoinRedeemAmountProperty = "bitcoinRedeemAmount";
            public const string FundStoredValueWithBitcoinRedeemPage = "fundStoredValueWithBitcoinRedeemPage";
            public const string BitpayIframe = "bitpayIframe";
            public const string ThreeDSIframe = "threeDSIframe";
            public const string SuggestedAddresses = "suggestedAddresses";
            public const string AddressSuggested = "addressSuggested";
            public const string AddressEntered = "addressEntered";
            public const string AddressEnteredGroup = "addressEnteredGroup";
            public const string AddressChangeGroup = "addressChangeGroup";
            public const string AddressChangeTradeAVSGroup = "addressChangeTradeAVSGroup";
            public const string AddressSuggestedTradeAVS = "addressSuggestedTradeAVS";
            public const string AddressSuggestedTradeAVSV2 = "addressSuggestedTradeAVSV2";
            public const string AddressEnteredTradeAVS = "addressEnteredTradeAVS";
            public const string AddressEnteredOnly = "addressEnteredOnly";
            public const string ListAddress = "listAddresses";
            public const string NewAddressLink = "newAddressLink";
            public const string PaypalRedirectLink = "paypalRedirectLink";
            public const string SepaRedirectLink = "sepaRedirectLink";
            public const string Cc3DSRedirectLink = "cc3DSRedirectLink";
            public const string PaypalNoButton = "paypalNoButton";
            public const string Cc3DSTryAgainButton = "cc3DSTryAgainButton";
            public const string UPITryAgainButton = "upiTryAgainButton";
            public const string Cc3DSRetryButton = "cc3DSRetryButton";
            public const string UPIRetryButton = "upiRetryButton";
            public const string PaypalRedirectTextGroup = "paypalRedirectTextGroup";
            public const string GenericRedirectLink = "genericredirectMessageLine1Link";
            public const string NoBackupPISelected = "NoBackupPISelected";
            public const string GlobalPIQrCodeIframe = "globalPIQrCodeIframe";
            public const string XboxCoBrandedCardIframe = "xboxCoBrandedCardIframe";
            public const string AddressEnteredOnlyLine1TradeAVS = "addressEnteredOnlyLine1TradeAVS";
            public const string AddressEnteredOnlyLine2TradeAVS = "addressEnteredOnlyLine2TradeAVS";
            public const string AddressEnteredOnlyCityRegionTradeAVS = "addressEnteredOnlyCityRegionTradeAVS";
            public const string AddressEnteredOnlyPostalCodeTradeAVS = "addressEnteredOnlyPostalCodeTradeAVS";
            public const string ThreeDSOneViewTermsFrame = "ThreeDSOneViewTermsFrame";
            public const string ThreeDSOneBankFrame = "ThreeDSOneBankFrame";
            public const string SuccessBackButton = "successBackButton";
            public const string MoveNext2Button = "moveNext2Button";
            public const string MoveBack2Button = "moveBack2Button";
            public const string OOBEPhoneConfirmButton = "oobePhoneConfirmButton";
            public const string CVV = "cvv";
            public const string CVVWithHint = "cvvWithHint";
            public const string CVVAmexWithHint = "cvvAmexWithHint";
            public const string SecureCVV = "secureCvv";
            public const string CVV3 = "cvv3";
            public const string SecureCVV3 = "secureCvv3";
            public const string CVV4 = "cvv4";
            public const string SecureCVV4 = "secureCvv4";
            public const string ChallengeCvvSecurityCode = "challengecvvSecurityCode";
            public const string ChallengeCvvSecurityCodeWithValidation = "challengecvvSecurityCodeWithValidation";
            public const string CVV3DSSubmitButton = "cvv3DSSubmitButton";
            public const string CC3DSPurchaseViewTermsButton = "cc3DSPurchaseViewTermsButton";
            public const string PrivacyWebviewBackButton = "privacyWebviewBackButton";
            public const string ThirdPartyPaymentsCheckoutChallengeIFrameId = "ThirdPartyPaymentsCheckoutChallengeIFrame";
            public const string ThirdPartyPaymentsCheckoutChallengePageName = "ThirdPartyPaymentsCheckoutChallengePage";
            public const string ThirdPartyPaymentsErrorPageCloseButton = "paymentErrorCloseButton";
            public const string NonSimMobiOperatorGroup = "nonSimMobiOperatorGroup";
            public const string NsmShowInfoHeaderGroup = "nsmShowInfoHeaderGroup";
            public const string DeletePageHeaderGroup = "deletePageHeaderGroup_Nsm";
            public const string PaymentMethodSelectHeading = "paymentMethodSelectHeading";
            public const string PaymentMethodPMGroupingSelectHeading = "paymentMethodPMGroupingSelectHeading";
            public const string BackGroup = "backGroup";
            public const string CreditCardAccountToken = "creditCardAccountToken";
            public const string IndiaTokenConsentMessageHyperlink = "indiaTokenConsentMessageHyperlink";
            public const string MoveNext = "moveNext";
            public const string CreditCardVisaWhereCVVGroup = "creditCardVisaWhereCVVGroup";
            public const string CreditCardVisaWhereCVVGroupUpdate = "creditCardVisaWhereCVVGroupUpdate";
            public const string CreditCardAmexWhereCVVGroupUpdate = "creditCardAmexWhereCVVGroupUpdate";
            public const string CreditCardMCWhereCVVGroupUpdate = "creditCardMCWhereCVVGroupUpdate";
            public const string CvvHelpGroup = "cvvHelpGroup";
            public const string CvvHelpNoLiveGroup = "cvvHelpNoLiveGroup";
            public const string CvvAmexHelpGroup = "cvvAmexHelpGroup";
            public const string TokenizationGroup = "tokenizationGroup";
            public const string IndiaTokenConsentMessage = "indiaTokenConsentMessage";
            public const string DisclaimerGroup = "disclaimerGroup";
            public const string ViewTermsButton = "viewTermsButton";
            public const string PrivacyStatementText = "microsoft_privacy_text";
            public const string SummaryFooterDonePreviousGroup = "summaryFooterDonePreviousGroup";
            public const string NumberDisplayHintId = "cardNumber";
            public const string SecureNumberDisplayHintId = "secureCardNumber";
            public const string AmexNumberDisplayHintId = "cardNumberAmex";
            public const string CupInternationalNumberDisplayHintId = "cardNumberCupInternational";
            public const string SecureAmexNumberDisplayHintId = "secureCardNumberAmex";
            public const string NameDisplayHintId = "cardholderName";
            public const string CreditCardMembersHintId = "creditCardSummaryGroup";
            public const string CreditCardGroupLine1HintId = "creditCardSummaryLine1";
            public const string CreditCardGroupLine2HintId = "creditCardSummaryLine2";
            public const string AddressCountry = "addressCountry";
            public const string AddressCounty = "addressCounty";
            public const string XboxCoBrandedCardQrCodeWebviewCancelButton = "xboxCoBrandedCardQrCodeWebviewCancelButton";
            public const string VenmoRedirectLink = "venmoRedirectLink";
            public const string UpiVpa = "upi_vpa";
            public const string BillingAddressPageHeader = "billingAddressPageHeader";
            public const string BookButton = "bookButton";
            public const string HapiCompanyName = "hapiV1ModernAccountV20190531Address_companyName";
            public const string HapiPhoneNumber = "hapiV1ModernAccountV20190531Address_phoneNumber";
            public const string HapiEmail = "hapiV1ModernAccountV20190531Address_email";
            public const string HapiFirstName = "hapiV1ModernAccountV20190531Address_firstName";
            public const string HapiMiddleName = "hapiV1ModernAccountV20190531Address_middleName";
            public const string HapiLastName = "hapiV1ModernAccountV20190531Address_lastName";
            public const string HapiCountry = "hapiV1ModernAccountV20190531Address_country";
            public const string HapiAddressLine1 = "hapiV1ModernAccountV20190531Address_addressLine1";
            public const string HapiAddressLine2 = "hapiV1ModernAccountV20190531Address_addressLine2";
            public const string HapiAddressLine3 = "hapiV1ModernAccountV20190531Address_addressLine3";
            public const string HapiRegion = "hapiV1ModernAccountV20190531Address_region";
            public const string HapiAddressHeading = "hapiV1AddressHeading";
            public const string PrivacyStatementHyperLinkDisplayText = "privacyStatement";
            public const string AddressDetailsDataGroup = "addressDetailsDataGroup";
            public const string PaymentOptionSaveText = "paymentOptionSaveText";
            public const string SuggestedAddressText = "suggestedAddressText";
            public const string Entered = "entered";
            public const string HapiTaxCountryProperty = "hapiTaxCountryProperty";
            public const string StarRequiredTextGroup = "starRequiredTextGroup";
            public const string ProfileAddressPageHeading = "profileAddressPageHeading";
            public const string CsvTotal = "csvTotal";
            public const string FormattedCsvTotal = "formattedCsvTotal";
            public const string PointsValueTotal = "pointsValueTotal";
            public const string FormattedPointsValueTotal = "formattedPointsValueTotal";
            public const string PaymentMethodHeading = "paymentMethodHeading";
            public const string GlobalPIQrCodeRedirectButton = "globalPIQrCodeRedirectButton";
            public const string GlobalPIQrCodeRedirectButtonPage2 = "globalPIQrCodeRedirectButtonPage2";
            public const string RewardsEditPhoneNumberQrCodeImage = "rewardsEditPhoneNumberQrCodeImage";
            public const string SuggestBlock = "suggestBlock";
            public const string Space = "space";
            public const string AddressSuggestedGroup = "addressSuggestedGroup";
            public const string AddressOptionsTradeAVSGroup = "addressOptionsTradeAVSGroup";
            public const string AddressOptionsGroup = "addressOptionsGroup";
            public const string PaymentInstrumentItemWalletCardGroup = "paymentInstrumentItemWalletCardGroup";
            public const string PaymentInstrumentItemWalletDetailsGroup = "paymentInstrumentItemWalletDetailsGroup";
            public const string PaymentInstrumentItemWalletColumnGroup = "paymentInstrumentItemWalletColumnGroup";
            public const string AddressOptionsTradeAVSV2Group = "addressOptionsTradeAVSV2Group";
            public const string MicrosoftPrivacyStaticText = "microsoft_privacy_static_text";
            public const string PointsRedemptionNegativeFormattedPointsValueTotalExpression = "pointsRedemptionNegativeFormattedPointsValueTotalExpression";
            public const string PointsRedemptionNegativeFormattedPointsValueTotalAccentedExpression = "pointsRedemptionNegativeFormattedPointsValueTotalAccentedExpression";
            public const string UseCsvNegativeFormattedCsvTotalExpression = "useCsvNegativeFormattedCsvTotalExpression";
            public const string UseCsvNegativeFormattedCsvTotalAccentedExpression = "useCsvNegativeFormattedCsvTotalAccentedExpression";
            public const string AcceptCardMessage = "accept_card_message";
            public const string VerticalTextTopGroup = "verticalTextTopGroup";
            public const string EwalletStoredValueOption = "ewallet_stored_value";
            public const string AddressLine1 = "addressLine1";
            public const string AddressLine2 = "addressLine2";
            public const string AddressLine3 = "addressLine3";
            public const string AddressProvince = "addressProvince";
            public const string AddressCity = "addressCity";
            public const string AddressPostalCode = "addressPostalCode";
            public const string AddressStatePostalCodeGroup = "addressStatePostalCodeGroup";
            public const string AddressPostalCodeStateGroup = "addressPostalCodeStateGroup";
            public const string AddressProvincePostalCodeGroup = "addressProvincePostalCodeGroup";
            public const string AddressPostalCodeProvinceGroup = "addressPostalCodeProvinceGroup";

            public const string AcceptedAmexCardGroup = "acceptedAmexCardGroup";
            public const string AcceptedVisaCardGroup = "acceptedVisaCardGroup";
            public const string AcceptedMCCardGroup = "acceptedMCCardGroup";
            public const string AcceptedDiscoverCardGroup = "acceptedDiscoverCardGroup";
            public const string AcceptedJCBCardGroup = "acceptedJCBCardGroup";
            public const string AcceptedEloCardGroup = "acceptedEloCardGroup";
            public const string AcceptedVerveCardGroup = "acceptedVerveCardGroup";
            public const string AcceptedHipercardCardGroup = "acceptedHipercardCardGroup";
            public const string AcceptedCupInternationalCardGroup = "acceptedCupInternationalCardGroup";
            public const string CreditCardRupayLogo = "creditCardRupayLogo";
            public const string CreditCardCupLogo = "cupImage";
            public const string AddCCQrCodeImage = "addCCQrCodeImage";
            public const string MandatoryFieldsMessage = "mandatory_fields_message";

            public const string KlarnaAddressLine1 = "klarnaAddressLine1";
            public const string KlarnaAddressLine2 = "klarnaAddressLine2";
            public const string KlarnaAddressPostalCode = "klarnaAddressPostalCode";
            public const string HapiV1ModernAccountV20190531AddressCity = "hapiV1ModernAccountV20190531Address_city";
            public const string HapiIndividualCompanyName = "hapiV1ModernAccountV20190531IndividualAddress_companyName";
            public const string HapiIndividualLastName = "hapiV1ModernAccountV20190531IndividualAddress_lastName";
            public const string HapiIndividualFirstName = "hapiV1ModernAccountV20190531IndividualAddress_firstName";
            public const string UpdateLegalEntityAddressLine1 = "updateLegalEntityAddressLine1";
            public const string UpdateLegalEntityAddressLine2 = "updateLegalEntityAddressLine2";
            public const string UpdateLegalEntityAddressLine3 = "updateLegalEntityAddressLine3";
            public const string UpdateProfileAddressCity = "updateProfileAddressCity";
            public const string UpdateProfileAddressState = "updateProfileAddressState";
            public const string UpdateLegalEntityAddressPostalCode = "updateLegalEntityAddressPostalCode";
            public const string UpdateProfileAddressCountry = "updateProfileAddressCountry";
            public const string UpdateProfileAddressCounty = "updateProfileAddressCounty";
            public const string UpdateProfileAddressProvince = "updateProfileAddressProvince";
            public const string AddressPhoneNumber = "addressPhoneNumber";
            public const string AddressStateGroup = "addressStateGroup";
            public const string UpdateProfileAddressLine1 = "updateProfileAddressLine1";
            public const string UpdateProfileAddressLine2 = "updateProfileAddressLine2";
            public const string UpdateProfileAddressLine3 = "updateProfileAddressLine3";
            public const string UpdateProfileAddressPostalCode = "updateProfileAddressPostalCode";
            public const string OrgAddressModernAddressLine1 = "orgAddressModern_addressLine1";
            public const string OrgAddressModernAddressLine2 = "orgAddressModern_addressLine2";
            public const string OrgAddressModernAddressLine3 = "orgAddressModern_addressLine3";
            public const string OrgAddressModernCity = "orgAddressModern_city";
            public const string OrgAddressModernRegion = "orgAddressModern_region";
            public const string OrgAddressModernPostalCode = "orgAddressModern_postalCode";
            public const string OrgAddressModernCountry = "orgAddressModern_country";
            public const string AddressCorrespondenceName = "addressCorrespondenceName";
            public const string AddressMobile = "addressMobile";
            public const string AddressFax = "addressFax";
            public const string AddressTelex = "addressTelex";
            public const string AddressEmailAddress = "addressEmailAddress";
            public const string AddressWebSiteUrl = "addressWebSiteUrl";
            public const string AddressStreetSupplement = "addressStreetSupplement";
            public const string AddressIsWithinCityLimits = "addressIsWithinCityLimits";
            public const string AddressFormOfAddress = "addressFormOfAddress";
            public const string AddressAddressNotes = "addressAddressNotes";
            public const string AddressTimeZone = "addressTimeZone";
            public const string AddressLatitude = "addressLatitude";
            public const string AddressLongitude = "addressLongitude";
            public const string HapiSUALine1 = "hapiSUALine1";
            public const string HapiSUALine2 = "hapiSUALine2";
            public const string HapiSUALine3 = "hapiSUALine3";
            public const string HapiSUACity = "hapiSUACity";
            public const string HapiSUAState = "hapiSUAState";
            public const string HapiSUAPostalCode = "hapiSUAPostalCode";
            public const string HapiSUACountryCode = "hapiSUACountryCode";
            public const string HapiSUACounty = "hapiSUACounty";
            public const string HapiSUAPhoneNumber = "hapiSUAPhoneNumber";
            public const string HapiSUAProvince = "hapiSUAProvince";
            public const string ShippingAddressState = "shippingAddressState";
            public const string AddressCountyGB = "addressCountyGB";
            public const string AddressPhoneNumberWithExplanation = "addressPhoneNumberWithExplanation";
            public const string ShippingAddressLine1 = "shippingAddressLine1";
            public const string ShippingAddressLine2 = "shippingAddressLine2";
            public const string ProfileAddressFirstName = "profileAddressFirstName";
            public const string ProfileAddressLastName = "profileAddressLastName";
            public const string ProfileAddressLine1 = "profileAddressLine1";
            public const string ProfileAddressLine2 = "profileAddressLine2";
            public const string ProfileAddressLine3 = "profileAddressLine3";
            public const string ProfileAddressCity = "profileAddressCity";
            public const string ProfileAddressState = "profileAddressState";
            public const string ProfileAddressPostalCode = "profileAddressPostalCode";
            public const string ProfileAddressPhoneNumber = "profileAddressPhoneNumber";
            public const string ProfileAddressCounty = "profileAddressCounty";
            public const string ProfileAddressProvince = "profileAddressProvince";
            public const string CancelButton = "cancelButton";
            public const string UpdateCCLogo = "updateCCLogo";
            public const string TaxVatIdProperty = "taxVatIdProperty";
            public const string TaxCpfProperty = "taxCpfProperty";
            public const string PrefillBillingAddressCheckbox = "prefillBillingAddressCheckbox";
            public const string DirectDebitSepaUpdateLine1 = "directDebitSepaUpdateLine1";
        }

        public static class ScenarioNames
        {
            public const string MonetaryCommit = "monetaryCommit";
            public const string MonetaryCommitModernAccounts = "monetaryCommitModernAccounts";
            public const string AddressNoCityState = "addressNoCityState";
            public const string BillingGroupPONumber = "billingGroupPONumber";
            public const string WithProfileAddress = "withProfileAddress";
            public const string PaypalQrCode = "paypalQrCode";
            public const string XboxCoBrandedCard = "xboxCoBrandedCard";
            public const string GenericQrCode = "genericQrCode";
            public const string ModernAccount = "modernAccount";
            public const string DepartmentalPurchase = "departmentalPurchase";
            public const string WithCountryDropdown = "withCountryDropdown";
            public const string TwoColumns = "twoColumns";
            public const string EligiblePI = "eligiblePI";
            public const string FixedCountrySelection = "fixedCountrySelection";
            public const string SuggestAddresses = "suggestAddresses";
            public const string DisplayOptionalFields = "displayOptionalFields";
            public const string Profile = "profile";
            public const string PaymentMethodAsDropdown = "paymentMethodAsDropdown";
            public const string PayNow = "payNow"; // amcweb paynow
            public const string ChangePI = "changePI"; // amcweb changepi
            public const string SuggestAddressesTradeAVS = "suggestAddressesTradeAVS";
            public const string SuggestAddressesTradeAVSUsePidlPageV2 = "suggestAddressesTradeAVSUsePidlPageV2";
            public const string SuggestAddressesTradeAVSUsePidlModal = "suggestAddressesTradeAVSUsePidlModal";
            public const string SuggestAddressesProfile = "suggestAddressesProfile";
            public const string ProfileAddress = "profileAddress";
            public const string Commercialhardware = "commercialhardware";
            public const string ThreeDSOnePolling = "threedsonepolling";
            public const string IndiaThreeDS = "indiathreeds";
            public const string SelectPMWithLogo = "selectPMWithLogo";
            public const string PMGrouping = "pmGrouping";
            public const string PollingAction = "pollingAction";
            public const string Roobe = "roobe"; // (Redesigned Out Of Box Experience) OOBE for Windows 11
            public const string VenmoWebPolling = "VenmoWebPolling";
            public const string VenmoQRCode = "venmoQrCode";
            public const string SecondScreenAddPi = "secondScreenAddPi";
        }

        public static class ChallengeDisplayHintIds
        {
            public const string CardPaymentName = "challengeCardPaymentName";
            public const string CardLogo = "challengeCardLogo";
            public const string CardName = "challengeCardName";
            public const string CardNumber = "challengeCardNumber";
            public const string CardExpiry = "challengeCardExpiry";
            public const string CvvChallengeHeading = "cvvChallengeHeading";
            public const string CvvChallengeText = "cvvChallengeText";
            public const string ChallengeCvv = "challengeCvv";
            public const string CvvToken = "cvvToken";
        }

        public static class AddCCDisplayHintIds
        {
            public const string MCLogo = "creditCardMCLogo";
        }

        public static class ListPIDisplayHintIds
        {
            public const string CirclePlusIcon = "circlePlusIcon_";
        }

        public static class UnicodeValues
        {
            public const string PlusCircle = "\uECC8";
        }

        public static class DisplayHintIdPrefixes
        {
            public const string PaymentOptionContainer = "optionContainer_";
            public const string PaymentOptionLogo = "optionLogo_";
            public const string PaymentOptionLogoGroup = "optionLogoGroup_";
            public const string PaymentOptionLogoType = "optionLogoType";
            public const string PaymentOptionFontIcon = "optionFontIcon_";
            public const string PaymentOptionText = "optionText_";
            public const string PaymentOptionAction = "optionAction_";
            public const string PaymentOptionDisplayGroup = "optionDisplayGroup_";
            public const string PaymentOptionExpiredInlineGroup = "optionExpiredInlineGroup_";
            public const string PaymentOptionSelectedTextGroup = "optionSelectedTextGroup_";
            public const string PaymentOptionTextGroup = "optionTextGroup_";
            public const string PaymentOptionBalance = "optionBalance_";
            public const string PaymentOptionDisabled = "optionDisabled_";
            public const string PaymentOptionUpdate = "optionUpdate_";
            public const string PaymentOptionEdit = "optionEdit_";
            public const string AddressOptionContainer = "optionAddress_";
            public const string PaymentOptionTextTemplate = "optionPaymentText_part{0}_{1}";
            public const string AddNSMLogo = "add_nsm_logo_{0}";
            public const string PaymentMethodSubGroupPage = "paymentMethodSubGroupPage_";
            public const string PaymentMethodSubGroupPageHeading = "paymentMethodSubGroupPageHeading_";
            public const string PaymentMethodLogoContainer = "logoContainer_";
            public const string PaymentMethodOption = "paymentMethodOption_";
        }

        public static class DataDescriptionIds
        {
            public const string BackupId = "backupId";
            public const string BackupInstance = "backupInstance";
            public const string Instance = "instance";
            public const string PaymentInstrumentFamily = "paymentMethodFamily";
            public const string PaymentInstrumentType = "paymentMethodType";
            public const string PaymentInstrumentAction = "action";
            public const string PaymentInstrumentId = "id";
            public const string BillingGroupId = "id";
            public const string AddressGroupId = "id";
            public const string Id = "id";
            public const string AddressSuggestId = "address_suggest_id";
            public const string PaymentInstrumentDisplayId = "displayId";
            public const string Region = "region";
            public const string Address = "address";
            public const string Amount = "amount";
            public const string DisplayId = "displayId";
            public const string DefaultAddress = "default_address";
            public const string Details = "details";
        }

        public static class DescriptionIdentityFields
        {
            public const string DescriptionType = "description_type";
            public const string Family = "family";
            public const string Type = "type";
            public const string Country = "country";
            public const string Operation = "operation";
            public const string Locale = "locale";
            public const string Step = "step";
            public const string Scenario = "scenario";
            public const string CountryCode = "country_code";
            public const string ResourceIdentity = "resource_id";
            public const string PaymentInstrumentId = "id";
            public const string BackupPaymentInstrumentId = "backupId";
            public const string SessionId = "session_id";
            public const string Value = "value";
        }

        public static class Instances
        {
            public const string RedeemGiftCardLink = "redeemGiftCardLink";
        }

        public static class PaymentMethodFamilyNames
        {
            public const string CreditCard = "credit_card";
            public const string MobileBillingNonSim = "mobile_billing_non_sim";
            public const string Ewallet = "ewallet";
            public const string DirectDebit = "direct_debit";
            public const string Virtual = "virtual";
            public const string OnlineBankTransfer = "online_bank_transfer";
            public const string RealTimePayments = "real_time_payments";
        }

        public static class PaymentMethodTypeNames
        {
            public const string Amex = "amex";
            public const string MasterCard = "mc";
            public const string Visa = "visa";
            public const string Discover = "discover";
            public const string CupCreditCard = "unionpay_creditcard";
            public const string CupDebitCard = "unionpay_debitcard";
            public const string Alipay = "alipay_billing_agreement";
            public const string Paypal = "paypal";
            public const string StoredValue = "stored_value";
            public const string StoredValueRedeem = "stored_value.redeem";
            public const string InvoiceBasicVirtual = "invoice_basic";
            public const string InvoiceCheckVirtual = "invoice_check";
            public const string Bitcoin = "bitcoin";
            public const string Klarna = "klarna";
            public const string JapanCreditBureau = "jcb";
            public const string HiperCard = "hipercard";
            public const string Elo = "elo";
            public const string Paysafecard = "paysafecard";
            public const string IdealBillingAgreement = "ideal_billing_agreement";
            public const string Verve = "verve";
            public const string Venmo = "venmo";
            public const string Upi = "upi";
            public const string UpiCommercial = "upi_commercial";
            public const string Rupay = "rupay";
            public const string LegacyInvoice = "legacy_invoice";
            public const string PayPay = "paypay";
            public const string AlipayHK = "alipayhk";
            public const string GCash = "gcash";
            public const string TrueMoney = "truemoney";
            public const string TouchNGo = "touchngo";
            public const string AlipayCN = "alipaycn";
            public const string Sepa = "sepa";
            public const string MSRewards = "MSRewards";
        }

        public static class PaymentMethodCardProductTypes
        {
            public const string XboxCreditCard = "XboxCreditCard";
        }

        public static class PaymentMethodTypeNonSimMobileNames
        {
            public const string TdcDenmark = "dkt-dk-nonsim";
            public const string KpnNetherlands = "nlk-nl-nonsim";
            public const string NetNorway = "net-no-nonsim";
            public const string AtOneAustria = "at1-at-nonsim";
            public const string DigiMalaysia = "dig-my-nonsim";
            public const string M1Singapore = "m13-sg-nonsim";
            public const string OrangeSpain = "org-es-nonsim";
            public const string SunriseSwitzerland = "sun-ch-nonsim";
            public const string StarHubSingapore = "sta-sg-nonsim";
            public const string TmobileTeleringAustria = "tmo-at-nonsim";
            public const string TmobileCzechia = "tmo-cz-nonsim";
            public const string TmobileUnitedKingdom = "tmo-gb-nonsim";
            public const string TmobileGermany = "tmo-de-nonsim";
            public const string TmobileNetherlands = "tmo-nl-nonsim";
            public const string TmobileSlovakia = "tmo-sk-nonsim";
            public const string ZainSouthAfrica = "zai-sa-nonsim";
            public const string MtcRussia = "mts-ru-nonsim";
            public const string ProBelgium = "pro-be-nonsim";
            public const string TliDenmark = "tli-dk-nonsim";
            public const string VivoBrazil = "viv-br-nonsim";
            public const string EraPoland = "era-pl-nonsim";
        }

        public static class PaymentMethodTypeNonSimAlternateLogoSvg
        {
            public const string TdcDenmark = "v2_logo_dkt_dk_nonsim.svg";
            public const string KpnNetherlands = "v2_logo_nlk_nl_nonsim.svg";
            public const string NetNorway = "v2_logo_net_no_nonsim.svg";
            public const string AtOneAustria = "v2_logo_at1_at_nonsim.svg";
            public const string DigiMalaysia = "v2_logo_dig_my_nonsim.svg";
            public const string M1Singapore = "v2_logo_m13_sg_nonsim.svg";
            public const string OrangeSpain = "v2_logo_org_es_nonsim.svg";
            public const string SunriseSwitzerland = "v2_logo_sun_ch_nonsim.svg";
            public const string StarHubSingapore = "v2_logo_sta_sg_nonsim.svg";
            public const string TmobileTeleringAustria = "v2_logo_tmo_at_nonsim.svg";
            public const string TmobileCzechia = "v2_logo_tmo_cz_nonsim.svg";
            public const string TmobileUnitedKingdom = "v2_logo_tmo_gb_nonsim.svg";
            public const string TmobileGermany = "v2_logo_tmo_de_nonsim.svg";
            public const string TmobileNetherlands = "v2_logo_tmo_nl_nonsim.svg";
            public const string TmobileSlovakia = "v2_logo_tmo_sk_nonsim.svg";
            public const string ZainSouthAfrica = "v2_logo_zai_sa_nonsim.svg";
            public const string MtcRussia = "v2_logo_mts_ru_nonsim.svg";
            public const string ProBelgium = "v2_logo_pro_be_nonsim.svg";
            public const string TliDenmark = "v2_logo_tli_dk_nonsim.svg";
            public const string VivoBrazil = "v2_logo_viv_br_nonsim.svg";
            public const string EraPoland = "v2_logo_era_pl_nonsim.svg";
        }

        public static class PaymentMethodTypePaysafeAlternateLogoSvg
        {
            public const string PaysafeCard = "v2_logo_paysafecard.svg";
        }

        public static class JarvisEndpoints
        {
            public const string MyFamily = "my-family";
        }

        public static class KoreaCreditCardType
        {
            internal static readonly IList<string> TypeNames = new ReadOnlyCollection<string>(
                new List<string>
                {
                    "shinhan",
                    "bc",
                    "kb_kook_min",
                    "samsung",
                    "hyundai",
                    "lotte",
                    "nh",
                    "hana",
                    "citi",
                    "jeju",
                    "woori",
                    "suhyup",
                    "jeonbok",
                    "kwangju",
                    "shinhyup"
                });
        }

        public static class PIDLPaymentMethodTypeNames
        {
            public const string AlipayEmail = "alipayEmail";
            public const string AlipayMobile = "alipayMobile";
            public const string AlipayQrCode = "alipayQrCode";
            public const string PaypalRedirect = "paypalRedirect";
            public const string PaymentMethodWithProfileAddressSuffix = "_with_profile_address";
            public const string PaypalQrCode = "paypalQrCode";
            public const string KlarnaCheckout = "klarnaCheckout";
            public const string XboxCardRedirect = "xboxCardRedirect";
            public const string XboxCardNotEligible = "xboxCardNotEligible";
            public const string XboxCardPending = "xboxCardPending";
            public const string XboxCardApplicationComplete = "xboxCardApplicationComplete";
            public const string VenmoQrCode = "venmoQrCode";
        }

        public static class PIDLProfileTypeNames
        {
            public const string ConsumerWallet = "consumerWallet";
        }

        public static class BillingGroupTypeNames
        {
            public const string LightWeight = "lightweight";
            public const string LightWeightV7 = "lightweightv7";
        }

        public static class PaymentMethodKeyNames
        {
            public const string Family = "family";
            public const string Type = "type";
        }

        public static class PaymentMethodId
        {
            public const string CreditCardVisa = "credit_card_visa";
            public const string CreditCardAmex = "credit_card_amex";
            public const string CreditCardMC = "credit_card_mc";
            public const string CreditCardDiscover = "credit_card_discover";
            public const string PayPal = "ewallet_paypal";
            public const string AliPay = "ewallet_alipay";
            public const string AliPayBillingAgreement = "ewallet_alipay_billing_agreement";
        }

        public static class ErrorCodes
        {
            public const string PIDLConfigFileDoesNotExist = "500000";
            public const string PIDLConfigFileInvalidNumberOfColumns = "500001";
            public const string PIDLConfigFileRequiredColumnIsMissing = "500002";
            public const string PIDLConfigFileColumnIsMalformed = "500003";
            public const string PIDLConfigUnknownPaymentMethodId = "500004";
            public const string PIDLConfigPIDLResourceIdIsMalformed = "500005";
            public const string PIDLConfigPropertyDescriptionIdIsMalformed = "500006";
            public const string PIDLConfigPropertyOrInfoDescriptionIdIsMalformed = "500007";
            public const string PIDLConfigPIDLResourceForIdIsMissing = "500008";
            public const string PIDLConfigDuplicateId = "500009";
            public const string PIDLConfigUnknownCountryId = "500010";
            public const string PIDLConfigUnknownPaymentMethodFamilyId = "500011";
            public const string PIDLConfigUnknownDictionaryName = "500012";
            public const string PIDLConfigDuplicateDataDescription = "500013";
            public const string PIDLConfigUnknownDisplayHintId = "500014";
            public const string PIDLConfigMissingGroupHintSequence = "500015";
            public const string PIDLConfigMissingPossibleValues = "500016";
            public const string PIDLConfigMissingDisplayDescriptions = "500017";
            public const string PIDLConfigInvalidPageConfiguration = "500018";
            public const string PIDLConfigUnknownBooleanValue = "500019";
            public const string PIDLConfigActivationMethodTypeIsMissing = "500020";
            public const string PIDLConfigInvalidTransformationTarget = "500021";
            public const string PIDLConfigInvalidTransformationCategory = "500022";
            public const string PIDLConfigMissingDataDescription = "500023";
            public const string PIDLConfigInvalidSourceUrlFormat = "500024";

            public const string PIDLArgumentCountryIsNullOrEmpty = "CountryIsNullOrEmpty";
            public const string PIDLArgumentCountryIsInvalid = "CountryIsInvalid";
            public const string PIDLArgumentOperationIsInvalid = "OperationIsInvalid";
            public const string PIDLArgumentFamilyIsNullOrEmpty = "PaymentMethodFamilyIsNullOrEmpty";
            public const string PIDLArgumentFamilyIsInvalid = "PaymentMethodFamilyIsInvalid";
            public const string PIDLArgumentFamilyIsNotSupportedForStoreInCountry = "PaymentMethodFamilyIsNotSupportedForStoreInCountry";
            public const string PIDLArgumentTypeIsInvalid = "PaymentMethodTypeIsInvalid";
            public const string PIDLArgumentTypeIsNotSupportedForStoreInCountry = "PaymentMethodTypeIsNotSupportedForStoreInCountry";
            public const string PIDLArgumentChallengeHasNullDigitizedCard = "ChallengeHasNullDigitizedCard";
            public const string PIDLArgumentChallengeHasNullAccountId = "ChallengeHasNullAccountId";
            public const string PIDLArgumentChallengeTypeIsInvalid = "ChallengeTypeIsInvalid";
            public const string PIDLArgumentAddressTypeIsNullOrBlank = "AddressTypeIsNullOrEmpty";
            public const string PIDLArgumentNoPaymentMethodsForCountryAndStore = "PaymentMethodsForCountryOrStoreNotSupported";
            public const string PIDLArgumentStoreIsNullOrBlank = "StoreIsNullOrEmpty";
            public const string PIDLArgumentDomainDictionaryNameIsNullOrBlank = "DomainDictionaryNameIsNullOrEmpty";
            public const string PIDLArgumentDisplayDictionaryNameIsNullOrBlank = "DisplayDictionaryNameIsNullOrEmpty";
            public const string PIDLArgumentDigitizationStepIsInvalid = "DigitizationStepIsInvalid";
            public const string PIDLArgumentScenarioIsNullOrEmpty = "ScenarioIsNullOrEmpty";
            public const string PIDLArgumentScenarioIsInvalid = "ScenarioIsInvalid";
            public const string PIDLArgumentRewardsTypeIsNullOrEmpty = "RewardsTypeIsNullOrEmpty";
            public const string PIDLArgumentRewardsTypeIsInvalid = "RewardsTypeIsInvalid";
            public const string PIDLArgumentTenantTypeIsNullOrEmpty = "TenantTypeIsNullOrEmpty";
            public const string PIDLArgumentSepaPicvRetryTimeIsInvalid = "SepaPicvRetryTimeIsInvalid";
            public const string PIDLProperyDescriptionNotFound = "PropertyDescriptionNotFound";
            public const string PIDLPaymentMethodTypeOrFamilyIsNotSupportedInCountry = "PaymentMethodTypeOrFamilyIsNotSupportedInCountry";
            public const string PIDLMissingCountryCodeInIdentity = "MissingCountryCodeInIdentity";
            public const string PIDLMissingCountryCodeInPhoneNumberValidation = "MissingCountryCodeInPhoneNumberValidation";
            public const string PIDLValidationTypeNotSupported = "ValidationTypeNotSupported";
            public const string PIDLTransformationTypeNotSupported = "TransformationTypeNotSupported";
            public const string PIDLInvalidPhoneNumberForCountry = "InvalidPhoneNumberForCountry";
            public const string PIDLCountryCodeNotSupported = "CountryCodeNotSupported";
            public const string PIDLArgumentChallengeDescriptionIdIsNullOrBlank = "InvalidChallengeId";
            public const string PIDLArgumentChallengeDescriptionIdInvalid = "ChallengeIdNotSupported";
            public const string PIDLArgumentChallengeDescriptionIdInvalidForPi = "InvalidChallengeIdForPi";
            public const string PIDLInvalidValidationParameter = "InvalidValidationParameter";
            public const string PIDLInvalidTransformationParameter = "InvalidTransformationParameter";
            public const string PIDLTransformationNotFoundForProperty = "TransformationNotFoundForProperty";
            public const string PIDLPartnerNameIsNotValid = "PIDLPartnerNameIsNotValid";
            public const string PIDLFlightHeaderNotFound = "PIDLFlightHeaderNotFound";
            public const string PIDLDisplayDescriptionNotFoundForFlight = "PIDLDisplayDescriptionNotFoundForFlight";
            public const string PIDLInvalidUrl = "PIDLInvalidUrl";
            public const string PIDLActivationMethodTypeIsNotValid = "PIDLActivationMethodTypeIsNotValid";
            public const string PIDLInvalidDisplayStringMapping = "PIDLInvalidDisplayStringMapping";
            public const string PIDLDisplayDescriptionNotFound = "PIDLDisplayDescriptionNotFound";
            public const string PIDLArugumentIsNullOrEmpty = "PIDLArugumentIsNullOrEmpty";
            public const string PIDLInvalidAllowedPaymentMethods = "AllowedPaymentMethodsIsInvalid";
            public const string PIDLInvalidFilters = "FiltersIsInvalid";
            public const string PIDLCouldNotLookupBankCode = "PIDLCouldNotLookupBankCode";
            public const string PIDLCouldNotGenerateQrCode = "PIDLCouldNotGenerateQrCode";
        }

        public static class ConfigSpecialStrings
        {
            public const string CollectionNamePrefix = "{}";
            public const string CollectionDelimiter = ";";
            public const string NameValueDelimiter = "=";
            public const string CountryId = "()CountryId";
            public const string Language = "()Language";
            public const string Operation = "()Operation";
            public const string EmailAddress = "()EmailAddress";
            public const string PaymentMethodDisplayName = "()PMDisplayName";
            public const string PaymentMethodSvgLogo = "()PMSvgLogoUrl";
            public const string Channel = "()Channel";
            public const string ChargeThresholdsMaxPrice = "()ChargeThresholdsMaxPrice";
            public const string FirstName = "()FirstName";
            public const string LastName = "()LastName";
        }

        public static class DomainDictionaryNames
        {
            public const string MSFTCommerceCountries = "MarketsAll";
            public const string TaxIdTypes = "TaxIdTypes";
            public const string USStates = "USStates";
            public const string BRStates = "BRStates";
        }

        public static class HTTPVerbs
        {
            public const string GET = "GET";
            public const string POST = "POST";
            public const string PUT = "PUT";
            public const string PATCH = "PATCH";
            public const string DELETE = "DELETE";
        }

        public static class LinkNames
        {
            public const string Self = "self";
            public const string CommerceJS = "commercejs";
            public const string SubmitUrl = "submit_url";
            public const string NextPidl = "nextPidl";
        }

        public static class ActivationType
        {
            public const string EmailActivation = "emailactivationcode";
            public const string TextActivation = "textactivationcode";
            public const string CallCustomerService = "callcustomerservice";
            public const string WebRedirect = "webredirect";
            public const string AppRedirection = "appredirection";
            public const string IssuerCallBackActivationCode = "issuercallbackactivationcode";
        }

        public static class CustomHeaders
        {
            public const string PidlFlightName = "pidl-flight-name";
            public const string ApiVersion = "api-version";
            public const string MsCorrelationId = "x-ms-correlation-id";
            public const string MsTrackingId = "x-ms-tracking-id";
            public const string IfMatch = "If-Match";
            public const string MSFlight = "x-ms-flight";
        }

        public static class PendingOperations
        {
            public const string ResumeSubPath = "/resume?partner={0}";
            public const string ActivationMethodIdProperty = "activationMethodId";
            public const string CancelSubPath = "/remove";
            public const string RevertChallengeSubPath = "&revertChallengeOption=true&partner={0}";
            public const string HaveCodeSubPath = "&partner={0}";
        }

        public static class FontIcons
        {
            public const string PlusSign = "\uE710";
            public const string GiftCard = "\uE8C7";
        }

        public static class StaticResourceNames
        {
            public const string AddBoldSvg = "add_bold_64px_dark_grey.svg";
            public const string GiftCardSvg = "gift_card_64px_dark_grey.svg";
            public const string WarningIcon = "icon_warning.svg";
            public const string MasterCardLogoLeftAligned = "logo_mc_left_aligned.svg";
            public const string VisaSvg = "v2_logo_visa.svg";
            public const string MCSvg = "logo_mc.svg";
            public const string DiscoverSvg = "logo_discover.svg";
            public const string AmexSvg = "logo_amex.svg";
            public const string JapanCreditBureauSvg = "logo_jcb.svg";
            public const string HiperCardSvg = "logo_hipercard.svg";
            public const string EloSvg = "logo_elo.svg";
            public const string VervePng = "v2_logo_verve.png";
            public const string VenmoSvg = "v2_logo_venmo.svg";
            public const string PaysafeCardPng = "logo_paysafecard.png";
        }

        public static class MaxAllowedPaymentMethodLogos
        {
            public const int Six = 6;
        }

        public static class XboxNativeEditPIHeadings
        {
            public const string EditYourBillingAddress = "Edit your billing address";
            public const string EditVisa = "(Edit Visa ending in {accountToken})";
            public const string EditMC = "(Edit Mastercard ending in {accountToken})";
            public const string EditDiscover = "(Edit Discover ending in {accountToken})";
            public const string EditAmex = "(Edit American Express ending in {accountToken})";
            public const string EditVerve = "(Edit Verve ending in {accountToken})";
            public const string EditElo = "(Edit Elo ending in {accountToken})";
            public const string EditHiperCard = "(Edit Hipercard ending in {accountToken})";
            public const string EditJCB = "(Edit JCB ending in {accountToken})";
        }

        public static class NativeStyleHints
        {
            public static readonly List<string> DummyStyleHint = new List<string>() { "dummy-stylehint" };
            public static readonly List<string> ListAddressSelectOptionStyleHints = new List<string>() { "height-auto", "margin-end-medium", "padding-vertical-x-small" };
            public static readonly List<string> AddressOptionContainerStyleHints = new List<string>() { "height-large", "width-medium", "padding-horizontal-small", "padding-vertical-medium" };
            public static readonly List<string> SelectPMOptionStyleHints = new List<string>() { "width-small-200", "height-small", "margin-end-small", "padding-vertical-x-small", "margin-vertical-medium" };
            public static readonly List<string> SelectPIOptionStyleHints = new List<string> { "width-medium", "height-medium", "padding-vertical-medium", "margin-end-small", "padding-bottom-small", "margin-vertical-medium" };
            public static readonly List<string> SelectPIButtonListStyleHints = new List<string>() { "layout-inline", "alignment-vertical-center", "padding-start-medium" };
            public static readonly List<string> SmallBoldText = new List<string> { "text-bold", "font-size-small", "line-height-small" };
            public static readonly List<string> WarningIcon = new List<string> { "text-bold", "font-size-2x-small", "line-height-x-small" };
            public static readonly List<string> FontIcon = new List<string> { "font-family-segoe-mdl2-assets", "font-size-large", "line-height-small-600", "margin-top-one-fifth" };
            public static readonly List<string> SuggestedAddressOptionStyleHints = new List<string>() { "height-auto", "width-medium", "margin-end-small", "padding-vertical-x-small" };
            public static readonly List<string> SuggestedAddressOptionsListStyleHints = new List<string>() { "layout-inline", "alignment-vertical-center", "padding-horizontal-small" };
            public static readonly List<string> AddPISuggestedAddressOptionsListStyleHints = new List<string>() { "layout-inline", "alignment-vertical-center", "padding-horizontal-x-small" };
        }

        public static class NativeDisplayTagValues
        {
            public const string SelectionBorderGutterAccent = "selection-border-gutter-accent";
        }

        internal static class PicvStatus
        {
            public const string InProgress = "inProgress";
        }

        internal static class PartnerNames
        {
            public const string Commercialstores = "commercialstores";
            public const string Cart = "cart";
            public const string Bing = "bing";
            public const string BingTravel = "bingtravel";
            public const string DefaultPartner = "default";
            public const string Webblends = "webblends";
            public const string Wallet = "wallet";
            public const string WebblendsInline = "webblends_inline";
            public const string Xbox = "xbox";
            public const string WebPay = "webpay";
            public const string AmcWeb = "amcweb";
            public const string AmcXbox = "amcxbox";
            public const string OfficeOobe = "officeoobe";
            public const string OfficeOobeInApp = "officeoobeinapp";
            public const string SmbOobe = "smboobe";
            public const string Azure = "azure";
            public const string AzureSignup = "azuresignup";
            public const string AzureIbiza = "azureibiza";
            public const string Mseg = "mseg";
            public const string OneDrive = "onedrive";
            public const string GGPDEDS = "GGPDEDS";
            public const string Payin = "payin";
            public const string SetupOffice = "setupoffice";
            public const string SetupOfficeSdx = "setupofficesdx";
            public const string StoreOffice = "storeoffice";
            public const string CommercialSupport = "commercialsupport";
            public const string ConsumerSupport = "consumersupport";
            public const string NorthStarWeb = "northstarweb";
            public const string OXODIME = "oxodime";
            public const string OXOWebDirect = "oxowebdirect";
            public const string OXOOobe = "oxooobe";
            public const string Storify = "storify";
            public const string XboxNative = "xboxnative";
            public const string XboxSubs = "xboxsubs";
            public const string XboxSettings = "xboxsettings";
            public const string Saturn = "saturn";
            public const string WindowsNative = "windowsnative";
            public const string WindowsSubs = "windowssubs";
            public const string WindowsSettings = "windowssettings";
            public const string XboxWeb = "xboxweb";
            public const string Battlenet = "battlenet";
        }

        internal static class TemplateName
        {
            public const string OnePage = "onepage";
            public const string TwoPage = "twopage";
            public const string SelectPMButtonList = "selectpmbuttonlist";
            public const string SelectPMRadioButtonList = "selectpmradiobuttonlist";
            public const string SelectPMDropDown = "selectpmdropdown";
            public const string DefaultTemplate = "defaulttemplate";
            public const string ListPiDropDown = "listpidropdown";
            public const string ListPiRadioButton = "listpiRadioButton";
            public const string ListPiButtonList = "listPiButtonList";
        }

        internal static class DataDescriptionPropertyNames
        {
            public const string Country = "country";
            public const string CVV = "cvvToken";
            public const string TokenizationConsent = "tokenizationConsent";
        }

        internal static class PendingOperation
        {
            internal const string Status = "status";
            internal const string Pending = "Pending";
            internal const string TermsAndConditions = "TermsAndConditions";
            internal const string TermsAndConditionsUrl = "termsAndConditionsUrl";
            internal const string TermsAndConditionsContext = "termsAndConditions";
            internal const string Url = "url";
            internal const string MimeType = "mimeType";
            internal const string Cvv = "Cvv";
            internal const string ChallengeSelection = "ChallengeSelection";
            internal const string ChallengeResolution = "ChallengeResolution";
            internal const string UserIdentificationAndVerification = "UserIdentificationAndVerification";
            internal const string Notification = "notification";
            internal const string ProvisionConfirmation = "ProvisionConfirmation";
            internal const string ActivationMethods = "activationMethods";
            internal const string Id = "id";
            internal const string ActivationType = "activationType";
            internal const string Value = "value";
            internal const string SelectedActivationMethod = "selectedActivationMethod";
        }

        internal static class RestResourceNames
        {
            internal const string PaymentInstrumentsEx = "paymentInstrumentsEx";
            internal const string PaymentInstruments = "paymentInstruments";
            internal const string PaymentTransactions = "paymentTransactions";
            internal const string Profiles = "profiles";
            internal const string ProfileAddresses = "addresses";
            internal const string TaxIds = "tax-ids";
            internal const string PaymentSessions = "paymentSessions";
            internal const string TokenDescriptionRequests = "tokenDescriptionRequests";
            internal const string Orders = "orders";
            internal const string Tokens = "tokensEx";
        }

        internal static class PaymentMethodOptionStrings
        {
            internal const string WebblendsCreditCard = "Credit card or debit card";
            internal const string WebblendsCreditCardBr = "Credit card"; //// #17660099 Debit is not an approved payment method in Brazil
            internal const string WebblendsCreditCardSe = "Debit card or credit card"; //// #26469247 Debit card must be before credit card in Sweden
            internal const string WebblendsNonSim = "Mobile phone";
            internal const string StoredValue = "Redeem a gift card";
            internal const string Disabled = "Not available for this purchase.";
            internal const string Update = "Update";
            internal const string Edit = "Edit";
            internal const string AddAWayToPay = "Add a way to pay";
            internal const string RedeemGiftCard = "Redeem a gift card";
            internal const string UseThisPaymentMethod = "Use this payment method";
            internal const string FixThisWayToPay = "Fix this way to pay";
            internal const string UsePaysafecard = "Use Paysafecard";
            internal const string PMGroupingCreditCard = "Credit or debit card";
        }

        internal static class PaymentMethodFormatStrings
        {
            internal const string MixerFormat = "{0} ending in {1}";
            internal const string UpdateYourCard = "Update your {0} ** {1} card";
            internal const string EditYourCard = "Edit {0} {1} ••{2} card";
            internal const string StarFormat = "{0} ** {1}";
            internal const string DotFormat = "{0} ••{1}";
            internal const string PointsFormat = "Card Points: {0}";
            internal const string PointsCurrencyFormat = "({0})";
        }

        internal static class PaymentMethodSelectType
        {
            internal const string Radio = "radio";
            internal const string DropDown = "dropDown";
            internal const string ButtonList = "buttonList";
        }

        internal static class PartnerHintsValues
        {
            internal const string InlinePlacement = "inline";
            internal const string VerticalPlacement = "vertical";
            internal const string PopupPlacement = "popup";
            internal const string PartnerSubmit = "partner";
            internal const string TriggeredByUpdateButton = "updateButton";
            internal const string TriggeredBySubmitGroup = "submitGroup";
            internal const string TriggeredByEmptyResourceList = "emptyResourceList";
        }

        internal static class HiddenOptionalFields
        {
            internal const string ContextKey = "NamesOfPropertyToHide";
            internal const char ContextDelimiter = '|';
            internal static readonly IList<string> AddressDescriptionPropertyNames = new ReadOnlyCollection<string>(
                new List<string>
                {
                    "address_line2",
                    "address_line3"
                });
        }

        internal static class AccountServiceApiVersion
        {
            public const string V2 = "v2";
            public const string V3 = "v3";
        }

        internal static class ApiVersions
        {
            public const string JarvisV3 = "2015-03-31";
            public const string BillingAuthFrontDoorApiVersion = "2016-02-28";
            public const string ModernAccountV20190531 = "2019-05-31";
        }

        internal static class AccountResourceType
        {
            public const string Address = "addresses";
            public const string Profile = "profiles";
        }

        internal static class ProfileTypes
        {
            public const string Consumer = "consumer";
            public const string ConsumerV3 = "consumerV3";
            public const string Organization = "organization";
            public const string Employee = "employee";
            public const string Isv = "isv";
            public const string Legal = "legalentity";
        }

        internal static class UseridTypes
        {
            public const string Me = "me";
            public const string MyOrg = "my-org";
        }

        internal static class AddressTypes
        {
            public const string BillingGroup = "billinggroup";
            public const string HapiServiceUsageAddress = "hapiServiceUsageAddress";
            public const string HapiV1SoldToOrganization = "hapiV1SoldToOrganization";
            public const string HapiV1ShipToOrganization = "hapiV1ShipToOrganization";
            public const string HapiV1BillToOrganization = "hapiV1BillToOrganization";
            public const string HapiV1SoldToIndividual = "hapiV1SoldToIndividual";
            public const string HapiV1ShipToIndividual = "hapiV1ShipToIndividual";
            public const string HapiV1BillToIndividual = "hapiV1BillToIndividual";
            public const string HapiV1SoldToOrganizationCSP = "hapiV1SoldToOrganizationCSP";
            public const string OrgAddress = "orgAddress";
            public const string ShippingV3 = "shipping_v3";
            public const string Shipping = "shipping";
            public const string Billing = "billing";
            public const string PXV3Shipping = "px_v3_shipping";
            public const string PXV3Billing = "px_v3_billing";
            public const string SoldTo = "soldTo";
            public const string Internal = "internal";
            public const string HapiV1 = "hapiV1";
        }

        internal static class AccountResourceOperation
        {
            public const string Create = "create";
            public const string Update = "update";
        }

        internal static class SubmitUrls
        {
            public const string PifdBaseUrl = "https://{pifd-endpoint}/users/{userId}";
            public const string PifdAddressCreateUrlTemplate = PifdBaseUrl + "/addresses";
            public const string PifdProfileCreateUrlTemplate = PifdBaseUrl + "/profiles";
            public const string PifdProfileUpdateUrlTemplate = "https://{{pifd-endpoint}}/users/{{userId}}/profiles/{0}/update";
            public const string PifdAddressPostUrlTemplate = PifdBaseUrl + "/addressesEx";

            public const string PifdAnonymousLegacyAddressValidationUrl = "https://{pifd-endpoint}/anonymous/addresses/legacyValidate";
            public const string PifdAnonymousLegacyAddressValidationWithTypeUrl = "https://{{pifd-endpoint}}/anonymous/addresses/legacyValidate?type={0}";
            public const string PifdAnonymousModernAVSForTrade = "https://{{pifd-endpoint}}/anonymous/addresses/ModernValidate?type={0}&partner={1}&language={2}&scenario={3}&country={4}";
            public const string PifdAnonymousModernAddressValidationUrl = "https://{pifd-endpoint}/anonymous/addresses/modernValidate";

            public const string JarvisFdAddressCreateUrlTemplate = "https://{jarvis-endpoint}/JarvisCM/{userId}/addresses";
            public const string JarvisFdProfileCreateUrlTemplate = "https://{jarvis-endpoint}/JarvisCM/{userId}/profiles";
            public const string JarvisFdProfileUpdateUrlTemplate = "https://{{jarvis-endpoint}}/JarvisCM/{{userId}}/profiles/{0}";
            public const string JarvisFdProfileUpdateClientPrefillingUrlTemplate = "https://{jarvis-endpoint}/JarvisCM/{userId}/profiles/{id}";
            public const string JarvisFdFamilyProfileUpdateUrlTemplate = "https://{{jarvis-endpoint}}/JarvisCM/{0}/profiles/{{partnerData.prefillData.childProfileId}}";

            public const string HapiProfileUpdateUrlTemplate = "https://{{hapi-endpoint}}/{0}/profiles/{{id}}";
            public const string HapiBillingGroupBaseUrl = "https://{hapi-endpoint}/{userId}/billinggroup";
            public const string HapiBillingGroupV7BaseUrl = "https://{hapi-endpoint}/my-org/billinggroupv7";
            public const string HapiLegalEntityProfileUrlTemplate = "https://{hapi-endpoint}/{userId}/customerService/{id}/updateSoldToAddress";
            public const string HapiBillingGroupUpdatePONumberUrlTemplate = "https://{hapi-endpoint}/{userId}/billinggroup/{id}";
            public const string HapiBillingGroupV7UpdatePONumberUrlTemplate = "https://{hapi-endpoint}/my-org/billinggroupv7/{id}";
            public const string HapiUpdateServiceUsageAddressUrlTemplate = "https://{hapi-endpoint}/my-org/orders/{partnerData.prefillData.orderId}/orderservice.updateServiceUsageAddress";
            public const string HapiV1SoldToOrganization = "https://{hapi-endpoint}/complexOrganization/soldTo?accountId={accountId}&organizationId={orgId}";
            public const string HapiV1ShipToOrganizationAdd = "https://{hapi-endpoint}/complexOrganization/shipTo?accountId={accountId}&organizationId={orgId}";
            public const string HapiV1ShipToOrganizationUpdate = "https://{hapi-endpoint}/complexOrganization/shipTo?accountId={accountId}&organizationId={orgId}&shipToBusinessLocationId={businessLocId}";
            public const string HapiV1BillToOrganization = "https://{hapi-endpoint}/my-org/billinggroupv7/{billingGroupId}/updateBillToByValue";

            public const string HapiV1SoldToIndividual = "https://{hapi-endpoint}/complexOrganization/soldTo?accountId={partnerData.prefillData.accountId}&organizationId={partnerData.prefillData.orgId})";

            public const string MockUrl = "https://mockurl";

            public const string PifdTokensMandatesUrlTemplate = "https://{pifd-endpoint}/users/{userId}/tokensEx/{tokenId}/mandates";
        }

        internal static class RedirectUrls
        {
            public const string RedirectTemplate = "https://{{redirection-endpoint}}/RedirectionService/CoreRedirection/Redirect/{0}";
            public const string RedirectTemplateV2 = "https://{{redirectionv2-endpoint}}/RedirectionService/CoreRedirection/Redirect/{0}";
        }

        internal static class AnonymousResumePaymentInstumentsExUrls
        {
            public const string ThreeDSOne = "{0}/paymentInstrumentsEx/{1}/resume?country={2}&language={3}&partner={4}&isSuccessful={5}&sessionQueryUrl={6}";
        }

        internal static class ImageResolution
        {
            public const int QrCodeImageResolution = 10;
        }

        internal static class SecondScreenQRCode
        {
            public const string PayMicrosoftINT = "https://payint.microsoft.com/addCard?sessionId={0}&country={1}&language={2}&partner={3}&type={4}&paymentFamily={5}&scenario={6}";
            public const string PayMicrosoftPPE = "https://payppe.microsoft.com/addCard?sessionId={0}&country={1}&language={2}&partner={3}&type={4}&paymentFamily={5}&scenario={6}";
            public const string PayMicrosoftPROD = "https://pay.microsoft.com/addCard?sessionId={0}&country={1}&language={2}&partner={3}&type={4}&paymentFamily={5}&scenario={6}";
            public const string PayMicrosoftEndpoint = "https://{{paymicrosoft-endpoint}}/addCard?sessionId={0}&country={1}&language={2}&partner={3}&type={4}&paymentFamily={5}&scenario={6}";
        }

        internal static class RedirectUrlStaticRuRx
        {
            public const string PaypalQrcodeChallengeRedirectUrlRuRx = "?ru=https%3A%2F%2Fwww.microsoft.com%2F{0}%2Fstore%2Frds%2Fv2%2FGeneralAddPISuccess%3FpicvRequired%3DFalse%26type%3Dpaypal%26family%3Dewallet%26id%3D{1}&rx=https%3A%2F%2Fwww.microsoft.com%2F{0}%2Fstore%2Frds%2Fv2%2FGeneralAddPIFailure";
            public const string PaypalQrcodeChallengeXboxNativeRedirectUrlRuRx = "?ru=https%3A%2F%2Fwww.microsoft.com%2F{0}%2Fstore%2Frds%2Fv2%2FGeneralAddPISuccess%3FpicvRequired%3DFalse%26type%3Dpaypal%26family%3Dewallet%26id%3D{1}%26redirectType%3D{2}&rx=https%3A%2F%2Fwww.microsoft.com%2F{0}%2Fstore%2Frds%2Fv2%2FGeneralAddPIFailure";
            public const string KakaopayQrcodeChallengeXboxNativeRedirectUrlRuRx = "?ru=https%3A%2F%2Fwww.microsoft.com%2F{0}%2Fstore%2Frds%2Fv2%2FGeneralAddPISuccess%3FpicvRequired%3DFalse%26type%3Dkakaopay%26family%3Dewallet%26id%3D{1}%26redirectType%3D{2}&rx=https%3A%2F%2Fwww.microsoft.com%2F{0}%2Fstore%2Frds%2Fv2%2FGeneralAddPIFailure";
            public const string GenericQrcodeChallengeRedirectUrlRuRx = "?ru=https%3A%2F%2Fwww.microsoft.com&rx=https%3A%2F%2Fwww.microsoft.com";
            public const string GlobalPIQrcodeChallengeRedirectUrlRuRx = "?rx=https%3A%2F%2Fwww.microsoft.com%2F{0}%2Fstore%2Frds%2FpurchaseFailure%3FsessionId%3D{1}&ru=https%3A%2F%2Fwww.microsoft.com%2F{0}%2Fstore%2Frds%2FpurchaseSuccess%3FsessionId%3D{1}";
            public const string ThreeDSOneChallengeRedirectUrlRuRx = "?ru=https%3A%2F%2Fwww.microsoft.com%2F{0}%2Fstore%2Frds%2Fv2%2FGeneralAddPISuccess%3FpicvRequired%3DFalse%26type%3D{2}%26family%3D{3}%26id%3D{1}&rx=https%3A%2F%2Fwww.microsoft.com%2F{0}%2Fstore%2Frds%2Fv2%2FGeneralAddPIFailure";
            public const string ThreeDSOneChallengeXboxNativeRedirectUrlRuRx = "?ru=https%3A%2F%2Fwww.microsoft.com%2F{0}%2Fstore%2Frds%2Fv2%2FGeneralAddPISuccess%3FpicvRequired%3DFalse%26type%3D{2}%26family%3D{3}%26id%3D{1}%26redirectType%3D{4}&rx=https%3A%2F%2Fwww.microsoft.com%2F{0}%2Fstore%2Frds%2Fv2%2FGeneralAddPIFailure";
            public const string ThreeDSOnePurchaseRedirectUrlRuRx = "?ru=https%3A%2F%2Fwww.microsoft.com%2F{0}%2Fstore%2Frds%2FPurchaseRiskChallengeRedirectSuccess&rx=https%3A%2F%2Fwww.microsoft.com%2F{0}%2Fstore%2Frds%2FPurchaseRiskChallengeRedirectFailure";
            //// TODO: Update to appropriate redirect url (waiting for PIMS)
            public const string VenmoQrcodeChallengeXboxNativeRedirectUrlRuRx = "";
        }

        internal static class HandleGlobalQrCodePIPendingPurchaseUrls
        {
            public const string HandleGlobalPendingPurchaseQrCodeUrlTemplate = "https://pmservices.cp.microsoft.com/RedirectionService/CoreRedirection/redirect/{0}";
            public const string QueryGlobalPendingPurchaseStateRedirectionServiceUrlTemplate = "https://pmservices.cp.microsoft.com/RedirectionService/CoreRedirection/Query/{0}";
            public const string QueryGlobalPendingPurchaseStatePurchaseServiceUrlTemplate = "https://purchase.mp.microsoft.com/v7.0/users/me/orders/{0}";
            public const string XboxCoBrandedCardRedirectionUrlTemplate = "https://{{redirection-endpoint}}/RedirectionService/CoreRedirection/Query/{0}";
        }

        internal static class ConfirmPaymentForUPIUrls
        {
            public const string QueryUPIPendingStateRedirectionServicePollUrlTemplate = "https://{{redirection-endpoint}}/RedirectionService/CoreRedirection/Query/{0}";
            public const string HandleRedirectUrlTemplate = "https://{{redirection-endpoint}}/RedirectionService/CoreRedirection/redirect/{0}";
        }

        internal static class PollingIntervals
        {
            public const int GenericPollingInterval = 3000;
            public const int PaypalPollingIntervalDefault = 3000;
            public const int PaypalPollingIntervalFiveSeconds = 5000;
            public const int PaypalPollingIntervalTenSeconds = 10000;
            public const int PaypalPollingIntervalFifteenSeconds = 15000;
            public const int BitpayPollingInterval = 1000;
            public const int GlobalPollingInterval = 3000;
            public const int ThreeDSOnePollingInterval = 3000;
            public const int XboxCardPollingInterval = 3000;
        }

        internal static class PollingMaxAttempts
        {
            public const int GenericPollingMaxTimes = 600;
            public const int PayPalPollingMaxTimeSixHundred = 600;
        }

        internal static class PollingResponseResultExpression
        {
            public const string GenericResponseResultExpression = "status";
            public const string PaypalResponseResultExpression = "status";
            public const string ThreeDSOneResponseResultExpression = "status";
            public const string BitcoinResponseResultExpression = "status";
            public const string XboxCoBrandedCardPendingResultExpression = "session_state";
            public const string XboxCoBrandedCardFinalResultExpression = "status";
            public const string ThreeDSOnePurchaseResponseResultExpression = "clientAction.context.challengeStatus";
            public const string GlobalPIResponseKeyExpressionForRedirectionService = "session_state";
            public const string GlobalPIResponseKeyExpressionForPurchaseService = "orderState";
            public const string CheckoutStatusResultExpression = "checkoutStatus";
            public const string APMStatusResultExpression = "clientAction.context.challengeStatus";
        }

        internal static class PollingResponseActionKey
        {
            public const string BitcoinFundStoredValueSuccess = "completed";
        }

        internal static class PayPalBillingAgreementTypes
        {
            internal const string MerchantInitiatedBilling = "MerchantInitiatedBilling";
            internal const string MerchantInitiatedBillingSingleAgreement = "MerchantInitiatedBillingSingleAgreement";
            internal const string ChannelInitiatedBilling = "ChannelInitiatedBilling";
            internal const string Unknown = "Unknown";
        }

        internal static class PendingOnOperations
        {
            internal const string Picv = "picv";
            internal const string AgreementUpdate = "agreementUpdate";
        }

        internal static class PhoneNumberErrorMessages
        {
            internal const string TooShort = "Phone number is too short.";
            internal const string TooLong = "Phone number is too long.";
            internal const string Invalid = "Invalid phone number.";
        }

        internal static class IFrameContentTemplates
        {
            internal const string PostThreeDSMethodData = "<html><head><script src=\"https://pmservices.cp.microsoft.com/staticresourceservice/scripts/v6/Prod/psd2_csp_onload_submit.js\" type=\"text/javascript\"></script></head><body><form action=\"{0}\" method=\"post\"><input type=\"hidden\" name=\"threeDSMethodData\" value=\"{1}\" /><input type=\"hidden\" name=\"cspStep\" value=\"{2}\" /></form></body></html>";
            internal const string PostThreeDSSessionData = "<html><head><script src=\"https://pmservices.cp.microsoft.com/staticresourceservice/scripts/v6/Prod/psd2_csp_onload_submit.js\" type=\"text/javascript\"></script></head><body><form action=\"{0}\" method=\"post\"><input type=\"hidden\" name=\"creq\" value=\"{1}\" /><input type=\"hidden\" name=\"threeDSSessionData\" value=\"{2}\" /><input type=\"hidden\" name=\"cspStep\" value=\"{3}\" /></form></body></html>";
            internal const string CSPPostThreeDSMethodData = "<html><script src=\"https://pmservices.cp.microsoft.com/staticresourceservice/scripts/v6/Prod/psd2_csp_receive_msg.js\" type=\"text/javascript\"></script><iframe frameborder=\"0\" style=\"border: none; width: 100%; height: 100%;\" srcdoc='<html><body onload=\"document.forms[0].submit();\"><form action=\"{0}\" method=\"post\"><input type=\"hidden\" name=\"threeDSMethodData\" value=\"{1}\" /></form></body></html>'></iframe></html>";
            internal const string CSPPostThreeDSSessionData = "<html><script src=\"https://pmservices.cp.microsoft.com/staticresourceservice/scripts/v6/Prod/psd2_csp_receive_msg.js\" type=\"text/javascript\"></script><iframe frameborder=\"0\" style=\"border: none; width: 100%; height: 100%;\" srcdoc='<html><body onload=\"document.forms[0].submit();\"><form action=\"{0}\" method=\"post\"><input type=\"hidden\" name=\"creq\" value=\"{1}\" /><input type=\"hidden\" name=\"threeDSSessionData\" value=\"{2}\" /></form></body></html>'></iframe></html>";
            internal const string CSPPostThreeDSMethodDataSrc = "<html><script src=\"https://pmservices.cp.microsoft.com/staticresourceservice/scripts/v6/Prod/psd2_csp_receive_msg.js\" type=\"text/javascript\"></script><iframe frameborder=\"0\" style=\"border: none; width: 100%; height: 100%;\" src='data:text/html,<html><body onload=\"document.forms[0].submit();\"><form action=\"{0}\" method=\"post\"><input type=\"hidden\" name=\"threeDSMethodData\" value=\"{1}\" /></form></body></html>'></iframe></html>";
            internal const string CSPPostThreeDSSessionDataSrc = "<html><script src=\"https://pmservices.cp.microsoft.com/staticresourceservice/scripts/v6/Prod/psd2_csp_receive_msg.js\" type=\"text/javascript\"></script><iframe frameborder=\"0\" style=\"border: none; width: 100%; height: 100%;\" src='data:text/html,<html><body onload=\"document.forms[0].submit();\"><form action=\"{0}\" method=\"post\"><input type=\"hidden\" name=\"creq\" value=\"{1}\" /><input type=\"hidden\" name=\"threeDSSessionData\" value=\"{2}\" /></form></body></html>'></iframe></html>";
            internal const string ThreeDSOneRedirect = "<html><body onload=\"window.location.href = '{0}'\"></body></html>";
            internal const string PostThreeDSOneSessionData = "<html><body onload=\"document.forms[0].submit();\"><form action=\"{0}\" method=\"post\">{1}</form></body></html>";
            internal const string PostThreeDSOneFormData = "<input type=\"hidden\" name=\"{0}\" value=\"{1}\" />";
            internal const string TimeoutOnPostThreeDSMethodData = "<html><body onload=\"document.forms[0].submit();\"><form action=\"{0}\" method=\"post\"><input type=\"hidden\" name=\"threeDSMethodData\" value=\"{1}\" /><input type=\"hidden\" name=\"fingerPrintTimedout\" value=\"true\" /></form></body></html>";
            internal const string VisaTokenIframeINT = "https://pmservices.cp.microsoft-int.com/staticresourceservice/resources/agentictoken/int/visa.html?action={0}";
            internal const string VisaTokenIframePROD = "https://pmservices.cp.microsoft.com/staticresourceservice/resources/agentictoken/prod/visa.html?action={0}";
            internal const string TimeoutOnPostThreeDSMethodDataViaSrc = "<html><head><script src=\"https://pmservices.cp.microsoft.com/staticresourceservice/scripts/v6/Prod/psd2_csp_onload_submit.js\" type=\"text/javascript\"></script></head><body><form action=\"{0}\" method=\"post\"><input type=\"hidden\" name=\"threeDSMethodData\" value=\"{1}\" /><input type=\"hidden\" name=\"fingerPrintTimedout\" value=\"true\" /></form></body></html>";
        }

        internal static class IFrameContentUrlTemplates
        {
            internal const string PostThreeDSMethodData = "https://pmservices.cp.microsoft.com/staticresourceservice/resources/threeDS/Prod/methodData.html?action={0}&threeDSMethodData={1}&cspStep={2}";
            internal const string PostThreeDSSessionData = "https://pmservices.cp.microsoft.com/staticresourceservice/resources/threeDS/Prod/sessionData.html?action={0}&creq={1}&threeDSSessionData={2}&cspStep={3}";
        }

        internal static class IFrameContentUrlSanitizedInputTemplates
        {
            internal const string PostThreeDSMethodData = "https://pmservices.cp.microsoft.com/staticresourceservice/resources/threeDS/Prod/methodDataSanitizedInput.html?action={0}&threeDSMethodData={1}&cspStep={2}";
            internal const string PostThreeDSSessionData = "https://pmservices.cp.microsoft.com/staticresourceservice/resources/threeDS/Prod/sessionDataSanitizedInput.html?action={0}&creq={1}&threeDSSessionData={2}&cspStep={3}";
        }

        internal static class ClientName
        {
            internal const string SecondScreen = "Payments-PIDL SDK-2nd Screen";
        }

        internal static class PurchaseOrderState
        {
            internal const string Purchased = "Purchased";
        }

        internal static class DataSourceTypes
        {
            internal const string PaymentInstrument = "paymentInstrument";
        }

        internal static class DataSourceIdentities
        {
            internal const string List = "list";
            internal const string ListModern = "listmodern";
            internal const string ListAdditionalPI = "listadditionalpi";
        }

        internal static class DataSourceOperations
        {
            internal const string Selectinstance = "selectinstance";
        }

        internal static class SuggestedAddressesStaticText
        {
            internal const string SuggestHeader = "We suggest";
            internal const string SuggestHeaderWithColon = "We suggest:";
            internal const string EnteredHeader = "You entered";
            internal const string Spacer = " ";
            internal const string UseThisAddress = "Use this address";
            internal const string Option = "Option";
            internal const string SuggestedAddress = "Suggested address";
            internal const string CurrentAddress = "Current address";
            internal const string KeepCurrentAddress = "No thanks, keep the address as I've entered it";
            internal const string KeepAddressEntered = "The address I entered";
            internal const string SuggestedLabel = "suggested_";
            internal const string WhichAddressSuggestionMessage = "Which address do you want to use?";
        }

        internal static class SuggestedAddressesAccessibilitySummaryLabels
        {
            internal const string Name = "Name";
            internal const string AddressLine1 = "Address Line 1";
            internal const string AddressLine2 = "Address Line 2";
            internal const string AddressLine3 = "Address Line 3";
            internal const string PostalCode = "Postal Code";
            internal const string City = "City";
            internal const string State = "State";
            internal const string Country = "Country";
            internal const string Optional = "(Optional)";
            internal const string Required = "required";
            internal const string FirstName = "First name";
            internal const string LastName = "Last name";
        }

        internal static class DiplayHintProperties
        {
            internal const string AccessibilityName = "accessibilityName";
        }

        internal static class LookupResponseStatus
        {
            public const string None = "None";
            public const string NotValidated = "NotValidated";
            public const string Multiple = "Multiple";
        }

        internal static class SuggestAddressDisplayIds
        {
            public const string AddressValidationMessage = "addressValidationMessage";
            public const string AddressTypeEntered = "address_type_entered";
            public const string AddressTypeSuggested = "address_type_suggested_";
        }

        internal static class MaxAddressCount
        {
            public const int SuggestAddressMaxCount = 3;
        }

        internal static class ListAddressStaticElements
        {
            public const string UseDefaultShipping = "Use default shipping address";
            public const string UseDefaultBilling = "Use default billing address";
            public const string AddNewShipping = "Add new shipping address";
            public const string AddNewBilling = "Add new billing address";
        }

        internal static class ListAddressDisplayHintIds
        {
            public const string AddNewAddress = "add_new_address";
        }

        internal static class ListPaymentInstrumentStaticElements
        {
            public const string NotWantBackupPI = "I don't want to use a back-up";
        }

        internal static class SinglePiDisplayLabels
        {
            public const string ExpiredPI = "{0} (expired)";
        }

        internal static class ActionType
        {
            public const string RestAction = "restAction";
            public const string Success = "success";
            public const string Redirect = "redirect";
            public const string Navigate = "navigate";
            public const string Submit = "submit";
            public const string DeletePaymentInstrument = "deletePaymentInstrument";
        }

        internal static class DisplayTag
        {
            public const string PiContainer = "pi-container";
            public const string AccessibilityName = "accessibilityName";
            public const string ActionTrigger = "action-trigger";
            public const string AutoHeight = "auto-height";
            public const string LabelText = "label-text";
            public const string SpaceBetween = "space-between";
            public const string ImageIcon = "image-icon";
        }

        internal static class DisplayType
        {
            public const string Page = "page";
            public const string Group = "group";
        }

        internal static class SubmitActionType
        {
            public const string LegacyValidate = "legacyValidate";
            public const string JarvisCM = "jarvisCM";
            public const string AddressEx = "addressEx";
        }

        internal static class HapiV1ModernAccountAddressDataDescriptionPropertyNames
        {
            public const string AddressLine1 = "addressLine1";
            public const string AddressLine2 = "addressLine2";
            public const string AddressLine3 = "addressLine3";
            public const string City = "city";
            public const string Region = "region";
            public const string Country = "country";
            public const string PostalCode = "postalCode";
        }

        internal static class LegacyAVSPropertyNames
        {
            internal const string AddressLine1 = "address_line1";
            internal const string AddressLine2 = "address_line2";
            internal const string AddressLine3 = "address_line3";
            internal const string City = "city";
            internal const string Region = "region";
            internal const string Country = "country";
            internal const string PostalCode = "postal_code";
        }

        internal static class HapiServiceUsageAddressPropertyNames
        {
            internal const string AddressLine1 = "line1";
            internal const string AddressLine2 = "line2";
            internal const string AddressLine3 = "line3";
            internal const string City = "city";
            internal const string Region = "state";
            internal const string Country = "countryCode";
            internal const string PostalCode = "postalCode";
        }

        internal static class CommercialZipPlusFourPropertyNames
        {
            internal const string IsUserConsented = "is_customer_consented";
            internal const string IsAvsFullValidationSucceeded = "is_avs_full_validation_succeeded";
        }

        internal static class PartnerFlightValues
        {
            internal const string ShowAVSSuggestions = "showAVSSuggestions";
            internal const string StandaloneProfile = "standaloneProfile";
            internal const string XboxOOBE = "xboxOOBE";
            internal const string EnablePaymentMethodGrouping = "enablePaymentMethodGrouping";
            internal const string EnablePMGroupingSubpageSubmitBlock = "enablePMGroupingSubpageSubmitBlock";
            internal const string IndiaExpiryGroupDelete = "IndiaExpiryGroupDelete";
            internal const string UpdateDisplayHint = "PXEnableTextForDisplayHint";
            internal const string UpdateAccessibilityNameWithPosition = "XboxUpdateAccessibilityNameWithPosition";
            internal const string EnableIndiaTokenExpiryDetails = "EnableIndiaTokenExpiryDetails";
            internal const string PXEnableDefaultPaymentMethod = "PXEnableDefaultPaymentMethod";
            internal const string PXSwapSelectPMPages = "PXSwapSelectPMPages";
            internal const string PXEnableSecureFieldAddCreditCard = "PXEnableSecureFieldAddCreditCard";
            internal const string PXEnableUpdateDiscoverCreditCardRegex = "PXEnableUpdateDiscoverCreditCardRegex";
            internal const string PXEnableUpdateVisaCreditCardRegex = "PXEnableUpdateVisaCreditCardRegex";
            internal const string PXEnableXboxAccessibilityHint = "PXEnableXboxAccessibilityHint";
            internal const string PXEnableXboxNativeStyleHints = "PXEnableXboxNativeStyleHints";
            internal const string ApplyAccentBorderWithGutterOnFocus = "ApplyAccentBorderWithGutterOnFocus";
            internal const string PXEnableSecureFieldUpdateCreditCard = "PXEnableSecureFieldUpdateCreditCard";
            internal const string PXEnableSecureFieldReplaceCreditCard = "PXEnableSecureFieldReplaceCreditCard";
            internal const string PXEnableSecureFieldSearchTransaction = "PXEnableSecureFieldSearchTransaction";
            internal const string PXEnableSecureFieldCvvChallenge = "PXEnableSecureFieldCvvChallenge";
            internal const string PXEnableSecureFieldIndia3DSChallenge = "PXEnableSecureFieldIndia3DSChallenge";
            internal const string UpdateNewPaymentMethodLinkActionContext = "UpdateNewPaymentMethodLinkActionContext";
            internal const string EnableItalyCodiceFiscale = "enableItalyCodiceFiscale";
            internal const string PXUsePartnerSettingsService = "PXUsePartnerSettingsService";
            internal const string PXEnableSetCancelButtonDisplayContentAsBack = "PXEnableSetCancelButtonDisplayContentAsBack";
            internal const string PXDisableRedeemCSVFlow = "PXDisableRedeemCSVFlow";
            internal const string SMDDisabled = "SMDDisabled";
            internal const string PXEnableUpdateCCLogo = "PXEnableUpdateCCLogo";
            internal const string PXUseInlineExpressCheckoutHtml = "PXUseInlineExpressCheckoutHtml";
            internal const string PXExpressCheckoutUseIntStaticResources = "PXExpressCheckoutUseIntStaticResources";
            internal const string PXExpressCheckoutUseProdStaticResources = "PXExpressCheckoutUseProdStaticResources";

            // Flight to enable purchase polling in confirm payment for UPI
            internal const string PXEnablePurchasePollingForUPIConfirmPayment = "PXEnablePurchasePollingForUPIConfirmPayment";

            // PX flighting to enable sepa jpmc account validation for the storefronts
            // flight cleanup task - 56373987
            internal const string EnableSepaJpmc = "EnableSepaJpmc";

            // PX flighting to enable new logo for sepa
            // flight cleanup task - 57811922
            internal const string EnableNewLogoSepa = "EnableNewLogoSepa";

            internal const string PxEnableAddCcQrCode = "PxEnableAddCcQrCode";
        }

        internal static class FeatureFlight
        {
            internal const string RemoveOptionalInLabel = "removeOptionalInLabel";
            internal const string SkipJarvisV3ForProfile = "skipJarvisV3ForProfile";
        }

        internal static class ExpiryPrefixes
        {
            internal const string Exp = "Exp";
            internal const string Expiry = "Expiry";
            internal const string ExpiryMonth = "expiryMonth";
            internal const string ExpiryYear = "expiryYear";
            internal const string ExpiryGroup = "expiryGroup";
            internal const string ExpiryGroupNoLive = "expiryGroupNoLive";
            internal const string ExpiryGroupAmexNoLive = "expiryGroupAmexNoLive";
            internal const string CreditCardExpiration = "creditCardExpiration";
            internal const string DeleteExpiry = "deleteExpiry";
            internal const string ExpiryDate = "expiryDate";
            internal const string CvvGroup = "cvvGroup";
        }

        internal static class TokenExpiryStatus
        {
            internal const string Expired = "Expired";
            internal const string Deleted = "Deleted";
            internal const string Suspended = "Suspended";
            internal const string Active = "Active";
            internal const string Pending = "Pending";
        }

        internal static class IndiaTokenizationHintIds
        {
            internal const string ExpiryGroupIndiaTokenization = "expiryGroup_IndiaTokenization";
            internal const string CvvIndiaTokenization = "cvv_IndiaTokenization";
            internal const string CreditCardWhereCVVGroupIndiaTokenization = "creditCardWhereCVVGroup_IndiaTokenization";
        }

        internal static class XboxNative3DS1AccessibilityLabels
        {
            internal const string TextBoxAccessibilityOrder = "text box {0} of {1}";
            internal const string ButtonAccessibilityOrder = "button {0} of {1}";
            internal const string CVVTextBoxAccessibilityLabel = "Enter your cvv for {0} ending in {1}";
            internal const string SubmitButtonAccessibilityLabel = "You'll be redirected to your bank's website for card verification. We'll collect your information but won't use it without your permission. Next";
            internal const string PrivacyStatement = "Microsoft respects your privacy. See our privacy statement. View Terms";
            internal const string Next = "Next";
            internal const string Back = "Back";
            internal const string GoToBank = "Can't use your phone to scan? Go to the bank website for verification.";
        }

        internal static class XboxNativeChallengeAccessibilityLabels
        {
            internal const string Cancel = "Cancel";
            internal const string Submit = "Submit";
            internal const string OK = "OK";
            internal const string ViewTermsButton = "View Terms Button";
        }

        internal static class UnlocalizedDisplayText
        {
            internal const string PaymentMethodSelectHeading = "Choose a way to pay.";
            internal const string PaymentInstrumentSelectHeading = "Pick a way to pay";
            internal const string XboxNativeCupCardsDisplayText = "UnionPay credit or debit card";
            internal const string NextButtonDisplayText = "Next";
            internal const string OkButtonDisplayText = "OK";
            internal const string CancelButtonDisplayText = "Cancel";
            internal const string BackButtonDisplayText = "Back";
            internal const string BookButtonDisplayText = "Book";
            internal const string DeleteButtonDisplayText = "Delete";
            internal const string DeleteButtonAccessibilityName = "Delete your {0}";
            internal const string PurchaseCvvChallengeText = "To protect your credit card from unauthorized charges, we require you to re-enter the security code.";
            internal const string XboxCardApplyIFrameLoadingtext = "Hi, we're processing your request\nThis might take a moment";
            internal const string AddYourBillingAddress = "Add your billing address";
            internal const string ShortUrlInstructionText = "-OR- Enter this link into a browser on another device:";
            internal const string RemoveAnotherDeviceTextFromShortUrlInstruction = "Or, enter address in a browser to sign in";
            internal const string PrefillCheckboxText = "Use shipping address for billing";
            internal const string ExpiryDatePlaceholder = "MM/YY";
            internal const string ExpiryDateText = "Expiration date";
        }

        internal static class AccessibilityLabels
        {
            internal const string Back = "Back";
            internal const string Cancel = "Cancel";
            internal const string ViewTems = "View terms";
            internal const string PickAPaymentMethod = "Pick a payment method";
        }

        internal static class AccessibilityLabelExpressions
        {
            internal const string NegativeFormattedPointsValueTotal = "(negative {formattedPointsValueTotal})";
            internal const string NegativeFormattedCSVTotal = "(negative {formattedCsvTotal})";
        }

        internal static class DisplayTagKeys
        {
            internal const string NoPidlddc = "noPidlddc";
            internal const string AccessibilityName = "accessibilityName";
            internal const string AccessibilityHint = "accessibilityHint";
            internal const string AccessibilityNameExpression = "accessibilityNameExpression";
            internal const string DisplayTagStyleHints = "displayTagStyleHints";
        }

        internal static class DisplayTags
        {
            internal const string Polite = "pidlddc-polite-live";
            internal const string Disable = "pidlddc-disable-live";
            internal const string AddressGroup = "address-group";
        }

        internal static class DisplayHintStyle
        {
            // New styleHint name created and sending this with JSON response
            internal const string DisplayHyperLink = "display-hyperlink";
        }

        internal static class XboxWebSpecificLabels
        {
            internal const string XboxwebZipPlusFourAdditionalInstructions = "We couldn't validate your address. Choose the address you want to use. Adding the final 4 digits to your zip code to ensure you are charged the proper amount for sales tax is required by law.";
        }

        internal static class CheckoutChallengeLabels
        {
            internal const string ChallengeIFrameDisplayLabel = "Third party payments checkout authentication dialog";
        }

        internal static class XboxNativeSummaryPageHeading
        {
            internal const string SummaryPageHeadingVisa = "creditCardVisaSummaryPageHeading";
            internal const string SummaryPageHeadingMC = "creditCardMCSummaryPageHeading";
            internal const string SummaryPageHeadingDiscover = "creditCardDiscoverSummaryPageHeading";
            internal const string SummaryPageHeadingAmex = "creditCardAmexSummaryPageHeading";
            internal const string SummaryPageHeadingVerve = "creditCardVerveSummaryPageHeading";
            internal const string SummaryPageHeadingJCB = "creditCardJCBSummaryPageHeading";
            internal const string SummaryPageHeadingElo = "creditCardEloSummaryPageHeading";
            internal const string SummaryPageHeadingHipercard = "creditCardHipercardSummaryPageHeading";
        }

        internal static class DataDescriptionVariableNames
        {
            internal const string AccountToken = "accountToken";
            internal const string CvvToken = "cvvToken";
            internal const string Permission = "permission";
            internal const string LastFourDigits = "lastFourDigits";
            internal const string Details = "details";
            internal const string Hmac = "hmac";
            internal const string RiskData = "riskData";
        }

        internal static class PropertyDataProtectionType
        {
            internal const string MSREncrypt = "MSREncrypt";
            internal const string TokenizeMSREncrypt = "TokenizeMSREncrypt";
            internal const string HMACSignatureMSREncrypt = "HMACSignatureMSREncrypt";
        }

        internal static class PropertyDataProtectionParamName
        {
            internal const string EncryptionScript = "encryptionScript";
            internal const string EncryptionFunction = "encryptionFunction";
            internal const string EncryptionLibrary = "encryptionLibrary";
            internal const string PublicKey = "publicKey";
            internal const string RemoveUseFallback = "removeUseFallback";
        }

        internal static class PropertyDataProtectionParamValue
        {
            internal const string EncryptionScript = "encryptAndTokenize.js";
            internal const string EncryptionFunction = "encrypt";
            internal const string EncryptionLibrary = "msrcrypto.min.js";
        }

        internal static class DisplayCustomizationDetail
        {
            internal const string SetGroupedSelectOptionTextBeforeLogo = "SetGroupedSelectOptionTextBeforeLogo";
            internal const string SetSelectPMWithLogo = "SetSelectPMWithLogo";
            internal const string MatchSelectPMMainPageStructureForSubPage = "MatchSelectPMMainPageStructureForSubPage";
            internal const string RemoveDefaultStyleHints = "RemoveDefaultStyleHints";
        }

        internal static class StandardizedDisplayText
        {
            internal const string AddressValidationUserEntered = "No thanks, keep the address as I've entered.";
            internal const string XboxCoBrandedCardApplyOnConsoleTooltipText = "Doing this on mobile is often quicker than using a controller";
            internal const string XboxCoBrandedCardQRCodeAccessibilityName = "QR code on left third of screen";
            internal const string AddUpdatePaymentMethod = "Add or update payment method";
        }

        internal static class XboxCardUpsellDisplayText
        {
            internal const string ApplyNow = "Apply now";
            internal const string MainText = "Get a 5,000 card point bonus ($50 value) after your first purchase.";
            internal const string SubText = "You're pre-selected for Xbox Mastercard";
        }

        internal static class XboxCardUpsellDisplayHintIds
        {
            internal const string XboxCardUpsellBuyNowContentWrapperGroup = "xboxCardUpsellBuyNowContentWrapperGroup";
            internal const string XboxCardUpsellInnerContentGroup = "xboxCardUpsellInnerContentGroup";
            internal const string XboxCardUpsellInnerLeftContentGroup = "xboxCardUpsellInnerLeftContentGroup";
            internal const string XboxCardUpsellInnerRightContentGroup = "xboxCardUpsellInnerRightContentGroup";
            internal const string XboxCardUpsellBackground = "xboxCardUpsellBackground";
            internal const string XboxCardUpsellCardImage = "xboxCardUpsellCardImage";
            internal const string XboxCardUpsellApplyNowText = "xboxCardUpsellApplyNowText";
            internal const string XboxCardUpsellMainText = "xboxCardUpsellMainText";
            internal const string XboxCardUpsellSubText = "xboxCardUpsellSubText";
        }

        internal static class ElementTypes
        {
            public const string ButtonList = "buttonList";
            public const string Textbox = "textbox";
            public const string Dropdown = "dropdown";
        }

        internal static class CVVHelpLinkText
        {
            internal const string WhatIsACVV = "What is a CVV?";
        }

        internal static class RedirectionPatterns
        {
            public const string FullPage = "fullPage";
            public const string Inline = "inline";
            public const string IFrame = "iFrame";
            public const string QRCode = "QRCode";
        }

        internal static class RequestType
        {
            internal const string AddPI = "addPI";
            internal const string GetPI = "getPI";
        }

        internal static class ControllerKeyCodes
        {
            internal const string GamePadB = "GamepadB";
        }

        internal static class PidlIdentityFields
        {
            internal const string Type = "type";
        }

        internal static class CreditCardLogoURLs
        {
            internal const string Visa = PidlUrlConstants.StaticResourceServiceProdAFDEndpoint + "/staticresourceservice/images/v4/logo_visa_rect.png";
            internal const string Mastercard = PidlUrlConstants.StaticResourceServiceProdAFDEndpoint + "/staticresourceservice/images/v4/logo_mc_rect.png";
            internal const string Amex = PidlUrlConstants.StaticResourceServiceProdAFDEndpoint + "/staticresourceservice/images/v4/logo_amex_rect.png";
            internal const string Discover = PidlUrlConstants.StaticResourceServiceProdAFDEndpoint + "/staticresourceservice/images/v4/logo_discover_rect.png";
        }

        internal static class FetchConfig
        {
            internal const int InitialRetryTimeout = 1000;
            internal const double RetryTimeoutMultiplier = 1.5;
            internal const int Retry1 = 1;
            internal const int Retry2 = 2;
            internal const int GetRequestTimeout = 12000;
            internal const int PostRequestTimeout = 42000;
            internal const int MaxServerErrorRetryCount = 3;

            internal const string GetTokenEndpoint = "{0}/{1}/GetToken";
            internal const string GetTokenFromEncryptedValueEndpoint = "{0}/{1}/GetTokenFromEncryptedValue";
            internal const string HmacTokenSet = "piAuthKey";
            internal const string GetTokenURL = "getTokenURL";
            internal const string GetTokenFromEncryptedValueURL = "getTokenFromEncryptedValueURL";

            internal static List<int> RetryableErrorCodes
            {
                get
                {
                    return new List<int>() { 500, 504, 429 };
                }
            }
        }

        internal static class TransformationRegex
        {
            internal const string ExpiryMonth = "^(0[1-9]|1[0-2])\\/(\\d{2})$";
            internal const string ExpiryYear = "^(0[1-9]|1[0-2])\\/(\\d{2})$";
        }

        internal static class ValidationRegex
        {
            internal const string ExpiryDate = "^(([1-9])|(0[1-9])|(1[0-2]))\\/(([2][5-9]|[3-4][0-9])|(202[5-9]|20[3-4][0-9]))$";
        }

        internal static class ExpiryDateErrorMessages
        {
            internal const string ExpiryDateInvalidCode = "expiry_date_invalid";
            internal const string ExpiryDateFormatMessage = "Incorrectly formatted expiration date";
            internal const string ExpiryDateRequiredCode = "required_field_empty";
            internal const string ExpiryDateRequiredMessage = "Expiration date is a required field";
            internal const string ExpiryDateInvalidMessage = "This is not a valid expiration date";
        }
    }
}