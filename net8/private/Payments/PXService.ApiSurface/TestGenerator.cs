// <copyright file="TestGenerator.cs" company="Microsoft">Copyright (c) Microsoft 2019. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PXService.ApiSurface.Diff
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Web;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;

    /// <summary>
    /// Generates a set of test criteria based on the requested configuration
    /// </summary>
    public class TestGenerator
    {
        private const string JarvisTestHeader = "px.account.v3.us.org.full.profile.default.address";
        private readonly HttpClient pifdClient;
        private List<Test> collection;
        private readonly IConfiguration? configuration;

        public TestGenerator(TestGeneratorConfig config = null)
        {
            this.configuration = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
             .Build();

            this.pifdClient = new HttpClient();
            this.pifdClient.BaseAddress = new Uri(@"https://paymentinstruments-int.mp.microsoft.com");

            this.Countries = Constants.CountriesTest;
            this.Languages = Constants.DiffTest.Languages;
            this.Partners = Constants.DiffTest.Partners;
            this.Operations = Constants.DiffTest.Operations;
            this.MSRewardsOperations = Constants.DiffTest.MSRewardsOperations;
            this.TaxIdScenarios = Constants.TaxIdScenarios;

            if (config != null)
            {
                this.Config = config;
            }
            else
            {
                this.Config = new TestGeneratorConfig()
                {
                    AddressDescription = true,
                    BillingGroupDescription = true,
                    ProfileDescriptionWithEmulator = true,
                    ProfileDescriptionWithoutEmulator = true,
                    ChallengeDescription = true,
                    CheckoutDescriptions = true,
                    TaxIdDescription = true,
                    PaymentMethodDescription = true,
                    RewardsDescriptions = true,
                    PaymentInstrumentEx = true,
                    RunDiffTestsForPSSFeatures = true
                };
            }

            this.collection = new List<Test>();
        }

        public static Dictionary<string, string> XboxNativeStyleHintsHeaders
        {
            get
            {
                return new Dictionary<string, string>
                   {
                       { "x-ms-flight", "PXEnableXboxNativeStyleHints" },
                       { "x-ms-pidlsdk-version", "2.7.0" }
                   };
            }
        }

        public List<string> Countries { get; private set; }

        public List<string> Languages { get; private set; }

        public List<string> Partners { get; private set; }

        public List<string> Operations { get; private set; }

        public List<string> TaxIdScenarios { get; private set; }

        public List<string> MSRewardsOperations { get; private set; }

        public TestGeneratorConfig Config { get; private set; }

        public List<Test> Set
        {
            get { return this.collection; }
        }

        public void GenerateTestSet()
        {
            if (this.Config.AddressDescription)
            {
                this.collection.AddRange(this.AddressDescriptionSet());
            }

            if (this.Config.BillingGroupDescription)
            {
                this.collection.AddRange(this.BillingGroupDescriptionSet());
            }

            if (this.Config.ProfileDescriptionWithEmulator)
            {
                this.collection.AddRange(this.ProfileDescriptionBasicSet());
            }

            if (this.Config.ProfileDescriptionWithoutEmulator)
            {
                this.collection.AddRange(this.ProfileDescriptionRemoteServerSet());
            }

            if (this.Config.ChallengeDescription)
            {
                this.collection.AddRange(this.ChallengeDescriptionSet());
            }

            if (this.Config.CheckoutDescriptions)
            {
                this.collection.AddRange(this.CheckoutDescriptions());
            }

            if (this.Config.TaxIdDescription)
            {
                this.collection.AddRange(this.TaxIdDescriptionSet());
            }

            if (this.Config.PaymentMethodDescription)
            {
                this.collection.AddRange(this.PaymentMethodDescriptionSet());
            }

            if (this.Config.RewardsDescriptions)
            {
                this.collection.AddRange(this.RewardsDescriptionsSet());
            }

            if (this.Config.PaymentInstrumentEx)
            {
                this.collection.AddRange(this.PaymentInstrumentsExSet());
            }

            if (this.Config.RunDiffTestsForPSSFeatures)
            {
                FeatureDiffTestGenerator pssDiffTestGenerator = new FeatureDiffTestGenerator(this.Config);
                this.collection.AddRange(pssDiffTestGenerator.GeneratePSSFeatureDiffTests());
            }
        }

        private static List<Test> PaymentMethodDescriptionsListPiIndiaExpiryDelete()
        {
            const string ResourceName = "paymentMethodDescriptions";
            List<Test> set = new List<Test>();

            Dictionary<string, string> additionalHeadersForIndiaExpiryDelete = new Dictionary<string, string>()
            {
                {
                    "x-ms-flight", Constants.FlightNames.IndiaExpiryGroupDelete
                }
            };

            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "in", language: "en-us", partner: Constants.PartnerNames.Azure, operation: "selectinstance"), null, additionalHeaders: additionalHeadersForIndiaExpiryDelete));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "in", language: "en-us", partner: Constants.PartnerNames.Commercialstores, operation: "selectinstance"), null, additionalHeaders: additionalHeadersForIndiaExpiryDelete));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "in", language: "en-us", partner: Constants.PartnerNames.Bing, operation: "selectinstance"), null, additionalHeaders: additionalHeadersForIndiaExpiryDelete));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "in", language: "en-us", partner: Constants.PartnerNames.Azure, operation: "selectinstance"), null));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "in", language: "en-us", partner: Constants.PartnerNames.Commercialstores, operation: "selectinstance"), null));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "in", language: "en-us", partner: Constants.PartnerNames.Payin, operation: "selectinstance"), null, additionalHeaders: additionalHeadersForIndiaExpiryDelete));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "in", language: "en-us", partner: Constants.PartnerNames.SetupOffice, operation: "selectinstance"), null, additionalHeaders: additionalHeadersForIndiaExpiryDelete));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "in", language: "en-us", partner: Constants.PartnerNames.AmcWeb, operation: "selectinstance"), null, additionalHeaders: additionalHeadersForIndiaExpiryDelete));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "in", language: "en-us", partner: Constants.PartnerNames.XboxNative, operation: "selectinstance"), null, additionalHeaders: additionalHeadersForIndiaExpiryDelete));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "in", language: "en-us", partner: Constants.PartnerNames.Payin, operation: "selectinstance"), null));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "in", language: "en-us", partner: Constants.PartnerNames.SetupOffice, operation: "selectinstance"), null));

            return Validate(set);
        }

        private static List<Test> PaymentMethodDescriptionsAddPiIndiaUPIEnable()
        {
            List<Test> set = new List<Test>();

            Dictionary<string, string> additionalHeadersForAddPiIndiaUPIEnable = new Dictionary<string, string>()
            {
                {
                    "x-ms-flight", $"{Constants.FlightNames.IndiaUPIEnable},{Constants.FlightNames.PxEnableUpi},{Constants.FlightNames.Vnext}"
                }
            };

            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "real_time_payments", type: "upi", language: "en-us", partner: Constants.PartnerNames.Webblends, operation: "add", completePrerequisites: bool.TrueString.ToLower()), null, null, additionalHeaders: additionalHeadersForAddPiIndiaUPIEnable));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "real_time_payments", type: "upi", language: "en-us", partner: Constants.PartnerNames.Webblends, operation: "add"), null, null, additionalHeaders: additionalHeadersForAddPiIndiaUPIEnable));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "real_time_payments", type: "upi", language: "en-us", partner: Constants.PartnerNames.AmcWeb, operation: "add", completePrerequisites: bool.TrueString.ToLower()), null, null, additionalHeaders: additionalHeadersForAddPiIndiaUPIEnable));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "real_time_payments", type: "upi", language: "en-us", partner: Constants.PartnerNames.AmcWeb, operation: "add"), null, null, additionalHeaders: additionalHeadersForAddPiIndiaUPIEnable));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "real_time_payments", type: "upi", language: "en-us", partner: "defaulttemplate", operation: "add", completePrerequisites: bool.TrueString.ToLower()), null, null, additionalHeaders: additionalHeadersForAddPiIndiaUPIEnable));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "real_time_payments", type: "upi", language: "en-us", partner: "defaulttemplate", operation: "add"), null, null, additionalHeaders: additionalHeadersForAddPiIndiaUPIEnable));

            return Validate(set);
        }

        private static List<Test> PaymentMethodDescriptionsAddANTBatchPi()
        {
            List<Test> set = new List<Test>();

            List<Tuple<string, string, string>> markets = new List<Tuple<string, string, string>>()
            {
                { new Tuple<string, string, string>("jp", Constants.PaymentMethodTypeNames.Paypay, "PayPay") },
                { new Tuple<string, string, string>("hk", Constants.PaymentMethodTypeNames.AlipayHK, "AlipayHK") },
                { new Tuple<string, string, string>("ph", Constants.PaymentMethodTypeNames.GCash, "GCash") },
                { new Tuple<string, string, string>("th", Constants.PaymentMethodTypeNames.TrueMoney, "TrueMoney") },
                { new Tuple<string, string, string>("my", Constants.PaymentMethodTypeNames.TouchNGo, "TouchNGo") }
            };

            foreach (var market in markets)
            {
                Dictionary<string, string> additionalHeaders = new Dictionary<string, string>()
                {
                    {
                        "x-ms-flight", $"{Constants.FlightNames.Vnext},PXEnable{market.Item3}"
                    }
                };

                set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: market.Item1, family: Constants.PaymentMethodFamilyNames.Ewallet, type: market.Item2, language: "en-US", partner: Constants.PartnerNames.Cart, operation: "add"), null, null, additionalHeaders: additionalHeaders));
                set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: market.Item1, family: Constants.PaymentMethodFamilyNames.Ewallet, type: market.Item2, language: "en-US", partner: Constants.PartnerNames.DefaultTemplate, operation: "add"), null, null, additionalHeaders: additionalHeaders));
            }

            return Validate(set);
        }

        private static List<Test> PaymentMethodDescriptionsAddAlipayCNPi()
        {
            List<Test> set = new List<Test>();

            Dictionary<string, string> additionalHeaders = new Dictionary<string, string>()
            {
                {
                    "x-ms-flight", $"{Constants.FlightNames.Vnext},PXEnableAlipayCN"
                }
            };

            Dictionary<string, string> xboxnativeStyleHintsHeaders = XboxNativeStyleHintsHeaders;

            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "cn", family: Constants.PaymentMethodFamilyNames.Ewallet, type: "alipaycn", language: "en-US", partner: Constants.PartnerNames.Azure, operation: "add", completePrerequisites: bool.FalseString.ToLower()), null, null, additionalHeaders: additionalHeaders));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "cn", family: Constants.PaymentMethodFamilyNames.Ewallet, type: "alipaycn", language: "en-US", partner: Constants.PartnerNames.Commercialstores, operation: "add", completePrerequisites: bool.FalseString.ToLower()), null, null, additionalHeaders: additionalHeaders));
            
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "cn", family: Constants.PaymentMethodFamilyNames.Ewallet, type: "alipay_billing_agreement", language: "en-US", partner: Constants.PartnerNames.Storify, operation: "add"), null, null));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "cn", family: Constants.PaymentMethodFamilyNames.Ewallet, type: "alipay_billing_agreement", language: "en-US", partner: Constants.PartnerNames.XboxSettings, operation: "add", completePrerequisites: bool.TrueString.ToLower()), null, null));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "cn", family: Constants.PaymentMethodFamilyNames.Ewallet, type: "alipay_billing_agreement", language: "en-US", partner: Constants.PartnerNames.Storify, operation: "add"), null, null, xboxnativeStyleHintsHeaders));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "cn", family: Constants.PaymentMethodFamilyNames.Ewallet, type: "alipay_billing_agreement", language: "en-US", partner: Constants.PartnerNames.XboxSettings, operation: "add", completePrerequisites: bool.TrueString.ToLower()), null, null, xboxnativeStyleHintsHeaders));

            return Validate(set);
        }

        private static List<Test> PaymentMethodDescriptionsAddPiIndiaUPICommercialEnable()
        {
            List<Test> set = new List<Test>();
            Dictionary<string, string> additionalHeadersForAddPiIndiaUPIEnable = new Dictionary<string, string>()
            {
                {
                    "x-ms-flight", $"{Constants.FlightNames.IndiaUPIEnable},{Constants.FlightNames.PxCommercialEnableUpi},{Constants.FlightNames.Vnext}"
                },
                {
                    "x-ms-test", "{ \"scenarios\": \"px.pims.upi.commercial.add.pi\", \"contact\":\"DiffTest\"}"
                }
            };

            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "real_time_payments", type: "upi_commercial", language: "en-us", partner: "defaulttemplate", operation: "add"), null, "px-get-upi-commercial-pi", additionalHeaders: additionalHeadersForAddPiIndiaUPIEnable));
            return Validate(set);
        }

        private static List<Test> PaymentMethodDescriptionsAddPiSepaWithJpmcEnable()
        {
            List<Test> set = new List<Test>();
            Dictionary<string, string> additionalHeadersForAddPiSepaWithJpmcEnable = new Dictionary<string, string>()
            {
                {
                    "x-ms-test", "{ \"scenarios\": \"px.pims.sepa.add.success\", \"contact\":\"DiffTest\"}"
                }
            };

            string[] partners = new string[]
            {
                Constants.PartnerNames.Azure,
                Constants.PartnerNames.Bing,
                Constants.PartnerNames.Cart,
                Constants.PartnerNames.Commercialstores,
                Constants.PartnerNames.DefaultTemplate,
                Constants.PartnerNames.Macmanage,
                Constants.PartnerNames.NorthstarWeb,
                Constants.PartnerNames.OfficeSMB,
                Constants.PartnerNames.OxoDIME,
                Constants.PartnerNames.OxoWebDirect,
                Constants.PartnerNames.SetupOfficeSdx,
                Constants.PartnerNames.Webblends,
                Constants.PartnerNames.XboxSubs
            };

            foreach (string partner in partners)
            {
                set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "de", family: "direct_debit", type: "sepa", language: "en-us", partner: partner, operation: "add"), null, null, additionalHeaders: additionalHeadersForAddPiSepaWithJpmcEnable));
            }

            return Validate(set);
        }

        private static List<Test> ExpressCheckoutTests()
        {
            List<Test> set = new List<Test>();
            string expressCheckoutData = "{\"amount\":0.99,\"currency\":\"USD\",\"country\":\"US\",\"language\":\"en-US\",\"topDomainUrl\":\"\",\"recurringPaymentDetails\":{\"frequencyUnit\":\"month\",\"frequency\":1,\"startTime\":\"2025-06-12T04:31:38.607Z\",\"amount\":1,\"label\":\"\"},\"isTaxIncluded\":false,\"options\":{\"cornerRadius\":0,\"buttonColor\":{\"googlepay\":\"\",\"applepay\":\"\"},\"buttonType\":{\"googlepay\":\"\",\"applepay\":\"\"},\"requireShippingAddress\":false}}";
            Dictionary<string, string> additionalHeaders = new Dictionary<string, string>
            {
                {
                    "x-ms-flight", "PXUseMockWalletConfig"
                },
                {
                    "x-ms-test", "{ \"scenarios\": \"px.pims.googlepay.expresscheckout.add.success\", \"contact\":\"DiffTest\"}"
                }
            };

            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "US", partner: Constants.PartnerNames.DefaultTemplate, expressCheckoutData: expressCheckoutData, operation: Constants.PidlOperationType.ExpressCheckout), null, null, additionalHeaders: additionalHeaders));
            return set;
        }

        private static List<Test> PaymentMethodDescriptionsAddPiIndiaRupayEnable()
        {
            List<Test> set = new List<Test>();
            Dictionary<string, string> additionalHeadersForAddPiIndiaRupayEnable = new Dictionary<string, string>()
            {
                {
                    "x-ms-flight", $"{Constants.FlightNames.IndiaRupayEnable},{Constants.FlightNames.PXEnableRupayForIN},{Constants.FlightNames.Vnext},{Constants.FlightNames.IndiaExpiryGroupDelete},{Constants.FlightNames.IndiaTokenizationConsentCapture}"
                },
                {
                    "x-ms-test", "{ \"scenarios\": \"px.pims.cc.rupay.3ds.consumer.add.pending.get.pending\", \"contact\":\"DiffTest\"}"
                }
            };

            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "rupay", language: "en-us", partner: "defaulttemplate", operation: "add"), null, null, additionalHeaders: additionalHeadersForAddPiIndiaRupayEnable));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "rupay", language: "en-us", partner: "defaulttemplate", operation: "update"), null, null, additionalHeaders: additionalHeadersForAddPiIndiaRupayEnable));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "rupay", language: "en-us", partner: "defaulttemplate", operation: "update", scenario: Constants.ScenariosForPaymentMethodDescription.IncludeCvv), null, null, additionalHeaders: additionalHeadersForAddPiIndiaRupayEnable));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "rupay", language: "en-us", partner: Constants.PartnerNames.Webblends, operation: "add"), null, null, additionalHeaders: additionalHeadersForAddPiIndiaRupayEnable));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "rupay", language: "en-us", partner: Constants.PartnerNames.Webblends, operation: "update"), null, null, additionalHeaders: additionalHeadersForAddPiIndiaRupayEnable));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "rupay", language: "en-us", partner: Constants.PartnerNames.Cart, operation: "add"), null, null, additionalHeaders: additionalHeadersForAddPiIndiaRupayEnable));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "rupay", language: "en-us", partner: Constants.PartnerNames.AmcWeb, operation: "add"), null, null, additionalHeaders: additionalHeadersForAddPiIndiaRupayEnable));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "rupay", language: "en-us", partner: Constants.PartnerNames.NorthstarWeb, operation: "add"), null, null, additionalHeaders: additionalHeadersForAddPiIndiaRupayEnable));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "rupay", language: "en-us", partner: Constants.PartnerNames.Xbox, operation: "add"), null, null, additionalHeaders: additionalHeadersForAddPiIndiaRupayEnable));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "rupay", language: "en-us", partner: Constants.PartnerNames.OfficeOobe, operation: "add"), null, null, additionalHeaders: additionalHeadersForAddPiIndiaRupayEnable));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "rupay", language: "en-us", partner: Constants.PartnerNames.ConsumerSupport, operation: "add"), null, null, additionalHeaders: additionalHeadersForAddPiIndiaRupayEnable));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "rupay", language: "en-us", partner: Constants.PartnerNames.OxoDIME, operation: "add"), null, null, additionalHeaders: additionalHeadersForAddPiIndiaRupayEnable));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "rupay", language: "en-us", partner: Constants.PartnerNames.OxoWebDirect, operation: "add"), null, null, additionalHeaders: additionalHeadersForAddPiIndiaRupayEnable));
            
            additionalHeadersForAddPiIndiaRupayEnable = new Dictionary<string, string>()
            {
                {
                    "x-ms-flight", $"{Constants.FlightNames.IndiaRupayEnable},{Constants.FlightNames.PXEnableRupayForIN},{Constants.FlightNames.Vnext},{Constants.FlightNames.IndiaExpiryGroupDelete},{Constants.FlightNames.IndiaTokenizationConsentCapture}"
                },
                {
                    "x-ms-test", "{ \"scenarios\": \"px.pims.cc.rupay.3ds.commercial.add.pending.get.pending\", \"contact\":\"DiffTest\"}"
                }
            };

            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "rupay", language: "en-us", partner: Constants.PartnerNames.Azure, operation: "add"), null, null, additionalHeaders: additionalHeadersForAddPiIndiaRupayEnable));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "rupay", language: "en-us", partner: Constants.PartnerNames.Bing, operation: "add"), null, null, additionalHeaders: additionalHeadersForAddPiIndiaRupayEnable));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "rupay", language: "en-us", partner: Constants.PartnerNames.Commercialstores, operation: "add"), null, null, additionalHeaders: additionalHeadersForAddPiIndiaRupayEnable));

            return Validate(set);
        }

        private static List<Test> PaymentMethodDescriptionsListPiEnableIndiaTokenExpiryDetails()
        {
            const string ResourceName = "paymentMethodDescriptions";
            List<Test> set = new List<Test>();

            Dictionary<string, string> additionalHeadersForEnableIndiaTokenExpiryDetails = new Dictionary<string, string>()
            {
                {
                    "x-ms-flight", Constants.FlightNames.EnableIndiaTokenExpiryDetails
                }
            };

            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "in", language: "en-us", partner: Constants.PartnerNames.AmcWeb, operation: "selectinstance"), null, additionalHeaders: additionalHeadersForEnableIndiaTokenExpiryDetails));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "in", language: "en-us", partner: Constants.PartnerNames.XboxNative, operation: "selectinstance"), null, additionalHeaders: additionalHeadersForEnableIndiaTokenExpiryDetails));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "in", language: "en-us", partner: Constants.PartnerNames.Payin, operation: "selectinstance"), null));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "in", language: "en-us", partner: Constants.PartnerNames.Azure, operation: "selectinstance"), null));

            return Validate(set);
        }

        private static List<Test> PaymentMethodDescriptionsListPISet()
        {
            const string ResourceName = "paymentMethodDescriptions";
            List<Test> set = new List<Test>();
            List<string> partners = new List<string> { Constants.PartnerNames.Storify, Constants.PartnerNames.XboxNative };
            List<string> countries = new List<string> { "us", "gb", "kr", "br", "it" };
            Dictionary<string, string> xboxNativeStyleHintsHeaders = XboxNativeStyleHintsHeaders;
            Dictionary<string, string> nativeListPiStyleHintsHeadersWithFontIcons = new Dictionary<string, string>
            {
                { "x-ms-flight", "PXEnableXboxNativeStyleHints,PXUseFontIcons" },
                { "x-ms-pidlsdk-version", "2.7.0" }
            };

            foreach (string partner in partners)
            {
                foreach (string country in countries)
                {
                    set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, language: "en-us", partner: partner, operation: "selectinstance"), null));
                    if (Constants.IsXboxNativePartner(partner))
                    {
                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, language: "en-us", partner: partner, operation: "selectinstance"), null, null, xboxNativeStyleHintsHeaders));
                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, language: "en-us", partner: partner, operation: "selectinstance"), null, null, nativeListPiStyleHintsHeadersWithFontIcons));
                    }
                }
            }

            return Validate(set);
        }

        private static List<Test> PaymentMethodDescriptionSelectPMForSepaWithNewlogo()
        {
            List<Test> set = new List<Test>();
            var pmGroupingHeaders = new Dictionary<string, string>()
            {
                {
                    "x-ms-flight", "enablePaymentMethodGrouping,EnableNewLogoSepa,PXUsePartnerSettingsService"
                }
            };

            var allowedMethodsSet = new string[] { "direct_debit.sepa" };
            string formattedAllowedMethods;
            formattedAllowedMethods = string.Format("[\"{0}\"]", string.Join("\",\"", allowedMethodsSet));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "de", language: "en-us", partner: "cart", allowedPaymentMethods: formattedAllowedMethods, operation: Constants.PidlOperationType.Select), null, null, pmGroupingHeaders));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "de", language: "en-us", partner: Constants.TemplateNames.SelectPMButtonList, allowedPaymentMethods: formattedAllowedMethods, operation: Constants.PidlOperationType.Select), null, null, pmGroupingHeaders));

            return Validate(set);
        }

        private static List<Test> PaymentMethodDescriptionPMGroupingSet()
        {
            List<Test> set = new List<Test>();
            var pmGroupingHeaders = new Dictionary<string, string>()
            {
                {
                    "x-ms-flight", "enablePaymentMethodGrouping"
                }
            };

            foreach (var allowedMethodsSet in Constants.DiffTest.AllowedPaymentMethods)
            {
                string formattedAllowedMethods;
                formattedAllowedMethods = string.Format("[\"{0}\"]", string.Join("\",\"", allowedMethodsSet));
                set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "us", language: "en-us", partner: "cart", allowedPaymentMethods: formattedAllowedMethods, operation: Constants.PidlOperationType.Select), null, null, pmGroupingHeaders));
                set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "us", language: "en-us", partner: Constants.PartnerNames.Webblends, allowedPaymentMethods: formattedAllowedMethods, operation: Constants.PidlOperationType.Select), null, null, pmGroupingHeaders));
                set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "us", language: "en-us", partner: Constants.TemplateNames.SelectPMButtonList, allowedPaymentMethods: formattedAllowedMethods, operation: Constants.PidlOperationType.Select), null, null, pmGroupingHeaders));
            }

            return Validate(set);
        }

        private static List<Test> PaymentMethodDescriptionPMGroupingSetForXboxNative()
        {
            List<Test> set = new List<Test>();
            var pmGroupingHeaders = new Dictionary<string, string>()
            {
                {
                    "x-ms-pidlsdk-version", "1.22.3"
                },
                {
                    "x-ms-flight", "enablePaymentMethodGrouping"
                }
            };

            var pmGroupingHeadersWithStyleHints = new Dictionary<string, string>
            {
                {
                    "x-ms-pidlsdk-version", "2.7.0"
                },
                {
                    "x-ms-flight", "enablePaymentMethodGrouping,PXEnableXboxNativeStyleHints"
                }
            };

            foreach (var allowedMethodsSet in Constants.DiffTest.AllowedPaymentMethods)
            {
                string formattedAllowedMethods;
                formattedAllowedMethods = string.Format("[\"{0}\"]", string.Join("\",\"", allowedMethodsSet));
                set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "us", language: "en-us", partner: "storify", allowedPaymentMethods: formattedAllowedMethods, operation: Constants.PidlOperationType.Select), null, null, pmGroupingHeaders));
                set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "us", language: "en-us", partner: "xboxsettings", allowedPaymentMethods: formattedAllowedMethods, operation: Constants.PidlOperationType.Select), null, null, pmGroupingHeaders));
                set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "us", language: "en-us", partner: "storify", allowedPaymentMethods: formattedAllowedMethods, operation: Constants.PidlOperationType.Select), null, null, pmGroupingHeadersWithStyleHints));
                set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "us", language: "en-us", partner: "xboxsettings", allowedPaymentMethods: formattedAllowedMethods, operation: Constants.PidlOperationType.Select), null, null, pmGroupingHeadersWithStyleHints));
            }

            return Validate(set);
        }

        private static List<Test> PaymentMethodDescriptionRedeemGiftCardForXboxNative()
        {
            List<Test> set = new List<Test>();

            // RedeemCSVToken
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "us", family: Constants.PaymentMethodFamilyNames.Ewallet, type: "stored_value", language: "en-us", partner: Constants.PartnerNames.XboxSettings, operation: Constants.PidlOperationType.Add), null));

            return Validate(set);
        }

        private static List<Test> PaymentMethodDescriptionsAddKakaopayForXboxNative()
        {
            const string ResourceName = "paymentMethodDescriptions";
            List<Test> set = new List<Test>();
            var kakaopayStyleHintsHeaders = new Dictionary<string, string>
            {
                { "x-ms-pidlsdk-version", "2.7.0" },
                { "x-ms-flight", "PXEnableXboxNativeStyleHints" }
            };

            // add test for add kakaopay for xboxnative partners
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "kr", family: Constants.PaymentMethodFamilyNames.Ewallet, type: Constants.PaymentMethodTypeNames.Kakaopay, language: "en-us", partner: Constants.PartnerNames.Storify, operation: Constants.PidlOperationType.Add), null));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "kr", family: Constants.PaymentMethodFamilyNames.Ewallet, type: Constants.PaymentMethodTypeNames.Kakaopay, language: "en-us", partner: Constants.PartnerNames.Saturn, operation: Constants.PidlOperationType.Add), null));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "kr", family: Constants.PaymentMethodFamilyNames.Ewallet, type: Constants.PaymentMethodTypeNames.Kakaopay, language: "en-us", partner: Constants.PartnerNames.XboxSubs, operation: Constants.PidlOperationType.Add), null));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "kr", family: Constants.PaymentMethodFamilyNames.Ewallet, type: Constants.PaymentMethodTypeNames.Kakaopay, language: "en-us", partner: Constants.PartnerNames.XboxSettings, operation: Constants.PidlOperationType.Add), null));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "kr", family: Constants.PaymentMethodFamilyNames.Ewallet, type: Constants.PaymentMethodTypeNames.Kakaopay, language: "en-us", partner: Constants.PartnerNames.Storify, operation: Constants.PidlOperationType.Add), null, null, kakaopayStyleHintsHeaders));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "kr", family: Constants.PaymentMethodFamilyNames.Ewallet, type: Constants.PaymentMethodTypeNames.Kakaopay, language: "en-us", partner: Constants.PartnerNames.Saturn, operation: Constants.PidlOperationType.Add), null, null, kakaopayStyleHintsHeaders));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "kr", family: Constants.PaymentMethodFamilyNames.Ewallet, type: Constants.PaymentMethodTypeNames.Kakaopay, language: "en-us", partner: Constants.PartnerNames.XboxSubs, operation: Constants.PidlOperationType.Add), null, null, kakaopayStyleHintsHeaders));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "kr", family: Constants.PaymentMethodFamilyNames.Ewallet, type: Constants.PaymentMethodTypeNames.Kakaopay, language: "en-us", partner: Constants.PartnerNames.XboxSettings, operation: Constants.PidlOperationType.Add), null, null, kakaopayStyleHintsHeaders));

            return Validate(set);
        }

        private static List<Test> PaymentMethodDescriptionsApplyXboxCoBrandedCardForXboxNative()
        {
            List<Test> set = new List<Test>();

            var headers = new Dictionary<string, string>()
            {
                {
                    "x-ms-clientcontext-encoding", "base64"
                },
                {
                    "x-ms-msaprofile", "PUID=OTg1MTU0NDIwNDI3ODYz,emailAddress=a293c2hpa190ZXN0MTFAb3V0bG9vay5jb20="
                },
                {
                    "x-ms-test", "{ \"scenarios\": \"px.issuerservice.default\", \"contact\":\"DiffTest\"}"
                }
            };

            // paymentMethodDescriptions and paymentInstrumentsEx need to be bundled.
            // First call creates a restAction that automatically calls back to paymentInstrumentsEx to get pidl.

            // Storify partner currently unused. Re-enable when rolling out to storify.
            //// set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "us", family: Constants.PaymentMethodFamilyNames.CreditCard, type: Constants.PaymentMethodTypeNames.MasterCard, language: "en-us", partner: Constants.PartnerNames.Storify, operation: Constants.PidlOperationType.Apply, scenario: Constants.ScenarioNames.XboxCoBrandedCard, channel: "Backend", referrerId: "Browser", ocid: "sample"), null, null, headers));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "us", family: Constants.PaymentMethodFamilyNames.CreditCard, type: Constants.PaymentMethodTypeNames.MasterCard, language: "en-us", partner: Constants.PartnerNames.XboxCardApp, operation: Constants.PidlOperationType.Apply, scenario: Constants.ScenarioNames.XboxCoBrandedCard, channel: "Backend", referrerId: "Browser", ocid: "sample"), null, null, headers));

            TestRequestContent requestContent = new TestRequestContent(".\\DiffTest\\ConfigFiles\\scenarios\\px_issuerservice_default.json");

            // Storify partner currently unused. Re-enable when rolling out to storify.
            //// set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentInstrumentsEx", country: "us", language: "en-us", partner: Constants.PartnerNames.Storify, operation: Constants.PidlOperationType.Apply, ocid: "sample"), requestContent, null, headers));
            //// set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentInstrumentsEx", country: "us", language: "en-us", partner: Constants.PartnerNames.XboxCardApp, operation: Constants.PidlOperationType.Apply, ocid: "sample"), requestContent, null, headers));

            return Validate(set);
        }

        private static List<Test> PaymentMethodDescriptionsApplyWebPartners()
        {
            List<Test> set = new List<Test>();

            var headers = new Dictionary<string, string>()
            {
                {
                    "x-ms-clientcontext-encoding", "base64"
                },
                {
                    "x-ms-msaprofile", "PUID=OTg1MTU0NDIwNDI3ODYz,emailAddress=a293c2hpa190ZXN0MTFAb3V0bG9vay5jb20="
                }
            };

            // First call creates a restAction that automatically calls back to paymentInstrumentsEx to get pidl.
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "us", family: Constants.PaymentMethodFamilyNames.CreditCard, type: Constants.PaymentMethodTypeNames.MasterCard, language: "en-us", partner: Constants.PartnerNames.XboxWeb, operation: Constants.PidlOperationType.Apply, channel: "TestChannel", referrerId: "TestReferrer"), null, null, headers));

            TestRequestContent requestContent = new TestRequestContent(".\\DiffTest\\ConfigFiles\\scenarios\\px_issuerservice_default.json");
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentInstrumentsEx", country: "us", language: "en-us", partner: Constants.PartnerNames.XboxWeb, operation: Constants.PidlOperationType.Apply), requestContent, null, headers));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentInstrumentsEx", country: "us", language: "en-us", partner: Constants.PartnerNames.XboxWeb, operation: Constants.PidlOperationType.Apply, sessionId: "TestSession"), requestContent, null, headers));

            return Validate(set);
        }

        private static List<Test> PaymentMethodDescriptionsAddVenmo()
        {
            const string ResourceName = "paymentMethodDescriptions";
            List<Test> set = new List<Test>();

            var venmoHeaders = new Dictionary<string, string>()
            {
                {
                    "x-ms-flight", "PxEnableVenmo,PxEnableSelectPMAddPIVenmo"
                }
            };

            var xboxNativeStyleHintsHeaders = new Dictionary<string, string>
            {
                { "x-ms-pidlsdk-version", "2.7.0" },
                { "x-ms-flight", "PXEnableXboxNativeStyleHints,PxEnableVenmo,PxEnableSelectPMAddPIVenmo" }
            };

            // Xbox partners
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "us", family: Constants.PaymentMethodFamilyNames.Ewallet, type: Constants.PaymentMethodTypeNames.Venmo, language: "en-us", partner: Constants.PartnerNames.Storify, operation: Constants.PidlOperationType.Add, scenario: "venmoQrCode"), null, "px.pims.venmo.add.pending.get.pending", venmoHeaders));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "us", family: Constants.PaymentMethodFamilyNames.Ewallet, type: Constants.PaymentMethodTypeNames.Venmo, language: "en-us", partner: Constants.PartnerNames.XboxSettings, operation: Constants.PidlOperationType.Add, scenario: "venmoQrCode"), null, "px.pims.venmo.add.pending.get.pending", venmoHeaders));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "us", family: Constants.PaymentMethodFamilyNames.Ewallet, type: Constants.PaymentMethodTypeNames.Venmo, language: "en-us", partner: Constants.PartnerNames.Storify, operation: Constants.PidlOperationType.Add, scenario: "venmoQrCode"), null, "px.pims.venmo.add.pending.get.pending", xboxNativeStyleHintsHeaders));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "us", family: Constants.PaymentMethodFamilyNames.Ewallet, type: Constants.PaymentMethodTypeNames.Venmo, language: "en-us", partner: Constants.PartnerNames.XboxSettings, operation: Constants.PidlOperationType.Add, scenario: "venmoQrCode"), null, "px.pims.venmo.add.pending.get.pending", xboxNativeStyleHintsHeaders));

            // Web partners
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "us", family: Constants.PaymentMethodFamilyNames.Ewallet, type: Constants.PaymentMethodTypeNames.Venmo, language: "en-us", partner: Constants.PartnerNames.AmcWeb, operation: Constants.PidlOperationType.Add), null, "px.pims.venmo.add.pending.get.pending", venmoHeaders));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "us", family: Constants.PaymentMethodFamilyNames.Ewallet, type: Constants.PaymentMethodTypeNames.Venmo, language: "en-us", partner: Constants.PartnerNames.Cart, operation: Constants.PidlOperationType.Add), null, "px.pims.venmo.add.pending.get.pending", venmoHeaders));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "us", family: Constants.PaymentMethodFamilyNames.Ewallet, type: Constants.PaymentMethodTypeNames.Venmo, language: "en-us", partner: Constants.PartnerNames.OxoWebDirect, operation: Constants.PidlOperationType.Add), null, "px.pims.venmo.add.pending.get.pending", venmoHeaders));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "us", family: Constants.PaymentMethodFamilyNames.Ewallet, type: Constants.PaymentMethodTypeNames.Venmo, language: "en-us", partner: Constants.PartnerNames.Webblends, operation: Constants.PidlOperationType.Add), null, "px.pims.venmo.add.pending.get.pending", venmoHeaders));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "us", family: Constants.PaymentMethodFamilyNames.Ewallet, type: Constants.PaymentMethodTypeNames.Venmo, language: "en-us", partner: Constants.TemplateNames.DefaultTemplate, operation: Constants.PidlOperationType.Add), null, "px.pims.venmo.add.pending.get.pending", venmoHeaders));

            return Validate(set);
        }

        private static List<Test> PaymentMethodDescriptionsDeleteSet()
        {
            const string ResourceName = "paymentMethodDescriptions";
            List<Test> set = new List<Test>();
            var xboxNativeStyleHintsHeaders = XboxNativeStyleHintsHeaders;

            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "us", family: Constants.PaymentMethodFamilyNames.CreditCard, type: Constants.PaymentMethodTypeNames.Visa, language: "en-us", partner: Constants.PartnerNames.Storify, operation: Constants.PidlOperationType.Delete), null, null));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "us", family: Constants.PaymentMethodFamilyNames.CreditCard, type: Constants.PaymentMethodTypeNames.Amex, language: "en-us", partner: Constants.PartnerNames.Storify, operation: Constants.PidlOperationType.Delete), null, null));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "us", family: Constants.PaymentMethodFamilyNames.CreditCard, type: Constants.PaymentMethodTypeNames.MC, language: "en-us", partner: Constants.PartnerNames.Storify, operation: Constants.PidlOperationType.Delete), null, null));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "us", family: Constants.PaymentMethodFamilyNames.CreditCard, type: Constants.PaymentMethodTypeNames.Discover, language: "en-us", partner: Constants.PartnerNames.Storify, operation: Constants.PidlOperationType.Delete), null, null));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "jp", family: Constants.PaymentMethodFamilyNames.CreditCard, type: Constants.PaymentMethodTypeNames.JCB, language: "en-us", partner: Constants.PartnerNames.Storify, operation: Constants.PidlOperationType.Delete), null, null));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "br", family: Constants.PaymentMethodFamilyNames.CreditCard, type: Constants.PaymentMethodTypeNames.ELO, language: "en-us", partner: Constants.PartnerNames.Storify, operation: Constants.PidlOperationType.Delete), null, null));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "br", family: Constants.PaymentMethodFamilyNames.CreditCard, type: Constants.PaymentMethodTypeNames.Hipercard, language: "en-us", partner: Constants.PartnerNames.Storify, operation: Constants.PidlOperationType.Delete), null, null));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "us", family: Constants.PaymentMethodFamilyNames.Ewallet, type: Constants.PaymentMethodTypeNames.Paypal, language: "en-us", partner: Constants.PartnerNames.Storify, operation: Constants.PidlOperationType.Delete), null, null));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "cn", family: Constants.PaymentMethodFamilyNames.Ewallet, type: Constants.PaymentMethodTypeNames.AlipayBillingAgreement, language: "en-us", partner: Constants.PartnerNames.Storify, operation: Constants.PidlOperationType.Delete), null, null));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "gb", family: Constants.PaymentMethodFamilyNames.MobileBillingNonSim, language: "en-us", partner: Constants.PartnerNames.Storify, operation: Constants.PidlOperationType.Delete), null, null));

            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "us", family: Constants.PaymentMethodFamilyNames.CreditCard, type: Constants.PaymentMethodTypeNames.Visa, language: "en-us", partner: Constants.PartnerNames.Storify, operation: Constants.PidlOperationType.Delete), null, null, xboxNativeStyleHintsHeaders));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "us", family: Constants.PaymentMethodFamilyNames.CreditCard, type: Constants.PaymentMethodTypeNames.Amex, language: "en-us", partner: Constants.PartnerNames.Storify, operation: Constants.PidlOperationType.Delete), null, null, xboxNativeStyleHintsHeaders));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "us", family: Constants.PaymentMethodFamilyNames.CreditCard, type: Constants.PaymentMethodTypeNames.MC, language: "en-us", partner: Constants.PartnerNames.Storify, operation: Constants.PidlOperationType.Delete), null, null, xboxNativeStyleHintsHeaders));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "us", family: Constants.PaymentMethodFamilyNames.CreditCard, type: Constants.PaymentMethodTypeNames.Discover, language: "en-us", partner: Constants.PartnerNames.Storify, operation: Constants.PidlOperationType.Delete), null, null, xboxNativeStyleHintsHeaders));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "jp", family: Constants.PaymentMethodFamilyNames.CreditCard, type: Constants.PaymentMethodTypeNames.JCB, language: "en-us", partner: Constants.PartnerNames.Storify, operation: Constants.PidlOperationType.Delete), null, null, xboxNativeStyleHintsHeaders));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "br", family: Constants.PaymentMethodFamilyNames.CreditCard, type: Constants.PaymentMethodTypeNames.ELO, language: "en-us", partner: Constants.PartnerNames.Storify, operation: Constants.PidlOperationType.Delete), null, null, xboxNativeStyleHintsHeaders));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "br", family: Constants.PaymentMethodFamilyNames.CreditCard, type: Constants.PaymentMethodTypeNames.Hipercard, language: "en-us", partner: Constants.PartnerNames.Storify, operation: Constants.PidlOperationType.Delete), null, null, xboxNativeStyleHintsHeaders));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "us", family: Constants.PaymentMethodFamilyNames.Ewallet, type: Constants.PaymentMethodTypeNames.Paypal, language: "en-us", partner: Constants.PartnerNames.Storify, operation: Constants.PidlOperationType.Delete), null, null, xboxNativeStyleHintsHeaders));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "cn", family: Constants.PaymentMethodFamilyNames.Ewallet, type: Constants.PaymentMethodTypeNames.AlipayBillingAgreement, language: "en-us", partner: Constants.PartnerNames.Storify, operation: Constants.PidlOperationType.Delete), null, null, xboxNativeStyleHintsHeaders));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "gb", family: Constants.PaymentMethodFamilyNames.MobileBillingNonSim, language: "en-us", partner: Constants.PartnerNames.Storify, operation: Constants.PidlOperationType.Delete), null, null, xboxNativeStyleHintsHeaders));

            return Validate(set);
        }

        private static List<Test> PaymentMethodDescriptionsAddNSM()
        {
            List<Test> set = new List<Test>();
            const string ResourceName = "paymentMethodDescriptions";
            var xboxNativeStyleHintsHeaders = XboxNativeStyleHintsHeaders;

            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "gb", family: Constants.PaymentMethodFamilyNames.MobileBillingNonSim, language: "en-us", partner: Constants.PartnerNames.Storify, operation: Constants.PidlOperationType.Add), null));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "gb", family: Constants.PaymentMethodFamilyNames.MobileBillingNonSim, language: "en-us", partner: Constants.PartnerNames.XboxSettings, operation: Constants.PidlOperationType.Add, completePrerequisites: bool.TrueString.ToLower()), null));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "gb", family: Constants.PaymentMethodFamilyNames.MobileBillingNonSim, language: "en-us", partner: Constants.PartnerNames.Storify, operation: Constants.PidlOperationType.Add), null, null, xboxNativeStyleHintsHeaders));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "gb", family: Constants.PaymentMethodFamilyNames.MobileBillingNonSim, language: "en-us", partner: Constants.PartnerNames.XboxSettings, operation: Constants.PidlOperationType.Add, completePrerequisites: bool.TrueString.ToLower()), null, null, xboxNativeStyleHintsHeaders));

            return Validate(set);
        }

        private static List<Test> PaymentMethodDescriptionsXboxNativeAddUpdateCC()
        {
            List<Test> set = new List<Test>();
            const string ResourceName = "paymentMethodDescriptions";
            Dictionary<string, List<string>> countrySupportedTypesMap = new Dictionary<string, List<string>>
            {
                { "us", new List<string> { Constants.PaymentMethodTypeNames.Visa, Constants.PaymentMethodTypeNames.Amex, Constants.PaymentMethodTypeNames.MC, Constants.PaymentMethodTypeNames.Discover } },
                { "br", new List<string> { Constants.PaymentMethodTypeNames.ELO, Constants.PaymentMethodTypeNames.Hipercard } },
                { "ng", new List<string> { Constants.PaymentMethodTypeNames.Verve } },
                { "jp", new List<string> { Constants.PaymentMethodTypeNames.JCB } },
                { "cn", new List<string> { Constants.PaymentMethodTypeNames.UnionPayCreditCard, Constants.PaymentMethodTypeNames.UnionPayDebitCard } }
            };
            var xboxNativeStyleHintsHeaders = XboxNativeStyleHintsHeaders;

            foreach (string country in countrySupportedTypesMap.Keys)
            {
                foreach (string type in countrySupportedTypesMap[country])
                {
                    set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, family: Constants.PaymentMethodFamilyNames.CreditCard, type: type, language: "en-us", partner: Constants.PartnerNames.Storify, operation: Constants.PidlOperationType.Add), null));
                    set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, family: Constants.PaymentMethodFamilyNames.CreditCard, type: type, language: "en-us", partner: Constants.PartnerNames.Storify, operation: Constants.PidlOperationType.Add, completePrerequisites: bool.TrueString.ToLower()), null));
                    set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, family: Constants.PaymentMethodFamilyNames.CreditCard, type: type, language: "en-us", partner: Constants.PartnerNames.Storify, operation: Constants.PidlOperationType.Add), null, null, xboxNativeStyleHintsHeaders));
                    set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, family: Constants.PaymentMethodFamilyNames.CreditCard, type: type, language: "en-us", partner: Constants.PartnerNames.Storify, operation: Constants.PidlOperationType.Add, completePrerequisites: bool.TrueString.ToLower()), null, null, xboxNativeStyleHintsHeaders));

                    set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, family: Constants.PaymentMethodFamilyNames.CreditCard, type: type, language: "en-us", partner: Constants.PartnerNames.Storify, operation: Constants.PidlOperationType.Update), null));
                    set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, family: Constants.PaymentMethodFamilyNames.CreditCard, type: type, language: "en-us", partner: Constants.PartnerNames.Storify, operation: Constants.PidlOperationType.Update), null, null, xboxNativeStyleHintsHeaders));
                }
            }

            return Validate(set);
        }

        private static List<Test> PaymentMethodDescriptionsWindowsStore()
        {
            const string ResourceName = "paymentMethodDescriptions";
            List<Test> set = new List<Test>();
            var paypalHeaders = new Dictionary<string, string>()
            {
                {
                    "x-ms-flight", "PXUsePartnerSettingsService"
                }
            };

            // AddCC
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "us", family: Constants.PaymentMethodFamilyNames.CreditCard, type: "visa%2Cmc%2Camex%2Cdiscover", language: "en-us", partner: Constants.PartnerNames.WindowsStore, operation: Constants.PidlOperationType.Add), null));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "us", family: Constants.PaymentMethodFamilyNames.CreditCard, type: "visa%2Cmc%2Camex%2Cdiscover", language: "en-us", partner: Constants.PartnerNames.WindowsSettings, operation: Constants.PidlOperationType.Add), null));

            // PayPal
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "us", family: Constants.PaymentMethodFamilyNames.Ewallet, type: "paypal", language: "en-us", partner: Constants.PartnerNames.WindowsStore, operation: Constants.PidlOperationType.Add), null, null, paypalHeaders));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "us", family: Constants.PaymentMethodFamilyNames.Ewallet, type: "paypal", language: "en-us", partner: Constants.PartnerNames.WindowsSettings, operation: Constants.PidlOperationType.Add), null, null, paypalHeaders));

            // RedeemCSVToken
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "us", family: Constants.PaymentMethodFamilyNames.Ewallet, type: "stored_value", language: "en-us", partner: Constants.PartnerNames.WindowsStore, operation: Constants.PidlOperationType.Add), null));

            return Validate(set);
        }

        private static List<Test> PaymentMethodDescriptionsUnionPayInternational()
        {
            const string ResourceName = "paymentMethodDescriptions";
            List<Test> set = new List<Test>();
            var unionPayInternationalHeaders = new Dictionary<string, string>()
            {
                {
                    "x-ms-flight", "PXUsePartnerSettingsService,vnext,PXEnableCUPInternational"
                }
            };

            // AddCC
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "cn", family: Constants.PaymentMethodFamilyNames.CreditCard, type: "unionpay_international", language: "en-us", partner: Constants.TemplateNames.DefaultTemplate, operation: Constants.PidlOperationType.Add), null, null, unionPayInternationalHeaders));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "cn", family: Constants.PaymentMethodFamilyNames.CreditCard, type: "unionpay_international", language: "en-us", partner: Constants.TemplateNames.DefaultTemplate, operation: Constants.PidlOperationType.Update), null, null, unionPayInternationalHeaders));

            return Validate(set);
        }

        private static List<Test> PaymentMethodDescriptionAddGlobalPISet()
        {
            List<Test> set = new List<Test>();
            var pmGroupingHeaders = new Dictionary<string, string>()
            {
                {
                    "x-ms-flight", "enableGlobalPiInAddResource"
                }
            };

            var xboxnativeGlobalPIStyleHintsHeaders = new Dictionary<string, string>
            {
                { "x-ms-flight", "enableGlobalPiInAddResource,PXEnableXboxNativeStyleHints" },
                { "x-ms-pidlsdk-version", "2.7.0" }
            };

            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "nl", language: "en-us", partner: "cart", family: "online_bank_transfer", type: "paysafecard", operation: Constants.PidlOperationType.Add), null, null, pmGroupingHeaders));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "us", language: "en-us", partner: "storify", family: "online_bank_transfer", type: "paysafecard", operation: Constants.PidlOperationType.Add), null, null, pmGroupingHeaders));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "us", language: "en-us", partner: "xboxsettings", family: "online_bank_transfer", type: "paysafecard", operation: Constants.PidlOperationType.Add), null, null, pmGroupingHeaders));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "us", language: "en-us", partner: "storify", family: "online_bank_transfer", type: "paysafecard", operation: Constants.PidlOperationType.Add), null, null, xboxnativeGlobalPIStyleHintsHeaders));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "us", language: "en-us", partner: "xboxsettings", family: "online_bank_transfer", type: "paysafecard", operation: Constants.PidlOperationType.Add), null, null, xboxnativeGlobalPIStyleHintsHeaders));
            return Validate(set);
        }

        /// <summary>
        /// Applies blacklisting on a given set
        /// See Constants.DiffTest.SkipPidlCombinations
        /// </summary>
        /// <param name="unverifiedSet">set of tests</param>
        /// <returns>filtered set</returns>
        private static List<Test> Validate(List<Test> unverifiedSet)
        {
            List<Test> verifiedSet = new List<Test>();

            foreach (Test element in unverifiedSet)
            {
                bool isSkipable = false;
                foreach (TestRequestRelativePath toSkip in Constants.DiffTest.SkipPidlCombinations)
                {
                    if (toSkip.UserType != element.Path.UserType || toSkip.ResourceName != element.Path.ResourceName)
                    {
                        continue;
                    }

                    bool allKeysMatch = true;
                    foreach (string key in toSkip.Keys)
                    {
                        // skip blank entry in skip dictionary
                        if (toSkip[key] == null)
                        {
                            continue;
                        }

                        // check if not set parameters should be skipped
                        if (element.Path[key] == null)
                        {
                            if (toSkip[key] != Constants.DiffTest.NotSent)
                            {
                                allKeysMatch = false;
                                break;
                            }
                        }
                        else if (toSkip[key] != element.Path[key])
                        {
                            allKeysMatch = false;
                            break;
                        }
                    }

                    if (allKeysMatch)
                    {
                        isSkipable = true;
                        break;
                    }
                }

                if (!isSkipable)
                {
                    verifiedSet.Add(element);
                }
            }

            return verifiedSet;
        }

        /// <summary>
        /// Creates a set of test data for the addressDescription api endpoint
        /// </summary>
        /// <returns>relative url string + body content</returns>
        private List<Test> AddressDescriptionSet()
        {
            const string ResourceName = "addressDescriptions";
            List<Test> set = new List<Test>();
            List<string> xboxNativeAddressCountries = new List<string> { "us", "gb", "br", "in", "be", "fr" };
            Dictionary<string, string> xboxNativeStyleHintsTestHeaders = new Dictionary<string, string>()
            {
                {
                    "x-ms-flight", "PXEnableXboxNativeStyleHints"
                },
                {
                    "x-ms-pidlsdk-version", "2.7.0"
                }
            };

            // for commercialstores HapiServiceUsageAddress with taxId checkbox
            foreach (string country in Constants.CommercialAddressCountriesTest)
            {
                set.Add(
                    new Test(
                        new TestRequestRelativePath(
                            Constants.UserTypes.UserMyOrg,
                            ResourceName,
                            country: country,
                            type: Constants.AddressTypeNames.HapiServiceUsageAddress,
                            language: "en-us",
                            partner: Constants.PartnerNames.Commercialstores),
                        null));
                set.Add(
                    new Test(
                        new TestRequestRelativePath(
                            Constants.UserTypes.UserMyOrg,
                            ResourceName,
                            country: country,
                            type: Constants.AddressTypeNames.HapiServiceUsageAddress,
                            language: "en-us",
                            partner: Constants.TemplateNames.DefaultTemplate),
                        null));
            }

            foreach (string country in this.Countries)
            {
                foreach (string type in Constants.AddressTypes)
                {
                    foreach (string lang in this.Languages)
                    {
                        var supportedPartners = this.Partners;

                        if (string.Equals(type, Constants.AddressTypeNames.BillingGroup, StringComparison.OrdinalIgnoreCase))
                        {
                            supportedPartners = new List<string> { Constants.PartnerNames.Commercialstores, Constants.PartnerNames.Azure, Constants.PartnerNames.AzureSignup, Constants.PartnerNames.AzureIbiza };
                        }
                        else if (string.Equals(type, Constants.AddressTypeNames.HapiServiceUsageAddress, StringComparison.OrdinalIgnoreCase))
                        {
                            supportedPartners = new List<string> { Constants.PartnerNames.Commercialstores, Constants.TemplateNames.DefaultTemplate };
                        }
                        else if (string.Equals(type, Constants.AddressTypeNames.Organization, StringComparison.OrdinalIgnoreCase)
                            || string.Equals(type, Constants.AddressTypeNames.Individual, StringComparison.OrdinalIgnoreCase))
                        {
                            supportedPartners = new List<string> { "commercialstores", "azure", "azuresignup", "azureibiza" };
                        }
                        else if (string.Equals(type, Constants.AddressTypeNames.HapiV1SoldToOrganization, StringComparison.OrdinalIgnoreCase)
                                || string.Equals(type, Constants.AddressTypeNames.HapiV1ShipToOrganization, StringComparison.OrdinalIgnoreCase)
                                || string.Equals(type, Constants.AddressTypeNames.HapiV1BillToOrganization, StringComparison.OrdinalIgnoreCase)
                                || string.Equals(type, Constants.AddressTypeNames.HapiV1SoldToIndividual, StringComparison.OrdinalIgnoreCase)
                                || string.Equals(type, Constants.AddressTypeNames.HapiV1BillToIndividual, StringComparison.OrdinalIgnoreCase))
                        {
                            supportedPartners = new List<string> { "commercialstores", "azure", "azuresignup", "azureibiza" };

                            if (string.Equals(type, Constants.AddressTypeNames.HapiV1SoldToIndividual, StringComparison.OrdinalIgnoreCase))
                            {
                                supportedPartners.AddRange(new List<string> { Constants.TemplateNames.DefaultTemplate, Constants.PartnerNames.OfficeSMB });
                            }
                        }
                        else if (string.Equals(type, Constants.AddressTypeNames.HapiV1ShipToIndividual, StringComparison.OrdinalIgnoreCase))
                        {
                            supportedPartners = new List<string> { "azure", "azuresignup", "azureibiza" };
                        }
                        else if (string.Equals(type, Constants.AddressTypeNames.HapiV1, StringComparison.OrdinalIgnoreCase))
                        {
                            supportedPartners = new List<string> { Constants.TemplateNames.DefaultTemplate };
                        }
                        else if (string.Equals(type, Constants.AddressTypeNames.OrgAddress, StringComparison.OrdinalIgnoreCase))
                        {
                            supportedPartners = new List<string> { Constants.TemplateNames.DefaultTemplate };
                        }

                        foreach (string partner in supportedPartners)
                        {
                            if (string.Equals(partner, Constants.PartnerNames.Commercialstores, StringComparison.OrdinalIgnoreCase) || string.Equals(partner, Constants.TemplateNames.DefaultTemplate, StringComparison.OrdinalIgnoreCase))
                            {
                                if (string.Equals(type, Constants.AddressTypeNames.Shipping, StringComparison.OrdinalIgnoreCase))
                                {
                                    set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, type: type, language: lang, partner: partner, scenario: Constants.ScenariosForAddress.CommercialHardware), null));
                                }
                                else if (string.Equals(type, Constants.AddressTypeNames.HapiServiceUsageAddress, StringComparison.OrdinalIgnoreCase))
                                {
                                    set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, type: type, language: lang, partner: partner, operation: Constants.PidlOperationType.Add), null));
                                    set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, type: type, language: lang, partner: partner, operation: Constants.PidlOperationType.Update), null));
                                    continue;
                                }
                                else if (string.Equals(type, Constants.AddressTypeNames.HapiV1SoldToOrganization, StringComparison.OrdinalIgnoreCase)
                                        || string.Equals(type, Constants.AddressTypeNames.HapiV1ShipToOrganization, StringComparison.OrdinalIgnoreCase)
                                        || string.Equals(type, Constants.AddressTypeNames.HapiV1BillToOrganization, StringComparison.OrdinalIgnoreCase)
                                        || string.Equals(type, Constants.AddressTypeNames.HapiV1SoldToIndividual, StringComparison.OrdinalIgnoreCase)
                                        || string.Equals(type, Constants.AddressTypeNames.HapiV1ShipToIndividual, StringComparison.OrdinalIgnoreCase)
                                        || string.Equals(type, Constants.AddressTypeNames.HapiV1BillToIndividual, StringComparison.OrdinalIgnoreCase))
                                {
                                    set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, type: type, language: lang, partner: partner, operation: Constants.PidlOperationType.Add), null));
                                    set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, type: type, language: lang, partner: partner, operation: Constants.PidlOperationType.Update), null));

                                    if (string.Equals(type, Constants.AddressTypeNames.HapiV1SoldToOrganization, StringComparison.OrdinalIgnoreCase))
                                    {
                                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, type: type, language: lang, partner: partner, operation: Constants.PidlOperationType.Add, scenario: Constants.ScenariosForAddress.CreateBillingAccount), null));
                                    }

                                    continue;
                                }

                                if (string.Equals(type, Constants.AddressTypeNames.Billing, StringComparison.OrdinalIgnoreCase)
                                    && string.Equals(partner, Constants.TemplateNames.DefaultTemplate, StringComparison.OrdinalIgnoreCase))
                                {
                                    set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, type: type, language: lang, partner: partner, operation: Constants.PidlOperationType.Add, scenario: Constants.ScenarioNames.ModernAccount), null));
                                    continue;
                                }
                            }

                            if ((string.Equals(partner, Constants.PartnerNames.Azure, StringComparison.OrdinalIgnoreCase) || string.Equals(partner, Constants.PartnerNames.AzureSignup, StringComparison.OrdinalIgnoreCase) || string.Equals(partner, Constants.PartnerNames.AzureIbiza, StringComparison.OrdinalIgnoreCase))
                                && (string.Equals(type, "shipping", StringComparison.OrdinalIgnoreCase) || string.Equals(type, "billinggroup", StringComparison.OrdinalIgnoreCase) || string.Equals(type, "billing", StringComparison.OrdinalIgnoreCase)))
                            {
                                set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.Anonymous, ResourceName, country: country, type: type, language: lang, partner: partner), null));
                                continue;
                            }

                            if (string.Equals(type, "organization", StringComparison.OrdinalIgnoreCase) || string.Equals(type, "individual", StringComparison.OrdinalIgnoreCase))
                            {
                                set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.Anonymous, ResourceName, country: country, type: type, language: lang, partner: partner), null));
                                continue;
                            }

                            // For appsource and bing, only test anonymous scenario
                            // Use "continue" to skip the common case
                            if ((string.Equals(partner, Constants.PartnerNames.AppSource, StringComparison.OrdinalIgnoreCase) && string.Equals(type, "shipping", StringComparison.OrdinalIgnoreCase))
                                || (string.Equals(partner, Constants.PartnerNames.Bing, StringComparison.OrdinalIgnoreCase) && string.Equals(type, "billing", StringComparison.OrdinalIgnoreCase)))
                            {
                                set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.Anonymous, ResourceName, country: country, type: type, language: lang, partner: partner), null));
                                continue;
                            }

                            // Add avsSuggest & setAsDefaultBilling tests for BAG and OMEX partners in US
                            if (string.Equals(country, "us", StringComparison.OrdinalIgnoreCase))
                            {
                                if (string.Equals(partner, Constants.PartnerNames.Cart, StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(partner, Constants.PartnerNames.OfficeOobe, StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(partner, Constants.PartnerNames.OXOOobe, StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(partner, Constants.PartnerNames.SmbOobe, StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(partner, Constants.PartnerNames.Webblends, StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(partner, Constants.PartnerNames.WebblendsInline, StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(partner, Constants.PartnerNames.OxoWebDirect, StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(partner, Constants.PartnerNames.OxoDIME, StringComparison.OrdinalIgnoreCase))
                                {
                                    var requestRelativePath = new TestRequestRelativePath(
                                        Constants.UserTypes.UserMe,
                                        ResourceName,
                                        country: country,
                                        type: type,
                                        language: lang,
                                        partner: partner,
                                        avsSuggest: true,
                                        setAsDefaultBilling: true);
                                    set.Add(new Test(requestRelativePath, null));
                                }
                            }

                            if (string.Equals(partner, Constants.PartnerNames.OxoWebDirect, StringComparison.OrdinalIgnoreCase)
                                || string.Equals(partner, Constants.PartnerNames.OxoDIME, StringComparison.OrdinalIgnoreCase)
                                || string.Equals(partner, Constants.PartnerNames.Webblends, StringComparison.OrdinalIgnoreCase)
                                || string.Equals(partner, Constants.PartnerNames.WebblendsInline))
                            {
                                set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, type: type, language: lang, partner: partner, scenario: Constants.ScenarioNames.DisplayOptionalFields), null));
                            }

                            if (string.Equals(partner, Constants.PartnerNames.OfficeSMB, StringComparison.OrdinalIgnoreCase))
                            {
                                set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, type: type, operation: Constants.PidlOperationType.Add, language: lang, partner: partner), null));
                                set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, type: type, operation: Constants.PidlOperationType.Update, language: lang, partner: partner), null));
                                continue;
                            }

                            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "ca", type: type, language: lang, partner: Constants.PartnerNames.Cart), null));
                            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "nz", type: type, language: lang, partner: Constants.PartnerNames.Cart), null));
                            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "sv", type: type, language: lang, partner: Constants.PartnerNames.Cart), null));
                        }
                    }
                }
            }

            // profile address tests for xboxnative partners
            foreach (string country in xboxNativeAddressCountries)
            {
                set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, type: "jarvis_v3", scenario: "profile", partner: Constants.PartnerNames.Storify, language: "en-US", operation: Constants.PidlOperationType.Add, setAsDefaultBilling: true), null));
                set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, type: "jarvis_v3", scenario: "profile", partner: Constants.PartnerNames.XboxSettings, language: "en-US", operation: Constants.PidlOperationType.Add, setAsDefaultBilling: true), null));
                set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, type: "jarvis_v3", scenario: "profile", partner: Constants.PartnerNames.Storify, language: "en-US", operation: Constants.PidlOperationType.Add, setAsDefaultBilling: true), null, additionalHeaders: xboxNativeStyleHintsTestHeaders));
                set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, type: "jarvis_v3", scenario: "profile", partner: Constants.PartnerNames.XboxSettings, language: "en-US", operation: Constants.PidlOperationType.Add, setAsDefaultBilling: true), null, additionalHeaders: xboxNativeStyleHintsTestHeaders));
            }

            // shipping address tests for xboxnative partners
            foreach (string country in xboxNativeAddressCountries)
            {
                set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, type: "jarvis_v3", scenario: "shipping", partner: Constants.PartnerNames.Storify, language: "en-US", operation: Constants.PidlOperationType.Add), null));
                set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, type: "jarvis_v3", scenario: "shipping", partner: Constants.PartnerNames.XboxSettings, language: "en-US", operation: Constants.PidlOperationType.Add), null));
                set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, type: "jarvis_v3", scenario: "shipping", partner: Constants.PartnerNames.Storify, language: "en-US", operation: Constants.PidlOperationType.Add), null, additionalHeaders: xboxNativeStyleHintsTestHeaders));
                set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, type: "jarvis_v3", scenario: "shipping", partner: Constants.PartnerNames.XboxSettings, language: "en-US", operation: Constants.PidlOperationType.Add), null, additionalHeaders: xboxNativeStyleHintsTestHeaders));
            }

            // billing address tests for xboxnative partners
            foreach (string country in xboxNativeAddressCountries)
            {
                set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, type: "jarvis_v3", scenario: "billing", partner: Constants.PartnerNames.Storify, language: "en-US", operation: Constants.PidlOperationType.Add), null));
                set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, type: "jarvis_v3", scenario: "billing", partner: Constants.PartnerNames.XboxSettings, language: "en-US", operation: Constants.PidlOperationType.Add), null));
                set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, type: "jarvis_v3", scenario: "billing", partner: Constants.PartnerNames.Storify, language: "en-US", operation: Constants.PidlOperationType.Add), null, additionalHeaders: xboxNativeStyleHintsTestHeaders));
                set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, type: "jarvis_v3", scenario: "billing", partner: Constants.PartnerNames.XboxSettings, language: "en-US", operation: Constants.PidlOperationType.Add), null, additionalHeaders: xboxNativeStyleHintsTestHeaders));
            }

            return Validate(set);
        }

        /// <summary>
        /// Creates a set of test data for the billingGroupDescription api endpoint
        /// </summary>
        /// <returns>relative url string + body content</returns>
        private List<Test> BillingGroupDescriptionSet()
        {
            const string ResourceName = "billingGroupDescriptions";
            List<Test> set = new List<Test>();

            // Test list billing gourp client prefill, operation selectinstance
            foreach (string country in this.Countries)
            {
                foreach (string partner in Constants.CommercialPartners)
                {
                    foreach (string lang in this.Languages)
                    {
                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, partner: partner, language: lang, operation: Constants.PidlOperationType.SelectInstance), null));
                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, partner: partner, language: lang, operation: Constants.PidlOperationType.Add, type: Constants.TypesForBillingGroup.Lightweight), null));
                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, partner: partner, language: lang, operation: Constants.PidlOperationType.Update, type: Constants.TypesForBillingGroup.Lightweight, scenario: Constants.ScenariosForBillingGroup.BillingGroupPONumber), null));
                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, partner: partner, language: lang, operation: Constants.PidlOperationType.SelectInstance, type: Constants.TypesForBillingGroup.LightweightV7), null));
                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, partner: partner, language: lang, operation: Constants.PidlOperationType.Add, type: Constants.TypesForBillingGroup.LightweightV7), null));
                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, partner: partner, language: lang, operation: Constants.PidlOperationType.Update, type: Constants.TypesForBillingGroup.LightweightV7, scenario: Constants.ScenariosForBillingGroup.BillingGroupPONumber), null));
                    }
                }
            }

            return Validate(set);
        }

        /// <summary>
        /// Creates a set of test data for the profileDescription api endpoint
        /// These tests apply to both Local and Server cases
        /// </summary>
        /// <returns>relative url string + body content</returns>
        private List<Test> ProfileDescriptionBasicSet()
        {
            const string ResourceName = "profileDescriptions";
            List<Test> set = new List<Test>();

            // Test consumer profile, operation: update
            // Test consumerV3 profile, operation: add
            foreach (string country in this.Countries)
            {
                foreach (string type in Constants.ConsumerProfileTypes)
                {
                    // Detailed test cases with partner and operation
                    foreach (string partner in Constants.ProfileConsumerPartners)
                    {
                        foreach (string lang in this.Languages)
                        {
                            string operation = string.Equals(type, Constants.ProfileType.ConsumerV3, StringComparison.OrdinalIgnoreCase) ? Constants.PidlOperationType.Add : Constants.PidlOperationType.Update;
                            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, type: type, language: lang, partner: partner, operation: operation), null));
                        }
                    }
                }
            }

            // Test add employee profile, operation add
            foreach (string country in Constants.CommercialProfileCountriesTest)
            {
                foreach (string partner in Constants.CommercialPartners)
                {
                    foreach (string lang in this.Languages)
                    {
                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, type: Constants.ProfileType.Employee, partner: partner, language: lang, operation: Constants.PidlOperationType.Add), null));
                    }
                }
            }

            // Test legalentity profile, operation show
            // Test legalentity profile, operation update
            // Test organization profile, operation update
            // Test organization profile, operation update, scenario twoColumns
            foreach (string country in Constants.CommercialProfileCountriesTest)
            {
                foreach (string partner in Constants.CommercialPartners)
                {
                    foreach (string lang in this.Languages)
                    {
                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMyOrg, ResourceName, country: country, type: Constants.ProfileType.Legal, partner: partner, language: lang, operation: Constants.PidlOperationType.Show), null));
                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMyOrg, ResourceName, country: country, type: Constants.ProfileType.Legal, partner: partner, language: lang, operation: Constants.PidlOperationType.Update), null));
                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMyOrg, ResourceName, country: country, type: Constants.ProfileType.Organization, partner: partner, language: lang, operation: Constants.PidlOperationType.Update), null));
                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMyOrg, ResourceName, country: country, type: Constants.ProfileType.Organization, partner: partner, language: lang, operation: Constants.PidlOperationType.Update, scenario: Constants.ScenarioNames.TwoColumns), null));                        
                    }
                }
            }

            //// Test consumer: profile, operation: update for xboxnative partners

            foreach (string country in Constants.CommercialProfileCountriesTest)
            {
                foreach (string lang in this.Languages)
                {
                    set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, type: Constants.ProfileType.Consumer, partner: Constants.PartnerNames.Storify, language: lang, operation: Constants.PidlOperationType.Update), null, additionalHeaders: XboxNativeStyleHintsHeaders));
                    set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, type: Constants.ProfileType.Consumer, partner: Constants.PartnerNames.XboxSettings, language: lang, operation: Constants.PidlOperationType.Update), null, additionalHeaders: XboxNativeStyleHintsHeaders));
                }
            }

            return Validate(set);
        }

        /// <summary>
        /// Creates a set of test data for the profileDescription api endpoint
        /// These tests only apply to Server case
        /// </summary>
        /// <returns>relative url string + body content</returns>
        private List<Test> ProfileDescriptionRemoteServerSet()
        {
            const string ResourceName = "profileDescriptions";
            List<Test> set = new List<Test>();

            // Test employee and organization profile, operation: update
            foreach (string country in Constants.CommercialProfileCountriesTest)
            {
                foreach (string partner in Constants.CommercialPartners)
                {
                    foreach (string lang in this.Languages)
                    {
                        // Test employee profile
                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, type: Constants.ProfileType.Employee, partner: partner, language: lang, operation: Constants.PidlOperationType.Update), null, JarvisTestHeader));

                        // Test organization profile, server side prefilling
                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMyOrg, ResourceName, country: country, type: Constants.ProfileType.Organization, partner: partner, language: lang, operation: Constants.PidlOperationType.Update), null, JarvisTestHeader));
                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMyOrg, ResourceName, country: country, type: Constants.ProfileType.Organization, partner: Constants.TemplateNames.DefaultTemplate, language: lang, operation: Constants.PidlOperationType.Update), null, JarvisTestHeader));
                    }
                }
            }

            return Validate(set);
        }

        /// <summary>
        /// Creates a set of test data for the ChallengeDescription api endpoint
        /// </summary>
        /// <returns>relative url string + body content</returns>
        private List<Test> ChallengeDescriptionSet()
        {
            const string ResourceName = "challengeDescriptions";
            List<Test> set = new List<Test>();
            var xboxnativeStyleHintsHeaders = XboxNativeStyleHintsHeaders;
            Dictionary<string, string> validateSmsChallengeHeaders = new Dictionary<string, string>
            {
                { "x-ms-flight", "PXEnableSMSChallengeValidation" }
            };

            foreach (string lang in this.Languages)
            {
                foreach (string type in Constants.ChallengeTypes.Keys)
                {
                    foreach (string partner in Constants.ChallengeTypes[type])
                    {
                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, type: type, language: lang, partner: partner), null));
                        if (Constants.IsXboxNativePartner(partner))
                        {
                            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, type: type, language: lang, partner: partner), null, null, xboxnativeStyleHintsHeaders));
                        }
                    }
                }
            }

            foreach (string lang in this.Languages)
            {
                set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, type: "cvv", language: lang, partner: "bing", piid: "q62zBAAAAAAJAACA"), null, "px.pims.cc.add.success"));
                set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, type: "cvv", language: lang, partner: "bing", piid: "q62zBAAAAAAJAACA", sessionId: "dummySessionId"), null, "px.pims.cc.add.success"));
            }

            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, type: "cvv", piid: "q62zBAAAAAAJAACA", partner: "amcweb", operation: Constants.PidlOperationType.RenderPidlPage, country: "GB", language: "en-US"), null, "px.pims.cc.add.success"));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, type: "sms", piid: "cfbef3e8-145e-4d94-8eb8-8d9875e614f", partner: "amcweb", operation: Constants.PidlOperationType.RenderPidlPage, country: "US", language: "en-US"), null, "px.pims.handlepaymentchallenge.sms.success"));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, type: "sms", piid: "cfbef3e8-145e-4d94-8eb8-8d9875e614f", partner: "amcweb", country: "US", language: "en-US"), null, "px.pims.handlepaymentchallenge.sms.success", validateSmsChallengeHeaders));

            string paymentSessionData = "{\"amount\":10.3,\"billableAccountId\":\"\",\"challengeScenario\":\"PaymentTransaction\",\"challengeWindowSize\":\"03\",\"classicProduct\":\"\",\"currency\":\"usd\",\"country\":\"de\",\"language\":\"en-us\",\"partner\":\"webblends\",\"piid\":\"lwFOBQAAAAABAACA\"}";
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, paymentSessionOrData: paymentSessionData, timezoneOffSet: "420", operation: "RenderPidlPage"), null));

            ////Creates a set of test data for the ChallengeDescription api endpoint India 3DS purchase challenge
            foreach (string partner in Constants.ChallengeTypes3DS)
            {
                string environment = this.configuration["UnderTestEnvironment"];
                string piCid = (environment == "PROD" || environment == "PPE")
                    ? "8e342cdc-771b-4b19-84a0-bef4c44911f7"
                    : "a389d54a-a4b0-4fc4-8de0-2070802b4885";

                string paymentSessionData3DS = $"{{\"id\":\"\",\"isChallengeRequired\":false,\"challengeStatus\":\"\",\"signature\":\"\",\"amount\":100,\"challengeScenario\":\"PaymentTransaction\",\"challengeWindowSize\":\"05\",\"currency\":\"INR\",\"country\":\"IN\",\"language\":\"en-US\",\"partner\":\"{partner}\",\"piCid\":\"{piCid}\",\"piid\":\"gyTangAAAAACAACA\"}}";
                set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, paymentSessionOrData: paymentSessionData3DS, timezoneOffSet: "420", operation: "RenderPidlPage"), null, "px-service-3ds1-test-emulator,px.pims.3ds"));
            }

            return Validate(set);
        }

        /// <summary>
        /// Creates a set of test data for the RewardsDescription api endpoint
        /// </summary>
        /// <returns>relative url string + body content</returns>
        private List<Test> RewardsDescriptionsSet()
        {
            const string ResourceName = "rewardsDescriptions";

            var rewardsContextData = new
            {
                OrderAmount = 10,
                Currency = "usd"
            };
            string rewardsContext = JsonConvert.SerializeObject(rewardsContextData);

            var noRewardsContextData = new
            {
                OrderAmount = 0,
                Currency = "usd"
            };
            string noRewardsContext = JsonConvert.SerializeObject(noRewardsContextData);

            var headers = new Dictionary<string, string>()
            {
                {
                    "x-ms-msaprofile", "PUID=OTg1MTU0NDIwNDI3ODYz,emailAddress=a293c2hpa190ZXN0MTFAb3V0bG9vay5jb20="
                },
                {
                    "x-ms-test", string.Format("{{scenarios: \"{0}\", contact: \"{1}\"}}", "px.partnersettings.windowsstore,px.msrewards.success,px.pims.listpi.success", "DiffTest")
                }
            };

            List<Test> set = new List<Test>();
            foreach (string partner in Constants.MSRewardsPartners)
            {
                foreach (string operation in this.MSRewardsOperations)
                {
                    set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, type: "msrewards", language: "en-US", operation: operation, partner: partner, country: "US", rewardsContextData: rewardsContext), null, additionalHeaders: headers));
                    set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, type: "msrewards", language: "en-US", operation: operation, partner: partner, country: "US", rewardsContextData: noRewardsContext), null, additionalHeaders: headers));
                }
            }

            return Validate(set);
        }

        /// <summary>
        /// Handle checkout DiffTest added for different scenarios
        /// </summary>
        /// <returns>relative url string + body content</returns>
        private List<Test> CheckoutDescriptions()
        {
            const string ResourceName = "checkoutDescriptions";
            List<Test> set = new List<Test>();

            foreach (string lang in this.Languages)
            {
                foreach (var testScenario in Constants.ThirdPartyPaymentTestScenarios)
                {
                    var headers = new Dictionary<string, string>()
                    {
                        {
                            "x-ms-test", string.Format("{{scenarios: \"{0}\", contact: \"{1}\"}}", testScenario, "DiffTest")
                        }
                    };

                    set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.Anonymous, ResourceName, redirectUrl: "pay.microsoft.com", paymentProviderId: "paypal", language: lang, partner: Constants.TemplateNames.DefaultTemplate, country: "sj", checkoutId: "123", operation: Constants.PidlOperationType.RenderPidlPage), null, additionalHeaders: headers));
                }
            }

            return Validate(set);
        }

        /// <summary>
        /// Creates a set of test data for the TaxIdDescription api endpoint
        /// </summary>
        /// <returns>relative url string + body content</returns>
        private List<Test> TaxIdDescriptionSet()
        {
            const string ResourceName = "taxIdDescriptions";
            List<Test> set = new List<Test>();

            // Test consumer tax scenario
            foreach (string country in Constants.CountriesTest)
            {
                foreach (string partner in Constants.TaxIdConsumerPartners)
                {
                    foreach (string lang in this.Languages)
                    {
                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, type: Constants.ScenariosForTaxId.ConsumerTaxId, partner: partner, language: lang), null));
                    }
                }
            }

            // Test commercial tax scenario
            foreach (string country in Constants.CommercialTaxCountriesTest)
            {
                foreach (string partner in Constants.CommercialPartners)
                {
                    foreach (string lang in this.Languages)
                    {
                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMyOrg, ResourceName, country: country, type: Constants.ScenariosForTaxId.CommercialTaxId, partner: partner, language: lang), null));

                        // Test standalone tax pidl by passing operation parameter
                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMyOrg, ResourceName, country: country, type: Constants.ScenariosForTaxId.CommercialTaxId, partner: partner, language: lang, operation: Constants.PidlOperationType.Add), null));
                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMyOrg, ResourceName, country: country, type: Constants.ScenariosForTaxId.CommercialTaxId, partner: partner, language: lang, operation: Constants.PidlOperationType.Add, scenario: Constants.ScenarioNames.WithCountryDropdown), null));
                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMyOrg, ResourceName, country: country, type: Constants.ScenariosForTaxId.CommercialTaxId, partner: partner, language: lang, operation: Constants.PidlOperationType.Update, scenario: Constants.ScenarioNames.WithCountryDropdown), null));

                        // Test standalone tax pidl for departmental purchase
                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMyOrg, ResourceName, country: country, type: Constants.ScenariosForTaxId.CommercialTaxId, partner: partner, language: lang, operation: Constants.PidlOperationType.Add, scenario: Constants.ScenarioNames.DepartmentalPurchase), null));
                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMyOrg, ResourceName, country: country, type: Constants.ScenariosForTaxId.CommercialTaxId, partner: partner, language: lang, operation: Constants.PidlOperationType.Update, scenario: Constants.ScenarioNames.DepartmentalPurchase), null));
                    }
                }
            }

            foreach (string country in Constants.AzureTaxCountriesTest)
            {
                foreach (string partner in Constants.AzurePartners)
                {
                    foreach (string lang in this.Languages)
                    {
                        // Test standalone tax pidl by passing operation parameter
                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMyOrg, ResourceName, country: country, type: Constants.ScenariosForTaxId.CommercialTaxId, partner: partner, language: lang, operation: Constants.PidlOperationType.Add, scenario: Constants.ScenarioNames.WithCountryDropdown), null));
                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMyOrg, ResourceName, country: country, type: Constants.ScenariosForTaxId.CommercialTaxId, partner: partner, language: lang, operation: Constants.PidlOperationType.Update, scenario: Constants.ScenarioNames.WithCountryDropdown), null));
                    }
                }
            }

            // Test commercial tax scenario
            foreach (string country in Constants.CommercialTaxCountriesTest)
            {
                foreach (string lang in this.Languages)
                {
                    // Test standalone tax pidl by passing operation parameter
                    set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMyOrg, ResourceName, country: country, type: Constants.ScenariosForTaxId.CommercialTaxId, partner: Constants.TemplateNames.DefaultTemplate, language: lang, operation: Constants.PidlOperationType.Add), null));
                }
            }

            return Validate(set);
        }

        /// <summary>
        /// Creates a set of test data for the paymentMethodDescription api endpoint
        /// </summary>
        /// <returns>relative url string + body content</returns>
        private List<Test> PaymentMethodDescriptionSet()
        {
            const string ResourceName = "paymentMethodDescriptions";
            List<Test> set = new List<Test>();
            HashSet<string> comboKeys = null;

            // generate selectPM tests
            set.AddRange(this.PaymentMethodDescriptionsSelectSet());

            // generate listPi tests
            set.AddRange(this.PaymentMethodDescriptionsListPiClientPrefilling());
            set.AddRange(PaymentMethodDescriptionsListPiIndiaExpiryDelete());
            set.AddRange(PaymentMethodDescriptionsListPiEnableIndiaTokenExpiryDetails());
            set.AddRange(PaymentMethodDescriptionsListPISet());

            // generate show/search Pi tests
            set.AddRange(this.PaymentMethodDescriptionsShowOrSearchPi());

            // generate get virtual PI tests
            set.AddRange(this.PaymentMethodDescriptionVirtualPaymentsSet());

            set.AddRange(this.PaymentMethodDescriptionListScenarioSet());

            set.AddRange(this.PaymentMethodDescriptionsFeatureFlight());

            set.AddRange(PaymentMethodDescriptionPMGroupingSet());

            set.AddRange(PaymentMethodDescriptionPMGroupingSetForXboxNative());

            set.AddRange(PaymentMethodDescriptionRedeemGiftCardForXboxNative());

            set.AddRange(PaymentMethodDescriptionAddGlobalPISet());

            set.AddRange(PaymentMethodDescriptionAddGlobalPISet());

            set.AddRange(PaymentMethodDescriptionsAddKakaopayForXboxNative());

            set.AddRange(PaymentMethodDescriptionsAddPiIndiaUPIEnable());

            set.AddRange(PaymentMethodDescriptionsAddANTBatchPi());

            set.AddRange(PaymentMethodDescriptionsAddAlipayCNPi());

            set.AddRange(PaymentMethodDescriptionsApplyXboxCoBrandedCardForXboxNative());

            set.AddRange(PaymentMethodDescriptionsApplyWebPartners());

            set.AddRange(PaymentMethodDescriptionsAddVenmo());

            set.AddRange(PaymentMethodDescriptionsAddPiIndiaUPICommercialEnable());

            set.AddRange(PaymentMethodDescriptionsAddPiIndiaRupayEnable());

            set.AddRange(PaymentMethodDescriptionsWindowsStore());

            set.AddRange(PaymentMethodDescriptionsUnionPayInternational());

            set.AddRange(PaymentMethodDescriptionsDeleteSet());

            set.AddRange(PaymentMethodDescriptionsAddNSM());

            set.AddRange(PaymentMethodDescriptionsXboxNativeAddUpdateCC());

            set.AddRange(PaymentMethodDescriptionsAddPiSepaWithJpmcEnable());

            set.AddRange(ExpressCheckoutTests());

            set.AddRange(PaymentMethodDescriptionSelectPMForSepaWithNewlogo());

            try
            {
                StreamReader file = new StreamReader(@"..\\..\\DiffTest\\ConfigFiles\\escapeCases_paymentMethodDescriptions.txt");
                string[] parameterNames = new string[] { "country", "family", "language", "partner", "operation", "type", "scenario" };
                comboKeys = new HashSet<string>();
                string queryString = string.Empty;

                while ((queryString = file.ReadLine()) != null)
                {
                    string key = string.Empty;
                    var parsedCollection = HttpUtility.ParseQueryString(queryString);
                    foreach (string name in parameterNames)
                    {
                        string currentValue = parsedCollection.Get(name);
                        if (currentValue != null)
                        {
                            key += currentValue + ",";
                        }
                    }

                    comboKeys.Add(key);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            set.AddRange(this.PaymentMethodDescriptionsPaymentMethodsInCountriesSet(comboKeys));

            Dictionary<string, string> additionalHeadersForIndiaTokenization = new Dictionary<string, string>()
            {
                {
                    "x-ms-flight", Constants.FlightNames.IndiaTokenizationConsentCapture
                }
            };

            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "mc", language: "en-us", partner: Constants.PartnerNames.Azure, operation: "add"), null, null, additionalHeaders: additionalHeadersForIndiaTokenization));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "visa", language: "en-us", partner: Constants.PartnerNames.Azure, operation: "add"), null, null, additionalHeaders: additionalHeadersForIndiaTokenization));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "mc", language: "en-us", partner: Constants.PartnerNames.Commercialstores, operation: "add"), null, null, additionalHeaders: additionalHeadersForIndiaTokenization));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "visa", language: "en-us", partner: Constants.PartnerNames.Commercialstores, operation: "add"), null, null, additionalHeaders: additionalHeadersForIndiaTokenization));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "visa", language: "en-us", partner: Constants.PartnerNames.Azure, operation: "add", completePrerequisites: bool.TrueString.ToLower()), null, null, additionalHeaders: additionalHeadersForIndiaTokenization));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "mc", language: "en-us", partner: Constants.PartnerNames.Azure, operation: "add", completePrerequisites: bool.TrueString.ToLower()), null, null, additionalHeaders: additionalHeadersForIndiaTokenization));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMyOrg, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "visa", language: "en-us", partner: Constants.PartnerNames.Azure, operation: "add", completePrerequisites: bool.TrueString.ToLower()), null, null, additionalHeaders: additionalHeadersForIndiaTokenization));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMyOrg, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "mc", language: "en-us", partner: Constants.PartnerNames.Azure, operation: "add", completePrerequisites: bool.TrueString.ToLower()), null, null, additionalHeaders: additionalHeadersForIndiaTokenization));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "visa", language: "en-us", partner: Constants.PartnerNames.Commercialstores, operation: "add", completePrerequisites: bool.TrueString.ToLower()), null, null, additionalHeaders: additionalHeadersForIndiaTokenization));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "mc", language: "en-us", partner: Constants.PartnerNames.Commercialstores, operation: "add", completePrerequisites: bool.TrueString.ToLower()), null, null, additionalHeaders: additionalHeadersForIndiaTokenization));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMyOrg, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "visa", language: "en-us", partner: Constants.PartnerNames.Commercialstores, operation: "add", completePrerequisites: bool.TrueString.ToLower()), null, null, additionalHeaders: additionalHeadersForIndiaTokenization));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMyOrg, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "mc", language: "en-us", partner: Constants.PartnerNames.Commercialstores, operation: "add", completePrerequisites: bool.TrueString.ToLower()), null, null, additionalHeaders: additionalHeadersForIndiaTokenization));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "us", family: "credit_card", type: "visa", language: "en-us", partner: "xboxsettings", operation: "Delete"), null, null, additionalHeaders: additionalHeadersForIndiaTokenization));

            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "mc", language: "en-us", partner: Constants.PartnerNames.Cart, operation: "add"), null, null, additionalHeaders: additionalHeadersForIndiaTokenization));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "mc", language: "en-us", partner: Constants.PartnerNames.Webblends, operation: "add"), null, null, additionalHeaders: additionalHeadersForIndiaTokenization));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "mc", language: "en-us", partner: Constants.PartnerNames.Xbox, operation: "add"), null, null, additionalHeaders: additionalHeadersForIndiaTokenization));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "mc", language: "en-us", partner: Constants.PartnerNames.AmcWeb, operation: "add"), null, null, additionalHeaders: additionalHeadersForIndiaTokenization));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "mc", language: "en-us", partner: Constants.PartnerNames.ConsumerSupport, operation: "add"), null, null, additionalHeaders: additionalHeadersForIndiaTokenization));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "mc", language: "en-us", partner: Constants.PartnerNames.Mseg, operation: "add"), null, null, additionalHeaders: additionalHeadersForIndiaTokenization));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "mc", language: "en-us", partner: Constants.PartnerNames.Payin, operation: "add"), null, null, additionalHeaders: additionalHeadersForIndiaTokenization));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "mc", language: "en-us", partner: Constants.PartnerNames.WebblendsInline, operation: "add"), null, null, additionalHeaders: additionalHeadersForIndiaTokenization));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "mc", language: "en-us", partner: Constants.PartnerNames.OxoWebDirect, operation: "add"), null, null, additionalHeaders: additionalHeadersForIndiaTokenization));

            Dictionary<string, string> additionalHeadersForIndiaExpiryDelete = new Dictionary<string, string>()
            {
                {
                    "x-ms-flight", Constants.FlightNames.IndiaExpiryGroupDelete
                }
            };

            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "visa", language: "en-us", partner: Constants.PartnerNames.Azure, operation: "update"), null, null, additionalHeaders: additionalHeadersForIndiaExpiryDelete));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "visa", language: "en-us", partner: Constants.PartnerNames.Commercialstores, operation: "update"), null, null, additionalHeaders: additionalHeadersForIndiaExpiryDelete));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "visa", language: "en-us", partner: Constants.PartnerNames.Bing, operation: "update"), null, null, additionalHeaders: additionalHeadersForIndiaExpiryDelete));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "mc", language: "en-us", partner: Constants.PartnerNames.Azure, operation: "update"), null, null, additionalHeaders: additionalHeadersForIndiaExpiryDelete));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "mc", language: "en-us", partner: Constants.PartnerNames.Commercialstores, operation: "update"), null, null, additionalHeaders: additionalHeadersForIndiaExpiryDelete));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "mc", language: "en-us", partner: Constants.PartnerNames.Bing, operation: "update"), null, null, additionalHeaders: additionalHeadersForIndiaExpiryDelete));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "visa", language: "en-us", partner: Constants.PartnerNames.AmcWeb, operation: "update"), null, null, additionalHeaders: additionalHeadersForIndiaExpiryDelete));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "visa", language: "en-us", partner: Constants.PartnerNames.AmcWeb, operation: "update", scenario: Constants.ScenariosForPaymentMethodDescription.PayNow), null, null, additionalHeaders: additionalHeadersForIndiaExpiryDelete));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "visa", language: "en-us", partner: Constants.PartnerNames.AmcWeb, operation: "update", scenario: Constants.ScenariosForPaymentMethodDescription.ChangePI), null, null, additionalHeaders: additionalHeadersForIndiaExpiryDelete));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "visa", language: "en-us", partner: Constants.PartnerNames.Payin, operation: "update"), null, null, additionalHeaders: additionalHeadersForIndiaExpiryDelete));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "visa", language: "en-us", partner: Constants.PartnerNames.Webblends, operation: "update"), null, null, additionalHeaders: additionalHeadersForIndiaExpiryDelete));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "visa", language: "en-us", partner: Constants.PartnerNames.NorthstarWeb, operation: "update"), null, null, additionalHeaders: additionalHeadersForIndiaExpiryDelete));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "visa", language: "en-us", partner: Constants.PartnerNames.OxoWebDirect, operation: "update"), null, null, additionalHeaders: additionalHeadersForIndiaExpiryDelete));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "visa", language: "en-us", partner: Constants.TemplateNames.DefaultTemplate, operation: "update"), null, null, additionalHeaders: additionalHeadersForIndiaExpiryDelete));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "mc", language: "en-us", partner: Constants.PartnerNames.AmcWeb, operation: "update"), null, null, additionalHeaders: additionalHeadersForIndiaExpiryDelete));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "mc", language: "en-us", partner: Constants.PartnerNames.AmcWeb, operation: "update", scenario: Constants.ScenariosForPaymentMethodDescription.PayNow), null, null, additionalHeaders: additionalHeadersForIndiaExpiryDelete));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "mc", language: "en-us", partner: Constants.PartnerNames.AmcWeb, operation: "update", scenario: Constants.ScenariosForPaymentMethodDescription.ChangePI), null, null, additionalHeaders: additionalHeadersForIndiaExpiryDelete));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "mc", language: "en-us", partner: Constants.PartnerNames.Payin, operation: "update"), null, null, additionalHeaders: additionalHeadersForIndiaExpiryDelete));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "mc", language: "en-us", partner: Constants.PartnerNames.Webblends, operation: "update"), null, null, additionalHeaders: additionalHeadersForIndiaExpiryDelete));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "mc", language: "en-us", partner: Constants.PartnerNames.NorthstarWeb, operation: "update"), null, null, additionalHeaders: additionalHeadersForIndiaExpiryDelete));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "mc", language: "en-us", partner: Constants.PartnerNames.OxoWebDirect, operation: "update"), null, null, additionalHeaders: additionalHeadersForIndiaExpiryDelete));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "mc", language: "en-us", partner: Constants.TemplateNames.DefaultTemplate, operation: "update"), null, null, additionalHeaders: additionalHeadersForIndiaExpiryDelete));

            Dictionary<string, string> additionalHeadersForIndiaConsumerTokenizationConsentMessage = new Dictionary<string, string>()
            {
                {
                    "x-ms-flight", Constants.FlightNames.IndiaTokenizationConsentCapture
                }
            };

            foreach (string partner in this.Partners)
            {
                set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "visa", language: "en-us", partner: partner, operation: "add", completePrerequisites: bool.TrueString.ToLower()), null, null, additionalHeaders: additionalHeadersForIndiaConsumerTokenizationConsentMessage));
                set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "in", family: "credit_card", type: "mc", language: "en-us", partner: partner, operation: "add", completePrerequisites: bool.TrueString.ToLower()), null, null, additionalHeaders: additionalHeadersForIndiaConsumerTokenizationConsentMessage));
            }

            // add test for SearchTransaction action for northstarweb partner
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "us", family: "credit_card", language: "en-us", partner: Constants.PartnerNames.NorthstarWeb, operation: Constants.PidlOperationType.SearchTransactions), null));

            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "us", family: "credit_card", language: "en-us", partner: Constants.TemplateNames.DefaultTemplate, operation: Constants.PidlOperationType.SearchTransactions), null));

            // add test for ModernIdealBillingAgreement
            Dictionary<string, string> additionalHeadersForModernIdealBillingAgreement = new Dictionary<string, string>()
            {
                {
                    "x-ms-flight", Constants.FlightNames.PXEnableModernIdealPayment
                }
            };

            string[] partners = new string[]
            {
                Constants.PartnerNames.AmcWeb,
                Constants.PartnerNames.Cart,
                Constants.PartnerNames.DefaultTemplate,
                Constants.PartnerNames.NorthstarWeb,
                Constants.PartnerNames.OxoDIME,
                Constants.PartnerNames.OxoWebDirect,
                Constants.PartnerNames.Webblends,
                Constants.PartnerNames.XboxNative,
                Constants.PartnerNames.DefaultPartner
            };

            foreach (string partner in partners)
            {
                set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentMethodDescriptions", country: "nl", family: "direct_debit", type: "ideal_billing_agreement", language: "en-us", partner: partner, operation: "add"), null, null, additionalHeaders: additionalHeadersForModernIdealBillingAgreement));
            }

            return Validate(set);
        }

        private List<Test> PaymentMethodDescriptionsPaymentMethodsInCountriesSet(HashSet<string> comboKeys = null)
        {
            const string ResourceName = "paymentMethodDescriptions";
            List<Test> set = new List<Test>();
            Dictionary<string, string> xboxNativeStyleHintsFlight = XboxNativeStyleHintsHeaders;

            foreach (string country in Constants.DiffTest.PaymentMethodsInCountries.Keys)
            {
                // generate tests without type
                foreach (string family in Constants.DiffTest.PaymentMethodsInCountries[country].Keys)
                {
                    foreach (string lang in this.Languages)
                    {
                        foreach (string partner in this.Partners)
                        {
                            foreach (string operation in this.Operations)
                            {
                                if (comboKeys == null || !comboKeys.Contains(string.Format("{0},{1},{2},{3},{4},", country, family, lang, partner, operation)))
                                {
                                    set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, family: family, language: lang, partner: partner, operation: operation), null));
                                    if (Constants.IsXboxNativePartner(partner))
                                    {
                                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, family: family, language: lang, partner: partner, operation: operation), null, null, xboxNativeStyleHintsFlight));
                                    }
                                }

                                set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, family: family, language: lang, partner: partner, operation: operation, completePrerequisites: bool.TrueString.ToLower()), null));
                                if (Constants.IsXboxNativePartner(partner))
                                {
                                    set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, family: family, language: lang, partner: partner, operation: operation, completePrerequisites: bool.TrueString.ToLower()), null, null, xboxNativeStyleHintsFlight));
                                }

                                if (string.Equals(partner, Constants.PartnerNames.Webblends, StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(partner, Constants.PartnerNames.WebblendsInline)
                                    || string.Equals(partner, Constants.PartnerNames.OxoWebDirect)
                                    || string.Equals(partner, Constants.PartnerNames.OxoDIME))
                                {
                                    set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, family: family, language: lang, partner: partner, operation: operation, scenario: Constants.ScenarioNames.DisplayOptionalFields, completePrerequisites: bool.TrueString.ToLower()), null));
                                }

                                if ((string.Equals(partner, Constants.PartnerNames.OfficeOobe, StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(partner, Constants.PartnerNames.OXOOobe, StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(partner, Constants.PartnerNames.SmbOobe, StringComparison.OrdinalIgnoreCase))
                                    && string.Equals(family, Constants.PaymentMethodFamilyNames.CreditCard, StringComparison.OrdinalIgnoreCase))
                                {
                                    if (comboKeys == null || !comboKeys.Contains(string.Format("{0},{1},{2},{3},{4},{5},", country, family, lang, partner, operation, Constants.ScenariosForPaymentMethodDescription.Rs5)))
                                    {
                                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, family: family, language: lang, partner: partner, operation: operation, scenario: Constants.ScenariosForPaymentMethodDescription.Rs5), null));
                                    }

                                    set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, family: family, language: lang, partner: partner, operation: operation, completePrerequisites: bool.TrueString.ToLower(), scenario: Constants.ScenariosForPaymentMethodDescription.Rs5), null));
                                }

                                if (string.Equals(partner, Constants.PartnerNames.SmbOobe, StringComparison.OrdinalIgnoreCase)
                                    && string.Equals(family, Constants.PaymentMethodFamilyNames.CreditCard, StringComparison.OrdinalIgnoreCase))
                                {
                                    if (comboKeys == null || !comboKeys.Contains(string.Format("{0},{1},{2},{3},{4},{5},", country, family, lang, partner, operation, Constants.ScenariosForPaymentMethodDescription.Roobe)))
                                    {
                                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, family: family, language: lang, partner: partner, operation: operation, scenario: Constants.ScenariosForPaymentMethodDescription.Roobe), null));
                                    }

                                    set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, family: family, language: lang, partner: partner, operation: operation, completePrerequisites: bool.TrueString.ToLower(), scenario: Constants.ScenariosForPaymentMethodDescription.Roobe), null));
                                }

                                if (string.Equals(family, Constants.PaymentMethodFamilyNames.CreditCard, StringComparison.OrdinalIgnoreCase)
                                    && (string.Equals(partner, Constants.PartnerNames.Commercialstores, StringComparison.OrdinalIgnoreCase)
                                        || string.Equals(partner, Constants.PartnerNames.Azure, StringComparison.OrdinalIgnoreCase)
                                        || string.Equals(partner, Constants.PartnerNames.AzureSignup, StringComparison.OrdinalIgnoreCase)
                                        || string.Equals(partner, Constants.PartnerNames.AzureIbiza, StringComparison.OrdinalIgnoreCase)
                                        || string.Equals(partner, Constants.TemplateNames.TwoPage, StringComparison.OrdinalIgnoreCase)
                                        || string.Equals(partner, Constants.PartnerNames.DefaultTemplate, StringComparison.OrdinalIgnoreCase)))
                                {
                                    if (comboKeys == null || !comboKeys.Contains(string.Format("{0},{1},{2},{3},{4},{5},", country, family, lang, partner, operation, Constants.ScenariosForPaymentMethodDescription.IncludeCvv)))
                                    {
                                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, family: family, language: lang, partner: partner, operation: operation, scenario: Constants.ScenariosForPaymentMethodDescription.IncludeCvv), null));
                                    }

                                    set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, family: family, language: lang, partner: partner, operation: operation, completePrerequisites: bool.TrueString.ToLower(), scenario: Constants.ScenariosForPaymentMethodDescription.IncludeCvv), null));
                                }
                            }
                        }
                    }

                    // generate tests with type
                    foreach (string type in Constants.DiffTest.PaymentMethodsInCountries[country][family])
                    {
                        foreach (string lang in this.Languages)
                        {
                            foreach (string partner in this.Partners)
                            {
                                foreach (string operation in this.Operations)
                                {
                                    Dictionary<string, string> additionalHeaders = null;
                                    Dictionary<string, string> xboxNativeAdditionalHeadersWithStyleHints = null;
                                    if (string.Equals(type, "JCB", StringComparison.OrdinalIgnoreCase))
                                    {
                                        additionalHeaders = new Dictionary<string, string>()
                                        {
                                            {
                                                "x-ms-flight", string.Format("PaymentMethodCustomConfiguration.JCB{0}", country.ToUpperInvariant())
                                            }
                                        };

                                        xboxNativeAdditionalHeadersWithStyleHints = new Dictionary<string, string>
                                        {
                                            {
                                                "x-ms-flight", string.Format("PXEnableXboxNativeStyleHints,PaymentMethodCustomConfiguration.JCB{0}", country.ToUpperInvariant())
                                            },
                                            {
                                                "x-ms-pidlsdk-version", "2.7.0"
                                            }
                                        };
                                    }

                                    if (comboKeys == null || !comboKeys.Contains(string.Format("{0},{1},{2},{3},{4},{5},", country, family, lang, partner, operation, type)))
                                    {
                                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, family: family, type: type, language: lang, partner: partner, operation: operation), null, null, additionalHeaders: additionalHeaders));
                                        if (Constants.IsXboxNativePartner(partner))
                                        {
                                            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, family: family, type: type, language: lang, partner: partner, operation: operation), null, null, additionalHeaders: xboxNativeAdditionalHeadersWithStyleHints));
                                        }
                                    }

                                    set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, family: family, type: type, language: lang, partner: partner, operation: operation, completePrerequisites: bool.TrueString.ToLower()), null, null, additionalHeaders));
                                    if (Constants.IsXboxNativePartner(partner))
                                    {
                                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, family: family, type: type, language: lang, partner: partner, operation: operation, completePrerequisites: bool.TrueString.ToLower()), null, null, xboxNativeAdditionalHeadersWithStyleHints));
                                    }

                                    if ((string.Equals(partner, Constants.PartnerNames.OfficeOobe, StringComparison.OrdinalIgnoreCase)
                                        || string.Equals(partner, Constants.PartnerNames.OXOOobe, StringComparison.OrdinalIgnoreCase)
                                        || string.Equals(partner, Constants.PartnerNames.SmbOobe, StringComparison.OrdinalIgnoreCase))
                                        && string.Equals(family, Constants.PaymentMethodFamilyNames.CreditCard, StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (comboKeys == null || !comboKeys.Contains(string.Format("{0},{1},{2},{3},{4},{5},{6},", country, family, lang, partner, operation, type, Constants.ScenariosForPaymentMethodDescription.Rs5)))
                                        {
                                            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, family: family, type: type, language: lang, partner: partner, operation: operation, scenario: Constants.ScenariosForPaymentMethodDescription.Rs5), null, null, additionalHeaders: additionalHeaders));
                                        }

                                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, family: family, type: type, language: lang, partner: partner, operation: operation, completePrerequisites: bool.TrueString.ToLower(), scenario: Constants.ScenariosForPaymentMethodDescription.Rs5), null, null, additionalHeaders: additionalHeaders));
                                    }

                                    if (string.Equals(family, Constants.PaymentMethodFamilyNames.Ewallet, StringComparison.OrdinalIgnoreCase)
                                        && (string.Equals(partner, Constants.PartnerNames.OxoWebDirect, StringComparison.OrdinalIgnoreCase) || string.Equals(partner, Constants.PartnerNames.OxoDIME, StringComparison.OrdinalIgnoreCase))
                                        && string.Equals(type, Constants.PaymentMethodTypeNames.Paypal, StringComparison.OrdinalIgnoreCase))
                                    {
                                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, family: family, type: type, language: lang, partner: partner, operation: operation, completePrerequisites: bool.TrueString.ToLower(), scenario: Constants.ScenariosForPaymentMethodDescription.PaypalQrCode), null));
                                    }
                                }
                            }
                        }
                    }

                    // add test for replace action for northstarweb partner
                    set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, family: family, language: "en-us", partner: Constants.PartnerNames.NorthstarWeb, operation: Constants.PidlOperationType.Replace), null));

                    set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, family: family, language: "en-us", partner: Constants.TemplateNames.DefaultTemplate, operation: Constants.PidlOperationType.Replace), null));

                    // add tests for replace and update actions for northstarweb partner for withNewAddress scenario
                    set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, family: family, language: "en-us", partner: Constants.PartnerNames.NorthstarWeb, operation: Constants.PidlOperationType.Update, scenario: Constants.ScenariosForPaymentMethodDescription.WithNewAddress), null));
                    set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, family: family, language: "en-us", partner: Constants.PartnerNames.NorthstarWeb, operation: Constants.PidlOperationType.Replace, scenario: Constants.ScenariosForPaymentMethodDescription.WithNewAddress), null));

                    set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, family: family, language: "en-us", partner: Constants.TemplateNames.DefaultTemplate, operation: Constants.PidlOperationType.Update, scenario: Constants.ScenariosForPaymentMethodDescription.WithNewAddress), null));
                    set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, family: family, language: "en-us", partner: Constants.TemplateNames.DefaultTemplate, operation: Constants.PidlOperationType.Replace, scenario: Constants.ScenariosForPaymentMethodDescription.WithNewAddress), null));
                }
            }

            return Validate(set);
        }

        private List<Test> PaymentMethodDescriptionsSelectSet()
        {
            const string ResourceName = "paymentMethodDescriptions";
            List<Test> set = new List<Test>();
            foreach (string country in Constants.DiffTest.PaymentMethodsInCountries.Keys)
            {
                foreach (string lang in this.Languages)
                {
                    foreach (string partner in this.Partners)
                    {
                        foreach (var allowedMethodsSet in Constants.DiffTest.AllowedPaymentMethods)
                        {
                            string formattedAllowedMethods;
                            formattedAllowedMethods = string.Format("[\"{0}\"]", string.Join("\",\"", allowedMethodsSet));
                            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, language: lang, partner: partner, allowedPaymentMethods: formattedAllowedMethods, operation: Constants.PidlOperationType.Select), null));
                            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, language: lang, partner: Constants.TemplateNames.SelectPMRadioButtonList, allowedPaymentMethods: formattedAllowedMethods, operation: Constants.PidlOperationType.Select), null));
                            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, language: lang, partner: Constants.TemplateNames.SelectPMDropDown, allowedPaymentMethods: formattedAllowedMethods, operation: Constants.PidlOperationType.Select), null));
                        }

                        foreach (var exclutionTagSet in Constants.DiffTest.FilterExclutionTags)
                        {
                            string filterString = string.Format("\"exclusionTags\":[\"{0}\"]", string.Join("\",\"", exclutionTagSet));
                            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, language: lang, partner: partner, filters: "{" + filterString + "}", operation: Constants.PidlOperationType.Select), null));

                            foreach (string chargeThreshold in Constants.DiffTest.FilterChargeThreshold)
                            {
                                string filterStringWithThreshold = string.Format("{0},\"chargeThreshold\":{1}", filterString, chargeThreshold);
                                set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, language: lang, partner: partner, filters: "{" + filterStringWithThreshold + "}", operation: Constants.PidlOperationType.Select), null));

                                // dont forget splitPaymentSupported
                                set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, language: lang, partner: partner, filters: "{" + filterStringWithThreshold + ",\"splitPaymentSupported\":true}", operation: Constants.PidlOperationType.Select), null));
                            }
                        }
                    }
                }
            }

            foreach (string country in Constants.CommercialProfileCountriesTest)
            {
                foreach (string partner in Constants.CommercialPartners)
                {
                    foreach (string lang in this.Languages)
                    {
                        // Test standalone tax pidl for departmental purchase
                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMyOrg, ResourceName, country: country, partner: partner, language: lang, operation: Constants.PidlOperationType.Select), null));
                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMyOrg, ResourceName, country: country, partner: partner, language: lang, operation: Constants.PidlOperationType.Select, scenario: Constants.ScenarioNames.EligiblePI), null));
                    }
                }
            }

            return Validate(set);
        }

        private List<Test> PaymentMethodDescriptionsListPiClientPrefilling()
        {
            const string ResourceName = "paymentMethodDescriptions";
            List<Test> set = new List<Test>();
            List<string> listPiClientPrefillingPartners = new List<string> { "appsource", "azure", "azuresignup", "azureibiza", "commercialstores", "ggpdeds", "marketplace", "northstarweb", "onedrive", "payin", "setupoffice", "storeoffice" };
            List<string> listPiServerSidePiPopulationPartners = new List<string> { "listpidropdown", "listpiradiobutton", "listpibuttonlist" };
            List<string> listPiPartners = listPiClientPrefillingPartners.Concat(listPiServerSidePiPopulationPartners).ToList();

            foreach (string country in Constants.DiffTest.PaymentMethodsInCountries.Keys)
            {
                foreach (string lang in this.Languages)
                {
                    foreach (string partner in listPiPartners)
                    {
                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, language: lang, partner: partner, operation: "selectinstance"), null));
                    }
                }
            }

            Dictionary<string, string> additionalHeadersForIndiaTokenizationMessage = new Dictionary<string, string>()
            {
                {
                    "x-ms-flight", Constants.FlightNames.IndiaTokenizationMessage
                }
            };

            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "in", language: "en-us", partner: Constants.PartnerNames.AmcWeb, operation: "selectinstance"), null, additionalHeaders: additionalHeadersForIndiaTokenizationMessage));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "in", language: "en-us", partner: Constants.PartnerNames.Mseg, operation: "selectinstance"), null, additionalHeaders: additionalHeadersForIndiaTokenizationMessage));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "in", language: "en-us", partner: Constants.PartnerNames.Payin, operation: "selectinstance"), null, additionalHeaders: additionalHeadersForIndiaTokenizationMessage));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "in", language: "en-us", partner: Constants.PartnerNames.SetupOffice, operation: "selectinstance"), null, additionalHeaders: additionalHeadersForIndiaTokenizationMessage));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: "in", language: "en-us", partner: Constants.PartnerNames.XboxNative, operation: "selectinstance"), null, additionalHeaders: additionalHeadersForIndiaTokenizationMessage));

            return Validate(set);
        }

        private List<Test> PaymentMethodDescriptionsShowOrSearchPi()
        {
            const string ResourceName = "paymentMethodDescriptions";
            List<Test> set = new List<Test>();
            List<string> searchPiPartners = new List<string> { "commercialsupport", "consumersupport", "defaulttemplate" };
            List<string> showPiPartners = new List<string> { "appsource", "commercialsupport", "defaulttemplate" };

            foreach (string country in Constants.DiffTest.PaymentMethodsInCountries.Keys)
            {
                foreach (string lang in this.Languages)
                {
                    foreach (string partner in searchPiPartners)
                    {
                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.Anonymous, ResourceName, family: Constants.PaymentMethodFamilyNames.CreditCard, country: country, language: lang, partner: partner, operation: "search"), null));
                    }

                    foreach (string partner in showPiPartners)
                    {
                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, family: Constants.PaymentMethodFamilyNames.CreditCard, country: country, language: lang, partner: partner, operation: "show"), null));

                        if (string.Equals(partner, "commercialsupport", StringComparison.OrdinalIgnoreCase) || string.Equals(partner, "defaulttemplate", StringComparison.OrdinalIgnoreCase))
                        {
                            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, family: Constants.PaymentMethodFamilyNames.Ewallet, type: Constants.PaymentMethodTypeNames.Paypal, country: country, language: lang, partner: partner, operation: "show"), null));
                            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, family: Constants.PaymentMethodFamilyNames.DirectDebit, type: Constants.PaymentMethodTypeNames.Sepa, country: country, language: lang, partner: partner, operation: "show"), null));
                            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, family: Constants.PaymentMethodFamilyNames.DirectDebit, type: Constants.PaymentMethodTypeNames.Ach, country: country, language: lang, partner: partner, operation: "show"), null));
                        }
                    }
                }
            }

            return Validate(set);
        }

        private List<Test> PaymentMethodDescriptionListScenarioSet()
        {
            const string ResourceName = "paymentMethodDescriptions";
            List<Test> set = new List<Test>();

            foreach (string country in Constants.CommercialProfileCountriesTest)
            {
                foreach (string lang in this.Languages)
                {
                    foreach (string partner in Constants.CommercialPartners)
                    {
                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMyOrg, ResourceName, country: country, language: lang, partner: partner, operation: Constants.PidlOperationType.SelectInstance), null));
                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMyOrg, ResourceName, country: country, language: lang, partner: partner, operation: Constants.PidlOperationType.SelectInstance, scenario: Constants.ScenarioNames.MonetaryCommit), null));
                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMyOrg, ResourceName, country: country, language: lang, partner: partner, operation: Constants.PidlOperationType.SelectInstance, scenario: Constants.ScenarioNames.MonetaryCommitModernAccounts), null));
                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMyOrg, ResourceName, country: country, language: lang, partner: partner, operation: Constants.PidlOperationType.SelectInstance, scenario: Constants.ScenarioNames.DepartmentalPurchase), null));
                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMyOrg, ResourceName, country: country, language: lang, partner: partner, operation: Constants.PidlOperationType.SelectInstance, scenario: Constants.ScenarioNames.EligiblePI), null));
                    }
                }
            }

            return Validate(set);
        }

        // This test is used to cover the DiffTest involving the feature flight, which enables the feature for the partners.
        private List<Test> PaymentMethodDescriptionsFeatureFlight()
        {
             const string ResourceName = "paymentMethodDescriptions";
            List<Test> set = new List<Test>();

            foreach (string country in Constants.DiffTest.PaymentMethodsInCountries.Keys)
            {
                foreach (string family in Constants.DiffTest.PaymentMethodsInCountries[country].Keys)
                {
                    foreach (string lang in this.Languages)
                    {
                        foreach (string partner in this.Partners)
                        {
                            foreach (string operation in this.Operations)
                            {
                                if (string.Equals(family, Constants.PaymentMethodFamilyNames.CreditCard, StringComparison.OrdinalIgnoreCase)
                                    && (string.Equals(operation, Constants.PidlOperationType.Add, StringComparison.OrdinalIgnoreCase)
                                        || string.Equals(operation, Constants.PidlOperationType.Update, StringComparison.OrdinalIgnoreCase)))
                                {
                                    // For partners who have the feature flag PXEnableAddAllFieldsRequiredText enabled, we need to add the text 'All fields are required' to the display content.
                                    var allMandatoryTextFeatureFlightHeader = new Dictionary<string, string>()
                                    {
                                        {
                                            "x-ms-flight", "PXEnableAddAllFieldsRequiredText"
                                        }
                                    };

                                    // skipping the test as the url for this currently not valid
                                    if (Constants.IsXboxNativePartner(partner))
                                    {
                                        continue;
                                    }

                                    set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, ResourceName, country: country, family: family, language: lang, partner: partner, operation: operation), null, null, additionalHeaders: allMandatoryTextFeatureFlightHeader));
                                }
                            }
                        }
                    }
                }
            }

            return Validate(set);
        }

        private List<Test> PaymentMethodDescriptionVirtualPaymentsSet()
        {
            const string ResourceName = "paymentMethodDescriptions";
            List<Test> set = new List<Test>();

            foreach (string country in Constants.CommercialProfileCountriesTest)
            {
                foreach (string lang in this.Languages)
                {
                    foreach (string partner in Constants.VirtualFamilyPartners)
                    {
                        foreach (string type in Constants.VirtualPaymentMethods)
                        {
                            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMyOrg, ResourceName, country: country, language: lang, partner: partner, operation: Constants.PidlOperationType.Add, family: Constants.PaymentMethodFamilyNames.Virtual, type: type), null));

                            if (string.Equals(type, "legacy_invoice", StringComparison.OrdinalIgnoreCase))
                            {
                                set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMyOrg, ResourceName, country: country, language: lang, partner: partner, operation: Constants.PidlOperationType.Update, family: Constants.PaymentMethodFamilyNames.Virtual, type: type), null));
                            }
                        }
                    }
                }
            }

            return Validate(set);
        }

        /// <summary>
        /// Creates a set of test data for the paymentInstrumentsEx api endpoint
        /// </summary>
        /// <returns>relative url string + body content</returns>
        private List<Test> PaymentInstrumentsExSet()
        {
            List<Test> set = new List<Test>();

            foreach (string payloadPath in Directory.GetFiles(".\\DiffTest\\ConfigFiles\\scenarios\\"))
            {
                TestRequestContent content = new TestRequestContent(payloadPath);

                if (content.Name == "px.tops.csvtoken.success")
                {
                    continue;
                }

                if (content.Name == "px.issuerservice.default")
                {
                    var bodyData = JsonConvert.DeserializeObject<Dictionary<string, string>>(content.Body);
                    foreach (string country in content.Countries)
                    {
                        foreach (string partner in content.Partners)
                        {
                            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentInstrumentsEx", country: country, partner: partner, language: bodyData["language"], operation: bodyData["paymentMethodOperation"], sessionId: bodyData["sessionId"]), content));
                        }
                    }

                    continue;
                }

                if (content.Name == "px.pims.unionpay_creditcard.sms.resume")
                {
                    var bodyData = JsonConvert.DeserializeObject<Dictionary<string, string>>(content.Body);
                    foreach (string country in content.Countries)
                    {
                        foreach (string partner in content.Partners)
                        {
                            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentInstrumentsEx", country: country, partner: partner, language: bodyData["language"], completePrerequisites: bool.TrueString.ToLower()), content, null, null, piid: "BHupZgEAAAABAACA"));
                            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentInstrumentsEx", country: country, partner: partner, language: bodyData["language"], completePrerequisites: bool.TrueString.ToLower()), content, null, XboxNativeStyleHintsHeaders, piid: "BHupZgEAAAABAACA"));
                        }
                    }

                    continue;
                }

                foreach (string country in content.Countries)
                {
                    foreach (string partner in content.Partners)
                    {
                        set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentInstrumentsEx", country: country, partner: partner), content));
                    }

                    foreach (string lang in this.Languages)
                    {
                        foreach (string partner in content.Partners)
                        {
                            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentInstrumentsEx", country: country, language: lang, partner: partner), content));
                        }
                    }
                }
            }

            //// Creates a set of test data for the paymentInstrumentsEx api endpoint India 3DS challenge add credit card scenario
            TestRequestContent content3ds = new TestRequestContent(".\\DiffTest\\ConfigFiles\\scenarios\\px_pims_cc_3ds_consumer_add_pending_get_pending.json");

            foreach (string country in content3ds.Countries)
            {
                foreach (string partner in content3ds.Partners)
                {
                    set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentInstrumentsEx", country: country, partner: partner), content3ds));
                }
            }

            var venmoHeaders = new Dictionary<string, string>()
            {
                {
                    "x-ms-flight", "PxEnableVenmo,PxEnableSelectPMAddPIVenmo"
                }
            };

            var venmoXboxNativeStyleHintsHeaders = new Dictionary<string, string>
            {
                { "x-ms-flight", "PxEnableVenmo,PxEnableSelectPMAddPIVenmo,PXEnableXboxNativeStyleHints" },
                { "x-ms-pidlsdk-version", "2.7.0" }
            };

            TestRequestContent contentVenmo = new TestRequestContent(".\\DiffTest\\ConfigFiles\\scenarios\\px_pims_venmo_add_pending_get_pending.json");

            // Xbox partners
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentInstrumentsEx", country: "us", language: "en-US", partner: Constants.PartnerNames.Storify, scenario: "venmoQrCode"), contentVenmo, null, venmoHeaders));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentInstrumentsEx", country: "us", language: "en-US", partner: Constants.PartnerNames.XboxSettings, scenario: "venmoQrCode"), contentVenmo, null, venmoHeaders));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentInstrumentsEx", country: "us", language: "en-US", partner: Constants.PartnerNames.Storify, scenario: "venmoQrCode"), contentVenmo, null, venmoXboxNativeStyleHintsHeaders));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentInstrumentsEx", country: "us", language: "en-US", partner: Constants.PartnerNames.XboxSettings, scenario: "venmoQrCode"), contentVenmo, null, venmoXboxNativeStyleHintsHeaders));

            // Web partners
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentInstrumentsEx", country: "us", language: "en-US", partner: Constants.PartnerNames.AmcWeb, scenario: null), contentVenmo, null, venmoHeaders));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentInstrumentsEx", country: "us", language: "en-US", partner: Constants.PartnerNames.Cart, scenario: null), contentVenmo, null, venmoHeaders));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentInstrumentsEx", country: "us", language: "en-US", partner: Constants.PartnerNames.OxoWebDirect, scenario: null), contentVenmo, null, venmoHeaders));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentInstrumentsEx", country: "us", language: "en-US", partner: Constants.PartnerNames.Webblends, scenario: null), contentVenmo, null, venmoHeaders));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentInstrumentsEx", country: "us", language: "en-US", partner: Constants.TemplateNames.DefaultTemplate, scenario: null), contentVenmo, null, venmoHeaders));

            // Windowsstore partner
            // No need to pass the PIMS test header below, as it can be derived from contentVenmo. The TestRunner file contains logic to add headers if "partnersettings" is sent from additional headers, and it will concatenate the PIMS header with the "partnersettings" additional headers.
            var windowsstoreVenmoHeaders = new Dictionary<string, string>
            {
                { "x-ms-flight", "PxEnableVenmo,PxEnableSelectPMAddPIVenmo,PXUsePartnerSettingsService" },
                { "x-ms-test", "{ \"scenarios\": \"px.partnersettings.windowsstore\", \"contact\":\"DiffTest\"}" }
            };

            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentInstrumentsEx", country: "us", language: "en-US", partner: Constants.PartnerNames.WindowsStore), contentVenmo, null, windowsstoreVenmoHeaders));

            // confirm csv token tests
            TestRequestContent confirmCSVToken = new TestRequestContent(".\\DiffTest\\ConfigFiles\\scenarios\\px_tops_csvtoken_success.json");
            var csvTokenHeaders = new Dictionary<string, string>()
            {
                {
                    "x-ms-test",  "{ \"scenarios\": \"px.partnersettings.windowsstore,px.pims.ewallet.giftcard,px.tops.csvtoken.success,px.purchasefd.redeemcsv.success\", \"contact\":\"DiffTest\"}"
                }
            };

            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentInstrumentsEx", country: "us", language: "en-US", partner: Constants.PartnerNames.WindowsStore), confirmCSVToken, null, csvTokenHeaders));

            // confirm csv token tests for xboxNative
            var xboxCsvTokenHeaders = csvTokenHeaders;
            xboxCsvTokenHeaders.Add("x-ms-flight", Constants.FlightNames.PXEnableRedeemCSVFlow);
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentInstrumentsEx", country: "us", language: "en-US", partner: Constants.PartnerNames.XboxSettings), confirmCSVToken, null, xboxCsvTokenHeaders));

            var contentPayPal = new TestRequestContent(".\\DiffTest\\ConfigFiles\\scenarios\\px_pims_paypalwithretrypage_add_success.json");
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentInstrumentsEx", country: "us", language: "en-US", partner: Constants.TemplateNames.DefaultTemplate), contentPayPal, null));

            // No need to pass the PIMS test header below, as it can be derived from contentPayPal. The TestRunner file contains logic to add headers if "partnersettings" is sent from additional headers, and it will concatenate the PIMS header with the "partnersettings" additional headers.
            var windowsStorePayPalHeaders = new Dictionary<string, string>()
            {
                {
                    "x-ms-flight", "PXUsePartnerSettingsService"
                },
                {
                    "x-ms-test", "{ \"scenarios\": \"px.partnersettings.windowsstore\", \"contact\":\"DiffTest\"}"
                }
            };

            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentInstrumentsEx", country: "us", language: "en-US", partner: Constants.PartnerNames.WindowsStore), contentPayPal, null, windowsStorePayPalHeaders));

            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentInstrumentsEx", country: "us", language: "en-US", partner: Constants.PartnerNames.Storify, scenario: "paypalQrCode"), contentPayPal, null, null));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentInstrumentsEx", country: "us", language: "en-US", partner: Constants.PartnerNames.XboxSettings, scenario: "paypalQrCode"), contentPayPal, null, null));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentInstrumentsEx", country: "us", language: "en-US", partner: Constants.PartnerNames.Storify, scenario: "paypalQrCode"), contentPayPal, null, XboxNativeStyleHintsHeaders));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentInstrumentsEx", country: "us", language: "en-US", partner: Constants.PartnerNames.XboxSettings, scenario: "paypalQrCode"), contentPayPal, null, XboxNativeStyleHintsHeaders));

            var xboxNativePayPalVenmoShortUrlHeadersWithStyleHints = new Dictionary<string, string>
            {
                { "x-ms-flight", "PXEnableXboxNativeStyleHints,PXEnableShortUrlVenmoText,PXEnableShortUrlVenmo,PXEnableShortUrlPayPal,PXEnableShortUrlPayPalText" },
                { "x-ms-pidlsdk-version", "2.7.0" }
            };

            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentInstrumentsEx", country: "us", language: "en-US", partner: Constants.PartnerNames.Storify), contentPayPal, null, xboxNativePayPalVenmoShortUrlHeadersWithStyleHints));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentInstrumentsEx", country: "us", language: "en-US", partner: Constants.PartnerNames.XboxSettings), contentPayPal, null, xboxNativePayPalVenmoShortUrlHeadersWithStyleHints));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentInstrumentsEx", country: "us", language: "en-US", partner: Constants.PartnerNames.Storify, scenario: "venmoQrCode"), contentVenmo, null, xboxNativePayPalVenmoShortUrlHeadersWithStyleHints));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentInstrumentsEx", country: "us", language: "en-US", partner: Constants.PartnerNames.XboxSettings, scenario: "venmoQrCode"), contentVenmo, null, xboxNativePayPalVenmoShortUrlHeadersWithStyleHints));

            var contentAlipayQrCode = new TestRequestContent(".\\DiffTest\\ConfigFiles\\scenarios\\px_pims_alipay_add_qrcode_pending.json");
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentInstrumentsEx", country: "cn", language: "en-US", partner: Constants.PartnerNames.Storify), contentAlipayQrCode, null, null));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentInstrumentsEx", country: "cn", language: "en-US", partner: Constants.PartnerNames.XboxSettings), contentAlipayQrCode, null, null));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentInstrumentsEx", country: "cn", language: "en-US", partner: Constants.PartnerNames.Storify), contentAlipayQrCode, null, XboxNativeStyleHintsHeaders));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentInstrumentsEx", country: "cn", language: "en-US", partner: Constants.PartnerNames.XboxSettings), contentAlipayQrCode, null, XboxNativeStyleHintsHeaders));

            var contentAlipayQrCodeSuccess = new TestRequestContent(".\\DiffTest\\ConfigFiles\\scenarios\\px_pims_alipay_add_success.json");
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentInstrumentsEx", country: "cn", language: "en-US", partner: Constants.PartnerNames.Storify), contentAlipayQrCodeSuccess, null, null));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentInstrumentsEx", country: "cn", language: "en-US", partner: Constants.PartnerNames.XboxSettings), contentAlipayQrCodeSuccess, null, null));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentInstrumentsEx", country: "cn", language: "en-US", partner: Constants.PartnerNames.Storify), contentAlipayQrCodeSuccess, null, XboxNativeStyleHintsHeaders));
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentInstrumentsEx", country: "cn", language: "en-US", partner: Constants.PartnerNames.XboxSettings), contentAlipayQrCodeSuccess, null, XboxNativeStyleHintsHeaders));

            // Sepa Redirec hyperlink text testing
            var sepaRedirectLinkHeaders = new Dictionary<string, string>()
            {
                {
                    "x-ms-flight", "PXEnableSepaRedirectUrlText"
                }
            };

            var sepaRedirectLinkContent = new TestRequestContent(".\\DiffTest\\ConfigFiles\\scenarios\\px_pims_sepa_add_success.json");
            set.Add(new Test(new TestRequestRelativePath(Constants.UserTypes.UserMe, "paymentInstrumentsEx", country: "de", language: "en-US", partner: Constants.PartnerNames.OxoDIME), sepaRedirectLinkContent, null, sepaRedirectLinkHeaders));

            return Validate(set);
        }
    }
}
