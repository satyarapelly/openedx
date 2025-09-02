// <copyright file="Constants.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace CIT.PidlFactory.V7
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    internal class TestConstants
    {
        private static HashSet<string> partnersWithPageSplits = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            PartnerNames.Xbox,
            PartnerNames.AmcXbox
        };

        private static HashSet<string> partnersToEnablePayPal2ndScreenRedirectButton = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            PartnerNames.Xbox,
            PartnerNames.AmcXbox
        };

        private static HashSet<string> partnersToEnablePaypalRedirectOnTryAgain = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            PartnerNames.Webblends
        };

        private static HashSet<string> allPartners = new HashSet<string>()
        {
             PartnerNames.Amc,
             PartnerNames.AmcWeb,
             PartnerNames.AmcXbox,
             PartnerNames.AppSource,
             PartnerNames.Azure,
             PartnerNames.AzureSignup,
             PartnerNames.AzureIbiza,
             PartnerNames.BingTravel,
             PartnerNames.Bing,
             PartnerNames.Cart,
             PartnerNames.CommercialStores,
             PartnerNames.CommercialSupport,
             PartnerNames.CommercialWebblends,
             PartnerNames.ConsumerSupport,
             PartnerNames.ConsoleTemplate,
             PartnerNames.DefaultPartner,
             PartnerNames.GGPDEDS,
             PartnerNames.Marketplace,
             PartnerNames.Mseg,
             PartnerNames.MSTeams,
             PartnerNames.NorthStarWeb,
             PartnerNames.Office,
             PartnerNames.OfficeOobe,
             PartnerNames.OfficeOobeInApp,
             PartnerNames.OXOOobe,
             PartnerNames.OneDrive,
             PartnerNames.OXODIME,
             PartnerNames.OXOWebDirect,
             PartnerNames.Payin,
             PartnerNames.SetupOffice,
             PartnerNames.SetupOfficeSdx,
             PartnerNames.SmbOobe,
             PartnerNames.StoreOffice,
             PartnerNames.Storify,
             PartnerNames.SecondScreenTemplate,
             PartnerNames.Test,
             PartnerNames.Wallet,
             PartnerNames.Webblends,
             PartnerNames.WebblendsInline,
             PartnerNames.WebPay,
             PartnerNames.WindowsSettings,
             PartnerNames.WindowsSubs,
             PartnerNames.Xbox,
             PartnerNames.XboxWeb,
             PartnerNames.WindowsNative,
             PartnerNames.WindowsStore,
             PartnerNames.XboxNative,
             PartnerNames.XboxSubs,
             PartnerNames.XboxSettings,
             PartnerNames.Saturn,
             TemplateNames.OnePage,
             TemplateNames.TwoPage,
             TemplateNames.Selectpmbuttonlist,
             TemplateNames.Selectpmdropdown,
             TemplateNames.Selectpmradiobuttonlist,
             TemplateNames.Listpidropdown,
             TemplateNames.DefaultTemplate,
             TemplateNames.Listpiradiobutton,
             TemplateNames.Listpibuttonlist
        };

        private static HashSet<string> addressTypesWithHapi = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            DescriptionTypes.HapiServiceUsageAddressDescription,
            DescriptionTypes.HapiV1ShipToIndividualAddressDescription,
            DescriptionTypes.HapiV1ShipToOrganizationAddressDescription,
            DescriptionTypes.HapiV1SoldToIndividualAddressDescription,
            DescriptionTypes.HapiV1SoldToOrganizationAddressDescription,
            DescriptionTypes.HapiV1BillToIndividualAddressDescription,
            DescriptionTypes.HapiV1BillToOrganizationAddressDescription
        };

        private static HashSet<string> addressTypesToValidate = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            DescriptionTypes.BillingDescription,
            DescriptionTypes.OrgAddressDescription,
            DescriptionTypes.BillingGroupDescription,
            DescriptionTypes.HapiServiceUsageAddressDescription,
            DescriptionTypes.HapiV1ShipToIndividualAddressDescription,
            DescriptionTypes.HapiV1ShipToOrganizationAddressDescription,
            DescriptionTypes.HapiV1SoldToIndividualAddressDescription,
            DescriptionTypes.HapiV1SoldToOrganizationAddressDescription,
            DescriptionTypes.HapiV1BillToIndividualAddressDescription,
            DescriptionTypes.HapiV1BillToOrganizationAddressDescription,
            DescriptionTypes.ShippingPatchDescription
        };

        public static HashSet<string> AddressTypesWithHapi
        {
            get
            {
                return addressTypesWithHapi;
            }
        }

        public static HashSet<string> AddressTypesToValidate
        {
            get
            {
                return addressTypesToValidate;
            }
        }

        internal static class TemplateNames
        {
            public const string OnePage = "onepage";
            public const string TwoPage = "twopage";
            public const string Selectpmbuttonlist = "selectpmbuttonlist";
            public const string Selectpmdropdown = "selectpmdropdown";
            public const string Selectpmradiobuttonlist = "selectpmradiobuttonlist";
            public const string Listpidropdown = "listpidropdown";
            public const string DefaultTemplate = "defaulttemplate";
            public const string Listpiradiobutton = "listpiradiobutton";
            public const string Listpibuttonlist = "listpibuttonlist";
        }

        internal static class VirtualPartnerNames
        {
            public const string Macmanage = "macmanage";
        }

        internal static class PartnerNames
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
            public const string ConsoleTemplate = "consoletemplate";
            public const string DefaultPartner = "default";
            public const string GGPDEDS = "ggpdeds";
            public const string Marketplace = "marketplace";
            public const string Mseg = "mseg";
            public const string NorthStarWeb = "northstarweb";
            public const string Office = "office";
            public const string OfficeOobe = "officeoobe";
            public const string OfficeOobeInApp = "officeoobeinapp";
            public const string OXOOobe = "oxooobe";
            public const string SmbOobe = "smboobe";
            public const string OneDrive = "onedrive";
            public const string OXODIME = "oxodime";
            public const string OXOWebDirect = "oxowebdirect";
            public const string Payin = "payin";
            public const string SetupOffice = "setupoffice";
            public const string SetupOfficeSdx = "setupofficesdx";
            public const string StoreOffice = "storeoffice";
            public const string Storify = "storify";
            public const string SecondScreenTemplate = "secondscreentemplate";
            public const string Test = "test";
            public const string Wallet = "wallet";
            public const string Webblends = "webblends";
            public const string WebblendsInline = "webblends_inline";
            public const string Xbox = "xbox";
            public const string XboxWeb = "xboxweb";
            public const string WebPay = "webpay";
            public const string WindowsSettings = "windowssettings";
            public const string WindowsSubs = "windowssubs";
            public const string WindowsStore = "windowsstore";
            public const string WindowsNative = "windowsnative";
            public const string XboxNative = "xboxnative";
            public const string XboxSubs = "xboxsubs";
            public const string XboxSettings = "xboxsettings";
            public const string Saturn = "saturn";
            public const string MSTeams = "msteams";
            public const string PlayXbox = "playxbox";
        }

        public static HashSet<string> AllPartners
        {
            get
            {
                return allPartners;
            }
        }

        public static HashSet<string> PartnersWithPageSplits
        {
            get
            {
                return partnersWithPageSplits;
            }
        }

        public static HashSet<string> PartnersToEnablePayPal2ndScreenRedirectButton
        {
            get
            {
                return partnersToEnablePayPal2ndScreenRedirectButton;
            }
        }

        public static HashSet<string> PartnersToEnablePaypalRedirectOnTryAgain
        {
            get
            {
                return partnersToEnablePaypalRedirectOnTryAgain;
            }
        }

        internal static class DescriptionTypes
        {
            public const string PaymentMethodDescription = "paymentMethod";
            public const string PaymentInstrumentDescription = "paymentInstrument";
            public const string AddressDescription = "address";
            public const string OrderDescription = "order";
            public const string ChallengeDescription = "challenge";
            public const string ProfileDescription = "profile";
            public const string DigitizationDescription = "digitization";
            public const string MiscellaneousDescription = "data";
            public const string TaxIdDescription = "taxId";
            public const string TenantDescription = "tenant";
            public const string StaticDescription = "static";
            public const string PrerequisitesSuffix = "prerequisites";
            public const string BillingGroupDescription = "billingGroup";
            public const string AddressGroupDescription = "addressGroup";
            public const string BillingDescription = "billing";
            public const string OrgAddressDescription = "orgAddress";
            public const string LegalEntityDescription = "legalentity";
            public const string ShippingPatchDescription = "shipping_patch";
            public const string HapiServiceUsageAddressDescription = "hapiServiceUsageAddress";
            public const string HapiV1BillToIndividualAddressDescription = "hapiV1BillToIndividual";
            public const string HapiV1ShipToIndividualAddressDescription = "hapiV1ShipToIndividual";
            public const string HapiV1SoldToIndividualAddressDescription = "hapiV1SoldToIndividual";
            public const string HapiV1BillToOrganizationAddressDescription = "hapiV1BillToOrganization";
            public const string HapiV1ShipToOrganizationAddressDescription = "hapiV1ShipToOrganization";
            public const string HapiV1SoldToOrganizationAddressDescription = "hapiV1SoldToOrganization";
        }

        internal static class DisplayLanguages
        {
            public const string EnUs = "en-US";
        }

        internal static class DescriptionIdentityFields
        {
            internal const string DescriptionType = "description_type";
            internal const string Family = "family";
            internal const string Type = "type";
            internal const string Country = "country";
            internal const string Operation = "operation";
            internal const string Locale = "locale";
            internal const string Step = "step";
            internal const string Scenario = "scenario";
            internal const string CountryCode = "country_code";
            internal const string ResourceIdentity = "resource_id";
            internal const string PaymentInstrumentId = "id";
            internal const string BackupPaymentInstrumentId = "backupId";
        }

        internal static class PidlOperationTypes
        {
            public const string Add = "add";
            public const string Update = "update";
            public const string UpdatePatch = "update_patch";
            public const string Select = "select";
            public const string SelectInstance = "selectinstance";
            public const string SelectSingleInstance = "selectsingleinstance";
            public const string ValidateInstance = "validateinstance";
            public const string Purchase = "purchase";
            public const string Show = "show";
        }

        internal static class HTTPVerbs
        {
            public const string Put = "put";
            public const string Patch = "patch";
            public const string Post = "post";
        }

        public static class PaymentMethodFamilyNames
        {
            public const string CreditCard = "credit_card";
            public const string MobileBillingNonSim = "mobile_billing_non_sim";
            public const string Ewallet = "ewallet";
            public const string DirectDebit = "direct_debit";
            public const string OnlineBankTransfer = "online_bank_transfer";
        }

        public static class PaymentMethodTypeNames
        {
            public const string Amex = "amex";
            public const string Mc = "mc";
            public const string Visa = "visa";
            public const string Jcb = "jcb";
            public const string CupCreditCard = "unionpay_creditcard";
            public const string CupDebitCard = "unionpay_debitcard";
            public const string Alipay = "alipay_billing_agreement";
            public const string Paypal = "paypal";
            public const string Kakaopay = "kakaopay";
            public const string StoredValue = "stored_value";
            public const string Sepa = "sepa";
            public const string Sofort = "sofort";
            public const string Paysafe = "Paysafe";
            public const string Hipercard = "hipercard";
            public const string Elo = "elo";
            public const string Verve = "verve";
            public const string Venmo = "venmo";
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

        internal static class InternalPaymentMethodTypeNames
        {
            public const string AlipayQrCode = "alipayQrCode";
            public const string PaypalRedirect = "paypalRedirect";
        }

        public static class ValidationTypes
        {
            public const string Service = "service";
            public const string Regex = "regex";
            public const string Function = "function";
        }

        internal static class DisplayHintIds
        {
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
            public const string PaymentInstrumentSelect = "paymentInstrument";
            public const string BackupPaymentInstrumentSelect = "backupPaymentInstrument";
            public const string NewPaymentMethodLink = "newPaymentMethodLink";
            public const string SelectPaymentMethodLink = "selectPaymentMethodLink";
            public const string PidlContainer = "pidlContainer";
            public const string PaymentSelection = "paymentInstrumentSelection";
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
            public const string BillingGroup = "billingGroup";
            public const string AddressGroup = "addressGroup";
            public const string BillingGroupListSIBillingGroupId = "billingGroupId";
            public const string PaymentInstrumentShowListPaymentInstrumentId = "paymentInstrumentId";
            public const string BillingGroupListAddBGHyperlinkId = "addNewBG";
            public const string BillingGroupListEditBillingDetailsBGHyperlinkId = "editBillingDetails";
            public const string ProfileEditLEHyperlinkId = "updateSoldToProfileLink";
            public const string BillingGroupLightWeightAddNewPaymentInstrument = "billingGroupLightWeightAddNewPaymentInstrument";
            public const string AddressState = "addressState";
            public const string ShippingAddressState = "shippingAddressState";
            public const string CardholderName = "cardholderName";
            public const string CardNumber = "cardNumber";
            public const string TestMessageForAddCC = "testMessageForAddCC";
            public const string PaymentMethodSelectPageSequence = "paymentMethodSelectPageSequence";
        }

        internal static class ButtonDisplayHintIds
        {
            public const string NextButton = "nextButton";
            public const string PreviousButton = "previousButton";
            public const string SaveButton = "saveButton";
            public const string OkButton = "okButton";
            public const string AgreeAndContinueButton = "agreeAndContinueButton";
            public const string AgreeAndPayButton = "agreeAndPayButton";
            public const string VerifyCodeButton = "verifyCodeButton";
            public const string CancelButton = "cancelButton";
            public const string SubmitButton = "submitButton";
            public const string SucessButton = "successButton";
            public const string SaveNextButton = "saveNextButton";
            public const string SaveButtonHidden = "saveButtonHidden";
            public const string PaypalSignInButton = "paypalSignInButton";
            public const string PaypalYesButton = "paypalYesButton";
            public const string IdealYesButton = "idealYesButton";
            public const string AlipayContinueButton = "alipayContinueButton";
            public const string BuyButton = "buyButton";
            public const string SuccessButtonHidden = "successButtonHidden";
            public const string UseButton = "useButtonPidlPayload";
            public const string UseButtonNext = "useButtonNext";
            public const string SendCodeButton = "sendCodeButton";
            public const string ContinueRedirectButton = "continueRedirectButton";
            public const string OkActionButton = "okActionButton";
            public const string ProfileEditLEHyperlinkId = "updateSoldToProfileLink";
            public const string VerifyPicvButton = "verifyPicvButton";
            public const string PaymentInstrumentShowPIChangeLink = "paymentInstrumentShowPIChangeLink";
            public const string LegacyBillDesk3DSYesButton = "legacyBillDesk3DSYesButton";
        }

        internal static class PaymentMethodSelectType
        {
            internal const string Radio = "radio";
            internal const string DropDown = "dropDown";
            internal const string ButtonList = "buttonList";
        }

        internal static class PidlResourceDescriptionType
        {
            internal const string PaypalRedirectStaticPidl = "paypalredirectpidl";
            internal const string VenmoRedirectStaticPidl = "venmoredirectpidl";
        }

        internal static class ChallengeDisplayHintIds
        {
            public const string CardLogo = "challengeCardLogo";
            public const string CardName = "challengeCardName";
            public const string CardNumber = "challengeCardNumber";
            public const string CardExpiry = "challengeCardExpiry";
            public const string UPIGoToBankButton = "upiGoToBankButton";
            public const string UPIYesVerificationButton = "upiYesVerificationButton";
        }

        internal static class StaticDisplayHintIds
        {
            public const string Cc3DSGoToBankButton = "cc3DSGoToBankButton";
            public const string Cc3DSYesButton = "cc3DSYesButton";
            public const string Cc3DSTryAgainButton = "cc3DSTryAgainButton";
            public const string PaypalRedirectHyperlink = "paypalRedirectLink";
            public const string VenmoRedirectHyperlink = "venmoRedirectLink";
            public const string PaypalNoButton = "paypalNoButton";
            public const string VenmoNoButton = "venmoNoButton";
            public const string ThreeDSIframe = "threeDSIframe";
            public const string Cc3DSRedirectLink = "cc3DSRedirectLink";
            public const string Cc3DSYesVerificationButton = "cc3DSYesVerificationButton";
            public const string Cc3DSRetryButton = "cc3DSRetryButton";
            public const string ThreeDSFingerprintIFrameId = "ThreeDSFingerprintIFrame";
            public const string ThreeDSTimeoutFingerprintIFrame = "ThreeDSTimeoutFingerprintIFrame";
        }

        public static class DomainDictionaryNames
        {
            public const string MSFTCommerceCountries = "MarketsAll";
            public const string TaxIdScenarios = "TaxIdScenarios";
            public const string US50States = "US50States";
            public const string USStates = "USStates";
            public const string BRStates = "BRStates";
            public const string INStates = "INStates";
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
            public const string PaymentInstrumentDisplayId = "displayId";
            public const string Region = "region";
        }

        public static class PidlConfig
        {
            public const string DisplayDescriptionFolderRootPath = "V7/Config/DisplayDescriptions/";
            public const string DefaultPartnerName = PartnerNames.DefaultPartner;
            public const string OxoWebDirectPartnerName = PartnerNames.OXOWebDirect;
            public const string WebblendsPartnerName = PartnerNames.Webblends;
            public const string WebblendsInlinePartnerName = PartnerNames.WebblendsInline;
            public const string XboxPartnerName = PartnerNames.Xbox;
            public const string AmcPartnerName = PartnerNames.Amc;
            public const string AmcWebPartnerName = PartnerNames.AmcWeb;
            public const string AmcXboxPartnerName = PartnerNames.AmcXbox;
            public const string DisplayDescriptionRootPageId = "rootPage";
            public const string FlightingFolderName = "Flights";
        }

        public static class FlightNames
        {
            public const string PXEnablePaypalRedirectUrlText = "PXEnablePaypalRedirectUrlText";
        }

        internal static class PaymentInstruments
        {
            internal const string Mastercard = "{\"id\": \"q62zBAAAAAAJAACA\",\"paymentMethod\": {\"paymentMethodType\": \"mc\",\"properties\": {\"offlineRecurring\": true,\"userManaged\": true,\"soldToAddressRequired\": true,\"splitPaymentSupported\": true,\"supportedOperations\": [\"authorize\", \"charge\", \"refund\", \"chargeback\"],\"taxable\": false,\"providerRemittable\": false},\"paymentMethodFamily\": \"credit_card\",\"display\": {\"name\": \"MasterCard\",\"logo\": \"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v3/logo_mc.png\"}},\"status\": \"Active\",\"details\": {\"accountHolderName\": \"Jie Fan\",\"address\": {\"address_line1\": \"1234\",\"city\": \"Issaquah\",\"region\": \"wa\",\"postal_code\": \"98029\",\"country\": \"us\"},\"expiryYear\": \"2019\",\"expiryMonth\": \"12\",\"picvRequired\": false}}";
            internal const string NonSimMobi = "{\"id\":\"dc9bc02d-8613-453b-a2de-e849087ddfc2\",\"status\":\"pending\",\"paymentMethod\":{\"paymentMethodFamily\":\"mobile_billing_non_sim\",\"paymentMethodType\":\"att-us-nonsim\",\"display\":{\"name\":\"AT&T\",\"logo\":\"https://pmservices.cp.microsoft.com/staticresourceservice/images/v3/logo_att.png\"},\"properties\":{\"offlineRecurring\":true,\"userManaged\":true,\"soldToAddressRequired\":false,\"chargeThresholds\":[{\"country\":\"US\",\"currency\":\"USD\",\"maxPrice\":49.99}],\"supportedOperations\":[\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":true,\"providerCountry\":\"US\"}},\"details\":{\"msisdn\":\"4255050346\",\"pendingOn\":\"Sms\"},\"creationDateTime\":\"2015-08-29T01:54:01.5496558Z\",\"lastUpdatedDateTime\":\"2015-08-29T01:54:01.9402882\"}";
            internal const string SepaPicv = "{\"id\":\"xG0AAAAAAAABAACA\",\"status\":\"active\",\"paymentMethod\":{\"paymentMethodFamily\":\"direct_debit\",\"paymentMethodType\":\"sepa\",\"display\":{},\"properties\":{},},\"details\":{\"bankCode\":\"CITIGB2L\",\"bankName\":\"CitiBank\",\"address\":{},\"picvDetails\":{\"status\":\"inProgress\",\"remainingAttempts\":\"3\"}},\"creationDateTime\":\"2016-01-07T00:00:00-08:00\",\"lastUpdatedDateTime\":\"2016-01-07T00:00:00-08:00\"}";
            internal const string SepaLegacyPicv = "{\"id\":\"xG0AAAAAAAABAACA\",\"status\":\"pending\",\"paymentMethod\":{\"paymentMethodFamily\":\"direct_debit\",\"paymentMethodType\":\"sepa\",\"display\":{},\"properties\":{},},\"details\":{\"bankCode\":\"CITIGB2L\",\"bankName\":\"CitiBank\",\"address\":{},\"pendingOn\":\"picv\",\"picvDetails\":{\"status\":\"inProgress\",\"remainingAttempts\":\"3\"}},\"creationDateTime\":\"2016-01-07T00:00:00-08:00\",\"lastUpdatedDateTime\":\"2016-01-07T00:00:00-08:00\"}";
            internal const string SepaPicvLastTry = "{\"id\":\"xG0AAAAAAAABAACA\",\"status\":\"active\",\"paymentMethod\":{\"paymentMethodFamily\":\"direct_debit\",\"paymentMethodType\":\"sepa\",\"display\":{},\"properties\":{},},\"details\":{\"bankCode\":\"CITIGB2L\",\"bankName\":\"CitiBank\",\"address\":{},\"picvDetails\":{\"status\":\"inProgress\",\"remainingAttempts\":\"1\"}},\"creationDateTime\":\"2016-01-07T00:00:00-08:00\",\"lastUpdatedDateTime\":\"2016-01-07T00:00:00-08:00\"}";
            internal const string India3dsPendingCc = "{\"id\":\"56ef424a-8ecd-47b0-84d7-b79510b9d404\",\"accountId\":\"abe71d91-7b8b-4f2f-aa67-7c57858e00ee\",\"paymentMethod\":{\"paymentMethodType\":\"visa\",\"properties\":{\"offlineRecurring\":true,\"userManaged\":true,\"soldToAddressRequired\":true,\"splitPaymentSupported\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false},\"paymentMethodFamily\":\"credit_card\",\"display\":{\"name\":\"Visa\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_visa_rect.png\",\"logos\":[{\"mimeType\":\"image/png\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_visa_rect.png\"},{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_visa.svg\"}]}},\"status\":\"Pending\",\"creationDateTime\":\"2019-05-06T06:59:45.2639355Z\",\"lastUpdatedDateTime\":\"2019-05-06T06:59:51.983\",\"details\":{\"pendingOn\":\"3ds\",\"sessionQueryUrl\":\"sessions/0b027a5d-09b9-4879-8fb7-64031b926c97\",\"exportable\":false,\"accountHolderName\":\"Kowshik\",\"address\":{\"address_line1\":\"D. No. 76-14-243/3\",\"city\":\"Vijayawada\",\"region\":\"andhra pradesh\",\"postal_code\":\"520012\",\"country\":\"IN\"},\"cardType\":\"credit\",\"lastFourDigits\":\"6840\",\"expiryYear\":\"2021\",\"expiryMonth\":\"2\",\"redirectUrl\":\"https://pmservices.cp.microsoft-int.com/RedirectionService/CoreRedirection/Redirect/0b027a5d-09b9-4879-8fb7-64031b926c97\",\"picvRequired\":false,\"balance\":0.0},\"clientAction\":{\"type\":\"Pidl\",\"context\":[{\"identity\":{\"description_type\":\"static\",\"type\":\"cc3dsredirectandstatuscheckpidl\"},\"data_description\":{\"staticType\":{\"propertyType\":\"clientData\",\"type\":\"hidden\",\"dataType\":\"hidden\",\"is_updatable\":true,\"default_value\":\"cc3dsredirectandstatuscheckpidl\"}},\"displayDescription\":[{\"displayName\":\"RedirectContinuePage\",\"members\":[{\"isSubmitGroup\":true,\"members\":[{\"displayContent\":\"Click here to go to bank website\",\"displayId\":\"cc3DSGoToBankButton\",\"displayType\":\"button\",\"isHighlighted\":true,\"pidlAction\":{\"type\":\"navigateAndMoveNext\",\"isDefault\":true,\"context\":{\"baseUrl\":\"https://pmservices.cp.microsoft-int.com/RedirectionService/CoreRedirection/Redirect/0b027a5d-09b9-4879-8fb7-64031b926c97\",\"successParams\":{\"family\":\"credit_card\",\"type\":\"visa\",\"pendingOn\":\"3ds\"},\"failureParams\":{}}},\"tags\":{\"accessibilityName\":\"Click here to go to bank website\",\"button-full-width\":\"button-full-width\"}},{\"displayContent\":\"Cancel\",\"displayId\":\"cc3DSCancelButton\",\"displayType\":\"button\",\"pidlAction\":{\"type\":\"gohome\",\"context\":\"\"},\"tags\":{\"accessibilityName\":\"Cancel\",\"button-full-width\":\"button-full-width\"}}],\"displayId\":\"cc3DSRedirectButtonGroup\",\"displayType\":\"group\"}],\"displayId\":\"cc3DSRedirectStaticPage\",\"displayType\":\"page\"},{\"displayName\":\"StatusCheckPage\",\"members\":[{\"isSubmitGroup\":true,\"members\":[{\"displayContent\":\"Yes, I am done with the bank verification\",\"displayId\":\"cc3DSYesButton\",\"displayType\":\"button\",\"isHighlighted\":true,\"pidlAction\":{\"type\":\"submit\",\"isDefault\":true,\"context\":{\"href\":\"https://{pifd-endpoint}/users/{userId}/paymentInstrumentsEx/56ef424a-8ecd-47b0-84d7-b79510b9d404?language=en-US&partner=azure&country=in&scenario=azureIbiza&sessionQueryUrl=sessions%2F0b027a5d-09b9-4879-8fb7-64031b926c97\",\"method\":\"GET\"}},\"tags\":{\"accessibilityName\":\"Yes, I am done with the bank verification\",\"button-full-width\":\"button-full-width\"}},{\"displayContent\":\"Cancel\",\"displayId\":\"cc3DSCancelButton\",\"displayType\":\"button\",\"pidlAction\":{\"type\":\"gohome\",\"context\":\"\"},\"tags\":{\"accessibilityName\":\"Cancel\",\"button-full-width\":\"button-full-width\"}}],\"displayId\":\"cc3DSStatusCheckButtonGroup\",\"displayType\":\"group\"}],\"displayId\":\"cc3DSStatusCheckStaticPage\",\"displayType\":\"page\"}],\"strings\":{\"constants\":{\"singleFieldErrorMessage\":\"There is 1 field in the form that requires your attention\",\"multipleFieldsErrorMessage\":\"There are {n} fields in the form that require your attention\"}}}]}}";
            internal const string PayPalRedirect = "{\"type\":null,\"id\":\"GlmnBgAAAAAuAACA\",\"accountId\":\"a951824d-8fea-4194-b56f-438d27c9e388\",\"paymentMethod\":{\"paymentMethodType\":\"paypal\",\"properties\":{\"offlineRecurring\":true,\"userManaged\":true,\"chargeThresholds\":null,\"redirectRequired\":null,\"soldToAddressRequired\":true,\"splitPaymentSupported\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"providerCountry\":null},\"exclusionTags\":[\"XBOXLegacySubscriptions\"],\"paymentMethodFamily\":\"ewallet\",\"display\":{\"name\":\"PayPal\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_paypal_rect.png\",\"logos\":[{\"mimeType\":\"image/png\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_paypal_rect.png\"},{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_paypal.svg\"}]},\"AdditionalDisplayText\":null},\"status\":\"Pending\",\"details\":{\"requiredChallenge\":null,\"supportedChallenge\":null,\"hashIdentity\":null,\"pendingOn\":null,\"sessionQueryUrl\":null,\"pendingDetails\":null,\"exportable\":false,\"accountHolderName\":null,\"accountToken\":null,\"cvvToken\":null,\"address\":null,\"bankIdentificationNumber\":null,\"cardType\":null,\"lastFourDigits\":null,\"expiryYear\":null,\"expiryMonth\":null,\"email\":null,\"redirectUrl\":\"https://paymentsredirectionservice.cp.microsoft-int.com/RedirectionService/CoreRedirection/Redirect/050e15f9-e874-22a4-ab60-1d28434736a4\",\"billingAgreementId\":null,\"firstName\":null,\"middleName\":null,\"lastName\":null,\"payerId\":null,\"billingAgreementType\":\"ChannelInitiatedBilling\",\"phone\":null,\"msisdn\":null,\"paymentAccount\":null,\"picvRequired\":false,\"bankName\":null,\"picvDetails\":null,\"bankCode\":null,\"bankAccountType\":null,\"qiwiAccount\":null,\"currency\":null,\"balance\":0.0,\"lots\":null,\"appSignUrl\":null,\"companyPONumber\":null},\"clientAction\":null,\"version\":null,\"links\":null}";
            internal const string VenmoRedirect = "{\"type\":null,\"id\":\"7cffca89-cb88-4675-9c57-6b4408ec0ca4\",\"accountId\":\"bc81f231-268a-4b9f-897a-43b7397302cc\",\"paymentMethod\":{\"paymentMethodType\":\"venmo\",\"properties\":{\"offlineRecurring\":true,\"userManaged\":true,\"chargeThresholds\":null,\"redirectRequired\":null,\"soldToAddressRequired\":true,\"splitPaymentSupported\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"providerCountry\":null,\"nonStoredPaymentMethodId\":null,\"isNonStoredPaymentMethod\":false},\"paymentMethodGroup\":\"ewallet\",\"groupDisplayName\":\"eWallet\",\"exclusionTags\":null,\"paymentMethodFamily\":\"ewallet\",\"display\":{\"name\":\"Venmo\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_venmo.svg\",\"logos\":[{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_venmo.svg\"}]},\"AdditionalDisplayText\":null},\"status\":\"Pending\",\"creationDateTime\":\"2023-05-05T21:40:52.1762295Z\",\"lastUpdatedDateTime\":\"2023-05-05T21:40:52.2\",\"details\":{\"requiredChallenge\":null,\"supportedChallenge\":null,\"hashIdentity\":null,\"pendingOn\":\"Redirection\",\"sessionQueryUrl\":null,\"pendingDetails\":null,\"exportable\":false,\"daysUntilExpired\":null,\"accountHolderName\":null,\"accountToken\":null,\"cvvToken\":null,\"address\":null,\"bankIdentificationNumber\":null,\"cardType\":null,\"isIndiaExpiryGroupDeleteFlighted\":false,\"physicalCardInstrumentId\":null,\"deviceInfo\":null,\"lastFourDigits\":null,\"physicalCardLastFourDigits\":null,\"expiryYear\":null,\"expiryMonth\":null,\"cardMetadata\":null,\"pendingActionDetailsUrl\":null,\"externalReferenceId\":null,\"paymentAppInstanceId\":null,\"devicePublicKeys\":null,\"cardProfile\":null,\"transactionNotificationHost\":null,\"apiKeyDataList\":null,\"email\":null,\"redirectUrl\":\"https://paymentsredirectionservice.cp.microsoft-int.com/CoreRedirection/redirect/7a9a342e-414d-3084-a7a5-6cab3ac622ef\",\"billingAgreementId\":null,\"firstName\":null,\"middleName\":null,\"lastName\":null,\"payerId\":null,\"billingAgreementType\":null,\"originMarket\":\"US\",\"userName\":null,\"phone\":null,\"msisdn\":null,\"paymentAccount\":null,\"picvRequired\":false,\"bankName\":null,\"picvDetails\":null,\"bankCode\":null,\"bankAccountType\":null,\"qiwiAccount\":null,\"currency\":null,\"balance\":0.0,\"lots\":null,\"appSignUrl\":null,\"companyPONumber\":null,\"defaultDisplayName\":\"@\",\"isFullPageRedirect\":null,\"bankAccountLastFourDigits\":null,\"issuer\":null},\"clientAction\":null,\"version\":null,\"links\":{\"resumeEnrollment\":{\"href\":\"https://paymentsinstrumentservice.cp.microsoft-int.com/InstrumentManagementService/v4.0/bc81f231-268a-4b9f-897a-43b7397302cc/PaymentInstrumentsController/7cffca89-cb88-4675-9c57-6b4408ec0ca4/resumeEnrollment\",\"method\":\"POST\",\"payload\":null,\"propertyName\":null,\"headers\":null,\"errorCodeExpressions\":null}}}";
            internal const string UPIPending = "{\"id\": \"f9fe440d-277e-4b3d-b0f2-a48366d11519\",\"accountId\": \"7e37e56d-9c22-483b-a430-ce4fa5525cba\",\"status\": \"active\",\"paymentMethod\": {\"paymentMethodFamily\": \"real_time_payments\",\"paymentMethodType\": \"upi\",\"paymentMethodGroup\": \"upi\",\"groupDisplayName\": \"UPI\",\"display\": {\"name\": \"UPI\",\"logo\": \"https://pmservices.cp.microsoft.com/staticresourceservice/images/v4/logo_upi.png\",\"logos\": [{\"mimeType\": \"image/png\",\"url\": \"https://pmservices.cp.microsoft.com/staticresourceservice/images/v4/logo_upi.png\"},{\"mimeType\": \"image/svg+xml\",\"url\": \"https://pmservices.cp.microsoft.com/staticresourceservice/images/v4/logo_upi.svg\"}]},\"properties\": {\"offlineRecurring\": false,\"userManaged\": true,\"soldToAddressRequired\": true,\"supportedOperations\": [\"authorize\", \"charge\", \"refund\", \"chargeback\"],\"taxable\": false,\"providerRemittable\": false,\"moBillingIdentityUrl\": ,\"riskyPaymentMethod\": false,\"fundsAvailabilityWindow\": 4320,\"multipleLineItemsSupported\": true,\"splitPaymentSupported\": false,\"purchaseWaitTime\": 20,\"isNonStoredPaymentMethod\": false}},\"details\": {\"vpa\": \"9403039900@ybl\",\"mandateRegistrationSupported\": true,\"vpaValidationStatus\": true,\"country\": \"in\",\"originMarket\": \"IN\",\"redirectUrl\":\"https://{redirection-endpoint}/RedirectionService/CoreRedirection/redirect/Z10010CF7KXN47013879-b66e-4298-b81f-20f0e938fc67\"},\"creationDateTime\": \"2023-07-18T09:46:33.80995Z\",\"lastUpdatedDateTime\": \"2023-07-28T05:51:35.91\"}";
        }

        internal static class UsCreditCardType
        {
            internal static readonly IList<string> TypeNames = new ReadOnlyCollection<string>(
                new List<string>
                {
                    "amex",
                    "discover",
                    "mc",
                    "visa"
                });
        }

        internal static class AccountV3ExtendedHttpHeaders
        {
            public const string Etag = "etag";
            public const string IfMatch = "If-Match";
        }

        internal static class ServiceEndpoints
        {
            public const string BillingGroupAddEndpoint = "https://{hapi-endpoint}/{userId}/billinggroup";
            public const string BillingGroupV7AddEndpoint = "https://{hapi-endpoint}/my-org/billinggroupv7";
            public const string BillingGroupUpdatePONumberEndpoint = "https://{hapi-endpoint}/{userId}/billinggroup/{id}";
            public const string BillingGroupV7UpdatePONumberEndpoint = "https://{hapi-endpoint}/my-org/billinggroupv7/{id}";
        }

        // This array for Japanese prefectures is in Japanese-specific string sort order
        internal static readonly string[] OrderedJPStateValues = new string[]
        {
            "Hokkaido",
            "Aomori-ken",
            "Iwate-ken",
            "Miyagi-ken",
            "Akita-ken",
            "Yamagata-ken",
            "Fukushima-ken",
            "Ibaraki-ken",
            "Tochigi-ken",
            "Gunma-ken",
            "Saitama-ken",
            "Chiba-ken",
            "Tokyo-to",
            "Kanagawa-ken",
            "Niigata-ken",
            "Toyama-ken",
            "Ishikawa-ken",
            "Fukui-ken",
            "Yamanashi-ken",
            "Nagano-ken",
            "Gifu-ken",
            "Shizuoka-ken",
            "Aichi-ken",
            "Mie-ken",
            "Shiga-ken",
            "Kyoto-fu",
            "Osaka-fu",
            "Hyogo-ken",
            "Nara-ken",
            "Wakayama-ken",
            "Tottori-ken",
            "Shimane-ken",
            "Okayama-ken",
            "Hiroshima-ken",
            "Yamaguchi-ken",
            "Tokushima-ken",
            "Kagawa-ken",
            "Ehime-ken",
            "Kochi-ken",
            "Fukuoka-ken",
            "Saga-ken",
            "Nagasaki-ken",
            "Kumamoto-ken",
            "Oita-ken",
            "Miyazaki-ken",
            "Kagoshima-ken",
            "Okinawa-ken"
        };

        internal static class ChallengeDescriptionTypes
        {
            public const string GlobalPIQrCode = "globalPIQrCode";
        }

        internal static class PollingUrls
        {
            public const string GlobalPIQrCodeQueryUrl = "https://pmservices.cp.microsoft.com/RedirectionService/CoreRedirection/Query/";
            public const string GlobalPIQrPurchaseUrl = "https://purchase.mp.microsoft.com/v7.0/users/me/orders/";
            public const string GlobalPIQrCodeIframeRedirectSourceUrl = "https://pmservices.cp.microsoft.com/RedirectionService/CoreRedirection/redirect/";
            public const string PaypalPIQrCodeIframeRedirectSourceUrl = "https://pmservices.cp.microsoft-int.com/RedirectionService/CoreRedirection/Redirect/PSS_0001613c3171c1134d2fbd0fd7ad3a588b06?ru=https%3A%2F%2Fwww.microsoft.com%2Fen-us%2Fstore%2Frds%2Fv2%2FGeneralAddPISuccess%3FpicvRequired%3DFalse%26type%3Dpaypal%26family%3Dewallet%26id%3D&rx=https%3A%2F%2Fwww.microsoft.com%2Fen-us%2Fstore%2Frds%2Fv2%2FGeneralAddPIFailure";
            public const string PaypalPIQrCodeIframeRedirectSourceUrlXboxNative = "https://pmservices.cp.microsoft-int.com/RedirectionService/CoreRedirection/Redirect/PSS_0001613c3171c1134d2fbd0fd7ad3a588b06?ru=https%3A%2F%2Fwww.microsoft.com%2Fen-us%2Fstore%2Frds%2Fv2%2FGeneralAddPISuccess%3FpicvRequired%3DFalse%26type%3Dpaypal%26family%3Dewallet%26id%3D%26redirectType%3DwebPage&rx=https%3A%2F%2Fwww.microsoft.com%2Fen-us%2Fstore%2Frds%2Fv2%2FGeneralAddPIFailure";
            public const string UPIQueryUrlForConfirmPayment = "https://{redirection-endpoint}/RedirectionService/CoreRedirection/Query/";
            public const string UPIPurchaseUrlForConfirmPayment = "https://purchase.mp.microsoft.com/v7.0/users/me/orders/";
        }

        internal static bool IsAzurePartner(string partnerName)
        {
            return string.Equals(partnerName, PartnerNames.Azure, StringComparison.InvariantCultureIgnoreCase)
                || string.Equals(partnerName, PartnerNames.AzureSignup, StringComparison.InvariantCultureIgnoreCase)
                || string.Equals(partnerName, PartnerNames.AzureIbiza, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}