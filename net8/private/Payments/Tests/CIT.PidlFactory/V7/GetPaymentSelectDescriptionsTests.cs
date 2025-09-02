// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace CIT.PidlFactory.V7
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Helpers;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Test.Common;
    using Newtonsoft.Json;
    using Microsoft.Commerce.Payments.PidlFactory;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;

    [TestClass]
    [TestCategory(TestCategory.SpecialCase)]
    public class GetPaymentSelectDescriptionsTests
    {
        readonly Dictionary<string, string[][]> namedPMLists = new Dictionary<string, string[][]>(StringComparer.OrdinalIgnoreCase)
        {
            {
                // These are samples of PM lists where users CANNOT be taken directly to the AddCC page (by skipping the Select PM page)
                "PMListSamples-PMPage-NotSkippable",
                new string[][]
                {
                    new string[] { "ewallet.stored_value" },
                    new string[] { "credit_card.unionpay_creditcard", "credit_card.unionpay_debitcard" },
                    new string[] { "ewallet.paypal", "credit_card.visa", "ewallet.stored_value" }
                }
            },
            {
                // These are sample PM lists where users CAN be taken directly to the AddCC page (by skipping the Select PM page)
                "PMListSamples-PMPage-IsSkippable",
                new string[][]
                {
                    new string[] { "credit_card.visa" },
                    new string[] { "credit_card.visa", "credit_card.mc" },
                    new string[] { "credit_card.visa", "ewallet.stored_value" }
                }
            }
        };

        readonly Dictionary<string, string[]> namedPartnerLists = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            {
                "Partners-SkipPMPagePolicy-Flighted",
                new string[] { "amc", "amcweb", "amcxbox", "appsource", "azure", "azuresignup", "azureibiza", "bing", "bingtravel", "cart", "commercialsupport", "commercialwebblends", "consumersupport", "consoletemplate", "default", "ggpdeds", "marketplace", "mseg", "northstarweb", "office", "officeoobe", "oxooobe", "officeoobeinapp", "onedrive", "oxodime", "oxowebdirect", "payin", "setupoffice", "setupofficesdx", "smboobe", "storeoffice", "storify", "xboxnative", "xboxsubs", "xboxsettings", "saturn", "test", "wallet", "webblends", "webblends_inline", "webpay", "xbox", "xboxweb", "windowsnative", "windowssubs", "windowssettings", "windowsstore", "selectpmdropdown", "selectpmbuttonlist", "selectpmradiobuttonlist" }
            },
            {
                "Partners-SkipPMPagePolicy-Always",
                new string[] { "commercialstores" }
            },
            {
                "Partners-PXSkipGetPMCCOnly",
                new string[] { "cart", "webblends", "oxodime", "oxowebdirect" }
            },
            {
                "Partners-NoPXSkipGetPMCCOnly", // Partners in "Partners-SkipPMPagePolicy-Flighted" without those in "Partners-PXSkipGetPMCCOnly"
                new string[] { "amc", "amcweb", "amcxbox", "appsource", "azure", "azuresignup", "azureibiza", "bing", "bingtravel", "commercialsupport", "commercialwebblends", "consumersupport", "default", "ggpdeds", "marketplace", "mseg", "northstarweb", "office", "officeoobe", "oxooobe", "officeoobeinapp", "onedrive", "payin", "setupoffice", "setupofficesdx", "smboobe", "storeoffice", "storify", "xboxsubs", "xboxsettings", "saturn", "test", "wallet", "webblends_inline", "webpay", "xbox", "xboxweb", "selectpmdropdown", "selectpmbuttonlist", "selectpmradiobuttonlist" }
            },
            {
                "Partners-SelectType-ButtonList",
                new string[] { "amc", "amcxbox", "appsource", "bing", "bingtravel", "commercialsupport", "commercialwebblends", "consumersupport", "consoletemplate", "default", "ggpdeds", "marketplace", "mseg", "northstarweb", "office", "officeoobe", "oxooobe", "officeoobeinapp", "oxodime", "oxowebdirect", "setupofficesdx", "smboobe", "storeoffice", "storify", "xboxnative", "xboxsubs", "xboxsettings", "saturn", "wallet", "webblends", "webblends_inline", "webpay", "xbox", "xboxweb", "windowssettings", "windowsnative", "windowssubs", "windowsstore", "selectpmbuttonlist" }
            },
            {
                "Partners-SelectType-DropDown",
                new string[] { "azure", "azuresignup", "azureibiza", "cart", "commercialstores", "onedrive", "payin", "setupoffice", "test", "selectpmdropdown" }
            },
            {
                "Partners-SelectType-Radio",
                new string[] { "amcweb", "selectpmradiobuttonlist" }
            },
            {
                "Partners-Skip",
                new string[] { "msteams", "onepage", "twopage", "listpidropdown", "defaulttemplate", "listpiradiobutton", "listpibuttonlist", "secondscreentemplate" }
            },
        };

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void GetPaymentSelectDescriptions_EnsureCITsCoverAllPartners()
        {
            // Arrange
            var allSkipPMPagePoliciesPartners = namedPartnerLists["Partners-SkipPMPagePolicy-Flighted"]
                .Concat(namedPartnerLists["Partners-SkipPMPagePolicy-Always"])
                .Concat(namedPartnerLists["Partners-Skip"]);

            var allSelectTypesPartners = namedPartnerLists["Partners-SelectType-ButtonList"]
                .Concat(namedPartnerLists["Partners-SelectType-DropDown"])
                .Concat(namedPartnerLists["Partners-SelectType-Radio"])
                .Concat(namedPartnerLists["Partners-Skip"]);

            // Assert
            CollectionAssert.AreEquivalent(TestConstants.AllPartners.ToList(), allSkipPMPagePoliciesPartners.ToList(), "CIT for Skip-SelectPM-page-feature is expected to cover all partners");
            CollectionAssert.AreEquivalent(TestConstants.AllPartners.ToList(), allSelectTypesPartners.ToList(), "CIT to verify correct SelectType (radio vs. dropdown vs. buttonlist) is expected to cover all partners");
        }

        [DataRow("PMListSamples-PMPage-NotSkippable", "Partners-SkipPMPagePolicy-Flighted", "us", false)]
        [DataRow("PMListSamples-PMPage-NotSkippable", "Partners-SkipPMPagePolicy-Always", "us", false)]
        [DataRow("PMListSamples-PMPage-IsSkippable", "Partners-PXSkipGetPMCCOnly", "br", true)]
        [DataRow("PMListSamples-PMPage-NotSkippable", "Partners-PXSkipGetPMCCOnly", "us", false)]
        [DataRow("PMListSamples-PMPage-IsSkippable", "Partners-NoPXSkipGetPMCCOnly", "br", false)]
        [DataTestMethod]
        public void GetPaymentSelectDescriptions_Select_SkipsPMOrNotAsExpected(string pmListName, string partnerListName, string country, bool isSkipPMExpected)
        {
            string language = "en-us";

            List<string> exposedFlightFeatures = new List<string>();

            foreach (var pmList in namedPMLists[pmListName])
            {
                foreach (var partner in namedPartnerLists[partnerListName])
                {
                    this.TestContext.WriteLine("Start testing: PMList \"{0}\", Partner \"{1}\", Country \"{2}\", isSkipPMExpected \"{3}\" ", pmList, partner, country, isSkipPMExpected);

                    // Arrange
                    var paymentMethods = new HashSet<PaymentMethod>(pmList.Select(pm =>
                    {
                        return new PaymentMethod()
                        {
                            PaymentMethodFamily = pm.Substring(0, pm.IndexOf('.')),
                            PaymentMethodType = pm.Substring(pm.IndexOf('.') + 1),
                            Properties = new PaymentMethodCapabilities()
                        };
                    }));

                    // Act
                    List<PIDLResource> pidls = PIDLResourceFactory.GetPaymentSelectDescriptions(paymentMethods, country, "select", language, partner, exposedFlightFeatures: exposedFlightFeatures);

                    // Assert
                    if (isSkipPMExpected)
                    {
                        PidlAssert.IsValid(pidls, 1, identity: false, dataDescription: false, displayDescription: false);
                        Assert.IsNotNull(pidls[0].ClientAction, "Pidl's client action is expected to be not null");
                        Assert.AreEqual(ClientActionType.ReturnContext, pidls[0].ClientAction.ActionType, "Pidl's client action type is expected to be ReturnContext");
                        Assert_IsAddPIActionContext(pidls[0].ClientAction.Context, country, language, paymentMethods, partner, false);
                    }
                    else
                    {
                        PidlAssert.IsValid(pidls, 1, descriptionType: "paymentMethod", operation: "select", country: country);
                        Assert.IsTrue(pidls[0].DataDescription.Count > 0, "Pidl is expected to contain non-empty data description");
                        Assert.IsTrue(pidls[0].DisplayPages.Count > 0, "Pidl is expected to contains non-empty display pages");
                    }

                    this.TestContext.WriteLine("...done");
                }
            }
        }

        [DataRow("Partners-SelectType-ButtonList", TestConstants.PaymentMethodSelectType.ButtonList)]
        [DataRow("Partners-SelectType-DropDown", TestConstants.PaymentMethodSelectType.DropDown)]
        [DataRow("Partners-SelectType-Radio", TestConstants.PaymentMethodSelectType.Radio)]
        [DataTestMethod]
        public void GetPaymentSelectDescriptions_Select_SelectTypeIsAsExpected(string partnersListName, string expectedSelectType)
        {
            foreach (var partner in namedPartnerLists[partnersListName])
            {
                this.TestContext.WriteLine("Start testing: Partner \"{0}\"", partner);

                // Arrange
                var paymentMethods = new HashSet<PaymentMethod>();
                paymentMethods.Add(new PaymentMethod()
                {
                    // Its important that this is not a CC so that we are gauranteed to  get the SelectPM pidl.  If CC is used, its possible
                    // (depending on partner and flight status) that we may get an AddCC action pidl instead and then the below asserts would be invalid.
                    PaymentMethodFamily = "ewallet",
                    PaymentMethodType = "paypal",
                    Properties = new PaymentMethodCapabilities()
                });

                // Act
                var pidls = PIDLResourceFactory.GetPaymentSelectDescriptions(paymentMethods, "us", "select", "en-us", partner);

                // Assert
                PropertyDisplayHint pmSelectionHint = pidls[0].DisplayHints().Where(dh => dh.HintId == TestConstants.DisplayHintIds.PaymentMethodSelect).Single() as PropertyDisplayHint;
                Assert.IsNotNull(pmSelectionHint, "SelectPM pidls are expected to have a display hint with id \"{0}\"", TestConstants.DisplayHintIds.PaymentMethodSelect);
                Assert.IsNotNull(pmSelectionHint.SelectType, "Display hint with id \"{0}\" is expected to have a non-null \"{1}\"", TestConstants.DisplayHintIds.PaymentMethodSelect, pmSelectionHint.SelectType);

                Assert.AreEqual(expectedSelectType, pmSelectionHint.SelectType, "A {0} is expected for partner \"{1}\"", expectedSelectType, partner);

                this.TestContext.WriteLine("...done");
            }
        }

        [DataRow(false)]
        [DataRow(true)]
        [DataTestMethod]
        public void GetPaymentSelectDescriptions_Select_PidlActionIsAsExpected(bool isDropdownButtonListMergeFlightOn)
        {
            List<string> exposedFlightFeatures = new List<string>();

            if (isDropdownButtonListMergeFlightOn)
            {
                exposedFlightFeatures.Add("PXSelectPMDropdownButtonListMerge");
            }

            foreach (var partner in TestConstants.AllPartners)
            {
                this.TestContext.WriteLine("Start testing: Partner \"{0}\"", partner);

                // Arrange
                var paymentMethods = new HashSet<PaymentMethod>();
                paymentMethods.Add(new PaymentMethod()
                {
                    PaymentMethodFamily = "ewallet",
                    PaymentMethodType = "paypal",
                    Properties = new PaymentMethodCapabilities()
                });

                // Act
                List<PIDLResource> pidls = PIDLResourceFactory.GetPaymentSelectDescriptions(paymentMethods, "us", "select", "en-us", partner, exposedFlightFeatures: exposedFlightFeatures);

                // Assert
                PropertyDisplayHint pmSelectionHint = pidls[0].DisplayHints().Where(dh => dh.HintId == TestConstants.DisplayHintIds.PaymentMethodSelect).Single() as PropertyDisplayHint;
                var pmOptionHint = pmSelectionHint.PossibleOptions.Where(po => string.Equals(po.Key, "ewallet_paypal", StringComparison.OrdinalIgnoreCase)).Single();
                var pmKey = pmOptionHint.Key;
                var pmHint = pmOptionHint.Value as SelectOptionDescription;

                if (pmSelectionHint.SelectType == TestConstants.PaymentMethodSelectType.ButtonList
                    || isDropdownButtonListMergeFlightOn
                    || partner == "amcweb" || partner == "selectpmradiobuttonlist")
                {
                    Assert.IsNotNull(pmHint.PidlAction, "PidlAction is expected to be not null");
                    Assert.AreEqual("success", pmHint.PidlAction.ActionType, "ActionType is expected to be \"success\"");
                    Assert_IsAddPIActionContext(pmHint.PidlAction.Context, "us", "en-us", paymentMethods, partner, true);

                    if (isDropdownButtonListMergeFlightOn)
                    {
                        Assert.AreEqual("returnObject", (pidls[0].DataDescription["id"] as PropertyDescription)?.PropertyDescriptionType, "PropertyDescriptionType is not as expected");
                    }
                }
                else
                {
                    // For dropDown / radio, the display element itself is expected to not have a pidlAction.
                    Assert.IsNull(pmHint.PidlAction, "PidlAction is expected to be null");

                    // Instead, the data description is expected to be keyed on the display.
                    var family = pidls[0].DataDescription["paymentMethodFamily"] as PropertyDescription;
                    Assert.AreEqual("ewallet", family?.PossibleValues?[pmKey].ToLower(), "Family in dataDescription is expected to match selection displayHint");
                    var type = pidls[0].DataDescription["paymentMethodType"] as PropertyDescription;
                    Assert.AreEqual("paypal", type?.PossibleValues?[pmKey].ToLower(), "Type in dataDescription is expected to match selection displayHint");
                }

                this.TestContext.WriteLine("...done");
            }
        }

        [DataRow("amcweb", "in", "en-us", true)]
        [DataRow("mseg", "in", "en-us", true)]
        [DataRow("xboxnative", "in", "en-us", true)]
        [DataRow("amcweb", "us", "en-us", true)]
        [DataRow("mseg", "us", "en-us", true)]
        [DataRow("xboxnative", "us", "en-us", true)]
        [DataRow("amcweb", "in", "en-us", false)]
        [DataRow("mseg", "in", "en-us", false)]
        [DataRow("xboxnative", "in", "en-us", false)]

        [DataTestMethod]
        public void ListPaymentInstrument_IndiaTokenizationPurgeMessage(string partner, string country, string language, bool flightPassed)
        {
            List<string> exposedFlightFeatures = new List<string>();
            if (flightPassed && string.Equals(country, "in"))
            {
                exposedFlightFeatures.Add("IndiaTokenizationMessage");
            }

            var piId = "MockPIId";
            var visaPM = new PaymentMethod()
            {
                PaymentMethodFamily = "credit_card",
                PaymentMethodType = "visa",
                Properties = new PaymentMethodCapabilities()
            };

            var pms = new HashSet<PaymentMethod>();
            pms.Add(visaPM);

            var pis = new List<PaymentInstrument>()
            {
                new PaymentInstrument()
                {
                    PaymentMethod = visaPM,
                    PaymentInstrumentId = piId,
                    Status = PaymentInstrumentStatus.Active,
                    PaymentInstrumentDetails = new PaymentInstrumentDetails()
                    {
                        LastFourDigits = "1234",
                        ExpiryYear = "30",
                        ExpiryMonth = "10"
                    }
                }
            };

            List<PIDLResource> pidls = PIDLResourceFactory.GetPaymentSelectDescriptions(pms, country, "SelectInstance", language, partner, exposedFlightFeatures: exposedFlightFeatures, paymentInstruments: pis);
            Assert.IsNotNull(pidls);

            var root = pidls[0]?.DisplayPages[0];
            var displayType = string.Empty;

            if (string.Equals(country, "in") && flightPassed)
            {
                if (!string.Equals(partner, "xboxnative", StringComparison.OrdinalIgnoreCase))
                {
                    var page = root?.Members[0] as TextGroupDisplayHint;
                    var group = page?.Members[0] as TextDisplayHint;
                    displayType = group.HintId;
                    Assert.AreEqual("paymentInstrumentSelectMessage", displayType, "Flighted Tokenization Message not displayed");
                }
                else
                {
                    var page = root?.Members[0] as GroupDisplayHint;
                    var group = page?.Members[1] as TextGroupDisplayHint;
                    displayType = group.HintId;
                    Assert.AreEqual("paymentInstrumentTokenizationTextGroup", displayType, "Flighted Tokenization Message not displayed");
                }
            }
            else
            {
                if (!string.Equals(partner, "xboxnative", StringComparison.OrdinalIgnoreCase))
                {
                    var page = root?.Members[0] as PropertyDisplayHint;
                    displayType = page.HintId;
                    Assert.AreEqual("paymentInstrument", displayType, "Flighted Tokenization Message displayed-not expected");
                }
                else
                {
                    var page = root?.Members[0] as GroupDisplayHint;
                    var group = page?.Members[1] as PropertyDisplayHint;
                    displayType = group.HintId;
                    Assert.AreEqual("paymentInstrument", displayType, "Flighted Tokenization Message displayed-not expected");
                }
            }

            this.TestContext.WriteLine("...done");
        }

        [DataRow("playxbox", "us", "en-us")]
        [DataTestMethod]
        public void ListPaymentInstrument_RemoveZeroBalanceCsv(string partner, string country, string language)
        {
            string zeroBalanceCsvId = "Mock-PiId-2-ZeroBalanceCsv";
            var visaPM = new PaymentMethod()
            {
                PaymentMethodFamily = "credit_card",
                PaymentMethodType = "visa",
                Properties = new PaymentMethodCapabilities()
            };

            var csvPm = new PaymentMethod()
            {
                PaymentMethodFamily = "ewallet",
                PaymentMethodType = "stored_value",
                Properties = new PaymentMethodCapabilities()
            };

            PaymentExperienceSetting setting = new PaymentExperienceSetting()
            {
                Template = "listpibuttonlist",
                Features = new Dictionary<string, FeatureConfig>
                {
                    {
                        "removeZeroBalanceCsv", new FeatureConfig
                        {
                            ApplicableMarkets = new List<string>()
                        }
                    }
                }
            };

            var pms = new HashSet<PaymentMethod>();
            pms.Add(visaPM);
            pms.Add(csvPm);

            var pis = new List<PaymentInstrument>()
            {
                new PaymentInstrument()
                {
                    PaymentMethod = visaPM,
                    PaymentInstrumentId = "Mock-PiId-1",
                    Status = PaymentInstrumentStatus.Active,
                    PaymentInstrumentDetails = new PaymentInstrumentDetails()
                    {
                        LastFourDigits = "1234",
                        ExpiryYear = "30",
                        ExpiryMonth = "10"
                    }
                },
                new PaymentInstrument()
                {
                    PaymentMethod = csvPm,
                    PaymentInstrumentId = zeroBalanceCsvId,
                    Status = PaymentInstrumentStatus.Active,
                    PaymentInstrumentDetails = new PaymentInstrumentDetails()
                    {
                        Currency = "USD",
                        Balance = 0
                    }
                },
                new PaymentInstrument()
                {
                    PaymentMethod = csvPm,
                    PaymentInstrumentId = "Mock-PiId-3",
                    Status = PaymentInstrumentStatus.Active,
                    PaymentInstrumentDetails = new PaymentInstrumentDetails()
                    {
                        Currency = "USD",
                        Balance = 10
                    }
                }
            };

            Assert.AreEqual(3, pis.Count, "There should be 3 payment instruments in the list");
            List<PaymentInstrument> paymentInstruments = PaymentSelectionHelper.GetFilteredPaymentInstruments(pis, null, pms, null, null, partner, country, setting);
            Assert.AreEqual(2, paymentInstruments.Count, "There should be 2 payment instruments in the list after filtering out zero balance CSV");
            paymentInstruments.ForEach(pi =>
            {
               Assert.IsFalse(string.Equals(pi.PaymentInstrumentId, zeroBalanceCsvId, StringComparison.OrdinalIgnoreCase), "Zero balance CSV should be filtered out");
            });

            this.TestContext.WriteLine("...done");
        }

        [DataRow("us", "ewallet.paypal", "MockPayPal", "MockPayPal")]
        [DataRow("br", "ewallet.paypal", "MockPayPal", "MockPayPal")]
        [DataRow("us", "credit_card.visa", "MockVisa", "Credit card or debit card")]
        [DataRow("br", "credit_card.visa", "MockVisa", "Credit card")]
        [DataRow("us", "ewallet.stored_value", "MockCsv", "Redeem a gift card")]
        [DataRow("us", "mobile_billing_non_sim.spt-us-nonsim", "MockSprint", "Mobile phone")]
        [DataRow("us", "credit_card.unionpay_creditcard", "MockCUPCC", "MockCUPCC")]
        [DataRow("se", "credit_card.visa", "MockVisa", "Debit card or credit card")]
        [DataTestMethod]
        public void GetPaymentSelectDescriptions_Select_DisplayTextIsAsExpected(string country, string pmUnderTest, string pmDisplayName, string expectedPMDisplayName)
        {
            foreach (var partner in TestConstants.AllPartners)
            {
                this.TestContext.WriteLine("Start testing: Country \"{0}\", PM \"{1}\", Partner \"{2}\"", country, pmUnderTest, partner);

                // Arrange
                var paymentMethods = new HashSet<PaymentMethod>();
                paymentMethods.Add(new PaymentMethod()
                {
                    PaymentMethodFamily = pmUnderTest.Substring(0, pmUnderTest.IndexOf('.')),
                    PaymentMethodType = pmUnderTest.Substring(pmUnderTest.IndexOf('.') + 1),
                    Display = new PaymentInstrumentDisplayDetails()
                    {
                        Name = pmDisplayName
                    },
                    Properties = new PaymentMethodCapabilities()
                });

                // Add a non-CC to ensure we get a SelectPM pidl back (instead of getting skipped over to an AddCC action Pidl)
                paymentMethods.Add(new PaymentMethod()
                {
                    PaymentMethodFamily = "direct_debit",
                    PaymentMethodType = "ach",
                    Properties = new PaymentMethodCapabilities()
                });

                // Act
                var pidls = PIDLResourceFactory.GetPaymentSelectDescriptions(paymentMethods, country, "select", "en-us", partner);

                // Assert
                PropertyDisplayHint pmSelectionHint = pidls[0].DisplayHints().Where(dh => dh.HintId == TestConstants.DisplayHintIds.PaymentMethodSelect).Single() as PropertyDisplayHint;
                var pmHint = pmSelectionHint.PossibleOptions.Where(po => string.Equals(po.Key, pmUnderTest.Replace('.', '_'), StringComparison.OrdinalIgnoreCase)).Single().Value;
                Assert.AreEqual(expectedPMDisplayName, pmHint.DisplayText, "Display text is expected to be {0}", expectedPMDisplayName);

                this.TestContext.WriteLine("...done");
            }
        }

        [TestMethod]
        public void GetPaymentSelectDescriptions_SelectInstance_PidlActionIsAsExpected()
        {
            foreach (var partner in TestConstants.AllPartners)
            {
                this.TestContext.WriteLine("Start testing: Partner \"{0}\"", partner);

                // Arrange
                var piId = "MockPIId";
                var visaPM = new PaymentMethod()
                {
                    PaymentMethodFamily = "credit_card",
                    PaymentMethodType = "visa",
                    Properties = new PaymentMethodCapabilities()
                };

                var pms = new HashSet<PaymentMethod>();
                pms.Add(visaPM);

                var pis = new List<PaymentInstrument>()
                {
                    new PaymentInstrument()
                    {
                        PaymentMethod = visaPM,
                        PaymentInstrumentId = piId,
                        Status = PaymentInstrumentStatus.Active,
                        PaymentInstrumentDetails = new PaymentInstrumentDetails()
                        {
                            LastFourDigits = "1234",
                            ExpiryYear = "30",
                            ExpiryMonth = "10"
                        }
                    }
                };

                // Act
                var pidls = PIDLResourceFactory.GetPaymentSelectDescriptions(pms, "us", "selectInstance", "en-us", "test", paymentInstruments: pis, allowedPaymentMethods: "[\"credit_card.visa\"]");

                // Assert
                PidlAssert.IsValid(pidls);
                var piSelectionHint = pidls[0].DisplayHints().Where(dh => dh.HintId == "paymentInstrument" && dh.PropertyName == "id").Single() as PropertyDisplayHint;
                Assert.IsNotNull(piSelectionHint, "Pidl is expected to have a PropertyDisplayHint pointing to a property named \"id\"");
                CollectionAssert.Contains(piSelectionHint.PossibleOptions.Keys, piId, "List PI Pidl's PossibleOptions is expected to have PIId as the key");

                var piHint = piSelectionHint.PossibleOptions[piId] as SelectOptionDescription;
                Assert.IsNotNull(piHint, "PossibleOptions is expected to a non-null SelectOptionDescription as the value");
                Assert.IsNotNull(piHint.PidlAction, "SelectOptionDescription.PidlAction is expected to be not null");

                Assert.AreEqual("success", piHint.PidlAction.ActionType, "PidlAction is expected to be \"success\"");
                Assert.IsNotNull(piHint.PidlAction.Context, "PidlAction.Context is expected to be not null");
                Assert.AreEqual(piId, (piHint.PidlAction.Context as ActionContext)?.Id, "PidlAction.Context.Instance is expected to match input");
                Assert.AreEqual(pis.First(), (piHint.PidlAction.Context as ActionContext)?.Instance, "PidlAction.Context.Instance is expected to be the full PI");

                this.TestContext.WriteLine("...done");
            }
        }

        [TestMethod]
        public void GetPaymentSelectDescriptions_SelectOptionAccessibilityName()
        {
            // Arrange
            List<string> partners = new List<string>() { "xboxsettings", "storify" };
            foreach (string partner in partners)
            {
                var paymentMethods = new HashSet<PaymentMethod>()
                {
                    new PaymentMethod()
                    {
                        PaymentMethodFamily = "credit_card",
                        PaymentMethodType = "visa",
                        Display = new PaymentInstrumentDisplayDetails() { Name = "Visa" },
                        Properties = new PaymentMethodCapabilities()
                    },
                    new PaymentMethod()
                    {
                        PaymentMethodFamily = "credit_card",
                        PaymentMethodType = "mc",
                        Display = new PaymentInstrumentDisplayDetails() { Name = "MasterCard" },
                        Properties = new PaymentMethodCapabilities()
                    },
                    new PaymentMethod()
                    {
                        PaymentMethodFamily = "credit_card",
                        PaymentMethodType = "discover",
                        Display = new PaymentInstrumentDisplayDetails() { Name = "Discover Network" },
                        Properties = new PaymentMethodCapabilities()
                    },
                    new PaymentMethod()
                    {
                        PaymentMethodFamily = "ewallet",
                        PaymentMethodType = "paypal",
                        Display = new PaymentInstrumentDisplayDetails() { Name = "paypal" },
                        Properties = new PaymentMethodCapabilities()
                    },
                    new PaymentMethod()
                    {
                        PaymentMethodFamily = "ewallet",
                        PaymentMethodType = "venmo",
                        Display = new PaymentInstrumentDisplayDetails() { Name = "venmo" },
                        Properties = new PaymentMethodCapabilities()
                    },
                    new PaymentMethod()
                    {
                        PaymentMethodFamily = "ewallet",
                        PaymentMethodType = "stored_value",
                        Display = new PaymentInstrumentDisplayDetails() { Name = "stored_value" },
                        Properties = new PaymentMethodCapabilities()
                    }
                };

                // Act
                List<PIDLResource> pidls = PIDLResourceFactory.GetPaymentSelectDescriptions(paymentMethods, "us", "select", "en-us", partner);
                PropertyDisplayHint propertyDisplayHint = pidls[0].GetDisplayHintById(TestConstants.DisplayHintIds.PaymentMethodSelect) as PropertyDisplayHint;

                // Assert
                PidlAssert.IsValid(pidls);

                Assert.AreEqual("Credit card or debit card", propertyDisplayHint.PossibleOptions.ElementAt(0).Value.DisplayText, "Accessbility name of the SelectOptionDescription for {0} is expected to be Credit card or debit card", partner);
                Assert.AreEqual("PayPal", propertyDisplayHint.PossibleOptions.ElementAt(1).Value.DisplayText, "Accessbility name of the SelectOptionDescription for {0} is expected to be PayPal", partner);
                Assert.AreEqual("venmo", propertyDisplayHint.PossibleOptions.ElementAt(2).Value.DisplayText, "Accessbility name of the SelectOptionDescription for {0} is expected to be venmo", partner);
                Assert.AreEqual("Redeem a gift card", propertyDisplayHint.PossibleOptions.ElementAt(3).Value.DisplayText, "Accessbility name of the SelectOptionDescription for {0} is expected to be Redeem a gift card", partner);
            }
        }

        // Test various allowedPaymentMethods inputs
        [DataRow("[\"f1\",\"f2\"]", "", "", "f1.t1,f1.t2,f2.t3", "test")]
        [DataRow("[\"f1\"]", "", "", "f1.t1,f1.t2", "test")]
        [DataRow("[]", "", "", "", "test")]
        [DataRow("[\"f1.t1\",\"f2.t3\"]", "", "", "f1.t1,f2.t3", "test")]
        [DataRow("[\"f1.t1\",\"f3.t4\"]", "", "", "f1.t1", "test")]

        // Test filter operations that apply to all operations and hence the same results are expected
        // irrespective of whether operation is "select" or not
        [DataRow("[\"f1\"]", @"{""filterPurchaseRedirectPayment"":false}", "", "f1.t1,f1.t2", "nonXboxNative")]
        [DataRow("[\"f1\"]", @"{""filterPurchaseRedirectPayment"":true}", "", "f1.t1", "nonXboxNative")]
        [DataRow("[\"f1\"]", @"{""splitPaymentSupported"":false}", "", "f1.t1,f1.t2", "nonXboxNative")]
        [DataRow("[\"f1\"]", @"{""splitPaymentSupported"":false}", "", "f1.t1,f1.t2", "xboxnative")]
        [DataRow("[\"f1\"]", @"{""splitPaymentSupported"":true}", "", "f1.t1", "nonXboxNative")]
        [DataRow("[\"f1\"]", @"{""splitPaymentSupported"":true}", "", "f1.t1,f1.t2", "xboxnative")]

        [DataRow("[\"f1\"]", @"{""filterPurchaseRedirectPayment"":false}", "select", "f1.t1,f1.t2", "nonXboxNative")]
        [DataRow("[\"f1\"]", @"{""filterPurchaseRedirectPayment"":true}", "select", "f1.t1", "nonXboxNative")]
        [DataRow("[\"f1\"]", @"{""splitPaymentSupported"":false}", "select", "f1.t1,f1.t2", "nonXboxNative")]
        [DataRow("[\"f1\"]", @"{""splitPaymentSupported"":false}", "select", "f1.t1,f1.t2", "xboxnative")]
        [DataRow("[\"f1\"]", @"{""splitPaymentSupported"":false}", "selectinstance", "f1.t1,f1.t2", "nonXboxNative")]
        [DataRow("[\"f1\"]", @"{""splitPaymentSupported"":false}", "selectinstance", "f1.t1,f1.t2", "xboxnative")]
        [DataRow("[\"f1\"]", @"{""splitPaymentSupported"":true}", "select", "f1.t1", "nonXboxNative")]
        [DataRow("[\"f1\"]", @"{""splitPaymentSupported"":true}", "select", "f1.t1", "xboxnative")]
        [DataRow("[\"f1\"]", @"{""splitPaymentSupported"":true}", "selectinstance", "f1.t1", "nonXboxNative")]
        [DataRow("[\"f1\"]", @"{""splitPaymentSupported"":true}", "selectinstance", "f1.t1,f1.t2", "xboxnative")]

        // Test filter operations that apply only to "select" operation and hence different results are expected
        // based on whether operation is "select" or not
        [DataRow("[\"f1\"]", @"{""exclusionTags"":[]}", "", "f1.t1,f1.t2", "nonXboxNative")]
        [DataRow("[\"f1\"]", @"{""exclusionTags"":[""x3""]}", "", "f1.t1,f1.t2", "nonXboxNative")]
        [DataRow("[\"f1\"]", @"{""exclusionTags"":[""x1""]}", "", "f1.t1,f1.t2", "nonXboxNative")]
        [DataRow("[\"f1\"]", @"{""exclusionTags"":[""x2""]}", "", "f1.t1,f1.t2", "nonXboxNative")]
        [DataRow("[\"f1\"]", @"{""exclusionTags"":[""x1"",""x2""]}", "", "f1.t1,f1.t2", "nonXboxNative")]
        [DataRow("[\"f1\"]", @"{""exclusionTags"":[""x1"",""x2"",""x3""]}", "", "f1.t1,f1.t2", "nonXboxNative")]
        [DataRow("[\"f1\"]", @"{""chargeThreshold"":4.0}", "", "f1.t1,f1.t2", "nonXboxNative")]
        [DataRow("[\"f1\"]", @"{""chargeThreshold"":5.0}", "", "f1.t1,f1.t2", "nonXboxNative")]
        [DataRow("[\"f1\"]", @"{""chargeThreshold"":6.0}", "", "f1.t1,f1.t2", "nonXboxNative")]
        [DataRow("[\"f1\"]", @"{""chargeThresholds"":[4.0]}", "", "f1.t1,f1.t2", "nonXboxNative")]
        [DataRow("[\"f1\"]", @"{""chargeThresholds"":[4.0,0.5]}", "", "f1.t1,f1.t2", "nonXboxNative")]
        [DataRow("[\"f1\"]", @"{""chargeThresholds"":[5.0]}", "", "f1.t1,f1.t2", "nonXboxNative")]
        [DataRow("[\"f1\"]", @"{""chargeThresholds"":[5.0,0.5]}", "", "f1.t1,f1.t2", "nonXboxNative")]
        [DataRow("[\"f1\"]", @"{""chargeThresholds"":[6.0]}", "", "f1.t1,f1.t2", "nonXboxNative")]

        [DataRow("[\"f1\"]", @"{""exclusionTags"":[]}", "select", "f1.t1,f1.t2", "nonXboxNative")]
        [DataRow("[\"f1\"]", @"{""exclusionTags"":[""x3""]}", "select", "f1.t1,f1.t2", "nonXboxNative")]
        [DataRow("[\"f1\"]", @"{""exclusionTags"":[""x1""]}", "select", "f1.t1", "nonXboxNative")]
        [DataRow("[\"f1\"]", @"{""exclusionTags"":[""x2""]}", "select", "f1.t1", "nonXboxNative")]
        [DataRow("[\"f1\"]", @"{""exclusionTags"":[""x1"",""x2""]}", "select", "f1.t1", "nonXboxNative")]
        [DataRow("[\"f1\"]", @"{""exclusionTags"":[""x1"",""x2"",""x3""]}", "select", "f1.t1", "nonXboxNative")]
        [DataRow("[\"f1\"]", @"{""chargeThreshold"":4.0}", "select", "f1.t1,f1.t2", "nonXboxNative")]
        [DataRow("[\"f1\"]", @"{""chargeThreshold"":5.0}", "select", "f1.t1,f1.t2", "nonXboxNative")]
        [DataRow("[\"f1\"]", @"{""chargeThreshold"":6.0}", "select", "f1.t1", "nonXboxNative")]
        [DataRow("[\"f1\"]", @"{""chargeThresholds"":[4.0]}", "select", "f1.t1,f1.t2", "nonXboxNative")]
        [DataRow("[\"f1\"]", @"{""chargeThresholds"":[4.0,0.5]}", "select", "f1.t1,f1.t2", "nonXboxNative")]
        [DataRow("[\"f1\"]", @"{""chargeThresholds"":[5.0]}", "select", "f1.t1,f1.t2", "nonXboxNative")]
        [DataRow("[\"f1\"]", @"{""chargeThresholds"":[5.0,0.5]}", "select", "f1.t1", "nonXboxNative")]
        [DataRow("[\"f1\"]", @"{""chargeThresholds"":[6.0]}", "select", "f1.t1", "nonXboxNative")]

        [TestMethod]
        public void GetFilteredPaymentMethods_FiltersAsExpected(string allowedPms, string filter, string operation, string expected, string partner)
        {
            string country = "US";
            if (partner == "xboxnative")
            {
                string[] xboxNativePartners = { "storify", "xboxsubs", "xboxsettings", "saturn" };
                for (int i = 0; i < xboxNativePartners.Length; i++)
                {
                    string pms = @"[
                        {""paymentMethodFamily"":""f1"",""paymentMethodType"":""t1"",""properties"":{""splitPaymentSupported"":true}},
                        {""paymentMethodFamily"":""f1"",""paymentMethodType"":""t2"",""properties"":{""chargeThresholds"":[{""country"":""US"",""maxPrice"":5.0}],""redirectRequired"":[""Purchase""]},""exclusionTags"":[""x1"",""x2""]},
                        {""paymentMethodFamily"":""f2"",""paymentMethodType"":""t3""}]";
                    var paymentMethods = JsonConvert.DeserializeObject<HashSet<PaymentMethod>>(pms);

                    var filteredPms = PaymentSelectionHelper.GetFilteredPaymentMethods(paymentMethods, allowedPms, filter, operation, xboxNativePartners[i], country);
                    var filteredPmsNames = filteredPms.Select(pm => pm.PaymentMethodFamily + "." + pm.PaymentMethodType);

                    CollectionAssert.AreEquivalent(expected.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries), filteredPmsNames.ToList());
                }
            }
            else
            {
                string pms = @"[
                    {""paymentMethodFamily"":""f1"",""paymentMethodType"":""t1"",""properties"":{""splitPaymentSupported"":true}},
                    {""paymentMethodFamily"":""f1"",""paymentMethodType"":""t2"",""properties"":{""chargeThresholds"":[{""country"":""US"",""maxPrice"":5.0}],""redirectRequired"":[""Purchase""]},""exclusionTags"":[""x1"",""x2""]},
                    {""paymentMethodFamily"":""f2"",""paymentMethodType"":""t3""}]";
                var paymentMethods = JsonConvert.DeserializeObject<HashSet<PaymentMethod>>(pms);

                var filteredPms = PaymentSelectionHelper.GetFilteredPaymentMethods(paymentMethods, allowedPms, filter, operation, partner, country);
                var filteredPmsNames = filteredPms.Select(pm => pm.PaymentMethodFamily + "." + pm.PaymentMethodType);

                CollectionAssert.AreEquivalent(expected.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries), filteredPmsNames.ToList());
            }
        }

        [DataRow("storify")]
        [DataRow("xboxsubs")]
        [DataRow("xboxsettings")]
        [DataRow("saturn")]
        [DataRow("webblends")]
        [DataRow("xbox")]
        [DataRow("amcxbox")]
        [TestMethod]
        public void GetPaymentSelectDescriptions_SelectInstance_ZeroBalanceCSVFiltered(string partner)
        {
            this.TestContext.WriteLine("Start testing: Partner \"{0}\"", partner);

            // Arrange
            var zeroPIId = "zeroPIId";
            var nonZeroPIId = "nonZeroPIId";
            var zeroBalanceCSV = new PaymentMethod()
            {
                PaymentMethodFamily = "ewallet",
                PaymentMethodType = "stored_value",
                Properties = new PaymentMethodCapabilities()
            };

            var nonZeroBalanceCSV = new PaymentMethod()
            {
                PaymentMethodFamily = "ewallet",
                PaymentMethodType = "stored_value",
                Properties = new PaymentMethodCapabilities()
            };

            var pms = new HashSet<PaymentMethod>();
            pms.Add(zeroBalanceCSV);
            pms.Add(nonZeroBalanceCSV);

            var pis = new List<PaymentInstrument>()
            {
                new PaymentInstrument()
                {
                    PaymentMethod = zeroBalanceCSV,
                    PaymentInstrumentId = zeroPIId,
                    Status = PaymentInstrumentStatus.Active,
                    PaymentInstrumentDetails = new PaymentInstrumentDetails()
                    {
                        Balance = decimal.Zero
                    }
                },
                new PaymentInstrument()
                {
                    PaymentMethod = nonZeroBalanceCSV,
                    PaymentInstrumentId = nonZeroPIId,
                    Status = PaymentInstrumentStatus.Active,
                    PaymentInstrumentDetails = new PaymentInstrumentDetails()
                    {
                        Balance = decimal.One
                    }
                }
            };

            // Act
            List<PaymentInstrument> disabledPaymentInstruments = new List<PaymentInstrument>();

            // structure of xboxsettings is different than all other partners
            if (partner == "xboxsettings")
            {
                var xboxSettingspidls = PIDLResourceFactory.GetPaymentSelectDescriptions(pms, "us", "selectInstance", "en-us", partner, paymentInstruments: pis, disabledPaymentInstruments: disabledPaymentInstruments, allowedPaymentMethods: "[\"ewallet.stored_value\"]", scenario: "manage");
                Assert.IsNotNull(xboxSettingspidls[0].DataSources, "XboxSetting's datasources contain the Pi's. Should not be null.");
                PaymentInstrument x = xboxSettingspidls[0].DataSources["paymentInstruments"].Members[0] as PaymentInstrument;
                Assert.AreEqual(nonZeroPIId, x.PaymentInstrumentId, "Payment instruments should have the non 0 balance csv");
                return;
            }

            var pidls = PIDLResourceFactory.GetPaymentSelectDescriptions(pms, "us", "selectInstance", "en-us", partner, paymentInstruments: pis, disabledPaymentInstruments: disabledPaymentInstruments, allowedPaymentMethods: "[\"ewallet.stored_value\"]");

            // Assert
            PidlAssert.IsValid(pidls);
            var piSelectionHint = pidls[0].DisplayHints().Where(dh => dh.HintId == "paymentInstrument" && dh.PropertyName == "id").Single() as PropertyDisplayHint;
            Assert.IsNotNull(piSelectionHint, "Pidl is expected to have a PropertyDisplayHint pointing to a property named \"id\"");
            Assert.IsTrue(piSelectionHint.PossibleOptions.ContainsKey(nonZeroPIId), "PossibleOptions should have the non 0 balance csv");
            if (Microsoft.Commerce.Payments.PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner))
            {
                Assert.IsFalse(piSelectionHint.PossibleOptions.ContainsKey(zeroPIId), "PossibleOptions should not have the 0 balance csv for xbox native partners");
            }
            else
            {
                Assert.IsTrue(piSelectionHint.PossibleOptions.ContainsKey(zeroPIId), "PossibleOptions should have 0 balance csv for other partners");
            }

            this.TestContext.WriteLine("...done");
        }

        // when no default PM is specified, the normal ordering is expected and IsSelectedFirst is unchanged
        [DataRow(null, "credit_card_visa_amex", false)]

        // when an incomplete non-collapsing PM is specified, the normal ordering is expected and IsSelectedFirst is unchanged
        [DataRow("invoice", "credit_card_visa_amex", false)]
        [DataRow("virtual", "credit_card_visa_amex", false)]

        // when a complete non-collapsing PM is specified that exists, the ordering expects that PM first and IsSelectedFirst is true
        [DataRow("virtual.invoice", "virtual_invoice", true)]

        // when a complete non-collapsing PM is specified that does not exist, the normal ordering is expected and IsSelectedFirst is unchanged
        [DataRow("a.b", "credit_card_visa_amex", false)]

        // when an incomplete collapsing PM with family is specified that exists, the ordering expects that PM first and IsSelectedFirst is true
        [DataRow("credit_card", "credit_card_visa_amex", true)]

        // alternatively, when a complete collapsing PM with family and collapsed types is specified that exists, the ordering expects that PM first and IsSelectedFirst is true
        [DataRow("credit_card.visa_amex", "credit_card_visa_amex", true)]

        // when an incomplete collapsing PM with type is specified, the normal ordering is expected and IsSelectedFirst is unchanged
        [DataRow("visa", "credit_card_visa_amex", false)]

        // when a complete collapsing PM is specified, the normal ordering is expected and IsSelectedFirst is unchanged
        [DataRow("credit_card.visa", "credit_card_visa_amex", false)]
        [TestMethod]
        public void GetPaymentSelectDescriptions_Select_DropDownPartner_DefaultPaymentMethodIsFirstAsExpected(string defaultPaymentMethod, string expectedFirstKey, bool isSelectedFirstPropertyExpected)
        {
            this.TestContext.WriteLine("Start testing: defaultPaymentMethod \"{0}\"", defaultPaymentMethod);

            // Arrange
            var paymentMethods = new HashSet<PaymentMethod>();
            paymentMethods.Add(new PaymentMethod() { PaymentMethodFamily = "credit_card", PaymentMethodType = "visa", Properties = new PaymentMethodCapabilities() });
            paymentMethods.Add(new PaymentMethod() { PaymentMethodFamily = "credit_card", PaymentMethodType = "amex", Properties = new PaymentMethodCapabilities() });
            paymentMethods.Add(new PaymentMethod() { PaymentMethodFamily = "virtual", PaymentMethodType = "invoice", Properties = new PaymentMethodCapabilities() });
            paymentMethods.Add(new PaymentMethod() { PaymentMethodFamily = "virtual", PaymentMethodType = "legacy_invoice", Properties = new PaymentMethodCapabilities() });

            var collapsedSanitizedPaymentMethodNames = new List<string>
            {
                "credit_card_visa_amex",
                "virtual_invoice",
                "virtual_legacy_invoice",
            };

            // Act
            var pidls = PIDLResourceFactory.GetPaymentSelectDescriptions(paymentMethods, "us", "select", "en-us", "azure", defaultPaymentMethod: defaultPaymentMethod);

            // Assert
            PidlAssert.IsValid(pidls, 1);

            var pidl = pidls.First();

            PropertyDisplayHint displayHint = pidl.DisplayHints().Where(dh => dh.HintId == "paymentMethod" && dh.PropertyName == "displayId").Single() as PropertyDisplayHint;

            Assert.IsNotNull(displayHint, "Pidl is expected to have a PropertyDisplayHint pointing to a property named \"displayId\"");
            CollectionAssert.AreEquivalent(displayHint.PossibleOptions.Keys, collapsedSanitizedPaymentMethodNames, "Select PM Pidl's paymentMethods.PossibleOptions is expected to have paymentMethods as the key");

            Assert.AreEqual(displayHint.PossibleOptions.Keys.First(), expectedFirstKey, string.Format("paymentMethods.PossibleOptions first key is expected to a be {0}", expectedFirstKey));
            Assert.AreEqual(displayHint.IsSelectFirstItem ?? false, isSelectedFirstPropertyExpected, string.Format("paymentMethods.IsSelectFirstItem is expected to be {0}", isSelectedFirstPropertyExpected));

            this.TestContext.WriteLine("...done");
        }

        [TestMethod]
        public void GetPaymentSelectDescriptions_Select_DropDownWebblends_Scenario()
        {
            this.TestContext.WriteLine("Start testing: dropdown scenario");

            // Arrange
            var paymentMethods = new HashSet<PaymentMethod>();
            paymentMethods.Add(new PaymentMethod() { PaymentMethodFamily = "credit_card", PaymentMethodType = "visa", Properties = new PaymentMethodCapabilities() });
            paymentMethods.Add(new PaymentMethod() { PaymentMethodFamily = "credit_card", PaymentMethodType = "amex", Properties = new PaymentMethodCapabilities() });
            paymentMethods.Add(new PaymentMethod() { PaymentMethodFamily = "virtual", PaymentMethodType = "invoice", Properties = new PaymentMethodCapabilities() });
            paymentMethods.Add(new PaymentMethod() { PaymentMethodFamily = "virtual", PaymentMethodType = "legacy_invoice", Properties = new PaymentMethodCapabilities() });

            // Act
            var pidls = PIDLResourceFactory.GetPaymentSelectDescriptions(paymentMethods, "us", "select", "en-us", "webblends", scenario: "paymentMethodAsDropdown");

            // Assert
            PidlAssert.IsValid(pidls, 1);

            var pidl = pidls.First();

            PropertyDisplayHint displayHint = pidl.GetDisplayHintById("paymentMethodDropdown") as PropertyDisplayHint;

            Assert.IsNotNull(displayHint, "Pidl is expected to have a DisplayHint pointing to a property named \"paymentMethodDropdown\"");
            Assert.IsTrue(displayHint.SelectType == "dropDown", "Pidl is expected to have a SelectType of \"dropDown\"");

            this.TestContext.WriteLine("...done");
        }

        [TestMethod]
        public void GetPaymentSelectDescriptions_Select_DropDownWebblends_NoScenario()
        {
            this.TestContext.WriteLine("Start testing: no scenario");

            // Arrange
            var paymentMethods = new HashSet<PaymentMethod>();
            paymentMethods.Add(new PaymentMethod() { PaymentMethodFamily = "credit_card", PaymentMethodType = "visa", Properties = new PaymentMethodCapabilities() });
            paymentMethods.Add(new PaymentMethod() { PaymentMethodFamily = "credit_card", PaymentMethodType = "amex", Properties = new PaymentMethodCapabilities() });
            paymentMethods.Add(new PaymentMethod() { PaymentMethodFamily = "virtual", PaymentMethodType = "invoice", Properties = new PaymentMethodCapabilities() });
            paymentMethods.Add(new PaymentMethod() { PaymentMethodFamily = "virtual", PaymentMethodType = "legacy_invoice", Properties = new PaymentMethodCapabilities() });

            // Act
            var pidls = PIDLResourceFactory.GetPaymentSelectDescriptions(paymentMethods, "us", "select", "en-us", "webblends", scenario: string.Empty);

            // Assert
            PidlAssert.IsValid(pidls, 1);

            var pidl = pidls.First();

            PropertyDisplayHint displayHint = pidl.GetDisplayHintById("paymentMethod") as PropertyDisplayHint;

            Assert.IsNotNull(displayHint, "Pidl is expected to have a DisplayHint pointing to a property named \"paymentMethod\"");
            Assert.IsTrue(displayHint.SelectType == "buttonList", "Pidl is expected to have a SelectType of \"buttonList\"");

            this.TestContext.WriteLine("...done");
        }

        [DataRow("mc", "logo_mc_left_aligned.svg", "storify", true, "")]
        [DataRow("dkt-dk-nonsim", "v2_logo_dkt_dk_nonsim.svg", "storify", true, "")]
        [DataRow("nlk-nl-nonsim", "v2_logo_nlk_nl_nonsim.svg", "storify", true, "")]
        [DataRow("net-no-nonsim", "v2_logo_net_no_nonsim.svg", "storify", true, "")]
        [DataRow("at1-at-nonsim", "v2_logo_at1_at_nonsim.svg", "storify", true, "")]
        [DataRow("dig-my-nonsim", "v2_logo_dig_my_nonsim.svg", "storify", true, "")]
        [DataRow("m13-sg-nonsim", "v2_logo_m13_sg_nonsim.svg", "storify", true, "")]
        [DataRow("org-es-nonsim", "v2_logo_org_es_nonsim.svg", "storify", true, "")]
        [DataRow("sun-ch-nonsim", "v2_logo_sun_ch_nonsim.svg", "storify", true, "")]
        [DataRow("sta-sg-nonsim", "v2_logo_sta_sg_nonsim.svg", "storify", true, "")]
        [DataRow("tmo-at-nonsim", "v2_logo_tmo_at_nonsim.svg", "storify", true, "")]
        [DataRow("tmo-cz-nonsim", "v2_logo_tmo_cz_nonsim.svg", "storify", true, "")]
        [DataRow("tmo-gb-nonsim", "v2_logo_tmo_gb_nonsim.svg", "storify", true, "")]
        [DataRow("tmo-de-nonsim", "v2_logo_tmo_de_nonsim.svg", "storify", true, "")]
        [DataRow("tmo-nl-nonsim", "v2_logo_tmo_nl_nonsim.svg", "storify", true, "")]
        [DataRow("tmo-sk-nonsim", "v2_logo_tmo_sk_nonsim.svg", "storify", true, "")]
        [DataRow("zai-sa-nonsim", "v2_logo_zai_sa_nonsim.svg", "storify", true, "")]
        [DataRow("mts-ru-nonsim", "v2_logo_mts_ru_nonsim.svg", "storify", true, "")]
        [DataRow("pro-be-nonsim", "v2_logo_pro_be_nonsim.svg", "storify", true, "")]
        [DataRow("tli-dk-nonsim", "v2_logo_tli_dk_nonsim.svg", "storify", true, "")]
        [DataRow("viv-br-nonsim", "v2_logo_viv_br_nonsim.svg", "storify", true, "")]
        [DataRow("paysafecard", "v2_logo_paysafecard.svg", "storify", true, "")]
        [DataRow("paysafecard", "v2_logo_paysafecard.svg", "xboxsubs", true, "")]
        [DataRow("paysafecard", "v2_logo_paysafecard.svg", "xboxsettings", true, "")]
        [DataRow("paysafecard", "v2_logo_paysafecard.svg", "saturn", true, "")]
        [DataRow("paysafecard", "v2_logo_paysafecard.svg", "windowssettings", true, "PXUseAlternateSVG")]
        [DataRow("paysafecard", "v2_logo_paysafecard.svg", "webblends", false, "")]
        [DataTestMethod]
        public void GetAlternatePaymentMethodLogoUrl_ReturnsExpectedAlternateLogoUrl_ForEnabledPartners(string type, string expectedAlternateSvg, string partner, bool alternateLogoExpected, string flight)
        {
            PaymentMethod pm = new PaymentMethod { PaymentMethodType = type };
            var exposedFlightFeatures = new List<string>
            {
                flight
            };
            string logoUrl = PaymentSelectionHelper.CheckForReactNativeAlternatePaymentMethodLogoUrl(pm, partner, exposedFlightFeatures);

            string baseUrl = "https://pmservices.cp.microsoft.com/staticresourceservice/images/v4";

            if (alternateLogoExpected)
            {
                Assert.AreEqual($"{baseUrl}/{expectedAlternateSvg}", logoUrl);
            }
            else
            {
                Assert.IsNull(logoUrl);
            }
        }

        [DataRow("mc", "storify", true, "")]
        [DataRow("mc", "windowssettings", true, "PXUseAlternateSVG")]
        [DataRow("mc", "oxowebdirect", false, "")]
        [DataRow("mc", "webblends", false, "")]
        [DataRow("mc", "xbox", false, "")]
        [DataRow("dkt-dk-nonsim", "storify", true, "")]
        [DataRow("dkt-dk-nonsim", "windowssettings", true, "PXUseAlternateSVG")]
        [DataRow("dkt-dk-nonsim", "oxowebdirect", false, "")]
        [DataRow("dkt-dk-nonsim", "webblends", false, "")]
        [DataRow("dkt-dk-nonsim", "xbox", false, "")]
        [DataRow("viv-br-nonsim", "storify", true, "")]
        [DataRow("viv-br-nonsim", "windowssettings", true, "PXUseAlternateSVG")]
        [DataRow("viv-br-nonsim", "oxowebdirect", false, "")]
        [DataRow("viv-br-nonsim", "webblends", false, "")]
        [DataRow("viv-br-nonsim", "xbox", false, "")]
        [DataRow("dkt-dk-nonsim", "storify", true, "")]
        [DataRow("dkt-dk-nonsim", "windowssettings", true, "PXUseAlternateSVG")]
        [DataRow("dkt-dk-nonsim", "oxowebdirect", false, "")]
        [DataRow("dkt-dk-nonsim", "webblends", false, "")]
        [DataRow("dkt-dk-nonsim", "xbox", false, "")]
        [DataRow("nlk-nl-nonsim", "storify", true, "")]
        [DataRow("nlk-nl-nonsim", "windowssettings", true, "PXUseAlternateSVG")]
        [DataRow("nlk-nl-nonsim", "webblends", false, "")]
        [DataRow("nlk-nl-nonsim", "oxowebdirect", false, "")]
        [DataRow("nlk-nl-nonsim", "xbox", false, "")]
        [DataTestMethod]
        public void AlternateSVGFunctionReturnNull_ForNonEnabledPartners(string paymentMethodType, string partner, bool alternateLogoExpected, string flight)
        {
            PaymentMethod pm = new PaymentMethod { PaymentMethodType = paymentMethodType };
            var exposedFlightFeatures = new List<string>
            {
                flight
            };
            string returnedVal = PaymentSelectionHelper.CheckForReactNativeAlternatePaymentMethodLogoUrl(pm, partner, exposedFlightFeatures);

            if (alternateLogoExpected)
            {
                Assert.IsNotNull(returnedVal);
            }
            else
            {
                Assert.IsNull(returnedVal);
            }
        }

        private void Assert_IsAddPIActionContext(object context, string country, string language, HashSet<PaymentMethod> paymentMethods, string partner, bool isTypeParameterExpected)
        {
            Assert.IsNotNull(context, "Action context is expected to be non null");
            Assert.IsInstanceOfType(context, typeof(ActionContext), "Action context is expected to be of type ActionContext");

            ActionContext actionContext = context as ActionContext;
            Assert.AreEqual("addResource", actionContext.Action, "Action context's action is expected to be addResource");
            Assert.IsNotNull(actionContext.ResourceActionContext, "Resource action context is expected to be not null");
            Assert.AreEqual("addResource", actionContext.ResourceActionContext.Action, "Resource action context's action is expected to be addResource");

            var expectedFamily = paymentMethods.Where(pm => !string.Equals("stored_value", pm.PaymentMethodType, StringComparison.OrdinalIgnoreCase)).First().PaymentMethodFamily;
            var expectedTypes = paymentMethods.Select(pm => pm.PaymentMethodType).Where(pmType => !string.Equals("stored_value", pmType, StringComparison.OrdinalIgnoreCase)).ToArray();
            Assert.AreEqual(expectedFamily, actionContext.PaymentMethodFamily, "Action context's PaymentMethodFamily is expected to be {0}", expectedFamily);
            CollectionAssert.AreEquivalent(expectedTypes, actionContext.PaymentMethodType.Split(','), "Comma separated payment method types are expected to have the same types ignoring order");
            Assert.AreEqual(string.Format("{0}.{1}", expectedFamily, actionContext.PaymentMethodType), actionContext.Id, "Action context's id must be as expected");

            PidlDocInfo returnPidlDocInfo = actionContext.ResourceActionContext.PidlDocInfo;
            Assert.IsNotNull(returnPidlDocInfo, "Resource action context's PidlDocInfo is expected to be not null");
            Assert.AreEqual(TestConstants.DescriptionTypes.PaymentInstrumentDescription, returnPidlDocInfo.ResourceType, "PidlDocInfo's ResourceType is expected to be paymentInstrument");

            var expectedParameters = new Dictionary<string, string>() { { "language", language }, { "country", country }, { "partner", partner }, { "family", expectedFamily } };
            if (isTypeParameterExpected)
            {
                expectedParameters.Add("type", expectedTypes.First());
            }

            CollectionAssert.AreEquivalent(expectedParameters, returnPidlDocInfo.Parameters);
        }

        [DataRow("issuer", "1234")]
        [DataRow("test", "12")]
        [DataRow("", "1234")]
        [DataRow("issuer", "")]
        [DataRow("LongTextString", "1234567890")]
        [TestMethod]
        public void GetPaymentSelectDescriptions_Select_IBAfieldsareasexpected(string issuer, string bankAccountLastFourDigits)
        {
            List<string> exposedFlightFeatures = new List<string>();
            foreach (var partner in TestConstants.AllPartners)
            {
                this.TestContext.WriteLine("Start testing: Partner \"{0}\"", partner);

                if (Microsoft.Commerce.Payments.PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner))
                {
                    continue;
                }

                // Arrange
                var paymentMethods = new HashSet<PaymentMethod>();
                var iba = new PaymentMethod()
                {
                    PaymentMethodFamily = "direct_debit",
                    PaymentMethodType = "ideal_billing_agreement",
                    Properties = new PaymentMethodCapabilities()
                };
                var piId = "MockPIId";
                paymentMethods.Add(iba);
                var pis = new List<PaymentInstrument>()
                {
                    new PaymentInstrument()
                    {
                        PaymentMethod = iba,
                        PaymentInstrumentId = piId,
                        Status = PaymentInstrumentStatus.Active,
                        PaymentInstrumentDetails = new PaymentInstrumentDetails()
                        {
                            BankAccountLastFourDigits = bankAccountLastFourDigits,
                            Issuer = issuer
                        }
                    }
                };

                // Act
                List<PIDLResource> pidls = PIDLResourceFactory.GetPaymentSelectDescriptions(paymentMethods, "nl", "test", "en-us", partner, paymentInstruments: pis);

                // Assert
                PidlAssert.IsValid(pidls);
                var piSelectionHint = pidls[0].DisplayHints().Where(dh => dh.HintId == "paymentInstrument" && dh.PropertyName == "id").Single() as PropertyDisplayHint;
                Assert.IsNotNull(piSelectionHint, "Pidl is expected to have a PropertyDisplayHint pointing to a property named \"id\"");
                CollectionAssert.Contains(piSelectionHint.PossibleOptions.Keys, piId, "List PI Pidl's PossibleOptions is expected to have PIId as the key");
                var piHint = piSelectionHint.PossibleOptions[piId] as SelectOptionDescription;
                Assert.AreEqual("success", piHint.PidlAction.ActionType, "PidlAction is expected to be \"success\"");
                Assert.IsNotNull(piHint.PidlAction.Context, "PidlAction.Context is expected to be not null");
                Assert.AreEqual(piId, (piHint.PidlAction.Context as ActionContext)?.Id, "PidlAction.Context.Instance is expected to match input");
                Assert.AreEqual(pis.First(), (piHint.PidlAction.Context as ActionContext)?.Instance, "PidlAction.Context.Instance is expected to be the full PI");
                Assert.IsNotNull(pis[0].PaymentInstrumentDetails.BankAccountLastFourDigits, "BankAccountLastFourDigits expected to exist on PI");
                Assert.IsNotNull(pis[0].PaymentInstrumentDetails.Issuer, "issuer property expected to exist on PI");
                Assert.AreEqual(pis[0].PaymentInstrumentDetails.BankAccountLastFourDigits, bankAccountLastFourDigits, "PaymentInstrumentDetails.BankAccountLastFourDigits of pidl is expected to match bankAccountLastFourDigits from input");
                Assert.AreEqual(pis[0].PaymentInstrumentDetails.Issuer, issuer, "PaymentInstrumentDetails.Issuer of pidl is expected to match issuer from input");
                this.TestContext.WriteLine("...done");
            }
        }
    }
}
