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
        internal const string PimsVersion = "v4.0";

        internal const string JarvisAccountIdHmacProperty = "pxmac";

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

        private static HashSet<string> countriesToCollectTaxIdUnderFlighting = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
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

        internal enum PaymentMethodFamily
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

        internal enum EwalletType
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

        internal static List<string> AvsSuggestEnabledPartners
        {
            get
            {
                return avsSuggestEnabledPartners;
            }
        }

        internal static List<string> HiddenLinkedProfilePIDLInAddCCPartners
        {
            get
            {
                return hiddenLinkedProfilePIDLInAddCCPartners;
            }
        }

        internal static List<string> LuhnValidationEnabledPartners
        {
            get
            {
                return luhnValidationEnabledPartners;
            }
        }

        internal static HashSet<string> JarvisAccountIdHmacPartners
        {
            get
            {
                return jarvisAccountIdHmacPartners;
            }
        }

        internal static List<string> ModalGroupIds
        {
            get
            {
                return modalGroupIds;
            }
        }

        internal static List<string> CountriesNeedsToShowAVSSuggestionsForAmcWeb
        {
            get
            {
                return countriesNeedsToShowAVSSuggestionsForAmcWeb;
            }
        }

        internal static HashSet<string> PartnersToEnablePaypalRedirectOnTryAgain
        {
            get
            {
                return partnersToEnablePaypalRedirectOnTryAgain;
            }
        }

        internal static List<string> TabbableDisplayHintTypes
        {
            get
            {
                return new List<string> { "button", "property", "hyperlink" };
            }
        }

        internal static List<string> AddressFieldsWithDefaultValueNotNeededForUpdateAndReplace
        {
            get
            {
                return new List<string> { "address_line1", "address_line2", "address_line3", "city", "region", "postal_code", "country" };
            }
        }

        internal static List<string> KoreaLocalCardTypes
        {
            get
            {
                return new List<string> { "shinhan", "bc", "kb_kook_min", "samsung", "hyundai", "lotte", "nh", "hana", "citi", "jeju", "woori", "suhyup", "jeonbok", "kwangju", "shinhyup" };
            }
        }

        internal static List<string> BrazilLocalCardTypes
        {
            get
            {
                return new List<string> { "hipercard", "elo" };
            }
        }

        internal static List<string> NigeriaLocalCardTypes
        {
            get
            {
                return new List<string> { "verve" };
            }
        }

        internal static Dictionary<string, string> XboxCardApplyCountryToLanguage
        {
            get
            {
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "us", "en-US" },
                };
            }
        }

        internal static Dictionary<string, Tuple<string, string>> LogosWithAlignedAlternative
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

        internal static Dictionary<string, List<string>> PIFamilyTypeEnableAVSAdditionalFlags
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

        internal static List<string> InlinePartners
        {
            get
            {
                return inlinePartners;
            }
        }

        internal static HashSet<string> PartnersToEnablePaypalSecondScreenForXbox
        {
            get
            {
                return partnersToEnablePaypalSecondScreenForXbox;
            }
        }

        internal static HashSet<string> PartnersToEnableFocusoutResolutionPolicy
        {
            get
            {
                return partnersToEnableFocusoutResolutionPolicy;
            }
        }

        internal static HashSet<string> PartnersToEnableRetryOnInvalidRequestData
        {
            get
            {
                return partnersToEnableRetryOnInvalidRequestData;
            }
        }

        internal static HashSet<string> CountriesToEnablePaypalSecondScreenForXbox
        {
            get
            {
                return countriesToEnablePaypalSecondScreenForXbox;
            }
        }

        internal static HashSet<string> CountriesToCollectTaxIdUnderFlighting
        {
            get
            {
                return countriesToCollectTaxIdUnderFlighting;
            }
        }

        internal static HashSet<string> PartnersToEnableReorderCCAndCardholder
        {
            get
            {
                return partnersToEnableReorderCCAndCardholder;
            }
        }

        internal static HashSet<string> ThirdPartyPaymentPartners
        {
            get
            {
                return thirdPartyPaymentPartners;
            }
        }

        internal static HashSet<string> PIAttachIncentivePartners
        {
            get
            {
                return paymentInstrumentAttachIncentivePartners;
            }
        }

        internal static List<string> PropertiesToMakeMandatory
        {
            get
            {
                return propertiesToMakeMandatory;
            }
        }

        internal static HashSet<string> ValidatePIOnAttachEnabledPartners
        {
            get
            {
                return validatePIOnAttachEnabledPartners;
            }
        }

        internal static HashSet<string> PSD2IgnorePIAuthorizationPartners
        {
            get
            {
                return psd2IgnorePIAuthorizationPartners;
            }
        }

        internal static HashSet<string> PXRateLimitAddCCSkipAccounts
        {
            get
            {
                return pxRateLimitAddCCSkipAccounts;
            }
        }

        internal static List<string> AllowedOneBoxINTBrowserAuthenticateThreeDSOneRedirectionUrlHostname
        {
            get
            {
                return allowedOneBoxINTBrowserAuthenticateThreeDSOneRedirectionUrlHostname;
            }
        }

        internal static List<string> AllowedPPEPRODBrowserAuthenticateThreeDSOneRedirectionUrlHostname
        {
            get
            {
                return allowedPPEPRODBrowserAuthenticateThreeDSOneRedirectionUrlHostname;
            }
        }

        internal static HashSet<string> ThirdPartyPaymentErrorMsgs
        {
            get
            {
                return thirdPartyPaymentErrorMsgs;
            }
        }

        internal static HashSet<string> ThirdPartyPaymentTerminalErrorsTypeOne
        {
            get
            {
                return thirdPartyPaymentTerminalErrorTypeOne;
            }
        }

        internal static HashSet<string> ThirdPartyPaymentTerminalErrorsTypeTwo
        {
            get
            {
                return thirdPartyPaymentTerminalErrorTypeTwo;
            }
        }

        // List of partners to enabled global PI in add resources.
        internal static List<string> PartnersEnabledWithGlobalPIInAddResource
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

        internal static class PaymentMethodFamilyName
        {
            internal const string Virtual = "virtual";
            internal const string OnlineBankTransfer = "online_bank_transfer";
            internal const string Ewallet = "ewallet";
            internal const string RealTimePayments = "real_time_payments";
        }

        internal static class DisplayCustomizationDetail
        {
            internal const string AddressSuggestion = "AddressSuggestion";
            internal const string SubmitActionType = "SubmitActionType";
            internal const string AddressEx = "addressEx";
        }

        internal static class PaymentMethodType
        {
            internal const string AlipayBillingAgreement = "alipay_billing_agreement";
            internal const string CreditCardVisa = "visa";
            internal const string CreditCardMasterCard = "mc";
            internal const string CreditCardAmericanExpress = "amex";
            internal const string CreditCardDiscover = "discover";
            internal const string UnionPay = "unionpay";
            internal const string UnionPayCreditCard = "unionpay_creditcard";
            internal const string UnionPayDebitCard = "unionpay_debitcard";
            internal const string IdealBillingAgreement = "ideal_billing_agreement";
            internal const string Kakaopay = "kakaopay";
            internal const string PayPal = "paypal";
            internal const string Ach = "ach";
            internal const string Sepa = "sepa";
            internal const string Klarna = "klarna";
            internal const string LegacyInvoice = "legacy_invoice";
            internal const string Check = "check";
            internal const string InvoiceBasicVirtual = "invoice_basic";
            internal const string InvoiceCheckVirtual = "invoice_check";
            internal const string AlipayVirtual = "alipay";
            internal const string UnionpayVirtual = "unionpay";
            internal const string UnionpayInternational = "unionpay_international";
            internal const string Paysafecard = "paysafecard";

            internal const string LegacyBilldeskPayment = "legacy_billdesk_payment";
            internal const string Venmo = "venmo";
            internal const string UPI = "upi";
            internal const string UPIQr = "upi_qr";
            internal const string UPICommercial = "upi_commercial";
            internal const string UPIQrCommercial = "upi_qr_commercial";
            internal const string GooglePay = "googlepay";
            internal const string ApplePay = "applepay";
            internal const string CreditCardRupay = "rupay";
            internal const string AddNewPM = "addnewpm";
            internal const string AddNewPMNoDefaultSelection = "addnewpm_no_default_selection";
            internal const string PayPay = "paypay";
            internal const string AlipayHK = "alipayhk";
            internal const string GCash = "gcash";
            internal const string TrueMoney = "truemoney";
            internal const string TouchNGo = "touchngo";
            internal const string AlipayCN = "alipaycn";
            internal const string StoredValue = "stored_value";
        }

        internal static class VirtualPIDisplayName
        {
            internal const string Invoice = "Invoice (Pay by check or wire transfer)";
            internal const string InvoiceBR = "Invoice (pay by boleto bancario or wire transfer)";
            internal const string Alipay = "Alipay";
            internal const string Unionpay = "Union Pay Online Payment";
        }

        internal static class PartnerName
        {
            internal const string Xbox = "xbox";
            internal const string Wallet = "wallet";
            internal const string Cart = "cart";
            internal const string Bing = "bing";
            internal const string OXODIME = "oxodime";
            internal const string OXOWebDirect = "oxowebdirect";
            internal const string Webblends = "webblends";
            internal const string WebblendsInline = "webblends_inline";
            internal const string CommercialStores = "commercialstores";
            internal const string CommercialWebblends = "commercialwebblends";
            internal const string CommercialSupport = "commercialsupport";
            internal const string WebPay = "webpay";
            internal const string Amc = "amc";
            internal const string AmcWeb = "amcweb";
            internal const string AmcXbox = "amcxbox";
            internal const string Azure = "azure";
            internal const string AzureIbiza = "azureibiza";
            internal const string AzureSignup = "azuresignup";
            internal const string MarketPlace = "marketplace";
            internal const string Mseg = "mseg";
            internal const string OfficeOobe = "officeoobe";
            internal const string OXOOobe = "oxooobe";
            internal const string SmbOobe = "smboobe";
            internal const string OneDrive = "onedrive";
            internal const string ConsumerSupport = "consumersupport";
            internal const string AppSource = "appsource";
            internal const string GGPDEDS = "ggpdeds";
            internal const string Payin = "payin";
            internal const string SetupOffice = "setupoffice";
            internal const string SetupOfficeSdx = "setupofficesdx";
            internal const string XboxWeb = "xboxweb";
            internal const string StoreOffice = "storeoffice";
            internal const string NorthStarWeb = "northstarweb";
            internal const string DefaultPartnerName = "default";
            internal const string Storify = "storify";
            internal const string XboxSettings = "xboxsettings";
            internal const string XboxNative = "xboxnative";
            internal const string XboxSubs = "xboxsubs";
            internal const string Saturn = "saturn";
            internal const string MSTeams = "msteams";
            internal const string Xbet = "xbet";
            internal const string MsTeams = "msteams";
            internal const string WindowsSettings = "windowssettings";
            internal const string WindowsStore = "windowsstore";
            internal const string MCPP = "mcpp";
            internal const string BattleNet = "battlenet";
            internal const string CandyCrush = "candycrush";
        }

        internal static class TemplateName
        {
            internal const string DefaultTemplate = "defaulttemplate";
            internal const string ListPIDropdown = "listpidropdown";
            internal const string ConsoleTemplate = "consoletemplate";
            internal const string SecondScreenTemplate = "secondscreentemplate";
        }

        internal static class TransationServiceStore
        {
            internal const string Azure = "Azure";
            internal const string OMS = "OMS";
        }

        internal static class PartnerFlightValues
        {
            internal const string SoldToHideButton = "soldToHideButton";
            internal const string DpHideCountry = "dpHideCountry";
            internal const string EnableItalyCodiceFiscale = "enableItalyCodiceFiscale";
            internal const string StandaloneProfile = "standaloneProfile";
            internal const string ShowMiddleName = "showMiddleName";
            internal const string ShowAVSSuggestions = "showAVSSuggestions";
            internal const string PXEnableXboxAccessibilityHint = "PXEnableXboxAccessibilityHint";
            internal const string PXEnableXboxNewAddressSequenceFrNl = "PXEnableXboxNewAddressSequenceFrNl";
            internal const string ApplyAccentBorderWithGutterOnFocus = "ApplyAccentBorderWithGutterOnFocus";
            internal const string ShowAVSSuggestionsModal = "showAVSSuggestionsModal";
            internal const string AADSupportSMD = "AADSupportSMD";
            internal const string SMDDisabled = "SMDDisabled";
            internal const string OriginCountryPrefix = "originCountry_";
            internal const string PartnerSettingsVersionPrefix = "partnerSettingsVersion_";
            internal const string XboxOOBE = "xboxOOBE";
            internal const string EnableThreeDSOne = "enableThreeDSOne";
            internal const string IndiaTokenizationMessage = "IndiaTokenizationMessage";
            internal const string IndiaExpiryGroupDelete = "IndiaExpiryGroupDelete";
            internal const string EnableIndiaTokenExpiryDetails = "EnableIndiaTokenExpiryDetails";
            internal const string IndiaUPIEnable = "IndiaUPIEnable";
            internal const string PxEnableUpi = "PxEnableUpi";
            internal const string PxEnableRiskEligibilityCheck = "PxEnableRiskEligibilityCheck";
            internal const string EnableGlobalUpiQr = "EnableGlobalUpiQr";
            internal const string EnableSelectUpiQr = "EnableSelectUpiQr";
            internal const string PXCommercialEnableUpi = "PxCommercialEnableUpi";
            internal const string EnableCommercialSelectUpiQr = "EnableCommercialSelectUpiQr";
            internal const string EnableCommercialGlobalUpiQr = "EnableCommercialGlobalUpiQr";
            internal const string DeletionSubscriptionErrorPidl = StaticDescriptionTypes.DeletionSubscriptionErrorPidl;
            internal const string ShowSummaryPage = "showSummaryPage";
            internal const string PxEnableVenmo = "PxEnableVenmo";
            internal const string PxEnableSelectPMAddPIVenmo = "PxEnableSelectPMAddPIVenmo";
            internal const string PXUseJarvisV3ForCompletePrerequisites = "PXUseJarvisV3ForCompletePrerequisites";
            internal const string PXUsePartnerSettingsService = "PXUsePartnerSettingsService";
            internal const string PXEnableSMSChallengeValidation = "PXEnableSMSChallengeValidation";
            internal const string IncludePIDLWithPaymentInstrumentList = "IncludePIDLWithPaymentInstrumentList";
            internal const string EnableLtsUpiQRConsumer = "EnableLtsUpiQRConsumer";
            internal const string PXReturnFailedSessionState = "PXReturnFailedSessionState";
            internal const string PXXboxCardApplyEnableFeedbackButton = "PXXboxCardApplyEnableFeedbackButton";
            internal const string PXXboxCardApplyDisableStoreButtonNavigation = "PXXboxCardApplyDisableStoreButtonNavigation";
            internal const string PXCOTTestAccounts = "PXCOTTestAccounts";

            // PX flighting to return 3ds Auth not supported error to storefronts.
            internal const string PXDisplay3dsNotEnabledErrorInline = "PXDisplay3dsNotEnabledErrorInline";

            // PX flighting to enable sepa jpmc account validation for the storefronts
            // flight cleanup task - 56373987
            internal const string EnableSepaJpmc = "EnableSepaJpmc";

            // PX flighting to enable new logo for sepa
            // flight cleanup task - 57811922
            internal const string EnableNewLogoSepa = "EnableNewLogoSepa";

            // The partner uses the flight to enable zip+4 2 additional property on jarvis profile address
            // (1) is_customer_consented = true, if user picks user entered address
            // (2) is_avs_full_validation_succeeded = true, if user picks suggested address
            // It is used for address clean up and only show trade avs screen when either flag = true
            internal const string EnableAVSAddtionalFlags = "enableAVSAddtionalFlags";
            internal const string IndiaTokenizationConsentCapture = "IndiaTokenizationConsentCapture";
            internal const string EnablePaymentMethodGrouping = "enablePaymentMethodGrouping";
            internal const string EnablePMGroupingSubpageSubmitBlock = "enablePMGroupingSubpageSubmitBlock";
            internal const string EnableGlobalPiInAddResource = "enableGlobalPiInAddResource";
            internal const string PaymentMethodsConfiguration = "PaymentMethodsConfiguration";
            internal const string PXEnableHIPCaptcha = "PXEnableHIPCaptcha";
            internal const string PXEnableHIPCaptchaGroup = "PXEnableHIPCaptchaGroup";
            internal const string PXEnableRedeemCSVFlow = "PXEnableRedeemCSVFlow";
            internal const string PXUsePostProcessingFeatureForRemovePI = "PXUsePostProcessingFeatureForRemovePI";
            internal const string PXDisableRedeemCSVFlow = "PXDisableRedeemCSVFlow";
            internal const string PXAddSecondaryClientActionWithIgnoreTerminatingErrorHandling = "PXAddSecondaryClientActionWithIgnoreTerminatingErrorHandling";
            internal const string PXServicePSSPPEEnvironment = "PXServicePSSPPEEnvironment";
            internal const string PXXboxCardApplicationEnableWebview = "PXXboxCardApplicationEnableWebview";
            internal const string PXXboxCardApplicationEnableShortUrl = "PXXboxCardApplicationEnableShortUrl";
            internal const string PXXboxCardApplicationEnableShortUrlText = "PXXboxCardApplicationEnableShortUrlText";
            internal const string PXEnableShortUrlPayPal = "PXEnableShortUrlPayPal";
            internal const string PXEnableShortUrlPayPalText = "PXEnableShortUrlPayPalText";
            internal const string PXEnableShortUrlVenmo = "PXEnableShortUrlVenmo";
            internal const string PXEnableUpdateCCLogo = "PXEnableUpdateCCLogo";
            internal const string PXEnableShortUrlVenmoText = "PXEnableShortUrlVenmoText";
            internal const string PXSwapSelectPMPages = "PXSwapSelectPMPages";
            internal const string PXEnableSecureFieldAddCreditCard = "PXEnableSecureFieldAddCreditCard";
            internal const string PXEnableSecureFieldUpdateCreditCard = "PXEnableSecureFieldUpdateCreditCard";
            internal const string PXEnableSecureFieldReplaceCreditCard = "PXEnableSecureFieldReplaceCreditCard";
            internal const string PXEnableSecureFieldSearchTransaction = "PXEnableSecureFieldSearchTransaction";
            internal const string PXEnableSecureFieldCvvChallenge = "PXEnableSecureFieldCvvChallenge";
            internal const string PXEnableSecureFieldIndia3DSChallenge = "PXEnableSecureFieldIndia3DSChallenge";
            internal const string PXEnableXboxNativeStyleHints = "PXEnableXboxNativeStyleHints";
            internal const string PXUseFontIcons = "PXUseFontIcons";
            internal const string PXEnableXboxCardUpsell = "PXEnableXboxCardUpsell";
            internal const string ListModernPIsWithCardArt = "ListModernPIsWithCardArt";
            internal const string PXEnableRupayForIN = "PXEnableRupayForIN";
            internal const string IndiaRupayEnable = "IndiaRupayEnable";
            internal const string PxEnableAddCcQrCode = "PxEnableAddCcQrCode";
            internal const string PXEnablePSD2PaymentInstrumentSession = "PXEnablePSD2PaymentInstrumentSession";
            internal const string PXEnableUpdateDiscoverCreditCardRegex = "PXEnableUpdateDiscoverCreditCardRegex";
            internal const string PXEnableUpdateVisaCreditCardRegex = "PXEnableUpdateVisaCreditCardRegex";
            internal const string PXUseInlineExpressCheckoutHtml = "PXUseInlineExpressCheckoutHtml";
            internal const string PXExpressCheckoutUseIntStaticResources = "PXExpressCheckoutUseIntStaticResources";
            internal const string PXExpressCheckoutUseProdStaticResources = "PXExpressCheckoutUseProdStaticResources";

            // Used to enable the china union pay payment method for the CN market for international partners
            internal const string PXEnableCUPInternational = "PXEnableCUPInternational";

            // used to prevent default selection of Add New Payment Method when PI list is empty
            internal const string PXPreventAddNewPaymentMethodDefaultSelection = "PXPreventAddNewPaymentMethodDefaultSelection";

            // Flight to enable purchase polling in confirm payment for UPI
            internal const string PXEnablePurchasePollingForUPIConfirmPayment = "PXEnablePurchasePollingForUPIConfirmPayment";

            // Flight to enable instance PI for GPay and Apay
            internal const string GPayApayInstancePI = "GPayApayInstancePI";

            internal const string IndiaCvvChallengeExpiryGroupDelete = "IndiaCvvChallengeExpiryGroupDelete";
        }

        internal static class FlightValues
        {
            internal const string PXAlipayQRCode = "PXAlipayQRCode";
            internal const string UpdateCaptchaErrorMessage = "UpdateCaptchaErrorMessage";
            internal const string HonorNewRiskCode = "HonorNewRiskCode";
            internal const string ReturnCardWalletInstanceIdForPidlList = "ReturnCardWalletInstanceIdForPidlList";
        }

        internal static class PidlUrlConstants
        {
            public const string StaticResourceServiceImagesV4 = "https://pmservices.cp.microsoft.com/staticresourceservice/images/v4";
            public const string XboxCoBrandedCardQRCodeURL = "https://www.xbox.com/{0}/xbox-mastercard/apply?channelname={1}&referrerid={2}&consoleapplysessionid={3}";
            public const string XboxCoBrandedCardWebviewURL = "https://www.xbox.com/{0}/xbox-mastercard/apply?channelname={1}&referrerid={2}&consoleapplysessionid={3}&isconsolewebview=true";
        }

        internal static class RequestHeaderValueTemplate
        {
            internal const string RequestContext = "{{\"tenantId\":\"{0}\"}}";
        }

        internal static class AppDetails
        {
            internal const string WalletPackageName = @"Microsoft.MicrosoftWallet";
            internal const string WalletPackageSid = @"ms-app://s-1-15-2-2222533797-2934089070-1576418215-35640970-881892585-609437930-4205438081";
            internal const string WalletCientSecrete = @"zu9+9JEbYxz8o8JIvu5o3cdOO97G1hjr";
            internal const string PaymentClientAppName = @"Microsoft.Payments.Client";
            internal const string PaymentOptionsAppName = @"Microsoft.Payment.Options";
        }

        internal static class PaymentInstrument
        {
            internal const string Details = "details";
            internal const string PaymentMethod = "paymentMethod";
            internal const string PaymentMethodFamily = "paymentMethodFamily";
            internal const string ClientAction = "clientAction";
            internal const string PaymentMethodOperation = "paymentMethodOperation";
            internal const string Channel = "channel";
        }

        internal static class DFPInstanceIds
        {
            internal const string INT = "8e23e7ff-e2a0-4b71-bede-2f0e7d1f6674";
            internal const string PROD = "2305fc2c-e4e2-4dc5-a921-00b9d46df0b7";
        }

        internal static class DeviceInfoProperty
        {
            internal const string IPAddress = "IPAddress";
            internal const string UserAgent = "UserAgent";
        }

        internal static class XboxConsoleBrowserAgent
        {
            internal const string Xbox = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; Xbox; Xbox One) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/111.0.0.0 Safari/537.36 Edg/111.0.1661.35";
        }

        internal static class WindowsBrowserAgent
        {
            internal const string Windows = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36 Edg/122.0.0.";
        }

        internal static class BillingGroupDataDiscriptionName
        {
            internal const string JarvisProfileId = "jarvisProfileId";
        }

        internal static class ServiceDefaults
        {
            internal const string DefaultPartnerName = "default";
            internal const string DefaultOperationType = "add";
            internal const string ProfileType = "consumer";
        }

        internal static class PidlResourceDescriptionType
        {
            internal const string Cc3DSRedirectStaticPidl = "cc3DSRedirectPidl";
            internal const string PaypalRedirectStaticPidl = "paypalredirectpidl";
            internal const string PaypalRetryStaticPidl = "paypalRetryStatic";
            internal const string IdealBillingAgreementRedirectStaticPidl = "idealredirectpidl";
            internal const string GenericRedirectStaticPidl = "genericredirectpidl";
            internal const string GenericPollingStaticPidl = "genericPollingStatic";
            internal const string Sms = "sms";
            internal const string AchPicVStatic = "achPicvStatic";
            internal const string AchPicVChallenge = "ach_picv";
            internal const string SepaPicVStatic = "sepaPicvStatic";
            internal const string SepaPicVChallenge = "sepa_picv";
            internal const string PaypalUpdateAgreementChallenge = "paypalUpdateAgreementChallenge";
            internal const string PaymentMethod = "paymentMethod";
            internal const string VenmoRedirectStaticPidl = "venmoredirectpidl";
            internal const string VenmoRetryStaticPidl = "venmoRetryStatic";
            internal const string TokensChallengeTypesPidl = "tokensChallengeTypesPidl";
        }

        internal static class PidlIdentityFields
        {
            internal const string Type = "type";
            internal const string ResourceId = "resource_id";
            internal const string Piid = "piid";
            internal const string Operation = "operation";
        }

        internal static class PidlIdentityValues
        {
            internal const string ConsumerPrerequisites = "consumerprerequisites";
        }

        internal static class PaymentProviderIds
        {
            internal const string Stripe = "stripe";
            internal const string PayPal = "paypal";
        }

        internal static class PidlResourceIdentity
        {
            internal const string List = "list";
        }

        internal static class CommercialTaxIdCountryRegionCodes
        {
            internal const string India = "IN";
            internal const string Taiwan = "TW";
            internal const string Italy = "IT";
            internal const string Egypt = "EG";
        }

        internal static class CommercialTaxIdStatus
        {
            internal const string Valid = "Valid";
        }

        internal static class CommercialTaxIdTypes
        {
            internal const string IndiaGst = "india_state_gst_in_id";
            internal const string IndiaPan = "india_pan_id";
            internal const string Vat = "vat_id";
            internal const string ItalyCodiceFiscale = "national_identification_number";
            internal const string EgyptNationalIdentificationNumber = "egypt_national_identification_number";
        }

        internal static class ScenarioNames
        {
            internal const string MergeData = "mergeData";
            internal const string Standalone = "standalone";
            internal const string BillingGroup = "billingGroup";
            internal const string RS5 = "rs5";
            internal const string AddressNoCityState = "addressNoCityState";
            internal const string TwoColumns = "twoColumns";
            internal const string AzureIbiza = "azureIbiza";
            internal const string WithProfileAddress = "withProfileAddress";
            internal const string PaypalQrCode = "paypalQrCode";
            internal const string GenericQrCode = "genericQrCode";
            internal const string ModernAccount = "modernAccount";
            internal const string HiddenProfile = "hiddenProfile";
            internal const string HiddenProfileWithName = "hiddenProfileWithName";
            internal const string HiddenProfileWithNameRoobe = "hiddenProfileWithNameRoobe";
            internal const string DepartmentalPurchase = "departmentalPurchase";
            internal const string CommercialSignUp = "commercialSignUp";
            internal const string WithCountryDropdown = "withCountryDropdown";
            internal const string EligiblePI = "eligiblePI";
            internal const string FixedCountrySelection = "fixedCountrySelection";
            internal const string PayNow = "paynow";
            internal const string ChangePI = "changePI";
            internal const string WithEditAddress = "withEditAddress";
            internal const string SuggestAddressesTradeAVS = "suggestAddressesTradeAVS";
            internal const string SuggestAddressesTradeAVSUsePidlModal = "suggestAddressesTradeAVSUsePidlModal";
            internal const string SuggestAddressesTradeAVSUsePidlPageV2 = "suggestAddressesTradeAVSUsePidlPageV2";
            internal const string Profile = "profile";
            internal const string ProfileAddress = "profileAddress";
            internal const string CreateBillingAccount = "createBillingAccount";
            internal const string DisplayOptionalFields = "displayOptionalFields";
            internal const string IndiaThreeDS = "indiathreeds";
            internal const string ThreeDSOnePolling = "threedsonepolling";
            internal const string PhoneConfirm = "phoneConfirm";
            internal const string PidlClientAction = "pidlClientAction";
            internal const string PidlContext = "pidlContext";
            internal const string PMGrouping = "pmGrouping";
            internal const string PollingAction = "pollingAction";
            internal const string Roobe = "roobe";
            internal const string XboxCoBrandedCard = "xboxCoBrandedCard";
            internal const string VenmoQRCode = "venmoQrCode";
            internal const string XboxApplyFullPageRender = "xboxApplyFullPageRender";
            internal const string VenmoWebPolling = "VenmoWebPolling";
            internal const string AddCCTwoPage = "addCCTwoPage";
            internal const string StoredValue = "storedValue";
            internal const string CreditCardQrCode = "creditCardQrCode";
            internal const string SecondScreenAddPi = "secondScreenAddPi";
            internal const string ThreeDSTwo = "3ds2";
            internal const string AddCCQrCode = "addCCQrCode";
        }

        internal static class TestScenarioNames
        {
            internal const string VenmoQRCode = "venmoQrCode";
        }

        internal static class PXServiceIntegrationErrorCodes
        {
            internal const string InvalidPendingOnType = "InvalidPendingOnType";
            internal const string InvalidPicvDetailsPayload = "InvalidPicvDetailsPayload";
            internal const string IncorrectPaymentMethodCount = "IncorrectPaymentMethodCount";
            internal const string InvalidSessionInfo = "InvalidSessionInfo";
            internal const string PimsSessionFailed = "PimsSessionFailed";
            internal const string PimsSessionExpired = "PimsSessionExpired";
            internal const string PimsRollbackReplacePIFailed = "PimsRollbackReplacePIFailed";
            internal const string PimsRemovePIAccessDeniedForTheCaller = "RemovePIAccessDeniedForTheCaller";
            internal const string PIDLInvalidAllowedPaymentMethods = "PIDLInvalidAllowedPaymentMethods";
            internal const string IssuerServiceBadStatus = "IssuerServiceBadStatus";
            internal const string InvalidOrExpiredSessionId = "InvalidOrExpiredSessionId";
        }

        internal static class ExpressCheckoutErrorCodes
        {
            internal const string InvalidAddress = "InvalidAddress";
            internal const string InvalidProfile = "InvalidProfile";
            internal const string InvalidPaymentInstrument = "InvalidPaymentInstrument";
        }

        internal static class XboxCardEligibilityStatus
        {
            internal const string None = "None";
            internal const string Approved = "Approved";
            internal const string PendingOnIssuer = "PendingOnIssuer";
            internal const string PendingOnApplication = "PendingOnApplication";
            internal const string Error = "Error";
            internal const string Cancelled = "Cancelled";
            internal const string Duplicate = "Duplicate";
            internal const string CardAlreadyIssued = "CardAlreadyIssued";
        }

        internal static class PXServiceErrorCodes
        {
            internal const string ArgumentIsNull = "ArgumentIsNull";
            internal const string RedirectPidlNotFound = "RedirectPidlNotFound";
            internal const string LegacyBillableAccountNotFound = "LegacyBillableAccountNotFound";
            internal const string LegacyBillableAccountUpdateFailed = "LegacyBillableAccountUpdateFailed";
            internal const string LegacyAccountServiceFailed = "LegacyAccountServiceFailed";
        }

        internal static class PaymentInstrumentStatus
        {
            internal const string Active = "active";
            internal const string Pending = "pending";
        }

        internal static class PaymentInstrumentPendingOnTypes
        {
            internal const string Sms = "sms";
            internal const string Redirect = "redirect";
            internal const string Notification = "notification";
            internal const string Picv = "picv";
            internal const string AgreementUpdate = "agreementUpdate";
        }

        internal static class PidlTemplatePath
        {
            internal const string Etag = "({dataSources.profileResource.etag})";
        }

        internal static class RequestType
        {
            internal const string AddPI = "addPI";
            internal const string GetPI = "getPI";
        }

        internal static class DescriptionTypes
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

        internal static class TaxIdPropertyDescriptionName
        {
            public const string TaxId = "taxId";
        }

        internal static class CreditCardPropertyDescriptionName
        {
            public const string AccountToken = "accountToken";
            public const string CvvToken = "cvvToken";
            public const string HMac = "hmac";
            public const string ExpiryYear = "expiryYear";
            public const string AccountHolderName = "accountHolderName";
        }

        internal static class ConditionalFieldsDescriptionName
        {
            public const string HideAddressGroup = "hideAddressGroup";
        }

        internal static class FraudDetectionServiceConstants
        {
            public const string ApprovedRecommendation = "Approved";
        }

        // Resource URL Teamplates
        internal static class UriTemplate
        {
            // List PIs and Add PI use the relative url below.
            internal const string ListPI = PimsVersion + "/{0}/paymentInstruments?deviceId={1}";
            internal const string ListEmpOrgPI = PimsVersion + "/emporg/paymentInstruments?deviceId={0}";
            internal const string PostPI = PimsVersion + "/{0}/paymentInstruments";
            internal const string PostPIForPaymentAccountId = "v3.0/paymentInstruments";
            internal const string GetPI = PimsVersion + "/{0}/paymentInstruments/{1}";
            internal const string GetSessionDetails = PimsVersion + "/{0}/{1}";
            internal const string AccountlessGetExtendedPI = PimsVersion + "/paymentInstruments/{0}/extendedView";
            internal const string UpdatePI = PimsVersion + "/{0}/paymentInstruments/{1}/update";
            internal const string CardProfile = PimsVersion + "/{0}/paymentInstruments/{1}/cardProfile?deviceId={2}";
            internal const string SeCardPersos = PimsVersion + "/{0}/paymentInstruments/{1}/seCardPersos?deviceId={2}";
            internal const string ReplenishTransactionCredentials = PimsVersion + "/{0}/paymentInstruments/{1}/replenishTransactionCredentials?deviceId={2}";
            internal const string AcquireLUKs = PimsVersion + "/{0}/paymentInstruments/{1}/acquireLuk?deviceId={2}";
            internal const string ConfirmLUKs = PimsVersion + "/{0}/paymentInstruments/{1}/confirmNotification?deviceId={2}";
            internal const string RemovePI = PimsVersion + "/{0}/paymentInstruments/{1}/remove";
            internal const string ReplacePI = PimsVersion + "/{0}/paymentInstruments/{1}/replace";
            internal const string PiPendingOperationsResume = PimsVersion + "/{0}/paymentInstruments/{1}/resume";
            internal const string ValidateCvv = PimsVersion + "/{0}/paymentInstruments/{1}/validateCvv";
            internal const string ValidatePicv = PimsVersion + "/{0}/paymentInstruments/{1}/verifyPicv";
            internal const string LinkSession = PimsVersion + "/{0}/paymentInstruments/{1}/LinkTransaction";
            internal const string Validate = PimsVersion + "/{0}/paymentInstruments/{1}/validate";
            internal const string PendingOperation = PimsVersion + "/{0}";
            internal const string DeviceId = "&deviceId={0}";
            internal const string Language = "&language={0}";
            internal const string GetPaymentMethods = PimsVersion + "/paymentMethods?country={0}&family={1}&type={2}&language={3}";
            internal const string GetHIPVisualCaptcha = "v1.0/challenge/visual?partnerid={0}";
            internal const string GetHIPAudioCaptcha = "v1.0/challenge/audio?partnerid={0}";
            internal const string PostHIPVisualCaptcha = "v1.0/challenge/visual/solution?partnerid={0}&azureregion={1}";
            internal const string PostHIPAudioCaptcha = "v1.0/challenge/audio/solution?partnerid={0}&azureregion={1}";
            internal const string PidlChallengeDescription = "/challengedescriptions?accountId={0}&piId={1}";
            internal const string GetProfilesByAccountId = "/{0}/profiles?type={1}";
            internal const string UpdateProfilesById = "/{0}/profiles/{1}";
            internal const string GetAddressByAddressId = "/{0}/addresses/{1}";
            internal const string PatchAddressByAddressId = "/{0}/addresses/{1}";
            internal const string PostAddressForAccountId = "/{0}/addresses";
            internal const string GetAddressesByCountry = "/{0}/addresses?country={1}";
            internal const string GetTaxIds = "/{0}/tax-ids";
            internal const string GetTenantCustomerByIdentity = "/customers/get-by-identity?provider=aad&type=tenant&tid={0}";
            internal const string GetEmployeeCustomerByIdentity = "/customers/get-by-identity?provider=aad&type=user&tid={0}&oid={1}";
            internal const string GetCustomerById = "/customers/{0}";
            internal const string LegacyAddressValidation = "/addresses";
            internal const string ModernAddressValidation = "/addresses/validate";
            internal const string JarvisPostAddressSyncLegacy3 = "/{0}/addresses?syncToLegacy={1}";
            internal const string JarvisGetOrCreateLegacyBillableAccount = "/{0}/get-or-create-legacy-billable-account?country={1}";
            internal const string ModernAddressLookup = "/addresses/lookup";
            internal const string GetStoredValueFundingCatalog = "/gift-catalog?currency={0}";
            internal const string FundStoredValue = "/{0}/funds";
            internal const string CheckFundStoredValue = "/{0}/funds/{1}";
            internal const string OrchestrationServiceReplacePaymentInstrument = "{0}/paymentInstruments/{1}/replace";
            internal const string OrchestrationServiceRemovePaymentInstrument = "{0}/paymentInstruments/{1}/remove";
            internal const string TransactionServiceCreatePaymentObject = "{0}/payments";
            internal const string TransactionServiceValidateCvv = "{0}/payments/{1}/validate";
            internal const string RDSServiceQuery = "/query/{0}";
            internal const string PifdAnonymousModernAVSForTrade = "https://{{pifd-endpoint}}/anonymous/addresses/ModernValidate?type={0}&partner={1}&language={2}&scenario={3}&country={4}";
            internal const string PxBrowserAuthenticateRedirectionUrlTemplate = "{0}/paymentSessions/{1}/browserAuthenticateRedirectionThreeDSOne";
            internal const string GetThirdPartyPaymentMethods = PimsVersion + "/thirdPartyPayments/eligiblePaymentMethods?provider={0}&sellerCountry={1}&buyerCountry={2}";
            internal const string PIMSPostSearchByAccountNumber = PimsVersion + "/paymentInstruments/searchByAccountNumber";
            internal const string PaymentOrchestratorServiceAttachPaymentInstrument = "paymentrequests/{0}/attachpaymentinstruments";
            internal const string PaymentOrchestratorServiceAttachAddressToCheckoutRequest = "checkoutRequests/{0}/attachaddress?type={1}";
            internal const string PaymentOrchestratorServiceAttachAddressToPaymentRequest = "paymentRequests/{0}/attachaddress?type={1}";
            internal const string PaymentOrchestratorServiceAttachProfileToCheckoutRequest = "checkoutRequests/{0}/attachprofile";
            internal const string PaymentOrchestratorServiceAttachProfileToPaymentRequest = "paymentRequests/{0}/attachprofile";
            internal const string PaymentOrchestratorServiceConfirmCheckoutRequest = "checkoutRequests/{0}/confirm";
            internal const string PaymentOrchestratorServiceAttachPaymentInstrumentToCheckoutRequest = "checkoutRequests/{0}/attachpaymentinstruments";
            internal const string PaymentOrchestratorServiceGetClientAction = "checkoutRequests/{0}/clientaction";
            internal const string PaymentOrchestratorServiceGetPaymentRequestClientAction = "paymentRequests/{0}/clientactions";
            internal const string PaymentOrchestratorServiceAttachPaymentInstrumentWallet = "walletrequests/{0}/attachpaymentinstruments";
            internal const string PaymentOrchestratorServiceGetPaymentRequest = "paymentRequests/{0}";
            internal const string PaymentOrchestratorServiceConfirmPaymentRequest = "paymentRequests/{0}/confirm";
            internal const string PaymentOrchestratorServiceAttachChallengeData = "paymentRequests/{0}/attachChallengeData";
            internal const string PaymentOrchestratorServiceGetEligiblePaymentMethods = "walletRequests/{0}/getEligiblePaymentMethods";
            internal const string PaymentOrchestratorServiceAttachCheckoutRequestChallengeData = "checkoutRequests/{0}/attachChallengeData";
            internal const string PaymentOrchestratorServiceRemoveEligiblePaymentmethods = "paymentRequests/{0}/removeEligiblePaymentMethods";

            // PaymentThirdPartyService url template
            internal const string GetPaymentRequest = "payment-providers/{0}/api/payment-requests/{1}";
            internal const string GetCheckout = "payment-providers/{0}/api/checkouts/{1}";
            internal const string CheckoutCharge = "payment-providers/{0}/api/checkouts/{1}/charge";

            // SellerMarketService url template
            internal const string GetSeller = "v1/payment-providers/{0}/sellers/{1} ";

            // Partner Settings url template
            internal const string GetPaymentExperienceSettings = "partnersettings/{0}?settingsType=PaymentExperience";

            // IssuerService Apply url template
            internal const string Apply = "applications/{0}";

            // IssuerService Application Details url template
            internal const string ApplicationDetails = "applications/{0}?cardProduct={1}&sessionId={2}";

            // IssuerService ApplyEligibility url template
            internal const string ApplyEligibility = "applications/{0}/eligibility?cardProduct={1}";

            // IssuerService ApplyInitialize url template
            internal const string ApplyInitalize = "applications/session";

            // WalletService getWalletConfig url template
            internal const string GetWalletConfig = "api/wallet/getproviderdata";

            // WalletService setupSession url template
            internal const string SetupProviderSession = "api/wallet/setupprovidersession";

            // WalletService provision wallet token url template
            internal const string ProvisionWalletToken = "api/wallet/provision";

            // WalletService setupSession url template
            internal const string GenerateDataId = "transactiondata/generatedataid";

            // WalletService walletValidate url template
            internal const string WalletValidate = "api/wallet/validate";

            // Transaction data service store api url template
            internal const string TransactionDataStore = "transactiondata/{0}/data/{1}";

            // Network tokens
            internal const string GetNetworkTokens = "tokens";

            // tokenizable
            internal const string Tokenizable = "tokenizable?bankIdentificationNumber={0}&cardProviderName={1}&networkTokenUsage={2}";

            // Network tokens
            internal const string ListNetworkTokensWithExternalCardReference = "tokens?externalCardReference={0}";

            // Fetch Credentials
            internal const string FetchCredentials = "tokens/{0}/credentials";

            // Request Device Binding Fido
            internal const string RequestDeviceBindingFido = "tokens/{0}/devicebinding/fido";

            // Request Challenge
            internal const string RequestChallenge = "tokens/{0}/devicebinding/challenges/{1}/request";

            // Validate Challenge
            internal const string ValidateChallenge = "tokens/{0}/devicebinding/challenges/{1}/validate";

            // Authenticate Passkeys
            internal const string PasskeyAuthenticate = "tokens/{0}/passkeys/authenticate";

            // Set up Passkeys
            internal const string PasskeySetup = "tokens/{0}/passkeys/setup";

            // Set Mandates
            internal const string SetMandates = "tokens/{0}/passkeys/mandate";

            // MSRewards GetUserInfo url template
            internal const string GetMSRewardsUserInfo = "api/users({0})?options=8&channel={1}";

            // MSRewards Redeem url template
            internal const string RedeemMSRewards = "api/users({0})/orders";

            // Redirection URL used in confirm payment component
            internal const string ConfirmPaymentRedirectUrlTemplate = "https://{{redirection-endpoint}}/RedirectionService/CoreRedirection/redirect/{0}";

            // Token descriptions request url template
            internal const string GetTokenDescription = "{0}/users/{1}/tokenDescriptionRequests";

            // Payment client descriptions override template
            internal const string GetPaymentClientDescription = "{pidl-endpoint}/paymentClient/descriptions";

            // Fraud detection bot detection url template
            internal const string FraudDetectionBotDetectionUrl = "api/v1/botcheck";
        }

        internal static class Versions
        {
            public const string ApiVersion = "v7.0";
            public const string Alpha = "alpha";
        }

        internal static class KlarnaErrorCodes
        {
            internal const string PersonalNumberBadFormat = "PersonalNumberBadFormat";
            internal const string InvalidPhoneValue = "InvalidPhoneValue";
            internal const string InvalidGender = "InvalidGender";
            internal const string InvalidNameAndAddress = "InvalidNameAndAddress";
            internal const string InvalidLastName = "InvalidLastName";
            internal const string InvalidFirstName = "InvalidFirstName";
            internal const string InvalidFirstAndLastNames = "InvalidFirstAndLastNames";
            internal const string InvalidPersonalNumber = "InvalidPersonalNumber";
        }

        internal static class KlarnaErrorMessages
        {
            internal const string PersonalNumberBadFormat = "Check the format of your personal number or date of birth.";
            internal const string InvalidPhoneValue = "Check your phone number. We can't match it to a registered address.";
            internal const string InvalidGender = "Check the field for gender. It doesn't match our records.";
            internal const string InvalidNameAndAddress = "Check your name and address. One doesn't match the other.";
            internal const string InvalidLastName = "Check your last name. It doesn't match our records.";
            internal const string InvalidFirstName = "Check your first name. It doesn't match our records.";
            internal const string InvalidFirstAndLastNames = "Check your first and last name. They don't match.";
            internal const string InvalidPersonalNumber = "Check your personal number or date of birth. It doesn't match our records.";
        }

        internal static class KlarnaErrorTargets
        {
            internal const string PersonalNumber = "nationalIdentificationNumber";
            internal const string Phone = "phone";
            internal const string LastName = "lastname";
            internal const string FirstName = "firstname";
            internal const string AddressLine1 = "address_line1";
            internal const string AddressLine2 = "address_line2";
            internal const string City = "city";
            internal const string Country = "country";
            internal const string PostalCode = "postal_code";
            internal const string Gender = "gender";
        }

        internal static class CreditCardErrorCodes
        {
            internal const string ValidationFailed = "ValidationFailed";
            internal const string TokenizationFailed = "TokenizationFailed";
            internal const string ExpiredCard = "ExpiredCard";
            internal const string InvalidAccountHolder = "InvalidAccountHolder";
            internal const string InvalidAddress = "InvalidAddress";
            internal const string InvalidCity = "InvalidCity";
            internal const string InvalidCountry = "InvalidCountry";
            internal const string InvalidCountryCode = "InvalidCountryCode";
            internal const string InvalidCvv = "InvalidCvv";
            internal const string InvalidExpiryDate = "InvalidExpiryDate";
            internal const string InvalidPaymentInstrumentInfo = "InvalidPaymentInstrumentInfo";
            internal const string InvalidState = "InvalidState";
            internal const string InvalidZipCode = "InvalidZipCode";
            internal const string InvalidRequestData = "InvalidRequestData";
            internal const string PrepaidCardNotSupported = "PrepaidCardNotSupported";
            internal const string InvalidStreet = "InvalidStreet";
            internal const string InvalidIssuerResponseWithTRPAU0009 = "InvalidIssuerResponseWithTRPAU0009";
            internal const string InvalidIssuerResponseWithTRPAU0008 = "InvalidIssuerResponseWithTRPAU0008";
            internal const string CaptchaChallengeRequired = "CaptchaChallengeRequired";
            internal const string ChallengeRequired = "ChallengeRequired";
            internal const string InvalidPaymentInstrumentType = "InvalidPaymentInstrumentType";
        }

        internal static class CreditCardErrorMessages
        {
            internal const string ValidationFailed = "Check that the details in all fields are correct or try a different card.";
            internal const string ExpiredCard = "Check your expiration date.";
            internal const string InvalidAccountHolder = "Check your name on the card. There appears to be an error in it.";
            internal const string InvalidAddress = "Check your address. There appears to be an error in it.";
            internal const string InvalidCity = "Check the city in your address. There appears to be an error in it.";
            internal const string InvalidCountry = "Choose your country or region again. There appears to be an error in it.";
            internal const string InvalidCvv = "Check your security code. There appears to be an error in it.";
            internal const string InvalidExpiryDate = "Try a different way to pay. This card has expired.";
            internal const string InvalidCardNumber = "Check your info. The card number entered is invalid.";
            internal const string InvalidState = "Check the state in your address. There appears to be an error in it.";
            internal const string InvalidZipCode = "Check the Zip or Postal code in your address. There appears to be an error in it.";
            internal const string PrepaidCardNotSupported = "Sorry, we can't accept pre-paid cards. Please try another payment method.";
            internal const string Generic = "Try that again. Something happened on our end. Waiting a bit can help.";
            internal const string PIMSValidationFailed = "The payment instrument cannot be validated. Please contact the payment processor for help.";
            internal const string InvalidIssuerResponse = "The card is not enabled for 3ds/otp authentication in India.";
            internal const string InvalidPaymentInstrumentType = "Check that the details in all fields are correct or try a different card.";
        }

        internal static class CreditCardErrorTargets
        {
            internal const string CardNumber = "accountToken";
            internal const string AccountHolderName = "accountHolderName";
            internal const string Cvv = "cvvToken";
            internal const string ExpiryMonth = "expiryMonth";
            internal const string ExpiryYear = "expiryYear";
            internal const string AddressLine1 = "address_line1";
            internal const string AddressLine2 = "address_line2";
            internal const string AddressLine3 = "address_line3";
            internal const string City = "city";
            internal const string State = "region";
            internal const string Country = "country";
            internal const string PostalCode = "postal_code";
        }

        internal static class LegacyInvoiceErrorCodes
        {
            internal const string InvalidAddress = "InvalidAddress";
            internal const string InvalidCity = "InvalidCity";
            internal const string InvalidCountry = "InvalidCountry";
            internal const string InvalidCountryCode = "InvalidCountryCode";
            internal const string InvalidState = "InvalidState";
            internal const string InvalidZipCode = "InvalidZipCode";
        }

        internal static class LegacyInvoiceErrorMessages
        {
            internal const string InvalidAddress = "Check your address. There appears to be an error in it.";
            internal const string InvalidCity = "Check the city in your address. There appears to be an error in it.";
            internal const string InvalidCountry = "Choose your country or region again. There appears to be an error in it.";
            internal const string InvalidState = "Check the state in your address. There appears to be an error in it.";
            internal const string InvalidZipCode = "Check the Zip or Postal code in your address. There appears to be an error in it.";
            internal const string Generic = "Try that again. Something happened on our end. Waiting a bit can help.";
        }

        internal static class LegacyInvoiceErrorTargets
        {
            internal const string AddressLine1 = "address_line1";
            internal const string AddressLine2 = "address_line2";
            internal const string AddressLine3 = "address_line3";
            internal const string City = "city";
            internal const string State = "region";
            internal const string Country = "country";
            internal const string PostalCode = "postal_code";
        }

        internal static class PicvStatus
        {
            internal const string InProgress = "inProgress";
            internal const string Failed = "failed";
            internal const string Expired = "expired";
            internal const string Success = "success";
        }

        internal static class DirectDebitErrorCodes
        {
            internal const string ValidationFailed = "ValidationFailed";
            internal const string InvalidAccountHolder = "InvalidAccountHolder";
            internal const string InvalidAddress = "InvalidAddress";
            internal const string InvalidBankCode = "InvalidBankCode";
            internal const string InvalidCity = "InvalidCity";
            internal const string InvalidCountry = "InvalidCountry";
            internal const string InvalidPaymentInstrumentInfo = "InvalidPaymentInstrumentInfo";
            internal const string InvalidState = "InvalidState";
            internal const string InvalidZipCode = "InvalidZipCode";
            internal const string InvalidAmount = "InvalidAmount";
            internal const string OperationNotSupported = "OperationNotSupported";
        }

        internal static class DirectDebitErrorMessages
        {
            internal const string ValidationFailedAch = "Check your bank account and routing numbers. The current pair isn't working.";
            internal const string ValidationFailedSepa = "Check your BIC and IBAN. The current pair isn't working.";
            internal const string Generic = "Try that again. Something happened on our end. Waiting a bit can help.";
            internal const string InvalidAccountHolder = "Check your name. This one's not right.";
            internal const string InvalidAddress = "Check your address. This one's not right.";
            internal const string InvalidBankCodeAch = "Check your bank routing number. This one's not right.";
            internal const string InvalidBankCodeSepa = "Check your bank code. This one's not right.";
            internal const string InvalidCity = "Check your city. Something's not right.";
            internal const string InvalidCountry = "Check your country. Something's not right.";
            internal const string InvalidPaymentInstrumentInfoAch = "Check your bank account number. This one's not right.";
            internal const string InvalidPaymentInstrumentInfoSepa = "Check your IBAN. This one's not right.";
            internal const string InvalidState = "Check your state. Something's not right.";
            internal const string InvalidZipCode = "Check your zip code. This one's not right.";
            internal const string InvalidAmount = "The amount you entered is incorrect.";
        }

        internal static class DirectDebitErrorTargets
        {
            internal const string AccountNumber = "accountToken";
            internal const string AccountHolderName = "accountHolderName";
            internal const string BankAccountType = "bankAccountType";
            internal const string BankCode = "bankCode";
            internal const string BankName = "bankName";
            internal const string AddressLine1 = "address_line1";
            internal const string AddressLine2 = "address_line2";
            internal const string City = "city";
            internal const string State = "region";
            internal const string Country = "country";
            internal const string PostalCode = "postal_code";
            internal const string Amount = "amount";
        }

        internal static class CupErrorCodes
        {
            internal const string ValidationFailed = "ValidationFailed";
            internal const string InvalidPhoneValue = "InvalidPhoneValue";
            internal const string InvalidPaymentInstrumentInfo = "InvalidPaymentInstrumentInfo";
            internal const string ProcessorUnreachable = "ProcessorUnreachable";
            internal const string InvalidRequestData = "InvalidRequestData";
            internal const string ProcessorTimeout = "ProcessorTimeout";
            internal const string InvalidExpiryDate = "InvalidExpiryDate";
            internal const string TooManyOperations = "TooManyOperations";
            internal const string InvalidChallengeCode = "InvalidChallengeCode";
            internal const string ChallengeCodeExpired = "ChallengeCodeExpired";
            internal const string ExpiredCard = "ExpiredCard";
            internal const string InvalidCvv = "InvalidCvv";
        }

        internal static class CupErrorMessages
        {
            internal const string InvalidPhoneOrCard = "Check your card and phone numbers. They don’t go together.";
            internal const string Generic = "Try that again. Something happened on our end. Waiting a bit can help.";
            internal const string InvalidExpiryDate = "Check your expiration date.";
            internal const string TooManySmsRequests = "Wait a bit before you ask for a new code. Your requests exceeded the limit.";
            internal const string InvalidSmsCode = "Check your code. The one entered isn't valid.";
            internal const string SmsCodeExpired = "Request a new code. This one expired.";
            internal const string InvalidPhoneOrCvv = "Check your card security code and your phone number.";
            internal const string InvalidCardNumber = "Check your card number. This one isn't valid.";
            internal const string InvalidPhoneNumber = "Check your phone number. This one isn't valid.";
        }

        internal static class CupErrorTargets
        {
            internal const string PhoneNumber = "phone";
            internal const string CardNumber = "accountToken";
            internal const string Cvv = "cvvToken";
            internal const string Sms = "pin";
            internal const string ExpiryMonth = "expiryMonth";
            internal const string ExpiryYear = "expiryYear";
        }

        internal static class QRCodeErrorMessages
        {
            internal const string RetryMax = "sessionIdData retry maximum exceeded";
            internal const string Expired = "sessionIdData is expired";
        }

        internal static class AlipayErrorCodes
        {
            internal const string InvalidAlipayAccount = "InvalidAlipayAccount";
            internal const string UserMobileNotMatch = "UserMobileNotMatch";
            internal const string UserCertNoMatch = "UserCertNoMatch";
            internal const string InvalidChallengeCode = "InvalidChallengeCode";
            internal const string ChallengeCodeExpired = "ChallengeCodeExpired";
        }

        internal static class AlipayErrorMessages
        {
            internal const string InvalidAlipayAccount = "Check your AliPay account info. There appears to be an error in it.";
            internal const string UserMobileNotMatch = "Check your mobile number. There appears to be an error in it.";
            internal const string UserCertNoMatch = "Check your last 5 digits info. There appears to be an error in it.";
            internal const string InvalidChallengeCode = "Check your code. The one entered isn't valid.";
            internal const string ChallengeCodeExpired = "Request a new code. This one expired.";
            internal const string Generic = "Try that again. Something happened on our end. Waiting a bit can help.";
        }

        internal static class AlipayErrorTargets
        {
            internal const string PhoneNumber = "phone";
            internal const string Account = "alipayAccount";
            internal const string Sms = "pin";
            internal const string LastFiveCertNo = "lastFiveCertNo";
        }

        internal static class UpiErrorCodes
        {
            internal const string InvalidUpiAccount = "AccountNotFound";
        }

        internal static class UpiErrorMessages
        {
            internal const string InvalidUpiAccount = "UPI Id verification failed.";
            internal const string Generic = "Try that again. Something happened on our end. Waiting a bit can help.";
        }

        internal static class UpiErrorTargets
        {
            internal const string Account = "vpa";
        }

        internal static class NonSimErrorCodes
        {
            internal const string RejectedByProvider = "RejectedByProvider";
            internal const string MOAccountNotFound = "MOAccountNotFound";
            internal const string InvalidChallengeCode = "InvalidChallengeCode";
            internal const string RiskRejected = "Rejected";
            internal const string PaymentInstrumentAddAlready = "PaymentInstrumentAddAlready";
        }

        internal static class NonSimErrorMessages
        {
            internal const string RejectedByProvider = "Check your phone number. The mobile operator you selected says it's not valid.";
            internal const string MOAccountNotFound = "Check your phone number. The mobile operator you selected can't find that number.";
            internal const string InvalidChallengeCode = "Check and re-enter the code. The code you entered is not valid.";
            internal const string Generic = "Try that again. Something happened on our end. Waiting a bit can help.";
            internal const string PaymentInstrumentAddAlready = "The instrument duplicated with an existing instrument for same account.";
        }

        internal static class NonSimErrorTargets
        {
            internal const string PhoneNumber = "msisdn";
            internal const string Sms = "pin";
        }

        internal static class PSD2ErrorCodes
        {
            internal const string RejectedByProvider = "RejectedByProvider";
            internal const string ValidatePIOnAttachFailed = "ValidatePIOnAttachFailed";
            internal const string InvalidPaymentSession = "InvalidPaymentSession";
            internal const string InvalidSuccessRedirectionUrl = "InvalidSuccessRedirectionUrl";
            internal const string InvalidFailureRedirectionUrl = "InvalidFailureRedirectionUrl";
        }

        internal static class ThreeDSErrorCodes
        {
            internal const string ThreeDSOneResumeAddPiFailed = "ThreeDSOneResumeAddPiFailed";
            internal const string InternalServerError = "InternalServerError";
        }

        internal static class PSD2UserDisplayMessages
        {
            internal const string ValidatePIOnAttachFailed = "Your bank could not authorize this payment method. Contact them for more info.";
        }

        internal static class PayPalBillingAgreementTypes
        {
            internal const string MerchantInitiatedBilling = "MerchantInitiatedBilling";
            internal const string MerchantInitiatedBillingSingleAgreement = "MerchantInitiatedBillingSingleAgreement";
            internal const string ChannelInitiatedBilling = "ChannelInitiatedBilling";
            internal const string Unknown = "Unknown";
        }

        internal static class PayPalErrorCodes
        {
            internal const string IncorrectCredential = "IncorrectCredential";
            internal const string AccountNotFound = "AccountNotFound";
        }

        internal static class PayPalErrorMessages
        {
            internal const string IncorrectCredential = "Check your PayPal sign-in info for errors.";
            internal const string Generic = "Try that again. Something happened on our end. Waiting a bit can help.";
        }

        internal static class PayPalErrorTargets
        {
            // [SuppressMessage("Microsoft.Security", "CS002:SecretInNextLine", Justification="This is not a secret")]
            internal const string Password = "encryptedPassword"; // lgtm[cs/password-hardcoded]
            internal const string Email = "email";
        }

        internal static class ValidateCvvErrorCodes
        {
            internal const string InvalidCvv = "InvalidCvv";
        }

        internal static class ValidateCvvErrorMessages
        {
            internal const string InvalidCvv = "Check your security code. There appears to be an error in it.";
        }

        internal static class ValidateCvvErrorTargets
        {
            internal const string Cvv = "cvvToken";
        }

        internal static class MissingErrorMessage
        {
            internal const string MissingValue = "{0} is missing";
        }

        internal static class CaptchaErrors
        {
            internal const string InvalidCaptcha = "InvalidCaptcha";
            internal const string CaptchaRequired = "Captcha is a required field";
            internal const string FirstTimeCaptchaRequiredMessage = "For security purposes, captcha verification is required.";
            internal const string InvalidCaptchaMessage = "Invalid captcha";
        }

        internal static class ChallengeEvidenceTypes
        {
            internal const string Captcha = "CAPTCHA";
            internal const string Challenge = "CHALLENGE";
        }

        internal static class ChallengeEvidenceResults
        {
            internal const string Success = "success";
        }

        internal static class ClientActionContract
        {
            internal const string NoMessage = "[]";
        }

        internal static class TaxIdTypes
        {
            public const string Consumer = "consumer_tax_id";
            public const string Commercial = "commercial_tax_id";
        }

        internal static class AddressTypes
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

        internal static class AuthenticationTypes
        {
            public const string Aad = "aad";
        }

        internal static class UserTypes
        {
            public const string UserMe = "me";
            public const string UserMyOrg = "my-org";
        }

        internal static class QueryParameterName
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
            public const string WindowSize = "windowSize";
        }

        internal static class CountryCodes
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

        internal static class Currencies
        {
            public const string USD = "USD";
        }

        internal static class Operations
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

        internal static class Component
        {
            public const string HandlePaymentChallenge = "handlePaymentChallenge";
            public const string HandlePurchaseRiskChallenge = "handlePurchaseRiskChallenge";
            public const string ConfirmPayment = "confirmPayment";
            internal const string Initialize = "initialize";
            internal const string QuickPayment = "quickPayment";
            internal const string ExpressCheckout = "ExpressCheckout";
            internal const string OrderSummary = "ordersummary";
            internal const string Payment = "payment";
            internal const string Address = "address";
            internal const string Profile = "profile";
            internal const string Confirm = "confirm";
            internal const string Challenge = "challenge";

            internal const string OrderSummaryProps = "orderSummaryProps";
            internal const string AddressProps = "addressProps";
            internal const string ProfileProps = "profileProps";
            internal const string ConfirmProps = "confirmProps";
            internal const string QuickPaymentProps = "quickPaymentProps";
            internal const string PaymentProps = "paymentProps";
        }

        internal static class ProfileType
        {
            public const string OrganizationProfile = "organization";
            public const string LegalEntityProfile = "legalentity";
            public const string Consumer = "consumer";
            public const string ConsumerV3 = "consumerV3";
            public const string Checkout = "checkout";
        }

        internal static class PIDLDataPropertyNames
        {
            public const string ComponentsDataConfirmWindowSize = "componentsData.confirm.windowSize";
            public const string PaymentRequestClientActions = "paymentRequestClientActions";
        }

        internal static class Profile
        {
            public const string DefaultAddress = "default_address";
            public const string Culture = "culture";
            public const string Language = "language";
            public const string ProfileEmployeeCulture = "profile_employee_culture";
            public const string ProfileEmployeeLanguage = "profile_employee_language";
        }

        internal static class DisplayHintIds
        {
            public const string EnterChallengeCodeText = "enterChallengeCodeText";
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
            public const string GotItButton = "gotItButton";
            public const string RedeemButton = "redeemButton";
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

            // XboxNative Rewards Pages
            public const string RewardsPointsValueText = "rewardsPointsValueText";
            public const string CurrencyValueText = "currencyValueText";
            public const string RightArrowText = "rightArrowText";
            public const string ErrorBadgeText = "errorBadgeText";

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

        internal static class ReturnContextClientActionTypes
        {
            public const string Refresh = "refresh";
        }

        internal static class ResourceTypes
        {
            public const string Address = "address";
        }

        internal static class PropertyDescriptionIds
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

            // payment client - Payment
            public const string AccountHolderName = "accountHolderName";
        }

        internal static class SubmitUrls
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

        internal static class StringPlaceholders
        {
            public const string ShortUrlPlaceholder = "{shortUrlPlaceholder}";
            public const string PIPlaceholder = "{PIPlaceholder}";
            public const string ChallengeMethodPlaceholder = "{challengeMethodPlaceholder}";
        }

        internal static class ButtonDisplayHintIds
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

        internal static class SessionTokenClaimTypes
        {
            public const string MerchantId = "mid";
            public const string ShopperId = "sid";
        }

        internal static class CSPStepNames
        {
            public const string Fingerprint = "cspStepFingerprint";
            public const string Challenge = "cspStepChallenge";
            public const string None = "cspNone";
        }

        internal static class SessionFieldNames
        {
            public const string CSPStep = "cspStep";
            public const string ThreeDSSessionData = "threeDSSessionData";
            public const string ThreeDSMethodData = "threeDSMethodData";
            public const string CReq = "creq";
            public const string CRes = "cres";
        }

        internal static class DataDescriptionPropertyNames
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

        internal static class CaptchaTypes
        {
            public const string Audio = "audio";
            public const string Visual = "image";
        }

        internal static class CaptchaLabels
        {
            public const string UseAudio = "Use audio captcha";
            public const string AudioText = "Type all of the words you hear, separated by a space. Characters are not case-sensitive.";
            public const string UseImage = "Use image captcha";
            public const string ImageText = "Type the characters above. Characters are not case-sensitive.";
            public const string CaptchaVerification = "Captcha verification";
        }

        internal static class HapiTaxIdDataDescriptionPropertyNames
        {
            public const string State = "state";
        }

        internal static class UpdatePropertyValueClientActionPropertyNames
        {
            public const string Id = "id";
        }

        internal static class ChallengeDescriptionTypes
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

        internal static class SyncToLegacyCodes
        {
            public const int ValidationUsingAVS = 0;  // validate address in using legacy avs
            public const int CreateLegacyAccountAndSync = 1; // create legacy account if user doesn't have and sync address to legacy
            public const int NoLegacyAccountCreationAndSync = 2; // not create legacy account if user doesn't have, otherwise sync address to legacy
            public const int NoSyncAndValidation = 3; // Do not sync to legacy and do not validate address in legacy
        }

        internal static class AddressNoCityStatePropertyNames
        {
            public const string City = "city";
            public const string Region = "region";
        }

        internal static class ChallengeTypes
        {
            public const string ValidatePIOnAttachChallenge = "ValidatePIOnAttachChallenge";
            public const string PSD2Challenge = "PSD2Challenge";
            public const string India3DSChallenge = "India3DSChallenge";
            public const string LegacyBillDeskPaymentChallenge = "LegacyBillDeskPaymentChallenge";
            public const string UPIChallenge = "UPIChallenge";
            public const string CreditCardQrCode = "creditCardQrCode";
        }

        internal static class ValidateResultMessages
        {
            public const string Failed = "Failed";
        }

        internal static class PaymentMethodCardProductTypes
        {
            public const string XboxCreditCard = "XboxCreditCard";
        }

        internal static class Prefixes
        {
            internal const string PaymentInstrumentSessionPrefix = "PX-3DS2-";
        }

        internal static class ShortURLServiceTimeToLive
        {
            internal const int ShortURLActiveTTL = 20;
        }

        internal static class AddPIQrCode
        {
            internal const int QrCodeSessionTimeoutInMinutes = 10;
            internal const int AnonymousSecondScreenFormRenderedTimeoutInMinutes = 15;
            internal const int MaxSessionIdRetry = 5;
            internal const string AccountId = "accountId";
            internal const string Email = "email";
            internal const string AllowTestHeader = "allowTestHeader";
        }

        internal static class CheckoutRequestPropertyName
        {
            internal const string Email = "email_address";
            internal const string PIID = "selected_PIID";
            internal const string ExpressCheckoutPaymentData = "expressCheckoutPaymentData";
            internal const string Country = "confirmCountry";
            internal const string PaymentMethodType = "paymentMethodType";
        }

        internal static class ExpressCheckoutPropertyValue
        {
            internal const string Country = "checkoutCountry";
            internal const string ExpressCheckoutPaymentData = "expressCheckoutPaymentData";
            internal const string PaymentMethodType = "paymentMethodType";
        }

        internal static class TestAccountHeaders
        {
            internal const string MDollarPurchase = "mdollarpurchase";
        }

        internal static class PidlSdkVersionNumber
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

        internal static class CharacterCount
        {
            internal const int MaxBrowserLanguageInput = 8;
        }

        internal static class PurchaseOrderState
        {
            internal const string Editing = "Editing";
        }

        internal static class RootCertVersion
        {
            internal const string UpdatedVersion17 = "V17";
            internal const int UpdatedVersionInt17 = 17;
        }

        internal static class PSD2ValidationErrorMessage
        {
            internal const string SignatureVerificationFailed = "Signature verification for the PSD2 certificate failed. ";
            internal const string PaymentMethodTypeIssue = "PaymentMethodType and dsInfo mismatch.";
        }

        internal static class ServiceNames
        {
            // We are overloading serviceName to track dsCertificateValidation failures
            internal const string DsCertificateValidation = "dsCertificateValidation";
            internal const string PayerAuth = "payerAuth";
        }

        internal static class PSD2NativeChallengeStrings
        {
            internal const string Header = "Secure Checkout";
            internal const string BackButtonLabel = "Back";
            internal const string BackButtonAccessibilityLabel = "Press to go back";
            internal const string CancelButtonLabel = "Cancel";
            internal const string CancelButtonAccessibilityLabel = "Press to cancel";
            internal const string OrderingAccessibilityLabel = "of";
            internal const string BankLogoAccessibilityLabel = "Bank Logo";
            internal const string CardLogoAccessibilityLabel = "Card Logo";
        }

        internal static class StaticDescriptionTypes
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

        internal static class DeletionErrorCodes
        {
            internal const string SubscriptionNotCanceled = "SubscriptionNotCanceled";
            internal const string OutstandingBalance = "OutstandingBalance";
            internal const string RemovePIAccessDeniedForTheCaller = "RemovePIAccessDeniedForTheCaller";
        }

        internal static class DeletionErrorMessages
        {
            internal const string SubscriptionNotCanceledMessage = "You have subscriptions/orders that use this payment method. Please update the subscriptions/orders to use a different payment method before removing this one.";
            internal const string OutstandingBalanceMessage = "The payment instrument has an outstanding balance that is not paid.";
            internal const string RemoveBusinessInstrumentNotSupportedMessage = "You have an Azure subscription associated with this payment method. Please update the subscription in the Azure portal.";
            internal const string ManageMessage = "Please go to account.microsoft.com to manage.";
        }

        internal static class ThirdPartyPaymentsErrorCodes
        {
            internal const string ResourceNotFound = "ResourceNotFound";
            internal const string ServiceError = "ServiceError";
            internal const string CvvValueMismatch = "CvvValueMismatch";
            internal const string ExpiredPaymentInstrument = "ExpiredPaymentInstrument";
            internal const string InvalidPaymentInstrument = "InvalidPaymentInstrument";
            internal const string RequestDeclined = "RequestDeclined";
            internal const string RequestFailed = "RequestFailed";
            internal const string InvalidPaymentInstrumentId = "InvalidPaymentInstrumentId";
            internal const string PaymentInstrumentNotActive = "PaymentInstrumentNotActive";
            internal const string InvalidRequestData = "InvalidRequestData";
            internal const string MerchantSelectionFailure = "MerchantSelectionFailure";
            internal const string RetryLimitExceeded = "RetryLimitExceeded";
            internal const string ProcessorDeclined = "ProcessorDeclined";
            internal const string ProcessorRiskCheckDeclined = "ProcessorRiskCheckDeclined";
            internal const string AmountLimitExceeded = "AmountLimitExceeded";
            internal const string InsufficientFund = "InsufficientFund";
            internal const string MissingFundingSource = "MissingFundingSource";
            internal const string TransactionNotAllowed = "TransactionNotAllowed";
            internal const string InvalidTransactionData = "InvalidTransactionData";
            internal const string AuthenticationRequired = "AuthenticationRequired";
            internal const string InvalidHeader = "InvalidHeader";
        }

        internal static class ThirdPartyPaymentsErrorMessages
        {
            // TO DO: update the messages
            internal const string ResourceNotFound = "The resource with given identity is not found";
            internal const string ServiceError = "Internal service error, usually means coordinator error";
            internal const string CvvValueMismatch = "The cvv didn't match the one on file";
            internal const string ExpiredPaymentInstrument = "The payment instrument has expired";
            internal const string InvalidPaymentInstrument = "An invalid payment instrument account number. Verify the information or use another payment instrument";
            internal const string RequestDeclined = "The transaction request is declined, maybe by Risk or Provider";
            internal const string RequestFailed = "The transaction request is failed, due to payment internal issue";
            internal const string InvalidPaymentInstrumentId = "The transaction request is failed, due to an invalid payment instrument Id";
            internal const string PaymentInstrumentNotActive = "The transaction request is failed, because the payment instrument is not active";
            internal const string InvalidRequestData = "There are some invalid data items in request object";
            internal const string MerchantSelectionFailure = "Can't select a merchant basing on the currency/country/market etc.";
            internal const string RetryLimitExceeded = "Retry count is greater than the permitted value";
            internal const string ProcessorDeclined = "Declined by processor for no clear explanation";
            internal const string ProcessorRiskCheckDeclined = "Processor decided that the transaction could be fraudulent and rejected it";
            internal const string AmountLimitExceeded = "The requested amount exceeded the allowed threshold set by the issuer or the provider";
            internal const string InsufficientFund = "The payment instrument lacks enough fund in the account";
            internal const string MissingFundingSource = "The payment instrument lacks a credible funding source";
            internal const string TransactionNotAllowed = "The customer's payment instrument cannot be used for this kind of purchase";
            internal const string InvalidTransactionData = "Generic error code for input parameter validation failure, detail message should contain the specific parameter type";
            internal const string AuthenticationRequired = "The customer need to perform SCA to complete authentication for 3DS";
            internal const string InvalidHeader = "One or more of the headers was incorrect";
            internal const string TryAgainMsg = "Please try again.";
            internal const string PaymentMethodErrorMsg = "Please use a different payment method.";
        }

        internal static class ThirdPartyPaymentsErrorTargets
        {
            internal const string CardNumber = "accountToken";
            internal const string CardHolderName = "accountHolderName";
            internal const string Cvv = "cvvToken";
            internal const string ExpiryMonth = "expiryMonth";
            internal const string ExpiryYear = "expiryYear";
            internal const string AddressLine1 = "address_line1";
            internal const string AddressLine2 = "address_line2";
            internal const string City = "city";
            internal const string State = "region";
            internal const string ZipCode = "postal_code";
        }

        internal static class PaymentOptionsAppErrorStrings
        {
            internal const string Header = "An error has occurred";
            internal const string Body = "Please try again later, or contact support if you have any questions.";
            internal const string SupportLink = "https://support.microsoft.com/account-billing/troubleshoot-payment-option-issues-135ed6ea-dd2b-47b7-cbcf-24f9f39da738";
            internal const string SupportLinkText = "Click here to learn more";
            internal const string ButtonText = "Close";
        }

        internal static class PaymentOptionsAppGenericErrorStringNames
        {
            internal const string Header = "header";
            internal const string Body = "body";
            internal const string SupportLink = "supportLink";
            internal const string SupportLinkText = "supportLinkText";
            internal const string ButtonText = "buttonText";
        }

        internal static class CaptchaHIPHeaders
        {
            public const string ChallengeID = "challenge-id";
            public const string AzureRegion = "azureregion";
        }

        internal static class DisplayPropertyName
        {
            public const string LegacyBusiness = "LegacyBusiness";
        }

        internal static class RedirectionPatterns
        {
            public const string FullPage = "fullPage";
            public const string Inline = "inline";
            public const string IFrame = "iFrame";
            public const string QRCode = "QRCode";
        }

        internal static class RequestDomains
        {
            public const string Localhost = "localhost";
            public const string XboxCom = "www.xbox.com";
            public const string OriginPPEXboxCom = "origin-ppe.xbox.com";
        }

        internal static class DestinationId
        {
            public const string ApplyOnConsole = "applyOnConsole";
        }

        internal static class WalletServiceConstants
        {
            public const string ApplePay = "ApplePay";
            public const string GooglePay = "GooglePay";
            public const string IntegrationType = "DIRECT";
            public const string ApplePayPiidPrefix = "cw_apay";
            public const string GooglePayPiidPrefix = "cw_gpay";
        }

        internal static class CharUnicodes
        {
            public const string SuperscriptOne = "\u00B9";
            public const string SuperscriptTwo = "\u00B2";
            public const string SuperscriptThree = "\u00B3";
            public const string SuperscriptOpenParenthesis = "\u207D";
            public const string SuperscriptCloseParenthesis = "\u207E";
        }

        internal static class FontIcons
        {
            public const string RightArrow = "\uE72A";
            public const string ErrorBadge = "\uEA39";
        }

        internal static class WalletConfigConstants
        {
            public const string DisplayName = "Microsoft";
            public const string Initiative = "Web";
            public const string InitiateContext = "mystore.example.com";
            public const string WalletConfig = "walletConfig";
            public const string PayLabel = "amount due plus applicable taxes";
            public const string TaxIncludedPayLabel = "amount due";
        }

        internal static class MSRewardsErrorMessages
        {
            public const string InsufficientBalance = "Insufficient balance";
        }

        internal static class ApplyErrrorCodes
        {
            public const string BadSessionState = "BadSessionState";
        }

        internal static class ChallengeManagementServiceErrorCodes
        {
            public const string Conflict = "Conflict";
        }

        internal static class ChallengeManagementDisplayLabels
        {
            internal const string BackButtonLabel = "Back";
            internal const string NextButtonLabel = "Next";
        }

        internal static class WalletValidationStatusConstants
        {
            internal const string Approved = "Approved";
        }

        internal static class RiskErrorCode
        {
            internal const string PIEligibilityCheckRejectedbyRisk = "PIEligibilityCheckRejectedbyRisk";
        }

        internal static class RiskErrorMessages
        {
            internal const string PIEligibilityCheckRejectedbyRisk = "The request to use the payment method has been rejected by risk eligibility check.";
        }

        internal static class WalletDeviceSupportedDebugMessages
        {
            internal const string IsCrossOrigin = "isCrossOrigin";
            internal const string ExcludedByFlight = "excludedByFlight";
        }

        internal static class WalletBrowserValues
        {
            internal const string Safari = "safari";
            internal const string Chrome = "chrome";
            internal const string Edge = "edge";
        }

        internal static class PropertyEventType
        {
            internal const string ValidateOnChange = "validateOnChange";
            internal const string MergeData = "mergeData";
        }

        internal static class PropertySelectType
        {
            internal const string Checkbox = "checkbox";
        }

        internal static class RequestContextType
        {
            internal const string Payment = "pr";
            internal const string Checkout = "cr";
            internal const string Wallet = "wr";
        }

        internal static class PropertyOrientation
        {
            public const string Inline = "inline";
        }

        internal static class DisplayTagKeys
        {
            internal const string AccessibilityName = "accessibilityName";
        }

        internal static class ExpressCheckoutButtonPayloadKey
        {
            internal const string Amount = "amount";
            internal const string ActionType = "actionType";
            internal const string Options = "options";
            internal const string RecurringPaymentDetails = "recurringPaymentDetails";
            internal const string TopDomainUrl = "topDomainUrl";
        }

        internal static class PIDLResourceDescriptionId
        {
            internal const string ExpressCheckout = "expressCheckout";
            internal const string QuickPayment = "quickPayment";
        }

        internal static class PropertyDisplayExample
        {
            internal const string AccountToken = "0000 0000 0000 0000";
            internal const string CVVToken = "000";
        }

        internal static class PropertyDisplayName
        {
            internal const string ShowDisplayNameTrue = "true";
            internal const string ShowDisplayNameFalse = "false";
            internal const string ExpirationDate = "Expiration date";
        }

        internal static class DisplayTag
        {
            internal const string CustomDropdown = "custom-dropdown";
            internal const string CardholderName = "Cardholder name";
        }

        internal static class UnlocalizedDisplayText
        {
            internal const string DeletePIDisplayTextLastFourDigits = "card ending in {0}";
        }

        internal static class EnvironmentEndpoint
        {
            internal const string INT = "pmservices.cp.microsoft-int.com";
            internal const string PROD = "pmservices.cp.microsoft.com";
        }

        internal static class AgenticPaymentRequestData
        {
            internal const string TotalAuthenticationAmount = "totalAuthenticationAmount";
            internal const string CurrencyCode = "currencyCode";
            internal const string SessionContextJsonString = "sessionContextJsonString";
            internal const string BrowserDataJsonString = "browserDataJsonString";
            internal const string ApplicationUrl = "applicationUrl";
            internal const string MerchantName = "merchantName";
            internal const string ChallengeMethodId = "challengeMethodId";
            internal const string Pin = "pin";
            internal const string PaymentMethodType = "paymentMethodType";
            internal const string FIDOResponse = "FIDOResponse";
            internal const string DfpSessionID = "dfpSessionID";
            internal const string AccountHolderEmail = "accountHolderEmail";
            internal const string MandateJsonString = "mandateJsonString";
            internal const string PaymentInstrumentId = "paymentInstrumentId";
            internal const string ChallengeMethodsJsonString = "challengeMethodsJsonString";
        }
    }
}