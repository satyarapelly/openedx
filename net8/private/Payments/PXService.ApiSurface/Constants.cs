// <copyright file="Constants.cs" company="Microsoft">Copyright (c) Microsoft 2019. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PXService.ApiSurface
{
    using System;
    using System.Collections.Generic;

    internal static class Constants
    {
        public static List<string> AddressTypes
        {
            get
            {
                return new List<string>
                {
                    "billing",
                    "shipping",
                    "billinggroup",
                    "hapiServiceUsageAddress",
                    "organization",
                    "individual",
                    "hapiV1SoldToOrganization",
                    "hapiV1ShipToOrganization",
                    "hapiV1BillToOrganization",
                    "hapiV1SoldToIndividual",
                    "hapiV1ShipToIndividual",
                    "hapiV1BillToIndividual",
                    "hapiV1",
                    "orgAddress"
                };
            }
        }

        public static List<string> ConsumerProfileTypes
        {
            get
            {
                return new List<string>
                {
                    ProfileType.Consumer,
                    ProfileType.ConsumerV3
                };
            }
        }

        ////public static List<string> CommercialProfileTypes
        ////{
        ////    get
        ////    {
        ////        return new List<string>
        ////        {
        ////            "employee", "organization", "legalentity"
        ////        };
        ////    }
        ////}

        public static List<string> TaxIdScenarios
        {
            get
            {
                return new List<string>
                {
                    "consumer_tax_id", "commercial_tax_id"
                };
            }
        }

        public static List<string> ThirdPartyPaymentTestScenarios
        {
            get
            {
                return new List<string>
                {
                    "px.3pp.stripe.guestcheckout.success,px.sellermarket.stripe.us",
                    "px.3pp.stripe.guestcheckout.prepaid.success,px.sellermarket.stripe.us,px.pims.3pp.stripe.guestcheckout.success",
                    "px.3pp.stripe.guestcheckout.processor.declined,px.sellermarket.stripe.us"
                };
            }
        }

        public static Dictionary<string, List<string>> ChallengeTypes
        {
            get
            {
                return new Dictionary<string, List<string>>
                {
                    {
                        "cvv", new List<string> { "oxodime", "oxowebdirect", "webblends", "webblends_inline", "xbox", "cart", "bing", "defaulttemplate", "storify", "xboxsettings" }
                    },
                    {
                        "sms", new List<string> { "oxodime", "oxowebdirect", "webblends", "webblends_inline", "xbox", "defaulttemplate", "storify", "xboxsettings" }
                    }
                };
            }
        }

        public static List<string> ChallengeTypes3DS
        {
            get
            {
                return new List<string> { "amcweb", "cart", "webblends", "webblends_inline", "xbox", "officeoobe", "oxooobe", "payin", "webpay", "consumersupport", "defaulttemplate" };
            }
        }

        public static List<string> CountriesTest
        {
            get
            {
                return new List<string>
                {
                    "us", "jp", "cn", "fi", "cz", "gb", "de", "br", "nl", "hk", "ru", "at", "in", "xk", "ly", "ss", "sv", "kh", "ve",
                    "tm", "bo", "ao", "bj", "bf", "bi", "cm", "cf", "td", "km", "cg", "cd", "ci", "dj", "gq", "er", "ga", "gh", "fr"
                };
            }
        }

        public static List<string> CommercialProfileCountriesTest
        {
            get
            {
                return new List<string>
                {
                    "us", "de", "gb", "tr", "tw", "in", "br", "rs", "bs", "nz", "be", "za", "co", "at", "cl", "mx", "id", "th", "ss",
                    "bh", "cm", "ge", "gh", "is", "ke", "md", "ng", "om", "tj", "ua", "uz", "zw", "fj", "gt", "kh", "ph", "vn",
                    "ae", "bd", "co", "sa", "kr", "sv", "bb", "il", "kz", "la", "np", "sg", "ug", "it", "eg", "ci", "sn", "zm"
                };
            }
        }

        public static List<string> CommercialTaxCountriesTest
        {
            get
            {
                return new List<string>
                {
                    "de", "gb", "tr", "tw", "in", "br", "rs", "bs", "nz", "be", "za", "co", "no", "by", "am", "my", "bd", "cl", "mx", "id", "th",
                    "bh", "cm", "ge", "gh", "is", "ke", "md", "ng", "om", "tj", "ua", "uz", "zw", "fj", "gt", "kh", "ph", "vn",
                    "ae", "bd", "co", "sa", "bb", "il", "kz", "la", "np", "sg", "ug", "it", "eg", "ci", "sn", "zm"
                };
            }
        }

        public static List<string> CommercialAddressCountriesTest
        {
            get
            {
                return new List<string>
                {
                    "de", "gb", "tr", "tw", "in", "br", "rs", "bs", "nz", "be", "za", "co", "no", "by", "am", "my", "at", "bd", "cl", "mx", "id", "th",
                    "bh", "cm", "ge", "gh", "is", "ke", "md", "ng", "om", "tj", "ua", "uz", "zw", "fj", "gt", "kh", "ph", "vn",
                    "ae", "bd", "co", "sa", "sv", "bb", "il", "kz", "la", "np", "sg", "ug", "it", "eg", "ci", "sn", "zm"
                };
            }
        }

        public static List<string> AzureTaxCountriesTest
        {
            get
            {
                return new List<string>
                {
                    "de", "gb", "tr", "tw", "in", "br", "rs", "bs", "nz", "be", "za", "co", "no", "by", "am", "my", "bd", "cl", "mx", "id", "th", "it", "eg"
                };
            }
        }

        public static List<string> ProfileConsumerPartners
        {
            get
            {
                return new List<string>
                {
                    "webblends", "oxodime", "oxowebdirect", "defaulttemplate", PartnerNames.OfficeSMB
                };
            }
        }

        public static List<string> TaxIdConsumerPartners
        {
            get
            {
                return new List<string>
                    {
                        "bing", "cart", "default", "officeoobe", "oxooobe", "smboobe", "wallet", "webblends", "webblends_inline", "webpay", "xbox", "amcweb", "amcxbox", "azure", "azuresignup", "azureibiza", "oxodime", "oxowebdirect"
                    };
            }
        }

        public static List<string> MSRewardsPartners
        {
            get
            {
                return new List<string>
                    {
                        "windowsstore"
                    };
            }
        }

        public static List<string> CommercialPartners
        {
            get
            {
                return new List<string>
                {
                    PartnerNames.Commercialstores, PartnerNames.DefaultTemplate
                };
            }
        }

        public static List<string> VirtualFamilyPartners
        {
            get
            {
                return new List<string>
                {
                    "commercialstores", "officesmb"
                };
            }
        }

        public static List<string> AzurePartners
        {
            get
            {
                return new List<string>
                    {
                        "azure",
                        "azuresignup",
                        "azureibiza"
                    };
            }
        }

        public static List<string> VirtualPaymentMethods
        {
            get
            {
                return new List<string>
                    {
                        "invoice_check", "invoice_basic", "alipay", "unionpay"
                    };
            }
        }

        public static bool IsXboxNativePartner(string partnerName)
        {
            return string.Equals(partnerName, PartnerNames.Storify, StringComparison.InvariantCultureIgnoreCase)
                || string.Equals(partnerName, PartnerNames.XboxSubs, StringComparison.InvariantCultureIgnoreCase)
                || string.Equals(partnerName, PartnerNames.XboxSettings, StringComparison.InvariantCultureIgnoreCase)
                || string.Equals(partnerName, PartnerNames.Saturn, StringComparison.InvariantCultureIgnoreCase)
                || string.Equals(partnerName, PartnerNames.Xbet, StringComparison.InvariantCultureIgnoreCase)
                || string.Equals(partnerName, PartnerNames.WebblendsXbox, StringComparison.InvariantCultureIgnoreCase)
                || string.Equals(partnerName, PartnerNames.CartXbox, StringComparison.InvariantCultureIgnoreCase)
                || string.Equals(partnerName, PartnerNames.XboxCardApp, StringComparison.InvariantCultureIgnoreCase);
        }

        public static class FlightNames
        {
            public const string IndiaUPIEnable = "IndiaUPIEnable";
            public const string PxEnableUpi = "PxEnableUpi";
            public const string PXEnableRedeemCSVFlow = "PXEnableRedeemCSVFlow";
            public const string PxCommercialEnableUpi = "PxCommercialEnableUpi";
            public const string Vnext = "vnext";
            public const string IndiaTokenizationMessage = "IndiaTokenizationMessage";
            public const string IndiaTokenizationConsentCapture = "IndiaTokenizationConsentCapture";
            public const string IndiaExpiryGroupDelete = "IndiaExpiryGroupDelete";
            public const string EnableIndiaTokenExpiryDetails = "EnableIndiaTokenExpiryDetails";
            public const string PXEnableRupayForIN = "PXEnableRupayForIN";
            public const string IndiaRupayEnable = "IndiaRupayEnable";
            public const string PXPxEnableRiskEligibilityCheck = "PXPxEnableRiskEligibilityCheck";
            public const string IndiaCvvChallengeExpiryGroupDelete = "IndiaCvvChallengeExpiryGroupDelete";
            public const string PXEnableModernIdealPayment = "PXEnableModernIdealPayment";
        }

        internal static class PaymentMethodFamilyNames
        {
            public const string CreditCard = "credit_card";
            public const string MobileBillingNonSim = "mobile_billing_non_sim";
            public const string Ewallet = "ewallet";
            public const string DirectDebit = "direct_debit";
            public const string Virtual = "virtual";
        }

        internal static class PaymentMethodTypeNames
        {
            public const string AlipayBillingAgreement = "alipay_billing_agreement";
            public const string Paypal = "paypal";
            public const string Kakaopay = "kakaopay";
            public const string Venmo = "venmo";
            public const string Sepa = "sepa";
            public const string Ach = "ach";
            public const string MasterCard = "mc";
            public const string Paypay = "paypay";
            public const string AlipayHK = "alipayhk";
            public const string GCash = "gcash";
            public const string TrueMoney = "truemoney";
            public const string TouchNGo = "touchngo";
            public const string Visa = "visa";
            public const string Amex = "amex";
            public const string MC = "mc";
            public const string Discover = "discover";
            public const string JCB = "jcb";
            public const string ELO = "elo";
            public const string Verve = "verve";
            public const string Hipercard = "hipercard";
            public const string UnionPayCreditCard = "unionpay_creditcard";
            public const string UnionPayDebitCard = "unionpay_debitcard";
        }

        internal static class ScenarioNames
        {
            internal const string TwoColumns = "twoColumns";
            internal const string MonetaryCommit = "monetaryCommit";
            internal const string MonetaryCommitModernAccounts = "monetaryCommitModernAccounts";
            internal const string DepartmentalPurchase = "departmentalPurchase";
            internal const string WithCountryDropdown = "withCountryDropdown";
            internal const string EligiblePI = "eligiblePI";
            internal const string DisplayOptionalFields = "displayOptionalFields";
            internal const string XboxCoBrandedCard = "XboxCoBrandedCard";
            internal const string ModernAccount = "modernAccount";
        }

        internal static class ScenariosForBillingGroup
        {
            internal const string BillingGroupPONumber = "billingGroupPONumber";
        }

        internal static class ScenariosForAddress
        {
            internal const string CommercialHardware = "commercialhardware";
            internal const string CreateBillingAccount = "createbillingaccount";
        }

        internal static class ScenariosForPaymentMethodDescription
        {
            internal const string Rs5 = "rs5";
            internal const string IncludeCvv = "includecvv";
            internal const string WithNewAddress = "withnewaddress";
            internal const string FixedCountrySelection = "fixedCountrySelection";
            internal const string HasSubsOrPreOrders = "hasSubsOrPreOrders";
            internal const string Roobe = "roobe";
            internal const string PayNow = "PayNow";
            internal const string ChangePI = "ChangePI";
            internal const string PaypalQrCode = "paypalQrcode";
        }

        internal static class TypesForBillingGroup
        {
            internal const string Lightweight = "lightweight";
            internal const string LightweightV7 = "lightweightv7";
        }

        internal static class ScenariosForTaxId
        {
            internal const string ConsumerTaxId = "consumer_tax_id";
            internal const string CommercialTaxId = "commercial_tax_id";
        }

        internal static class PidlOperationType
        {
            internal const string Add = "add";
            internal const string Update = "update";
            internal const string Show = "show";
            internal const string Select = "select";
            internal const string SelectInstance = "selectinstance";
            internal const string RenderPidlPage = "RenderPidlPage";
            internal const string Replace = "replace";
            internal const string SearchTransactions = "searchTransactions";
            internal const string Apply = "apply";
            internal const string Delete = "delete";
            internal const string ExpressCheckout = "ExpressCheckout";
        }

        internal static class UserTypes
        {
            public const string Anonymous = "anonymous";
            public const string UserMe = "me";
            public const string UserMyOrg = "my-org";
        }

        internal static class ProfileType
        {
            public const string Consumer = "consumer";
            public const string Employee = "employee";
            public const string Organization = "organization";
            public const string Legal = "legalentity";
            public const string ConsumerV3 = "consumerV3";
        }

        internal static class AddressTypeNames
        {
            public const string Billing = "billing";
            public const string Shipping = "shipping";
            public const string BillingGroup = "billinggroup";
            public const string Organization = "organization";
            public const string Individual = "individual";
            public const string HapiServiceUsageAddress = "hapiServiceUsageAddress";
            public const string HapiV1SoldToOrganization = "hapiV1SoldToOrganization";
            public const string HapiV1ShipToOrganization = "hapiV1ShipToOrganization";
            public const string HapiV1BillToOrganization = "hapiV1BillToOrganization";
            public const string HapiV1SoldToIndividual = "hapiV1SoldToIndividual";
            public const string HapiV1 = "hapiV1";
            public const string HapiV1ShipToIndividual = "hapiV1ShipToIndividual";
            public const string HapiV1BillToIndividual = "hapiV1BillToIndividual";
            public const string OrgAddress = "orgAddress";
        }

        internal static class TemplateNames
        {
            public const string OnePage = "onepage";
            public const string TwoPage = "twopage";
            public const string SelectPMButtonList = "selectpmbuttonlist";
            public const string SelectPMDropDown = "selectpmdropdown";
            public const string SelectPMRadioButtonList = "selectpmradiobuttonlist";
            public const string ListPiRadioButton = "listpiradiobutton";
            public const string DefaultTemplate = "defaulttemplate";
        }

        internal static class PartnerNames
        {
            public const string Commercialstores = "commercialstores";
            public const string Cart = "cart";
            public const string ConsumerSupport = "consumersupport";
            public const string Bing = "bing";
            public const string BingTravel = "bingtravel";
            public const string DefaultPartner = "default";
            public const string Webblends = "webblends";
            public const string Wallet = "wallet";
            public const string WebblendsInline = "webblends_inline";
            public const string Xbox = "xbox";
            public const string XboxSubs = "xboxsubs";
            public const string XboxSettings = "xboxsettings";
            public const string Saturn = "saturn";
            public const string Storify = "storify";
            public const string WebPay = "webpay";
            public const string Amc = "amc";
            public const string AmcWeb = "amcweb";
            public const string AmcXbox = "amcxbox";
            public const string OfficeOobe = "officeoobe";
            public const string OfficeOobeInApp = "officeoobeinapp";
            public const string OXOOobe = "oxooobe";
            public const string SmbOobe = "smboobe";
            public const string Azure = "azure";
            public const string AzureSignup = "azuresignup";
            public const string AzureIbiza = "azureibiza";
            public const string Mseg = "mseg";
            public const string AppSource = "appsource";
            public const string OneDrive = "onedrive";
            public const string OxoDIME = "oxodime";
            public const string OxoWebDirect = "oxowebdirect";
            public const string NorthstarWeb = "northstarweb";
            public const string MSTeams = "msteams";
            public const string Payin = "payin";
            public const string SetupOffice = "setupoffice";
            public const string XboxNative = "xboxnative";
            public const string XboxCardApp = "xboxcardapp";
            public const string Xbet = "xbet";
            public const string WebblendsXbox = "webblendsxbox";
            public const string CartXbox = "cartxbox";
            public const string XboxWeb = "xboxweb";
            public const string OfficeSMB = "officesmb";
            public const string WindowsSettings = "windowssettings";
            public const string WindowsStore = "windowsstore";
            public const string DefaultTemplate = "defaulttemplate";
            public const string SetupOfficeSdx = "setupofficesdx";
            public const string Macmanage = "macmanage";
        }

        internal static class DiffTest
        {
            //// Used to build a paths for KnownDiffs (See KnownDiffsConfig.cs)
            //// Identifies any value for a given parameter
            //// i.e. for sets where Country = Any, Language = Any, partner = "webblends" apply KnownDiffs
            public const string Any = "_Any_";
            //// Used by SkipPidlCombinations to identify pidls that are missing a parameter
            //// i.e. family: "virtual" type: NotSent
            public const string NotSent = "_NotSent_";

            public static Dictionary<string, Dictionary<string, List<string>>> PaymentMethodsInCountries
            {
                get
                {
                    return new Dictionary<string, Dictionary<string, List<string>>>(StringComparer.OrdinalIgnoreCase)
                    {
                        {
                            "us", new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                            {
                                { "credit_card", new List<string>() { "visa", "amex", "mc", "discover" } },
                                { "ewallet", new List<string>() { "paypal" } },
                                { "mobile_billing_non_sim", new List<string>() { "att-us-nonsim", "vzw-us-nonsim" } },
                                { "virtual", new List<string>() { "legacy_invoice" } },
                            }
                        },
                        {
                            "id", new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                            {
                                { "credit_card", new List<string>() { "visa", "amex", "mc", "jcb" } }
                            }
                        },
                        {
                            "jp", new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                            {
                                { "credit_card", new List<string>() { "visa", "amex", "mc", "jcb" } },
                                { "ewallet", new List<string>() { "paypal" } }
                            }
                        },
                        {
                            "ph", new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                            {
                                { "credit_card", new List<string>() { "visa", "amex", "mc", "jcb" } }
                            }
                        },
                        {
                            "tw", new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                            {
                                { "credit_card", new List<string>() { "visa", "mc", "jcb" } }
                            }
                        },
                        {
                            "th", new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                            {
                                { "credit_card", new List<string>() { "visa", "amex", "mc", "jcb" } }
                            }
                        },
                        {
                            "vn", new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                            {
                                { "credit_card", new List<string>() { "visa", "amex", "mc", "jcb" } }
                            }
                        },
                        {
                            "cn", new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                            {
                                { "credit_card", new List<string>() { "unionpay_creditcard", "unionpay_debitcard" } },
                                { "ewallet", new List<string>() { "alipay_billing_agreement" } },
                                { "mobile_billing_non_sim", new List<string>() { "cmc-cn-nonsim" } }
                            }
                        },
                        {
                            "fi", new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                            {
                                { "credit_card", new List<string>() { "visa", "amex", "mc" } },
                                { "ewallet", new List<string>() { "paypal" } },
                                //// direct_debit is disabled for FI by PIMS
                                ////{ "direct_debit", new List<string>() { "sepa" } },
                                { "mobile_billing_non_sim", new List<string>() { "dna-fi-nonsim", "eli-fi-nonsim", "son-fi-nonsim" } }
                            }
                        },
                        {
                            "cz", new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                            {
                                { "credit_card", new List<string>() { "visa", "amex", "mc" } },
                                { "ewallet", new List<string>() { "paypal" } },
                                { "mobile_billing_non_sim", new List<string>() { "tmo-cz-nonsim", "vod-cz-nonsim", "o2o-cz-nonsim" } }
                            }
                        },
                        {
                            "gb", new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                            {
                                { "credit_card", new List<string>() { "visa", "amex", "mc" } },
                                { "ewallet", new List<string>() { "paypal" } },
                                { "mobile_billing_non_sim", new List<string>() { "hut-gb-nonsim", "o2o-gb-nonsim", "org-gb-nonsim", "tmo-gb-nonsim", "vod-gb-nonsim" } }
                            }
                        },
                        {
                            "de", new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                            {
                                { "credit_card", new List<string>() { "visa", "amex", "mc" } },
                                { "ewallet", new List<string>() { "paypal" } },
                                { "direct_debit", new List<string>() { "sepa" } },
                                { "mobile_billing_non_sim", new List<string>() { "o2o-de-nonsim", "tmo-de-nonsim", "vod-de-nonsim" } },
                                ////in DE klarna is supposed to have gender field
                                { "invoice_credit", new List<string>() { "klarna" } }
                            }
                        },
                        {
                            "br", new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                            {
                                { "credit_card", new List<string>() { "visa", "amex", "mc", "hipercard", "elo" } },
                                { "mobile_billing_non_sim", new List<string>() { "cla-br-nonsim", "viv-br-nonsim" } }
                            }
                        },
                        {
                            "nl", new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                            {
                                { "credit_card", new List<string>() { "visa", "amex", "mc" } },
                                { "ewallet", new List<string>() { "paypal" } },
                                { "direct_debit", new List<string>() { "sepa", "ideal_billing_agreement" } },
                                { "mobile_billing_non_sim", new List<string>() { "tmo-nl-nonsim", "vod-nl-nonsim", "nlk-nl-nonsim", "tlf-nl-nonsim" } },
                                { "invoice_credit", new List<string>() { "klarna" } }
                            }
                        },
                        {
                            "hk", new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                            {
                                { "credit_card", new List<string>() { "visa", "amex", "mc" } },
                                { "ewallet", new List<string>() { "paypal", "alipay_billing_agreement" } },
                                { "mobile_billing_non_sim", new List<string>() { "hut-hk-nonsim" } }
                            }
                        },
                        {
                            "ru", new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                            {
                                { "credit_card", new List<string>() { "visa", "amex", "mc" } },
                                { "mobile_billing_non_sim", new List<string>() { "bee-ru-nonsim", "meg-ru-nonsim", "mts-ru-nonsim", "tl2-ru-nonsim" } }
                            }
                        },
                        {
                            "tr", new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                            {
                                { "credit_card", new List<string>() { "visa", "amex", "mc" } }
                            }
                        },
                        {
                            "in", new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                            {
                                { "credit_card", new List<string>() { "visa", "mc" } }
                            }
                        },
                        {
                            "se", new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                            {
                                { "credit_card", new List<string>() { "visa", "amex", "mc" } }
                            }
                        },
                        {
                            "xk", new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                            {
                                { "credit_card", new List<string>() { "visa", "amex", "mc" } }
                            }
                        },
                        {
                            "ng", new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                            {
                                { "credit_card", new List<string>() { "visa", "amex", "mc", "verve" } }
                            }
                        }
                    };
                }
            }

            //// Languages for which the comparision has to be conducted
            public static List<string> Languages
            {
                get
                {
                    return new List<string>
                    {
                        "en-US"
                    };
                }
            }

            public static List<string> Operations
            {
                get
                {
                    return new List<string>
                    {
                        "add", "update"
                    };
                }
            }

            public static List<string> MSRewardsOperations
            {
                get
                {
                    return new List<string>
                    {
                        "select", "redeem"
                    };
                }
            }

            public static List<string> Partners
            {
                get
                {
                    return new List<string>
                    {
                        "onepage", "twopage", "defaulttemplate", "officesmb", "commercialstores", "bing", "bingtravel", "cart", "consumersupport", "default", "officeoobe", "oxooobe", "smboobe", "wallet", "webblends", "webblends_inline", "webpay", "xbox", "amcweb", "amcxbox", "azure", "azuresignup", "azureibiza", "appsource",  "xboxweb", "ggpdeds", "onedrive", "oxodime", "oxowebdirect", "payin", "setupoffice", "setupofficesdx", "commercialsupport", "storeoffice", "northstarweb", "storify", "xboxsubs", "xboxsettings", "saturn"
                    };
                }
            }

            public static string[][] AllowedPaymentMethods
            {
                get
                {
                    return new string[][]
                    {
                        // None
                        new string[] { },

                        // Only one family
                        new string[] { "credit_card" },

                        // More than one family
                        new string[] { "credit_card", "ewallet", "mobile_billing_non_sim" },

                        // Only one family.type
                        new string[] { "ewallet.paypal" },

                        // More than one family.types
                        new string[] { "ewallet.paypal", "ewallet.stored_value", "ewallet.venmo", "direct_debit.ach", "direct_debit.sepa" },

                        // More than one family and family.types
                        new string[] { "credit_card", "ewallet", "mobile_billing_non_sim", "ewallet.paypal", "ewallet.stored_value", "ewallet.venmo", "direct_debit.ach", "direct_debit.sepa" },
                    };
                }
            }

            public static string[][] FilterExclutionTags
            {
                get
                {
                    return new string[][]
                    {
                        // None
                        new string[] { },

                        // One
                        new string[] { "LegacySubscriptions" },

                        // More than one
                        new string[] { "LegacySubscriptions", "StoredValue" },

                        // Most
                        new string[] { "LegacySubscriptions", "mobi_csv", "MsReading", "NonMobileOperatorContent", "O365Subscription", "PassPerpetual", "Perpetual", "PhysicalGoods", "StoredValue", "Subscriptions", "XBOXLegacySubscriptions" },
                    };
                }
            }

            public static List<string> FilterChargeThreshold
            {
                get
                {
                    return new List<string>
                    {
                        "0.01", "49.99", "50.00", "1000.00"
                    };
                }
            }

            //// Not Allowed List for incorrect or unwanted url parameter combinations
            //// new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "ewallet", type: NotSent), --> skips ewallet family, all partners w/no type
            //// new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "ewallet"), --> skips ewallet family and all its types, all partners
            //// new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "ewallet", type: NotSent, partner: "amcweb), --> skips ewallet family w/no type for amcweb partner

            public static List<Diff.TestRequestRelativePath> SkipPidlCombinations
            {
                get
                {
                    return new List<Diff.TestRequestRelativePath>
                    {
                        //// Applicable for all partners
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", type: "ideal_billing_agreement", operation: "update"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "ewallet", type: NotSent),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "ewallet", type: "stored_value", operation: "update"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "ewallet", type: "tvpay"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "ewallet", type: "paypal", operation: "update"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "ewallet", type: "monetary_commitment"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "invoice_credit", operation: "update"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "mobile_billing_non_sim", operation: "update"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "mobile_carrier_billing"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "online_bank_transfer"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "offline_bank_transfer"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "virtual", type: NotSent),

                        //// Special nonsim cases
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "mobile_billing_non_sim", type: NotSent, country: "jp"),

                        //// Cart
                        //// Only test ACH and SEPA
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", type: "hapiServiceUsageAddress", partner: "cart"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", type: NotSent, partner: "cart"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", operation: "update", partner: "cart"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "ewallet", type: "alipay_billing_agreement", partner: "cart"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "mobile_billing_non_sim", partner: "cart"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_creditcard", partner: "cart"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_debitcard", partner: "cart"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "virtual", type: "legacy_invoice", partner: "cart"),

                        //// ConsumerSupport
                        //// paymentMethod: family: virtual, type: legacy_invoice and operation: add, update, not valid
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "virtual", type: "legacy_invoice", partner: "consumersupport"),

                        //// Appsource
                        //// Only test Credit Card and Shipping Address
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", type: "hapiServiceUsageAddress", partner: "appsource"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", type: "billing", partner: "appsource"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", partner: "appsource"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "ewallet", type: "alipay_billing_agreement", partner: "appsource"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "mobile_billing_non_sim", partner: "appsource"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "amex", country: "in", operation: "add", partner: "appsource"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_creditcard", partner: "appsource"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_debitcard", partner: "appsource"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", operation: "update", partner: "appsource"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "invoice_credit", partner: "appsource"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", operation: "select", partner: "appsource"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", completePrerequisites: "true", partner: "appsource"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "virtual", type: "legacy_invoice", partner: "appsource"),

                        //// Webpay
                        //// Only test Credit Card and Shipping Address
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", type: "hapiServiceUsageAddress", partner: "webpay"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", type: "billing", partner: "webpay"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", partner: "webpay"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "ewallet", type: "alipay_billing_agreement", partner: "webpay"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "mobile_billing_non_sim", partner: "webpay"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "hipercard", partner: "webpay"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_creditcard", partner: "webpay"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_debitcard", partner: "webpay"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "elo", country: "br", partner: "webpay"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "invoice_credit", partner: "webpay"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", operation: "select", partner: "webpay"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", completePrerequisites: "true", partner: "webpay"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "virtual", type: "legacy_invoice", partner: "webpay"),

                        //// Webblends
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", type: "hapiServiceUsageAddress", partner: "webblends"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", type: "shipping", partner: "webblends"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", type: NotSent, partner: "webblends"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", operation: "update", partner: "webblends"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "hipercard", partner: "webblends"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "virtual", type: "legacy_invoice", partner: "webblends"),

                        //// Storify
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", type: "hapiServiceUsageAddress", partner: "storify"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", type: "shipping", partner: "storify"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", type: NotSent, partner: "storify"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", operation: "update", partner: "storify"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "hipercard", partner: "storify"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "virtual", type: "legacy_invoice", partner: "storify"),

                        //// XboxSubs
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", type: "hapiServiceUsageAddress", partner: "xboxsubs"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", type: "shipping", partner: "xboxsubs"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", type: NotSent, partner: "xboxsubs"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", operation: "update", partner: "xboxsubs"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "hipercard", partner: "xboxsubs"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "virtual", type: "legacy_invoice", partner: "xboxsubs"),

                        //// XbosSettings
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", type: "hapiServiceUsageAddress", partner: "xboxsettings"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", type: "shipping", partner: "xboxsettings"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", type: NotSent, partner: "xboxsettings"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", operation: "update", partner: "xboxsettings"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "hipercard", partner: "xboxsettings"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "virtual", type: "legacy_invoice", partner: "xboxsettings"),

                        //// Saturn
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", type: "hapiServiceUsageAddress", partner: "saturn"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", type: "shipping", partner: "saturn"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", type: NotSent, partner: "saturn"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", operation: "update", partner: "saturn"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "hipercard", partner: "saturn"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "virtual", type: "legacy_invoice", partner: "saturn"),

                        //// OXODIME
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", type: "hapiServiceUsageAddress", partner: "oxodime"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", type: "shipping", partner: "oxodime"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", type: NotSent, partner: "oxodime"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", operation: "update", partner: "oxodime"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "hipercard", partner: "oxodime"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "virtual", type: "legacy_invoice", partner: "oxodime"),

                        //// OXOWebDirect
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", type: "hapiServiceUsageAddress", partner: "oxowebdirect"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", type: "shipping", partner: "oxowebdirect"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", type: NotSent, partner: "oxowebdirect"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", operation: "update", partner: "oxowebdirect"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "virtual", type: "legacy_invoice", partner: "oxowebdirect"),

                        //// Webblends_inline
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", type: "hapiServiceUsageAddress", partner: "webblends_inline"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", type: "shipping", partner: "webblends_inline"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "ewallet", type: "alipay_billing_agreement", partner: "webblends_inline"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", type: NotSent, partner: "webblends_inline"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", operation: "update", partner: "webblends_inline"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "hipercard", partner: "webblends_inline"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_creditcard", partner: "webblends_inline"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_debitcard", partner: "webblends_inline"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "invoice_credit", partner: "webblends_inline"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "mobile_billing_non_sim", partner: "webblends_inline"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "virtual", type: "legacy_invoice", partner: "webblends_inline"),

                        //// Xbox
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", type: "hapiServiceUsageAddress", partner: "xbox"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", type: NotSent, partner: "xbox"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", operation: "update", partner: "xbox"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", country: "jp", partner: "xbox"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", country: "cn", partner: "xbox"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", country: "fi", partner: "xbox"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", country: "cz", partner: "xbox"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", country: "gb", partner: "xbox"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", country: "de", partner: "xbox"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", country: "br", partner: "xbox"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", country: "nl", partner: "xbox"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", country: "hk", partner: "xbox"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", country: "ru", partner: "xbox"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", type: NotSent, partner: "xbox"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", type: "sepa", partner: "xbox"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", operation: "update", partner: "xbox"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "hipercard", partner: "xbox"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "virtual", type: "legacy_invoice", partner: "xbox"),

                        //// Wallet
                        //// Only test Credit Card and Profile
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", type: "hapiServiceUsageAddress", partner: "wallet"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", partner: "wallet"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", partner: "wallet"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "ewallet", partner: "wallet"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "mobile_billing_non_sim", partner: "wallet"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: NotSent, type: NotSent, partner: "wallet"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", partner: "wallet"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "invoice_credit", partner: "wallet"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", operation: "select", partner: "wallet"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "virtual", type: "legacy_invoice", partner: "wallet"),

                        //// Officeoobe
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", type: "hapiServiceUsageAddress", partner: "officeoobe"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", type: "shipping", partner: "officeoobe"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", type: NotSent, partner: "officeoobe"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", partner: "officeoobe"),
                        ////JP credit_card.jcb and BR credit_card.hipercard not enabled PR#17869450
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "hipercard", partner: "officeoobe"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "jcb", partner: "officeoobe"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", country: "jp", partner: "officeoobe"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_creditcard", partner: "officeoobe"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_debitcard", partner: "officeoobe"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "elo", country: "br", partner: "officeoobe"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "invoice_credit", partner: "officeoobe"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", operation: "select", partner: "officeoobe"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "mobile_billing_non_sim", partner: "officeoobe"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "virtual", type: "legacy_invoice", partner: "officeoobe"),

                        //// OxoOobe
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", type: "hapiServiceUsageAddress", partner: "oxooobe"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", type: "shipping", partner: "oxooobe"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", type: NotSent, partner: "oxooobe"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", partner: "oxooobe"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_creditcard", partner: "oxooobe"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_debitcard", partner: "oxooobe"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "elo", country: "br", partner: "oxooobe"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "invoice_credit", partner: "oxooobe"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", operation: "select", partner: "oxooobe"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "mobile_billing_non_sim", partner: "oxooobe"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "virtual", type: "legacy_invoice", partner: "oxooobe"),

                        //// Smboobe
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", type: "hapiServiceUsageAddress", partner: "smboobe"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", type: "shipping", partner: "smboobe"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", type: NotSent, partner: "smboobe"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", partner: "smboobe"),
                        ////JP credit_card.jcb and BR credit_card.hipercard not enabled PR#17869450
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "hipercard", partner: "smboobe"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "jcb", partner: "smboobe"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", country: "jp", partner: "smboobe"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_creditcard", partner: "smboobe"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_debitcard", partner: "smboobe"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "elo", country: "br", partner: "smboobe"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "invoice_credit", partner: "smboobe"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", operation: "select", partner: "smboobe"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "mobile_billing_non_sim", partner: "smboobe"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "virtual", type: "legacy_invoice", partner: "smboobe"),

                        //// Default
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", type: "hapiServiceUsageAddress", partner: "default"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", type: NotSent, partner: "default"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", type: "ach", partner: "default"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", type: "sepa", partner: "default"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "ewallet", type: "paypal", partner: "default"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "ewallet", type: "alipay_billing_agreement", partner: "default"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "mobile_billing_non_sim", type: NotSent, partner: "default"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: NotSent, type: NotSent, partner: "default"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "hipercard", partner: "default"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "invoice_credit", partner: "default"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", operation: "select", partner: "default"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "virtual", type: "legacy_invoice", partner: "default"),

                        //// Bing
                        //// Only test Credit Card
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", partner: "bing"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", partner: "bing"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "ewallet", type: "alipay_billing_agreement", partner: "bing"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "mobile_billing_non_sim", partner: "bing"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: NotSent, type: NotSent, partner: "bing"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "amex", country: "in", partner: "bing"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "hipercard", partner: "bing"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "invoice_credit", partner: "bing"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", operation: "select", partner: "bing"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", completePrerequisites: "true", partner: "bing"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_creditcard", partner: "bing"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_debitcard", partner: "bing"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "elo", country: "br", partner: "bing"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "virtual", type: "legacy_invoice", partner: "bing"),

                        //// SetupOffice
                        //// Verify Credit Card, Paypal, ACH, SEPA, SelectPM, Complete Prerequisites
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", partner: "bingtravel"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "hipercard", partner: "bingtravel"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_creditcard", partner: "bingtravel"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_debitcard", partner: "bingtravel"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "elo", country: "br", partner: "bingtravel"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", type: "ideal_billing_agreement", partner: "bingtravel"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "ewallet", type: "alipay_billing_agreement", partner: "bingtravel"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "invoice_credit", partner: "bingtravel"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "mobile_billing_non_sim", partner: "bingtravel"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "virtual", type: "legacy_invoice", partner: "bingtravel"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "taxIdDescriptions", partner: "bingtravel"),

                        ////Commercialstores
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", operation: "select", partner: "commercialstores"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", completePrerequisites: "true", partner: "commercialstores"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "amex", country: "in", partner: "commercialstores"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "hipercard", partner: "commercialstores"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_creditcard", partner: "commercialstores"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_debitcard", partner: "commercialstores"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "elo", country: "br", partner: "commercialstores"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "ewallet", type: "alipay_billing_agreement", partner: "commercialstores"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "mobile_billing_non_sim", partner: "commercialstores"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "invoice_credit", partner: "commercialstores"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "taxIdDescriptions", scenario: "consumer_tax_id", partner: "commercialstores"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "virtual", type: "legacy_invoice", partner: "commercialstores"),

                        ////onepage
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", operation: "select", partner: "onepage"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "hipercard", partner: "onepage"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_creditcard", partner: "onepage"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_debitcard", partner: "onepage"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "elo", country: "br", partner: "onepage"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "ewallet", type: "alipay_billing_agreement", partner: "onepage"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "mobile_billing_non_sim", partner: "onepage"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "invoice_credit", partner: "onepage"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "taxIdDescriptions", scenario: "consumer_tax_id", partner: "onepage"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "virtual", type: "legacy_invoice", partner: "onepage"),

                        ////twopage
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", operation: "select", partner: "twopage"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "hipercard", partner: "twopage"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_creditcard", partner: "twopage"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_debitcard", partner: "twopage"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "elo", country: "br", partner: "twopage"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "ewallet", type: "alipay_billing_agreement", partner: "twopage"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "mobile_billing_non_sim", partner: "twopage"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "invoice_credit", partner: "twopage"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "taxIdDescriptions", scenario: "consumer_tax_id", partner: "twopage"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "virtual", type: "legacy_invoice", partner: "twopage"),

                        ////DefaultTemplate
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "hipercard", partner: "defaulttemplate"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "elo", country: "br", partner: "defaulttemplate"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "invoice_credit", partner: "defaulttemplate"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "taxIdDescriptions", scenario: "consumer_tax_id", partner: "defaulttemplate"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "virtual", type: "legacy_invoice", partner: "defaulttemplate"),

                        // Officesmb
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", type: NotSent, partner: PartnerNames.OfficeSMB),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", type: "ach", partner: PartnerNames.OfficeSMB),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "virtual", type: "legacy_invoice", partner: PartnerNames.OfficeSMB),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", operation: PidlOperationType.Select, partner: PartnerNames.OfficeSMB),

                        ////AMCweb
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", type: "hapiServiceUsageAddress", partner: "amcweb"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", partner: "amcweb"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", operation: "select", partner: "amcweb"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "virtual", type: "legacy_invoice", partner: "amcweb"),

                        ////AMCXbox
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", partner: "amcxbox"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", operation: "select", partner: "amcxbox"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", completePrerequisites: "true", partner: "amcxbox"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", operation: "sepa", partner: "amcxbox"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", operation: "ideal_billing_agreement", partner: "amcxbox"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_creditcard", partner: "amcxbox"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_debitcard", partner: "amcxbox"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "virtual", type: "legacy_invoice", partner: "amcxbox"),

                        //// Azure
                        //// Only test Credit Card and Legacy Invoice
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", type: "hapiServiceUsageAddress", partner: "azure"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", partner: "azure"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "ewallet", partner: "azure"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "mobile_billing_non_sim", partner: "azure"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "amex", country: "in", partner: "azure"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "hipercard", partner: "azure"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_creditcard", partner: "azure"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_debitcard", partner: "azure"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "elo", country: "br", partner: "azure"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "invoice_credit", partner: "azure"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", operation: "select", partner: "azure"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "profileDescriptions", partner: "azure"),

                        //// AzureSignup
                        //// Only test Credit Card and Legacy Invoice
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", type: "hapiServiceUsageAddress", partner: "azuresignup"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", partner: "azuresignup"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "ewallet", partner: "azuresignup"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "mobile_billing_non_sim", partner: "azuresignup"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "amex", country: "in", partner: "azuresignup"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "hipercard", partner: "azuresignup"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_creditcard", partner: "azuresignup"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_debitcard", partner: "azuresignup"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "elo", country: "br", partner: "azuresignup"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "invoice_credit", partner: "azuresignup"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", operation: "select", partner: "azuresignup"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "profileDescriptions", partner: "azuresignup"),

                        //// AzureIbiza
                        //// Only test Credit Card and Legacy Invoice
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", type: "hapiServiceUsageAddress", partner: "azureibiza"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", partner: "azureibiza"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "ewallet", partner: "azureibiza"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "mobile_billing_non_sim", partner: "azureibiza"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "amex", country: "in", partner: "azureibiza"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "hipercard", partner: "azureibiza"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_creditcard", partner: "azureibiza"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_debitcard", partner: "azureibiza"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "elo", country: "br", partner: "azureibiza"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "invoice_credit", partner: "azureibiza"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", operation: "select", partner: "azureibiza"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "profileDescriptions", partner: "azureibiza"),

                        //// Xboxweb
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", partner: "xboxweb"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "taxIdDescriptions", partner: "xboxweb"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", operation: "select", partner: "xboxweb"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "virtual", type: "legacy_invoice", partner: "xboxweb"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "invoice_credit", partner: "xboxweb"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", partner: "xboxweb"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "ewallet", partner: "xboxweb"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "mobile_billing_non_sim", partner: "xboxweb"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "hipercard", partner: "xboxweb"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_creditcard", partner: "xboxweb"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_debitcard", partner: "xboxweb"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "elo", partner: "xboxweb"),

                        //// GGPDEDS
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", partner: "ggpdeds"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "taxIdDescriptions", partner: "ggpdeds"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", operation: "select", partner: "ggpdeds"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "virtual", type: "legacy_invoice", partner: "ggpdeds"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "invoice_credit", partner: "ggpdeds"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", partner: "ggpdeds"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "ewallet", partner: "ggpdeds"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "mobile_billing_non_sim", partner: "ggpdeds"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "hipercard", partner: "ggpdeds"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_creditcard", partner: "ggpdeds"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_debitcard", partner: "ggpdeds"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "elo", partner: "ggpdeds"),

                        //// OneDrive
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", type: "shipping", partner: "onedrive"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "taxIdDescriptions", partner: "onedrive"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "virtual", type: "legacy_invoice", partner: "onedrive"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "invoice_credit", partner: "onedrive"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", partner: "onedrive"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "ewallet", type: "alipay_billing_agreement", partner: "onedrive"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "mobile_billing_non_sim", partner: "onedrive"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "hipercard", partner: "onedrive"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_creditcard", partner: "onedrive"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_debitcard", partner: "onedrive"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "elo", partner: "onedrive"),

                        //// Payin
                        //// Only test Credit Card, Paypal, SelectPM
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", partner: "payin"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", partner: "payin"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "ewallet", type: "alipay_billing_agreement", partner: "payin"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "mobile_billing_non_sim", partner: "payin"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", operation: "update", partner: "payin"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "hipercard", partner: "payin"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_creditcard", partner: "payin"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_debitcard", partner: "payin"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "elo", country: "br", partner: "payin"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "invoice_credit", partner: "payin"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", completePrerequisites: "true", partner: "payin"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "virtual", type: "legacy_invoice", partner: "payin"),

                        //// SetupOffice
                        //// Verify Credit Card, Paypal, ACH, SEPA, SelectPM, Complete Prerequisites
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", partner: "setupoffice"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "hipercard", partner: "setupoffice"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_creditcard", partner: "setupoffice"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_debitcard", partner: "setupoffice"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "elo", country: "br", partner: "setupoffice"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", type: "ideal_billing_agreement", partner: "setupoffice"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "ewallet", type: "alipay_billing_agreement", partner: "setupoffice"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "invoice_credit", partner: "setupoffice"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "mobile_billing_non_sim", partner: "setupoffice"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "virtual", type: "legacy_invoice", partner: "setupoffice"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", type: "sepa", country: "nl", partner: "setupoffice", operation: "add"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "taxIdDescriptions", partner: "setupoffice"),

                        //// SetupOfficeSdx
                        //// Verify Credit Card, Paypal, ACH, SEPA, SelectPM, Complete Prerequisites
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", partner: "setupofficesdx"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "hipercard", partner: "setupofficesdx"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_creditcard", partner: "setupofficesdx"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_debitcard", partner: "setupofficesdx"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "elo", country: "br", partner: "setupofficesdx"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", type: "ideal_billing_agreement", partner: "setupofficesdx"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "ewallet", type: "alipay_billing_agreement", partner: "setupofficesdx"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "invoice_credit", partner: "setupofficesdx"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "mobile_billing_non_sim", partner: "setupofficesdx"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "virtual", type: "legacy_invoice", partner: "setupofficesdx"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", type: "sepa", country: "nl", partner: "setupofficesdx", operation: "add"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "taxIdDescriptions", partner: "setupofficesdx"),

                        //// Commercial Support
                        //// Only test Credit Card update
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", partner: "commercialsupport"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", partner: "commercialsupport", operation: "add"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", partner: "commercialsupport", operation: "update"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "ewallet", type: "alipay_billing_agreement", partner: "commercialsupport"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "mobile_billing_non_sim", partner: "commercialsupport"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: NotSent, type: NotSent, partner: "commercialsupport"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "hipercard", partner: "commercialsupport"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "invoice_credit", partner: "commercialsupport"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", operation: "select", partner: "commercialsupport"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", completePrerequisites: "true", partner: "commercialsupport"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_creditcard", partner: "commercialsupport"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_debitcard", partner: "commercialsupport"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "elo", country: "br", partner: "commercialsupport"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", partner: "commercialsupport", operation: "add"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "virtual", type: "legacy_invoice", partner: "commercialsupport"),

                        //// StoreOffice
                        //// Verify (add) Credit Card, Complete Prerequisites
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "addressDescriptions", partner: "storeoffice"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", operation: "select", partner: "storeoffice"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", operation: "update", partner: "storeoffice"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "hipercard", partner: "storeoffice"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_creditcard", partner: "storeoffice"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "unionpay_debitcard", partner: "storeoffice"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "credit_card", type: "elo", country: "br", partner: "storeoffice"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "direct_debit", partner: "storeoffice"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "ewallet", partner: "storeoffice"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "invoice_credit", partner: "storeoffice"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "mobile_billing_non_sim", partner: "storeoffice"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "virtual", type: "legacy_invoice", partner: "storeoffice"),
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "taxIdDescriptions", partner: "storeoffice"),

                        //// Northstarweb
                        new Diff.TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", family: "virtual", type: "legacy_invoice", partner: "northstarweb"),
                    };
                }
            }
        }
    }
}
