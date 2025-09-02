// <copyright company="Microsoft">Copyright (c) Microsoft 2016. All rights reserved.</copyright>

namespace CIT.Payments.Common
{
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    [TestClass]
    public class JsonDataMaskerTests
    {
        [TestMethod]
        public void MaskAllNoLengthWorksAsExpected()
        {
            var testCases = new List<string>()
            {
                "'SomeData'",                       // A typical string
                "''",                               // An empty string
                "{ \"propertyA\" : 1 }",            // An object
                "[ \"elementA\", \"elementB\" ]",   // An array
                "null"                              // A null value
            };

            foreach (var testCase in testCases)
            {
                string actual = JsonDataMasker.DelegateMaskAllNoLength(JToken.Parse(testCase)).ToString();
                Assert.AreEqual("MASKED", actual);
            }
        }

        [TestMethod]
        public void MaskAllWorksAsExpected()
        {
            var testCases = new Dictionary<string, string>()
            {
                { "'SomeData'", "MASKED(8)" },                  // A typical string
                { "''", "MASKED(0)" },                          // An empty string
                { "{ \"propertyA\" : 1 }", "MASKED" },          // An object
                { "[ \"elementA\", \"elementB\" ]", "MASKED" }, // An array
                { "null", "MASKED" }                            // A null value
            };

            foreach (var testCase in testCases)
            {
                string actual = JsonDataMasker.DelegateMaskAll(JToken.Parse(testCase.Key)).ToString();
                Assert.AreEqual(testCase.Value, actual);
            }
        }

        [TestMethod]
        public void MaskKeepFirst1CharWorksAsExpected()
        {
            var testCases = new Dictionary<string, string>()
            {
                { "'SomeData'", "S...(8)" },                    // Longer than 1 character
                { "'S'", "MASKED" },                            // Exactly 1 character long
                { "''", "MASKED" },                             // An empty string
                { "{ \"propertyA\" : 1 }", "MASKED" },          // An object
                { "[ \"elementA\", \"elementB\" ]", "MASKED" }, // An array
                { "null", "MASKED" }                            // A null value
            };

            foreach (var testCase in testCases)
            {
                string actual = JsonDataMasker.DelegateMaskKeepFirst1Char(JToken.Parse(testCase.Key)).ToString();
                Assert.AreEqual(testCase.Value, actual);
            }
        }

        [TestMethod]
        public void MaskKeepFirst2CharWorksAsExpected()
        {
            var testCases = new Dictionary<string, string>()
            {
                { "'SomeData'", "So...(8)" },                   // Longer than 2 characters
                { "'So'", "MASKED" },                           // Exactly 2 characters long
                { "'S'", "MASKED" },                            // Shorter than 2 characters
                { "''", "MASKED" },                             // An empty string
                { "{ \"propertyA\" : 1 }", "MASKED" },          // An object
                { "[ \"elementA\", \"elementB\" ]", "MASKED" }, // An array
                { "null", "MASKED" }                            // A null value
            };

            foreach (var testCase in testCases)
            {
                string actual = JsonDataMasker.DelegateMaskKeepFirst2Char(JToken.Parse(testCase.Key)).ToString();
                Assert.AreEqual(testCase.Value, actual);
            }
        }

        [TestMethod]
        public void MaskKeepFirst3CharWorksAsExpected()
        {
            var testCases = new Dictionary<string, string>()
            {
                { "'SomeData'", "Som...(8)" },                  // Longer than 3 characters
                { "'Som'", "MASKED" },                          // Exactly 3 characters long
                { "'So'", "MASKED" },                           // Shorter than 3 characters
                { "''", "MASKED" },                             // An empty string
                { "{ \"propertyA\" : 1 }", "MASKED" },          // An object
                { "[ \"elementA\", \"elementB\" ]", "MASKED" }, // An array
                { "null", "MASKED" }                            // A null value
            };

            foreach (var testCase in testCases)
            {
                string actual = JsonDataMasker.DelegateMaskKeepFirst3Char(JToken.Parse(testCase.Key)).ToString();
                Assert.AreEqual(testCase.Value, actual);
            }
        }

        [TestMethod]
        public void MaskKeepFirst5CharWorksAsExpected()
        {
            var testCases = new Dictionary<string, string>()
            {
                { "'SomeData'", "SomeD...(8)" },                  // Longer than 5 characters
                { "'SomeD'", "MASKED" },                        // Exactly 5 characters long
                { "'So'", "MASKED" },                           // Shorter than 5 characters
                { "''", "MASKED" },                             // An empty string
                { "{ \"propertyA\" : 1 }", "MASKED" },          // An object
                { "[ \"elementA\", \"elementB\" ]", "MASKED" }, // An array
                { "null", "MASKED" }                            // A null value
            };

            foreach (var testCase in testCases)
            {
                string actual = JsonDataMasker.DelegateMaskKeepFirst5Char(JToken.Parse(testCase.Key)).ToString();
                Assert.AreEqual(testCase.Value, actual);
            }
        }

        [TestMethod]
        public void MaskEmailWorksAsExpected()
        {
            var testCases = new Dictionary<string, string>()
            {
                { "'someemail@outlook.com'", "so...l(9)@outlook.com" },     // A typical email address
                { "'foo@outlook.com'", "MASKED@outlook.com" },              // A short email address
                { "'not.an.email.address'", "MASKED(20)" },                 // Not an email address
                { "''", "MASKED(0)" },                                      // An empty string
                { "{ \"propertyA\" : 1 }", "MASKED" },                      // An object
                { "[ \"elementA\", \"elementB\" ]", "MASKED" },             // An array
                { "null", "MASKED" }                                        // A null value
            };

            foreach (var testCase in testCases)
            {
                string actual = JsonDataMasker.DelegateMaskEmail(JToken.Parse(testCase.Key)).ToString();
                Assert.AreEqual(testCase.Value, actual);
            }
        }

        [TestMethod]
        public void MaskKeepLast4CharWorksAsExpected()
        {
            var testCases = new Dictionary<string, string>()
            {
                { "'SomeData'", "...Data(8)" },                 // Longer than 4 characters
                { "'Some'", "MASKED" },                         // Exactly 4 characters long
                { "'Som'", "MASKED" },                          // Shorter than 4 characters
                { "''", "MASKED" },                             // An empty string
                { "{ \"propertyA\" : 1 }", "MASKED" },          // An object
                { "[ \"elementA\", \"elementB\" ]", "MASKED" }, // An array
                { "null", "MASKED" }                            // A null value
            };

            foreach (var testCase in testCases)
            {
                string actual = JsonDataMasker.DelegateMaskKeepLast4Char(JToken.Parse(testCase.Key)).ToString();
                Assert.AreEqual(testCase.Value, actual);
            }
        }

        [TestMethod]
        public void MaskEmailOrPhoneWorksAsExpected()
        {
            var testCases = new Dictionary<string, string>()
            {
                { "'some.one@outlook.co.fr'", "so...e(8)@outlook.co.fr" },  // A typical email address
                { "'foo@outlook.co.fr'", "MASKED@outlook.co.fr" },          // A short email address
                { "'not.an.email.address'", "not.a...(20)" },               // Not an email address
                { "'+12124736200'", "+1212...(12)" },                       // A typical US phone number
                { "'+448719429100'", "+4487...(13)" },                      // A typical UK phone number
                { "'+496987407950'", "+4969...(13)" },                      // A typical DE phone number
                { "'+861068338822'", "+8610...(13)" },                      // A typical CN phone number
                { "''", "MASKED" },                                         // An empty string
                { "{ \"propertyA\" : 1 }", "MASKED" },                      // An object
                { "[ \"elementA\", \"elementB\" ]", "MASKED" },             // An array
                { "null", "MASKED" }                                        // A null value
            };

            foreach (var testCase in testCases)
            {
                string actual = JsonDataMasker.DelegateMaskEmailOrPhone(JToken.Parse(testCase.Key)).ToString();
                Assert.AreEqual(testCase.Value, actual);
            }
        }

        [DataRow("[{\"Key\":\"x-ms-authinfo\",\"Value\":\"MASKED(54)\"}]", "[{\"Key\":\"x-ms-authinfo\",\"Value\":\"type=QUFE,email=MaskedEmail,tid=MaskedTId,context=bWU=\"}]")]
        [DataRow("[{\"Key\":\"X-ARR-ClientCert\",\"Value\":\"MASKED(11)\"}]", "[{\"Key\":\"X-ARR-ClientCert\",\"Value\":\"MASKEDCerts\"}]")]
        [DataTestMethod]
        public void MaskHeaderWithARRClientCertAsExpected(string expectedMaskedHeaderValue, string incomingHeader)
        {
            // Arrange
            var jsonDataMasker = new JsonDataMasker();

            // Act
            string actual = jsonDataMasker.MaskHeader(JToken.Parse(incomingHeader).ToString());

            // Assert
            Assert.AreEqual(expectedMaskedHeaderValue, actual);
        }

        [TestMethod]
        public void MaskVPAs_SimplePayload_MasksCorrectly()
        {
            // Arrange
            var jsonDataMasker = new JsonDataMasker();

            string payload = "{\"payer_vpa\":\"souravsharma@bank\"}";
            string maskedPayload = jsonDataMasker.MaskSingle(payload);

            Assert.IsTrue(maskedPayload.Contains("so...a(12)@bank"));
        }

        [TestMethod]
        public void MaskVPAs_NestedObjects_MasksCorrectly()
        {
            // Arrange
            var jsonDataMasker = new JsonDataMasker();

            string payload = "{\"transaction\":{\"payer_vpa\":\"souravsharma@bank\"}}";
            string maskedPayload = jsonDataMasker.MaskSingle(payload);

            Assert.IsTrue(maskedPayload.Contains("so...a(12)@bank"));
        }

        [TestMethod]
        public void MaskVPAs_Arrays_MasksCorrectly()
        {
            // Arrange
            var jsonDataMasker = new JsonDataMasker();

            string payload = "{\"vpas\":[{\"payer_vpa\":\"souravsharma@bank\"}, {\"vpa\":\"sourav@bank\"}]}";
            string maskedPayload = jsonDataMasker.MaskSingle(payload);
            Assert.IsTrue(maskedPayload.Contains("so...a(12)@bank"));
            Assert.IsTrue(maskedPayload.Contains("so...v(6)@bank"));
        }

        [TestMethod]
        public void MaskVPAs_MixedContent_MasksCorrectly()
        {
            // Arrange
            var jsonDataMasker = new JsonDataMasker();

            string payload = "{\"payer_vpa\":\"sourav@bank\", \"amount\":100, \"transaction\":{\"merchant_vpa\":\"microsoft@bank\"}}";
            string maskedPayload = jsonDataMasker.MaskSingle(payload);

            Assert.IsTrue(maskedPayload.Contains("so...v(6)@bank"));
            Assert.IsTrue(maskedPayload.Contains("mi...t(9)@bank"));
        }

        [TestMethod]
        public void MaskVPAs_EmptyValues_HandlesGracefully()
        {
            // Arrange
            var jsonDataMasker = new JsonDataMasker();

            string payload = "{\"payer_vpa\":\"\"}";
            string maskedPayload = jsonDataMasker.MaskSingle(payload);
            Assert.IsTrue(maskedPayload.Contains("MASKED(0)"));
        }
    }
}
