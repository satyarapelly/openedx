// <copyright file="Constants.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXCommon
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;

    public static class Constants
    {
        public enum PaymentMethodFamily
        {
            credit_card,
            mobile_billing_non_sim,
            ewallet,
            direct_debit,
            invoice_credit,
            @virtual,
            offline_bank_transfer,
            online_bank_transfer,
            unionpay_creditcard,
            unionpay_debitcard,
            real_time_payments,
        }

        public static class ClientNames
        {
            public const string Unknown = "UnknownCaller";
        }

        public static class RedirectionPatterns
        {
            public const string FullPage = "fullPage";
            public const string Inline = "inline";
            public const string IFrame = "iFrame";
            public const string QRCode = "QRCode";
        }

        public static class PaymentMethodType
        {
            public const string AlipayBillingAgreement = "alipay_billing_agreement";
            public const string CreditCardVisa = "visa";
            public const string CreditCardMasterCard = "mc";
            public const string CreditCardAmericanExpress = "amex";
            public const string CreditCardDiscover = "discover";
            public const string UnionPay = "unionpay";
            public const string UnionPayCreditCard = "unionpay_creditcard";
            public const string UnionPayDebitCard = "unionpay_debitcard";
            public const string IdealBillingAgreement = "ideal_billing_agreement";
            public const string Kakaopay = "kakaopay";
            public const string PayPal = "paypal";
            public const string Ach = "ach";
            public const string Sepa = "sepa";
            public const string Klarna = "klarna";
            public const string LegacyInvoice = "legacy_invoice";
            public const string Check = "check";
            public const string InvoiceBasicVirtual = "invoice_basic";
            public const string InvoiceCheckVirtual = "invoice_check";
            public const string AlipayVirtual = "alipay";
            public const string UnionpayVirtual = "unionpay";
            public const string Paysafecard = "paysafecard";
            public const string LegacyBilldeskPayment = "legacy_billdesk_payment";
            public const string Venmo = "venmo";
            public const string UPI = "upi";
            public const string UPIQr = "upi_qr";
            public const string UPICommercial = "upi_commercial";
            public const string UPIQrCommercial = "upi_qr_commercial";
            public const string GooglePay = "googlepay";
            public const string ApplePay = "applepay";
            public const string CreditCardRupay = "rupay";
        }

        public static class Dimensions
        {
            public const string IPAddress = "IPAddress";
            public const string AccountId = "AccountId";
        }

        public static class PIDLIntegrationErrorCodes
        {
            public const string InvalidPicvDetailsPayload = "InvalidPicvDetailsPayload";
        }

        public static class PXServiceErrorCodes
        {
            public const string InvalidParameter = "InvalidParameter";
        }

        public static class StaticDescriptionTypes
        {
            public const string Cc3DSRedirectAndStatusCheckPidl = "cc3DSRedirectAndStatusCheckPidl";
            public const string Cc3DSStatusCheckPidl = "cc3DSStatusCheckPidl";
            public const string GenericPollingStaticPidl = "genericPollingStatic";
            public const string LegacyBillDesk3DSRedirectAndStatusCheckPidl = "legacyBillDesk3DSRedirectAndStatusCheckPidl";
            public const string LegacyBillDesk3DSStatusCheckPidl = "legacyBillDesk3DSStatusCheckPidl";
        }

        public static class FlightValues
        {
            public const string PXAlipayQRCode = "PXAlipayQRCode";
            public const string PxEnableAddCcQrCode = "PxEnableAddCcQrCode";
            public const string PXEnableHandleTransactionNotAllowed = "PXEnableHandleTransactionNotAllowed"; 
        }

        public static class DisplayCustomizationDetail
        {
            public const string EnableIndia3dsForNonZeroPaymentTransaction = "EnableIndia3dsForNonZeroPaymentTransaction";
            public const string PXEnableIndia3DS1Challenge = "PXEnableIndia3DS1Challenge";
            public const string India3dsEnableForBilldesk = "India3dsEnableForBilldesk";
            public const string UsePSSForPXFeatureFlighting = "UsePSSForPXFeatureFlighting";
            public const string Psd2IgnorePIAuthorization = "Psd2IgnorePIAuthorization";
        }

        public static class ServiceNames
        {
            public const string PXService = "PXService";
            public const string InstrumentManagementService = "InstrumentManagementService";
            public const string OrchestrationService = "OrchestrationService";
            public const string AccountService = "AccountService";
            public const string TaxIdService = "TaxIdService";
            public const string MerchantCapabilitiesService = "MerchantCapabilitiesService";
            public const string PayerAuthService = "PayerAuthService";
            public const string SessionService = "SessionService";
            public const string PurchaseService = "PurchaseService";
            public const string D365Service = "D365Service";
            public const string CatalogService = "CatalogService";
            public const string TokenPolicyService = "TokenPolicyService";
            public const string StoredValueService = "StoredValueService";
            public const string AddressEnrichmentService = "AddressEnrichmentService";
            public const string TransactionService = "TransactionService";
            public const string RDSService = "RDSService";
            public const string ShortURLService = "ShortURLService";
            public const string SellerMarketPlaceService = "SellerMarketPlaceService";
            public const string PaymentThirdPartyService = "PaymentThirdPartyService";
            public const string HIPService = "HIPService";
            public const string MSRewardsService = "MSRewardsService";
            public const string AzureExPService = "AzureExPService";
            public const string RiskService = "RiskService";
            public const string PartnerSettingsService = "PartnerSettingsService";
            public const string IssuerService = "IssuerService";
            public const string WalletService = "WalletService";
            public const string TransactionDataService = "TransactionDataService";
            public const string ChallengeManagementService = "ChallengeManagementService";
            public const string NetworkTokenizationService = "NetworkTokenizationService";
            public const string TokenizationService = "TokenizationService";
            public const string PaymentOrchestratorService = "PaymentOrchestratorService";
            public const string FraudDetectionService = "FraudDetectionService";
        }

        public static class FeatureNames
        {
            public const string PSD2 = "PSD2";
            public const string ThreeDSOne = "threeDSOne";
        }

        public static class ScenarioNames
        {
            public const string SecondScreenAddPi = "secondScreenAddPi";
        }

        public static class PartnerNames
        {
            public const string Xbox = "xbox";
            public const string Cart = "cart";
            public const string OXODIME = "oxodime";
            public const string OXOWebDirect = "oxowebdirect";
            public const string Webblends = "webblends";
            public const string WebblendsInline = "webblends_inline";
            public const string CommercialStores = "commercialstores";
            public const string AmcWeb = "amcweb";
            public const string Azure = "azure";
            public const string AzureIbiza = "azureibiza";
            public const string AzureSignup = "azuresignup";
            public const string OfficeOobe = "officeoobe";
            public const string OXOOobe = "oxooobe";
            public const string SmbOobe = "smboobe";
            public const string AppSource = "appsource";
            public const string SetupOffice = "setupoffice";
            public const string SetupOfficeSdx = "setupofficesdx";
            public const string NorthStarWeb = "northstarweb";
            public const string DefaultPartnerName = "default";
            public const string DefaultTemplate = "defaulttemplate";
            public const string Storify = "storify";
            public const string XboxNative = "xboxnative";
            public const string XboxSubs = "xboxsubs";
            public const string Saturn = "saturn";
            public const string WindowsSettings = "windowssettings";
            public const string WindowsSubs = "windowssubs";
            public const string WindowsStore = "windowsstore";
            public const string Xbet = "xbet";
            public const string XboxCardApp = "xboxcardapp";
            public const string XboxSettings = "xboxsettings";
            public const string Macmanage = "macmanage";
            public const string PlayXbox = "playxbox";

            // Partners with ONLY Test Traffic
            public const string AppInsights = "appinsights";
            public const string PXCOT = "px.cot";
            public const string PIFDCOT = "pifd.cot";
            public const string PifdCot2 = "pifdCot";

            // Partners that don't have config files
            public const string MST = "mst";
            public const string ThreeDS2AuthN = "3ds2authn";
        }

        public static class ExpiryDate
        {
            public const string IndiaCvvChallengeExpiryDateMasked = "Unknown";
        }

        public static class ChallengeDescriptionTypes
        {
            public const string PaypalQrCode = "paypalQrCode";
        }

        public static class ChallengeTypes
        {
            public const string ThreeDSOneQrCode = "threeDSOneQrCode";
            public const string CreditCardQrCode = "creditCardQrCode";
        }

        public static class CharUnicodes
        {
            public const string SuperscriptOne = "\u00B9";
            public const string SuperscriptTwo = "\u00B2";
            public const string SuperscriptThree = "\u00B3";
            public const string SuperscriptOpenParenthesis = "\u207D";
            public const string SuperscriptCloseParenthesis = "\u207E";
        }

        public static class PartnerGroups
        {
            [SuppressMessage("Microsoft.Security", "CA2104", Justification = "ReadOnlyCollection is immutable and this rule is obsolete.")]
            public static readonly ReadOnlyCollection<string> SMDEnabledPartners = new ReadOnlyCollection<string>(new[]
            {
                PartnerNames.Cart,
                PartnerNames.Webblends,
                PartnerNames.WebblendsInline,
                PartnerNames.OXODIME,
                PartnerNames.OXOWebDirect,
                PartnerNames.SetupOffice,
                PartnerNames.OfficeOobe,
                PartnerNames.OXOOobe,
                PartnerNames.Xbox,
                PartnerNames.SetupOfficeSdx
            });

            // Partners enabled for credit card tokenization retry on failures.
            [SuppressMessage("Microsoft.Security", "CA2104", Justification = "ReadOnlyCollection is immutable and this rule is obsolete.")]
            public static readonly ReadOnlyCollection<string> CCTokenizationRetryEnabledPartners = new ReadOnlyCollection<string>(new[]
            {
                PartnerNames.Azure
            });

            // Partners enabled for credit card quick resolution.
            [SuppressMessage("Microsoft.Security", "CA2104", Justification = "ReadOnlyCollection is immutable and this rule is obsolete.")]
            public static readonly ReadOnlyCollection<string> CCQuickResolutionEnabledPartners = new ReadOnlyCollection<string>(new string[] { });

            [SuppressMessage("Microsoft.Security", "CA2104", Justification = "ReadOnlyCollection is immutable and this rule is obsolete.")]
            public static readonly ReadOnlyCollection<string> CommercialSMDEnabledPartners = new ReadOnlyCollection<string>(new[]
            {
                PartnerNames.AppSource,
                PartnerNames.Azure,
                PartnerNames.AzureIbiza,
                PartnerNames.AzureSignup,
                PartnerNames.CommercialStores,
                PartnerNames.XboxSettings,
                PartnerNames.Storify,
                PartnerNames.SmbOobe
            });

            // Partners that only have test traffic.
            [SuppressMessage("Microsoft.Security", "CA2104", Justification = "ReadOnlyCollection is immutable and this rule is obsolete.")]
            public static readonly ReadOnlyCollection<string> TestTrafficOnlyPartners = new ReadOnlyCollection<string>(new[]
            {
                PartnerNames.AppInsights,
                PartnerNames.PXCOT,
                PartnerNames.PIFDCOT,
                PartnerNames.PifdCot2,
             });

            // Partners that don't require config files
            [SuppressMessage("Microsoft.Security", "CA2104", Justification = "ReadOnlyCollection is immutable and this rule is obsolete.")]
            public static readonly ReadOnlyCollection<string> NoConfigNeededPartners = new ReadOnlyCollection<string>(new[]
            {
                PartnerNames.MST,
                PartnerNames.ThreeDS2AuthN
            });

            public static bool IsStandardizedPartner(string partnerName)
            {
                return string.Equals(partnerName, PartnerNames.WindowsSettings, StringComparison.InvariantCultureIgnoreCase); 
            }

            public static bool IsAzureBasedPartner(string partnerName)
            {
                return string.Equals(partnerName, PartnerNames.Azure, StringComparison.InvariantCultureIgnoreCase)
                    || string.Equals(partnerName, PartnerNames.AzureSignup, StringComparison.InvariantCultureIgnoreCase)
                    || string.Equals(partnerName, PartnerNames.AzureIbiza, StringComparison.InvariantCultureIgnoreCase)
                    || string.Equals(partnerName, PartnerNames.AppSource, StringComparison.InvariantCultureIgnoreCase);
            }

            public static bool IsXboxNativePartner(string partnerName)
            {
                return string.Equals(partnerName, PartnerNames.Storify, StringComparison.InvariantCultureIgnoreCase)
                    || string.Equals(partnerName, PartnerNames.XboxSubs, StringComparison.InvariantCultureIgnoreCase)
                    || string.Equals(partnerName, PartnerNames.XboxSettings, StringComparison.InvariantCultureIgnoreCase)
                    || string.Equals(partnerName, PartnerNames.Saturn, StringComparison.InvariantCultureIgnoreCase)
                    || string.Equals(partnerName, PartnerNames.Xbet, StringComparison.InvariantCultureIgnoreCase)
                    || string.Equals(partnerName, PartnerNames.XboxCardApp, StringComparison.InvariantCultureIgnoreCase);
            }

            public static bool IsVenmoEnabledPartner(string partnerName)
            {
                return IsXboxNativePartner(partnerName)
                    || IsVenmoEnabledWebPartner(partnerName);
            }

            public static bool IsVenmoEnabledWebPartner(string partnerName)
            {
                return string.Equals(partnerName, PartnerNames.AmcWeb, StringComparison.InvariantCultureIgnoreCase)
                    || string.Equals(partnerName, PartnerNames.Cart, StringComparison.InvariantCultureIgnoreCase)
                    || string.Equals(partnerName, PartnerNames.NorthStarWeb, StringComparison.InvariantCultureIgnoreCase)
                    || string.Equals(partnerName, PartnerNames.OXOWebDirect, StringComparison.InvariantCultureIgnoreCase)
                    || string.Equals(partnerName, PartnerNames.OXODIME, StringComparison.InvariantCultureIgnoreCase)
                    || string.Equals(partnerName, PartnerNames.Webblends, StringComparison.InvariantCultureIgnoreCase)
                    || string.Equals(partnerName, PartnerNames.PlayXbox, StringComparison.InvariantCultureIgnoreCase);
            }

            public static bool IsWindowsNativePartner(string partnerName)
            {
                return string.Equals(partnerName, Constants.PartnerNames.WindowsSettings, StringComparison.InvariantCultureIgnoreCase)
                    || string.Equals(partnerName, Constants.PartnerNames.WindowsSubs, StringComparison.InvariantCultureIgnoreCase)
                    || string.Equals(partnerName, Constants.PartnerNames.WindowsStore, StringComparison.InvariantCultureIgnoreCase);
            }

            public static bool IsGroupedSelectPMPartner(string partnerName)
            {
                return IsXboxNativePartner(partnerName) || IsWindowsNativePartner(partnerName);
            }

            public static bool IsTestPartner(string partnerName)
            {
                return TestTrafficOnlyPartners.Contains(partnerName, StringComparer.InvariantCultureIgnoreCase);
            }

            public static bool IsNoConfigPartner(string partnerName)
            {
                return NoConfigNeededPartners.Contains(partnerName, StringComparer.InvariantCultureIgnoreCase);
            }
        }

        public static class MarketGroups
        {
            [SuppressMessage("Microsoft.Security", "CA2104", Justification = "ReadOnlyCollection is immutable and this rule is obsolete.")]
            public static readonly ReadOnlyCollection<string> CommercialSMDEnabledMarkets = new ReadOnlyCollection<string>(new[]
            {
                "li",
                "mt",
                "is",
                "cy",
                "lu",
                "sk",
                "si",
                "hr",
                "lv",
                "hu",
                "bg",
                "ie",
                "ee",
                "lt",
                "cz",
                "gr",
                "no",
                "dk",
                "fi",
                "ch",
                "pt",
                "be",
                "se",
                "it",
                "ro",
                "at",
                "nl",
                "es",
                "pl",
                "fr",
                "de",
                "gb",
            });
        }

        public static class OrderingStringAccessibilityLabels
        {
            public const string ButtonOrdering = "{0} of {1}";
        }

        public static class AuthenticationErrorCode
        {
            /// <summary>
            /// Error code for no token found in request
            /// </summary>
            public static readonly string NoSecurityTokenFound = "NoSecurityTokenFound";

            /// <summary>
            /// Error code for signature is invalid. If we have multiple token authenticator againest different authority
            /// eg. AAD PPE and PROD, the signatures are different, we may hit the error as expected.
            /// If we saw this error, we can continue with next token authenticator
            /// </summary>
            public static readonly string SecurityTokenInvalidSignature = "SecurityTokenInvalidSignature";

            /// <summary>
            /// Error code for non Bearer schema
            /// </summary>
            public static readonly string InvalidSchema = "InvalidSchema";

            /// <summary>
            /// Error code for Security token validation failures incluing invalid issuer, audience, life time , invalid signature, no Expiration and etc
            /// </summary>
            public static readonly string SecurityTokenValidationFailed = "SecurityTokenValidationFailed";

            /// <summary>
            /// Error code for Security Token Signature Key Not Found Exception
            /// </summary>
            public static readonly string SecurityTokenSignatureKeyNotFound = "SecurityTokenSignatureKeyNotFound";

            /// <summary>
            /// Error code if MISE faliure response is empty.
            /// </summary>
            public static readonly string EmptyModuleCreatedFailureResponse = "EmptyModuleCreatedFailureResponse";

            /// <summary>
            /// Error code for unknown MISE failure.
            /// </summary>
            public static readonly string UnexpectedMiseValidaionFailure = "UnexpectedMiseValidaionFailure";
        }

        public static class HeaderKey
        {
            public const string RequesterKey = "x-ms-requestor";
        }

        public static class HeaderValue
        {
            public const string RequesterValue = "Microsoft";
        }

        public static class WalletTypeValues
        {
            public const string ApplePay = "apay";
            public const string GooglePay = "gpay";
        }

        public static class DataSourceConstants
        {
            public const string PaymentInstruments = "paymentInstruments";
            public const string AllowedCountries = "allowedCountries";
            public const string FilterPaymentInstrumentsByCountry = "filterPaymentInstrumentsByCountry";
        }

        public static class ButtonActions
        {
            public const string MovePrevious = "movePrevious";
        }

        public static class ButtonDisplayHints
        {
            public const string CancelBackButton = "cancelBackButton";
            public const string SaveButton = "saveButton";
            public const string SaveSecondScreenButton = "saveSecondScreenButton";
        } 
        
        public static class DisplayPageIds
        {
            public const string CreditCardQrCodePage = "creditCardQrCodePage";
        }
        
        public static class ConsentMessageIds
        {
            public const string ConsentMessageHintId = "consentMessage";
            public const string ConsentMessageStatement = "By selecting Save, I authorize Microsoft to add my card information to the account signed in with {0}.";
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

        public static class DataDescriptionPropertyType
        {
            public const string UserData = "userData";
            public const string ClientData = "clientData";
        }

        public static class DataDescriptionTransformationType
        {
            public const string RegexTransformation = "regex";
        }

        public static class DataDescriptionTransformationTarget
        {
            public const string ForSubmit = "forSubmit";
        }

        public static class DataDescriptionDataType
        {
            public const string TypeString = "string";
            public const string TypeBool = "bool";
        }

        public static class ShortURL
        {
            public const int CodeLength = 6;
            public const int TTLMinutes = 20;
            public const int PurgeIntervalDays = 30;
        }

        public static class ShortURLDBAction
        {
            public const string Create = "create";
            public const string Read = "read";
            public const string Update = "update";
            public const string Delete = "delete";
        }
    }
}
