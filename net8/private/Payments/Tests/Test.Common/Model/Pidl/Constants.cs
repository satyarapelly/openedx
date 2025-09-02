// <copyright file="Constants.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class Constants
    {
        public static IReadOnlyList<string> Countries => countriesFromCollectionName;

        public static readonly Dictionary<string, List<string>> PaymentMethodCountries = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
        {
            { PaymentMethodFamilyType.Verve, new List<string> { "ng" } },
            { PaymentMethodFamilyType.Hipercard, new List<string> { "br" } },
            { PaymentMethodFamilyType.Elo, new List<string> { "br" } },
            { PaymentMethodFamilyType.Rupay, new List<string> { "in" } },
            { PaymentMethodFamilyType.Sepa, new List<string> { "de" } },
            { PaymentMethodFamilyType.Ach, new List<string> { "us" } },
            { PaymentMethodFamilyType.Klarna, new List<string> { "no" } }
        };

        public static class ElementTypes
        {
            public const string ButtonList = "buttonList";
            public const string Textbox = "textbox";
            public const string Dropdown = "dropdown";
        }

        public static Dictionary<string, List<string>> GetDisplayHintIdsByType(string type)
        {
            var displayHints = new Dictionary<string, List<string>>
            {
                ["orgaddress"] = new List<string>
                {
                    "orgAddressModern_email",
                    "orgAddressModern_phoneNumber"
                },
                ["hapiv1SoldToIndividual"] = new List<string>
                {
                    "hapiV1ModernAccountV20190531Address_companyName",
                    "hapiV1ModernAccountV20190531Address_phoneNumber",
                    "hapiV1ModernAccountV20190531Address_email"
                },
                ["hapiV1BillToIndividual"] = new List<string>
                {
                    "hapiV1ModernAccountV20190531Address_companyName",
                    "hapiV1ModernAccountV20190531Address_phoneNumber",
                    "hapiV1ModernAccountV20190531Address_email",
                    "hapiV1ModernAccountV20190531Address_firstName",
                    "hapiV1ModernAccountV20190531Address_lastName"
                }
            };

            var defaultDisplayHints = new List<string>
            {
                "addressFirstNameOptional",
                "addressLastNameOptional",
                "addressPhoneNumberOptional",
                "emailAddressOptional"
            };

            return displayHints.ContainsKey(type)
                ? new Dictionary<string, List<string>> { { type, displayHints[type] } }
                : new Dictionary<string, List<string>> { { type, defaultDisplayHints } };
        }

        public static List<string> PartnersToEnableKlarnaCheckout()
        {
            return new List<string>
            {
                PartnerNames.Webblends,
                PartnerNames.Cart,
                PartnerNames.OXOWebDirect,
                PartnerNames.OXODIME
            };
        }

        public static Dictionary<string, List<string>> GetPropertyDescriptionIdsByType(string type)
        {
            var propertyDescriptions = new Dictionary<string, List<string>>
            {
                ["orgaddress"] = new List<string>
                {
                    "email", "phoneNumber"
                },
                ["hapiv1SoldToIndividual"] = new List<string>
                {
                    "companyName", "phoneNumber", "email"
                },
                ["hapiV1BillToIndividual"] = new List<string>
                {
                    "companyName", "phoneNumber", "email", "firstName", "lastName"
                }
            };

            var defaultPropertyDescriptions = new List<string>
            {
                "first_name", "last_name", "phone_number", "email_address"
            };

            return propertyDescriptions.ContainsKey(type)
                ? new Dictionary<string, List<string>> { { type, propertyDescriptions[type] } }
                : new Dictionary<string, List<string>> { { type, defaultPropertyDescriptions } };
        }

        public static List<string> AllPartners
        {
            get
            {
                return typeof(PartnerNames).GetFields().Select(x => x.GetValue(null).ToString()).ToList();
            }
        }

        public static List<string> LuhnValidationEnabledPartners
        {
            get
            {
                return luhnValidationEnabledPartners;
            }
        }

        public static class PartnerNames
        {
            public const string Amc = "amc";
            public const string AmcWeb = "amcweb";
            public const string AmcXbox = "amcxbox";
            public const string AppSource = "appsource";
            public const string Azure = "azure";
            public const string AzureSignup = "azuresignup";
            public const string AzureIbiza = "azureibiza";
            public const string Bing = "bing";
            public const string BingTravel = "bingtravel";
            public const string Cart = "cart";
            public const string CommercialStores = "commercialstores";
            public const string CommercialSupport = "commercialsupport";
            public const string CommercialWebblends = "commercialwebblends";
            public const string ConsumerSupport = "consumersupport";
            public const string DefaultPartner = "default";
            public const string DefaultTemplate = "defaulttemplate";
            public const string Marketplace = "marketplace";
            public const string Mseg = "mseg";
            public const string MSTeams = "msteams";
            public const string NorthStarWeb = "northstarweb";
            public const string Office = "office";
            public const string OfficeOobe = "officeoobe";
            public const string OfficeOobeInApp = "officeoobeinapp";
            public const string OneDrive = "onedrive";
            public const string OXODIME = "oxodime";
            public const string OXOOobe = "oxooobe";
            public const string OXOWebDirect = "oxowebdirect";
            public const string Payin = "payin";
            public const string SetupOffice = "setupoffice";
            public const string SetupOfficeSdx = "setupofficesdx";
            public const string SmbOobe = "smboobe";
            public const string StoreOffice = "storeoffice";
            public const string TwoPage = "twopage";
            public const string Wallet = "wallet";
            public const string Webblends = "webblends";
            public const string WebblendsInline = "webblends_inline";
            public const string WebPay = "webpay";
            public const string WindowsNative = "windowsnative";
            public const string WindowsSettings = "windowssettings";
            public const string WindowsSubs = "windowssubs";
            public const string WindowsStore = "windowsstore";
            public const string Xbox = "xbox";
            public const string XboxNative = "xboxnative";
            public const string XboxWeb = "xboxweb";
            public const string XboxSubs = "xboxsubs";
            public const string XboxSettings = "xboxsettings";
            public const string Storify = "storify";
            public const string Saturn = "saturn";
            public const string PlayXbox = "playxbox";
        }

        public static class CreditCardPropertyDescriptionName
        {
            public const string AccountToken = "accountToken";
        }

        public static class PartnerType
        {
            public const string Commercial = "commercial";
            public const string Consumer = "consumer";
        }

        public static class VirtualPartnerNames
        {
            public const string OfficeSmb = "officesmb";
            public const string Macmanage = "macmanage";
        }

        public static class OperationTypes
        {
            public const string Add = "add";
            public const string Update = "update";
            public const string SelectInstance = "selectInstance";
        }

        public static class ProfileTypes
        {
            public const string Employee = "employee";
            public const string LegalEntity = "legalentity";
            public const string Organization = "organization";
            public const string Consumer = "consumer";
        }

        public static class DisplayHintTypes
        {
            public const string Property = "property";
            public const string Image = "image";
            public const string TextGroup = "textgroup";
            public const string Group = "group";
        }

        public static class AddressTypes
        {
            public const string HapiV1 = "hapiv1";
            public const string Billing = "billing";
            public const string OrgAddress = "orgAddress";
            public const string BillingGroup = "billingGroup";
            public const string Shipping = "shipping";
            public const string ShippingV3 = "shipping_v3";
            public const string HapiV1BillToIndividual = "hapiV1BillToIndividual";
            public const string HapiV1ShipToIndividual = "hapiV1ShipToIndividual";
            public const string HapiV1SoldToIndividual = "hapiV1SoldToIndividual";
            public const string HapiServiceUsageAddress = "hapiServiceUsageAddress";
            public const string HapiV1BillToOrganization = "hapiV1BillToOrganization";
            public const string HapiV1ShipToOrganization = "hapiV1ShipToOrganization";
            public const string HapiV1SoldToOrganization = "hapiV1SoldToOrganization";
        }

        public static class CustomHeaders
        {
            public const string IfMatch = "If-Match";
            public const string RegionIsoEnabled = "regionIsoEnabled";
        }

        public static class Values
        {
            public const string True = "true";
        }

        public static class CommercialZipPlusFourPropertyNames
        {
            public const string IsUserConsented = "is_customer_consented";
            public const string IsAvsFullValidationSucceeded = "is_avs_full_validation_succeeded";
        }

        public static class PropertyErrorConstants
        {
            public const string RequiredFieldEmpty = "required_field_empty";
            public const string RequiredFieldEmptyMessage = "The required field is null or empty";
            public const string InvalidPaymentInstrumentType = "InvalidPaymentInstrumentType";
            public const string InvalidPaymentInstrumentTypeErrorMessage = "Check that the details in all fields are correct or try a different card.";
            public const int InvalidPaymentInstrumentTypeRetryPolicyCount = 3;
        }

        public static class DescriptionIdentityFields
        {
            public const string DescriptionType = "description_type";
            public const string Family = "family";
            public const string Type = "type";
            public const string Country = "country";
            public const string Locale = "locale";
            public const string Step = "step";
            public const string Scenario = "scenario";
            public const string CountryCode = "country_code";
            public const string ResourceId = "resource_id";
            public const string IssuerId = "issuerId";
        }

        public static class QueryParamFields
        {
            public const string Language = "language";
            public const string Country = "country";
            public const string Partner = "partner";
            public const string Type = "type";
            public const string Family = "family";
            public const string Scenario = "scenario";
        }

        public static class ConfigSpecialStrings
        {
            public const string CollectionNamePrefix = "{}";
            public const string CollectionDelimiter = ";";
            public const string NameValueDelimiter = "=";
            public const string CountryId = "()CountryId";
        }

        public static class ScenarioContextsFields
        {
            public const string ResourceType = "resourceType";
            public const string TerminatingErrorHandling = "terminatingErrorHandling";
        }

        public static class ResourceTypes
        {
            public const string Primary = "primary"; // default
            public const string Secondary = "secondary";
        }

        public static class TerminatingErrorHandlingMethods
        {
            public const string Throw = "throw"; // default
            public const string Ignore = "ignore";
        }

        public static class Languages
        {
            public const string EnUS = "en-US";
            public const string FrFR = "fr-FR";
            public const string AfZA = "af-ZA";
        }

        public static class GroupDisplayHintIds
        {
            public const string AddressPostalCodeGroup = "addressPostalCodeGroup";
            public const string AddressStatePostalCodeGroup = "addressStatePostalCodeGroup";
            public const string AddressPostalCodeStateGroup = "addressPostalCodeStateGroup";
            public const string AddressProvincePostalCodeGroup = "addressProvincePostalCodeGroup";
            public const string AddressPostalCodeProvinceGroup = "addressPostalCodeProvinceGroup";
            public const string HapiV1ModernAccountV20190531AddressRegionAndPostalCodeGroup = "hapiV1ModernAccountV20190531Address_regionAndPostalCodeGroup";
            public const string HapiV1ModernAccountV20190531AddressRegionGroup = "hapiV1ModernAccountV20190531Address_regionGroup";
            public const string HapiV1ModernAccountV20190531AddressPostalCodeGroup = "hapiV1ModernAccountV20190531Address_postalCodeGroup";
            public const string HapiV1ModernAccountV20190531AddressPostalCodeAndRegionGroup = "hapiV1ModernAccountV20190531Address_postalCodeAndRegionGroup";
        }

        public static class ButtonDisplayHintIds
        {
            public const string HiddenCancelBackButton = "hiddenCancelBackButton";
            public const string RedeemGiftCardLink = "redeemGiftCardLink";
        }

        public static class DisplayHintIds
        {
            public const string PidlContainer = "pidlContainer";
            public const string PaymentInstrument = "paymentInstrument";
            public const string NewPaymentMethodLink = "newPaymentMethodLink";
            public const string HapiV1ModernAccountV20190531AddressFirstAndLastNameGroup = "hapiV1ModernAccountV20190531Address_firstAndLastNameGroup";
            public const string HapiV1ModernAccountV20190531IndividualAddressFirstAndLastNameGroup = "hapiV1ModernAccountV20190531IndividualAddress_firstAndLastNameGroup";
            public const string HapiFirstName = "hapiV1ModernAccountV20190531Address_firstName";
            public const string HapiLastName = "hapiV1ModernAccountV20190531Address_lastName";
            public const string HapiV1ModernAccountV20190531AddressMiddleName = "hapiV1ModernAccountV20190531Address_middleName";
            public const string HapiCompanyName = "hapiV1ModernAccountV20190531Address_companyName";
            public const string HapiEmail = "hapiV1ModernAccountV20190531Address_email";
            public const string AddressHapiV1Page = "addressHapiV1Page";
            public const string ValidateButtonHidden = "validateButtonHidden";
            public const string NameDisplayHintId = "cardholderName";
            public const string AmexNumberDisplayHintId = "cardNumberAmex";
            public const string NumberDisplayHintId = "cardNumber";
            public const string PaymentInstrumentItemWalletCardGroup = "paymentInstrumentItemWalletCardGroup";
            public const string PaymentInstrumentItemWalletDetailsGroup = "paymentInstrumentItemWalletDetailsGroup";
            public const string PaymentInstrumentItemWalletColumnGroup = "paymentInstrumentItemWalletColumnGroup";
            public const string PaymentInstrumentListPi = "paymentInstrumentListPi";
            public const string ContinueRedirectButton = "continueRedirectButton";
            public const string ContinueSubmitButton = "continueSubmitButton";
            public const string Cvv3DSSubmitButton = "cvv3DSSubmitButton";
            public const string OKButton = "okButton";
            public const string SubmitButton = "submitButton";
            public const string SaveNextButton = "saveNextButton";
            public const string VerifyPicvButton = "verifyPicvButton";
            public const string AddressLine1 = "addressLine1";
            public const string AddressLine2 = "addressLine2";
            public const string AddressLine3 = "addressLine3";
            public const string AddressCity = "addressCity";
            public const string AddressStatePostalCodeGroup = "addressStatePostalCodeGroup";
            public const string AddressPostalCodeStateGroup = "addressPostalCodeStateGroup";
            public const string AddressProvincePostalCodeGroup = "addressProvincePostalCodeGroup";
            public const string AddressPostalCodeProvinceGroup = "addressPostalCodeProvinceGroup";
            public const string AddressState = "addressState";
            public const string AddressProvince = "addressProvince";
            public const string AddressPostalCode = "addressPostalCode";
            public const string AddressCountry = "addressCountry";
            public const string AddressCounty = "addressCounty";
            public const string AddressGroup = "addressGroup";
            public const string SepaYesButton = "sepaYesButton";
            public const string SepaTryAgainButton = "sepaTryAgainButton";
            public const string CVV = "cvv";
            public const string PayPalText1 = "paypalText1";
            public const string StarRequiredTextGroup = "starRequiredTextGroup";
            public const string MandatoryFieldsMessage = "mandatory_fields_message";
            public const string CancelCvv3DSSubmitGroup = "cancelCvv3DSSubmitGroup";
            public const string CancelCvv3DSSubmitWithAdjustedGapGroup = "cancelCvv3DSSubmitWithAdjustedGapGroup";
            public const string PrefillBillingAddressCheckbox = "prefillBillingAddressCheckbox";

            public const string AddressFirstName = "addressFirstName";
            public const string AddressMiddleName = "addressMiddleName";
            public const string AddressLastName = "addressLastName";
            public const string AddressFirstNameOptional = "addressFirstNameOptional";
            public const string AddressLastNameOptional = "addressLastNameOptional";
            public const string AddressEmailOptional = "emailAddressOptional";
            public const string HapiAddressLine1 = "hapiV1ModernAccountV20190531Address_addressLine1";
            public const string HapiAddressLine2 = "hapiV1ModernAccountV20190531Address_addressLine2";
            public const string HapiAddressLine3 = "hapiV1ModernAccountV20190531Address_addressLine3";
            public const string HapiCountry = "hapiV1ModernAccountV20190531Address_country";
            public const string AddressPhoneNumberOptional = "addressPhoneNumberOptional";
            public const string KlarnaAddressLine1 = "klarnaAddressLine1";
            public const string KlarnaAddressLine2 = "klarnaAddressLine2";
            public const string KlarnaAddressPostalCode = "klarnaAddressPostalCode";
            public const string HapiV1ModernAccountV20190531AddressCity = "hapiV1ModernAccountV20190531Address_city";
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
            public const string OrgAddressModernEmail = "orgAddressModern_email";
            public const string OrgAddressModernPhoneNumber = "orgAddressModern_phoneNumber";
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
            public const string KlarnaCheckoutAddPageHeading = "klarnaCheckoutAddPageHeading";
            public const string AddKlarnaPageHeading = "addKlarnaPageHeading";
            public const string PaymentInstrumentSelectHeading = "paymentInstrumentSelectHeading";
        }

        public static class ActionType
        {
            public const string RestAction = "restAction";
            public const string Success = "success";
            public const string Submit = "submit";
        }

        public static class PaymentMethodFamilyType
        {
            public const string Verve = "verve";
            public const string Elo = "elo";
            public const string Hipercard = "hipercard";
            public const string Rupay = "rupay";
            public const string Amex = "amex";
            public const string Visa = "visa";
            public const string Mc = "mc";
            public const string AmexVisaMcDiscoverJcb = "amex%2Cvisa%2Cmc%2Cdiscover%2Cjcb";
            public const string VisaMc = "visa%2Cmc";
            public const string Sepa = "sepa";
            public const string IdealBillingAgreement = "ideal_billing_agreement";
            public const string Ach = "ach";
            public const string PayPal = "paypal";
            public const string Klarna = "klarna";
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

        public static class DisplayContent
        {
            public const string NextDisplayContentText = "Next";
            public const string OkDisplayContentText = "OK";
            public const string SubmitDisplayContentText = "Submit";
            public const string DisplayHelpNameCVV = "What is a CVV?";
            public const string PayPalContentEN = "In the next step, you'll be redirected to PayPal's website to verify your account. We'll collect your PayPal account info but won't use it without your permission.";
            public const string PayPalContentFR = "Dans la nouvelle étape, nous allez être redirigé(e) vers le site web de PayPal pour vérifier votre compte. Nous allons collecter vos informations de compte PayPal, mais nous ne les utiliserons pas sans votre autorisation.";
            public const string PayPalContentAF = "Jy sal in die volgende stap na PayPal se webwerf herlei word om jou rekening te verifieer. Ons sal jou PayPal-rekeninginligting versamel, maar sal dit nie sonder jou toestemming gebruik nie.";
        }

        public static List<string> AddressFieldsWithDefaultValueNotNeededForUpdateAndReplace
        {
            get
            {
                return new List<string> { "address_line1", "address_line2", "address_line3", "city", "region", "postal_code", "country" };
            }
        }

        public static class SubmitUrls
        {
            public const string PifdBaseUrl = "https://{pifd-endpoint}/users/{userId}";
            public const string PifdAddressPostUrlTemplate = PifdBaseUrl + "/addressesEx";
        }

        public static class PartnerFlightValues
        {
            public const string PXEnableSecondaryValidationMode = "PXEnableSecondaryValidationMode";
            public const string EnableItalyCodiceFiscale = "enableItalyCodiceFiscale";
            public const string PXEnableAddAllFieldsRequiredText = "PXEnableAddAllFieldsRequiredText";
            public const string ApplyAccentBorderWithGutterOnFocus = "ApplyAccentBorderWithGutterOnFocus";
            public const string PXUseFontIcons = "PXUseFontIcons";
            public const string PXEnableEGTaxIdsRequired = "PXEnableEGTaxIdsRequired";
            public const string PXEnableAddAsteriskToAllMandatoryFields = "PXEnableAddAsteriskToAllMandatoryFields";
        }

        public static class DisplayTagKeys
        {
            public const string DisplayTagStyleHints = "displayTagStyleHints";
        }

        public static class StyleHints
        {
            public static readonly List<string> CancelCvv3dsSubmitButtonGroupAtBottomOfPageStyleHints = new List<string> { "padding-vertical-medium", "gap-medium", "width-fill", "anchor-bottom" };

            public static readonly List<string> CancelCvv3dsSubmitButtonGroupAtBottomOfFormStyleHints = new List<string> { "padding-top-small", "gap-small" };
            public const string ImageHeightSmall = "image-height-small";
            public const string AlignHorizontalCenter = "alignment-horizontal-center";
            public const string GapSmall = "gap-small";
            public const string WidthFill = "width-fill";
            public const string AlignverticalCenter = "align-vertical-center";
            public const string DirectionHorizontal = "direction-horizontal";
            public const string PaddingVerticalSmall = "padding-vertical-small";
        }

        public static class DisplayTagValues
        {
            public const string SelectionBorderGutterAccent = "selection-border-gutter-accent";
        }

        public static readonly List<string> AddressFields = new List<string>
        {
            // For Billing, Internal Address
            DisplayHintIds.AddressLine1,
            DisplayHintIds.AddressLine2,
            DisplayHintIds.AddressLine3,
            DisplayHintIds.AddressCity,
            GroupDisplayHintIds.AddressStatePostalCodeGroup,
            GroupDisplayHintIds.AddressPostalCodeStateGroup,
            GroupDisplayHintIds.AddressProvincePostalCodeGroup,
            GroupDisplayHintIds.AddressPostalCodeProvinceGroup,
            GroupDisplayHintIds.AddressPostalCodeGroup,
            DisplayHintIds.AddressState,
            DisplayHintIds.AddressProvince,
            DisplayHintIds.AddressPostalCode,
            DisplayHintIds.AddressCountry,
            DisplayHintIds.AddressCounty,

            // for Klarna Address
            DisplayHintIds.KlarnaAddressLine1,
            DisplayHintIds.KlarnaAddressLine2,
            DisplayHintIds.KlarnaAddressPostalCode,

            // For Address hapiV1ModernAccountV20190531Address
            DisplayHintIds.HapiAddressLine1,
            DisplayHintIds.HapiAddressLine2,
            DisplayHintIds.HapiAddressLine3,
            DisplayHintIds.HapiV1ModernAccountV20190531AddressCity,
            GroupDisplayHintIds.HapiV1ModernAccountV20190531AddressRegionAndPostalCodeGroup,
            GroupDisplayHintIds.HapiV1ModernAccountV20190531AddressRegionGroup,
            GroupDisplayHintIds.HapiV1ModernAccountV20190531AddressPostalCodeGroup,
            DisplayHintIds.HapiCountry,
            GroupDisplayHintIds.HapiV1ModernAccountV20190531AddressPostalCodeAndRegionGroup,

            // For address legalentity
            DisplayHintIds.UpdateLegalEntityAddressLine1,
            DisplayHintIds.UpdateLegalEntityAddressLine2,
            DisplayHintIds.UpdateLegalEntityAddressLine3,
            DisplayHintIds.UpdateProfileAddressCity,
            DisplayHintIds.UpdateProfileAddressState,
            DisplayHintIds.UpdateLegalEntityAddressPostalCode,
            DisplayHintIds.UpdateProfileAddressCountry,
            DisplayHintIds.UpdateProfileAddressCounty,
            DisplayHintIds.UpdateProfileAddressProvince,

            // For shipping_patch
            DisplayHintIds.AddressFirstName,
            DisplayHintIds.AddressMiddleName,
            DisplayHintIds.AddressLastName,
            DisplayHintIds.AddressPhoneNumber,
            DisplayHintIds.AddressStateGroup,
            DisplayHintIds.UpdateProfileAddressLine1,
            DisplayHintIds.UpdateProfileAddressLine2,
            DisplayHintIds.UpdateProfileAddressLine3,
            DisplayHintIds.UpdateProfileAddressPostalCode,

            // For orgAddressModern
            DisplayHintIds.OrgAddressModernAddressLine1,
            DisplayHintIds.OrgAddressModernAddressLine2,
            DisplayHintIds.OrgAddressModernAddressLine3,
            DisplayHintIds.OrgAddressModernCity,
            DisplayHintIds.OrgAddressModernRegion,
            DisplayHintIds.OrgAddressModernPostalCode,
            DisplayHintIds.OrgAddressModernCountry,
            DisplayHintIds.OrgAddressModernEmail,
            DisplayHintIds.OrgAddressModernPhoneNumber,

            // For Address SoldTo
            DisplayHintIds.AddressCorrespondenceName,
            DisplayHintIds.AddressMobile,
            DisplayHintIds.AddressFax,
            DisplayHintIds.AddressTelex,
            DisplayHintIds.AddressEmailAddress,
            DisplayHintIds.AddressWebSiteUrl,
            DisplayHintIds.AddressStreetSupplement,
            DisplayHintIds.AddressIsWithinCityLimits,
            DisplayHintIds.AddressFormOfAddress,
            DisplayHintIds.AddressAddressNotes,
            DisplayHintIds.AddressTimeZone,
            DisplayHintIds.AddressLatitude,
            DisplayHintIds.AddressLongitude,

            // For Address HapiSua
            DisplayHintIds.HapiSUALine1,
            DisplayHintIds.HapiSUALine2,
            DisplayHintIds.HapiSUALine3,
            DisplayHintIds.HapiSUACity,
            DisplayHintIds.HapiSUAState,
            DisplayHintIds.HapiSUAPostalCode,
            DisplayHintIds.HapiSUACountryCode,
            DisplayHintIds.HapiSUACounty,
            DisplayHintIds.HapiSUAPhoneNumber,
            DisplayHintIds.HapiSUAProvince,

            // For Shipping_v3, shippingAddressHardware
            DisplayHintIds.ShippingAddressState,
            DisplayHintIds.AddressCountyGB,
            DisplayHintIds.AddressFirstNameOptional,
            DisplayHintIds.AddressLastNameOptional,
            DisplayHintIds.AddressEmailOptional,
            DisplayHintIds.AddressPhoneNumberOptional,
            DisplayHintIds.AddressPhoneNumberWithExplanation,
            DisplayHintIds.ShippingAddressLine1,
            DisplayHintIds.ShippingAddressLine2,
            DisplayHintIds.ProfileAddressFirstName,
            DisplayHintIds.ProfileAddressLastName,
            DisplayHintIds.ProfileAddressLine1,
            DisplayHintIds.ProfileAddressLine2,
            DisplayHintIds.ProfileAddressLine3,
            DisplayHintIds.ProfileAddressCity,
            DisplayHintIds.ProfileAddressState,
            DisplayHintIds.ProfileAddressPostalCode,
            DisplayHintIds.ProfileAddressPhoneNumber,
            DisplayHintIds.ProfileAddressCounty,
            DisplayHintIds.ProfileAddressProvince
        };

        public static class PSSMockResponses
        {
            public const string PXPartnerSettingsWindowsStore = "{\"select\":{\"template\":\"selectpmbuttonlist\",\"features\":{\"paymentMethodGrouping\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"setGroupedSelectOptionTextBeforeLogo\":true,\"removeDefaultStyleHints\":true}]},\"selectPMButtonListStyleForWindows\":{\"applicableMarkets\":[]},\"customizeDisplayTag\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addAccessibilityNameExpressionToNegativeValue\":true}]}}},\"add\":{\"template\":\"twopage\",\"resources\":{\"address\":{\"px_v3_billing\": {\"template\":\"defaulttemplate\"},\"billing\": {\"template\":\"defaulttemplate\"}},\"paymentMethod\":{\"ewallet.stored_value\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\"}}},\"features\":{\"addCCTwoPageForWindows\":{\"applicableMarkets\":[]},\"addPayPalForWindows\":{\"applicableMarkets\":[]},\"addVenmoForWindows\":{\"applicableMarkets\":[]},\"addBillingAddressForWindows\":{\"applicableMarkets\":[]},\"redeemGiftCard\":{\"applicableMarkets\":[]},\"showRedirectURLInIframe\":{\"applicableMarkets\":[]},\"useV3AddressPIDL\":{\"applicableMarkets\":[]},\"shortURLPaypal\":{\"applicableMarkets\":[]},\"shortURLTextPaypal\":{\"applicableMarkets\":[]},\"shortURLVenmo\":{\"applicableMarkets\":[]},\"shortURLTextVenmo\":{\"applicableMarkets\":[]},\"removeElement\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"removeEwalletYesButtons\":true,\"removeEwalletBackButtons\":true,\"removeSpaceInPrivacyTextGroup\":true}]},\"enableShortURL\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"shortUrlPayPalText\":true,\"shortUrlVenmoText\":true,\"removeAnotherDeviceTextFromShortUrlInstruction\":true,\"displayShortUrlAsHyperlink\":true}]}},\"redirectionPattern\":\"QRCode\"}}";
            public const string PXPartnerSettingsPlayXbox = "{\"default\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":null,\"resources\":null,\"features\":null},\"add\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"QRCode\",\"features\":{\"useV3AddressPIDL\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null},\"useAddressesExSubmit\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null},\"addressValidation\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"verifyAddressPidlModification\":true}]},\"skipJarvisV3ForProfile\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null},\"PXEnableVenmo\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]},\"PxEnableSelectPMAddPIVenmo\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]},\"PXSkipPifdAddressPostForNonAddressesType\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]},\"PXPassUserAgentToPIMSForAddUpdatePI\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]},\"PXRemoveJarvisHeadersFromSubmitUrl\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]},\"showRedirectURLInIframe\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null}}},\"update\":{\"template\":\"defaulttemplate\",\"features\":{\"useV3AddressPIDL\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null},\"useAddressesExSubmit\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null},\"addressValidation\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"verifyAddressPidlModification\":true}]},\"PXSkipPifdAddressPostForNonAddressesType\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]},\"PXRemoveJarvisHeadersFromSubmitUrl\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]}}},\"select\":{\"template\":\"selectpmbuttonlist\",\"features\":{\"paymentMethodGrouping\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addCancelButtonToHomePage\":true}]},\"PXSwapSelectPMPages\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]},\"PXEnableVenmo\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]},\"PxEnableSelectPMAddPIVenmo\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]}}},\"selectinstance\":{\"template\":\"listpibuttonlist\",\"resources\":{\"address\":{\"billing\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":null},\"px_v3_billing\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":null}}},\"features\":{\"addRedeemGiftCardButton\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null},\"addElement\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"AddPickAWayToPayHeading\":true}]},\"unhideElements\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"elementsToBeUnhidden\":[\"hiddenCancelBackButton\"]}]},\"customizeDisplayTag\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"displayTagsToBeAdded\":{\"newPaymentMethodLink\":{\"addIcon\":\"addIcon\"},\"redeemGiftCardLink\":{\"giftCardIcon\":\"giftCardIcon\",\"accessibilityName\":\"Redeem a gift card\"},\"optionUpdate_\":{\"target\":\"_self\"}}}]},\"addStyleHintsToDisplayHints\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"styleHintsToBeAdded\":{\"newPaymentMethodLink\":[\"left\"],\"redeemGiftCardLink\":[\"left\"],\"hiddenCancelBackButton\":[\"large\"]}}]},\"selectInstanceForAddress\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null},\"useV3AddressPIDL\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null},\"PXEnableVenmo\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]},\"PxEnableSelectPMAddPIVenmo\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]},\"removeZeroBalanceCsv\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null}}}}";
            public const string OfficeSmbQrCodeRedirection = "{\"add\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"QRCode\",\"features\":{\"enableShortURL\":{\"applicableMarkets\":null,\"displayCustomizationDetail\":[{\"removeAnotherDeviceTextFromShortUrlInstruction\":true}]}}},\"update\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"QRCode\"},\"apply\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"QRCode\"},\"validateinstance\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"QRCode\"}}";
            public const string OfficeSmbQRCodeRedirectionWithDisplayVertialLayoutFeature = "{\"add\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"QRCode\",\"resources\":{\"paymentMethod\":{\"ewallet.paypal\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"QRCode\"}},\"challenge\":{\"paypalqrcode\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\"}}},\"features\":{\"enableShortURL\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"displayShortUrlAsVertical\":true,\"removeAnotherDeviceTextFromShortUrlInstruction\":true,\"displayShortUrlAsHyperlink\":false}]}}}}";
            public const string ExpectedPSSResponsePaymentClient = "{\"add\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\",\"resources\":null,\"features\":{\"enableSavePaymentDetails\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]},\"paymentClientHandlePaymentCollection\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null},\"enableExpiryCVVGrouping\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]},\"enablePlaceholder\":{\"applicableMarkets\":[]},\"singleMarketDirective\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null},\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"moveCardNumberBeforeCardHolderName\":false,\"moveOrganizationNameBeforeEmailAddress\":false,\"moveLastNameBeforeFirstName\":null,\"setSaveButtonDisplayContentAsNext\":false,\"removeStarRequiredTextGroup\":null,\"updateCvvChallengeTextForGCO\":false,\"enableIsSelectPMskippedValue\":null,\"enableCountryAddorUpdateCC\":false,\"hideCountryDropdown\":null,\"hideFirstAndLastNameForCompletePrerequisites\":false,\"hideAddCreditDebitCardHeading\":false,\"updatePaymentMethodHeadingTypeToText\":null,\"removeAddCreditDebitCardHeading\":null,\"setGroupedSelectOptionTextBeforeLogo\":null,\"setSelectPMWithLogo\":null,\"useTextForCVVHelpLink\":null,\"cvvDisplayHelpPosition\":null,\"matchSelectPMMainPageStructureForSubPage\":null,\"useFixedSVGForMC\":null,\"enableIndia3dsForNonZeroPaymentTransaction\":null,\"pxEnableIndia3DS1Challenge\":null,\"india3dsEnableForBilldesk\":null,\"usePSSForPXFeatureFlighting\":null,\"enableSecureFieldAddCC\":false,\"setSaveButtonDisplayContentAsBook\":false,\"removeCancelButton\":false,\"removeSelectPiEditButton\":null,\"removeSelectPiNewPaymentMethodLink\":null,\"removeSpaceInPrivacyTextGroup\":null,\"setBackButtonDisplayContentAsCancel\":false,\"setPrivacyStatementHyperLinkDisplayToButton\":null,\"hidePaymentSummaryText\":false,\"hidepaymentOptionSaveText\":false,\"addressType\":\"billing\",\"dataSource\":\"jarvis\",\"submitActionType\":null,\"fieldsToBeHidden\":null,\"fieldsToBeEnabled\":null,\"fieldsToMakeRequired\":null,\"fieldsToBeRemoved\":null,\"removeOptionalTextFromFields\":null,\"psd2IgnorePIAuthorization\":null,\"addressSuggestionMessage\":false,\"updateAccessibilityNameWithPosition\":null,\"disableSelectPiRadioOption\":null,\"updateSelectPiButtonText\":null,\"removeGroupForExpiryMonthAndYear\":null,\"useAddressDataSourceForUpdate\":null,\"disableCountryDropdown\":null,\"ungroupAddressFirstNameLastName\":null,\"endPoint\":null,\"operation\":null,\"profileType\":null,\"dataFieldsToRemoveFromPayload\":null,\"dataFieldsToRemoveFullPath\":null,\"convertProfileTypeTo\":null,\"fieldsToBeDisabled\":null,\"hidePaymentMethodHeading\":null,\"hideChangeSettingText\":null,\"removeEwalletYesButtons\":null,\"removeEwalletBackButtons\":null,\"removeDefaultStyleHints\":null,\"useSuggestAddressesTradeAVSV1Scenario\":null,\"addCCAddressValidationPidlModification\":null,\"verifyAddressPidlModification\":null,\"displayTagsToBeRemoved\":null,\"replaceContextInstanceWithPaymentInstrumentId\":null,\"enablePlaceholder\":null,\"hideAcceptCardMessage\":null,\"addAccessibilityNameExpressionToNegativeValue\":null,\"removeAnotherDeviceTextFromShortUrlInstruction\":null,\"displayShortUrlAsHyperlink\":null,\"hideAddressCheckBoxIfAddressIsNotPrefilledFromServer\":null,\"fieldsToSetIsSubmitGroupFalse\":null,\"removeMicrosoftPrivacyTextGroup\":null,\"hideCardLogos\":null,\"hideAddress\":null,\"addCancelButton\":false,\"displayShortUrlAsVertical\":null,\"setSubmitURLToEmptyForTaxId\":null,\"addressSuggestion\":null,\"updateXboxElementsAccessibilityHints\":null,\"setCancelButtonDisplayContentAsBack\":false,\"setButtonDisplayContent\":null,\"updateConsumerProfileSubmitLinkToJarvisPatch\":null,\"removeDataSourceResources\":null,\"useIFrameForPiLogOn\":null,\"removeAddressFormHeading\":null,\"displayAccentBorderOnButtonFocus\":null,\"addStyleHints\":null,\"removeStyleHints\":null,\"removeAcceptCardMessage\":null,\"addAllFieldsRequiredText\":null}]},\"chinaAllowVisaMasterCard\":{\"applicableMarkets\":[\"cn\"],\"displayCustomizationDetail\":null}}},\"select\":{\"template\":\"selectpmradiobuttonlist\",\"redirectionPattern\":null,\"resources\":null,\"features\":{\"chinaAllowVisaMasterCard\":{\"applicableMarkets\":[\"cn\"],\"displayCustomizationDetail\":null}}},\"selectinstance\":{\"template\":\"listpidropdown\",\"redirectionPattern\":null,\"resources\":null,\"features\":{\"chinaAllowVisaMasterCard\":{\"applicableMarkets\":[\"cn\"],\"displayCustomizationDetail\":null}}},\"handlepaymentchallenge\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\",\"resources\":null,\"features\":{\"threeDSOne\":{\"applicableMarkets\":[\"in\"],\"displayCustomizationDetail\":[{\"moveCardNumberBeforeCardHolderName\":false,\"moveOrganizationNameBeforeEmailAddress\":false,\"moveLastNameBeforeFirstName\":null,\"setSaveButtonDisplayContentAsNext\":false,\"removeStarRequiredTextGroup\":null,\"updateCvvChallengeTextForGCO\":false,\"enableIsSelectPMskippedValue\":null,\"enableCountryAddorUpdateCC\":false,\"hideCountryDropdown\":null,\"hideFirstAndLastNameForCompletePrerequisites\":false,\"hideAddCreditDebitCardHeading\":false,\"updatePaymentMethodHeadingTypeToText\":null,\"removeAddCreditDebitCardHeading\":null,\"setGroupedSelectOptionTextBeforeLogo\":null,\"setSelectPMWithLogo\":null,\"useTextForCVVHelpLink\":null,\"cvvDisplayHelpPosition\":null,\"matchSelectPMMainPageStructureForSubPage\":null,\"useFixedSVGForMC\":null,\"enableIndia3dsForNonZeroPaymentTransaction\":false,\"pxEnableIndia3DS1Challenge\":true,\"india3dsEnableForBilldesk\":true,\"usePSSForPXFeatureFlighting\":null,\"enableSecureFieldAddCC\":false,\"setSaveButtonDisplayContentAsBook\":false,\"removeCancelButton\":false,\"removeSelectPiEditButton\":null,\"removeSelectPiNewPaymentMethodLink\":null,\"removeSpaceInPrivacyTextGroup\":null,\"setBackButtonDisplayContentAsCancel\":false,\"setPrivacyStatementHyperLinkDisplayToButton\":null,\"hidePaymentSummaryText\":false,\"hidepaymentOptionSaveText\":false,\"addressType\":null,\"dataSource\":null,\"submitActionType\":null,\"fieldsToBeHidden\":null,\"fieldsToBeEnabled\":null,\"fieldsToMakeRequired\":null,\"fieldsToBeRemoved\":null,\"removeOptionalTextFromFields\":null,\"psd2IgnorePIAuthorization\":null,\"addressSuggestionMessage\":false,\"updateAccessibilityNameWithPosition\":null,\"disableSelectPiRadioOption\":null,\"updateSelectPiButtonText\":null,\"removeGroupForExpiryMonthAndYear\":null,\"useAddressDataSourceForUpdate\":null,\"disableCountryDropdown\":null,\"ungroupAddressFirstNameLastName\":null,\"endPoint\":null,\"operation\":null,\"profileType\":null,\"dataFieldsToRemoveFromPayload\":null,\"dataFieldsToRemoveFullPath\":null,\"convertProfileTypeTo\":null,\"fieldsToBeDisabled\":null,\"hidePaymentMethodHeading\":null,\"hideChangeSettingText\":null,\"removeEwalletYesButtons\":null,\"removeEwalletBackButtons\":null,\"removeDefaultStyleHints\":null,\"useSuggestAddressesTradeAVSV1Scenario\":null,\"addCCAddressValidationPidlModification\":null,\"verifyAddressPidlModification\":null,\"displayTagsToBeRemoved\":null,\"replaceContextInstanceWithPaymentInstrumentId\":null,\"enablePlaceholder\":null,\"hideAcceptCardMessage\":null,\"addAccessibilityNameExpressionToNegativeValue\":null,\"removeAnotherDeviceTextFromShortUrlInstruction\":null,\"displayShortUrlAsHyperlink\":null,\"hideAddressCheckBoxIfAddressIsNotPrefilledFromServer\":null,\"fieldsToSetIsSubmitGroupFalse\":null,\"removeMicrosoftPrivacyTextGroup\":null,\"hideCardLogos\":null,\"hideAddress\":null,\"addCancelButton\":false,\"displayShortUrlAsVertical\":null,\"setSubmitURLToEmptyForTaxId\":null,\"addressSuggestion\":null,\"updateXboxElementsAccessibilityHints\":null,\"setCancelButtonDisplayContentAsBack\":false,\"setButtonDisplayContent\":null,\"updateConsumerProfileSubmitLinkToJarvisPatch\":null,\"removeDataSourceResources\":null,\"useIFrameForPiLogOn\":null,\"removeAddressFormHeading\":null,\"displayAccentBorderOnButtonFocus\":null,\"addStyleHints\":null,\"removeStyleHints\":null,\"removeAcceptCardMessage\":null,\"addAllFieldsRequiredText\":null}]},\"PSD2\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"moveCardNumberBeforeCardHolderName\":false,\"moveOrganizationNameBeforeEmailAddress\":false,\"moveLastNameBeforeFirstName\":null,\"setSaveButtonDisplayContentAsNext\":false,\"removeStarRequiredTextGroup\":null,\"updateCvvChallengeTextForGCO\":false,\"enableIsSelectPMskippedValue\":null,\"enableCountryAddorUpdateCC\":false,\"hideCountryDropdown\":null,\"hideFirstAndLastNameForCompletePrerequisites\":false,\"hideAddCreditDebitCardHeading\":false,\"updatePaymentMethodHeadingTypeToText\":null,\"removeAddCreditDebitCardHeading\":null,\"setGroupedSelectOptionTextBeforeLogo\":null,\"setSelectPMWithLogo\":null,\"useTextForCVVHelpLink\":null,\"cvvDisplayHelpPosition\":null,\"matchSelectPMMainPageStructureForSubPage\":null,\"useFixedSVGForMC\":null,\"enableIndia3dsForNonZeroPaymentTransaction\":null,\"pxEnableIndia3DS1Challenge\":null,\"india3dsEnableForBilldesk\":null,\"usePSSForPXFeatureFlighting\":null,\"enableSecureFieldAddCC\":false,\"setSaveButtonDisplayContentAsBook\":false,\"removeCancelButton\":false,\"removeSelectPiEditButton\":null,\"removeSelectPiNewPaymentMethodLink\":null,\"removeSpaceInPrivacyTextGroup\":null,\"setBackButtonDisplayContentAsCancel\":false,\"setPrivacyStatementHyperLinkDisplayToButton\":null,\"hidePaymentSummaryText\":false,\"hidepaymentOptionSaveText\":false,\"addressType\":null,\"dataSource\":null,\"submitActionType\":null,\"fieldsToBeHidden\":null,\"fieldsToBeEnabled\":null,\"fieldsToMakeRequired\":null,\"fieldsToBeRemoved\":null,\"removeOptionalTextFromFields\":null,\"psd2IgnorePIAuthorization\":true,\"addressSuggestionMessage\":false,\"updateAccessibilityNameWithPosition\":null,\"disableSelectPiRadioOption\":null,\"updateSelectPiButtonText\":null,\"removeGroupForExpiryMonthAndYear\":null,\"useAddressDataSourceForUpdate\":null,\"disableCountryDropdown\":null,\"ungroupAddressFirstNameLastName\":null,\"endPoint\":null,\"operation\":null,\"profileType\":null,\"dataFieldsToRemoveFromPayload\":null,\"dataFieldsToRemoveFullPath\":null,\"convertProfileTypeTo\":null,\"fieldsToBeDisabled\":null,\"hidePaymentMethodHeading\":null,\"hideChangeSettingText\":null,\"removeEwalletYesButtons\":null,\"removeEwalletBackButtons\":null,\"removeDefaultStyleHints\":null,\"useSuggestAddressesTradeAVSV1Scenario\":null,\"addCCAddressValidationPidlModification\":null,\"verifyAddressPidlModification\":null,\"displayTagsToBeRemoved\":null,\"replaceContextInstanceWithPaymentInstrumentId\":null,\"enablePlaceholder\":null,\"hideAcceptCardMessage\":null,\"addAccessibilityNameExpressionToNegativeValue\":null,\"removeAnotherDeviceTextFromShortUrlInstruction\":null,\"displayShortUrlAsHyperlink\":null,\"hideAddressCheckBoxIfAddressIsNotPrefilledFromServer\":null,\"fieldsToSetIsSubmitGroupFalse\":null,\"removeMicrosoftPrivacyTextGroup\":null,\"hideCardLogos\":null,\"hideAddress\":null,\"addCancelButton\":false,\"displayShortUrlAsVertical\":null,\"setSubmitURLToEmptyForTaxId\":null,\"addressSuggestion\":null,\"updateXboxElementsAccessibilityHints\":null,\"setCancelButtonDisplayContentAsBack\":false,\"setButtonDisplayContent\":null,\"updateConsumerProfileSubmitLinkToJarvisPatch\":null,\"removeDataSourceResources\":null,\"useIFrameForPiLogOn\":null,\"removeAddressFormHeading\":null,\"displayAccentBorderOnButtonFocus\":null,\"addStyleHints\":null,\"removeStyleHints\":null,\"removeAcceptCardMessage\":null,\"addAllFieldsRequiredText\":null}]}}},\"default\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":null,\"resources\":null,\"features\":{\"paymentClientHandlePaymentCollection\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null}}}}";
        }

        public static Dictionary<string, List<string>> TestValidPhoneNumbersByCountry
        {
            get
            {
                return new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                {
                    { "us", new List<string>() { "+1 123-456-7890", "+1(123)456-7890", "1-800-123-4567", "5555555555", "1234567890" } },
                    { "cn", new List<string>() { "13812345678", "+8613812345678", "12345678901", "0101234567" } },
                    { "ca", new List<string>() { "+1 (647) 123-4567", "416-123-4567", "7801234567", "+1 819-123-4567" } },
                    { "gb", new List<string>() { "+44 20 1234 5678", "020 1234 5678", "+44 (0) 1234 567890" } },
                    { "de", new List<string>() { "+49 30 12345678", "030 12345678", "+49 40 123456789" } },
                    { "dk", new List<string>() { "+45 12 34 56 78", "12 34 56 78", "+45 12 34 56789" } },
                };
            }
        }

        public static Dictionary<string, List<string>> TestInvalidPhoneNumbersByCountry
        {
            get
            {
                return new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                {
                    { "us", new List<string>() { "ssssssss", "2837373sj", "123456789", "123-45-6789", "555-555-55555", "123456" } },
                    { "cn", new List<string>() { "123456", "+86123456789123456789", "123-456-7890" } },
                    { "common", new List<string>() { "sssss", "+123\\45678901234567890", "12345678901234567890", "123-43348?56-789", "+49 123!45678901", "+8" } },
                };
            }
        }

        // It contains the list of countries for which there is one country code from the collectionName in DomainDictionary.
        private static readonly List<string> countriesFromCollectionName = new List<string>
        {
            "us", "gb", "fr", "in", "ao", "ie", "no", "ng", "ca", "nl", "iq", "al", "be", "xk", "hk", "kr", "cn", "jp", "cl", "br"
        };

        private static List<string> luhnValidationEnabledPartners = new List<string>()
        {
            PartnerNames.WebPay,
            PartnerNames.Webblends,
            PartnerNames.Xbox,
            PartnerNames.Cart,
            PartnerNames.AmcWeb,
            PartnerNames.AmcXbox,
            PartnerNames.OfficeOobe,
            PartnerNames.OXOOobe,
            PartnerNames.OXODIME,
            PartnerNames.OXOWebDirect,
            PartnerNames.Amc
        };
    }
}
