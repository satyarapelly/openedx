// <copyright file="GetPaymentMethodDescriptionsTests.cs" company="Microsoft">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace CIT.PidlFactory.V7
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Helpers;
    using Microsoft.Commerce.Payments.PidlFactory;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Microsoft.Commerce.Payments.Common.Web;
    using PXCommonConstants = Microsoft.Commerce.Payments.PXCommon.Constants;
    using static CIT.PidlFactory.V7.TestConstants;

    [TestClass]
    public class GetPaymentMethodDescriptionsTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void PidlFactoryPositivePaymentMethodDescriptions()
        {
            using (StreamReader sw = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestData\GetPaymentMethodDescriptionsPositiveTestCases.csv")))
            {
                // Skip headers
                sw.ReadLine();
                string line;

                while ((line = sw.ReadLine()) != null)
                {
                    // Column order is Country, Family, Type, Operation, Language
                    string[] rowValues = line.Split(',');

                    PaymentMethod pi = new PaymentMethod() { PaymentMethodId = "1a2b3c4d", PaymentMethodFamily = rowValues[(int)GetPaymentMethodDescriptionTestColumns.Family], PaymentMethodType = rowValues[(int)GetPaymentMethodDescriptionTestColumns.Type] };
                    HashSet<PaymentMethod> testPIs = new HashSet<PaymentMethod>();
                    testPIs.Add(pi);

                    List<PIDLResource> pidls = null;
                    pidls = PIDLResourceFactory.Instance.GetPaymentMethodDescriptions(
                        testPIs,
                        rowValues[(int)GetPaymentMethodDescriptionTestColumns.Country],
                        rowValues[(int)GetPaymentMethodDescriptionTestColumns.Family],
                        rowValues[(int)GetPaymentMethodDescriptionTestColumns.Type],
                        rowValues[(int)GetPaymentMethodDescriptionTestColumns.Operation],
                        rowValues[(int)GetPaymentMethodDescriptionTestColumns.Language]);

                    string pidl = JsonConvert.SerializeObject(pidls);
                    Assert.IsFalse(string.IsNullOrWhiteSpace(pidl));
                    Assert.AreEqual(pidls.SingleOrDefault().Identity[TestConstants.DescriptionIdentityFields.Country], rowValues[(int)GetPaymentMethodDescriptionTestColumns.Country]);

                    if (!rowValues[(int)GetPaymentMethodDescriptionTestColumns.Country].Equals("kr"))
                    {
                        Assert.AreEqual(pidls.SingleOrDefault().Identity[TestConstants.DescriptionIdentityFields.Type], rowValues[(int)GetPaymentMethodDescriptionTestColumns.Type]);
                    }

                    Assert.AreEqual(pidls.SingleOrDefault().Identity[TestConstants.DescriptionIdentityFields.Family], rowValues[(int)GetPaymentMethodDescriptionTestColumns.Family]);
                    Assert.AreEqual(pidls.SingleOrDefault().Identity[TestConstants.DescriptionIdentityFields.Operation], rowValues[(int)GetPaymentMethodDescriptionTestColumns.Operation]);
                }
            }
        }

        [TestMethod]
        public void PidlFactoryNegativePaymentMethodDescriptions()
        {
            using (StreamReader sw = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestData\GetPaymentMethodDescriptionsNegativeTestCases.csv")))
            {
                // Skip headers
                sw.ReadLine();
                string line;

                while ((line = sw.ReadLine()) != null)
                {
                    // Column order is Country, Family, Type, Operation, Language, ExpectedExceptionType
                    string[] rowValues = line.Split(',');

                    PaymentMethod pi = new PaymentMethod() { PaymentMethodId = "1a2b3c4d", PaymentMethodFamily = rowValues[(int)GetPaymentMethodDescriptionTestColumns.Family], PaymentMethodType = rowValues[(int)GetPaymentMethodDescriptionTestColumns.Type] };
                    HashSet<PaymentMethod> testPIs = new HashSet<PaymentMethod>();
                    testPIs.Add(pi);

                    try
                    {
                        List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetPaymentMethodDescriptions(
                            testPIs,
                            rowValues[(int)GetPaymentMethodDescriptionTestColumns.Country],
                            rowValues[(int)GetPaymentMethodDescriptionTestColumns.Family],
                            rowValues[(int)GetPaymentMethodDescriptionTestColumns.Type],
                            rowValues[(int)GetPaymentMethodDescriptionTestColumns.Operation],
                            rowValues[(int)GetPaymentMethodDescriptionTestColumns.Language]);
                    }
                    catch (PIDLArgumentException pidlArgEx)
                    {
                        var ex = pidlArgEx;
                        Assert.AreEqual(rowValues[(int)GetPaymentMethodDescriptionTestColumns.ExpectedExceptionType], "PIDLArgumentException");
                    }
                    catch (Exception ex)
                    {
                        var extype = Activator.CreateInstance(Type.GetType("System." + rowValues[(int)GetPaymentMethodDescriptionTestColumns.ExpectedExceptionType]));
                        Assert.IsInstanceOfType(ex, extype.GetType());
                    }
                }
            }
        }

        [TestMethod]
        public void PidlFactoryGetAlipayEwalletPidlsForSupportedOperations()
        {
            const string Country = "cn";
            const string Family = TestConstants.PaymentMethodFamilyNames.Ewallet;
            const string Type = TestConstants.PaymentMethodTypeNames.Alipay;
            string[] supportedOperations = new string[] { TestConstants.PidlOperationTypes.Add, TestConstants.PidlOperationTypes.Update };
            const string Language = "en-us";
            PaymentMethod pi = new PaymentMethod() { PaymentMethodId = "1a2b3c4d", PaymentMethodFamily = Family, PaymentMethodType = Type };
            HashSet<PaymentMethod> testPIs = new HashSet<PaymentMethod>();
            testPIs.Add(pi);

            foreach (string operation in supportedOperations)
            {
                List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetPaymentMethodDescriptions(testPIs, Country, Family, Type, operation, Language);

                foreach (PIDLResource pidl in pidls)
                {
                    Assert.AreEqual(pidl.Identity[TestConstants.DescriptionIdentityFields.Country], "cn");
                    Assert.AreEqual(pidl.Identity[TestConstants.DescriptionIdentityFields.Family], TestConstants.PaymentMethodFamilyNames.Ewallet);
                    Assert.AreEqual(pidl.Identity[TestConstants.DescriptionIdentityFields.Type], TestConstants.PaymentMethodTypeNames.Alipay);
                    Assert.IsTrue(pidl.Identity[TestConstants.DescriptionIdentityFields.ResourceIdentity].Equals(TestConstants.PaymentMethodFamilyNames.Ewallet + "." + TestConstants.InternalPaymentMethodTypeNames.AlipayQrCode));
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void PidlFactoryGetPaypalEwalletPidlsForUpdateShouldFail()
        {
            // Updating an PayPal ewallet is not supported.
            const string Country = "us";
            const string Family = TestConstants.PaymentMethodFamilyNames.Ewallet;
            const string Type = TestConstants.PaymentMethodTypeNames.Paypal;
            const string Operation = TestConstants.PidlOperationTypes.Update;
            const string Language = "en-us";
            PaymentMethod pi = new PaymentMethod() { PaymentMethodId = "1a2b3c4d", PaymentMethodFamily = Family, PaymentMethodType = Type };
            HashSet<PaymentMethod> testPIs = new HashSet<PaymentMethod>();
            testPIs.Add(pi);

            List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetPaymentMethodDescriptions(testPIs, Country, Family, Type, Operation, Language);
        }

        [TestMethod]
        [ExpectedException(typeof(PIDLArgumentException))]
        public void PidlFactoryAddMobileBillingNonSimCountryIsEmpty()
        {
            const string Country = "";
            const string Family = TestConstants.PaymentMethodFamilyNames.MobileBillingNonSim;
            const string Type = TestConstants.DescriptionTypes.PaymentMethodDescription;
            const string Operation = "add";
            const string Language = "en-us";
            PaymentMethod pi = new PaymentMethod() { PaymentMethodId = "1a2b3c4d", PaymentMethodFamily = Family, PaymentMethodType = Type };
            HashSet<PaymentMethod> testPIs = new HashSet<PaymentMethod>();
            testPIs.Add(pi);

            List<PIDLResource> pidls = null;
            pidls = PIDLResourceFactory.Instance.GetPaymentMethodDescriptions(testPIs, Country, Family, Type, Operation, Language);
        }

        [TestMethod]
        [ExpectedException(typeof(PIDLArgumentException))]
        public void PidlFactoryAddMobileBillingNonSimOperationIsEmpty()
        {
            const string Country = "cn";
            const string Family = TestConstants.PaymentMethodFamilyNames.MobileBillingNonSim;
            const string Type = TestConstants.DescriptionTypes.PaymentMethodDescription;
            string operation = string.Empty;
            const string Language = "en-us";
            PaymentMethod pi = new PaymentMethod() { PaymentMethodId = "1a2b3c4d", PaymentMethodFamily = Family, PaymentMethodType = Type };
            HashSet<PaymentMethod> testPIs = new HashSet<PaymentMethod>();
            testPIs.Add(pi);

            List<PIDLResource> pidls = null;
            pidls = PIDLResourceFactory.Instance.GetPaymentMethodDescriptions(testPIs, Country, Family, Type, operation, Language);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void PidlFactoryAddMobileBillingWrongPaymentFamily()
        {
            const string Country = "cn";
            const string Family = "wrong_payment_family";
            const string Type = TestConstants.DescriptionTypes.PaymentMethodDescription;
            const string Operation = TestConstants.PidlOperationTypes.Add;
            const string Language = "en-us";
            PaymentMethod pi = new PaymentMethod() { PaymentMethodId = "1a2b3c4d", PaymentMethodFamily = Family, PaymentMethodType = Type };
            HashSet<PaymentMethod> testPIs = new HashSet<PaymentMethod>();
            testPIs.Add(pi);

            List<PIDLResource> pidls = null;

            pidls = PIDLResourceFactory.Instance.GetPaymentMethodDescriptions(testPIs, Country, Family, Type, Operation, Language);
        }

        [TestMethod]
        public void PidlFactoryAddCreditCardIfCountryIsKoreaThenRemoveType()
        {
            const string Country = "kr";
            const string Family = TestConstants.PaymentMethodFamilyNames.CreditCard;
            const string Operation = TestConstants.PidlOperationTypes.Add;
            const string Language = "en-us";
            foreach (string krtype in TestConstants.KoreaCreditCardType.TypeNames)
            {
                PaymentMethod pi = new PaymentMethod() { PaymentMethodId = "1a2b3c4d", PaymentMethodFamily = Family, PaymentMethodType = krtype };
                HashSet<PaymentMethod> testPIs = new HashSet<PaymentMethod>();
                testPIs.Add(pi);

                List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetPaymentMethodDescriptions(testPIs, Country, Family, krtype, Operation, Language);
                string pidl = JsonConvert.SerializeObject(pidls);
                Assert.IsFalse(string.IsNullOrWhiteSpace(pidl));
                Assert.IsTrue(pidls.SingleOrDefault().Identity[TestConstants.DescriptionIdentityFields.Country].Equals(Country));
                Assert.IsTrue(pidls.SingleOrDefault().Identity[TestConstants.DescriptionIdentityFields.Operation].Equals(Operation));
                Assert.IsTrue(pidls.SingleOrDefault().Identity[TestConstants.DescriptionIdentityFields.Family].Equals(Family));
                Assert.IsFalse(pidls.Any(p => p.Identity.ContainsKey("type")));
            }
        }

        [TestMethod]
        public void PidlFactoryAddCreditCardCheckValidation()
        {
            const string Country = "us";
            const string Family = TestConstants.PaymentMethodFamilyNames.CreditCard;
            const string Operation = TestConstants.PidlOperationTypes.Add;
            const string Language = "en-us";
            foreach (string type in TestConstants.UsCreditCardType.TypeNames)
            {
                PaymentMethod pi = new PaymentMethod() { PaymentMethodId = "1a2b3c4d", PaymentMethodFamily = Family, PaymentMethodType = type };
                HashSet<PaymentMethod> testPIs = new HashSet<PaymentMethod>();
                testPIs.Add(pi);

                List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetPaymentMethodDescriptions(testPIs, Country, Family, type, Operation, Language);

                PropertyDescription accountToken = pidls.SingleOrDefault().GetPropertyDescriptionByPropertyName("accountToken");
                Assert.IsNotNull(accountToken, "AccountToken property not found in credit card PIDL");

                // verify validation object used by previous client versions
                Assert.IsNotNull(accountToken.Validation);
                Assert.AreEqual(accountToken.Validation.ValidationType, TestConstants.ValidationTypes.Regex);
                Assert.IsNotNull(accountToken.Validation.Regex);

                // verify validations list used by new client versions
                Assert.IsNotNull(accountToken.Validations);
                Assert.AreEqual(accountToken.Validations.Count, 2);

                Assert.AreEqual(accountToken.Validations[0].ValidationType, TestConstants.ValidationTypes.Regex);
                Assert.IsNotNull(accountToken.Validations[0].Regex);

                Assert.AreEqual(accountToken.Validations[1].ValidationType, TestConstants.ValidationTypes.Function);
                Assert.AreEqual(accountToken.Validations[1].ValidationFunction, "luhn");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(PIDLArgumentException))]
        public void PidlFactoryAddMobileBillingBadPartnerName()
        {
            const string Country = "cn";
            const string Family = TestConstants.PaymentMethodFamilyNames.MobileBillingNonSim;
            const string Type = TestConstants.DescriptionTypes.PaymentMethodDescription;
            const string Operation = TestConstants.PidlOperationTypes.Add;
            const string Language = "en-us";
            PaymentMethod pi = new PaymentMethod() { PaymentMethodId = "1a2b3c4d", PaymentMethodFamily = Family, PaymentMethodType = Type };
            HashSet<PaymentMethod> testPIs = new HashSet<PaymentMethod>();
            testPIs.Add(pi);

            List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetPaymentMethodDescriptions(testPIs, Country, Family, Type, Operation, Language, "wrongpartnername");
        }

        [TestMethod]
        public void PidlFactoryAddMobileBillingIgnoreGarbageParameters()
        {
            const string Country = "cn";
            const string Family = TestConstants.PaymentMethodFamilyNames.MobileBillingNonSim;
            const string Type = TestConstants.DescriptionTypes.PaymentMethodDescription;
            const string Operation = TestConstants.PidlOperationTypes.Add;
            const string Language = "en-us";
            PaymentMethod pi = new PaymentMethod() { PaymentMethodId = "1a2b3c4d", PaymentMethodFamily = Family, PaymentMethodType = Type };
            HashSet<PaymentMethod> testPIs = new HashSet<PaymentMethod>();
            testPIs.Add(pi);

            List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetPaymentMethodDescriptions(
                testPIs,
                Country,
                Family,
                Type,
                Operation,
                Language,
                TestConstants.PidlConfig.DefaultPartnerName,
                "invalidemailaddress",
                "invalidclassicProduct",
                "invalidbillableAccountId",
                new Dictionary<string, object>(),
                true);

            string pidl = JsonConvert.SerializeObject(pidls);
            Assert.IsFalse(string.IsNullOrWhiteSpace(pidl));
            Assert.AreEqual(pidls.SingleOrDefault().Identity["country"], Country);
            Assert.AreEqual(pidls.SingleOrDefault().Identity["operation"], Operation);
            Assert.AreEqual(pidls.SingleOrDefault().Identity["family"], Family);
            Assert.AreEqual(pidls.SingleOrDefault().Identity["type"], Type);
        }

        [TestMethod]
        public void PidlFactoryExpectUSArmedForcesStatesForUS()
        {
            string country = "us";
            string family = TestConstants.PaymentMethodFamilyNames.CreditCard;
            string operation = TestConstants.PidlOperationTypes.Add;
            string language = "en-us";

            Dictionary<string, string> expectedStateValues = PIDLResourceFactory.GetCopiedDictionaryFromDomainDictionaries(TestConstants.DomainDictionaryNames.USStates);
            PaymentMethod pi = new PaymentMethod() { PaymentMethodId = "1a2b3c4d", PaymentMethodFamily = family, PaymentMethodType = TestConstants.PaymentMethodTypeNames.Amex };
            HashSet<PaymentMethod> testPIs = new HashSet<PaymentMethod>();
            testPIs.Add(pi);

            List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetPaymentMethodDescriptions(testPIs, country, family, TestConstants.PaymentMethodTypeNames.Amex, operation, language);

            foreach (PIDLResource pidl in pidls)
            {
                PropertyDisplayHint displayHint = pidl.GetDisplayHintById(TestConstants.DisplayHintIds.AddressState) as PropertyDisplayHint;
                PropertyDescription propertyDescription = pidl.GetPropertyDescriptionByPropertyName(TestConstants.DataDescriptionIds.Region);

                if (displayHint != null && propertyDescription != null)
                {
                    Assert.AreEqual(expectedStateValues.Count, displayHint.PossibleValues.Count);
                    Assert.AreEqual(expectedStateValues.Count, propertyDescription.PossibleValues.Count);

                    foreach (string key in expectedStateValues.Keys)
                    {
                        Assert.IsTrue(displayHint.PossibleValues.ContainsKey(key));
                        Assert.AreEqual(expectedStateValues[key], displayHint.PossibleValues[key]);
                        Assert.IsTrue(propertyDescription.PossibleValues.ContainsKey(key));
                        Assert.AreEqual(expectedStateValues[key], propertyDescription.PossibleValues[key]);
                    }
                }
            }
        }

        [DataRow("amcweb", "add", "visa %2Cmc")]
        [DataRow("amcweb", "add", "visa %2Cmc")]
        [DataRow("azure", "update", "visa%2Camex")]
        [DataRow("azure", "update", "visa%2Camex")]
        [DataRow("cart", "add", "amex%2Cmc")]
        [DataRow("cart", "add", "amex%2Cmc")]
        [DataRow("commercialsupport", "update", "visa%2Camex%2Cmc")]
        [DataRow("commercialsupport", "update", "visa%2Camex%2Cmc")]
        [DataRow("consumersupport", "add", "amex%2Cmc")]
        [DataRow("consumersupport", "add", "amex%2Cmc")]
        [DataRow("office", "update", "visa%2Camex")]
        [DataRow("office", "update", "visa%2Camex")]
        [DataRow("webblends", "update", "amex%2Cdiscover%2Cmc%2Cvisa")]
        [DataRow("webblends", "update", "amex%2Cdiscover%2Cmc%2Cvisa")]
        [DataRow("oxowebdirect", "update", "amex%2Cdiscover%2Cmc%2Cvisa")]
        [DataRow("oxowebdirect", "update", "amex%2Cdiscover%2Cmc%2Cvisa")]
        [DataTestMethod]
        public void PidlFactoryExpectedINStates(string partner, string operation, string type)
        {
            string country = "in";
            string language = "en-us";
            string family = "credit_card";

            var expectedDomainDictionary = TestConstants.DomainDictionaryNames.INStates;
            var expectedDisplayHintId = TestConstants.DisplayHintIds.AddressState;

            Dictionary<string, string> allINStateValues = PIDLResourceFactory.GetCopiedDictionaryFromDomainDictionaries(TestConstants.DomainDictionaryNames.INStates);
            Dictionary<string, string> expectedINStateValues = PIDLResourceFactory.GetCopiedDictionaryFromDomainDictionaries(expectedDomainDictionary);

            HashSet<PaymentMethod> paymentMethods = new HashSet<PaymentMethod>()
            {
                new PaymentMethod() { PaymentMethodId = "credit_card.amex", PaymentMethodFamily = family, PaymentMethodType = TestConstants.PaymentMethodTypeNames.Amex },
                new PaymentMethod() { PaymentMethodId = "credit_card.mc", PaymentMethodFamily = family, PaymentMethodType = TestConstants.PaymentMethodTypeNames.Mc },
                new PaymentMethod() { PaymentMethodId = "credit_card.visa", PaymentMethodFamily = family, PaymentMethodType = TestConstants.PaymentMethodTypeNames.Visa }
            };

            List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetPaymentMethodDescriptions(paymentMethods, country, family, type, operation, language, partner);

            foreach (PIDLResource pidl in pidls)
            {
                PropertyDisplayHint displayHint = pidl.GetDisplayHintById(expectedDisplayHintId) as PropertyDisplayHint;
                PropertyDescription propertyDescription = pidl.GetPropertyDescriptionByPropertyName(TestConstants.DataDescriptionIds.Region);

                if (displayHint != null)
                {
                    Assert.AreEqual(allINStateValues.Count, propertyDescription.PossibleValues.Count);
                    Assert.AreEqual(expectedINStateValues.Count, displayHint.PossibleValues.Count);

                    foreach (string key in expectedINStateValues.Keys)
                    {
                        Assert.IsTrue(displayHint.PossibleValues.ContainsKey(key));
                        Assert.AreEqual(expectedINStateValues[key], displayHint.PossibleValues[key]);
                        Assert.IsTrue(propertyDescription.PossibleValues.ContainsKey(key));
                        Assert.AreEqual(expectedINStateValues[key], propertyDescription.PossibleValues[key]);
                    }
                }
            }
        }

        [TestMethod]
        public void PidlFactoryExpectBRStatesUsingFlightForBR()
        {
            string country = "br";
            string family = TestConstants.PaymentMethodFamilyNames.CreditCard;
            string operation = TestConstants.PidlOperationTypes.Add;
            string language = "en-us";

            Dictionary<string, string> expectedStateValues = PIDLResourceFactory.GetCopiedDictionaryFromDomainDictionaries(TestConstants.DomainDictionaryNames.BRStates);
            PaymentMethod pi = new PaymentMethod() { PaymentMethodId = "1a2b3c4d", PaymentMethodFamily = family, PaymentMethodType = TestConstants.PaymentMethodTypeNames.Amex };
            HashSet<PaymentMethod> testPIs = new HashSet<PaymentMethod>();
            testPIs.Add(pi);

            List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetPaymentMethodDescriptions(testPIs, country, family, TestConstants.PaymentMethodTypeNames.Amex, operation, language);

            foreach (PIDLResource pidl in pidls)
            {
                PropertyDisplayHint displayHint = pidl.GetDisplayHintById(TestConstants.DisplayHintIds.AddressState) as PropertyDisplayHint;
                PropertyDescription propertyDescription = pidl.GetPropertyDescriptionByPropertyName(TestConstants.DataDescriptionIds.Region);

                if (displayHint != null && propertyDescription != null)
                {
                    Assert.AreEqual(expectedStateValues.Count, displayHint.PossibleValues.Count);
                    Assert.AreEqual(expectedStateValues.Count, propertyDescription.PossibleValues.Count);

                    foreach (string key in expectedStateValues.Keys)
                    {
                        Assert.IsTrue(displayHint.PossibleValues.ContainsKey(key));
                        Assert.AreEqual(expectedStateValues[key], displayHint.PossibleValues[key]);
                        Assert.IsTrue(propertyDescription.PossibleValues.ContainsKey(key));
                        Assert.AreEqual(expectedStateValues[key], propertyDescription.PossibleValues[key]);
                    }
                }
            }
        }

        [TestMethod]
        public void PidlFactoryGetCupCreditCard()
        {
            const string Country = "cn";
            const string Language = "en-us";
            const string Family = "credit_card";
            const string Type = "unionpay_creditcard";
            const string Operation = "Add";
            PaymentMethod pi = new PaymentMethod() { PaymentMethodId = "1a2b3c4d", PaymentMethodFamily = Family, PaymentMethodType = Type };
            HashSet<PaymentMethod> testPIs = new HashSet<PaymentMethod>();
            testPIs.Add(pi);

            List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetPaymentMethodDescriptions(testPIs, Country, Family, Type, Operation, Language);
            Assert.IsTrue(pidls.Count == 1);
            PidlAssert.IsValid(pidls);
        }

        [DataRow("commercialstores", "add", "us", "withProfileAddress", 8)]
        [DataRow("commercialstores", "add", "us", "", 4)]
        [DataRow("commercialstores", "add", "jp", "withProfileAddress", 10)]
        [DataRow("commercialstores", "add", "jp", "", 5)]
        [DataRow("webblends", "add", "us", "", 4)]
        [DataRow("webblends", "add", "jp", "", 5)]
        [DataRow("oxowebdirect", "add", "us", "", 4)]
        [DataRow("oxowebdirect", "add", "jp", "", 5)]
        [DataTestMethod]
        public void GetPaymentMethodDescriptions_AddCreditCardWithUpdateAddressScenario(string partner, string operation, string country, string scenario, int expectedPidlNumber)
        {
            const string Language = "en-us";
            const string Family = "credit_card";
            PaymentMethod visa = new PaymentMethod() { PaymentMethodId = "001", PaymentMethodFamily = Family, PaymentMethodType = "visa" };
            PaymentMethod mc = new PaymentMethod() { PaymentMethodId = "002", PaymentMethodFamily = Family, PaymentMethodType = "mc" };
            PaymentMethod amex = new PaymentMethod() { PaymentMethodId = "003", PaymentMethodFamily = Family, PaymentMethodType = "amex" };
            PaymentMethod discover = new PaymentMethod() { PaymentMethodId = "004", PaymentMethodFamily = Family, PaymentMethodType = "discover" };
            PaymentMethod jcb = new PaymentMethod() { PaymentMethodId = "005", PaymentMethodFamily = Family, PaymentMethodType = "jcb" };
            HashSet<PaymentMethod> testPIs = new HashSet<PaymentMethod>();
            testPIs.Add(visa);
            testPIs.Add(mc);
            testPIs.Add(amex);
            testPIs.Add(discover);

            if (string.Equals(country, "jp", StringComparison.OrdinalIgnoreCase))
            {
                testPIs.Add(jcb);
            }

            List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetPaymentMethodDescriptions(testPIs, country, Family, null, operation, Language, partner, scenario: scenario);
            Assert.IsTrue(pidls.Count == expectedPidlNumber);
            PidlAssert.IsValid(pidls);
        }

        [DataRow("appsource", "credit_card", "")]
        [DataTestMethod]
        public void GetPaymentMethodShowDescriptions_ChangeLinkIsAsExpected(string partner, string family, string type)
        {
            const string Country = "us";
            const string Language = "en-US";

            List<PIDLResource> listPIPidls = PIDLResourceFactory.GetPaymentMethodShowDescriptions(family, type, Country, Language, partner);

            Assert.AreEqual(1, listPIPidls.Count, "Only one show PI PIDL is expected");

            PIDLResource listPIPidl = listPIPidls[0];

            // Verify Change link
            var changePILink = listPIPidl.GetDisplayHintById(TestConstants.ButtonDisplayHintIds.PaymentInstrumentShowPIChangeLink) as HyperlinkDisplayHint;
            Assert.AreEqual(changePILink.Action.ActionType, DisplayHintActionType.partnerAction.ToString(), "changePILink should have partnerAction as action type");

            var actionContext = changePILink.Action.Context as ActionContext;
            Assert.AreEqual(PaymentInstrumentActions.ToString(PIActionType.SelectResource), actionContext.Action);
            Assert.AreEqual(TestConstants.DescriptionTypes.PaymentMethodDescription, actionContext.ResourceActionContext.PidlDocInfo.ResourceType);
        }

        /// <summary>
        /// This test is used to verify the GetPaymentMethodSearchDescriptions method.
        /// </summary>
        /// <param name="partner"></param>
        /// <param name="family"></param>
        /// <param name="type"></param>
        [DataRow("consumersupport", "credit_card", "")]
        [DataRow("commercialsupport", "credit_card", "")]
        [DataRow("defaulttemplate", "credit_card", "")]
        [DataTestMethod]
        public void GetPaymentMethodSearchDescriptions_PidlIsExpected(string partner, string family, string type)
        {
            // Arrange
            const string Country = "us";
            const string Language = "en-US";

            // Act
            List<PIDLResource> listPIPidls = PIDLResourceFactory.GetPaymentMethodSearchDescriptions(family, type, Country, Language, partner);

            // Assert
            Assert.AreEqual(1, listPIPidls.Count, "Only one show PI PIDL is expected");
            Assert.AreEqual(1, listPIPidls[0].DisplayPages.Count, "Only one DisplayPages PI PIDL is expected");
            Assert.AreEqual("creditCardSearchPage", listPIPidls[0].DisplayPages[0].HintId, "search PI pidl sequenceId is expected to creditCardSearchPage");
        }

        /// <summary>
        /// This test is used to verify the pidl for show operation.
        /// </summary>
        /// <param name="partner"></param>
        /// <param name="family"></param>
        /// <param name="type"></param>
        [DataRow("commercialsupport", "credit_card", "")]
        [DataRow("commercialsupport", "ewallet", "paypal")]
        [DataRow("commercialsupport", "direct_debit", "sepa")]
        [DataRow("defaulttemplate", "credit_card", "")]
        [DataRow("defaulttemplate", "ewallet", "paypal")]
        [DataRow("defaulttemplate", "direct_debit", "sepa")]
        [DataTestMethod]
        public void GetPaymentMethodShowDescriptions_PidlIsExpected(string partner, string family, string type)
        {
            // Arrange
            const string Country = "us";
            const string Language = "en-US";
            var displayHintValueBasedOnFamilyOrType =
                family == "credit_card" ? "paymentInstrumentShowCCSinglePage" :
                type == "paypal" ? "paymentInstrumentShowPaypalSinglePage" :
                type == "sepa" ? "paymentInstrumentShowSepaSinglePage" :
                type == "ach" ? "paymentInstrumentShowAchSinglePage" :
                null;

            // Act
            List<PIDLResource> listPIPidls = PIDLResourceFactory.GetPaymentMethodShowDescriptions(family, type, Country, Language, partner);

            // Assert
            Assert.AreEqual(1, listPIPidls.Count, "Only one show PI PIDL is expected");
            Assert.AreEqual(1, listPIPidls[0].DisplayPages.Count, "Only one DisplayPages PI PIDL is expected");
            Assert.AreEqual(displayHintValueBasedOnFamilyOrType, listPIPidls[0].DisplayPages[0].HintId, "show PI pidl sequenceId is expected HintId");
        }

        [DataRow("commercialstores", "virtual", "invoice_basic")]
        [DataRow("commercialstores", "virtual", "invoice_check")]
        [DataTestMethod]
        public void GetPaymentMethodDescriptions_InvoiceAddressValidation(string partner, string family, string type)
        {
            const string Operation = "add";
            const string Country = "us";
            const string Language = "en-US";
            PaymentMethod pi = new PaymentMethod() { PaymentMethodId = "1a2b3c4d", PaymentMethodFamily = family, PaymentMethodType = type };
            HashSet<PaymentMethod> testPIs = new HashSet<PaymentMethod>();
            testPIs.Add(pi);

            List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetPaymentMethodDescriptions(testPIs, Country, family, type, Operation, Language, partner);
            Assert.AreEqual(1, pidls.Count, "inovice PI returns as expected");
        }

        [DataRow("oxowebdirect", "fr", "fr-FR", "visa%2Cmc", false, false, false)]
        [DataRow("oxowebdirect", "fr", "fr-FR", "visa%2Cmc", false, true, false)]
        [DataRow("webblends", "fr", "fr-FR", "visa%2Cmc", true, false, true)]
        [DataRow("webblends", "fr", "fr-FR", "visa%2Cmc", true, true, false)]
        [DataRow("webblends", "us", "en-US", "visa%2Cmc", true, false, false)]
        [DataRow("webblends", "us", "en-US", "visa%2Cmc", true, true, false)]
        [DataRow("amcxbox", "fi", "fi-FI", "visa%2Camex%2Cmc", false, false, false)]
        [DataRow("amcxbox", "fi", "fi-FI", "visa%2Camex%2Cmc", false, true, false)]
        [DataRow("xbox", "pt", "nl-PT", "amex%2Cmc", true, false, true)]
        [DataRow("xbox", "pt", "nl-PT", "amex%2Cmc", true, true, false)]
        [DataRow("xbox", "us", "en-US", "amex%2Cmc", true, false, false)]
        [DataRow("xbox", "us", "en-US", "amex%2Cmc", true, true, false)]
        [DataRow("azure", "it", "it-IT", "visa%2Camex", false, false, false)]
        [DataRow("azure", "it", "it-IT", "visa%2Camex", false, true, false)]
        [DataRow("azure", "us", "en-US", "visa%2Camex", false, true, false)]
        [DataRow("azuresignup", "us", "en-US", "visa%2Camex", false, true, false)]
        [DataRow("azureibiza", "us", "en-US", "visa%2Camex", false, true, false)]
        [DataTestMethod]
        public void AddPaymentMethod_ValidateSingleMarketDirective(string partner, string country, string language, string type, bool smdExpected, bool fixCountrySelection, bool countryEnabledExpected)
        {
            string operation = "add";
            string family = "credit_card";

            var flightFeatures = new List<string>();

            string scenario = null;
            if (fixCountrySelection)
            {
                scenario = "fixedCountrySelection";
            }

            HashSet<PaymentMethod> paymentMethods = new HashSet<PaymentMethod>()
            {
                new PaymentMethod() { PaymentMethodId = "credit_card.amex", PaymentMethodFamily = family, PaymentMethodType = TestConstants.PaymentMethodTypeNames.Amex },
                new PaymentMethod() { PaymentMethodId = "credit_card.mc", PaymentMethodFamily = family, PaymentMethodType = TestConstants.PaymentMethodTypeNames.Mc },
                new PaymentMethod() { PaymentMethodId = "credit_card.visa", PaymentMethodFamily = family, PaymentMethodType = TestConstants.PaymentMethodTypeNames.Visa }
            };

            // Making same changes to PIDL that PaymentMethodDescriptionsController does for EU SMD markets
            if ((string.Equals(partner, "xbox", StringComparison.OrdinalIgnoreCase)
                || string.Equals(partner, "amcxbox", StringComparison.OrdinalIgnoreCase))
                && smdExpected)
            {
                var singleMarkets = new List<string>(PIDLResourceFactory.GetCopiedDictionaryFromDomainDictionaries("MarketsEUSMD").Keys);
                if (!singleMarkets.Contains(country.ToLower()))
                {
                    scenario = "fixedCountrySelection";
                }
            }

            List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetPaymentMethodDescriptions(paymentMethods, country, family, type, operation, language, partner, exposedFlightFeatures: flightFeatures, scenario: scenario);

            foreach (PIDLResource pidl in pidls)
            {
                // Making same changes to PIDL that PaymentMethodDescriptionsController does for EU SMD markets
                if (smdExpected && !string.Equals(scenario, "fixedCountrySelection", StringComparison.OrdinalIgnoreCase))
                {
                    var singleMarkets = new List<string>(PIDLResourceFactory.GetCopiedDictionaryFromDomainDictionaries("MarketsEUSMD").Keys);
                    if (singleMarkets.Contains(country.ToLower()))
                    {
                        pidl.SetPropertyState("country", true);
                        pidl.UpdateDisplayHintPossibleOptions("country", singleMarkets);
                    }
                }

                Assert.AreEqual(operation, pidl.Identity["operation"]);
                Assert.AreEqual(country.ToLower(), pidl.Identity["country"]);

                PropertyDisplayHint addressCountry = pidl.GetDisplayHintById("addressCountry") as PropertyDisplayHint;

                if (countryEnabledExpected)
                {
                    Assert.AreEqual(32, addressCountry.PossibleOptions.Count, "Only the 32 countries in SMD are expected");
                    Assert.AreEqual(32, addressCountry.PossibleValues.Count, "Only the 32 countries in SMD are expected");
                    Assert.AreEqual(false, addressCountry.IsDisabled, "Country dropdown needs to be enabled");
                    Assert.IsTrue(addressCountry.IsHidden == null || addressCountry.IsHidden == false, "Country dropdown should not be hidden");
                }
                else
                {
                    Assert.AreEqual(243, addressCountry.PossibleOptions.Count, "All countries are expected");
                    Assert.AreEqual(243, addressCountry.PossibleValues.Count, "All countries are expected");
                    Assert.IsTrue((addressCountry.IsDisabled == null && addressCountry.IsHidden == true) || addressCountry.IsDisabled == true, "Country dropdown should not be enabled");
                }

                if (string.Equals(partner, "amcxbox", StringComparison.OrdinalIgnoreCase))
                {
                    int expectedDisplayPageCount = countryEnabledExpected ? 10 : 9;
                    if (string.Equals(country, "us"))
                    {
                        expectedDisplayPageCount = countryEnabledExpected ? 11 : 10;
                    }

                    Assert.AreEqual(expectedDisplayPageCount, pidl.DisplayPages.Count);
                    Assert.AreEqual(countryEnabledExpected ? "country" : "address_line1", pidl.DisplayPages[4].Members[1].PropertyName, "Expected a new DisplayPage for country when flight is enabled");
                }

                if (string.Equals(partner, "xbox", StringComparison.OrdinalIgnoreCase))
                {
                    int expectedDisplayPageCount = countryEnabledExpected ? 9 : 8;
                    if (string.Equals(country, "us"))
                    {
                        expectedDisplayPageCount = countryEnabledExpected ? 10 : 9;
                    }

                    Assert.AreEqual(expectedDisplayPageCount, pidl.DisplayPages.Count);
                    Assert.AreEqual(countryEnabledExpected ? "country" : "address_line1", pidl.DisplayPages[4].Members[1].PropertyName, "Expected a new DisplayPage for country when flight is enabled");
                }
            }
        }

        [DataRow("ewallet", "bitcoin", "us", "en-us", "amcweb", "F2D44338-A605-4A7E-AA50-18B0B2B1E967")]
        [DataTestMethod]
        public void GetPaymentMethodDescriptions_FundStoredValueSelectStage(string family, string type, string country, string language, string partnerName, string piid)
        {
            var fundingOptions = new Dictionary<string, string>()
            {
                { "5", "$5" },
                { "10", "$10" },
                { "15", "$15" },
                { "20", "$20" },
                { "25", "$25" },
                { "50", "$50" },
                { "75", "$75" },
                { "100", "$100" },
            };
            List<PIDLResource> pidls = PIDLResourceFactory.GetPaymentMethodFundStoredValueSelectDescriptions(family, type, country, language, partnerName, piid, fundingOptions);
            Assert.AreEqual(1, pidls.Count, "fundStoredValue select amount PIDL returns as expected");
        }

        [DataRow("10", "ewallet", "bitcoin", "us", "USD", "en-us", "amcweb", "F2D44338-A605-4A7E-AA50-18B0B2B1E967", "https://bitpay.com/invoice?id=PujzFdsApS3EymrK5BzZbo&view=iframe&lang=en-us", "referenceId", "greenId")]
        [DataTestMethod]
        public void GetPaymentMethodDescriptions_FundStoredValueRedeemStage(string amount, string family, string type, string country, string currency, string language, string partnerName, string piid, string redirectContentUrl, string referenceId, string greenId)
        {
            List<PIDLResource> pidls = PIDLResourceFactory.GetPaymentMethodFundStoredValueRedeemDescriptions(amount, family, type, country, currency, language, partnerName, piid, redirectContentUrl, referenceId, greenId);
            Assert.AreEqual(1, pidls.Count, "fundStoredValue redeem amount PIDL returns as expected");
        }

        [DataRow("jp")]
        [DataRow("tw")]
        [DataRow("th")]
        [DataRow("ph")]
        [DataRow("vn")]
        [DataRow("id")]
        [DataTestMethod]
        public void PidlGetPaymentMethodJCBSpecific_Countries(string country)
        {
            string descriptionType = TestConstants.DescriptionTypes.PaymentMethodDescription;
            string family = TestConstants.PaymentMethodFamilyNames.CreditCard;
            string type = TestConstants.PaymentMethodTypeNames.Jcb;
            string operation = TestConstants.PidlOperationTypes.Add;
            string language = "en-us";

            PaymentMethod pi = new PaymentMethod() { PaymentMethodId = "credit_card.jcb", PaymentMethodFamily = family, PaymentMethodType = TestConstants.PaymentMethodTypeNames.Jcb };
            HashSet<PaymentMethod> testPIs = new HashSet<PaymentMethod>();
            testPIs.Add(pi);
            List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetPaymentMethodDescriptions(testPIs, country, family, TestConstants.PaymentMethodTypeNames.Jcb, operation, language);

            Assert.AreEqual(1, pidls.Count);
            Assert.IsTrue(pidls.SingleOrDefault().Identity[TestConstants.DescriptionIdentityFields.DescriptionType].Equals(descriptionType));
            Assert.IsTrue(pidls.SingleOrDefault().Identity[TestConstants.DescriptionIdentityFields.Family].Equals(family));
            Assert.IsTrue(pidls.SingleOrDefault().Identity[TestConstants.DescriptionIdentityFields.Type].Equals(type));
            Assert.IsTrue(pidls.SingleOrDefault().Identity[TestConstants.DescriptionIdentityFields.Operation].Equals(operation));
        }

        [DataRow("amcweb")]
        [DataRow("amcxbox")]
        [DataRow("cart")]
        [DataRow("consumersupport")]
        [DataRow("default")]
        [DataRow("northstarweb")]
        [DataRow("oxodime")]
        [DataRow("oxowebdirect")]
        [DataRow("storify")]
        [DataRow("xboxsubs")]
        [DataRow("xboxsettings")]
        [DataRow("saturn")]
        [DataRow("webblends")]
        [DataRow("webblends_inline")]
        [DataRow("xbox")]
        [DataRow("xboxweb")]
        [DataTestMethod]
        public void PidlGetPaymentMethodHipercardAndElo(string partner)
        {
            string country = "br";
            string descriptionType = TestConstants.DescriptionTypes.PaymentMethodDescription;
            string family = TestConstants.PaymentMethodFamilyNames.CreditCard;
            string type = TestConstants.PaymentMethodTypeNames.Elo;
            string language = "en-us";

            HashSet<PaymentMethod> testPIs = new string[]
            {
                TestConstants.PaymentMethodTypeNames.Visa,
                TestConstants.PaymentMethodTypeNames.Amex,
                TestConstants.PaymentMethodTypeNames.Mc,
                TestConstants.PaymentMethodTypeNames.Hipercard,
                TestConstants.PaymentMethodTypeNames.Elo
            }.Select(t => new PaymentMethod() { PaymentMethodId = "credit_card." + t, PaymentMethodFamily = family, PaymentMethodType = t }).ToHashSet();

            foreach (string operation in new string[] { TestConstants.PidlOperationTypes.Add, TestConstants.PidlOperationTypes.Update })
            {
                List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetPaymentMethodDescriptions(testPIs, country, family, type, operation, language, partner);
                Assert.AreEqual(testPIs.Count, pidls.Count);

                // Difference in DisplayPages means the PI is not correctly declared
                pidls.ForEach(p => Assert.AreEqual(pidls[0].DisplayPages.Count, p.DisplayPages.Count, "DisplayPages count is different for " + p.Identity[TestConstants.DescriptionIdentityFields.Type]));

                // Test the pidl for required PIs
                var pidlHipercard = pidls.SingleOrDefault(p => p.Identity[TestConstants.DescriptionIdentityFields.Type].Equals(TestConstants.PaymentMethodTypeNames.Hipercard));
                Assert.IsNotNull(pidlHipercard);
                Assert.IsTrue(pidlHipercard.Identity[TestConstants.DescriptionIdentityFields.DescriptionType].Equals(descriptionType));
                Assert.IsTrue(pidlHipercard.Identity[TestConstants.DescriptionIdentityFields.Family].Equals(family));
                Assert.IsTrue(pidlHipercard.Identity[TestConstants.DescriptionIdentityFields.Type].Equals(TestConstants.PaymentMethodTypeNames.Hipercard));
                Assert.IsTrue(pidlHipercard.Identity[TestConstants.DescriptionIdentityFields.Operation].Equals(operation));

                var pidlElo = pidls.SingleOrDefault(p => p.Identity[TestConstants.DescriptionIdentityFields.Type].Equals(type));
                Assert.IsNotNull(pidlElo);
                Assert.IsTrue(pidlElo.Identity[TestConstants.DescriptionIdentityFields.DescriptionType].Equals(descriptionType));
                Assert.IsTrue(pidlElo.Identity[TestConstants.DescriptionIdentityFields.Family].Equals(family));
                Assert.IsTrue(pidlElo.Identity[TestConstants.DescriptionIdentityFields.Type].Equals(type));
                Assert.IsTrue(pidlElo.Identity[TestConstants.DescriptionIdentityFields.Operation].Equals(operation));

                // Specific to Add operation
                if (operation == TestConstants.PidlOperationTypes.Add)
                {
                    EnsureCvvHelpTextAligned(pidls, hintId: "creditCardsCVV3Text", expectedCvvHelpText: "VISA, MasterCard, Hipercard, Elo:");
                    EnsureButtonGroupsAligned(pidls);
                }

                // For both Add and Update operations
                EnsureSummaryPageAligned(pidls);
            }
        }

        [DataRow("xbox")]
        [DataRow("storify")]
        [DataRow("saturn")]
        [DataRow("xboxsubs")]
        [DataRow("xboxsettings")]
        [DataTestMethod]
        public void PidlFactoryGetKakaopayEwalletPidlsForAddOperation(string partner)
        {
            const string Country = "kr";
            const string Family = TestConstants.PaymentMethodFamilyNames.Ewallet;
            const string Type = TestConstants.PaymentMethodTypeNames.Kakaopay;
            string[] supportedOperations = new string[] { TestConstants.PidlOperationTypes.Add };
            const string Language = "en-us";
            PaymentMethod pi = new PaymentMethod() { PaymentMethodId = "1a2b3c4d", PaymentMethodFamily = Family, PaymentMethodType = Type };
            HashSet<PaymentMethod> testPIs = new HashSet<PaymentMethod>();
            testPIs.Add(pi);

            foreach (string operation in supportedOperations)
            {
                List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetPaymentMethodDescriptions(testPIs, Country, Family, Type, operation, Language, partner);

                foreach (PIDLResource pidl in pidls)
                {
                    Assert.AreEqual(pidl.Identity[TestConstants.DescriptionIdentityFields.Country], "kr");
                    Assert.AreEqual(pidl.Identity[TestConstants.DescriptionIdentityFields.Family], TestConstants.PaymentMethodFamilyNames.Ewallet);
                    Assert.AreEqual(pidl.Identity[TestConstants.DescriptionIdentityFields.Type], TestConstants.PaymentMethodTypeNames.Kakaopay);
                    Assert.IsTrue(pidl.Identity[TestConstants.DescriptionIdentityFields.ResourceIdentity].Equals(TestConstants.PaymentMethodFamilyNames.Ewallet + "." + TestConstants.PaymentMethodTypeNames.Kakaopay));
                }
            }
        }

        [DataRow("xbox")]
        [DataRow("storify")]
        [DataRow("saturn")]
        [DataRow("xboxsubs")]
        [DataRow("xboxsettings")]
        [DataTestMethod]
        public void PidlFactoryGetVenmoEwalletPidlsForAddOperation(string partner)
        {
            const string Country = "us";
            const string Family = TestConstants.PaymentMethodFamilyNames.Ewallet;
            const string Type = TestConstants.PaymentMethodTypeNames.Venmo;
            string[] supportedOperations = new string[] { TestConstants.PidlOperationTypes.Add };
            const string Language = "en-us";
            PaymentMethod pi = new PaymentMethod() { PaymentMethodId = "1a2b3c4d", PaymentMethodFamily = Family, PaymentMethodType = Type };
            HashSet<PaymentMethod> testPIs = new HashSet<PaymentMethod>();
            testPIs.Add(pi);

            List<string> flights = new List<string> { "PxEnableVenmo" };

            foreach (string operation in supportedOperations)
            {
                List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetPaymentMethodDescriptions(testPIs, Country, Family, Type, operation, Language, partner, exposedFlightFeatures: flights);

                foreach (PIDLResource pidl in pidls)
                {
                    Assert.AreEqual(pidl.Identity[TestConstants.DescriptionIdentityFields.Country], "us");
                    Assert.AreEqual(pidl.Identity[TestConstants.DescriptionIdentityFields.Family], TestConstants.PaymentMethodFamilyNames.Ewallet);
                    Assert.AreEqual(pidl.Identity[TestConstants.DescriptionIdentityFields.Type], TestConstants.PaymentMethodTypeNames.Venmo);
                    Assert.IsTrue(pidl.Identity[TestConstants.DescriptionIdentityFields.ResourceIdentity].Equals(TestConstants.PaymentMethodFamilyNames.Ewallet + "." + TestConstants.PaymentMethodTypeNames.Venmo));
                }
            }
        }

        [DataRow("amcweb")]
        [DataRow("amcxbox")]
        [DataRow("cart")]
        [DataRow("consumersupport")]
        [DataRow("default")]
        [DataRow("northstarweb")]
        [DataRow("oxodime")]
        [DataRow("oxowebdirect")]
        [DataRow("storify")]
        [DataRow("xboxsubs")]
        [DataRow("xboxsettings")]
        [DataRow("saturn")]
        [DataRow("webblends")]
        [DataRow("webblends_inline")]
        [DataRow("xbox")]
        [DataRow("xboxweb")]
        [DataTestMethod]
        public void PidlGetPaymentMethodVerve(string partner)
        {
            string country = "ng";
            string descriptionType = TestConstants.DescriptionTypes.PaymentMethodDescription;
            string family = TestConstants.PaymentMethodFamilyNames.CreditCard;
            string type = TestConstants.PaymentMethodTypeNames.Verve;
            string language = "en-us";

            HashSet<PaymentMethod> testPIs = new string[]
            {
                TestConstants.PaymentMethodTypeNames.Visa,
                TestConstants.PaymentMethodTypeNames.Amex,
                TestConstants.PaymentMethodTypeNames.Mc,
                TestConstants.PaymentMethodTypeNames.Verve
            }.Select(t => new PaymentMethod() { PaymentMethodId = "credit_card." + t, PaymentMethodFamily = family, PaymentMethodType = t }).ToHashSet();

            foreach (string operation in new string[] { TestConstants.PidlOperationTypes.Add, TestConstants.PidlOperationTypes.Update })
            {
                List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetPaymentMethodDescriptions(testPIs, country, family, type, operation, language, partner);
                Assert.AreEqual(testPIs.Count, pidls.Count);

                // Difference in DisplayPages means the PI is not correctly declared
                pidls.ForEach(p => Assert.AreEqual(pidls[0].DisplayPages.Count, p.DisplayPages.Count, "DisplayPages count is different for " + p.Identity[TestConstants.DescriptionIdentityFields.Type]));

                // Test the pidl for required PIs
                var pidlVerve = pidls.SingleOrDefault(p => p.Identity[TestConstants.DescriptionIdentityFields.Type].Equals(type));
                Assert.IsNotNull(pidlVerve);
                Assert.IsTrue(pidlVerve.Identity[TestConstants.DescriptionIdentityFields.DescriptionType].Equals(descriptionType));
                Assert.IsTrue(pidlVerve.Identity[TestConstants.DescriptionIdentityFields.Family].Equals(family));
                Assert.IsTrue(pidlVerve.Identity[TestConstants.DescriptionIdentityFields.Type].Equals(type));
                Assert.IsTrue(pidlVerve.Identity[TestConstants.DescriptionIdentityFields.Operation].Equals(operation));

                // Specific to Add operation
                if (operation == TestConstants.PidlOperationTypes.Add)
                {
                    EnsureCvvHelpTextAligned(pidls, hintId: "creditCardsCVV3Text", expectedCvvHelpText: "VISA, MasterCard, Verve:");
                    EnsureButtonGroupsAligned(pidls);
                }

                // For both Add and Update operations
                EnsureSummaryPageAligned(pidls);
            }
        }

        private void EnsureCvvHelpTextAligned(List<PIDLResource> pidls, string hintId, string expectedCvvHelpText)
        {
            if (pidls.Count == 0)
            {
                Assert.Inconclusive();
            }

            foreach (var p in pidls)
            {
                // CVV help text
                var cvvHelp = p.GetDisplayHintById(hintId, includeHelpDisplayDescriptions: true) as ContentDisplayHint;
                Assert.IsNotNull(cvvHelp, "Cannot find CVV help for " + p.Identity[TestConstants.DescriptionIdentityFields.Type]);
                Assert.AreEqual(expectedCvvHelpText, cvvHelp.DisplayContent);
            }
        }

        private void EnsureButtonGroupsAligned(List<PIDLResource> pidls)
        {
            if (pidls.Count == 0)
            {
                Assert.Inconclusive();
            }

            string firstButtonGroupId = null;
            foreach (var p in pidls)
            {
                // Button groups should be aligned
                var cancelAddGroup = p.GetDisplayHintById("cancelAddGroup", includeHelpDisplayDescriptions: true);
                var cancelSaveGroup = p.GetDisplayHintById("cancelSaveGroup", includeHelpDisplayDescriptions: true);
                var cancelNextGroup = p.GetDisplayHintById("cancelNextGroup", includeHelpDisplayDescriptions: true);

                string currentButtonGroupId = cancelAddGroup?.HintId ?? cancelSaveGroup?.HintId ?? cancelNextGroup?.HintId;
                if (firstButtonGroupId == null)
                {
                    firstButtonGroupId = currentButtonGroupId;
                }
                else
                {
                    Assert.AreEqual(firstButtonGroupId, currentButtonGroupId, "Button group is different for " + p.Identity[TestConstants.DescriptionIdentityFields.Type]);
                }
            }
        }

        private void EnsureSummaryPageAligned(List<PIDLResource> pidls)
        {
            if (pidls.Count == 0)
            {
                Assert.Inconclusive();
            }

            bool? summaryPagePresent = null;
            foreach (var p in pidls)
            {
                // Summary page presence should be aligned
                var summaryPage = p.GetDisplayHintById("creditCardSummaryPage", includeHelpDisplayDescriptions: true) as PageDisplayHint;
                if (summaryPagePresent.HasValue)
                {
                    Assert.AreEqual(summaryPagePresent.Value, summaryPage != null, "Summary page presence is different for " + p.Identity[TestConstants.DescriptionIdentityFields.Type]);
                }
                else
                {
                    summaryPagePresent = summaryPage != null;
                }
            }
        }

        [DataRow("ie")]
        [DataTestMethod]
        public void PidlGetPaymentMethodSEPA_Ireland(string country)
        {
            string family = TestConstants.PaymentMethodFamilyNames.DirectDebit;
            string type = TestConstants.PaymentMethodTypeNames.Sepa;
            string language = "en-us";

            PaymentMethod pi = new PaymentMethod()
            {
                PaymentMethodId = "direct_debit.sepa.ireland",
                PaymentMethodFamily = family,
                PaymentMethodType = type
            };
            HashSet<PaymentMethod> testPIs = new HashSet<PaymentMethod>();
            testPIs.Add(pi);
            List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetPaymentMethodDescriptions(testPIs, country, family, type, TestConstants.PidlOperationTypes.Update, language);
            Assert.AreEqual(1, pidls.Count);
        }

        [DataRow("amcweb")]
        [DataRow("azure")]
        [DataRow("cart")]
        [DataRow("commercialsupport")]
        [DataRow("northstarweb")]
        [DataRow("office")]
        [DataRow("webblends")]
        [DataRow("oxowebdirect")]
        [DataTestMethod]
        public void PidlFactoryXKCreditCard(string partner)
        {
            string country = "xk";
            string language = "en-us";
            string family = "credit_card";
            string type = "visa";

            HashSet<string> operations = new HashSet<string>() { "add", "update" };
            HashSet<PaymentMethod> paymentMethods = new HashSet<PaymentMethod>()
            {
                new PaymentMethod() { PaymentMethodId = "credit_card.amex", PaymentMethodFamily = family, PaymentMethodType = TestConstants.PaymentMethodTypeNames.Amex },
                new PaymentMethod() { PaymentMethodId = "credit_card.mc", PaymentMethodFamily = family, PaymentMethodType = TestConstants.PaymentMethodTypeNames.Mc },
                new PaymentMethod() { PaymentMethodId = "credit_card.visa", PaymentMethodFamily = family, PaymentMethodType = TestConstants.PaymentMethodTypeNames.Visa }
            };
            foreach (string operation in operations)
            {
                List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetPaymentMethodDescriptions(paymentMethods, country, family, type, operation, language, partner);
                Assert.IsTrue(pidls.Count == 3);
                PidlAssert.IsValid(pidls);
            }
        }

        [DataRow("us", "credit_card", new[] { "visa", "amex", "mc" }, "add", "en-us", "azure", "saveNextButton")]
        [DataRow("us", "credit_card", new[] { "visa", "amex", "mc" }, "add", "en-us", "azuresignup", "saveNextButton")]
        [DataRow("us", "credit_card", new[] { "visa", "amex", "mc" }, "add", "en-us", "azureibiza", "saveNextButton")]
        [DataRow("us", "credit_card", new[] { "visa", "amex", "mc" }, "add", "en-us", "smboobe", "saveButton")]
        [DataRow("de", "direct_debit", new[] { "sepa" }, "add", "en-us", "webblends", "saveNextButton")]
        [DataRow("de", "direct_debit", new[] { "sepa" }, "add", "en-us", "defaulttemplate", "saveNextButton")]
        [DataRow("de", "direct_debit", new[] { "sepa" }, "add", "en-us", "setupoffice", "saveNextButton")]
        [DataRow("de", "direct_debit", new[] { "sepa" }, "add", "en-us", "setupofficesdx", "saveContinueButton")]
        [DataTestMethod]
        public void GetPaymentMethodDescriptions_Add(string country, string family, string[] types, string operation, string language, string partnerName, string submitButtonId)
        {
            // Arrange
            var paymentMethods = types.Select(type => new PaymentMethod()
            {
                PaymentMethodId = string.Format("{0}.{1}", family, type),
                PaymentMethodFamily = family,
                PaymentMethodType = type
            }).ToHashSet();

            // Act
            List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetPaymentMethodDescriptions(paymentMethods, country, family, string.Join("%2", types), operation, language, partnerName, null, null, null, null, false, null, null);

            // Assert
            Assert.IsNotNull(pidls, "Pidl is expected to be not null");
            Assert.AreEqual(types.Length, pidls.Count, "The number of Pidls should be equal to the number of payment method types");
            var submitButton = pidls[0].GetDisplayHintById(submitButtonId) as ButtonDisplayHint;
            Assert.IsNotNull(submitButton, "Submit button is expected");
            var context = JsonConvert.DeserializeObject<RestLink>(JsonConvert.SerializeObject(submitButton.Action.Context));
            Assert.IsTrue(string.Equals(string.Format("https://{{pifd-endpoint}}/users/{{userId}}/paymentInstrumentsEx?country={0}&language={1}&partner={2}", country, language, partnerName), context.Href, StringComparison.OrdinalIgnoreCase));
        }

        [DataRow("us", "credit_card", new[] { "visa", "amex", "mc" }, "add", "en-us", "smboobe")]
        [DataTestMethod]
        public void GetPaymentMethodDescriptions_AddCreditCardIsSplitIntoTwoPages(string country, string family, string[] types, string operation, string language, string partnerName)
        {
            var paymentMethods = types.Select(type => new PaymentMethod()
            {
                PaymentMethodId = string.Format("{0}.{1}", family, type),
                PaymentMethodFamily = family,
                PaymentMethodType = type
            }).ToHashSet();

            List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetPaymentMethodDescriptions(paymentMethods, country, family, string.Join("%2", types), operation, language, partnerName, null, null, null, null, false, null, null);

            Assert.IsNotNull(pidls, "Pidl is expected to be not null");
            Assert.AreEqual(types.Length, pidls.Count, "The number of Pidls should be equal to the number of payment method types");

            foreach (var pidl in pidls)
            {
                Assert.AreEqual(2, pidl.DisplayPages.Count);

                // Verify contents of first page.
                Assert.AreEqual("AccountDetailsPage", pidl.DisplayPages.First().DisplayName);
                Assert.AreEqual("logo", pidl.DisplayPages.First().Members.First().DisplayHintType);
                Assert.AreEqual("privacyNextGroup", pidl.DisplayPages.First().Members.Last().HintId);

                var moveNextButton = pidl.GetDisplayHintById("nextButton") as ButtonDisplayHint;
                Assert.AreEqual("moveNext", moveNextButton.Action.ActionType);

                // Verify contents of last page.
                Assert.AreEqual("CreditCardAddressPage", pidl.DisplayPages.Last().DisplayName);
                Assert.AreEqual("property", pidl.DisplayPages.Last().Members.First().DisplayHintType);
                Assert.AreEqual("privacySaveGroup", pidl.DisplayPages.Last().Members.Last().HintId);

                var submitButton = pidl.GetDisplayHintById("saveButton") as ButtonDisplayHint;
                Assert.AreEqual("submit", submitButton.Action.ActionType);
            }
        }

        [DataRow("us", "credit_card", new[] { "visa", "amex", "mc" }, "add", "en-us", "smboobe", null, "false", "country,accountHolderName,accountToken,first_name,last_name,address_line1,address_line2,address_line3,city,region,postal_code")]
        [DataRow("us", "credit_card", new[] { "visa", "amex", "mc" }, "add", "en-us", "smboobe", "roobe", "true", "")]
        [DataTestMethod]
        public void GetPaymentMethodDescriptions_ShouldShowDisplayNameAsExpected(string country, string family, string[] types, string operation, string language, string partnerName, string scenario, string showDisplayName, string propertiesToIgnore)
        {
            var paymentMethods = types.Select(type => new PaymentMethod()
            {
                PaymentMethodId = string.Format("{0}.{1}", family, type),
                PaymentMethodFamily = family,
                PaymentMethodType = type
            }).ToHashSet();

            List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetPaymentMethodDescriptions(paymentMethods, country, family, string.Join("%2", types), operation, language, partnerName, null, null, null, null, false, null, scenario);

            Assert.IsNotNull(pidls, "Pidl is expected to be not null");
            Assert.AreEqual(types.Length, pidls.Count, "The number of Pidls should be equal to the number of payment method types");

            List<string> excludedProperties = propertiesToIgnore.Split(',').ToList();

            // Select all property displayhints that are not nested members of pages.
            List<DisplayHint> properties = pidls
                .SelectMany(pidl => pidl.DisplayPages)
                .SelectMany(displayPages => displayPages.Members)
                .Where(displayHint => displayHint.DisplayHintType.Equals("property") && !excludedProperties.Contains(displayHint.PropertyName))
                .ToList();

            // Verify all valid properties have the expected value for showDisplayName
            foreach (DisplayHint displayHint in properties)
            {
                Assert.AreEqual(showDisplayName, ((PropertyDisplayHint)displayHint).ShowDisplayName);
            }
        }

        [DataRow("paypal")]
        [DataRow("venmo")]
        [DataTestMethod]
        public void PidlFactoryVenmoAndPaypalPaymentChangeSettingsTextInSeparateGroupsForCartPartner(string piType)
        {
            const string Partner = PartnerNames.Cart;
            const string Country = "us";
            const string Family = PaymentMethodFamilyNames.Ewallet;
            const string Language = "en-us";

            PaymentMethod pi = new PaymentMethod() { PaymentMethodId = "1a2b3c4d", PaymentMethodFamily = Family, PaymentMethodType = piType };

            HashSet<PaymentMethod> testPIs = new HashSet<PaymentMethod>
            {
                pi
            };

            List<string> flights = new List<string> { "PxEnableVenmo" };

            List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetPaymentMethodDescriptions(testPIs, Country, Family, piType, PidlOperationTypes.Add, Language, Partner, exposedFlightFeatures: flights);
            Assert.IsNotNull(pidls);

            PIDLResource pidl = pidls.First();
            Assert.IsNotNull(pidl);
            Assert.AreEqual(1, pidl.DisplayPages.Count);

            TextGroupDisplayHint changeSettingsTextGroup = pidl.DisplayPages.First().Members[2] as TextGroupDisplayHint;
            TextDisplayHint paymentSummaryText = pidl.GetDisplayHintById("paymentSummaryText") as TextDisplayHint;
            Assert.IsNotNull(changeSettingsTextGroup);
            Assert.IsNotNull(paymentSummaryText);

            TextGroupDisplayHint changeSettingsTextGroup2 = pidl.DisplayPages.First().Members[3] as TextGroupDisplayHint;
            TextDisplayHint paymentChangeSettingsStaticText = pidl.GetDisplayHintById("paymentChangeSettingsStaticText") as TextDisplayHint;
            HyperlinkDisplayHint accountManagement = pidl.GetDisplayHintById("accountManagement") as HyperlinkDisplayHint;
            TextDisplayHint changeSettingsStaticPeriodText = pidl.GetDisplayHintById($"{piType}ChangeSettingsStaticPeriodText") as TextDisplayHint;
            Assert.IsNotNull(changeSettingsTextGroup2);
            Assert.IsNotNull(paymentChangeSettingsStaticText);
            Assert.IsNotNull(accountManagement);
            Assert.IsNotNull(changeSettingsStaticPeriodText);
        }
    }

    enum GetPaymentMethodDescriptionTestColumns
    {
        Country, Family, Type, Operation, Language, ExpectedExceptionType
    }
}
