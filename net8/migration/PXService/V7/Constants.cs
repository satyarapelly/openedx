// <copyright file="Constants.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Constants container, each set of constants will be grouped into a nested class
    /// </summary>
    public static class Constants
    {
        public const string PimsVersion = "v4.0";

        public const string JarvisAccountIdHmacProperty = "pxmac";

        // Based on how Partners handle Redirect ClientActions, they can be categorized as below:
        // 1. Cannot handle redirects
        // 2. There is only one window and this window is navigated to the redirect url
        // 3. There is one primary window but a new secondary window is launched and navigated to the redirect url
        // We call #2 above as InlinePartners
        private static List<string> inlinePartners = new List<string>()
        {
            PartnerName.AmcWeb,
            PartnerName.AppSource,
            PartnerName.Azure,
            PartnerName.AzureSignup,
            PartnerName.AzureIbiza,
            PartnerName.Bing,
            PartnerName.Cart,
            PartnerName.CommercialStores,
            PartnerName.OneDrive,
            PartnerName.Payin,
            PartnerName.SetupOffice,
            PartnerName.OXOWebDirect,
            PartnerName.WebblendsInline,
            PartnerName.WebPay,
            PartnerName.NorthStarWeb
        };

        private static List<string> avsSuggestEnabledPartners = new List<string>()
        {
            PartnerName.Cart,
            PartnerName.OXOWebDirect,
            PartnerName.OXODIME,
            PartnerName.Webblends,
            PartnerName.WebblendsInline,
            PartnerName.Xbox,
            PartnerName.OfficeOobe,
            PartnerName.OXOOobe,
            PartnerName.SmbOobe,
            PartnerName.AmcWeb,
            PartnerName.Mseg,
            PartnerName.OneDrive,
            PartnerName.StoreOffice,
            PartnerName.Payin,
            PartnerName.SetupOffice,
            PartnerName.SetupOfficeSdx,
            PartnerName.ConsumerSupport,
            PartnerName.XboxWeb,
            PartnerName.WindowsSettings
        };

        // MAC Prereq
        // When add PI, if user doesn't have profile, instead of prereq page in consumer flow,
        // Prereq flow in the partners below shows Add PI page with hidden linked profile pidl,
        // no profile fields shown on UI,
        // pidlsdk is responsible for prefilling the data
        private static List<string> hiddenLinkedProfilePIDLInAddCCPartners = new List<string>()
        {
            PartnerName.CommercialStores,
            PartnerName.Azure,
            PartnerName.SmbOobe
        };

        private static List<string> luhnValidationEnabledPartners = new List<string>()
        {
            PartnerName.WebPay,
            PartnerName.Webblends,
            PartnerName.Xbox,
            PartnerName.Cart,
            PartnerName.AmcWeb,
            PartnerName.AmcXbox,
            PartnerName.OfficeOobe,
            PartnerName.OXOOobe,
            PartnerName.OXODIME,
            PartnerName.OXOWebDirect,
            PartnerName.Amc
        };

        private static List<string> modalGroupIds = new List<string>()
        {
            "suggestBlockUserEntered",
            "suggestBlock",
            "suggestBlockUserEnteredV2",
            "suggestBlockV2",
        };

        private static List<string> countriesNeedsToShowAVSSuggestionsForAmcWeb = new List<string>()
        {
            "us"
        };

        private static HashSet<string> partnersToEnablePaypalSecondScreenForXbox = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Constants.PartnerName.Xbox,
            Constants.PartnerName.AmcXbox,
            Constants.PartnerName.Storify,
            Constants.PartnerName.XboxSubs,
            Constants.PartnerName.XboxSettings,
            Constants.PartnerName.Saturn,
        };

        private static HashSet<string> partnersToEnableFocusoutResolutionPolicy = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Constants.PartnerName.Xbox,
            Constants.PartnerName.Webblends,
            Constants.PartnerName.Cart
        };

        private static HashSet<string> partnersToEnableRetryOnInvalidRequestData = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Constants.PartnerName.Azure,
            Constants.PartnerName.CommercialStores,
            Constants.PartnerName.Webblends
        };

        private static HashSet<string> partnersToEnableReorderCCAndCardholder = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Constants.PartnerName.Webblends,
            Constants.PartnerName.OXODIME,
            Constants.PartnerName.OXOWebDirect
        };

        private static HashSet<string> countriesToEnablePaypalSecondScreenForXbox = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "AT",
            "BE",
            "BG",
            "HR",
            "CY",
            "CZ",
            "DK",
            "EE",
            "FI",
            "FR",
            "DE",
            "GR",
            "HU",
            "IS",
            "IE",
            "IT",
            "LV",
            "LI",
            "LT",
            "LU",
            "MT",
            "NL",
            "NO",
            "PL",
            "PT",
            "RO",
            "SK",
            "SI",
            "ES",
            "SE",
            "GB"
        };

        public static HashSet<string> countriesToCollectTaxIdUnderFlighting = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "bb",
            "il",
            "kz",
            "la",
            "np",
            "sg",
            "ug",
            "eg",
            "ci",
            "gh",
            "sn",
            "zm"
        };

        private static HashSet<string> thirdPartyPaymentPartners = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            PartnerName.WebPay
        };

        private static HashSet<string> paymentInstrumentAttachIncentivePartners = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            PartnerName.Webblends,
            PartnerName.Xbox,
            PartnerName.OXODIME,
            PartnerName.OXOWebDirect
        };

        private static HashSet<string> jarvisAccountIdHmacPartners = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            PartnerName.NorthStarWeb,
            PartnerName.Webblends
        };

        private static HashSet<string> countriesToDoLegacyValidationBeforeModernValidationAVS = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "us",
            "ca",
        };

        private static List<string> propertiesToMakeMandatory = new List<string>()
        {
            "default_address.first_name",
            "default_address.last_name"
        };

        private static HashSet<string> validatePIOnAttachEnabledPartners = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            PartnerName.AmcWeb,
            PartnerName.OfficeOobe,
            PartnerName.OXOOobe,
            PartnerName.Webblends
        };

        private static HashSet<string> psd2IgnorePIAuthorizationPartners = new HashSet<string>()
        {
            PartnerName.CommercialStores
        };

        private static HashSet<string> partnersToEnablePaypalRedirectOnTryAgain = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            PartnerName.Webblends
        };

        // Test accountIds to skip PX RateLimit. Include all Diff test accounts here so that rate limit won't block them
        private static HashSet<string> pxRateLimitAddCCSkipAccounts = new HashSet<string>()
        {
            "8e342cdc-771b-4b19-84a0-bef4c44911f7"
        };

        private static List<string> allowedOneBoxINTBrowserAuthenticateThreeDSOneRedirectionUrlHostname = new List<string>()
        {
            "www.microsoft.com",
            "account.microsoft.com",
            "stores.office.com",
            "www.xbox.com",
            "origin-int.xbox.com",
            "subs-paynow.msdx.microsoft.com",
            "subs-paynow.msdx.microsoft-ppe.com",
            "checkout.microsoft365.com",
            "checkout.office.com"
        };

        private static List<string> allowedPPEPRODBrowserAuthenticateThreeDSOneRedirectionUrlHostname = new List<string>()
        {
            "www.microsoft.com",
            "onestore-ppe.microsoft.com",
            "account.microsoft.com",
            "stores.office.com",
            "www.xbox.com",
            "origin-int.xbox.com",
            "subs-paynow.msdx.microsoft.com",
            "subs-paynow.msdx.microsoft-ppe.com",
            "checkout.microsoft365.com",
            "checkout.office.com"
        };

        private static HashSet<string> thirdPartyPaymentErrorMsgs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ThirdPartyPaymentsErrorMessages.ResourceNotFound,
            ThirdPartyPaymentsErrorMessages.ServiceError,
            ThirdPartyPaymentsErrorMessages.CvvValueMismatch,
            ThirdPartyPaymentsErrorMessages.ExpiredPaymentInstrument,
            ThirdPartyPaymentsErrorMessages.InvalidPaymentInstrument,
            ThirdPartyPaymentsErrorMessages.RequestDeclined,
            ThirdPartyPaymentsErrorMessages.RequestFailed,
            ThirdPartyPaymentsErrorMessages.InvalidPaymentInstrumentId,
            ThirdPartyPaymentsErrorMessages.PaymentInstrumentNotActive,
            ThirdPartyPaymentsErrorMessages.InvalidRequestData,
            ThirdPartyPaymentsErrorMessages.MerchantSelectionFailure,
            ThirdPartyPaymentsErrorMessages.RetryLimitExceeded,
            ThirdPartyPaymentsErrorMessages.ProcessorDeclined,
            ThirdPartyPaymentsErrorMessages.ProcessorRiskCheckDeclined,
            ThirdPartyPaymentsErrorMessages.AmountLimitExceeded,
            ThirdPartyPaymentsErrorMessages.InsufficientFund,
            ThirdPartyPaymentsErrorMessages.MissingFundingSource,
            ThirdPartyPaymentsErrorMessages.TransactionNotAllowed,
            ThirdPartyPaymentsErrorMessages.InvalidTransactionData,
            ThirdPartyPaymentsErrorMessages.AuthenticationRequired
        };

        private static HashSet<string> thirdPartyPaymentTerminalErrorTypeOne = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ThirdPartyPaymentsErrorCodes.InvalidRequestData,
            ThirdPartyPaymentsErrorCodes.MerchantSelectionFailure,
            ThirdPartyPaymentsErrorCodes.InvalidHeader,
            ThirdPartyPaymentsErrorCodes.ResourceNotFound,
            ThirdPartyPaymentsErrorCodes.RetryLimitExceeded,
            ThirdPartyPaymentsErrorCodes.AmountLimitExceeded,
            ThirdPartyPaymentsErrorCodes.InvalidTransactionData,
            ThirdPartyPaymentsErrorCodes.AuthenticationRequired,
            ThirdPartyPaymentsErrorCodes.ServiceError,
            ThirdPartyPaymentsErrorCodes.RequestDeclined,
            ThirdPartyPaymentsErrorCodes.RequestFailed
        };

        private static HashSet<string> thirdPartyPaymentTerminalErrorTypeTwo = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ThirdPartyPaymentsErrorCodes.InvalidPaymentInstrumentId,
            ThirdPartyPaymentsErrorCodes.PaymentInstrumentNotActive,
            ThirdPartyPaymentsErrorCodes.ProcessorDeclined,
            ThirdPartyPaymentsErrorCodes.ProcessorRiskCheckDeclined,
            ThirdPartyPaymentsErrorCodes.InsufficientFund,
            ThirdPartyPaymentsErrorCodes.MissingFundingSource,
            ThirdPartyPaymentsErrorCodes.TransactionNotAllowed,
        };

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
            add_new_payment_method
        }

        public enum EwalletType
        {
            bitcoin,
            stored_value,
        }

        public static HashSet<string> CountriesToDoLegacyValidationBeforeModernValidationAVS
        {
            get
            {
                return countriesToDoLegacyValidationBeforeModernValidationAVS;
            }
        }

        public static List<string> AvsSuggestEnabledPartners
        {
            get
            {
                return avsSuggestEnabledPartners;
            }
        }

        public static List<string> HiddenLinkedProfilePIDLInAddCCPartners
        {
            get
            {
                return hiddenLinkedProfilePIDLInAddCCPartners;
            }
        }

        public static List<string> LuhnValidationEnabledPartners
        {
            get
            {
                return luhnValidationEnabledPartners;
            }
        }

        public static HashSet<string> JarvisAccountIdHmacPartners
        {
            get
            {
                return jarvisAccountIdHmacPartners;
            }
        }

        public static List<string> ModalGroupIds
        {
            get
            {
                return modalGroupIds;
            }
        }

        public static List<string> CountriesNeedsToShowAVSSuggestionsForAmcWeb
        {
            get
            {
                return countriesNeedsToShowAVSSuggestionsForAmcWeb;
            }
        }

        public static HashSet<string> PartnersToEnablePaypalRedirectOnTryAgain
        {
            get
            {
                return partnersToEnablePaypalRedirectOnTryAgain;
            }
        }

        public static List<string> TabbableDisplayHintTypes
        {
            get
            {
                return new List<string> { "button", "property", "hyperlink" };
            }
        }

        public static List<string> AddressFieldsWithDefaultValueNotNeededForUpdateAndReplace
        {
            get
            {
                return new List<string> { "address_line1", "address_line2", "address_line3", "city", "region", "postal_code", "country" };
            }
        }

        public static List<string> KoreaLocalCardTypes
        {
            get
            {
                return new List<string> { "shinhan", "bc", "kb_kook_min", "samsung", "hyundai", "lotte", "nh", "hana", "citi", "jeju", "woori", "suhyup", "jeonbok", "kwangju", "shinhyup" };
            }
        }

        public static List<string> BrazilLocalCardTypes
        {
            get
            {
                return new List<string> { "hipercard", "elo" };
            }
        }

        public static List<string> NigeriaLocalCardTypes
        {
            get
            {
                return new List<string> { "verve" };
            }
        }

        public static Dictionary<string, string> XboxCardApplyCountryToLanguage
        {
            get
            {
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "us", "en-US" },
                };
            }
        }

        public static Dictionary<string, Tuple<string, string>> LogosWithAlignedAlternative
        {
            get
            {
                return new Dictionary<string, Tuple<string, string>>(StringComparer.OrdinalIgnoreCase)
                {
                    { "logo_visa.svg", new Tuple<string, string>("logo_visa_left_aligned.svg", "logo_visa_right_aligned.svg") },
                    { "logo_mc.svg", new Tuple<string, string>("logo_mc_left_aligned.svg", "logo_mc_right_aligned.svg") },
                    { "logo_amex.svg", new Tuple<string, string>("logo_amex_left_aligned.svg", "logo_amex_right_aligned.svg") },
                    { "logo_discover.svg", new Tuple<string, string>("logo_discover_left_aligned.svg", "logo_discover_right_aligned.svg") },
                    { "logo_hipercard.svg", new Tuple<string, string>("logo_hipercard_left_aligned.svg", "logo_hipercard_right_aligned.svg") },
                    { "logo_elo.svg", new Tuple<string, string>("logo_elo_left_aligned.svg", "logo_elo_right_aligned.svg") },
                    { "logo_paypal_noborder.svg", new Tuple<string, string>("logo_paypal_left_aligned.svg", "logo_paypal_right_aligned.svg") }
                };
            }
        }

        public static Dictionary<string, List<string>> PIFamilyTypeEnableAVSAdditionalFlags
        {
            get
            {
                return new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                {
                    {
                        Constants.PaymentMethodFamilyName.Virtual,
                        new List<string>
                        {
                            Constants.PaymentMethodType.InvoiceBasicVirtual, Constants.PaymentMethodType.InvoiceCheckVirtual
                        }
                    }
                };
            }
        }

        public static List<string> InlinePartners
        {
            get
            {
                return inlinePartners;
            }
        }

        public static HashSet<string> PartnersToEnablePaypalSecondScreenForXbox
        {
            get
            {
                return partnersToEnablePaypalSecondScreenForXbox;
            }
        }

        public static HashSet<string> PartnersToEnableFocusoutResolutionPolicy
        {
            get
            {
                return partnersToEnableFocusoutResolutionPolicy;
            }
        }

        public static HashSet<string> PartnersToEnableRetryOnInvalidRequestData
        {
            get
            {
                return partnersToEnableRetryOnInvalidRequestData;
            }
        }

        public static HashSet<string> CountriesToEnablePaypalSecondScreenForXbox
        {
            get
            {
                return countriesToEnablePaypalSecondScreenForXbox;
            }
        }

        public static HashSet<string> CountriesToCollectTaxIdUnderFlighting
        {
            get
            {
                return countriesToCollectTaxIdUnderFlighting;
            }
        }

        public static HashSet<string> PartnersToEnableReorderCCAndCardholder
        {
            get
            {
                return partnersToEnableReorderCCAndCardholder;
            }
        }

        public static HashSet<string> ThirdPartyPaymentPartners
        {
            get
            {
                return thirdPartyPaymentPartners;
            }
        }

        public static HashSet<string> PIAttachIncentivePartners
        {
            get
            {
                return paymentInstrumentAttachIncentivePartners;
            }
        }

        public static List<string> PropertiesToMakeMandatory
        {
            get
            {
                return propertiesToMakeMandatory;
            }
        }

        public static HashSet<string> ValidatePIOnAttachEnabledPartners
        {
            get
            {
                return validatePIOnAttachEnabledPartners;
            }
        }

        public static HashSet<string> PSD2IgnorePIAuthorizationPartners
        {
            get
            {
                return psd2IgnorePIAuthorizationPartners;
            }
        }

        public static HashSet<string> PXRateLimitAddCCSkipAccounts
        {
            get
            {
                return pxRateLimitAddCCSkipAccounts;
            }
        }

        public static List<string> AllowedOneBoxINTBrowserAuthenticateThreeDSOneRedirectionUrlHostname
        {
            get
            {
                return allowedOneBoxINTBrowserAuthenticateThreeDSOneRedirectionUrlHostname;
            }
        }

        public static List<string> AllowedPPEPRODBrowserAuthenticateThreeDSOneRedirectionUrlHostname
        {
            get
            {
                return allowedPPEPRODBrowserAuthenticateThreeDSOneRedirectionUrlHostname;
            }
        }

        public static HashSet<string> ThirdPartyPaymentErrorMsgs
        {
            get
            {
                return thirdPartyPaymentErrorMsgs;
            }
        }

        public static HashSet<string> ThirdPartyPaymentTerminalErrorsTypeOne
        {
            get
            {
                return thirdPartyPaymentTerminalErrorTypeOne;
            }
        }

        public static HashSet<string> ThirdPartyPaymentTerminalErrorsTypeTwo
        {
            get
            {
                return thirdPartyPaymentTerminalErrorTypeTwo;
            }
        }

        // List of partners to enabled global PI in add resources.
        public static List<string> PartnersEnabledWithGlobalPIInAddResource
        {
            get
            {
                return new List<string>()
                {
                    PartnerName.Cart,
                    PartnerName.Webblends,
                    PartnerName.WindowsStore
                };
            }
        }

        public static class PaymentMethodFamilyName
        {
            public const string Virtual = "virtual";
            public const string OnlineBankTransfer = "online_bank_transfer";
            public const string Ewallet = "ewallet";
            public const string RealTimePayments = "real_time_payments";
        }

        public static class DisplayCustomizationDetail
        {
            public const string AddressSuggestion = "AddressSuggestion";
            public const string SubmitActionType = "SubmitActionType";
            public const string AddressEx = "addressEx";
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
            public const string UnionpayInternational = "unionpay_international";
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
            public const string AddNewPM = "addnewpm";
            public const string AddNewPMNoDefaultSelection = "addnewpm_no_default_selection";
            public const string PayPay = "paypay";
            public const string AlipayHK = "alipayhk";
            public const string GCash = "gcash";
            public const string TrueMoney = "truemoney";
            public const string TouchNGo = "touchngo";
            public const string AlipayCN = "alipaycn";
            public const string StoredValue = "stored_value";
        }

        public static class VirtualPIDisplayName
        {
            public const string Invoice = "Invoice (Pay by check or wire transfer)";
            public const string InvoiceBR = "Invoice (pay by boleto bancario or wire transfer)";
            public const string Alipay = "Alipay";
            public const string Unionpay = "Union Pay Online Payment";
        }

        public static class PartnerName
        {
            public const string Xbox = "xbox";
            public const string Wallet = "wallet";
            public const string Cart = "cart";
            public const string Bing = "bing";
            public const string OXODIME = "oxodime";
            public const string OXOWebDirect = "oxowebdirect";
            public const string Webblends = "webblends";
            public const string WebblendsInline = "webblends_inline";
            public const string CommercialStores = "commercialstores";
            public const string CommercialWebblends = "commercialwebblends";
            public const string CommercialSupport = "commercialsupport";
            public const string WebPay = "webpay";
            public const string Amc = "amc";
            public const string AmcWeb = "amcweb";
            public const string AmcXbox = "amcxbox";
            public const string Azure = "azure";
            public const string AzureIbiza = "azureibiza";
            public const string AzureSignup = "azuresignup";
            public const string MarketPlace = "marketplace";
            public const string Mseg = "mseg";
            public const string OfficeOobe = "officeoobe";
            public const string OXOOobe = "oxooobe";
            public const string SmbOobe = "smboobe";
            public const string OneDrive = "onedrive";
            public const string ConsumerSupport = "consumersupport";
            public const string AppSource = "appsource";
            public const string GGPDEDS = "ggpdeds";
            public const string Payin = "payin";
            public const string SetupOffice = "setupoffice";
            public const string SetupOfficeSdx = "setupofficesdx";
            public const string XboxWeb = "xboxweb";
            public const string StoreOffice = "storeoffice";
            public const string NorthStarWeb = "northstarweb";
            public const string DefaultPartnerName = "default";
            public const string Storify = "storify";
            public const string XboxSettings = "xboxsettings";
            public const string XboxNative = "xboxnative";
            public const string XboxSubs = "xboxsubs";
            public const string Saturn = "saturn";
            public const string MSTeams = "msteams";
            public const string Xbet = "xbet";
            public const string MsTeams = "msteams";
            public const string WindowsSettings = "windowssettings";
            public const string WindowsStore = "windowsstore";
            public const string MCPP = "mcpp";
            public const string BattleNet = "battlenet";
            public const string CandyCrush = "candycrush";
        }

        public static class TemplateName
        {
            public const string DefaultTemplate = "defaulttemplate";
            public const string ListPIDropdown = "listpidropdown";
            public const string ConsoleTemplate = "consoletemplate";
            public const string SecondScreenTemplate = "secondscreentemplate";
        }

        public static class TransationServiceStore
        {
            public const string Azure = "Azure";
            public const string OMS = "OMS";
        }

        public static class PartnerFlightValues
        {
            public const string SoldToHideButton = "soldToHideButton";
            public const string DpHideCountry = "dpHideCountry";
            public const string EnableItalyCodiceFiscale = "enableItalyCodiceFiscale";
            public const string StandaloneProfile = "standaloneProfile";
            public const string ShowMiddleName = "showMiddleName";
            public const string ShowAVSSuggestions = "showAVSSuggestions";
            public const string PXEnableXboxAccessibilityHint = "PXEnableXboxAccessibilityHint";
            public const string PXEnableXboxNewAddressSequenceFrNl = "PXEnableXboxNewAddressSequenceFrNl";
            public const string ApplyAccentBorderWithGutterOnFocus = "ApplyAccentBorderWithGutterOnFocus";
            public const string ShowAVSSuggestionsModal = "showAVSSuggestionsModal";
            public const string AADSupportSMD = "AADSupportSMD";
            public const string SMDDisabled = "SMDDisabled";
            public const string OriginCountryPrefix = "originCountry_";
            public const string PartnerSettingsVersionPrefix = "partnerSettingsVersion_";
            public const string XboxOOBE = "xboxOOBE";
            public const string EnableThreeDSOne = "enableThreeDSOne";
            public const string IndiaTokenizationMessage = "IndiaTokenizationMessage";
            public const string IndiaExpiryGroupDelete = "IndiaExpiryGroupDelete";
            public const string EnableIndiaTokenExpiryDetails = "EnableIndiaTokenExpiryDetails";
            public const string IndiaUPIEnable = "IndiaUPIEnable";
            public const string PxEnableUpi = "PxEnableUpi";
            public const string PxEnableRiskEligibilityCheck = "PxEnableRiskEligibilityCheck";
            public const string EnableGlobalUpiQr = "EnableGlobalUpiQr";
            public const string EnableSelectUpiQr = "EnableSelectUpiQr";
            public const string PXCommercialEnableUpi = "PxCommercialEnableUpi";
            public const string EnableCommercialSelectUpiQr = "EnableCommercialSelectUpiQr";
            public const string EnableCommercialGlobalUpiQr = "EnableCommercialGlobalUpiQr";
            public const string DeletionSubscriptionErrorPidl = StaticDescriptionTypes.DeletionSubscriptionErrorPidl;
            public const string ShowSummaryPage = "showSummaryPage";
            public const string PxEnableVenmo = "PxEnableVenmo";
            public const string PxEnableSelectPMAddPIVenmo = "PxEnableSelectPMAddPIVenmo";
            public const string PXUseJarvisV3ForCompletePrerequisites = "PXUseJarvisV3ForCompletePrerequisites";
            public const string PXUsePartnerSettingsService = "PXUsePartnerSettingsService";
            public const string PXEnableSMSChallengeValidation = "PXEnableSMSChallengeValidation";
            public const string IncludePIDLWithPaymentInstrumentList = "IncludePIDLWithPaymentInstrumentList";
            public const string EnableLtsUpiQRConsumer = "EnableLtsUpiQRConsumer";
            public const string PXReturnFailedSessionState = "PXReturnFailedSessionState";
            public const string PXXboxCardApplyEnableFeedbackButton = "PXXboxCardApplyEnableFeedbackButton";
            public const string PXXboxCardApplyDisableStoreButtonNavigation = "PXXboxCardApplyDisableStoreButtonNavigation";
            public const string PXCOTTestAccounts = "PXCOTTestAccounts";

            // PX flighting to return 3ds Auth not supported error to storefronts.
            public const string PXDisplay3dsNotEnabledErrorInline = "PXDisplay3dsNotEnabledErrorInline";

            // PX flighting to enable sepa jpmc account validation for the storefronts
            // flight cleanup task - 56373987
            public const string EnableSepaJpmc = "EnableSepaJpmc";

            // PX flighting to enable new logo for sepa
            // flight cleanup task - 57811922
            public const string EnableNewLogoSepa = "EnableNewLogoSepa";

            // The partner uses the flight to enable zip+4 2 additional property on jarvis profile address
            // (1) is_customer_consented = true, if user picks user entered address
            // (2) is_avs_full_validation_succeeded = true, if user picks suggested address
            // It is used for address clean up and only show trade avs screen when either flag = true
            public const string EnableAVSAddtionalFlags = "enableAVSAddtionalFlags";
            public const string IndiaTokenizationConsentCapture = "IndiaTokenizationConsentCapture";
            public const string EnablePaymentMethodGrouping = "enablePaymentMethodGrouping";
            public const string EnablePMGroupingSubpageSubmitBlock = "enablePMGroupingSubpageSubmitBlock";
            public const string EnableGlobalPiInAddResource = "enableGlobalPiInAddResource";
            public const string PaymentMethodsConfiguration = "PaymentMethodsConfiguration";
            public const string PXEnableHIPCaptcha = "PXEnableHIPCaptcha";
            public const string PXEnableHIPCaptchaGroup = "PXEnableHIPCaptchaGroup";
            public const string PXEnableRedeemCSVFlow = "PXEnableRedeemCSVFlow";
            public const string PXUsePostProcessingFeatureForRemovePI = "PXUsePostProcessingFeatureForRemovePI";
            public const string PXDisableRedeemCSVFlow = "PXDisableRedeemCSVFlow";
            public const string PXAddSecondaryClientActionWithIgnoreTerminatingErrorHandling = "PXAddSecondaryClientActionWithIgnoreTerminatingErrorHandling";
            public const string PXServicePSSPPEEnvironment = "PXServicePSSPPEEnvironment";
            public const string PXXboxCardApplicationEnableWebview = "PXXboxCardApplicationEnableWebview";
            public const string PXXboxCardApplicationEnableShortUrl = "PXXboxCardApplicationEnableShortUrl";
            public const string PXXboxCardApplicationEnableShortUrlText = "PXXboxCardApplicationEnableShortUrlText";
            public const string PXEnableShortUrlPayPal = "PXEnableShortUrlPayPal";
            public const string PXEnableShortUrlPayPalText = "PXEnableShortUrlPayPalText";
            public const string PXEnableShortUrlVenmo = "PXEnableShortUrlVenmo";
            public const string PXEnableUpdateCCLogo = "PXEnableUpdateCCLogo";
            public const string PXEnableShortUrlVenmoText = "PXEnableShortUrlVenmoText";
            public const string PXSwapSelectPMPages = "PXSwapSelectPMPages";
            public const string PXEnableSecureFieldAddCreditCard = "PXEnableSecureFieldAddCreditCard";
            public const string PXEnableSecureFieldUpdateCreditCard = "PXEnableSecureFieldUpdateCreditCard";
            public const string PXEnableSecureFieldReplaceCreditCard = "PXEnableSecureFieldReplaceCreditCard";
            public const string PXEnableSecureFieldSearchTransaction = "PXEnableSecureFieldSearchTransaction";
            public const string PXEnableSecureFieldCvvChallenge = "PXEnableSecureFieldCvvChallenge";
            public const string PXEnableSecureFieldIndia3DSChallenge = "PXEnableSecureFieldIndia3DSChallenge";
            public const string PXEnableXboxNativeStyleHints = "PXEnableXboxNativeStyleHints";
            public const string PXUseFontIcons = "PXUseFontIcons";
            public const string PXEnableXboxCardUpsell = "PXEnableXboxCardUpsell";
            public const string ListModernPIsWithCardArt = "ListModernPIsWithCardArt";
            public const string PXEnableRupayForIN = "PXEnableRupayForIN";
            public const string IndiaRupayEnable = "IndiaRupayEnable";
            public const string PxEnableAddCcQrCode = "PxEnableAddCcQrCode";
            public const string PXEnablePSD2PaymentInstrumentSession = "PXEnablePSD2PaymentInstrumentSession";
            public const string PXEnableUpdateDiscoverCreditCardRegex = "PXEnableUpdateDiscoverCreditCardRegex";
            public const string PXEnableUpdateVisaCreditCardRegex = "PXEnableUpdateVisaCreditCardRegex";
            public const string PXUseInlineExpressCheckoutHtml = "PXUseInlineExpressCheckoutHtml";
            public const string PXExpressCheckoutUseIntStaticResources = "PXExpressCheckoutUseIntStaticResources";
            public const string PXExpressCheckoutUseProdStaticResources = "PXExpressCheckoutUseProdStaticResources";

            // Used to enable the china union pay payment method for the CN market for international partners
            public const string PXEnableCUPInternational = "PXEnableCUPInternational";

            // used to prevent default selection of Add New Payment Method when PI list is empty
            public const string PXPreventAddNewPaymentMethodDefaultSelection = "PXPreventAddNewPaymentMethodDefaultSelection";

            // Flight to enable purchase polling in confirm payment for UPI
            public const string PXEnablePurchasePollingForUPIConfirmPayment = "PXEnablePurchasePollingForUPIConfirmPayment";

            // Flight to enable instance PI for GPay and Apay
            public const string GPayApayInstancePI = "GPayApayInstancePI";

            public const string IndiaCvvChallengeExpiryGroupDelete = "IndiaCvvChallengeExpiryGroupDelete";
        }

        public static class FlightValues
        {
            public const string PXAlipayQRCode = "PXAlipayQRCode";
            public const string UpdateCaptchaErrorMessage = "UpdateCaptchaErrorMessage";
            public const string HonorNewRiskCode = "HonorNewRiskCode";
            public const string ReturnCardWalletInstanceIdForPidlList = "ReturnCardWalletInstanceIdForPidlList";
        }

        public static class PidlUrlConstants
        {
            public const string StaticResourceServiceImagesV4 = "https://pmservices.cp.microsoft.com/staticresourceservice/images/v4";
            public const string XboxCoBrandedCardQRCodeURL = "https://www.xbox.com/{0}/xbox-mastercard/apply?channelname={1}&referrerid={2}&consoleapplysessionid={3}";
            public const string XboxCoBrandedCardWebviewURL = "https://www.xbox.com/{0}/xbox-mastercard/apply?channelname={1}&referrerid={2}&consoleapplysessionid={3}&isconsolewebview=true";
        }

        public static class RequestHeaderValueTemplate
        {
            public const string RequestContext = "{{\"tenantId\":\"{0}\"}}";
        }

        public static class AppDetails
        {
            public const string WalletPackageName = @"Microsoft.MicrosoftWallet";
            public const string WalletPackageSid = @"ms-app://s-1-15-2-2222533797-2934089070-1576418215-35640970-881892585-609437930-4205438081";
            public const string WalletCientSecrete = @"zu9+9JEbYxz8o8JIvu5o3cdOO97G1hjr";
            public const string PaymentClientAppName = @"Microsoft.Payments.Client";
            public const string PaymentOptionsAppName = @"Microsoft.Payment.Options";
        }

        public static class PaymentInstrument
        {
            public const string Details = "details";
            public const string PaymentMethod = "paymentMethod";
            public const string PaymentMethodFamily = "paymentMethodFamily";
            public const string ClientAction = "clientAction";
            public const string PaymentMethodOperation = "paymentMethodOperation";
            public const string Channel = "channel";
        }

        public static class DFPInstanceIds
        {
            public const string INT = "8e23e7ff-e2a0-4b71-bede-2f0e7d1f6674";
            public const string PROD = "2305fc2c-e4e2-4dc5-a921-00b9d46df0b7";
        }

        public static class DeviceInfoProperty
        {
            public const string IPAddress = "IPAddress";
            public const string UserAgent = "UserAgent";
        }

        public static class XboxConsoleBrowserAgent
        {
            public const string Xbox = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; Xbox; Xbox One) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/111.0.0.0 Safari/537.36 Edg/111.0.1661.35";
        }

        public static class WindowsBrowserAgent
        {
            public const string Windows = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36 Edg/122.0.0.";
        }

        public static class BillingGroupDataDiscriptionName
        {
            public const string JarvisProfileId = "jarvisProfileId";
        }

        public static class ServiceDefaults
        {
            public const string DefaultPartnerName = "default";
            public const string DefaultOperationType = "add";
            public const string ProfileType = "consumer";
        }

        public static class PidlResourceDescriptionType
        {
            public const string Cc3DSRedirectStaticPidl = "cc3DSRedirectPidl";
            public const string PaypalRedirectStaticPidl = "paypalredirectpidl";
            public const string PaypalRetryStaticPidl = "paypalRetryStatic";
            public const string IdealBillingAgreementRedirectStaticPidl = "idealredirectpidl";
            public const string GenericRedirectStaticPidl = "genericredirectpidl";
            public const string GenericPollingStaticPidl = "genericPollingStatic";
            public const string Sms = "sms";
            public const string AchPicVStatic = "achPicvStatic";
            public const string AchPicVChallenge = "ach_picv";
            public const string SepaPicVStatic = "sepaPicvStatic";
            public const string SepaPicVChallenge = "sepa_picv";
            public const string PaypalUpdateAgreementChallenge = "paypalUpdateAgreementChallenge";
            public const string PaymentMethod = "paymentMethod";
            public const string VenmoRedirectStaticPidl = "venmoredirectpidl";
            public const string VenmoRetryStaticPidl = "venmoRetryStatic";
            public const string TokensChallengeTypesPidl = "tokensChallengeTypesPidl";
        }

        public static class PidlIdentityFields
        {
            public const string Type = "type";
            public const string ResourceId = "resource_id";
            public const string Piid = "piid";
            public const string Operation = "operation";
        }

        public static class PidlIdentityValues
        {
            public const string ConsumerPrerequisites = "consumerprerequisites";
        }

        public static class PaymentProviderIds
        {
            public const string Stripe = "stripe";
            public const string PayPal = "paypal";
        }

        public static class PidlResourceIdentity
        {
            public const string List = "list";
        }

        public static class CommercialTaxIdCountryRegionCodes
        {
            public const string India = "IN";
            public const string Taiwan = "TW";
            public const string Italy = "IT";
            public const string Egypt = "EG";
        }

        public static class CommercialTaxIdStatus
        {
            public const string Valid = "Valid";
        }

        public static class CommercialTaxIdTypes
        {
            public const string IndiaGst = "india_state_gst_in_id";
            public const string IndiaPan = "india_pan_id";
            public const string Vat = "vat_id";
            public const string ItalyCodiceFiscale = "national_identification_number";
            public const string EgyptNationalIdentificationNumber = "egypt_national_identification_number";
        }

        public static class ScenarioNames
        {
            public const string MergeData = "mergeData";
            public const string Standalone = "standalone";
            public const string BillingGroup = "billingGroup";
            public const string RS5 = "rs5";
            public const string AddressNoCityState = "addressNoCityState";
            public const string TwoColumns = "twoColumns";
            public const string AzureIbiza = "azureIbiza";
            public const string WithProfileAddress = "withProfileAddress";
            public const string PaypalQrCode = "paypalQrCode";
            public const string GenericQrCode = "genericQrCode";
            public const string ModernAccount = "modernAccount";
            public const string HiddenProfile = "hiddenProfile";
            public const string HiddenProfileWithName = "hiddenProfileWithName";
            public const string HiddenProfileWithNameRoobe = "hiddenProfileWithNameRoobe";
            public const string DepartmentalPurchase = "departmentalPurchase";
            public const string CommercialSignUp = "commercialSignUp";
            public const string WithCountryDropdown = "withCountryDropdown";
            public const string EligiblePI = "eligiblePI";
            public const string FixedCountrySelection = "fixedCountrySelection";
            public const string PayNow = "paynow";
            public const string ChangePI = "changePI";
            public const string WithEditAddress = "withEditAddress";
            public const string SuggestAddressesTradeAVS = "suggestAddressesTradeAVS";
            public const string SuggestAddressesTradeAVSUsePidlModal = "suggestAddressesTradeAVSUsePidlModal";
            public const string SuggestAddressesTradeAVSUsePidlPageV2 = "suggestAddressesTradeAVSUsePidlPageV2";
            public const string Profile = "profile";
            public const string ProfileAddress = "profileAddress";
            public const string CreateBillingAccount = "createBillingAccount";
            public const string DisplayOptionalFields = "displayOptionalFields";
            public const string IndiaThreeDS = "indiathreeds";
            public const string ThreeDSOnePolling = "threedsonepolling";
            public const string PhoneConfirm = "phoneConfirm";
            public const string PidlClientAction = "pidlClientAction";
            public const string PidlContext = "pidlContext";
            public const string PMGrouping = "pmGrouping";
            public const string PollingAction = "pollingAction";
            public const string Roobe = "roobe";
            public const string XboxCoBrandedCard = "xboxCoBrandedCard";
            public const string VenmoQRCode = "venmoQrCode";
            public const string XboxApplyFullPageRender = "xboxApplyFullPageRender";
            public const string VenmoWebPolling = "VenmoWebPolling";
            public const string AddCCTwoPage = "addCCTwoPage";
            public const string StoredValue = "storedValue";
            public const string CreditCardQrCode = "creditCardQrCode";
            public const string SecondScreenAddPi = "secondScreenAddPi";
            public const string ThreeDSTwo = "3ds2";
            public const string AddCCQrCode = "addCCQrCode";
        }

        public static class TestScenarioNames
        {
            public const string VenmoQRCode = "venmoQrCode";
        }

        public static class PXServiceIntegrationErrorCodes
        {
            public const string InvalidPendingOnType = "InvalidPendingOnType";
            public const string InvalidPicvDetailsPayload = "InvalidPicvDetailsPayload";
            public const string IncorrectPaymentMethodCount = "IncorrectPaymentMethodCount";
            public const string InvalidSessionInfo = "InvalidSessionInfo";
            public const string PimsSessionFailed = "PimsSessionFailed";
            public const string PimsSessionExpired = "PimsSessionExpired";
            public const string PimsRollbackReplacePIFailed = "PimsRollbackReplacePIFailed";
            public const string PimsRemovePIAccessDeniedForTheCaller = "RemovePIAccessDeniedForTheCaller";
            public const string PIDLInvalidAllowedPaymentMethods = "PIDLInvalidAllowedPaymentMethods";
            public const string IssuerServiceBadStatus = "IssuerServiceBadStatus";
            public const string InvalidOrExpiredSessionId = "InvalidOrExpiredSessionId";
        }

        public static class ExpressCheckoutErrorCodes
        {
            public const string InvalidAddress = "InvalidAddress";
            public const string InvalidProfile = "InvalidProfile";
            public const string InvalidPaymentInstrument = "InvalidPaymentInstrument";
        }

        public static class XboxCardEligibilityStatus
        {
            public const string None = "None";
            public const string Approved = "Approved";
            public const string PendingOnIssuer = "PendingOnIssuer";
            public const string PendingOnApplication = "PendingOnApplication";
            public const string Error = "Error";
            public const string Cancelled = "Cancelled";
            public const string Duplicate = "Duplicate";
            public const string CardAlreadyIssued = "CardAlreadyIssued";
        }

        public static class PXServiceErrorCodes
        {
            public const string ArgumentIsNull = "ArgumentIsNull";
            public const string RedirectPidlNotFound = "RedirectPidlNotFound";
            public const string LegacyBillableAccountNotFound = "LegacyBillableAccountNotFound";
            public const string LegacyBillableAccountUpdateFailed = "LegacyBillableAccountUpdateFailed";
            public const string LegacyAccountServiceFailed = "LegacyAccountServiceFailed";
        }

        public static class PaymentInstrumentStatus
        {
            public const string Active = "active";
            public const string Pending = "pending";
        }

        public static class PaymentInstrumentPendingOnTypes
        {
            public const string Sms = "sms";
            public const string Redirect = "redirect";
            public const string Notification = "notification";
            public const string Picv = "picv";
            public const string AgreementUpdate = "agreementUpdate";
        }

        public static class PidlTemplatePath
        {
            public const string Etag = "({dataSources.profileResource.etag})";
        }

        public static class RequestType
        {
            public const string AddPI = "addPI";
            public const string GetPI = "getPI";
        }

        public static class DescriptionTypes
        {
            public const string PaymentMethodDescription = "paymentMethod";
            public const string PaymentInstrumentDescription = "paymentInstrument";
            public const string AddressDescription = "address";
            public const string AddressBillingV3 = "addressBillingV3";
            public const string AddressShippingV3 = "addressShippingV3";
            public const string BillingAddressDescription = "billing_address";
            public const string ChallengeDescription = "challenge";
            public const string ProfileDescription = "profile";
            public const string DigitizationDescription = "digitization";
            public const string DataDescription = "data";
            public const string TaxIdDescription = "taxId";
            public const string TenantDescription = "tenant";
            public const string Customer = "customer";
            public const string Checkout = "Checkout";
            public const string Addresses = "Addresses";
            public const string RewardsDescription = "rewards";
            public const string StaticDescription = "static";
            public const string ConfirmDescription = "confirm";
            public const string InitializeDescription = "initialize";
            public const string ExpressCheckout = "expressCheckout";
        }

        public static class TaxIdPropertyDescriptionName
        {
            public const string TaxId = "taxId";
        }

        public static class CreditCardPropertyDescriptionName
        {
            public const string AccountToken = "accountToken";
            public const string CvvToken = "cvvToken";
            public const string HMac = "hmac";
            public const string ExpiryYear = "expiryYear";
            public const string AccountHolderName = "accountHolderName";
        }

        public static class ConditionalFieldsDescriptionName
        {
            public const string HideAddressGroup = "hideAddressGroup";
        }

        public static class FraudDetectionServiceConstants
        {
            public const string ApprovedRecommendation = "Approved";
        }

        // Resource URL Teamplates
        public static class UriTemplate
        {
            // List PIs and Add PI use the relative url below.
            public const string ListPI = PimsVersion + "/{0}/paymentInstruments?deviceId={1}";
            public const string ListEmpOrgPI = PimsVersion + "/emporg/paymentInstruments?deviceId={0}";
            public const string PostPI = PimsVersion + "/{0}/paymentInstruments";
            public const string PostPIForPaymentAccountId = "v3.0/paymentInstruments";
            public const string GetPI = PimsVersion + "/{0}/paymentInstruments/{1}";
            public const string GetSessionDetails = PimsVersion + "/{0}/{1}";
            public const string AccountlessGetExtendedPI = PimsVersion + "/paymentInstruments/{0}/extendedView";
            public const string UpdatePI = PimsVersion + "/{0}/paymentInstruments/{1}/update";
            public const string CardProfile = PimsVersion + "/{0}/paymentInstruments/{1}/cardProfile?deviceId={2}";
            public const string SeCardPersos = PimsVersion + "/{0}/paymentInstruments/{1}/seCardPersos?deviceId={2}";
            public const string ReplenishTransactionCredentials = PimsVersion + "/{0}/paymentInstruments/{1}/replenishTransactionCredentials?deviceId={2}";
            public const string AcquireLUKs = PimsVersion + "/{0}/paymentInstruments/{1}/acquireLuk?deviceId={2}";
            public const string ConfirmLUKs = PimsVersion + "/{0}/paymentInstruments/{1}/confirmNotification?deviceId={2}";
            public const string RemovePI = PimsVersion + "/{0}/paymentInstruments/{1}/remove";
            public const string ReplacePI = PimsVersion + "/{0}/paymentInstruments/{1}/replace";
            public const string PiPendingOperationsResume = PimsVersion + "/{0}/paymentInstruments/{1}/resume";
            public const string ValidateCvv = PimsVersion + "/{0}/paymentInstruments/{1}/validateCvv";
            public const string ValidatePicv = PimsVersion + "/{0}/paymentInstruments/{1}/verifyPicv";
            public const string LinkSession = PimsVersion + "/{0}/paymentInstruments/{1}/LinkTransaction";
            public const string Validate = PimsVersion + "/{0}/paymentInstruments/{1}/validate";
            public const string PendingOperation = PimsVersion + "/{0}";
            public const string DeviceId = "&deviceId={0}";
            public const string Language = "&language={0}";
            public const string GetPaymentMethods = PimsVersion + "/paymentMethods?country={0}&family={1}&type={2}&language={3}";
            public const string GetHIPVisualCaptcha = "v1.0/challenge/visual?partnerid={0}";
            public const string GetHIPAudioCaptcha = "v1.0/challenge/audio?partnerid={0}";
            public const string PostHIPVisualCaptcha = "v1.0/challenge/visual/solution?partnerid={0}&azureregion={1}";
            public const string PostHIPAudioCaptcha = "v1.0/challenge/audio/solution?partnerid={0}&azureregion={1}";
            public const string PidlChallengeDescription = "/challengedescriptions?accountId={0}&piId={1}";
            public const string GetProfilesByAccountId = "/{0}/profiles?type={1}";
            public const string UpdateProfilesById = "/{0}/profiles/{1}";
            public const string GetAddressByAddressId = "/{0}/addresses/{1}";
            public const string PatchAddressByAddressId = "/{0}/addresses/{1}";
            public const string PostAddressForAccountId = "/{0}/addresses";
            public const string GetAddressesByCountry = "/{0}/addresses?country={1}";
            public const string GetTaxIds = "/{0}/tax-ids";
            public const string GetTenantCustomerByIdentity = "/customers/get-by-identity?provider=aad&type=tenant&tid={0}";
            public const string GetEmployeeCustomerByIdentity = "/customers/get-by-identity?provider=aad&type=user&tid={0}&oid={1}";
            public const string GetCustomerById = "/customers/{0}";
            public const string LegacyAddressValidation = "/addresses";
            public const string ModernAddressValidation = "/addresses/validate";
            public const string JarvisPostAddressSyncLegacy3 = "/{0}/addresses?syncToLegacy={1}";
            public const string JarvisGetOrCreateLegacyBillableAccount = "/{0}/get-or-create-legacy-billable-account?country={1}";
            public const string ModernAddressLookup = "/addresses/lookup";
            public const string GetStoredValueFundingCatalog = "/gift-catalog?currency={0}";
            public const string FundStoredValue = "/{0}/funds";
            public const string CheckFundStoredValue = "/{0}/funds/{1}";
            public const string OrchestrationServiceReplacePaymentInstrument = "{0}/paymentInstruments/{1}/replace";
            public const string OrchestrationServiceRemovePaymentInstrument = "{0}/paymentInstruments/{1}/remove";
            public const string TransactionServiceCreatePaymentObject = "{0}/payments";
            public const string TransactionServiceValidateCvv = "{0}/payments/{1}/validate";
            public const string RDSServiceQuery = "/query/{0}";
            public const string PifdAnonymousModernAVSForTrade = "https://{{pifd-endpoint}}/anonymous/addresses/ModernValidate?type={0}&partner={1}&language={2}&scenario={3}&country={4}";
            public const string PxBrowserAuthenticateRedirectionUrlTemplate = "{0}/paymentSessions/{1}/browserAuthenticateRedirectionThreeDSOne";
            public const string GetThirdPartyPaymentMethods = PimsVersion + "/thirdPartyPayments/eligiblePaymentMethods?provider={0}&sellerCountry={1}&buyerCountry={2}";
            public const string PIMSPostSearchByAccountNumber = PimsVersion + "/paymentInstruments/searchByAccountNumber";
            public const string PaymentOrchestratorServiceAttachPaymentInstrument = "paymentrequests/{0}/attachpaymentinstruments";
            public const string PaymentOrchestratorServiceAttachAddressToCheckoutRequest = "checkoutRequests/{0}/attachaddress?type={1}";
            public const string PaymentOrchestratorServiceAttachAddressToPaymentRequest = "paymentRequests/{0}/attachaddress?type={1}";
            public const string PaymentOrchestratorServiceAttachProfileToCheckoutRequest = "checkoutRequests/{0}/attachprofile";
            public const string PaymentOrchestratorServiceAttachProfileToPaymentRequest = "paymentRequests/{0}/attachprofile";
            public const string PaymentOrchestratorServiceConfirmCheckoutRequest = "checkoutRequests/{0}/confirm";
            public const string PaymentOrchestratorServiceAttachPaymentInstrumentToCheckoutRequest = "checkoutRequests/{0}/attachpaymentinstruments";
            public const string PaymentOrchestratorServiceGetClientAction = "checkoutRequests/{0}/clientaction";
            public const string PaymentOrchestratorServiceGetPaymentRequestClientAction = "paymentRequests/{0}/clientactions";
            public const string PaymentOrchestratorServiceAttachPaymentInstrumentWallet = "walletrequests/{0}/attachpaymentinstruments";
            public const string PaymentOrchestratorServiceGetPaymentRequest = "paymentRequests/{0}";
            public const string PaymentOrchestratorServiceConfirmPaymentRequest = "paymentRequests/{0}/confirm";
            public const string PaymentOrchestratorServiceAttachChallengeData = "paymentRequests/{0}/attachChallengeData";
            public const string PaymentOrchestratorServiceGetEligiblePaymentMethods = "walletRequests/{0}/getEligiblePaymentMethods";
            public const string PaymentOrchestratorServiceAttachCheckoutRequestChallengeData = "checkoutRequests/{0}/attachChallengeData";
            public const string PaymentOrchestratorServiceRemoveEligiblePaymentmethods = "paymentRequests/{0}/removeEligiblePaymentMethods";

            // PaymentThirdPartyService url template
            public const string GetPaymentRequest = "payment-providers/{0}/api/payment-requests/{1}";
            public const string GetCheckout = "payment-providers/{0}/api/checkouts/{1}";
            public const string CheckoutCharge = "payment-providers/{0}/api/checkouts/{1}/charge";

            // SellerMarketService url template
            public const string GetSeller = "v1/payment-providers/{0}/sellers/{1} ";

            // Partner Settings url template
            public const string GetPaymentExperienceSettings = "partnersettings/{0}?settingsType=PaymentExperience";

            // IssuerService Apply url template
            public const string Apply = "applications/{0}";

            // IssuerService Application Details url template
            public const string ApplicationDetails = "applications/{0}?cardProduct={1}&sessionId={2}";

            // IssuerService ApplyEligibility url template
            public const string ApplyEligibility = "applications/{0}/eligibility?cardProduct={1}";

            // IssuerService ApplyInitialize url template
            public const string ApplyInitalize = "applications/session";

            // WalletService getWalletConfig url template
            public const string GetWalletConfig = "api/wallet/getproviderdata";

            // WalletService setupSession url template
            public const string SetupProviderSession = "api/wallet/setupprovidersession";

            // WalletService provision wallet token url template
            public const string ProvisionWalletToken = "api/wallet/provision";

            // WalletService setupSession url template
            public const string GenerateDataId = "transactiondata/generatedataid";

            // WalletService walletValidate url template
            public const string WalletValidate = "api/wallet/validate";

            // Transaction data service store api url template
            public const string TransactionDataStore = "transactiondata/{0}/data/{1}";

            // Network tokens
            public const string GetNetworkTokens = "tokens";

            // tokenizable
            public const string Tokenizable = "tokenizable?bankIdentificationNumber={0}&cardProviderName={1}&networkTokenUsage={2}";

            // Network tokens
            public const string ListNetworkTokensWithExternalCardReference = "tokens?externalCardReference={0}";

            // Fetch Credentials
            public const string FetchCredentials = "tokens/{0}/credentials";

            // Request Device Binding Fido
            public const string RequestDeviceBindingFido = "tokens/{0}/devicebinding/fido";

            // Request Challenge
            public const string RequestChallenge = "tokens/{0}/devicebinding/challenges/{1}/request";

            // Validate Challenge
            public const string ValidateChallenge = "tokens/{0}/devicebinding/challenges/{1}/validate";

            // Authenticate Passkeys
            public const string PasskeyAuthenticate = "tokens/{0}/passkeys/authenticate";

            // Set up Passkeys
            public const string PasskeySetup = "tokens/{0}/passkeys/setup";

            // Set Mandates
            public const string SetMandates = "tokens/{0}/passkeys/mandate";

            // MSRewards GetUserInfo url template
            public const string GetMSRewardsUserInfo = "api/users({0})?options=8&channel={1}";

            // MSRewards Redeem url template
            public const string RedeemMSRewards = "api/users({0})/orders";

            // Redirection URL used in confirm payment component
            public const string ConfirmPaymentRedirectUrlTemplate = "https://{{redirection-endpoint}}/RedirectionService/CoreRedirection/redirect/{0}";

            // Token descriptions request url template
            public const string GetTokenDescription = "{0}/users/{1}/tokenDescriptionRequests";

            // Payment client descriptions override template
            public const string GetPaymentClientDescription = "{pidl-endpoint}/paymentClient/descriptions";

            // Fraud detection bot detection url template
            public const string FraudDetectionBotDetectionUrl = "api/v1/botcheck";
        }

        public static class Versions
        {
            public const string ApiVersion = "v7.0";
            public const string Alpha = "alpha";
        }

        public static class KlarnaErrorCodes
        {
            public const string PersonalNumberBadFormat = "PersonalNumberBadFormat";
            public const string InvalidPhoneValue = "InvalidPhoneValue";
            public const string InvalidGender = "InvalidGender";
            public const string InvalidNameAndAddress = "InvalidNameAndAddress";
            public const string InvalidLastName = "InvalidLastName";
            public const string InvalidFirstName = "InvalidFirstName";
            public const string InvalidFirstAndLastNames = "InvalidFirstAndLastNames";
            public const string InvalidPersonalNumber = "InvalidPersonalNumber";
        }

        public static class KlarnaErrorMessages
        {
            public const string PersonalNumberBadFormat = "Check the format of your personal number or date of birth.";
            public const string InvalidPhoneValue = "Check your phone number. We can't match it to a registered address.";
            public const string InvalidGender = "Check the field for gender. It doesn't match our records.";
            public const string InvalidNameAndAddress = "Check your name and address. One doesn't match the other.";
            public const string InvalidLastName = "Check your last name. It doesn't match our records.";
            public const string InvalidFirstName = "Check your first name. It doesn't match our records.";
            public const string InvalidFirstAndLastNames = "Check your first and last name. They don't match.";
            public const string InvalidPersonalNumber = "Check your personal number or date of birth. It doesn't match our records.";
        }

        public static class KlarnaErrorTargets
        {
            public const string PersonalNumber = "nationalIdentificationNumber";
            public const string Phone = "phone";
            public const string LastName = "lastname";
            public const string FirstName = "firstname";
            public const string AddressLine1 = "address_line1";
            public const string AddressLine2 = "address_line2";
            public const string City = "city";
            public const string Country = "country";
            public const string PostalCode = "postal_code";
            public const string Gender = "gender";
        }

        public static class CreditCardErrorCodes
        {
            public const string ValidationFailed = "ValidationFailed";
            public const string TokenizationFailed = "TokenizationFailed";
            public const string ExpiredCard = "ExpiredCard";
            public const string InvalidAccountHolder = "InvalidAccountHolder";
            public const string InvalidAddress = "InvalidAddress";
            public const string InvalidCity = "InvalidCity";
            public const string InvalidCountry = "InvalidCountry";
            public const string InvalidCountryCode = "InvalidCountryCode";
            public const string InvalidCvv = "InvalidCvv";
            public const string InvalidExpiryDate = "InvalidExpiryDate";
            public const string InvalidPaymentInstrumentInfo = "InvalidPaymentInstrumentInfo";
            public const string InvalidState = "InvalidState";
            public const string InvalidZipCode = "InvalidZipCode";
            public const string InvalidRequestData = "InvalidRequestData";
            public const string PrepaidCardNotSupported = "PrepaidCardNotSupported";
            public const string InvalidStreet = "InvalidStreet";
            public const string InvalidIssuerResponseWithTRPAU0009 = "InvalidIssuerResponseWithTRPAU0009";
            public const string InvalidIssuerResponseWithTRPAU0008 = "InvalidIssuerResponseWithTRPAU0008";
            public const string CaptchaChallengeRequired = "CaptchaChallengeRequired";
            public const string ChallengeRequired = "ChallengeRequired";
            public const string InvalidPaymentInstrumentType = "InvalidPaymentInstrumentType";
        }

        public static class CreditCardErrorMessages
        {
            public const string ValidationFailed = "Check that the details in all fields are correct or try a different card.";
            public const string ExpiredCard = "Check your expiration date.";
            public const string InvalidAccountHolder = "Check your name on the card. There appears to be an error in it.";
            public const string InvalidAddress = "Check your address. There appears to be an error in it.";
            public const string InvalidCity = "Check the city in your address. There appears to be an error in it.";
            public const string InvalidCountry = "Choose your country or region again. There appears to be an error in it.";
            public const string InvalidCvv = "Check your security code. There appears to be an error in it.";
            public const string InvalidExpiryDate = "Try a different way to pay. This card has expired.";
            public const string InvalidCardNumber = "Check your info. The card number entered is invalid.";
            public const string InvalidState = "Check the state in your address. There appears to be an error in it.";
            public const string InvalidZipCode = "Check the Zip or Postal code in your address. There appears to be an error in it.";
            public const string PrepaidCardNotSupported = "Sorry, we can't accept pre-paid cards. Please try another payment method.";
            public const string Generic = "Try that again. Something happened on our end. Waiting a bit can help.";
            public const string PIMSValidationFailed = "The payment instrument cannot be validated. Please contact the payment processor for help.";
            public const string InvalidIssuerResponse = "The card is not enabled for 3ds/otp authentication in India.";
            public const string InvalidPaymentInstrumentType = "Check that the details in all fields are correct or try a different card.";
        }

        public static class CreditCardErrorTargets
        {
            public const string CardNumber = "accountToken";
            public const string AccountHolderName = "accountHolderName";
            public const string Cvv = "cvvToken";
            public const string ExpiryMonth = "expiryMonth";
            public const string ExpiryYear = "expiryYear";
            public const string AddressLine1 = "address_line1";
            public const string AddressLine2 = "address_line2";
            public const string AddressLine3 = "address_line3";
            public const string City = "city";
            public const string State = "region";
            public const string Country = "country";
            public const string PostalCode = "postal_code";
        }

        public static class LegacyInvoiceErrorCodes
        {
            public const string InvalidAddress = "InvalidAddress";
            public const string InvalidCity = "InvalidCity";
            public const string InvalidCountry = "InvalidCountry";
            public const string InvalidCountryCode = "InvalidCountryCode";
            public const string InvalidState = "InvalidState";
            public const string InvalidZipCode = "InvalidZipCode";
        }

        public static class LegacyInvoiceErrorMessages
        {
            public const string InvalidAddress = "Check your address. There appears to be an error in it.";
            public const string InvalidCity = "Check the city in your address. There appears to be an error in it.";
            public const string InvalidCountry = "Choose your country or region again. There appears to be an error in it.";
            public const string InvalidState = "Check the state in your address. There appears to be an error in it.";
            public const string InvalidZipCode = "Check the Zip or Postal code in your address. There appears to be an error in it.";
            public const string Generic = "Try that again. Something happened on our end. Waiting a bit can help.";
        }

        public static class LegacyInvoiceErrorTargets
        {
            public const string AddressLine1 = "address_line1";
            public const string AddressLine2 = "address_line2";
            public const string AddressLine3 = "address_line3";
            public const string City = "city";
            public const string State = "region";
            public const string Country = "country";
            public const string PostalCode = "postal_code";
        }

        public static class PicvStatus
        {
            public const string InProgress = "inProgress";
            public const string Failed = "failed";
            public const string Expired = "expired";
            public const string Success = "success";
        }

        public static class DirectDebitErrorCodes
        {
            public const string ValidationFailed = "ValidationFailed";
            public const string InvalidAccountHolder = "InvalidAccountHolder";
            public const string InvalidAddress = "InvalidAddress";
            public const string InvalidBankCode = "InvalidBankCode";
            public const string InvalidCity = "InvalidCity";
            public const string InvalidCountry = "InvalidCountry";
            public const string InvalidPaymentInstrumentInfo = "InvalidPaymentInstrumentInfo";
            public const string InvalidState = "InvalidState";
            public const string InvalidZipCode = "InvalidZipCode";
            public const string InvalidAmount = "InvalidAmount";
            public const string OperationNotSupported = "OperationNotSupported";
        }

        public static class DirectDebitErrorMessages
        {
            public const string ValidationFailedAch = "Check your bank account and routing numbers. The current pair isn't working.";
            public const string ValidationFailedSepa = "Check your BIC and IBAN. The current pair isn't working.";
            public const string Generic = "Try that again. Something happened on our end. Waiting a bit can help.";
            public const string InvalidAccountHolder = "Check your name. This one's not right.";
            public const string InvalidAddress = "Check your address. This one's not right.";
            public const string InvalidBankCodeAch = "Check your bank routing number. This one's not right.";
            public const string InvalidBankCodeSepa = "Check your bank code. This one's not right.";
            public const string InvalidCity = "Check your city. Something's not right.";
            public const string InvalidCountry = "Check your country. Something's not right.";
            public const string InvalidPaymentInstrumentInfoAch = "Check your bank account number. This one's not right.";
            public const string InvalidPaymentInstrumentInfoSepa = "Check your IBAN. This one's not right.";
            public const string InvalidState = "Check your state. Something's not right.";
            public const string InvalidZipCode = "Check your zip code. This one's not right.";
            public const string InvalidAmount = "The amount you entered is incorrect.";
        }

        public static class DirectDebitErrorTargets
        {
            public const string AccountNumber = "accountToken";
            public const string AccountHolderName = "accountHolderName";
            public const string BankAccountType = "bankAccountType";
            public const string BankCode = "bankCode";
            public const string BankName = "bankName";
            public const string AddressLine1 = "address_line1";
            public const string AddressLine2 = "address_line2";
            public const string City = "city";
            public const string State = "region";
            public const string Country = "country";
            public const string PostalCode = "postal_code";
            public const string Amount = "amount";
        }

        public static class CupErrorCodes
        {
            public const string ValidationFailed = "ValidationFailed";
            public const string InvalidPhoneValue = "InvalidPhoneValue";
            public const string InvalidPaymentInstrumentInfo = "InvalidPaymentInstrumentInfo";
            public const string ProcessorUnreachable = "ProcessorUnreachable";
            public const string InvalidRequestData = "InvalidRequestData";
            public const string ProcessorTimeout = "ProcessorTimeout";
            public const string InvalidExpiryDate = "InvalidExpiryDate";
            public const string TooManyOperations = "TooManyOperations";
            public const string InvalidChallengeCode = "InvalidChallengeCode";
            public const string ChallengeCodeExpired = "ChallengeCodeExpired";
            public const string ExpiredCard = "ExpiredCard";
            public const string InvalidCvv = "InvalidCvv";
        }

        public static class CupErrorMessages
        {
            public const string InvalidPhoneOrCard = "Check your card and phone numbers. They don’t go together.";
            public const string Generic = "Try that again. Something happened on our end. Waiting a bit can help.";
            public const string InvalidExpiryDate = "Check your expiration date.";
            public const string TooManySmsRequests = "Wait a bit before you ask for a new code. Your requests exceeded the limit.";
            public const string InvalidSmsCode = "Check your code. The one entered isn't valid.";
            public const string SmsCodeExpired = "Request a new code. This one expired.";
            public const string InvalidPhoneOrCvv = "Check your card security code and your phone number.";
            public const string InvalidCardNumber = "Check your card number. This one isn't valid.";
            public const string InvalidPhoneNumber = "Check your phone number. This one isn't valid.";
        }

        public static class CupErrorTargets
        {
            public const string PhoneNumber = "phone";
            public const string CardNumber = "accountToken";
            public const string Cvv = "cvvToken";
            public const string Sms = "pin";
            public const string ExpiryMonth = "expiryMonth";
            public const string ExpiryYear = "expiryYear";
        }

        public static class QRCodeErrorMessages
        {
            public const string RetryMax = "sessionIdData retry maximum exceeded";
            public const string Expired = "sessionIdData is expired";
        }

        public static class AlipayErrorCodes
        {
            public const string InvalidAlipayAccount = "InvalidAlipayAccount";
            public const string UserMobileNotMatch = "UserMobileNotMatch";
            public const string UserCertNoMatch = "UserCertNoMatch";
            public const string InvalidChallengeCode = "InvalidChallengeCode";
            public const string ChallengeCodeExpired = "ChallengeCodeExpired";
        }

        public static class AlipayErrorMessages
        {
            public const string InvalidAlipayAccount = "Check your AliPay account info. There appears to be an error in it.";
            public const string UserMobileNotMatch = "Check your mobile number. There appears to be an error in it.";
            public const string UserCertNoMatch = "Check your last 5 digits info. There appears to be an error in it.";
            public const string InvalidChallengeCode = "Check your code. The one entered isn't valid.";
            public const string ChallengeCodeExpired = "Request a new code. This one expired.";
            public const string Generic = "Try that again. Something happened on our end. Waiting a bit can help.";
        }

        public static class AlipayErrorTargets
        {
            public const string PhoneNumber = "phone";
            public const string Account = "alipayAccount";
            public const string Sms = "pin";
            public const string LastFiveCertNo = "lastFiveCertNo";
        }

        public static class UpiErrorCodes
        {
            public const string InvalidUpiAccount = "AccountNotFound";
        }

        public static class UpiErrorMessages
        {
            public const string InvalidUpiAccount = "UPI Id verification failed.";
            public const string Generic = "Try that again. Something happened on our end. Waiting a bit can help.";
        }

        public static class UpiErrorTargets
        {
            public const string Account = "vpa";
        }

        public static class NonSimErrorCodes
        {
            public const string RejectedByProvider = "RejectedByProvider";
            public const string MOAccountNotFound = "MOAccountNotFound";
            public const string InvalidChallengeCode = "InvalidChallengeCode";
            public const string RiskRejected = "Rejected";
            public const string PaymentInstrumentAddAlready = "PaymentInstrumentAddAlready";
        }

        public static class NonSimErrorMessages
        {
            public const string RejectedByProvider = "Check your phone number. The mobile operator you selected says it's not valid.";
            public const string MOAccountNotFound = "Check your phone number. The mobile operator you selected can't find that number.";
            public const string InvalidChallengeCode = "Check and re-enter the code. The code you entered is not valid.";
            public const string Generic = "Try that again. Something happened on our end. Waiting a bit can help.";
            public const string PaymentInstrumentAddAlready = "The instrument duplicated with an existing instrument for same account.";
        }

        public static class NonSimErrorTargets
        {
            public const string PhoneNumber = "msisdn";
            public const string Sms = "pin";
        }

        public static class PSD2ErrorCodes
        {
            public const string RejectedByProvider = "RejectedByProvider";
            public const string ValidatePIOnAttachFailed = "ValidatePIOnAttachFailed";
            public const string InvalidPaymentSession = "InvalidPaymentSession";
            public const string InvalidSuccessRedirectionUrl = "InvalidSuccessRedirectionUrl";
            public const string InvalidFailureRedirectionUrl = "InvalidFailureRedirectionUrl";
        }

        public static class ThreeDSErrorCodes
        {
            public const string ThreeDSOneResumeAddPiFailed = "ThreeDSOneResumeAddPiFailed";
            public const string InternalServerError = "InternalServerError";
        }

        public static class PSD2UserDisplayMessages
        {
            public const string ValidatePIOnAttachFailed = "Your bank could not authorize this payment method. Contact them for more info.";
        }

        public static class PayPalBillingAgreementTypes
        {
            public const string MerchantInitiatedBilling = "MerchantInitiatedBilling";
            public const string MerchantInitiatedBillingSingleAgreement = "MerchantInitiatedBillingSingleAgreement";
            public const string ChannelInitiatedBilling = "ChannelInitiatedBilling";
            public const string Unknown = "Unknown";
        }

        public static class PayPalErrorCodes
        {
            public const string IncorrectCredential = "IncorrectCredential";
            public const string AccountNotFound = "AccountNotFound";
        }

        public static class PayPalErrorMessages
        {
            public const string IncorrectCredential = "Check your PayPal sign-in info for errors.";
            public const string Generic = "Try that again. Something happened on our end. Waiting a bit can help.";
        }

        public static class PayPalErrorTargets
        {
            // [SuppressMessage("Microsoft.Security", "CS002:SecretInNextLine", Justification="This is not a secret")]
            public const string Password = "encryptedPassword"; // lgtm[cs/password-hardcoded]
            public const string Email = "email";
        }

        public static class ValidateCvvErrorCodes
        {
            public const string InvalidCvv = "InvalidCvv";
        }

        public static class ValidateCvvErrorMessages
        {
            public const string InvalidCvv = "Check your security code. There appears to be an error in it.";
        }

        public static class ValidateCvvErrorTargets
        {
            public const string Cvv = "cvvToken";
        }

        public static class MissingErrorMessage
        {
            public const string MissingValue = "{0} is missing";
        }

        public static class CaptchaErrors
        {
            public const string InvalidCaptcha = "InvalidCaptcha";
            public const string CaptchaRequired = "Captcha is a required field";
            public const string FirstTimeCaptchaRequiredMessage = "For security purposes, captcha verification is required.";
            public const string InvalidCaptchaMessage = "Invalid captcha";
        }

        public static class ChallengeEvidenceTypes
        {
            public const string Captcha = "CAPTCHA";
            public const string Challenge = "CHALLENGE";
        }

        public static class ChallengeEvidenceResults
        {
            public const string Success = "success";
        }

        public static class ClientActionContract
        {
            public const string NoMessage = "[]";
        }

        public static class TaxIdTypes
        {
            public const string Consumer = "consumer_tax_id";
            public const string Commercial = "commercial_tax_id";
        }

        public static class AddressTypes
        {
            public const string Shipping = "shipping";
            public const string ShippingV3 = "shipping_v3";
            public const string Billing = "billing";
            public const string BillingForm = "billing.form";
            public const string BillingSummary = "billing.summary";
            public const string HapiServiceUsageAddress = "hapiServiceUsageAddress";
            public const string Organization = "organization";
            public const string Individual = "individual";
            public const string HapiV1SoldToOrganization = "hapiV1SoldToOrganization";
            public const string HapiV1ShipToOrganization = "hapiV1ShipToOrganization";
            public const string HapiV1BillToOrganization = "hapiV1BillToOrganization";
            public const string HapiV1SoldToIndividual = "hapiV1SoldToIndividual";
            public const string HapiV1 = "hapiV1";
            public const string HapiV1ShipToIndividual = "hapiV1ShipToIndividual";
            public const string HapiV1BillToIndividual = "hapiV1BillToIndividual";
            public const string HapiV1SoldToOrganizationCSP = "hapiV1SoldToOrganizationCSP";
            public const string OrgAddress = "orgAddress";
            public const string PXV3 = "px_v3";
            public const string PXV3Shipping = "px_v3_shipping";
            public const string PXV3Billing = "px_v3_billing";
            public const string Internal = "internal";
        }

        public static class AuthenticationTypes
        {
            public const string Aad = "aad";
        }

        public static class UserTypes
        {
            public const string UserMe = "me";
            public const string UserMyOrg = "my-org";
        }

        public static class QueryParameterName
        {
            public const string Country = "country";
            public const string Partner = "partner";
            public const string Language = "language";
            public const string IncludeDuplicates = "includeDuplicates";
            public const string BillableAccountId = "billableAccountId";
            public const string IncludeOneTimeChargeInstrument = "includeOneTimeChargeInstrument";
            public const string ShowChallenge = "showChallenge";
            public const string ChallengeSource = "challengeSource";
            public const string SecondScreen = "secondScreen";
            public const string OCID = "ocid";
            public const string PXChallengeSessionId = "pxChallengeSessionId";
            public const string Component = "Component";
            public const string Currency = "currency";
            public const string Family = "family";
            public const string Type = "type";
            public const string Scenario = "scenario";
        }

        public static class CountryCodes
        {
            public const string Australia = "au";
            public const string Brazil = "br";
            public const string Canada = "ca";
            public const string India = "in";
            public const string Ireland = "ie";
            public const string Portugal = "pt";
            public const string UnitedStates = "us";
            public const string HongKong = "hk";
        }

        public static class Currencies
        {
            public const string USD = "USD";
        }

        public static class Operations
        {
            public const string Select = "select";
            public const string SelectInstance = "selectInstance";
            public const string SelectSingleInstance = "selectSingleInstance";
            public const string Add = "add";
            public const string AddPartial = "add_partial";
            public const string AddAdditional = "add_additional";
            public const string Apply = "apply";
            public const string Delete = "delete";
            public const string Update = "update";
            public const string UpdatePatch = "update_patch";
            public const string UpdatePartial = "update_partial";
            public const string ValidateInstance = "validateInstance";
            public const string Show = "show";
            public const string Search = "search";
            public const string SearchTransactions = "searchTransactions";
            public const string Replace = "replace";
            public const string FundStoredValue = "fundStoredValue";
            public const string Default = "default";
            public const string Offer = "offer";
            public const string RenderPidlPage = "RenderPidlPage";
            public const string AddSecondScreen = "addSecondScreen";
            public const string ExpressCheckout = "expressCheckout";
            public const string Confirm = "confirm";
            public const string Initialize = "initialize";
            public const string Get = "get";
        }

        public static class Component
        {
            public const string HandlePaymentChallenge = "handlePaymentChallenge";
            public const string HandlePurchaseRiskChallenge = "handlePurchaseRiskChallenge";
            public const string ConfirmPayment = "confirmPayment";
            public const string Initialize = "initialize";
            public const string QuickPayment = "quickPayment";
            public const string ExpressCheckout = "ExpressCheckout";
            public const string OrderSummary = "ordersummary";
            public const string Payment = "payment";
            public const string Address = "address";
            public const string Profile = "profile";
            public const string Confirm = "confirm";
            public const string Challenge = "challenge";

            public const string OrderSummaryProps = "orderSummaryProps";
            public const string AddressProps = "addressProps";
            public const string ProfileProps = "profileProps";
            public const string ConfirmProps = "confirmProps";
            public const string QuickPaymentProps = "quickPaymentProps";
            public const string PaymentProps = "paymentProps";
        }

        public static class ProfileType
        {
            public const string OrganizationProfile = "organization";
            public const string LegalEntityProfile = "legalentity";
            public const string Consumer = "consumer";
            public const string ConsumerV3 = "consumerV3";
            public const string Checkout = "checkout";
        }

        public static class Profile
        {
            public const string DefaultAddress = "default_address";
            public const string Culture = "culture";
            public const string Language = "language";
            public const string ProfileEmployeeCulture = "profile_employee_culture";
            public const string ProfileEmployeeLanguage = "profile_employee_language";
        }

        public static class DisplayHintIds
        {
            public const string AddressSuggestionTradePage = "addressSuggestionTradePage";
            public const string AddressChangeTradeAVSGroup = "addressChangeTradeAVSGroup";
            public const string AddressUseEnteredGroup = "addressUseEnteredGroup";
            public const string AddressUseCloseGroup = "addressUseCloseGroup";
            public const string PidlContainer = "pidlContainer";
            public const string AddressPhoneNumber = "addressPhoneNumber";
            public const string AddressIsWithinCityLimits = "addressIsWithinCityLimits";
            public const string SaveButton = "saveButton";
            public const string SubmitButton = "submitButton";
            public const string CancelNextGroup = "cancelNextGroup";
            public const string ConfirmButton = "confirmButton";
            public const string SaveNextButton = "saveNextButton";
            public const string SaveConfirmButton = "saveConfirmButton";
            public const string CancelButton = "cancelButton";
            public const string ContinueSubmitButton = "continueSubmitButton";
            public const string SaveButtonSuccess = "saveButtonSuccess";
            public const string SubmitButtonHidden = "submitButtonHidden";
            public const string ValidateButtonHidden = "validateButtonHidden";
            public const string AddressState = "addressState";
            public const string AddressFirstName = "addressFirstName";
            public const string AddressLastName = "addressLastName";
            public const string HapiTaxCountryProperty = "hapiTaxCountryProperty";
            public const string AddressChangeTradeAVSButton = "addressChangeTradeAVSButton";
            public const string AddressChangeTradeAVSV2Button = "addressChangeTradeAVSV2Button";
            public const string NextButton = "nextButton";
            public const string XboxNativeBaseErrorTopGroup = "xboxNativeBaseErrorTopGroup";
            public const string ViewTermsButton = "viewTermsButton";
            public const string NewPaymentMethodLink = "newPaymentMethodLink";
            public const string ChooseNewPaymentMethodLink = "chooseNewPaymentMethodLink";
            public const string NextModernValidateButton = "nextModernValidateButton";
            public const string XboxCoBrandedCardQrCodeRedirectButton = "xboxCoBrandedCardQrCodeRedirectButton";
            public const string XboxCoBrandedCardQrCodeShortUrlText = "xboxCoBrandedCardQrCodeShortUrlText";
            public const string CancelSaveGroup = "cancelSaveGroup";
            public const string PaymentInstrumentListPi = "paymentInstrumentListPi";
            public const string FirstName = "first_name";
            public const string LastName = "last_name";
            public const string CsvTotal = "csvTotal";
            public const string FormattedCsvTotal = "formattedCsvTotal";
            public const string PointsValueTotal = "pointsValueTotal";
            public const string FormattedPointsValueTotal = "formattedPointsValueTotal";
            public const string PointsRedemptionContentLineGroup = "pointsRedemptionContentLineGroup";
            public const string PointsRedemptionContentLine1 = "pointsRedemptionContentLine1";
            public const string PointsRedemptionContentLine = "pointsRedemptionContentLine";
            public const string UseCsvContentLine1 = "useCsvContentLine1";
            public const string UseCsvContentLine2 = "useCsvContentLine2";
            public const string RedeemMSRewardsPage = "redeemMSRewardsPage";
            public const string SelectMSRewardsPage = "selectMSRewardsPage";
            public const string MSRewardsNewCodeButton = "msRewardsNewCodeButton";
            public const string RedeemMSRewardsPhoneChallengeSendCodeButton = "redeemMSRewardsPhoneChallengeSendCodeButton";
            public const string PointsRedemptionGroup = "pointsRedemptionGroup";
            public const string UseCsvGroup = "useCsvGroup";
            public const string CsvAmountText = "csvAmountText";
            public const string PointsRedemptionContentGroup = "pointsRedemptionContentGroup";
            public const string GiftCardToken = "giftCardToken";
            public const string DummyText = "dummyText";
            public const string BackSaveGroup = "backSaveGroup";
            public const string LoadArkoseChallenge = "Load Arkose Challenge";
            public const string ProfilePrerequisitesPage = "profilePrerequisitesPage";
            public const string AddCCQrCodeImage = "addCCQrCodeImage";
            public const string WelcomeUserText = "welcomeUserText";
            public const string WelcomeUserEmail = "welcomeUserEmail";
            public const string ConsentMessage = "consentMessage";
            public const string AnonymousSaveButton = "anonymousSaveButton";
            public const string ChallengeCvvHeading = "challengecvvHeading";

            // Payment client - Order summary
            public const string CartTotal = "cartTotal";
            public const string CartSubtotal = "cartSubtotal";
            public const string CartTax = "cartTax";
            public const string CartTaxExpression = "cartTaxExpression";
            public const string ShowSummary = "showSummary";
            public const string PaymentMethod = "paymentMethod";
            public const string PaymentOptionText = "paymentOptionText";
            public const string PaymentMethodOption = "paymentMethodOption";
            public const string PIDLInstanceListPI = "pidlInstanceListPI";
            public const string LogoContainer = "logoContainer";
            public const string PaymentInstrument = "paymentInstrument";
            public const string AddressLine1 = "addressLine1";
            public const string AddressLine2 = "addressLine2";
            public const string AddressLine3 = "addressLine3";
            public const string AddressCity = "addressCity";
            public const string AddressCounty = "addressCounty";
            public const string AddressProvince = "addressProvince";
            public const string AddressCountry = "addressCountry";
            public const string AddressPostalCode = "addressPostalCode";
            public const string SelectedPIID = "selectedPIID";
            public const string PrivacyTextGroup = "microsoftPrivacyTextGroup";
            public const string SavePaymentDetails = "savePaymentDetails";
            public const string CVVGroup = "cvvGroup";
            public const string AcceptCardMessage = "accept_card_message";
            public const string PaymentOptionSaveText = "paymentOptionSaveText";
            public const string BillingAddressTitle = "billingAddressTitle";
            public const string OrderItemImage = "orderItemImage";
            public const string CartHeading = "cartHeading";
            public const string GooglepayExpressCheckoutFrame = "googlepayExpressCheckoutFrame";
            public const string ApplepayExpressCheckoutFrame = "applepayExpressCheckoutFrame";
            public const string StarRequiredTextGroup = "starRequiredTextGroup";

            public const string ExpiryGroup = "expiryGroup";
            public const string ExpiryMonth = "expiryMonth";
            public const string ExpiryYear = "expiryYear";
            public const string ExpiryCVVGroup = "expiryCVVGroup";

            public const string DeleteText = "deleteText";
        }

        public static class ReturnContextClientActionTypes
        {
            public const string Refresh = "refresh";
        }

        public static class ResourceTypes
        {
            public const string Address = "address";
        }

        public static class PropertyDescriptionIds
        {
            public const string UseCsv = "useCsv";
            public const string UseRedeemPoints = "useRedeemPoints";
            public const string CatalogItem = "catalogItem";
            public const string CatalogItemAmount = "catalogItemAmount";
            public const string ChallengeToken = "challengeToken";
            public const string ChallengePreference = "challengePreference";
            public const string PhoneNumber = "phoneNumber";
            public const string PointsValueCurrency = "pointsValueCurrency";
            public const string CsvCurrency = "csvCurrency";
            public const string CsvPiid = "csvPiid";
            public const string CsvBalance = "csvBalance";
            public const string RedeemTokenIdentifierValue = "tokenIdentifierValue";
            public const string RedeemTokenActionType = "actionType";
            public const string ValidationMode = "validation_mode";
            public const string SecondaryValidationMode = "secondary_validation_mode";

            // Payment client - Order summary
            public const string CartTax = "cart_tax";
            public const string CartTotal = "cart_total";
            public const string CartSubtotal = "cart_subtotal";
            public const string ShowSummary = "show_summary";

            // Payment client - Address
            public const string Address = "address";
            public const string AddressType = "addressType";
            public const string AddressResourceId = "addressResource_id";
            public const string AddressCountry = "addressCountry";
            public const string AddressOperation = "addressOperation";
            public const string AddressLine1 = "address_line1";
            public const string AddressLine2 = "address_line2";
            public const string AddressLine3 = "address_line3";
            public const string City = "city";
            public const string Region = "region";
            public const string Country = "country";
            public const string PostalCode = "postal_code";
            public const string SelectedPIID = "selected_PIID";

            // Payment client - Profile
            public const string EmailAddress = "email_address";

            public const string Id = "id";
            public const string Details = "details";
            public const string AttachmentType = "AttachmentType";
            public const string PiId = "piid";

            public const string PaymentMethodFamily = "paymentMethodFamily";
            public const string PaymentMethodType = "paymentMethodType";
            public const string SavePaymentDetails = "savePaymentDetails";
            public const string UpdateAddressEnabled = "update_address_enabled";
            public const string ExpressCheckoutPaymentData = "expressCheckoutPaymentData";

            public const string AccountToken = "accountToken";
            public const string CVVToken = "cvvToken";

            public const string PaymentInstrumentId = "paymentInstrumentId";
        }

        public static class SubmitUrls
        {
            public const string RedeemMSRewards = "https://{pifd-endpoint}/users/{userId}/msRewards";
            public const string RedeemCSVToken = "https://{pifd-endpoint}/users/{userId}/paymentInstrumentsEx";
            public const string PayMicrosoftMobilePageSubmit = "https://{{pifd-endpoint}}/paymentInstrumentsEx/create?country={0}&language={1}&partner={2}&sessionId={3}&scenario={4}";
            public const string CheckoutRequestsExConfirm = "https://{{pifd-endpoint}}/PaymentClient/CheckoutRequestsEx/{0}/confirm?partner={1}";
            public const string CheckoutRequestsExAttachAddressMergeData = "https://{{pifd-endpoint}}/PaymentClient/CheckoutRequestsEx/{0}/attachAddress?type={1}&scenario={2}";
            public const string CheckoutRequestsExAttachAddress = "https://{{pifd-endpoint}}/PaymentClient/CheckoutRequestsEx/{0}/attachAddress?type={1}";
            public const string CheckoutRequestsExAttachProfile = "https://{{pifd-endpoint}}/PaymentClient/CheckoutRequestsEx/{0}/attachProfile";
            public const string PaymentComponentPIEx = "https://{{pifd-endpoint}}/users/{{userId}}/PaymentInstrumentsEx?country={0}&language={1}&partner={2}";
            public const string PaymentRequestsAttachChallengeData = "https://{{pifd-endpoint}}/PaymentClient/paymentRequestsEx/{0}/attachChallengeData";
            public const string ExpressCheckoutConfirm = "https://{{pifd-endpoint}}/users/{{userId}}/expressCheckout/confirm?partner={0}&country={1}&language={2}";
            public const string PaymentRequestsRemoveEligiblePaymentmethods = "https://{{pifd-endpoint}}/PaymentClient/paymentRequestsEx/{0}/removeEligiblePaymentmethods";
        }

        public static class StringPlaceholders
        {
            public const string ShortUrlPlaceholder = "{shortUrlPlaceholder}";
            public const string PIPlaceholder = "{PIPlaceholder}";
        }

        public static class ButtonDisplayHintIds
        {
            public const string SaveButton = "saveButton";
            public const string NextButton = "nextButton";
            public const string ConfirmationButton = "confirmationButton";
            public const string BackButton = "backButton";
            public const string CancelButton = "cancelButton";
            public const string CancelBackButton = "cancelBackButton";
            public const string AddressPreviousButton = "addressPreviousButton";
            public const string ViewTermsButton = "viewTermsButton";
            public const string SubmitButtonHidden = "submitButtonHidden";
            public const string YesButton = "yesButton";
        }

        public static class SessionTokenClaimTypes
        {
            public const string MerchantId = "mid";
            public const string ShopperId = "sid";
        }

        public static class CSPStepNames
        {
            public const string Fingerprint = "cspStepFingerprint";
            public const string Challenge = "cspStepChallenge";
            public const string None = "cspNone";
        }

        public static class SessionFieldNames
        {
            public const string CSPStep = "cspStep";
            public const string ThreeDSSessionData = "threeDSSessionData";
            public const string ThreeDSMethodData = "threeDSMethodData";
            public const string CReq = "creq";
            public const string CRes = "cres";
        }

        public static class DataDescriptionPropertyNames
        {
            public const string AddressCountry = "addressCountry";
            public const string AddressType = "addressType";
            public const string DataCountry = "dataCountry";
            public const string DataOperation = "dataOperation";
            public const string DataType = "dataType";
            public const string Identity = "identity";
            public const string ResourceActionContext = "currentContext";
            public const string Country = "country";
            public const string CaptchaID = "captchaId";
            public const string CaptchaRegion = "captchaReg";
            public const string AudioCaptchaID = "audioCaptchaId";
            public const string AudioCaptchaRegion = "audioCaptchaReg";
            public const string CaptchaSolution = "captchaSolution";
            public const string Captcha = "captcha";
            public const string CaptchaType = "captchaType";
            public const string ChallengeEvidence = "challengeEvidence";
            public const string IssuerId = "issuerId";
            public const string SavePaymentDetails = "savePaymentDetails";
        }

        public static class CaptchaTypes
        {
            public const string Audio = "audio";
            public const string Visual = "image";
        }

        public static class CaptchaLabels
        {
            public const string UseAudio = "Use audio captcha";
            public const string AudioText = "Type all of the words you hear, separated by a space. Characters are not case-sensitive.";
            public const string UseImage = "Use image captcha";
            public const string ImageText = "Type the characters above. Characters are not case-sensitive.";
            public const string CaptchaVerification = "Captcha verification";
        }

        public static class HapiTaxIdDataDescriptionPropertyNames
        {
            public const string State = "state";
        }

        public static class UpdatePropertyValueClientActionPropertyNames
        {
            public const string Id = "id";
        }

        public static class ChallengeDescriptionTypes
        {
            public const string PaypalQrCodeXboxNative = "paypalQrCodeXboxNative";
            public const string PaypalQrCode = "paypalQrCode";
            public const string KakaopayQrCode = "kakaopayQrCode";
            public const string GenericQrCode = "genericQrCode";
            public const string AlipayQrCode = "alipayQrCode";
            public const string ThreeDSOneQrCode = "ThreeDSOneQrCode";
            public const string Cvv = "cvv";
            public const string XboxCoBrandedCard = "xboxCoBrandedCard";
            public const string VenmoQrCode = "venmoQrCode";
            public const string CreditCardQrCode = "creditCardQrCode";
        }

        public static class HapiV1ModernAccountAddressDataDescriptionPropertyNames
        {
            public const string AddressLine1 = "addressLine1";
            public const string AddressLine2 = "addressLine2";
            public const string AddressLine3 = "addressLine3";
            public const string City = "city";
            public const string Region = "region";
            public const string Country = "country";
            public const string PostalCode = "postalCode";
        }

        public static class LegacyAVSPropertyNames
        {
            public const string AddressLine1 = "address_line1";
            public const string AddressLine2 = "address_line2";
            public const string AddressLine3 = "address_line3";
            public const string City = "city";
            public const string Region = "region";
            public const string Country = "country";
            public const string PostalCode = "postal_code";
        }

        public static class HapiServiceUsageAddressPropertyNames
        {
            public const string AddressLine1 = "line1";
            public const string AddressLine2 = "line2";
            public const string AddressLine3 = "line3";
            public const string City = "city";
            public const string Region = "state";
            public const string Country = "countryCode";
            public const string PostalCode = "postalCode";
        }

        public static class SyncToLegacyCodes
        {
            public const int ValidationUsingAVS = 0;  // validate address in using legacy avs
            public const int CreateLegacyAccountAndSync = 1; // create legacy account if user doesn't have and sync address to legacy
            public const int NoLegacyAccountCreationAndSync = 2; // not create legacy account if user doesn't have, otherwise sync address to legacy
            public const int NoSyncAndValidation = 3; // Do not sync to legacy and do not validate address in legacy
        }

        public static class AddressNoCityStatePropertyNames
        {
            public const string City = "city";
            public const string Region = "region";
        }

        public static class ChallengeTypes
        {
            public const string ValidatePIOnAttachChallenge = "ValidatePIOnAttachChallenge";
            public const string PSD2Challenge = "PSD2Challenge";
            public const string India3DSChallenge = "India3DSChallenge";
            public const string LegacyBillDeskPaymentChallenge = "LegacyBillDeskPaymentChallenge";
            public const string UPIChallenge = "UPIChallenge";
            public const string CreditCardQrCode = "creditCardQrCode";
        }

        public static class ValidateResultMessages
        {
            public const string Failed = "Failed";
        }

        public static class PaymentMethodCardProductTypes
        {
            public const string XboxCreditCard = "XboxCreditCard";
        }

        public static class Prefixes
        {
            public const string PaymentInstrumentSessionPrefix = "PX-3DS2-";
        }

        public static class ShortURLServiceTimeToLive
        {
            public const int ShortURLActiveTTL = 20;
        }

        public static class AddPIQrCode
        {
            public const int QrCodeSessionTimeoutInMinutes = 10;
            public const int AnonymousSecondScreenFormRenderedTimeoutInMinutes = 15;
            public const int MaxSessionIdRetry = 5;
            public const string AccountId = "accountId";
            public const string Email = "email";
            public const string AllowTestHeader = "allowTestHeader";
        }

        public static class CheckoutRequestPropertyName
        {
            public const string Email = "email_address";
            public const string PIID = "selected_PIID";
            public const string ExpressCheckoutPaymentData = "expressCheckoutPaymentData";
            public const string Country = "confirmCountry";
            public const string PaymentMethodType = "paymentMethodType";
        }

        public static class ExpressCheckoutPropertyValue
        {
            public const string Country = "checkoutCountry";
            public const string ExpressCheckoutPaymentData = "expressCheckoutPaymentData";
            public const string PaymentMethodType = "paymentMethodType";
        }

        public static class TestAccountHeaders
        {
            public const string MDollarPurchase = "mdollarpurchase";
        }

        public static class PidlSdkVersionNumber
        {
            public const int PidlSdkMajor1 = 1;
            public const int PidlSdkMajor2 = 2;
            public const int PidlSdkMinor4 = 4;
            public const int PidlSdkMinor5 = 5;
            public const int PidlSdkMinor6 = 6;
            public const int PidlSdkMinor7 = 7;
            public const int PidlSdkMinor22 = 22;
            public const int PidlSdkBuild0 = 0;
            public const int PidlSdkBuild1 = 1;
            public const int PidlSdkBuild3 = 3;
            public const int PidlSdkBuild7 = 7;
            public const int PidlSdkAlpha0 = 0;
            public const int PidlSdkAlpha144 = 144;
        }

        public static class CharacterCount
        {
            public const int MaxBrowserLanguageInput = 8;
        }

        public static class PurchaseOrderState
        {
            public const string Editing = "Editing";
        }

        public static class RootCertVersion
        {
            public const string UpdatedVersion17 = "V17";
            public const int UpdatedVersionInt17 = 17;
        }

        public static class PSD2ValidationErrorMessage
        {
            public const string SignatureVerificationFailed = "Signature verification for the PSD2 certificate failed. ";
            public const string PaymentMethodTypeIssue = "PaymentMethodType and dsInfo mismatch.";
        }

        public static class ServiceNames
        {
            // We are overloading serviceName to track dsCertificateValidation failures
            public const string DsCertificateValidation = "dsCertificateValidation";
            public const string PayerAuth = "payerAuth";
        }

        public static class PSD2NativeChallengeStrings
        {
            public const string Header = "Secure Checkout";
            public const string BackButtonLabel = "Back";
            public const string BackButtonAccessibilityLabel = "Press to go back";
            public const string CancelButtonLabel = "Cancel";
            public const string CancelButtonAccessibilityLabel = "Press to cancel";
            public const string OrderingAccessibilityLabel = "of";
            public const string BankLogoAccessibilityLabel = "Bank Logo";
            public const string CardLogoAccessibilityLabel = "Card Logo";
        }

        public static class StaticDescriptionTypes
        {
            public const string DeletionSubscriptionErrorPidl = "deletionSubscriptionErrorPidl";
            public const string LegacyBillDesk3DSRedirectAndStatusCheckPidl = "legacyBillDesk3DSRedirectAndStatusCheckPidl";
            public const string LegacyBillDesk3DSStatusCheckPidl = "legacyBillDesk3DSStatusCheckPidl";
            public const string Cc3DSStatusCheckPidl = "cc3DSStatusCheckPidl";
            public const string XboxNativeBaseErrorPidl = "xboxNativeBaseErrorPidl";
            public const string XboxCardNotEligibleErrorStaticPidl = "xboxCardNotEligibleErrorPidl";
            public const string XboxCardPendingErrorStaticPidl = "xboxCardPendingErrorPidl";
            public const string XboxCardApprovedErrorStaticPidl = "xboxCardApprovedErrorPidl";
            public const string XboxCardInternalErrorStaticPidl = "xboxCardInternalErrorPidl";
            public const string XboxCoBrandedCardQRCodePidl = "xboxCoBrandedCardQRCodePidl";
            public const string XboxCardSuccessPidl = "xboxCardSuccessPidl";
            public const string XboxCardApplicationPendingPidl = "xboxCardApplicationPendingPidl";
            public const string XboxCardApplicationErrorPidl = "xboxCardApplicationErrorPidl";
            public const string XboxCardUpsellBuyNowPidl = "xboxCardUpsellBuyNowPidl";
        }

        public static class DeletionErrorCodes
        {
            public const string SubscriptionNotCanceled = "SubscriptionNotCanceled";
            public const string OutstandingBalance = "OutstandingBalance";
            public const string RemovePIAccessDeniedForTheCaller = "RemovePIAccessDeniedForTheCaller";
        }

        public static class DeletionErrorMessages
        {
            public const string SubscriptionNotCanceledMessage = "You have subscriptions/orders that use this payment method. Please update the subscriptions/orders to use a different payment method before removing this one.";
            public const string OutstandingBalanceMessage = "The payment instrument has an outstanding balance that is not paid.";
            public const string RemoveBusinessInstrumentNotSupportedMessage = "You have an Azure subscription associated with this payment method. Please update the subscription in the Azure portal.";
            public const string ManageMessage = "Please go to account.microsoft.com to manage.";
        }

        public static class ThirdPartyPaymentsErrorCodes
        {
            public const string ResourceNotFound = "ResourceNotFound";
            public const string ServiceError = "ServiceError";
            public const string CvvValueMismatch = "CvvValueMismatch";
            public const string ExpiredPaymentInstrument = "ExpiredPaymentInstrument";
            public const string InvalidPaymentInstrument = "InvalidPaymentInstrument";
            public const string RequestDeclined = "RequestDeclined";
            public const string RequestFailed = "RequestFailed";
            public const string InvalidPaymentInstrumentId = "InvalidPaymentInstrumentId";
            public const string PaymentInstrumentNotActive = "PaymentInstrumentNotActive";
            public const string InvalidRequestData = "InvalidRequestData";
            public const string MerchantSelectionFailure = "MerchantSelectionFailure";
            public const string RetryLimitExceeded = "RetryLimitExceeded";
            public const string ProcessorDeclined = "ProcessorDeclined";
            public const string ProcessorRiskCheckDeclined = "ProcessorRiskCheckDeclined";
            public const string AmountLimitExceeded = "AmountLimitExceeded";
            public const string InsufficientFund = "InsufficientFund";
            public const string MissingFundingSource = "MissingFundingSource";
            public const string TransactionNotAllowed = "TransactionNotAllowed";
            public const string InvalidTransactionData = "InvalidTransactionData";
            public const string AuthenticationRequired = "AuthenticationRequired";
            public const string InvalidHeader = "InvalidHeader";
        }

        public static class ThirdPartyPaymentsErrorMessages
        {
            // TO DO: update the messages
            public const string ResourceNotFound = "The resource with given identity is not found";
            public const string ServiceError = "Internal service error, usually means coordinator error";
            public const string CvvValueMismatch = "The cvv didn't match the one on file";
            public const string ExpiredPaymentInstrument = "The payment instrument has expired";
            public const string InvalidPaymentInstrument = "An invalid payment instrument account number. Verify the information or use another payment instrument";
            public const string RequestDeclined = "The transaction request is declined, maybe by Risk or Provider";
            public const string RequestFailed = "The transaction request is failed, due to payment internal issue";
            public const string InvalidPaymentInstrumentId = "The transaction request is failed, due to an invalid payment instrument Id";
            public const string PaymentInstrumentNotActive = "The transaction request is failed, because the payment instrument is not active";
            public const string InvalidRequestData = "There are some invalid data items in request object";
            public const string MerchantSelectionFailure = "Can't select a merchant basing on the currency/country/market etc.";
            public const string RetryLimitExceeded = "Retry count is greater than the permitted value";
            public const string ProcessorDeclined = "Declined by processor for no clear explanation";
            public const string ProcessorRiskCheckDeclined = "Processor decided that the transaction could be fraudulent and rejected it";
            public const string AmountLimitExceeded = "The requested amount exceeded the allowed threshold set by the issuer or the provider";
            public const string InsufficientFund = "The payment instrument lacks enough fund in the account";
            public const string MissingFundingSource = "The payment instrument lacks a credible funding source";
            public const string TransactionNotAllowed = "The customer's payment instrument cannot be used for this kind of purchase";
            public const string InvalidTransactionData = "Generic error code for input parameter validation failure, detail message should contain the specific parameter type";
            public const string AuthenticationRequired = "The customer need to perform SCA to complete authentication for 3DS";
            public const string InvalidHeader = "One or more of the headers was incorrect";
            public const string TryAgainMsg = "Please try again.";
            public const string PaymentMethodErrorMsg = "Please use a different payment method.";
        }

        public static class ThirdPartyPaymentsErrorTargets
        {
            public const string CardNumber = "accountToken";
            public const string CardHolderName = "accountHolderName";
            public const string Cvv = "cvvToken";
            public const string ExpiryMonth = "expiryMonth";
            public const string ExpiryYear = "expiryYear";
            public const string AddressLine1 = "address_line1";
            public const string AddressLine2 = "address_line2";
            public const string City = "city";
            public const string State = "region";
            public const string ZipCode = "postal_code";
        }

        public static class PaymentOptionsAppErrorStrings
        {
            public const string Header = "An error has occurred";
            public const string Body = "Please try again later, or contact support if you have any questions.";
            public const string SupportLink = "https://support.microsoft.com/account-billing/troubleshoot-payment-option-issues-135ed6ea-dd2b-47b7-cbcf-24f9f39da738";
            public const string SupportLinkText = "Click here to learn more";
            public const string ButtonText = "Close";
        }

        public static class PaymentOptionsAppGenericErrorStringNames
        {
            public const string Header = "header";
            public const string Body = "body";
            public const string SupportLink = "supportLink";
            public const string SupportLinkText = "supportLinkText";
            public const string ButtonText = "buttonText";
        }

        public static class CaptchaHIPHeaders
        {
            public const string ChallengeID = "challenge-id";
            public const string AzureRegion = "azureregion";
        }

        public static class DisplayPropertyName
        {
            public const string LegacyBusiness = "LegacyBusiness";
        }

        public static class RedirectionPatterns
        {
            public const string FullPage = "fullPage";
            public const string Inline = "inline";
            public const string IFrame = "iFrame";
            public const string QRCode = "QRCode";
        }

        public static class RequestDomains
        {
            public const string Localhost = "localhost";
            public const string XboxCom = "www.xbox.com";
            public const string OriginPPEXboxCom = "origin-ppe.xbox.com";
        }

        public static class DestinationId
        {
            public const string ApplyOnConsole = "applyOnConsole";
        }

        public static class WalletServiceConstants
        {
            public const string ApplePay = "ApplePay";
            public const string GooglePay = "GooglePay";
            public const string IntegrationType = "DIRECT";
            public const string ApplePayPiidPrefix = "cw_apay";
            public const string GooglePayPiidPrefix = "cw_gpay";
        }

        public static class CharUnicodes
        {
            public const string SuperscriptOne = "\u00B9";
            public const string SuperscriptTwo = "\u00B2";
            public const string SuperscriptThree = "\u00B3";
            public const string SuperscriptOpenParenthesis = "\u207D";
            public const string SuperscriptCloseParenthesis = "\u207E";
        }

        public static class WalletConfigConstants
        {
            public const string DisplayName = "Microsoft";
            public const string Initiative = "Web";
            public const string InitiateContext = "mystore.example.com";
            public const string WalletConfig = "walletConfig";
            public const string PayLabel = "amount due plus applicable taxes";
            public const string TaxIncludedPayLabel = "amount due";
        }

        public static class MSRewardsErrorMessages
        {
            public const string InsufficientBalance = "Insufficient balance";
        }

        public static class ApplyErrrorCodes
        {
            public const string BadSessionState = "BadSessionState";
        }

        public static class ChallengeManagementServiceErrorCodes
        {
            public const string Conflict = "Conflict";
        }

        public static class ChallengeManagementDisplayLabels
        {
            public const string BackButtonLabel = "Back";
            public const string NextButtonLabel = "Next";
        }

        public static class WalletValidationStatusConstants
        {
            public const string Approved = "Approved";
        }

        public static class RiskErrorCode
        {
            public const string PIEligibilityCheckRejectedbyRisk = "PIEligibilityCheckRejectedbyRisk";
        }

        public static class RiskErrorMessages
        {
            public const string PIEligibilityCheckRejectedbyRisk = "The request to use the payment method has been rejected by risk eligibility check.";
        }

        public static class WalletDeviceSupportedDebugMessages
        {
            public const string IsCrossOrigin = "isCrossOrigin";
            public const string ExcludedByFlight = "excludedByFlight";
        }

        public static class WalletBrowserValues
        {
            public const string Safari = "safari";
            public const string Chrome = "chrome";
            public const string Edge = "edge";
        }

        public static class PropertyEventType
        {
            public const string ValidateOnChange = "validateOnChange";
            public const string MergeData = "mergeData";
        }

        public static class PropertySelectType
        {
            public const string Checkbox = "checkbox";
        }

        public static class RequestContextType
        {
            public const string Payment = "pr";
            public const string Checkout = "cr";
            public const string Wallet = "wr";
        }

        public static class PropertyOrientation
        {
            public const string Inline = "inline";
        }

        public static class ExpressCheckoutButtonPayloadKey
        {
            public const string Amount = "amount";
            public const string ActionType = "actionType";
            public const string Options = "options";
            public const string RecurringPaymentDetails = "recurringPaymentDetails";
            public const string TopDomainUrl = "topDomainUrl";
        }

        public static class PIDLResourceDescriptionId
        {
            public const string ExpressCheckout = "expressCheckout";
            public const string QuickPayment = "quickPayment";
        }

        public static class PropertyDisplayExample
        {
            public const string AccountToken = "0000 0000 0000 0000";
            public const string CVVToken = "000";
        }

        public static class PropertyDisplayName
        {
            public const string ShowDisplayNameTrue = "true";
            public const string ShowDisplayNameFalse = "false";
            public const string ExpirationDate = "Expiration date";
        }

        public static class DisplayTag
        {
            public const string CustomDropdown = "custom-dropdown";
        }

        public static class UnlocalizedDisplayText
        {
            public const string DeletePIDisplayTextLastFourDigits = "card ending in {0}";
        }

        public static class EnvironmentEndpoint
        {
            public const string INT = "pmservices.cp.microsoft-int.com";
            public const string PROD = "pmservices.cp.microsoft.com";
        }

        public static class AgenticPaymentRequestData
        {
            public const string TotalAuthenticationAmount = "totalAuthenticationAmount";
            public const string CurrencyCode = "currencyCode";
            public const string SessionContextJsonString = "sessionContextJsonString";
            public const string BrowserDataJsonString = "browserDataJsonString";
            public const string ApplicationUrl = "applicationUrl";
            public const string MerchantName = "merchantName";
            public const string ChallengeMethodId = "challengeMethodId";
            public const string Pin = "pin";
            public const string PaymentMethodType = "paymentMethodType";
            public const string FIDOResponse = "FIDOResponse";
            public const string DfpSessionID = "dfpSessionID";
            public const string AccountHolderEmail = "accountHolderEmail";
            public const string MandateJsonString = "mandateJsonString";
            public const string PaymentInstrumentId = "paymentInstrumentId";
        }
    }
}