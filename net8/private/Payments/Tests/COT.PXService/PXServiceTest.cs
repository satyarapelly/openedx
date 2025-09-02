// <copyright file="PXServiceTest.cs" company="Microsoft">Copyright (c) Microsoft 2016. All rights reserved.</copyright>

namespace COT.PXService
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PXService.Model.IssuerService;
    using Microsoft.Commerce.Payments.PXService.Model.ThreeDSExternalService;
    using Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Test.Common;
    using Common = Microsoft.Commerce.Payments.Common.Transaction;
    using PXCommon = Microsoft.Commerce.Payments.PXCommon;

    [TestClass]
    public class PXServiceTest : TestBase
    {
        public TestSettings TestSettings { get; private set; }

        [TestInitialize]
        public void TestInitialize()
        {
            this.Initialize();
            this.TestSettings = TestSettings.CreateInstance();
        }

        /// <summary>
        /// The test is to verify the connectivity between 
        /// 1.  PX and PIMS. Also, client cert verification is required by pims in the call.
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestListPI_PIMSAuth()
        {
            this.ExecuteRequest(
                string.Format("v7.0/{0}/paymentInstrumentsEx?status=active&status=pending&status=deactivated&country=us", this.TestSettings.AccountId),
                HttpMethod.Get,
                null,
                null,
                null,
                (responseCode, responseBody) =>
                {
                    Assert.AreEqual(HttpStatusCode.OK, responseCode);
                });
        }

        /// <summary>
        /// The test is to verify the connectivity between 
        /// PX and StoredValue.
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestFundStoredValue_StoredValueAuth()
        {
            this.ExecuteRequest(
                string.Format("v7.0/{0}/paymentMethodDescriptions?type=bitcoin&partner=amcweb&operation=FundStoredValue&country=US&language=en-US&family=ewallet", this.TestSettings.AccountId),
                HttpMethod.Get,
                null,
                null,
                null,
                (responseCode, responseBody) =>
                {
                    Assert.AreEqual(HttpStatusCode.OK, responseCode);
                });
        }

        /// <summary>
        /// The test is to verify the connectivity between 
        /// PX and StoredValue.
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestFundStoredValue_StoredValue_Emulator()
        {
            var requestHeaders = new Dictionary<string, string>()
            {
                {
                    "x-ms-test", "{ \"scenarios\": \"px.storedvalue.cot\"}"
                }
            };

            this.ExecuteRequest(
                string.Format("v7.0/{0}/paymentMethodDescriptions?type=bitcoin&partner=amcweb&operation=FundStoredValue&country=US&language=en-US&family=ewallet", this.TestSettings.AccountId),
                HttpMethod.Get,
                null,
                null,
                requestHeaders,
                (responseCode, responseBody) =>
                {
                    Assert.AreEqual(HttpStatusCode.OK, responseCode);
                });
        }

        /// <summary>
        /// The test is to verify the following two connectivities
        /// 1.PX and Account (Jarvis)
        /// 2.PX and TaxId (PPE are not able to connect TaxId Service)
        /// </summary>
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        [DataRow(TestConstants.PartnerNames.Xbox, "br")]
        [DataRow(TestConstants.PartnerNames.Xbox, "pt")]
        [DataTestMethod]
        public void TestGetPaymentMethodDescriptionCcBr_Account_TaxId(string partner, string country)
        {
            this.ExecuteRequest(
                string.Format("v7.0/{0}/paymentMethodDescriptions?country={2}&partner={1}&language=en-US&completePrerequisites=true&family=credit_card", this.TestSettings.AccountId, partner, country),
                HttpMethod.Get,
                null,
                null,
                null,
                (responseCode, responseBody) =>
                {
                    Assert.AreEqual(HttpStatusCode.OK, responseCode);

                    // since the country requires tax ID collection and the test account doesn't have taxID, 
                    // there should be 4 pages showing up for the partner first name, last name, email and CPF.
                    Assert.AreEqual(4, (int)responseBody.First.displayDescription.Count);

                    // the third page (email address) should have previousNextGroup which won't submit request and ensure the next page CPF will show
                    Assert.AreEqual("previousNextGroup", (string)responseBody.First.displayDescription[2].members.Last.displayId);

                    // the last page (CPF) should have SaveNextGroup which will submit the request and end the flow
                    Assert.AreEqual("previousSaveNextPrivacyStatementGroup", (string)responseBody.First.displayDescription.Last.members.Last.displayId);
                });
        }

        /// <summary>
        /// The test is to verify the connectivity 
        /// 1. between PX and Risk
        /// 2. between PX and Account
        /// 3. between PX and PIMS. Also, client cert verification is NOT required by pims in the call.
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestSelectPaymentMethodDescriptionCart_Account_Risk_PIMSUnauth()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>()
            {
                { "x-ms-msaprofile", "PUID=123456789qwerta,emailAddress=pineapple@Green.com,firstName=Pineapple,lastName=Green" }
            };
            this.ExecuteRequest(
                string.Format("v7.0/{0}/paymentMethodDescriptions?country=US&language=en-US&allowedPaymentMethods=%5B%22credit_card.visa%22,%22credit_card.amex%22,%22credit_card.mc%22,%22credit_card.discover%22,%22credit_card.jcb%22,%22credit_card.hipercard%22,%22credit_card.unionpay_creditcard%22,%22credit_card.unionpay_debitcard%22,%22ewallet.paypal%22,%22ewallet.alipay_billing_agreement%22,%22mobile_billing_non_sim%22,%22direct_debit.sepa%22,%22direct_debit.ideal_billing_agreement%22,%22direct_debit.ach%22,%22ewallet.stored_value%22,%22invoice_credit.klarna%22%5D&filters=%7B%22chargeThreshold%22:0,%22exclusionTags%22:%5B%22Subscriptions%22,%22ModernSubscriptions%22%5D%7D&sessionId=8d4036dd-158a-4ba0-846b-99303c0f4ef0&orderId=ae26f1cd-23c1-4d69-9781-665ef10a0508&partner=webblends&operation=Select", this.TestSettings.AccountId),
                HttpMethod.Get,
                null,
                null,
                headers,
                (responseCode, responseBody) =>
                {
                    Assert.AreEqual(HttpStatusCode.OK, responseCode);
                });
        }

        /// <summary>
        /// The test is to verify legacy address validation scenario and check the connectivity 
        /// 1. between PX and Account (Jarvis)
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestLegacyAddressValidation_ValidAddress_Success()
        {
            var validAddress = new
            {
                first_name = "fname",
                last_name = "lname",
                address_line1 = "1 Microsoft Way",
                city = "Redmond",
                region = "WA",
                postal_code = "98052",
                country = "US"
            };

            this.ExecuteRequest(
                "v7.0/addresses/legacyValidate",
                HttpMethod.Post,
                null,
                validAddress,
                null,
                (responseCode, responseBody) =>
                {
                    Assert.AreEqual(HttpStatusCode.OK, responseCode, $"v7.0/addresses/legacyValidate failed, expected response code is {HttpStatusCode.OK} but was {responseCode}");
                    Assert.AreEqual(JsonConvert.SerializeObject(validAddress), JsonConvert.SerializeObject(responseBody.original_address), $"v7.0/addresses/legacyValidate failed, expected original_address in the response");
                    Assert.AreEqual("Verified", (string)responseBody.status, $"v7.0/addresses/legacyValidate failed, expected response is 'Verified' but was {(string)responseBody.status}");
                });
        }

        /// <summary>
        /// The test is to verify modern address validation scenario and check the connectivity 
        /// 1. between PX and Account (Jarvis)
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestModernAddressValidation_ValidAddress_Success()
        {
            var validAddress = new
            {
                first_name = "fname",
                last_name = "lname",
                address_line1 = "1 Microsoft Way",
                city = "Redmond",
                region = "WA",
                postal_code = "98052",
                country = "US"
            };

            this.ExecuteRequest(
                "v7.0/addresses/modernValidate",
                HttpMethod.Post,
                null,
                validAddress,
                null,
                (responseCode, responseBody) =>
                {
                    Assert.AreEqual(HttpStatusCode.OK, responseCode, $"v7.0/addresses/modernValidate failed, expected response code is {HttpStatusCode.OK} but was {responseCode}");
                    Assert.AreEqual(JsonConvert.SerializeObject(validAddress), JsonConvert.SerializeObject(responseBody.original_address), $"v7.0/addresses/modernValidate failed, expected original_address in the response");
                    Assert.AreEqual("VerifiedShippable", (string)responseBody.status, $"v7.0/addresses/modernValidate failed, expected response is 'Verified' but was {(string)responseBody.status}");
                });
        }

        /// <summary>
        /// The test is to verify core scenario CC GetPaymentMethodDescription for oxowebdirect, webblends, xbox, bing, webblends_inline
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestGetPaymentMethodDescriptionCCUS()
        {
            string[] partners = { TestConstants.PartnerNames.OxoWebDirect, TestConstants.PartnerNames.Webblends, TestConstants.PartnerNames.Xbox, TestConstants.PartnerNames.Bing, TestConstants.PartnerNames.WebblendsInline };
            foreach (string partner in partners)
            {
                this.ExecuteRequest(
                    string.Format("v7.0/{0}/paymentMethodDescriptions?family=credit_card&country=US&language=en-US&partner={1}&complete=true", this.TestSettings.AccountId, partner),
                    HttpMethod.Get,
                    null,
                    null,
                    null,
                    (responseCode, responseBody) =>
                    {
                        Assert.AreEqual(HttpStatusCode.OK, responseCode, string.Format("TestGetPaymentMethodDescriptionCCUs fails for partner {0}, expected response code {1}, actual response {2}", partner, HttpStatusCode.OK, responseCode));
                    });
            }
        }

        /// <summary>
        /// The test is to verify core scenario CC GetPaymentMethodDescription for webpay
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestGetPaymentMethodDescriptionCCUSNoPrerequisites()
        {
            string[] partners = { TestConstants.PartnerNames.Webpay };
            foreach (string partner in partners)
            {
                this.ExecuteRequest(
                    string.Format("v7.0/{0}/paymentMethodDescriptions?family=credit_card&country=US&language=en-US&partner={1}", this.TestSettings.AccountId, partner),
                    HttpMethod.Get,
                    null,
                    null,
                    null,
                    (responseCode, responseBody) =>
                    {
                        Assert.AreEqual(HttpStatusCode.OK, responseCode, string.Format("TestGetPaymentMethodDescriptionCCUSNoPrerequisites fails for partner {0}, expected response code {1}, actual response {2}", partner, HttpStatusCode.OK, responseCode));
                    });
            }
        }

        /// <summary>
        /// The test is to verify core scenario Paypal GetPaymentMethodDescription for webblends and xbox
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestGetPaymentMethodDescriptionPaypalUs()
        {
            string[] partners = { TestConstants.PartnerNames.OxoWebDirect, TestConstants.PartnerNames.Webblends, TestConstants.PartnerNames.Xbox, TestConstants.PartnerNames.WebblendsInline };
            foreach (string partner in partners)
            {
                this.ExecuteRequest(
                    string.Format("v7.0/{0}/paymentMethodDescriptions?family=ewallet&type=paypal&country=US&language=en-US&partner={1}&completePrerequisites=true&operation=Add", this.TestSettings.AccountId, partner),
                    HttpMethod.Get,
                    null,
                    null,
                    null,
                    (responseCode, responseBody) =>
                    {
                        Assert.AreEqual(HttpStatusCode.OK, responseCode, string.Format("TestGetPaymentMethodDescriptionPaypalUs fails for partner {0}, expected response code {1}, actual response {2}", partner, HttpStatusCode.OK, responseCode));
                    });
            }
        }

        /// <summary>
        /// The test is to verify core scenario Nonsim GetPaymentMethodDescription for webblends and xbox
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestGetPaymentMethodDescriptionMobiNonSimDe()
        {
            string[] partners = { TestConstants.PartnerNames.OxoWebDirect, TestConstants.PartnerNames.Webblends, TestConstants.PartnerNames.Xbox };
            foreach (string partner in partners)
            {
                this.ExecuteRequest(
                    string.Format("v7.0/{0}/paymentMethodDescriptions?family=mobile_billing_non_sim&type=o2o-de-nonsim&country=DE&language=de-DE&partner={1}&completePrerequisites=true&operation=Add", this.TestSettings.AccountId, partner),
                    HttpMethod.Get,
                    null,
                    null,
                    null,
                    (responseCode, responseBody) =>
                    {
                        Assert.AreEqual(HttpStatusCode.OK, responseCode, string.Format("TestGetPaymentMethodDescriptionMobiNonSimDe fails for partner {0}, expected response code {1}, actual response {2}", partner, HttpStatusCode.OK, responseCode));
                    });
            }
        }

        /// <summary>
        /// The test is to verify the connectivity between PX and WD
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestPostPIAgainstPIMSEmulator()
        {
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.nsm.add.success");
            object payload = new
            {
                paymentMethodFamily = "mobile_carrier_billing",
                paymentMethodType = "att-us",
                context = "purchase",
                sessionId = Guid.NewGuid().ToString(),
                details = new
                {
                    iccId = "112233445566771",
                    simStatus = "active",
                    phoneSerialNumber = "123456789054321"
                }
            };

            this.ExecuteRequest(
                string.Format("v7.0/{0}/paymentInstrumentsEx?country=nl&language=en-US&partner=webblends&completePrerequisites=True&billableAccountId=12334", this.TestSettings.AccountId),
                HttpMethod.Post,
                tc,
                payload,
                null,
                (responseCode, responseBody) =>
                {
                    Assert.AreEqual(HttpStatusCode.OK, responseCode);
                });
        }

        /// <summary>
        /// The test is to verify List PI returns 200 when user has no CSV
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestListPINoCsvAgainstPIMSEmulator()
        {
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.pims.listpi.nocsv.success");

            foreach (string partner in TestConstants.ListPIPartners)
            {
                string url = string.Format("v7.0/{0}/paymentMethodDescriptions?country=us&language=en-US&partner={1}&operation=selectInstance&allowedPaymentMethods=%5B%22credit_card%22%2C%22ewallet.paypal%22%2C%22mobile_billing_non_sim%22%2C%22ewallet.stored_value%22%2C%22direct_debit.ach%22%5D&filters=%7BchargeThreshold%3A4.00%7D", this.TestSettings.AccountId, partner);

                this.ExecuteRequest(
                    url,
                    HttpMethod.Get,
                    tc,
                    null,
                    null,
                    (responseCode, responseBody) =>
                    {
                        Assert.AreEqual(HttpStatusCode.OK, responseCode);
                    });
            }
        }

        /// <summary>
        /// The test is to verify List PI returns 200 when user has CSV
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestListPICsvAgainstPIMSEmulator()
        {
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.pims.listpi.csv.success");

            foreach (string partner in TestConstants.ListPIPartners)
            {
                string url = string.Format("v7.0/{0}/paymentMethodDescriptions?country=us&language=en-US&partner={1}&operation=selectInstance&allowedPaymentMethods=%5B%22credit_card%22%2C%22ewallet.paypal%22%2C%22mobile_billing_non_sim%22%2C%22ewallet.stored_value%22%2C%22direct_debit.ach%22%5D&filters=%7BchargeThreshold%3A4.00%7D", this.TestSettings.AccountId, partner);

                this.ExecuteRequest(
                    url,
                    HttpMethod.Get,
                    tc,
                    null,
                    null,
                    (responseCode, responseBody) =>
                    {
                        Assert.AreEqual(HttpStatusCode.OK, responseCode);
                    });
            }
        }

        // <summary>
        /// The test is to verify List PI returns 200 when user has CSV
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestListPIListBackPIAgainstPIMSEmulator()
        {
            Common.TestContext[] testHeaders = new Common.TestContext[]
            {
                new Common.TestContext("PX.COT", DateTime.Now, "px.pims.listpi.success"),
                new Common.TestContext("PX.COT", DateTime.Now, "px.pims.listpi.expiredcc.success"),
                new Common.TestContext("PX.COT", DateTime.Now, "px.pims.listpi.cconly.nopi.success")
            };

            string urlTemplate = "v7.0/{0}/paymentMethodDescriptions?partner={1}"
                + "&operation=SelectInstance&country=US&language=en-US&"
                + "filters=%7B%22splitPaymentSupported%22%3A{2}%2C%22isBackupPiOptional%22%3A{3}%2C%22id%22%3A%22{4}%22%7D";
            string[] partners = new string[] { "amcweb" };
            dynamic[] combinations = new dynamic[]
            {
                // test list PI
                new
                {
                    splitPaymentSupported = false,
                    isBackupPiOptional = false,
                    id = "FgGmBAAAAAApAACD"
                },
                // test list backup PI
                new
                {
                    splitPaymentSupported = true,
                    isBackupPiOptional = false,
                    id = "FgGmBAAAAAApAACD"
                },
                // test list backup PI without i don't want backup 
                new
                {
                    splitPaymentSupported = true,
                    isBackupPiOptional = true,
                    id = string.Empty
                }
            };


            foreach (dynamic comb in combinations)
            {
                foreach (var tc in testHeaders)
                {
                    string url = string.Format(
                        urlTemplate,
                        this.TestSettings.AccountId,
                        "amcweb",
                        comb.splitPaymentSupported.ToString().ToLower(),
                        comb.isBackupPiOptional.ToString().ToLower(),
                        comb.id);

                    this.ExecuteRequest(
                        url,
                        HttpMethod.Get,
                        tc,
                        null,
                        null,
                        (responseCode, responseBody) =>
                        {
                            Assert.AreEqual(HttpStatusCode.OK, responseCode);
                            dynamic dataDescription = responseBody.First.data_description;
                            if (tc.ScenariosContain("px.pims.listpi.success"))
                            {
                                if (!string.IsNullOrEmpty(comb.id))
                                {
                                    Assert.AreEqual((string)comb.id, (string)dataDescription.id.default_value);
                                    Assert.AreEqual((bool)responseBody.First.displayDescription.First.members.First.isSelectFirstItem, true);
                                }
                                else
                                {
                                    // if default id is empty, then the first pi is the default pi
                                    Assert.IsNull(responseBody.First.displayDescription.First.members.First.isSelectFirstItem);
                                }
                            }
                            // after the pidl form is more solid, expect to add more check here
                            if (tc.ScenariosContain("px.pims.listpi.expiredcc.success"))
                            {
                                dynamic usePaymentInstrumentGroup = responseBody.First.displayDescription.First.members.Last;
                                Assert.AreEqual((bool)usePaymentInstrumentGroup.isSubmitGroup, true);
                                Assert.AreEqual((string)usePaymentInstrumentGroup.members.First.pidlAction.type, "successWithSelectedPidlAction");
                                dynamic expiredPI = responseBody.First.displayDescription.First.members.First.possibleOptions.FgGmBAAAAAApAACA;
                                Assert.AreEqual((string)expiredPI.pidlAction.context.partnerHints.triggeredBy, "submitGroup");
                                Assert.AreEqual((string)expiredPI.displayContent.members[1].members.Last.pidlAction.context.partnerHints.triggeredBy, "updateButton");
                            }
                            else if (tc.ScenariosContain("px.pims.listpi.cconly.nopi.success"))
                            {
                                // If eligible pi list is empty and user doesn't need a backup pi in the select back pi 
                                // (splitPaymentSupported == true and isBackupPiOptional = true)
                                // we return AddPaymentInstrument action 
                                if (comb.splitPaymentSupported == false || (comb.splitPaymentSupported == true && comb.isBackupPiOptional == false))
                                {
                                    Assert.AreEqual((string)responseBody.First.clientAction.type, "ReturnContext");
                                    Assert.AreEqual((string)responseBody.First.clientAction.context.action, "addPaymentInstrument");
                                    Assert.AreEqual((string)responseBody.First.clientAction.context.resourceActionContext.action, "addPaymentInstrument");
                                }
                                else if (comb.splitPaymentSupported == true && comb.isBackupPiOptional == true)
                                {
                                    Assert.AreEqual(string.Empty, (string)responseBody.First.displayDescription.First.members.First.possibleValues.NoBackupPISelected);
                                    Assert.AreEqual(string.Empty, (string)responseBody.First.displayDescription.First.members.First.possibleOptions.NoBackupPISelected.displayText);
                                    Assert.AreEqual("Add a way to pay", (string)responseBody.First.displayDescription.First.members[1].displayContent);
                                }
                            }
                        });
                }
            }
        }

        /// <summary>
        /// The test is to verify CC GetPaymentMethodDescription when user doesn't have completed profile
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestGetPMDAccountHasNoFirstName_PIMSEmulator_AccountEmulator()
        {
            string[] partners = { TestConstants.PartnerNames.Webblends, TestConstants.PartnerNames.OxoWebDirect };
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.account.v2.no.firstname,px.pims.cc.add.success");
            foreach (string partner in partners)
            {
                this.ExecuteRequest(
                    string.Format("v7.0/{0}/paymentMethodDescriptions?family=credit_card&type=visa&country=US&language=en-US&partner={1}&completePrerequisites=true", this.TestSettings.AccountId, partner),
                    HttpMethod.Get,
                    tc,
                    null,
                    null,
                    (responseCode, responseBody) =>
                    {
                        Assert.AreEqual(HttpStatusCode.OK, responseCode, string.Format("TestGetPMDAccountHasNoFirstName fails for partner {0}, expected response code {1}, actual response {2}", partner, HttpStatusCode.OK, responseCode));
                        Assert.AreEqual("profile", (string)responseBody.First.identity.description_type);
                    });
            }
        }

        /// <summary>
        /// The test is to verify select resource type with payment methods grouped
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestGetPMD_GroupPM_PIMSEmulator()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>()
            {
                { "x-ms-flight", "enablePaymentMethodGrouping" }
            };

            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.pims.pmgrouping");

            string[] partners = { TestConstants.PartnerNames.Cart, TestConstants.PartnerNames.Webblends };

            foreach (string partner in partners)
            {
                // With flight header, pm grouping should be enabled
                this.ExecuteRequest(
                    string.Format("v7.0/{0}/paymentMethodDescriptions?partner={1}&operation=Select&country=us&language=en-US&filters=%7BchargeThreshold%3A1400.00%7D", this.TestSettings.AccountId, partner),
                    HttpMethod.Get,
                    tc,
                    null,
                    headers,
                    (responseCode, responseBody) =>
                    {
                        Assert.AreEqual(HttpStatusCode.OK, responseCode, string.Format("TestGetPMD_GroupPM_PIMSEmulator fails for partner cart with PM grouping enabled, expected response code {0}, actual response {1}", HttpStatusCode.OK, responseCode));

                        Assert.AreEqual(responseBody.First.displayDescription.Count, 2, "A pidl with 2 pages should be returned, the first page is the main page including PM groups and PM couldn't be grouped, the second page includes PMs for online bank transfer PM group");
                        Assert.AreEqual(responseBody.First.displayDescription[0].displayName.Value, "paymentMethodSelectPMGroupingPage");
                        Assert.AreEqual(responseBody.First.displayDescription.First.members[1].selectType.Value, "buttonList");
                        Assert.AreEqual(responseBody.First.displayDescription.First.members[1].possibleOptions.credit_card_amex_discover_mc_visa.pidlAction.type.Value, "success", "for a single PM option, the pidlaction should be success");
                        Assert.IsNotNull(responseBody.First.displayDescription.First.members[1].possibleOptions.credit_card_amex_discover_mc_visa.displayContent);
                        Assert.AreEqual(responseBody.First.displayDescription.First.members[1].possibleOptions.credit_card_amex_discover_mc_visa.pidlAction.context.paymentMethodFamily.Value, "credit_card");
                        Assert.AreEqual(responseBody.First.displayDescription.First.members[1].possibleOptions.credit_card_amex_discover_mc_visa.pidlAction.context.paymentMethodType.Value, "amex,discover,mc,visa");

                        // if there is only one PM in a group, do not show it as a group
                        Assert.AreEqual(responseBody.First.displayDescription.First.members[1].possibleOptions.ewallet_paypal.pidlAction.type.Value, "success", "for a single PM option, the pidlaction should be success");
                        Assert.IsNotNull(responseBody.First.displayDescription.First.members[1].possibleOptions.ewallet_paypal.displayContent);
                        Assert.AreEqual(responseBody.First.displayDescription.First.members[1].possibleOptions.ewallet_paypal.pidlAction.context.paymentMethodFamily.Value, "ewallet");
                        Assert.AreEqual(responseBody.First.displayDescription.First.members[1].possibleOptions.ewallet_paypal.pidlAction.context.paymentMethodType.Value, "paypal");

                        // selectOption of a PM group should naviage the user to a sub page containing all indiviual PMs belonging this the PM group
                        Assert.AreEqual(responseBody.First.displayDescription.First.members[1].possibleOptions.online_bank_transfer.pidlAction.type.Value, "moveToPageIndex", "for a pm group option, the pidlaction should be moveToPageIndex");
                        Assert.AreEqual(responseBody.First.displayDescription.First.members[1].possibleOptions.online_bank_transfer.pidlAction.context.pageIndex.Value, 2, "moveToPageIndex action should naviatge to page index 2 for the subpage of online bank transfer PM group");
                        Assert.IsNotNull(responseBody.First.displayDescription.First.members[1].possibleOptions.online_bank_transfer.displayContent);

                        // For a non PM group option, clicking the button should return a success event with PM information.
                        Assert.AreEqual(responseBody.First.displayDescription[2].members[1].possibleOptions.online_bank_transfer_paysafecard.pidlAction.type.Value, "success", "for a single PM option, the pidlaction should be success");
                        Assert.IsNotNull(responseBody.First.displayDescription[2].members[1].possibleOptions.online_bank_transfer_paysafecard.displayContent);
                        Assert.AreEqual(responseBody.First.displayDescription[2].members[1].possibleOptions.online_bank_transfer_paysafecard.pidlAction.context.paymentMethodFamily.Value, "online_bank_transfer");
                        Assert.AreEqual(responseBody.First.displayDescription[2].members[1].possibleOptions.online_bank_transfer_paysafecard.pidlAction.context.paymentMethodType.Value, "paysafecard");
                    });

                // Without flight header, pm grouping should not be enabled
                this.ExecuteRequest(
                    string.Format("v7.0/{0}/paymentMethodDescriptions?partner={1}&operation=Select&country=nl&language=en-US", this.TestSettings.AccountId, partner),
                    HttpMethod.Get,
                    tc,
                    null,
                    null,
                    (responseCode, responseBody) =>
                    {
                        Assert.AreEqual(HttpStatusCode.OK, responseCode, string.Format("TestGetPMD_GroupPM_PIMSEmulator fails for partner cart without PM grouping enabled, expected response code {0}, actual response {1}", HttpStatusCode.OK, responseCode));
                        Assert.AreEqual(responseBody.First.displayDescription.Count, 1);
                        Assert.AreEqual(responseBody.First.displayDescription[0].displayName.Value, "PaymentMethodSelectionPage");
                        if (partner.Equals(TestConstants.PartnerNames.Cart, StringComparison.OrdinalIgnoreCase))
                        {
                            Assert.AreEqual(responseBody.First.displayDescription[0].members[0].members[0].selectType.Value, "dropDown");
                        }

                        if (partner.Equals(TestConstants.PartnerNames.Webblends, StringComparison.OrdinalIgnoreCase))
                        {
                            Assert.AreEqual(responseBody.First.displayDescription[0].members[1].selectType.Value, "buttonList");
                        }

                    });
            }
        }

        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestGetPaymentMethod_PMGrouping_SubPage_SubmitBlock()
        {
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.pims.pmgrouping");

            Dictionary<string, string> headers = new Dictionary<string, string>()
            {
                { "x-ms-flight", "enablePaymentMethodGrouping,enablePMGroupingSubpageSubmitBlock" },
            };

            // With flight header, pm grouping should be enabled
            this.ExecuteRequest(
                string.Format("v7.0/{0}/paymentMethodDescriptions?partner={1}&operation=Select&country=us&language=en-US&filters=%7BchargeThreshold%3A1400.00%7D", this.TestSettings.AccountId, TestConstants.PartnerNames.Cart),
                HttpMethod.Get,
                tc,
                null,
                headers,
                (responseCode, responseBody) =>
                {
                    Assert.AreEqual(HttpStatusCode.OK, responseCode, string.Format("TestGetPaymentMethod_PMGrouping_SubPage_SubmitBlock fails for partner cart with PM grouping enabled, expected response code {0}, actual response {1}", HttpStatusCode.OK, responseCode));

                    Assert.AreEqual(responseBody.First.displayDescription.Count, 2, "A pidl with 2 pages should be returned, the first page is the main page including PM groups and PM couldn't be grouped, the second page includes PMs for online bank transfer PM group");
                    Assert.AreEqual(responseBody.First.displayDescription[0].displayName.Value, "paymentMethodSelectPMGroupingPage");
                    Assert.AreEqual(responseBody.First.displayDescription.First.members[1].selectType.Value, "buttonList", "Payment method should be in button list");
                    Assert.AreEqual(responseBody.First.displayDescription[1].members[2].displayType.Value, "group", "Back button should be in submit page block");
                });
        }

        /// <summary>
        /// The test is to verify core scenario CC GetPaymentMethodDescription for Webblends, PX should return CC pidl with linked address pidl
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestGetPMDAccountHasNoAddressPidlAppendAddress_PIMSEmulator_AccountEmulator()
        {
            string[] partners = { TestConstants.PartnerNames.Webblends, TestConstants.PartnerNames.OxoWebDirect };
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.account.v2.no.address,px.pims.cc.add.success");
            foreach (string partner in partners)
            {
                this.ExecuteRequest(
                    string.Format("v7.0/{0}/paymentMethodDescriptions?family=credit_card&type=visa&country=US&language=en-US&partner={1}&completePrerequisites=true", this.TestSettings.AccountId, partner),
                    HttpMethod.Get,
                    tc,
                    null,
                    null,
                    (responseCode, responseBody) =>
                    {
                        Assert.AreEqual(HttpStatusCode.OK, responseCode, string.Format("TestGetPMDAccountHasNoAddressPidlAppendAddress fails for partner {0}, expected response code {1}, actual response {2}", partner, HttpStatusCode.OK, responseCode));
                        Assert.IsNotNull(responseBody.First.linkedPidls);
                    });
            }
        }

        /// <summary>
        /// The test is to verify core scenario CC GetPaymentMethodDescription for commercial store
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestAddCreditCardWithEmployeeProfileCompletePrerequisiteWithAVS()
        {
            string[] partners = { TestConstants.PartnerNames.CommercialStores };
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.account.v3.us.employee.empty.profile,px.pims.cc.add.success");
            Dictionary<string, string> headers = new Dictionary<string, string>()
            {
                { "x-ms-clientcontext-encoding", "base64" },
                { "x-ms-authinfo", string.Format(
                    "type={0},context={1}",
                    Convert.ToBase64String(Encoding.UTF8.GetBytes("aad")),
                    Convert.ToBase64String(Encoding.UTF8.GetBytes("me")))
                },
                { "x-ms-flight", "enableAVSAddtionalFlags,showAVSSuggestions"}
            };

            foreach (string partner in partners)
            {
                this.ExecuteRequest(
                    string.Format("v7.0/{0}/paymentMethodDescriptions?family=credit_card&type=visa&country=US&language=en-US&partner={1}&completePrerequisites=true", this.TestSettings.AccountId, partner),
                    HttpMethod.Get,
                    tc,
                    null,
                    headers,
                    (responseCode, responseBody) =>
                    {
                        Assert.AreEqual(HttpStatusCode.OK, responseCode, string.Format("TestGetPMDAccountHasNoAddressPidlAppendAddress fails for partner {0}, expected response code {1}, actual response {2}", partner, HttpStatusCode.OK, responseCode));

                        // Verify 2 additional AVS flags are added. 
                        Assert.IsNotNull(responseBody.First.linkedPidls.First.data_description["default_address"].First.data_description["is_customer_consented"]);
                        Assert.IsNotNull(responseBody.First.linkedPidls.First.data_description["default_address"].First.data_description["is_avs_full_validation_succeeded"]);
                    });
            }
        }

        /// <summary>
        /// The test is to verify CC GetPaymentMethodDescription for Office and Payin, px should return cc pidl without linked address pidl
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestGetPMDAccountHasNoAddressPidlNotAppendAddress_PIMSEmulator_AccountEmulator()
        {
            string[] partners = { TestConstants.PartnerNames.Office, TestConstants.PartnerNames.Payin };
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.account.v2.no.address,px.pims.cc.add.success");
            foreach (string partner in partners)
            {
                this.ExecuteRequest(
                    string.Format("v7.0/{0}/paymentMethodDescriptions?family=credit_card&type=visa&country=US&language=en-US&partner={1}&completePrerequisites=true", this.TestSettings.AccountId, partner),
                    HttpMethod.Get,
                    tc,
                    null,
                    null,
                    (responseCode, responseBody) =>
                    {
                        Assert.AreEqual(HttpStatusCode.OK, responseCode, string.Format("TestGetPMDAccountHasNoAddressPidlNotAppendAddress fails for partner {0}, expected response code {1}, actual response {2}", partner, HttpStatusCode.OK, responseCode));
                        Assert.IsNull(responseBody.First.linkedPidls);
                    });
            }
        }

        /// <summary>
        /// The test is to verify core scenario Post CC for webblends and xbox
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestPostCCAgainstPIMSEmulator()
        {
            string[] partners = { TestConstants.PartnerNames.OxoWebDirect, TestConstants.PartnerNames.Webblends, TestConstants.PartnerNames.Xbox };
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.cc.add.success");
            foreach (string partner in partners)
            {
                object payload = new
                {
                    paymentMethodFamily = "credit_card",
                    paymentMethodType = "visa",
                    paymentMethodOpertion = "add",
                    paymentMethodCountry = "us",
                    paymentMethodResource_id = "credit_card.visa",
                    context = "purchase",
                    sessionId = Guid.NewGuid().ToString(),
                    riskData = new
                    {
                        dataType = "payment_method_riskData",
                        dataOperation = "add",
                        dataCountry = "us",
                        greenId = "bb606624-79b0-401b-920a-ce3b66861462"
                    },
                    details = new
                    {
                        dataType = "credit_card_visa_details",
                        dataOperation = "add",
                        dataCountry = "us",
                        accountHolderName = "PX Test",
                        accountToken = "AAkHGDuto7uVF5D/FACaO3SzcjKtE4sR7r98MmS91ILE",
                        expiryMonth = "2",
                        expiryYear = "2020",
                        cvvToken = "placeholder",
                        address = new
                        {
                            addressType = "billing",
                            addressOperation = "add",
                            addressCountry = "us",
                            address_line1 = "1st Street",
                            city = "Baggs",
                            region = "wy",
                            postal_code = "82321",
                            country = "us"
                        },
                        permission = new
                        {
                            dataType = "permission_details",
                            dataOperation = "add",
                            dataCountry = "us",
                            hmac = string.Empty
                        }
                    },
                    pxmac = PXCommon.ObfuscationHelper.GetHashValue(this.TestSettings.AccountId, PXCommon.ObfuscationHelper.JarvisAccountIdHashSalt),
                };

                this.ExecuteRequest(
                    string.Format("v7.0/{0}/paymentInstrumentsEx?country=us&language=en-US&partner={1}&completePrerequisites=True", this.TestSettings.AccountId, partner),
                    HttpMethod.Post,
                    tc,
                    payload,
                    null,
                    (responseCode, responseBody) =>
                    {
                        Assert.AreEqual(HttpStatusCode.OK, responseCode, string.Format("TestPostCCAgainstPIMSEmulator fails for partner {0}, expected response code {1}, actual response {2}", partner, HttpStatusCode.OK, responseCode));
                        Assert.IsNotNull(responseBody.id);
                    });
            }
        }

        /// <summary>
        /// The test is to verify core scenario Post Paypal for webblends and xbox
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestPostPaypalAgainstPIMSEmulator()
        {
            string[] partners = { TestConstants.PartnerNames.OxoWebDirect, TestConstants.PartnerNames.Webblends, TestConstants.PartnerNames.Xbox };
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.paypal.add.success");
            foreach (string partner in partners)
            {
                object payload = new
                {
                    paymentMethodFamily = "ewallet",
                    paymentMethodType = "paypal",
                    paymentMethodOpertion = "add",
                    paymentMethodCountry = "us",
                    paymentMethodResource_id = "ewallet.paypal",
                    context = "purchase",
                    sessionId = Guid.NewGuid().ToString(),
                    details = new
                    {
                        dataType = "ewallet_paypal_details",
                        dataOperation = "add",
                        dataCountry = "us",
                        email = "dummy@gmail.com", // lgtm[cs/hard-coded-id]
                        encryptedPassword = "placeholder",
                        authenticationMode = "UsernameAndPassword"
                    },
                };

                this.ExecuteRequest(
                    string.Format("v7.0/{0}/paymentInstrumentsEx?country=us&language=en-US&partner={1}&completePrerequisites=True", this.TestSettings.AccountId, partner),
                    HttpMethod.Post,
                    tc,
                    payload,
                    null,
                    (responseCode, responseBody) =>
                    {
                        Assert.AreEqual(HttpStatusCode.OK, responseCode, string.Format("TestPostPaypalAgainstPIMSEmulator fails for partner {0}, expected response code {1}, actual response {2}", partner, HttpStatusCode.OK, responseCode));
                    });
            }
        }

        /// <summary>
        /// The test is to verify core scenario Post Nonsim for webblends and xbox
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestPostNonSimAgainstPIMSEmulator()
        {
            string[] partners = { TestConstants.PartnerNames.OxoWebDirect, TestConstants.PartnerNames.Webblends, TestConstants.PartnerNames.Xbox };
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.nsm.add.success");
            foreach (string partner in partners)
            {
                object payload = new
                {
                    paymentMethodFamily = "mobile_billing_non_sim",
                    paymentMethodType = "spt-us-nonsim",
                    paymentMethodOpertion = "add",
                    paymentMethodCountry = "us",
                    paymentMethodResource_id = "mobile_billing_non_sim",
                    context = "purchase",
                    sessionId = Guid.NewGuid().ToString(),
                    details = new
                    {
                        dataType = "mobile_billing_non_sim_details",
                        dataOperation = "add",
                        dataCountry = "us",
                        msisdn = "16143957466"
                    },
                };

                this.ExecuteRequest(
                    string.Format("v7.0/{0}/paymentInstrumentsEx?country=us&language=en-US&partner={1}&completePrerequisites=True", this.TestSettings.AccountId, partner),
                    HttpMethod.Post,
                    tc,
                    payload,
                    null,
                    (responseCode, responseBody) =>
                    {
                        Assert.AreEqual(HttpStatusCode.OK, responseCode, string.Format("TestPostNonSimAgainstPIMSEmulator fails for partner {0}, expected response code {1}, actual response {2}", partner, HttpStatusCode.OK, responseCode));
                    });
            }
        }

        /// <summary>
        /// The test is to verify Validate CVV returns 204 for success
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestValidateCvvSuccessAgainstPimsEmulator()
        {
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.pims.validatecvv.success");
            object payload = new
            {
                paymentInstrumentOperation = "validateInstance",
                paymentInstrumentCountry = "us",
                cvvToken = "placeholder",
                sessionId = Guid.NewGuid().ToString(),
                riskData = new
                {
                    greenId = Guid.NewGuid().ToString()
                }
            };

            this.ExecuteRequest(
                string.Format("v7.0/{0}/paymentInstrumentsEx/TestPiid/validateCvv?language=us", this.TestSettings.AccountId),
                HttpMethod.Post,
                tc,
                payload,
                null,
                (responseCode, responseBody) =>
                {
                    Assert.AreEqual(HttpStatusCode.NoContent, responseCode);
                });
        }

        /// <summary>
        /// The test is to verify Validate CVV returns 400 retryable error for invalid cvv
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestValidateCvvInvalidErrorAgainstPimsEmulator()
        {
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.pims.validatecvv.error.invalidcvv");
            object payload = new
            {
                paymentInstrumentOperation = "validateInstance",
                paymentInstrumentCountry = "us",
                cvvToken = "placeholder",
                sessionId = Guid.NewGuid().ToString(),
                riskData = new
                {
                    greenId = Guid.NewGuid().ToString()
                }
            };

            this.ExecuteRequest(
                string.Format("v7.0/{0}/paymentInstrumentsEx/TestPiid/validateCvv?language=us", this.TestSettings.AccountId),
                HttpMethod.Post,
                tc,
                payload,
                null,
                (responseCode, responseBody) =>
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, responseCode);

                    JObject error = JObject.Parse(responseBody);
                    Assert.AreEqual("InvalidCvv", error["ErrorCode"]);
                    Assert.AreEqual("cvvToken", error["Details"][0]["Target"]);
                });
        }

        /// <summary>
        /// The test is to verify Validate CVV returns 400 terminating error for validation failed
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestValidateCvvValidationErrorAgainstPimsEmulator()
        {
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.pims.validatecvv.error.validationfailed");
            object payload = new
            {
                paymentInstrumentOperation = "validateInstance",
                paymentInstrumentCountry = "us",
                cvvToken = "placeholder",
                sessionId = Guid.NewGuid().ToString(),
                riskData = new
                {
                    greenId = Guid.NewGuid().ToString()
                }
            };

            this.ExecuteRequest(
                string.Format("v7.0/{0}/paymentInstrumentsEx/TestPiid/validateCvv?language=us", this.TestSettings.AccountId),
                HttpMethod.Post,
                tc,
                payload,
                null,
                (responseCode, responseBody) =>
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, responseCode);

                    JObject error = JObject.Parse(responseBody);
                    Assert.AreEqual("ValidationFailed", error["ErrorCode"]);
                    Assert.IsNull(error["Details"]);
                });
        }

        /// <summary>
        /// The test is to verify GetByTypeAndPiid challenge endpoint with type = cvv
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestGetChallengeByTypeAndPiidCvv_PIMSEmulator()
        {
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.pims.cc.add.success");
            string[] cvvPartners =
                {
                    TestConstants.PartnerNames.Webblends,
                    TestConstants.PartnerNames.OxoWebDirect,
                    TestConstants.PartnerNames.Cart,
                    TestConstants.PartnerNames.WebblendsInline
                };

            foreach (string partner in cvvPartners)
            {
                this.ExecuteRequest(
                    string.Format("v7.0/{0}/challengeDescriptions?language=en-us&country=us&partner={1}&piid=TestPiid&type=cvv&sessionId=123456", this.TestSettings.AccountId, partner),
                    HttpMethod.Get,
                    tc,
                    null,
                    null,
                    (responseCode, responseBody) =>
                    {
                        Assert.AreEqual(HttpStatusCode.OK, responseCode);
                        Assert.AreEqual("cvv", (string)responseBody.First.identity.type);
                    });
            }
        }

        /// <summary>
        /// The test is to verify GetByTypeAndPiid challenge endpoint with type = sms
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestGetChallengeByTypeAndPiidSms_PIMSEmulator()
        {
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.pims.nsm.add.success");
            string[] smsPartners =
                {
                    TestConstants.PartnerNames.Webblends,
                    TestConstants.PartnerNames.OxoWebDirect,
                    TestConstants.PartnerNames.WebblendsInline
                };

            foreach (string partner in smsPartners)
            {
                this.ExecuteRequest(
                    string.Format("v7.0/{0}/challengeDescriptions?language=en-us&country=us&partner={1}&piid=TestPiid&type=sms&sessionId=123456", this.TestSettings.AccountId, partner),
                    HttpMethod.Get,
                    tc,
                    null,
                    null,
                    (responseCode, responseBody) =>
                    {
                        Assert.AreEqual(HttpStatusCode.OK, responseCode);
                        Assert.AreEqual("sms", (string)responseBody.First.identity.type);
                    });
            }
        }

        /// <summary>
        /// The test is to verify GetByPiidAndSessionId challenge endpoint
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestGetChallengeByPiidAndSessionId_PIMSEmulator()
        {
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.pims.dpa.add.success");
            this.ExecuteRequest(
                string.Format("v7.0/{0}/challengeDescriptions?language=en-us&country=us&piid=TestPiid&sessionId=123456", this.TestSettings.AccountId),
                HttpMethod.Get,
                tc,
                null,
                null,
                (responseCode, responseBody) =>
                {
                    Assert.AreEqual(HttpStatusCode.OK, responseCode);
                    Assert.IsNotNull(responseBody.First.clientAction);
                    Assert.AreEqual("Redirect", (string)responseBody.First.clientAction.type);
                });
        }

        /// <summary>
        /// The test is to verify the connectivity between PX to flighting service
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestFlightingGetPaymentMethodDescription()
        {
            this.ExecuteRequest(
                string.Format("v7.0/{0}/paymentMethodDescriptions?country=us&partner={1}&language=en-US&family=credit_card", this.TestSettings.TestFlightAccountId, TestConstants.PartnerNames.Xbox),
                HttpMethod.Get,
                null,
                null,
                null,
                (responseCode, responseBody, responseHeaders) =>
                {
                    Assert.AreEqual(HttpStatusCode.OK, responseCode);
                    Assert.IsTrue(responseHeaders != null && responseHeaders["x-ms-flight"] != null, "Flight header was not found in the response header");

                    // We always run the experiment to set PXCOTFlightTest for TestFlightAccountId
                    Assert.IsTrue(responseHeaders["x-ms-flight"].Contains("PXCOTFlightTest"), "TestFlight was not found in the x-ms-flights response header");
                });
        }

        /// <summary>
        /// The test is to verify the connectivity between PX and Legacy Commerce Service
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        //// [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestAzurePostPIAgainstPIMSEmulator()
        {
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.pims.cc.add.success");
            object payload = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "visa",
                paymentMethodOperation = "add",
                paymentMethodCountry = "us",

                context = "purchase",
                sessionId = Guid.NewGuid().ToString(),
                details = new
                {
                    accountHolderName = "PX COT",
                    accountToken = "dummyAccountTokenValue",
                    expiryMonth = "3",
                    expiryYear = "2023",
                    cvvToken = "dummyCvvTokenValue",
                    address = new
                    {
                        addressType = "billing",
                        addressOperation = "add",
                        addressCountry = "us",
                        address_line1 = "1 Microsoft Way",
                        city = "redmond",
                        region = "wa",
                        postal_code = "98052",
                        country = "us"
                    }
                }
            };

            Dictionary<string, string> headers = new Dictionary<string, string>()
            {
                { "x-ms-clientcontext-encoding", "base64" },
                { "x-ms-aadinfo", string.Format("altSecId={0}", Convert.ToBase64String(Encoding.UTF8.GetBytes(this.TestSettings.Puid))) }
            };

            this.ExecuteRequest(
                string.Format("v7.0/{0}/paymentInstrumentsEx?country=us&language=en-US&partner=Azure&billableAccountId={1}", this.TestSettings.AccountId, this.TestSettings.LegacyBillableAccountId),
                HttpMethod.Post,
                tc,
                payload,
                headers,
                (responseCode, responseBody) =>
                {
                    Assert.AreEqual(HttpStatusCode.OK, responseCode);
                });
        }

        /// <summary>
        /// The test is to verify the connectivity between PX and Address Enrichment Service
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestAddressEnrichmentService()
        {
            object payload = new
            {
                address_line1 = "1 Amiryan str.y",
                city = "Yerevan",
                country = "AM",
                region = "Yerevan",
                postal_code = "0010"
            };

            this.ExecuteRequest(
                string.Format("v7.0/addresses/ModernValidate?type=soldTo&partner=commercialstores&language=en-us&scenario=suggestAddressesTradeAVS&country=am", this.TestSettings.AccountId),
                HttpMethod.Post,
                null,
                payload,
                null,
                (responseCode, responseBody) =>
                {
                    Assert.AreEqual(HttpStatusCode.OK, responseCode);
                });
        }

        /// <summary>
        /// The test is to verify the connectivity between 
        /// PX and Orchestration service.
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestOrchestrationService_OrchestrationServiceAuth()
        {
            this.ExecuteRequest(
                string.Format("v7.0/{0}/paymentInstrumentsEx/rw5g3QEAAAAeAACA/remove", this.TestSettings.AccountId),
                HttpMethod.Post,
                null,
                null,
                null,
                (responseCode, responseBody) =>
                {
                    var canConnect = responseCode == HttpStatusCode.BadRequest || responseCode == HttpStatusCode.NotFound;
                    Assert.IsTrue(canConnect, "Unable to connect to OrchestrationService.");
                });
        }

        [TestMethod]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestResponseFlightHeader()
        {
            string exposableFlightsHeader = "x-ms-flight";

            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.pims.cc.add.success");
            string[] testPartners =
                {
                    TestConstants.PartnerNames.Webblends,
                    TestConstants.PartnerNames.OxoWebDirect,
                    TestConstants.PartnerNames.Cart,
                    TestConstants.PartnerNames.Xbox,
                    TestConstants.PartnerNames.CommercialStores,
                };

            foreach (string partner in testPartners)
            {
                this.ExecuteRequest(
                    string.Format("v7.0/{0}/paymentmethodDescriptions?family=credit_card&language=en-us&partner={1}&country=us&operation=Add", this.TestSettings.AccountId, partner),
                    HttpMethod.Get,
                    tc,
                    null,
                    null,
                    (responseCode, responseBody, responseHeader) =>
                    {
                        Assert.AreEqual(HttpStatusCode.OK, responseCode);
                        string[] flightHeaders = responseHeader.GetValues(exposableFlightsHeader);
                        Assert.IsNotNull(flightHeaders, "Response headers contains " + exposableFlightsHeader + " header");
                        Assert.AreEqual(1, flightHeaders.Length, "Response headers contains only one " + exposableFlightsHeader + " header");
                    });
            }
        }

        [TestMethod]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void ListPaymentTransactions()
        {
            string puid = "1055518870507325";
            string email = "mstest_pymentsnstar1@outlook.com";

            var headers = new Dictionary<string, string>();
            headers.Add("x-ms-msaprofile", string.Format("PUID={0},emailAddress={1}", ToBase64(puid), ToBase64(email)));
            headers.Add("x-ms-clientcontext-encoding", "base64");
            this.ExecuteRequest(
                string.Format("v7.0/{0}/paymentTransactions", this.TestSettings.AccountId),
                HttpMethod.Get,
                null,
                null,
                headers,
                (responseCode, responseBody) =>
                {
                    Assert.AreEqual(HttpStatusCode.OK, responseCode);
                });
        }

        [TestMethod]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [DataRow(false)]
        [DataRow(true)]
        public void ListPaymentTransactionsPostMethod(bool isFlightEnabled)
        {
            dynamic body = null;
            HttpStatusCode code = HttpStatusCode.Unused;
            var requestHeaders = new Dictionary<string, string>()
            {
                {
                    "x-ms-test", "{ \"scenarios\": \"px.purchasefd.listrx.checkpi.lstorder.success, px.account.customer.profile,px.listrx.products.catologeservice.success\"}"
                },
                {
                    "x-ms-clientcontext-encoding", "base64"
                }
            };

            if (isFlightEnabled)
            {
                requestHeaders.Add("x-ms-flight", "PXEnableSearchTransactionParallelRequest");
            }

            object payload = new
            {
                id = "aw74jAAAAAABAACA",
                cvvToken = "placeholder"
            };
            this.ExecuteRequest(
            string.Format("v7.0/{0}/paymentTransactions?country=us&language=en-US&partner=northstarweb", this.TestSettings.AccountId),
            HttpMethod.Post,
            null,
            payload,
            requestHeaders,
            (responseCode, responseBody) =>
            {
                code = responseCode;
                body = responseBody;
            });
            Assert.AreEqual(HttpStatusCode.OK, code);
            JArray orders = (JArray)body.orders;
            Assert.AreEqual(4, orders.Count);
        }

        [TestMethod]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        public void ListPaymentTransactionsCheckPi_Emulator()
        {
            string puid = "1055518870507325";
            string email = "mstest_pymentsnstar1@outlook.com";

            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.purchasefd.listtrx.checkpi.success");
            var headers = new Dictionary<string, string>();
            headers.Add("x-ms-msaprofile", string.Format("PUID={0},emailAddress={1}", ToBase64(puid), ToBase64(email)));
            headers.Add("x-ms-clientcontext-encoding", "base64");
            dynamic body = null;
            HttpStatusCode code = HttpStatusCode.Unused;

            this.ExecuteRequest(
                string.Format("v7.0/{0}/paymentTransactions", this.TestSettings.TestFlightAccountId),
                HttpMethod.Get,
                tc,
                null,
                headers,
                 (responseCode, responseBody) =>
                 {
                     code = responseCode;
                     body = responseBody;
                 });
            Assert.AreEqual(HttpStatusCode.OK, code);
            JArray orders = (JArray)body.orders;
            JArray subs = (JArray)body.subscriptions;
            Assert.AreEqual(4, orders.Count);
            Assert.AreEqual(1, subs.Count);
        }

        [TestMethod]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        public void ListPaymentTransactionsCheckPi_Emulator_IsBlockingPi()
        {
            string puid = "1055518870507325";
            string email = "mstest_pymentsnstar1@outlook.com";

            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.purchasefd.listtrx.checkpi.isblockingpi");
            var headers = new Dictionary<string, string>();
            headers.Add("x-ms-msaprofile", string.Format("PUID={0},emailAddress={1}", ToBase64(puid), ToBase64(email)));
            headers.Add("x-ms-clientcontext-encoding", "base64");
            dynamic body = null;
            HttpStatusCode code = HttpStatusCode.Unused;

            this.ExecuteRequest(
                string.Format("v7.0/{0}/paymentTransactions", this.TestSettings.TestFlightAccountId),
                HttpMethod.Get,
                tc,
                null,
                headers,
                 (responseCode, responseBody) =>
                 {
                     code = responseCode;
                     body = responseBody;
                 });

            Assert.AreEqual(HttpStatusCode.OK, code);
            JArray orders = (JArray)body.orders;
            JArray subs = (JArray)body.subscriptions;
            Assert.AreEqual(4, orders.Count);
            Assert.AreEqual(4, subs.Count);
        }

        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void ListPaymentTransactions_GetProducts_Emulator()
        {
            string puid = "1055518870507325";
            string email = "mstest_pymentsnstar1@outlook.com";

            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.listtrx.catalogservice.success");
            var headers = new Dictionary<string, string>();
            headers.Add("x-ms-msaprofile", string.Format("PUID={0},emailAddress={1}", ToBase64(puid), ToBase64(email)));
            headers.Add("x-ms-clientcontext-encoding", "base64");
            dynamic body = null;
            HttpStatusCode code = HttpStatusCode.Unused;

            this.ExecuteRequest(
                string.Format("v7.0/{0}/paymentTransactions", this.TestSettings.TestFlightAccountId),
                HttpMethod.Get,
                tc,
                null,
                headers,
                 (responseCode, responseBody) =>
                 {
                     code = responseCode;
                     body = responseBody;
                 });
            Assert.AreEqual(HttpStatusCode.OK, code);
            Assert.AreEqual("Xbox Game Pass Ultimate", body.orders[0].description.ToString());
        }

        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void ListPaymentTransactions_CatalogProductIdNotFound_Emulator()
        {
            string puid = "1055518870507325";
            string email = "mstest_pymentsnstar1@outlook.com";

            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.listtrx.catalogservice.productidnotfoundexception");
            var headers = new Dictionary<string, string>();
            headers.Add("x-ms-msaprofile", string.Format("PUID={0},emailAddress={1}", ToBase64(puid), ToBase64(email)));
            headers.Add("x-ms-clientcontext-encoding", "base64");
            dynamic body = null;
            HttpStatusCode code = HttpStatusCode.Unused;

            this.ExecuteRequest(
                string.Format("v7.0/{0}/paymentTransactions", this.TestSettings.TestFlightAccountId),
                HttpMethod.Get,
                tc,
                null,
                headers,
                 (responseCode, responseBody) =>
                 {
                     code = responseCode;
                     body = responseBody;
                 });
            Assert.AreEqual(HttpStatusCode.OK, code);
        }

        [TestMethod]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        public void ListPaymentTransactionsCheckPi_Emulator_DuplicatedOrderIdInCheckResponses()
        {
            string puid = "1055518870507325";
            string email = "mstest_pymentsnstar1@outlook.com";

            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.purchasefd.listtrx.checkpi.duplicatedresponses");
            var headers = new Dictionary<string, string>();
            headers.Add("x-ms-msaprofile", string.Format("PUID={0},emailAddress={1}", ToBase64(puid), ToBase64(email)));
            headers.Add("x-ms-clientcontext-encoding", "base64");
            dynamic body = null;
            HttpStatusCode code = HttpStatusCode.Unused;

            this.ExecuteRequest(
                string.Format("v7.0/{0}/paymentTransactions", this.TestSettings.TestFlightAccountId),
                HttpMethod.Get,
                tc,
                null,
                headers,
                (responseCode, responseBody) =>
                {
                    code = responseCode;
                    body = responseBody;
                });

            Assert.AreEqual(HttpStatusCode.OK, code);
            JArray orders = (JArray)body.orders;
            Assert.AreEqual(4, orders.Count);
            Assert.AreEqual(body.orders[0].csvTopOffPaymentInstrumentId.ToString(), "lchqggaAAAAIAACA");
        }

        [TestMethod]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        public void ListPaymentTransactionsCheckPi_Emulator_FilterPis()
        {
            string puid = "1055518870507325";
            string email = "mstest_pymentsnstar1@outlook.com";

            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.purchasefd.listtrx.checkpi.filterpis");
            var headers = new Dictionary<string, string>();
            headers.Add("x-ms-msaprofile", string.Format("PUID={0},emailAddress={1}", ToBase64(puid), ToBase64(email)));
            headers.Add("x-ms-clientcontext-encoding", "base64");
            dynamic body = null;
            HttpStatusCode code = HttpStatusCode.Unused;

            this.ExecuteRequest(
                string.Format("v7.0/{0}/paymentTransactions", this.TestSettings.TestFlightAccountId),
                HttpMethod.Get,
                tc,
                null,
                headers,
                (responseCode, responseBody) =>
                {
                    code = responseCode;
                    body = responseBody;
                });

            Assert.AreEqual(HttpStatusCode.OK, code);
        }

        [TestMethod]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        public void ListPaymentTransactionsCheckPi_Emulator_OrderMissingBillingInformation()
        {
            string puid = "1055518870507325";
            string email = "mstest_pymentsnstar1@outlook.com";

            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.purchasefd.listtrx.nullobject");
            var headers = new Dictionary<string, string>();
            headers.Add("x-ms-msaprofile", string.Format("PUID={0},emailAddress={1}", ToBase64(puid), ToBase64(email)));
            headers.Add("x-ms-clientcontext-encoding", "base64");
            dynamic body = null;
            HttpStatusCode code = HttpStatusCode.Unused;
            this.ExecuteRequest(
                string.Format("v7.0/{0}/paymentTransactions", this.TestSettings.TestFlightAccountId),
                HttpMethod.Get,
                tc,
                null,
                headers,
                (responseCode, responseBody) =>
                {
                    code = responseCode;
                    body = responseBody;
                });
            Assert.AreEqual(HttpStatusCode.OK, code);
            JArray orders = (JArray)body.orders;
            Assert.AreEqual(1, orders.Count);
            Assert.AreEqual(body.orders[0].piid.ToString(), "D+-WfwAAAAABAACA");
        }

        /// <summary>
        /// The test is to verify CC GetPaymentMethodDescription when user doesn't have completed profile
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestGetPMD_UPI_AddPIAndGetPM()
        {
            string[] partners = { TestConstants.PartnerNames.Azure, TestConstants.PartnerNames.CommercialStores, TestConstants.PartnerNames.Defaulttemplate };
            foreach (string partner in partners)
            {
                this.ExecuteRequest(
                    string.Format("v7.0/{0}/paymentMethodDescriptions?type=legacy_billdesk_payment&partner={1}&operation=Add&country=IN&language=en-US&family=ewallet", this.TestSettings.AccountId, partner),
                    HttpMethod.Get,
                    null,
                    null,
                    null,
                    (responseCode, responseBody) =>
                    {
                        Assert.AreEqual(HttpStatusCode.OK, responseCode, string.Format("TestGetPMD_UPI_PIMSEmulator fails for partner {0}, expected response code {1}, actual response {2}", partner, HttpStatusCode.OK, responseCode));
                        Assert.AreEqual(responseBody.First.clientAction.type.Value, "ReturnContext");
                        Assert.AreEqual(responseBody.First.clientAction.context.response.paymentMethod.paymentMethodType.Value, "legacy_billdesk_payment");
                        Assert.AreEqual(responseBody.First.clientAction.context.response.paymentMethod.paymentMethodFamily.Value, "ewallet");
                    });
                this.ExecuteRequest(
                    string.Format("v7.0/{0}/paymentMethodDescriptions?partner={1}&operation=Select&country=IN&language=en-US&allowedPaymentMethods=%5B%22credit_card.mc%22%2C%22ewallet.legacy_billdesk_payment%22%2C%22credit_card.visa%22%5D", this.TestSettings.AccountId, partner),
                    HttpMethod.Get,
                    null,
                    null,
                    null,
                    (responseCode, responseBody) =>
                    {
                        Assert.AreEqual(HttpStatusCode.OK, responseCode, string.Format("TestGetPMD_UPI_PIMSEmulator fails for partner {0}, expected response code {1}, actual response {2}", partner, HttpStatusCode.OK, responseCode));                       
                        
                        if (partner == TestConstants.PartnerNames.Defaulttemplate)
                        {
                            Assert.IsNotNull(responseBody.First.displayDescription.First.members[1].possibleOptions.ewallet_legacy_billdesk_payment, "Second member of displayDescription is expected to be ewallet_legacy_billdesk_payment under possible options.");
                            Assert.IsNotNull(responseBody.First.displayDescription.First.members[1].possibleOptions.credit_card_visa_mc, "Second member of displayDescription is expected to be credit_card_visa_mc under possible options.");
                        }
                        else
                        {
                            Assert.IsNotNull(responseBody.First.displayDescription.First.members.First.possibleOptions.ewallet_legacy_billdesk_payment);
                            Assert.IsNotNull(responseBody.First.displayDescription.First.members.First.possibleOptions.credit_card_visa_mc);
                        }
                    });
            }
        }

        /// <summary>
        /// The test is to verify the following two connectivities
        /// 1.PX and Account (Jarvis)
        /// 2.PX and TaxId (PPE are not able to connect TaxId Service)
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestValidateAddressAvsLookup()
        {
            string addressId = "058c70b6-5ae0-57c9-c733-997c2d4740ab";
            string validateAddressTemplate = "v7.0/{0}/addressDescriptions?type=jarvis_v3&partner={1}&operation=ValidateInstance&language=en-us&country={2}&addressId={3}";
            string[] partners = {
                TestConstants.PartnerNames.Amcweb,
                TestConstants.PartnerNames.OxoWebDirect,
                TestConstants.PartnerNames.SetupOffice,
                TestConstants.PartnerNames.SetupOfficeSdx,
                TestConstants.PartnerNames.Xbox,
                TestConstants.PartnerNames.Webblends
            };
            string[] accountIds = {
                this.TestSettings.TestFlightAccountId,
                this.TestSettings.AccountId,
            };

            foreach (string accountId in accountIds)
            {
                foreach (string partner in partners)
                {
                    // 1. For non-us address, return pidl with client action ReturnContext
                    Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.account.v3.address.ca");
                    this.ExecuteRequest(
                    string.Format(validateAddressTemplate, accountId, partner, "us", addressId),
                    HttpMethod.Get,
                    tc,
                    null,
                    null,
                    (responseCode, responseBody) =>
                    {
                        VerifyValidateAddressReturnContext(responseCode, responseBody, addressId, replace: false);
                    });

                    // 2. For US address with 9 digits zip code, return pidl with client action ReturnContext
                    tc = new Common.TestContext("PX.COT", DateTime.Now, "px.account.v3.address.zipcode9digits");
                    this.ExecuteRequest(
                    string.Format(validateAddressTemplate, accountId, partner, "us", addressId),
                    HttpMethod.Get,
                    tc,
                    null,
                    null,
                    (responseCode, responseBody) =>
                    {
                        VerifyValidateAddressReturnContext(responseCode, responseBody, addressId, replace: false);
                    });

                    // 3. For US address interaction required, return one suggested address in pidl page
                    tc = new Common.TestContext("PX.COT", DateTime.Now, "px.account.v3.address.interactionrequired");
                    this.ExecuteRequest(
                    string.Format(validateAddressTemplate, accountId, partner, "us", addressId),
                    HttpMethod.Get,
                    tc,
                    null,
                    null,
                    (responseCode, responseBody) =>
                    {
                        Assert.AreEqual(HttpStatusCode.OK, responseCode);
                        Assert.AreEqual(2, responseBody.Count);
                    });

                    this.ExecuteRequest(
                    $"v7.0/{accountId}/addressesEx?partner={partner}&language=en-us&avsSuggest=False",
                    HttpMethod.Post,
                    tc,
                    new object(),
                    null,
                    (responseCode, responseBody) =>
                    {
                        Assert.AreEqual(HttpStatusCode.OK, responseCode);
                        Assert.AreEqual("ReturnContext", responseBody.clientAction.type.Value);
                        Assert.AreNotEqual(addressId, responseBody.clientAction.context.id.Value);
                    });

                    // 4. For US address having multiple suggested addresses, return 3 suggested address pidl page.
                    tc = new Common.TestContext("PX.COT", DateTime.Now, "px.account.v3.address.multiplesuggestedaddresses");
                    this.ExecuteRequest(
                    string.Format(validateAddressTemplate, accountId, partner, "us", addressId),
                    HttpMethod.Get,
                    tc,
                    null,
                    null,
                    (responseCode, responseBody) =>
                    {
                        Assert.AreEqual(HttpStatusCode.OK, responseCode);

                        // If flighting is on, multiple address will return original address 
                        Assert.IsTrue(4 == responseBody.Count || 1 == responseBody.Count);
                        if (responseBody.Count == 1)
                        {
                            VerifyValidateAddressReturnContext(responseCode, responseBody, addressId, replace: false);
                        }
                    });

                    this.ExecuteRequest(
                    $"v7.0/{accountId}/addressesEx?partner={partner}&language=en-us&avsSuggest=False",
                    HttpMethod.Post,
                    tc,
                    new object(),
                    null,
                    (responseCode, responseBody) =>
                    {
                        Assert.AreEqual(HttpStatusCode.OK, responseCode);
                        Assert.AreEqual("ReturnContext", responseBody.clientAction.type.Value);
                        Assert.AreNotEqual(addressId, responseBody.clientAction.context.id.Value);
                    });

                    // 5. For invalid US address, return user entered address only pidl page.
                    tc = new Common.TestContext("PX.COT", DateTime.Now, "px.account.v3.address.suggestedaddressnone");
                    this.ExecuteRequest(
                        string.Format(validateAddressTemplate, accountId, partner, "us", addressId),
                        HttpMethod.Get,
                        tc,
                        null,
                        null,
                        (responseCode, responseBody) =>
                        {
                            Assert.AreEqual(HttpStatusCode.OK, responseCode);
                            Assert.AreEqual(1, responseBody.Count);
                        });

                    this.ExecuteRequest(
                        $"v7.0/{accountId}/addressesEx?partner={partner}&language=en-us&avsSuggest=False",
                        HttpMethod.Post,
                        tc,
                        new object(),
                        null,
                        (responseCode, responseBody) =>
                        {
                            Assert.AreEqual(HttpStatusCode.OK, responseCode);
                            Assert.AreEqual("ReturnContext", responseBody.clientAction.type.Value);
                            Assert.AreNotEqual(addressId, responseBody.clientAction.context.id.Value);
                        });
                }
            }
        }

        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestPostAddressPOBOXSilientCreation()
        {
            string[] partners = {
                TestConstants.PartnerNames.Webblends,
                TestConstants.PartnerNames.OxoWebDirect,
                TestConstants.PartnerNames.WebblendsInline,
                TestConstants.PartnerNames.Xbox,
                TestConstants.PartnerNames.Cart
            };

            string[] accountIds = {
                this.TestSettings.TestFlightAccountId,
                this.TestSettings.AccountId,
            };
            foreach (string accountId in accountIds)
            {
                foreach (string partner in partners)
                {
                    string addressId = null;
                    var address = new
                    {
                        address_line1 = "PO Box 528",
                        city = "Platteville",
                        region = "CO",
                        postal_code = "80651",
                        country = "US",
                        set_as_default_billing_address = true
                    };

                    this.ExecuteRequest(
                    $"v7.0/{accountId}/addressesEx?partner={partner}&language=en-us&avsSuggest=true",
                    HttpMethod.Post,
                    null,
                    address,
                    null,
                    (responseCode, responseBody) =>
                    {
                        Assert.AreEqual(HttpStatusCode.OK, responseCode);
                        Assert.AreEqual(true, (bool)responseBody.set_as_default_billing_address);
                        Assert.AreEqual(address.address_line1, responseBody.address_line1.ToString());
                        Assert.AreEqual(address.city, responseBody.city.ToString());
                        Assert.AreEqual(address.region, responseBody.region.ToString());
                        Assert.AreEqual("80651-0528", responseBody.postal_code.ToString());
                        Assert.IsNotNull(responseBody.id.ToString());
                        addressId = responseBody.id.ToString();
                    });

                    this.ExecuteRequest(
                    $"v7.0/{accountId}/addressesEx/{addressId}",
                    HttpMethod.Get,
                    null,
                    null,
                    null,
                    (responseCode, responseBody) =>
                    {
                        Assert.AreEqual(HttpStatusCode.OK, responseCode);
                        Assert.AreEqual(address.address_line1, responseBody.address_line1.ToString());
                        Assert.AreEqual(address.city, responseBody.city.ToString());
                        Assert.AreEqual(address.region, responseBody.region.ToString());
                        Assert.AreEqual("80651-0528", responseBody.postal_code.ToString());
                        Assert.AreEqual(addressId, responseBody.id.ToString());
                        Assert.AreEqual(true, (bool)responseBody.is_zip_plus_4_present);
                    });
                }
            }
        }

        /// <summary>
        /// The test is to verify we can update both default billing and shipping by Jarvis profile patch 
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        [DataRow("storify")]
        [DataRow("xboxsettings")]
        [DataRow("xboxsubs")]
        [DataRow("saturn")]
        public void TestPostAddressNoAvsSuggestedSetDefaultAddressAndSetDefaultShippingAddress(string partner)
        {
            var address = new
            {
                address_line1 = "1 Microsoft Way",
                city = "Redmond",
                region = "WA",
                postal_code = "98052-8300",
                country = "US",
                set_as_default_billing_address = true,
                set_as_default_shipping_address = true // only xbox native partners take adding shipping address to profile 
            };

            PostAddressAndVerify(
                        this.TestSettings.AccountId,
                        partner,
                        address,
                        false);
        }

        /// <summary>
        /// The test is to verify post address flow works 
        /// when no need to set default billing and shipping
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestPostAddressNoAvsSuggested()
        {
            string[] partners =
            {
                TestConstants.PartnerNames.Cart,
                TestConstants.PartnerNames.Xbox,
                TestConstants.PartnerNames.Webblends,
            };

            string[] accountIds =
            {
                this.TestSettings.AccountId,
            };

            var address = new
            {
                address_line1 = "1 Microsoft Way",
                city = "Redmond",
                region = "WA",
                postal_code = "98004-5124",
                country = "US",
                set_as_default_billing_address = false,
                set_as_default_shipping_address = false
            };

            foreach (string accountId in accountIds)
            {
                foreach (string partner in partners)
                {
                    PostAddressAndVerify(
                        this.TestSettings.AccountId,
                        partner,
                        address,
                        false);
                }
            }
        }

        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestPostAddressAvsSuggestedSetDefaultAddress()
        {
            string[] partners =
            {
                TestConstants.PartnerNames.Webblends
            };

            string[] accountIds =
            {
                this.TestSettings.AccountId,
            };

            var bellevueAddress = new
            {
                address_line1 = "555 110th Ave NE",
                city = "Bellevue",
                region = "WA",
                postal_code = "98004-5124",
                country = "US",
                set_as_default_billing_address = true,
                set_as_default_shipping_address = false
            };

            var redmondAddress = new
            {
                address_line1 = "1 Microsoft Way",
                city = "Redmond",
                region = "WA",
                postal_code = "98052-8300",
                country = "US",
                set_as_default_billing_address = true,
                set_as_default_shipping_address = false
            };

            // post 2 different addresses to ensure the address is successfully set to be default one. 
            PostAddressAndVerify(
                this.TestSettings.AccountId,
                TestConstants.PartnerNames.Webblends,
                bellevueAddress,
                true);

            PostAddressAndVerify(
                this.TestSettings.AccountId,
                TestConstants.PartnerNames.Webblends,
                redmondAddress,
                true);

            PostAddressAndVerify(
                this.TestSettings.AccountId,
                TestConstants.PartnerNames.Webblends,
                bellevueAddress,
                false);

            PostAddressAndVerify(
                this.TestSettings.AccountId,
                TestConstants.PartnerNames.Webblends,
                redmondAddress,
                false);
        }

        private void PostAddressAndVerify(
            string accountId,
            string partner,
            dynamic expectedAddress,
            bool avsSuggest)
        {
            this.ExecuteRequest(
                        $"v7.0/{accountId}/addressesEx?partner={partner}&language=en-us&avsSuggest={avsSuggest.ToString().ToUpper()}",
                        HttpMethod.Post,
                        null,
                        (object)expectedAddress,
                        null,
                        (responseCode, responseBody) =>
                        {
                            Assert.AreEqual(HttpStatusCode.OK, responseCode);
                            var savedAddress = avsSuggest ? responseBody : responseBody.clientAction.context;
                            Assert.AreEqual(accountId, savedAddress.customer_id.Value);
                            Assert.AreEqual(expectedAddress.set_as_default_billing_address, (bool)savedAddress.set_as_default_billing_address.Value);
                            Assert.AreEqual(expectedAddress.set_as_default_shipping_address, (bool)savedAddress.set_as_default_shipping_address.Value);
                            Assert.AreEqual(expectedAddress.country, (string)savedAddress.country);
                            Assert.AreEqual(expectedAddress.city, (string)savedAddress.city);
                            Assert.AreEqual(expectedAddress.region, (string)savedAddress.region);
                            Assert.AreEqual(expectedAddress.postal_code, (string)savedAddress.postal_code);
                            Assert.AreEqual(expectedAddress.address_line1, (string)savedAddress.address_line1);
                        });

        }

        // <summary>
        /// AAD Auth using first party application ID should return unauthorized as this appID is no longer whitelisted. Test only valid in INT environmnet. 
        /// </summary>
        [Ignore]
        [TestMethod]
        [TestCategory(TestCategory.INTContinuousTest)]
        public void TestAADAuth_FirstPartyCaller_NotAllowed()
        {
            // Multiple Requests to test Latency
            for (int i = 0; i < TestConstants.AuthenticationTestConstants.NumAuthenticationRequests; i++)
            {
                Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.pims.cc.add.success");
                this.ExecuteRequest(
                        string.Format("v7.0/{0}/challengeDescriptions?language=en-us&country=us&partner={1}&piid=DummyPiidFirstPartyCaller&type=cvv&sessionId=45678", this.TestSettings.AccountId, TestConstants.PartnerNames.Webblends),
                        HttpMethod.Get,
                        tc,
                        null,
                        null,
                        (responseCode, responseBody) =>
                        {
                            Assert.AreEqual(HttpStatusCode.Unauthorized, responseCode);
                        },
                        authType: Constants.AuthenticationType.AAD,
                        aadClientType: Constants.AADClientType.FirstParty);
            }
        }

        /// <summary>
        /// AAD Auth using third party application (COT) to call PX (first party). Test should hit MISE Token Validation logic. Test not configured in OneBox environmnet.  
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestAADAuth_PMECaller()
        {
            // Multiple Requests to test Latency
            for (int i = 0; i < TestConstants.AuthenticationTestConstants.NumAuthenticationRequests; i++)
            {
                Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.pims.cc.add.success");
                this.ExecuteRequest(
                        string.Format("v7.0/{0}/challengeDescriptions?language=en-us&country=us&partner={1}&piid==DummyPiidPMECaller&type=cvv&sessionId=123456", this.TestSettings.AccountId, TestConstants.PartnerNames.Webblends),
                        HttpMethod.Get,
                        tc,
                        null,
                        null,
                        (responseCode, responseBody) =>
                        {
                            Assert.AreEqual(HttpStatusCode.OK, responseCode);
                            Assert.AreEqual("cvv", (string)responseBody.First.identity.type);
                        },
                        authType: Constants.AuthenticationType.AAD,
                        aadClientType: Constants.AADClientType.PME);
            }
        }

        /// <summary>
        /// AAD Auth using PX INT PME COT account to call PX itself with not well formed token, 
        /// It is to test when token auth failed, we can safely fall back to certificate
        /// Test not configured in OneBox environmnet. To be removed once PIFD switches to AAD tokens. 
        /// </summary>
        [Ignore]
        [TestMethod]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestListPI_AADAuthFallsBackToCert_PMECaller()
        {
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.pims.cc.add.success");
            this.ExecuteRequest(
                    string.Format("v7.0/{0}/challengeDescriptions?language=en-us&country=us&partner={1}&piid==DummyPiidNotWellFormedJWT&type=cvv&sessionId=123456", this.TestSettings.AccountId, TestConstants.PartnerNames.Webblends),
                    HttpMethod.Get,
                    tc,
                    null,
                    null,
                    (responseCode, responseBody) =>
                    {
                        Assert.AreEqual(HttpStatusCode.OK, responseCode);
                        Assert.AreEqual("cvv", (string)responseBody.First.identity.type);
                    },
                    authType: Constants.AuthenticationType.TestAADFallsBackToCert,
                    aadClientType: Constants.AADClientType.PME);
        }

        /// <summary>
        /// Test when there is no aad token nor cert. Test not configured in OneBox environmnet. 
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestAADAuth_NoAADAndCert()
        {
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.pims.cc.add.success");
            this.ExecuteRequest(
                    string.Format("v7.0/{0}/challengeDescriptions?language=en-us&country=us&partner={1}&piid=NOAADNOCERT&type=cvv&sessionId=123456", this.TestSettings.AccountId, TestConstants.PartnerNames.Webblends),
                    HttpMethod.Get,
                    tc,
                    null,
                    null,
                    (responseCode, responseBody) =>
                    {
                        Assert.AreEqual(HttpStatusCode.Unauthorized, responseCode);
                    },
                    authType: Constants.AuthenticationType.NONE);
        }

        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void India3DSChallengeTest()
        {
            // Arrange
            string accountId = "ec8c235c-65e2-4a3d-bd7d-a20ed8ec1688";
            string partner = TestConstants.PartnerNames.Azure;
            string currency = "INR";
            string country = "in";
            string piid = "Account001-Pi001-Visa";
            string language = "en-us";
            string testScenario = "px.transactionservice.success";

            var paymentSessionData = new PaymentSessionData()
            {
                PaymentInstrumentId = piid,
                Language = language,
                Partner = partner,
                Amount = 800,
                Currency = currency,
                Country = country,
                ChallengeScenario = ChallengeScenario.PaymentTransaction
            };

            // Act - CreatePaymentSession
            var paymentSession = CreatePaymentSession(accountId, paymentSessionData, testScenario);

            // Assert
            Assert.AreEqual(true, paymentSession.IsChallengeRequired);
            Assert.AreEqual("India3DSChallenge", paymentSession.ChallengeType);
            Assert.AreEqual(PaymentChallengeStatus.Unknown, paymentSession.ChallengeStatus);

            // Arrange
            string urlTemplateHandlePaymentChallenge = "v7.0/{0}/challengeDescriptions?paymentSessionOrData={1}";
            var headers = new Dictionary<string, string>();
            headers.Add("x-ms-test", "{scenarios: \"" + testScenario + "\", contact: \"px.cot\"}");

            // Act - HandlePaymentChallenge
            dynamic paymentChallengeResponse = null;
            HttpStatusCode statusCode = HttpStatusCode.Unused;
            this.ExecuteRequest(
                string.Format(urlTemplateHandlePaymentChallenge, accountId, JsonConvert.SerializeObject(paymentSession)),
                HttpMethod.Get,
                null,
                null,
                headers,
                (responseCode, responseBody) =>
                {
                    statusCode = responseCode;
                    paymentChallengeResponse = responseBody;
                });

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, statusCode);

            Assert.IsNotNull(paymentChallengeResponse);
            Assert.AreEqual("cvv", paymentChallengeResponse[0]["identity"]["type"].Value);

            // Arrange
            PIDLData sampleCvv = new PIDLData();
            sampleCvv.Add("cvvToken", "123");

            string urlAuthenticateIndia3DS = "/v7.0/{0}/paymentSessions/{1}/AuthenticateIndiaThreeDS";

            // Act - AuthenticateIndiaThreeDS
            dynamic authenticateIndiaThreeDResponse = null;
            this.ExecuteRequest(
               string.Format(urlAuthenticateIndia3DS, accountId, paymentSession.Id),
               HttpMethod.Post,
               null,
               sampleCvv,
               headers,
               (responseCode, responseBody) =>
               {
                   statusCode = responseCode;
                   authenticateIndiaThreeDResponse = responseBody;
               });

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, statusCode);
            Assert.AreEqual("Pidl", authenticateIndiaThreeDResponse["clientAction"]["type"].Value);
        }

        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void ThirdPartyGuestCheckout_Stripe_InMeeting_Emulator()
        {
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.3pp.stripe.guestcheckout.success,px.sellermarket.stripe.us");

            string paymentProviderId = "stripe";
            string displayContent = "You are paying USD 10.01 to Peaceful Yoga";
            string context = "InMeeting";
            ThirdPartyGuestCheckout_CreditCard(tc, paymentProviderId, context, displayContent);
        }

        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void ThirdPartyGuestCheckout_Stripe_PrepaidMeeting_Emulator()
        {
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.3pp.stripe.guestcheckout.prepaid.success,px.sellermarket.stripe.us,px.pims.3pp.stripe.guestcheckout.success");

            string paymentProviderId = "stripe";
            string displayContent = "You are paying USD 10.01 to Peaceful Yoga";
            string context = "PrepaidMeeting";
            ThirdPartyGuestCheckout_CreditCard(tc, paymentProviderId, context, displayContent);
        }

        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void ThirdPartyGuestCheckout_Paypal_InMeeting_Emulator()
        {
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.3pp.paypal.guestcheckout.success,px.sellermarket.paypal.us");

            // The user loads guest checkout page - payment selection
            string paymentProviderId = "paypal";
            string paymentInfo = "You are paying USD 10.01 to Peaceful Yoga";
            int paymentInfoIndex = 0;
            string context = "InMeeting";

            //  Paypal payment seletion
            ThirdPartyGuestCheckout_Paypal_PaymentSelection(tc, context, paymentInfo, paymentInfoIndex, null);

            // Paypal account
            ThirdPartyGuestCheckout_Paypal(tc, context, paymentInfo, paymentInfoIndex, null);

            // Paypal credit card 
            ThirdPartyGuestCheckout_CreditCard(tc, paymentProviderId, context, paymentInfo);
        }

        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void ThirdPartyGuestCheckout_Paypal_PrepaidMeeting_Emulator()
        {
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.3pp.paypal.guestcheckout.prepaid.success,px.sellermarket.paypal.us");

            // The user loads guest checkout page - payment selection
            string paymentProviderId = "paypal";
            string productDescription = "Yoga class for beginners – level 0-5";
            string paymentInfo = "You are paying USD 10.01 to Peaceful Yoga";
            int paymentInfoIndex = 1;
            string context = "PrepaidMeeting";

            //  Paypal payment seletion
            ThirdPartyGuestCheckout_Paypal_PaymentSelection(tc, context, paymentInfo, paymentInfoIndex, productDescription);

            // Paypal account
            ThirdPartyGuestCheckout_Paypal(tc, context, paymentInfo, paymentInfoIndex, productDescription);

            // Paypal credit card
            ThirdPartyGuestCheckout_CreditCard(tc, paymentProviderId, context, productDescription);
        }

        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        public void ApplyPaymentInstrument_Web_WithSession_IssuerServiceEmulator()
        {
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.issuerservice.default");

            EventTraceActivity traceActivityID = new EventTraceActivity();

            var headers = new Dictionary<string, string>();
            headers.Add("x-ms-msaprofile", "PUID=123456789");

            var payload = new InitializeRequest
            {
                Card = "XboxCreditCard",
                Market = "us",
                Channel = "TestChannel",
                ReferrerId = "TestReferrer"
            };

            var requestUrl = "/v7.0/paymentInstrumentsEx?operation=apply&country=us&language=en-us&partner=xboxweb&sessionId=3348324";
            this.ExecuteRequest(
                requestUrl,
                HttpMethod.Post,
                tc,
                payload,
                headers,
                    (responseCode, responseBody) =>
                    {
                        Assert.AreEqual(HttpStatusCode.OK, responseCode);
                    });

        }

        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        public void ApplyPaymentInstrument_Web_WithoutSession_IssuerServiceEmulator()
        {
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.issuerservice.default");

            EventTraceActivity traceActivityID = new EventTraceActivity();

            var headers = new Dictionary<string, string>();
            headers.Add("x-ms-msaprofile", "PUID=123456789");

            var payload = new InitializeRequest
            {
                Card = "XboxCreditCard",
                Market = "us",
                Channel = "TestChannel",
                ReferrerId = "TestReferrer"
            };

            var requestUrl = "/v7.0/paymentInstrumentsEx?operation=apply&country=us&language=en-us&partner=xboxweb";
            this.ExecuteRequest(
                requestUrl,
                HttpMethod.Post,
                tc,
                payload,
                headers,
                    (responseCode, responseBody) =>
                    {
                        Assert.AreEqual(HttpStatusCode.OK, responseCode);
                    });

        }

        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        public void ApplyPaymentMethodDescriptions_Web_IssuerServiceEmulator()
        {
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.issuerservice.default");

            EventTraceActivity traceActivityID = new EventTraceActivity();

            var headers = new Dictionary<string, string>();
            headers.Add("x-ms-msaprofile", "PUID=123456789");

            var requestUrl = string.Format("/v7.0/{0}/paymentMethodDescriptions?operation=apply&country=us&language=en-us&partner=xboxweb&family=credit_card&type=mc&channel=TestChannel&referrerId=TestReferrer", this.TestSettings.AccountId);
            this.ExecuteRequest(
                requestUrl,
                HttpMethod.Get,
                tc,
                null,
                headers,
                    (responseCode, responseBody) =>
                    {
                        Assert.AreEqual(HttpStatusCode.OK, responseCode);
                    });

        }

        private void ThirdPartyGuestCheckout_Paypal_PaymentSelection(Common.TestContext tc, string context, string paymentInfo, int paymentInfoIndex, string productDescription)
        {
            // Arrange
            string checkoutId = "123";
            string country = "sj";
            List<string> partners = new List<string> { "msteams", "defaulttemplate" };
            var headers = new Dictionary<string, string>();
            dynamic body = null;
            HttpStatusCode code = HttpStatusCode.Unused;

            // Act
            foreach (string partner in partners)
            {
                this.ExecuteRequest(
                    $"/v7.0/checkoutDescriptions?operation=RenderPidlPage&checkoutId={checkoutId}&partner={partner}&paymentProviderId=paypal&redirectUrl=pay.microsoft.com&country={country}",
                    HttpMethod.Get,
                    tc,
                    null,
                    headers,
                        (responseCode, responseBody) =>
                        {
                            code = responseCode;
                            body = responseBody;

                            // Assert
                            Assert.AreEqual(HttpStatusCode.OK, code);
                            Assert.AreEqual("credit_card_visa_amex_mc_discover", (string)body[0].identity.resource_id);
                            Assert.AreEqual("ewallet_paypal", (string)body[1].identity.resource_id);

                            if (context.Equals("PrepaidMeeting"))
                            {
                                // Verify product description are displayed as expected
                                Assert.AreEqual(productDescription, (string)body[0].displayDescription[0].members[0].members[2].displayContent);
                                Assert.AreEqual(productDescription, (string)body[1].displayDescription[0].members[0].members[2].displayContent);
                            }

                            Assert.AreEqual(paymentInfo, (string)body[0].displayDescription[0].members[paymentInfoIndex].members[0].displayContent);
                            Assert.AreEqual(paymentInfo, (string)body[1].displayDescription[0].members[paymentInfoIndex].members[0].displayContent);
                        });
            }
        }

        private void ThirdPartyGuestCheckout_CreditCard(Common.TestContext tc, string paymentProviderId, string context, string displayContent)
        {
            // Arrange
            string checkoutId = "123";
            string country = "sj";
            List<string> partners = new List<string> { "msteams", "defaulttemplate" };
            var headers = new Dictionary<string, string>();
            dynamic body = null;
            HttpStatusCode code = HttpStatusCode.Unused;

            foreach (string partner in partners)
            {
                string url = $"/v7.0/checkoutDescriptions?operation=RenderPidlPage&checkoutId={checkoutId}&partner={partner}&paymentProviderId={paymentProviderId}&redirectUrl=pay.microsoft.com&country={country}";

                if (paymentProviderId.Equals("paypal"))
                {
                    url += "&family=credit_card&scenario=pidlClientAction&type=visa,amex,mc,discover&language=en-us";
                }

                // Act - The user loads guest checkout page            
                this.ExecuteRequest(
                    url,
                    HttpMethod.Get,
                    tc,
                    null,
                    headers,
                     (responseCode, responseBody) =>
                     {
                         code = responseCode;
                         body = responseBody;

                         // Assert
                         Assert.AreEqual(HttpStatusCode.OK, code);

                         Assert.AreEqual("paymentMethod", (string)body.First.identity.description_type);
                         Assert.AreEqual("credit_card", (string)body.First.identity.family);

                         if (context.Equals("PrepaidMeeting"))
                         {
                             if (paymentProviderId.Equals("stripe"))
                             {
                                 Assert.AreEqual("Event details", (string)body.First.displayDescription[0].members[0].members[1].displayContent);
                                 Assert.AreEqual(displayContent, (string)body.First.displayDescription[0].members[1].members[0].displayContent);
                             }
                             else if (paymentProviderId.Equals("paypal"))
                             {
                                 // Verify that payment details including the amount are displayed as expected
                                 Assert.AreEqual(displayContent, (string)body[0].displayDescription[0].members[0].members[2].displayContent);
                                 Assert.AreEqual(displayContent, (string)body[1].displayDescription[0].members[0].members[2].displayContent);
                                 Assert.AreEqual(displayContent, (string)body[2].displayDescription[0].members[0].members[2].displayContent);
                                 Assert.AreEqual(displayContent, (string)body[3].displayDescription[0].members[0].members[2].displayContent);
                             }
                         }
                         else if (context.Equals("InMeeting"))
                         {
                             if (paymentProviderId.Equals("stripe"))
                             {
                                 // Verify that payment details including the amount are displayed as expected
                                 Assert.AreEqual(displayContent, (string)body.First.displayDescription[0].members[0].members[0].displayContent);
                             }
                             else if (paymentProviderId.Equals("paypal"))
                             {
                                 // Verify that payment details including the amount are displayed as expected
                                 Assert.AreEqual(displayContent, (string)body[0].displayDescription[0].members[0].members[0].displayContent);
                                 Assert.AreEqual(displayContent, (string)body[1].displayDescription[0].members[0].members[0].displayContent);
                                 Assert.AreEqual(displayContent, (string)body[2].displayDescription[0].members[0].members[0].displayContent);
                                 Assert.AreEqual(displayContent, (string)body[3].displayDescription[0].members[0].members[0].displayContent);
                             }
                         }
                     });

                // The user submits credit card information to pay
                var payload = new
                {
                    context = "purchase",
                    paymentMethodFamily = "credit_card",
                    paymentMethodType = "visa",
                    emailAddress = "test@microsoft.com",
                    paymentMethodCountry = "sj",
                    details = new
                    {
                        accountHolderName = "test test",
                        accountToken = "dummytoken",
                        cvvToken = "dummytoken",
                        expiryMonth = "11",
                        expiryYear = "2033",
                        address = new
                        {
                            // postal code is intentionally not included since it is optional.
                            country = "sj",
                        }
                    }
                };

                this.ExecuteRequest(
                       $"v7.0/checkoutsEx/{checkoutId}/charge?partner={partner}&paymentProviderId={paymentProviderId}&redirectUrl=pay.microsoft.com",
                       HttpMethod.Post,
                       tc,
                       payload,
                       null,
                       (responseCode, responseBody) =>
                        {
                            body = responseBody;
                            Assert.AreEqual(HttpStatusCode.OK, responseCode);
                            Assert.AreEqual("Redirect", body.clientAction.type.ToString());
                        });
            }
        }

        private void ThirdPartyGuestCheckout_Paypal(Common.TestContext tc, string context, string paymentInfo, int paymentInfoIndex, string productDescription)
        {
            // Arrange
            string checkoutId = "123";
            string country = "sj";
            List<string> partners = new List<string> { "msteams", "defaulttemplate" };
            var headers = new Dictionary<string, string>();
            dynamic body = null;
            HttpStatusCode code = HttpStatusCode.Unused;

            foreach (string partner in partners)
            {

                // Act - The user loads guest checkout page - if user selected payment method family  as paypal            
                this.ExecuteRequest(
                    $"/v7.0/checkoutDescriptions?operation=RenderPidlPage&checkoutId={checkoutId}&partner={partner}&paymentProviderId=paypal&redirectUrl=pay.microsoft.com&country={country}&family=ewallet&scenario=pidlClientAction&type=paypal&language=en-us",
                    HttpMethod.Get,
                    tc,
                    null,
                    headers,
                     (responseCode, responseBody) =>
                     {
                         code = responseCode;
                         body = responseBody;

                         // Assert
                         Assert.AreEqual(HttpStatusCode.OK, code);
                         Assert.AreEqual("paymentMethod", (string)body.First.identity.description_type);
                         Assert.AreEqual("ewallet", (string)body.First.identity.family);
                         Assert.AreEqual("ewallet.paypalRedirect", (string)body[0].identity.resource_id);

                         if (context.Equals("PrepaidMeeting"))
                         {
                             // Verify product description are displayed as expected
                             Assert.AreEqual(productDescription, (string)body[0].displayDescription[0].members[0].members[2].displayContent);
                         }

                         // Verify that payment details including the amount are displayed as expected
                         Assert.AreEqual(paymentInfo, (string)body[0].displayDescription[0].members[paymentInfoIndex].members[0].displayContent);
                     });
            }
        }

        private PaymentSession CreatePaymentSession(string accountId, PaymentSessionData data, string scenario, string flightName = null)
        {
            string urlTemplateCreatePaymentSession = "v7.0/{0}/paymentSessionDescriptions?paymentSessionData={1}";

            Common.TestContext tc = new Common.TestContext(
                    contact: "px.cot",
                    retention: DateTime.MaxValue,
                    scenarios: scenario);

            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(flightName))
            {
                headers.Add(flightName, "base64");
            }

            HttpStatusCode code = HttpStatusCode.Unused;
            dynamic body = null;
            this.ExecuteRequest(
                string.Format(
                    urlTemplateCreatePaymentSession,
                    accountId,
                    JsonConvert.SerializeObject(data)),
                HttpMethod.Get,
                tc,
                null,
                headers,
                (responseCode, responseBody) =>
                {
                    code = responseCode;
                    body = responseBody;
                });

            Assert.AreEqual(HttpStatusCode.OK, code);

            var pidlRes = RetrievePaymentSessionFromPIDLResource(body);
            return JsonConvert.DeserializeObject<PaymentSession>(pidlRes.ClientAction.Context.ToString());
        }

        private Task<PaymentSession> HandlePaymentChallenge(string accountId, PaymentSessionData data)
        {
            string urlTemplateHandlePaymentChallenge = "v7.0/{0}/challengeDescriptions?paymentSessionOrData={1}";

            var headers = new Dictionary<string, string>();
            headers.Add("x-ms-clientcontext-encoding", "base64");
            headers.Add("x-ms-msaprofile", "PUID = OTg1MTU0MzIwMzI3ODI0, emailAddress = a293c2hpa190ZXN0MTFAb3V0bG9vay5jb20=");
            return Task.Run(() =>
            {
                dynamic body = null;
                HttpStatusCode code = HttpStatusCode.Unused;
                this.ExecuteRequest(
                    string.Format(urlTemplateHandlePaymentChallenge, accountId, JsonConvert.SerializeObject(data)),
                    HttpMethod.Get,
                    null,
                    null,
                    headers,
                    (responseCode, responseBody) =>
                    {
                        code = responseCode;
                        body = responseBody;
                    });

                Assert.AreEqual(HttpStatusCode.OK, code);
                var pidlRes = RetrievePaymentSessionFromPIDLResource(body);
                return JsonConvert.DeserializeObject<PaymentSession>(((PIDLResource)pidlRes).ClientAction.Context.ToString());
            });
        }

        private static PIDLResource RetrievePaymentSessionFromPIDLResource(dynamic body)
        {
            var jsonResponse = body as JArray;
            var pidlResource = JsonConvert.DeserializeObject<PIDLResource>(JsonConvert.SerializeObject(jsonResponse[0]));
            return pidlResource;
        }

        private void VerifyValidateAddressReturnContext(HttpStatusCode responseCode, dynamic responseBody, string addressId, bool replace)
        {
            Assert.AreEqual(HttpStatusCode.OK, responseCode);
            Assert.AreEqual(1, responseBody.Count);
            Assert.AreEqual("ReturnContext", responseBody.First.clientAction.type.Value);
            if (replace)
            {
                Assert.AreNotEqual(addressId, responseBody.First.clientAction.context.id.Value);
            }
            else
            {
                Assert.AreEqual(addressId, responseBody.First.clientAction.context.id.Value);
            }
        }

        private string ToBase64(string value)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(plainTextBytes);
        }

        /// <summary>
        /// The test is to verify core scenario Post CC for webblends and xbox
        /// </summary>
        [TestMethod]
        public void TestICMSetupByTrigger500OnPX()
        {
            string[] partners = { TestConstants.PartnerNames.OxoWebDirect, TestConstants.PartnerNames.Webblends, TestConstants.PartnerNames.Xbox };
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.pims.cc.add.500");
            foreach (string partner in partners)
            {
                object payload = new
                {
                    paymentMethodFamily = "credit_card",
                    paymentMethodType = "visa",
                    paymentMethodOpertion = "add",
                    paymentMethodCountry = "us",
                    paymentMethodResource_id = "credit_card.visa",
                    context = "purchase",
                    sessionId = Guid.NewGuid().ToString(),
                    riskData = new
                    {
                        dataType = "payment_method_riskData",
                        dataOperation = "add",
                        dataCountry = "us",
                        greenId = "bb606624-79b0-401b-920a-ce3b66861462"
                    },
                    details = new
                    {
                        dataType = "credit_card_visa_details",
                        dataOperation = "add",
                        dataCountry = "us",
                        accountHolderName = "PX Test",
                        accountToken = "AAkHGDuto7uVF5D/FACaO3SzcjKtE4sR7r98MmS91ILE",
                        expiryMonth = "2",
                        expiryYear = "2020",
                        cvvToken = "placeholder",
                        address = new
                        {
                            addressType = "billing",
                            addressOperation = "add",
                            addressCountry = "us",
                            address_line1 = "1st Street",
                            city = "Baggs",
                            region = "wy",
                            postal_code = "82321",
                            country = "us"
                        },
                        permission = new
                        {
                            dataType = "permission_details",
                            dataOperation = "add",
                            dataCountry = "us",
                            hmac = string.Empty
                        }
                    },
                };

                this.ExecuteRequest(
                    string.Format("v7.0/{0}/paymentInstrumentsEx?country=us&language=en-US&partner={1}&completePrerequisites=True", this.TestSettings.AccountId, partner),
                    HttpMethod.Post,
                    tc,
                    payload,
                    null,
                    (responseCode, responseBody) =>
                    {
                        Assert.AreEqual(HttpStatusCode.InternalServerError, responseCode, string.Format("TestPostCCAgainstPIMSEmulator fails for partner {0}, expected response code {1}, actual response {2}", partner, HttpStatusCode.OK, responseCode));
                    });
            }
        }

        /// <summary>
        /// Invalid ChekoutStatus indicates terminal error and user will be redirected to redirectUrl
        /// No checkoutform should be rendered
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void ThirdPartyGuestCheckout_Invalid_InMeeting_Emulator()
        {
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.3pp.stripe.guestcheckout.invalid,px.sellermarket.stripe.us");
            var headers = new Dictionary<string, string>();
            dynamic body = null;
            HttpStatusCode code = HttpStatusCode.Unused;

            // The user loads guest checkout page 
            string checkoutId = "123";
            string country = "sj";
            this.ExecuteRequest(
                $"/v7.0/checkoutDescriptions?operation=RenderPidlPage&checkoutId={checkoutId}&partner=msteams&paymentProviderId=stripe&redirectUrl=pay.microsoft.com&country={country}",
                HttpMethod.Get,
                tc,
                null,
                headers,
                 (responseCode, responseBody) =>
                 {
                     code = responseCode;
                     body = responseBody;
                     Assert.AreEqual(HttpStatusCode.OK, code);
                     Assert.AreEqual("Redirect", (string)body.First.clientAction.type);
                     Assert.AreEqual("pay.microsoft.com", (string)body.First.clientAction.context.baseUrl);
                 });
        }

        /// <summary>
        /// Failed ChekoutStatus indicates retriable error and checkout form should be rendered for user to retry
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void ThirdPartyGuestCheckout_Failed_InMeeting_Emulator()
        {
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.3pp.stripe.guestcheckout.failed,px.sellermarket.stripe.us");
            var headers = new Dictionary<string, string>();
            dynamic body = null;
            HttpStatusCode code = HttpStatusCode.Unused;

            // The user loads guest checkout page 
            string checkoutId = "123";
            string country = "sj";
            this.ExecuteRequest(
                $"/v7.0/checkoutDescriptions?operation=RenderPidlPage&checkoutId={checkoutId}&partner=msteams&paymentProviderId=stripe&redirectUrl=pay.microsoft.com&country={country}",
                HttpMethod.Get,
                tc,
                null,
                headers,
                 (responseCode, responseBody) =>
                 {
                     code = responseCode;
                     body = responseBody;
                     Assert.AreEqual(HttpStatusCode.OK, code);
                     Assert.AreEqual("paymentMethod", (string)body.First.identity.description_type);
                     Assert.AreEqual("credit_card", (string)body.First.identity.family);

                     // Verify that payment details including the amount are displayed as expected
                     Assert.AreEqual("You are paying USD 10.01 to Peaceful Yoga", (string)body.First.displayDescription[0].members[0].members[0].displayContent);
                 });

            // The user submits credit card information to pay
            var payload = new
            {
                context = "purchase",
                paymentMethodFamily = "credit_card",
                paymentMethodType = "visa",
                emailAddress = "test@microsoft.com",
                paymentMethodCountry = "sj",
                details = new
                {
                    accountHolderName = "test test",
                    accountToken = "dummytoken",
                    cvvToken = "dummytoken",
                    expiryMonth = "11",
                    expiryYear = "2033",
                    address = new
                    {
                        // postal code is intentionally not included since it is optional.
                        country = "sj",
                    }
                }

            };

            this.ExecuteRequest(
                   $"v7.0/checkoutsEx/{checkoutId}/charge?partner=msteams&paymentProviderId=stripe&redirectUrl=pay.microsoft.com",
                   HttpMethod.Post,
                   tc,
                   payload,
                   null,
                   (responseCode, responseBody) =>
                   {
                       body = responseBody;
                       Assert.AreEqual(HttpStatusCode.OK, responseCode);
                       Assert.AreEqual("tppcheckouterrorpidl", (string)body.clientAction.context[0].identity.type);
                   });
        }

        /// <summary>
        /// The test is to verify the connectivity between PX to HIP service
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestHIPService()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>()
            {
                { "x-ms-flight", "PXEnableHIPCaptcha,PXEnableHIPCaptchaGroup" }
            };

            this.ExecuteRequest(
                string.Format("v7.0/{0}/paymentMethodDescriptions?partner=cart&operation=Add&language=en-US&family=credit_card&country=us", this.TestSettings.AccountId),
                HttpMethod.Get,
                null,
                null,
                headers,
                (responseCode, responseBody) =>
                {
                    Assert.AreEqual(HttpStatusCode.OK, responseCode);
                    Assert.IsNotNull(responseBody.First.data_description["captchaId"]);
                    Assert.IsNotNull(responseBody.First.data_description["captchaReg"]);
                    Assert.IsNotNull(responseBody.First.data_description["details"].First.data_description["captchaSolution"]);
                    Assert.IsNotNull(responseBody.First.data_description["details"].First.data_description["currentContext"]);
                    //// PXEnableHIPCaptchaGroup flight is enabled 100% for Cart on PROD
                    Assert.AreEqual(responseBody.First.displayDescription.First.members[14].displayId.Value, "captchaGroup");
                });

            object payload = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "visa",
                captchaId = "0e90810e-c3e6-4d20-8102-b8dd94da3100",
                captchaReg = "EastUS",
                details = new
                {
                    captchaSolution = "5RLLHQW",
                    currentContext = "{\"id\":\"credit_card.\",\"instance\":null,\"backupId\":null,\"backupInstance\":null,\"action\":\"addResource\",\"paymentMethodFamily\":\"credit_card\",\"paymentMethodType\":null,\"resourceActionContext\":{\"action\":\"addResource\",\"pidlDocInfo\":{\"anonymousPidl\":false,\"resourceType\":\"paymentMethod\",\"parameters\":{\"partner\":\"cart\",\"operation\":\"Add\",\"country\":\"us\",\"language\":\"en-US\",\"family\":\"credit_card\"}},\"pidlIdentity\":null,\"resourceInfo\":null,\"resourceObjPath\":null,\"resource\":null,\"prefillData\":null},\"partnerHints\":null,\"prefillData\":null,\"targetIdentity\":null}"
                }
            };

            this.ExecuteRequest(
                string.Format("v7.0/{0}/paymentInstrumentsEx?country=us&language=en-US&partner=cart", this.TestSettings.AccountId),
                HttpMethod.Post,
                null,
                payload,
                headers,
                (responseCode, responseBody) =>
                {
                    Assert.AreEqual(HttpStatusCode.OK, responseCode);
                    Assert.IsNotNull(responseBody.clientAction.context.resourceActionContext.pidlDocInfo);
                    Assert.AreEqual(responseBody.clientAction.context.resourceActionContext.action.Value, "addResource");
                    Assert.AreEqual(responseBody.clientAction.pidlRetainUserInput.Value, true);
                    Assert.IsNotNull(responseBody.clientAction.pidlError);
                });
        }

        /// <summary>
        /// The test is to verify the connectivity between PX to MSRewards service emulator
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestMSRewardsServiceEmulator()
        {
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.msrewards.success,px.pims.listpi.success");
            this.SendRequestToMSRewardsService(tc, "select");
            this.SendRequestToMSRewardsService(tc, "redeem");
        }

        /// <summary>
        /// The test is to verify the connectivity between PX to MSRewards service
        /// </summary>
        [Ignore]
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestMSRewardsService()
        {
            this.SendRequestToMSRewardsService(null, "select");
            this.SendRequestToMSRewardsService(null, "redeem");
        }

        private void SendRequestToMSRewardsService(Common.TestContext tc, string operation)
        {
            this.ExecuteRequest(
                string.Format("v7.0/{0}/rewardsDescriptions?type=MSRewards&rewardsContextData=%7B%22orderAmount%22%3A%222%22%2C%22currency%22%3A%22usd%22%7D&partner=windowsstore&operation={1}&language=en-US&country=us", this.TestSettings.AccountId, operation),
                HttpMethod.Get,
                tc,
                null,
                null,
                (responseCode, responseBody) =>
                {
                    Assert.AreEqual(HttpStatusCode.OK, responseCode, $"v7.0/userId/rewardsDescriptions failed with an internal error: {JsonConvert.SerializeObject(responseBody)}");
                    Assert.AreEqual("rewards", (string)responseBody.First.identity.description_type);
                    Assert.AreEqual(operation, (string)responseBody.First.identity.operation);
                    if (string.Equals("select", operation, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Assert.AreEqual("selectMSRewardsPage", (string)responseBody.First.displayDescription[0].displayId);
                    }
                    else if (string.Equals("redeem", operation, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Assert.AreEqual("redeemMSRewardsPage", (string)responseBody.First.displayDescription[0].displayId);
                    }
                        
                });
        }

        /// <summary>
        /// The test is to verify the successful redemption of a CSV token in PX
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestValidateCSVTokenEmulator()
        {
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.partnersettings.windowsstore,px.pims.ewallet.giftcard,px.tops.csvtoken.success,px.purchasefd.redeemcsv.success");
            this.SendRequestToValidateCSVToken(tc);
        }

        /// <summary>
        /// The test is to verify the successful redemption of a Non-CSV token in PX
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestRedeemNonCSVTokenEmulator()
        {
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.partnersettings.windowsstore,px.pims.ewallet.giftcard,px.tops.othertoken.success,px.purchasefd.redeemcsv.success,px.catalogservice.purchaseproduct.success");
            this.SendRequestToValidateCSVToken(tc);
        }

        private void SendRequestToValidateCSVToken(Common.TestContext tc)
        {
            var payload = new
            {
                paymentMethodFamily = "ewallet",
                paymentMethodType = "stored_value",
                tokenIdentifierValue = "asdfg-asdfg-asdfg-asdfg-asdfg",
                actionType = "validate"
            };

            Dictionary<string, string> headers = new Dictionary<string, string>()
            {
                { "x-ms-flight", "PXDisablePSSCache" }
            };

            this.ExecuteRequest(
                string.Format("v7.0/{0}/paymentInstrumentsEx?country=us&language=en-US&partner=windowsstore", this.TestSettings.AccountId),
                HttpMethod.Post,
                tc,
                payload,
                headers,
                (responseCode, responseBody) =>
                {
                    Assert.AreEqual(HttpStatusCode.OK, responseCode, $"v7.0/userId/paymentInstrumentsEx failed with an internal error: {JsonConvert.SerializeObject(responseBody)}");
                    Assert.AreEqual("Pidl", responseBody.clientAction.type.Value);
                });
        }


        /// <summary>
        /// The test is to verify the connectivity between PX to PartnerSettings service emulator
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestPartnerSettingsServiceEmulator()
        {
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.partnersettings.success");
            this.SendRequestToPartnerSettingsService(tc);
        }

        /// <summary>
        /// The test is to verify the connectivity between PX to PartnerSettings service
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestPartnerSettingsService()
        {
            this.SendRequestToPartnerSettingsService(null);
        }

        private void SendRequestToPartnerSettingsService(Common.TestContext tc)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>()
            {
                { "x-ms-flight", "enablePaymentMethodGrouping,PXUsePartnerSettingsService" }
            };

            this.ExecuteRequest(
                string.Format("v7.0/{0}/paymentMethodDescriptions?partner=pxcot&operation=select&language=en-US&country=us", this.TestSettings.AccountId),
                HttpMethod.Get,
                tc,
                null,
                headers,
                (responseCode, responseBody) =>
                {
                    Assert.AreEqual(HttpStatusCode.OK, responseCode);
                    Assert.AreEqual("paymentMethod", (string)responseBody.First.identity.description_type);
                    Assert.AreEqual("select", (string)responseBody.First.identity.operation);

                    Assert.AreEqual("paymentMethodSelectPMGroupingPage", (string)responseBody.First.displayDescription[0].displayId);
                });
        }

        /// <summary>
        /// Test not configured in OneBox environmnet.  
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void GetPaymentMethodDescriptions_SearchTransaction()
        {
            // Multiple Requests to test Latency
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.pims.listpi.success");
            this.ExecuteRequest(
                    string.Format("v7.0/{0}/paymentMethodDescriptions?family=credit_card&partner=northstarweb&operation=searchTransactions&country=us&language=en-Us", this.TestSettings.AccountId),
                    HttpMethod.Get,
                    tc,
                    null,
                    null,
                    (responseCode, responseBody) =>
                    {
                        Assert.AreEqual(HttpStatusCode.OK, responseCode);
                        Assert.IsNotNull(responseBody.First.data_description["paymentInstrumentOperation"]);
                        Assert.IsNotNull(responseBody.First.data_description["paymentInstrumentCountry"]);
                        Assert.IsNotNull(responseBody.First.data_description["id"]);
                        Assert.IsNotNull(responseBody.First.data_description["cvvToken"]);
                        Assert.AreEqual("paymentInstrumentSearchTransactionsId", responseBody.First.displayDescription[0].members[0].displayId.Value);
                        Assert.AreEqual("newPaymentMethodLinkGroup", responseBody.First.displayDescription[0].members[1].displayId.Value);
                        Assert.AreEqual("newPaymentStaticText", responseBody.First.displayDescription[0].members[1].members[0].displayId.Value);
                        Assert.AreEqual("newPaymentStatementLink", responseBody.First.displayDescription[0].members[1].members[1].displayId.Value);
                        Assert.AreEqual("paymentInstrumentSearchTransactionsSubHeading", responseBody.First.displayDescription[0].members[2].displayId.Value);
                        Assert.AreEqual("paymentInstrumentSearchTransactionsCvv", responseBody.First.displayDescription[0].members[3].displayId.Value);
                        Assert.AreEqual("searchSubmitButtonGroup", responseBody.First.displayDescription[0].members[4].displayId.Value);
                    });
        }

        /// <summary>
        /// The test is to verify the CMS(Challenge Management Service) flights
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        public void TestCMSFlightingGetPaymentMethodDescription()
        {
            var requestHeaders = new Dictionary<string, string>()
            {
                {
                    "x-ms-test", "{ \"scenarios\": \"px.pims.cc.add.success\" }"
                }
            };

            this.ExecuteRequest(
                string.Format("v7.0/{0}/paymentMethodDescriptions?country=us&partner={1}&language=en-US&family=credit_card", this.TestSettings.TestFlightAccountId, TestConstants.PartnerNames.Xbox),
                HttpMethod.Get,
                null,
                null,
                requestHeaders,
                (responseCode, responseBody, responseHeaders) =>
                {
                    Assert.AreEqual(HttpStatusCode.OK, responseCode);
                    Assert.IsTrue(responseHeaders != null && responseHeaders["x-ms-flight"] != null, "Flight header was not found in the response header");
                    Assert.IsTrue(responseHeaders["x-ms-flight"].Contains("PXChallengeSwitch"), "PXChallengeSwitch flight was not found in the x-ms-flights response header");
                });
        }
    }

}