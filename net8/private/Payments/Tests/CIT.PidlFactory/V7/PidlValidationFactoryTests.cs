// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace CIT.PidlFactory.V7
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;
    using System.Text.RegularExpressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;

    [TestClass]
    public class PidlValidationFactoryTests
    {
        private TestContext testContext;

        public TestContext TestContext
        {
            get { return testContext; }
            set { testContext = value; }
        }
        
        [TestMethod]
        public void PidlValidationFactoryCardHolderNamePositiveTests()
        {
            string propertyName = "accountHolderName";
            string country = "us";
            string family = "credit_card";
            string id = "credit_card.visa";
            string type = "visa";
            string operation = "add";
            string language = "en-us";
            string partner = "cart";
            var paymentMethods = new HashSet<PaymentMethod>()
             {
                new PaymentMethod()
                {
                    PaymentMethodId = id,
                    PaymentMethodFamily = family,
                    PaymentMethodType = type
                }
             };
            var pidl = PIDLResourceFactory.Instance.GetPaymentMethodDescriptions(paymentMethods, country, family, type, operation, language, partner).FirstOrDefault();
            var ahn = pidl.GetPropertyDescriptionByPropertyName(propertyName);
            Regex rgx = new Regex(ahn.Validation.Regex);

            using (StreamReader sw = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestData\CardHolderNameValidationPositiveTestCases.csv")))
            {
                // Skip headers
                sw.ReadLine();
                string line;
                bool testPass = true;
                while ((line = sw.ReadLine()) != null)
                {
                    Match match = rgx.Match(line);

                    testContext.WriteLine("{0}, {1}", line, match.Success ? "Passed" : "Failed");
                    testPass = testPass && match.Success;
                }

                Assert.IsTrue(testPass, "One or more valid cardholder names failed validation");
            }
        }

        [TestMethod]
        public void PidlValidationFactoryCardHolderNameNegativeTests()
        {
            string propertyName = "accountHolderName";
            string country = "us";
            string family = "credit_card";
            string id = "credit_card.visa";
            string type = "visa";
            string operation = "add";
            string language = "en-us";
            string partner = "cart";
            var paymentMethods = new HashSet<PaymentMethod>()
             {
                new PaymentMethod()
                {
                    PaymentMethodId = id,
                    PaymentMethodFamily = family,
                    PaymentMethodType = type
                }
             };
            var pidl = PIDLResourceFactory.Instance.GetPaymentMethodDescriptions(paymentMethods, country, family, type, operation, language, partner).FirstOrDefault();
            var ahn = pidl.GetPropertyDescriptionByPropertyName(propertyName);
            Regex rgx = new Regex(ahn.Validation.Regex);

            using (StreamReader sw = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestData\CardHolderNameValidationNegativeTestCases.csv")))
            {
                // Skip headers
                sw.ReadLine();
                string line;
                bool testPass = true;
                while ((line = sw.ReadLine()) != null)
                {
                    string[] rowValues = line.Split(',');
                    Match match = rgx.Match(rowValues[0]);
                    
                    testContext.WriteLine("{0}, {1}", rowValues[0], match.Success ? "Passed" : "Failed");
                    testPass = testPass && !match.Success;
                }

                Assert.IsTrue(testPass, "One or more invalid cardholder names passed validation");
            }
        }

        [DataRow("credit_card.mc", "mc", @"TestData\MCCardNumberValidationPositiveTestCases.csv", true)]
        [DataRow("credit_card.mc", "mc", @"TestData\MCCardNumberValidationNegativeTestCases.csv", false)]
        [TestMethod]
        public void PidlValidationFactoryAccountTokenTests(string resourceId, string identityType, string filePath, bool expected)
        {
            string propertyName = "accountToken";   
            string country = "us";
            string family = "credit_card";
            string id = resourceId;
            string type = identityType;
            string operation = "add";
            string language = "en-us";
            string partner = "cart";
            var paymentMethods = new HashSet<PaymentMethod>()
             {
                new PaymentMethod()
                {
                    PaymentMethodId = id,
                    PaymentMethodFamily = family,
                    PaymentMethodType = type
                }
             };
            var pidl = PIDLResourceFactory.Instance.GetPaymentMethodDescriptions(paymentMethods, country, family, type, operation, language, partner).FirstOrDefault();
            var accountToken = pidl.GetPropertyDescriptionByPropertyName(propertyName);
            Regex rgx = new Regex(accountToken.Validation.Regex);

            using (StreamReader sw = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath)))
            {
                // Skip headers
                sw.ReadLine();
                string line;
                while ((line = sw.ReadLine()) != null)
                {
                    string[] rowValues = line.Split(',');
                    Match match = rgx.Match(rowValues[0]);

                    testContext.WriteLine("{0}, {1}", line, match.Success ? "Passed" : "Failed");
                    bool actual = match.Success;
                    Assert.AreEqual(expected, actual);
                }
            }
        }

        [TestMethod]
        public void PidlValidationFactoryPhoneNumberPositiveTests()
        {
            using (StreamReader sw = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestData\PhoneNumberValidationPositiveTestCases.csv")))
            {
                // Skip headers
                string line = sw.ReadLine();

                bool testPass = true;
                while ((line = sw.ReadLine()) != null)
                {
                    string[] rowValues = line.Split(',');

                    PidlValidationParameter pidlValidationParameter = new PidlValidationParameter()
                    {
                        UrlValidationType = string.Format("phonenumber.{0}", rowValues[0]),
                        Value = rowValues[1]
                    };

                    var pidlExecutionResult = PidlPropertyValidationFactory.ValidateProperty(pidlValidationParameter, rowValues[0]);

                    testContext.WriteLine("{0}, {1}, {2}", rowValues[0], rowValues[1], pidlExecutionResult.Status);
                    testPass = testPass && (pidlExecutionResult.Status == PidlExecutionResultStatus.Passed);
                }

                Assert.IsTrue(testPass, "One or more valid phone numbers failed validation");
            }
        }

        [TestMethod]
        public void PidlValidationFactoryPhoneNumberNegativeTests()
        {
            using (StreamReader sw = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestData\PhoneNumberValidationNegativeTestCases.csv")))
            {
                // Skip headers
                string line = sw.ReadLine();

                bool testPass = true;
                while ((line = sw.ReadLine()) != null)
                {
                    string[] rowValues = line.Split(',');

                    var pidlValidationParameter = new PidlValidationParameter()
                    {
                        UrlValidationType = string.Format("phonenumber.{0}", rowValues[0]),
                        Value = rowValues[1],
                    };

                    var pidlExecutionResult = PidlPropertyValidationFactory.ValidateProperty(pidlValidationParameter, rowValues[0]);

                    testContext.WriteLine("{0}, {1}, {2}", rowValues[0], rowValues[1], pidlExecutionResult.Status);
                    testPass = testPass && (pidlExecutionResult.Status == PidlExecutionResultStatus.Failed);
                }

                Assert.IsTrue(testPass, "One or more invalid phone numbers passed validation");
            }
        }

        [TestMethod]
        public void PidlValidationFactoryPhoneNumberPositiveTestsOldContract()
        {
            PidlValidationParameter pidlValidationParameter = new PidlValidationParameter();

            PidlExecutionResult pidlExecutionResult = null;

            pidlValidationParameter.PropertyName = "msisdn";
            pidlValidationParameter.PidlIdentity = new Dictionary<string, string>();
            pidlValidationParameter.PidlIdentity[TestConstants.DescriptionIdentityFields.Country] = null;
            pidlValidationParameter.PidlIdentity[TestConstants.DescriptionIdentityFields.DescriptionType] = "data";
            pidlValidationParameter.PidlIdentity[TestConstants.DescriptionIdentityFields.Type] = "mobile_billing_non_sim_details";

            using (StreamReader sw = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestData\PhoneNumberValidationPositiveTestCases.csv")))
            {
                // Skip headers
                sw.ReadLine();
                string line;
                bool testPass = true;
                while ((line = sw.ReadLine()) != null)
                {
                    string[] rowValues = line.Split(',');

                    pidlValidationParameter.Value = rowValues[1];
                    pidlValidationParameter.PidlIdentity[TestConstants.DescriptionIdentityFields.Country] = rowValues[0];
                    pidlExecutionResult = PidlPropertyValidationFactory.ValidateProperty(pidlValidationParameter, rowValues[0]);

                    testContext.WriteLine("{0}, {1}, {2}", rowValues[0], rowValues[1], pidlExecutionResult.Status);
                    testPass = testPass && (pidlExecutionResult.Status == PidlExecutionResultStatus.Passed);
                }

                Assert.IsTrue(testPass, "One or more valid phone numbers failed validation");
            }
        }

        [TestMethod]
        public void PidlValidationFactoryPhoneNumberNegativeTestsOldContract()
        {
            PidlValidationParameter pidlValidationParameter = new PidlValidationParameter();

            PidlExecutionResult pidlExecutionResult = null;

            pidlValidationParameter.PropertyName = "msisdn";
            pidlValidationParameter.PidlIdentity = new Dictionary<string, string>();
            pidlValidationParameter.PidlIdentity[TestConstants.DescriptionIdentityFields.Country] = null;
            pidlValidationParameter.PidlIdentity[TestConstants.DescriptionIdentityFields.DescriptionType] = "data";
            pidlValidationParameter.PidlIdentity[TestConstants.DescriptionIdentityFields.Type] = "mobile_billing_non_sim_details";

            using (StreamReader sw = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestData\PhoneNumberValidationNegativeTestCases.csv")))
            {
                // Skip headers
                sw.ReadLine();
                string line;
                bool testPass = true;
                while ((line = sw.ReadLine()) != null)
                {
                    string[] rowValues = line.Split(',');

                    pidlValidationParameter.Value = rowValues[1];
                    pidlValidationParameter.PidlIdentity[TestConstants.DescriptionIdentityFields.Country] = rowValues[0];

                    pidlExecutionResult = PidlPropertyValidationFactory.ValidateProperty(pidlValidationParameter, rowValues[0]);

                    testContext.WriteLine("{0}, {1}, {2}", rowValues[0], rowValues[1], pidlExecutionResult.Status);
                    testPass = testPass && (pidlExecutionResult.Status == PidlExecutionResultStatus.Failed);
                }

                Assert.IsTrue(testPass, "One or more invalid phone numbers passed validation");
            }
        }

        [TestMethod]
        public void PidlValidationFactoryInvalidCountryCodeForPhoneNumber()
        {
            PidlValidationParameter pidlValidationParameter = new PidlValidationParameter();

            PidlExecutionResult pidlExecutionResult = null;

            pidlValidationParameter.PropertyName = "msisdn";
            pidlValidationParameter.Value = "1(206)555.1212";
            pidlValidationParameter.PidlIdentity = new Dictionary<string, string>();
            pidlValidationParameter.PidlIdentity[TestConstants.DescriptionIdentityFields.Country] = "qqqqqzzzzz";
            pidlValidationParameter.PidlIdentity[TestConstants.DescriptionIdentityFields.DescriptionType] = "data";
            pidlValidationParameter.PidlIdentity[TestConstants.DescriptionIdentityFields.Type] = "mobile_billing_non_sim_details";

            pidlExecutionResult = PidlPropertyValidationFactory.ValidateProperty(pidlValidationParameter, string.Empty);

            Assert.AreEqual(PidlExecutionResultStatus.Failed, pidlExecutionResult.Status, "qqqqqzzzzz should not be a valid country code");        
        }
    }
}
