// <copyright file="Constants.cs" company="Microsoft">Copyright (c) Microsoft 2016. All rights reserved.</copyright>
namespace PidlTest
{
    internal static class Constants
    {
        internal static class ErrorMessages
        {
            // Format strings that will be used to show errors in command line arguments
            public const string ArgMissingFormat = "Missing {0} argument. Loading value from config";
            public const string ArgValueMissingFormat = "Command line argument {0} should be followed by a value";
            public const string ArgValueInvalidFormat = "Command line argement {0} has an invalid value";
            public const string EnvIncorrectFormat = "{0} environment {1} is not INT, PPE, PROD";
            public const string ConfigValueMissing = "Failed to load the attribute {0} from the config file";
        }

        internal static class Environment
        {
            public const string PaymentMethodCallBase = "https://paymentinstruments-int.mp.microsoft.com";
            public const string INT = "https://pifd.cp.microsoft-int.com/V6.0/";
            public const string PPE = "https://paymentinstruments-int.mp.microsoft.com/V6.0/";
            public const string Prod = "https://paymentinstruments.mp.microsoft.com/V6.0/";
            public const string Feature = "https://st-pifd-prod-wcus.azurewebsites.net/V6.0/";
            public const string Local = "local";
            public const string SelfHost = "SelfHost";
        }

        internal static class PaymentMethodFamilyNames
        {
            public const string MobileBillingNonSim = "mobile_billing_non_sim";
        }

        internal static class UserTypes
        {
            public const string Anonymous = "anonymous";
            public const string UserMe = "me";
            public const string UserMyOrg = "my-org";
        }

        internal static class PartnerNames
        {
            public const string Commercialstores = "commercialstores";
            public const string Cart = "cart";
            public const string Bing = "bing";
            public const string DefaultPartner = "default";
            public const string Webblends = "webblends";
            public const string OxoWebDirect = "oxowebdirect";
            public const string Wallet = "wallet";
            public const string WebblendsInline = "webblends_inline";
            public const string Xbox = "xbox";
            public const string WebPay = "webpay";
            public const string Amc = "amc";
            public const string AmcWeb = "amcweb";
            public const string AmcXbox = "amcxbox";
            public const string OfficeOobe = "officeoobe";
            public const string OXOOobe = "oxooobe";
            public const string OfficeOobeInApp = "officeoobeinapp";
            public const string SmbOobe = "smboobe";
            public const string Azure = "azure";
            public const string Mseg = "mseg";
            public const string AppSource = "appsource";
            public const string OneDrive = "onedrive";
            public const string Storify = "storify";
            public const string XboxSubs = "xboxsubs";
            public const string XboxSettings = "xboxsettings";
            public const string Saturn = "saturn";
        }

        internal static class Operation
        {
            public const string RenderPidlPage = "RenderPidlPage";
        }

        internal static class ResourceName
        {
            public const string ChallengeDescriptions = "challengeDescriptions";
        }

        internal static class LoadTest
        {
            public static string[] TestUrlsTriggerTwoHundredResponse
            {
                get
                {
                    return new string[]
                    {
                        "addressDescriptions?type=billing&country=fr&language=fr-fr&operation=add&partner=default"
                    };
                }
            }

            public static string[] TestUrlsTriggerFiveHundredResponse
            {
                get
                {
                    return new string[]
                    {
                        "paymentMethodDescriptions?country=br&family=credit_card&language=en-US&partner=commercialstores&operation=update"
                    };
                }
            }
        }

        internal static class DiffTest
        {
            //// Used to build a paths for KnownDiffs (See KnownDiffsConfig.cs)
            //// Identifies any value for a given parameter
            //// i.e. for sets where Country = Any, Language = Any, partner = "webblends" apply KnownDiffs
            public const string Any = "_Any_";
            //// Used by SkipPidlCombinations to identify pidls that are missing a parameter
            //// i.e. type: NotSent
            public const string NotSent = "_NotSent_";

            /// <summary>
            /// This variable is used to indicate all values for a given identity field when
            /// applying deltas. For example:
            /// urlPath[resource][id(family.type)][country][language]...
            /// urlPath["paymentMethod"]["credit_card.visa"]["_Any_"]["en"]
            /// </summary>
            internal static class DiffCellIndexDescription
            {
                public const int ResourceName = 0;
                public const int ID = 1;
                public const int Country = 2;
                public const int Language = 3;
                public const int Partner = 4;
                public const int Operation = 5;
                public const int Scenario = 6;
                public const int DeltaType = 7;
                public const int BaselineJPath = 8;
                public const int NewJPath = 9;
                public const int NewValue = 10;
            }
        }
    }
}
