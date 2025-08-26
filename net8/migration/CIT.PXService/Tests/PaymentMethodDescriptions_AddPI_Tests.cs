// <copyright file="PaymentMethodDescriptions_AddPI_Tests.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace CIT.PXService.Tests
{
    using global::Tests.Common.Model.Pidl;
    using Microsoft.Commerce.Payments.PXService.Model.PXInternal;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    [TestClass]
    public class PaymentMethodDescriptions_AddPI_Tests : TestBase
    {
        [DataRow(GlobalConstants.CountryCodes.China, GlobalConstants.Partners.Azure, "visa,mc,unionpay_international", "add", true)]
        [DataRow(GlobalConstants.CountryCodes.China, GlobalConstants.Partners.Azure, "visa,mc", "add")]

        [DataRow(GlobalConstants.CountryCodes.China, GlobalConstants.Partners.CommercialStores, "visa,mc,unionpay_international", "add", true)]
        [DataRow(GlobalConstants.CountryCodes.China, GlobalConstants.Partners.CommercialStores, "visa,mc", "add")]

        [DataRow(GlobalConstants.CountryCodes.China, GlobalConstants.Partners.DefaultTemplate, "visa,mc,unionpay_international", "add", true)]
        [DataRow(GlobalConstants.CountryCodes.China, GlobalConstants.Partners.DefaultTemplate, "visa,mc", "add")]
        [DataTestMethod]
        public async Task GetPaymentMethodDescriptions_CUPCreditCard_TypesAreAsExpected(string country, string partner, string allowedPMs, string operation, bool unionPayflight = false)
        {
            var flights = "PXUsePartnerSettingsService,vnext";
            if (unionPayflight)
            {
                flights += ",PXEnableCUPInternational";
            }

            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "x-ms-flight", flights }
            };

            // feature is enabled for countries br, us, in
            string expectedPSSResponse = "{\"selectinstance\":{\"template\":\"listpidropdown\",\"redirectionPattern\":null,\"resources\":null,\"features\":null},\"default\":{\"template\":\"" + partner + "\",\"redirectionPattern\":null,\"resources\":null,\"features\":{\"chinaAllowVisaMasterCard\":{\"applicableMarkets\":[\"cn\"],\"displayCustomizationDetail\":null}}}}";
            if (partner.Equals(GlobalConstants.Partners.DefaultTemplate, System.StringComparison.OrdinalIgnoreCase))
            {
                expectedPSSResponse = "{\"add\":{\"template\":\"" + partner + "\",\"redirectionPattern\":\"inline\",\"resources\":{\"paymentMethod\":{\"credit_card\":{\"template\":\"" + partner + "\",\"redirectionPattern\":\"inline\"}}},\"features\":{\"chinaAllowVisaMasterCard\":{\"applicableMarkets\":[\"cn\"],\"displayCustomizationDetail\":null},\"threeDSOne\":{\"applicableMarkets\":[\"in\"],\"displayCustomizationDetail\":null}}},\"selectinstance\":{\"template\":\"listpidropdown\",\"redirectionPattern\":null,\"resources\":null,\"features\":null},\"default\":{\"template\":\"" + partner + "\",\"redirectionPattern\":null,\"resources\":null,\"features\":{\"chinaAllowVisaMasterCard\":{\"applicableMarkets\":[\"cn\"],\"displayCustomizationDetail\":null}}}}";
            }

            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            List<PIDLResource> pidls = await GetPidlFromPXService(
                string.Format(
                    "/v7.0/Account002/paymentMethodDescriptions?country={0}&partner={1}&family=credit_card&type={2}&operation={3}",
                    country,
                    partner,
                    allowedPMs,
                    operation),
                additionaHeaders: headers);

            // Assert
            Assert.IsNotNull(pidls, "Pidl is expected to be not null");

            if (unionPayflight)
            {
                Assert.AreEqual(3, pidls.Count, "3 payment methods are expected");
                Assert.IsTrue(pidls[2].Identity.ContainsValue("credit_card.unionpay_international"), "UnionPay International is expected to be present");
                Assert.IsNotNull(pidls[2].GetDisplayHintById("cardNumberCupInternational"), "UnionPay International card number is expected to be present");

                // defaultTemplate has been updated for CUP international to use the same cvv as visa and mc etc.
                if (partner.Equals(GlobalConstants.Partners.DefaultTemplate, System.StringComparison.OrdinalIgnoreCase))
                {
                    Assert.IsNotNull(pidls[2].GetDisplayHintById("cvv"), "UnionPay International CVV is expected to be present");
                }
                else
                {
                    Assert.IsNotNull(pidls[2].GetDisplayHintById("cvvCupInternational"), "UnionPay International CVV is expected to be present");
                }
            }
            else
            {
                Assert.AreEqual(2, pidls.Count, "2 payment methods are expected");
            }
        }

        [DataRow("battlenet", true)]
        [DataRow("battlenet", false)]
        [DataRow("otherthan_battlenet", true)]
        [DataRow("otherthan_battlenet", false)]
        [DataTestMethod]
        public async Task GePaymentMethodDescriptions_RemoveAddressFieldsValidationForCC(string partner, bool enableFeature)
        {
            // Arrange
            string[] countries = "us,br,gb,jp,fr,ca,in,ph,th,kr,cl,it,tw,pe,sg,nl,my,pt,ng,be,hk,ch,fi,ie,no,xk,gt,iq,sk,rs,ao".Split(',');
            List<string> addressProperties = new List<string>() { "address_line1", "address_line2", "address_line3", "city", "region", "postal_code", "country" };
            Dictionary<string, string> headers = new Dictionary<string, string>() { { "x-ms-flight", "PXDisablePSSCache" } };

            foreach (string country in countries)
            {
                string expectedPSSResponse = "{\"add\":{\"template\":\"defaulttemplate\"}}";
                if (enableFeature)
                {
                    expectedPSSResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"features\":{\"removeAddressFieldsValidationForCC\":{\"applicableMarkets\":[]}}}}";
                }

                PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse, urlPattern: $".*{partner}.*");

                // Act
                string requestUrl = $"/v7.0/Account001/paymentMethodDescriptions?partner={partner}&operation=add&country={country}&language=en-US&family=credit_card&type=amex%2Cvisa%2Cmc%2Cdiscover%2Cjcb";
                List<PIDLResource> pidls = await GetPidlFromPXService(requestUrl, additionaHeaders: headers);
                TestContext.WriteLine($"Url: {requestUrl}");

                // Assert
                Assert.IsNotNull(pidls, "Pidls expected to not be null");

                foreach (var pidl in pidls)
                {
                    foreach (string addressProperty in addressProperties)
                    {
                        PropertyDisplayHint addressPropertyDisplayHint = pidl.GetDisplayHintByPropertyName(addressProperty) as PropertyDisplayHint;
                        PropertyDescription addressPropertyDescription = pidl.GetPropertyDescriptionByPropertyName(addressProperty);

                        if (addressPropertyDisplayHint != null)
                        {
                            if (enableFeature)
                            {
                                Assert.IsNull(addressPropertyDisplayHint.MaxLength, "Max length for address property expected to be null");
                                Assert.IsNull(addressPropertyDisplayHint.MinLength, "Min length for address property expected to be null");
                                Assert.IsNull(addressPropertyDisplayHint.DisplayErrorMessages, "DisplayErrorMessages for address property expected to be null");
                                Assert.IsNull(addressPropertyDisplayHint.PossibleOptions, "PossibleOptions for address property expected to be null");
                                Assert.IsNull(addressPropertyDisplayHint.PossibleValues, "PossibleValues for address property expected to be null");
                            }
                            else
                            {
                                Assert.IsTrue(addressPropertyDisplayHint.DisplayErrorMessages != null || addressPropertyDescription.IsOptional == true, "DisplayErrorMessages for address property expected to be not null");
                                if (string.Equals(addressProperty, "country"))
                                {
                                    Assert.IsNotNull(addressPropertyDisplayHint.PossibleOptions, $"{addressProperty}'s PossibleOptions for address property expected to be not null");
                                    Assert.IsNotNull(addressPropertyDisplayHint.PossibleValues, $"{addressProperty}'s PossibleValues for address property expected to be not null");
                                }
                            }
                        }

                        if (enableFeature)
                        {
                            Assert.IsNotNull(addressPropertyDescription, $"{addressProperty}'s is expected to have DataDescription.");
                            Assert.IsTrue(addressPropertyDescription.IsOptional, "Address fields expected to be optional.");
                            Assert.IsTrue(addressPropertyDescription.Validations.Count == 1, "Address fields expected to have one regex validation.");
                            Assert.AreEqual("^(.*?)$", addressPropertyDescription.Validation.Regex, "Address field Regex is not as expected.");
                        }
                    }
                }

                PXSettings.PartnerSettingsService.ResetToDefaults();
            }
        }

        [DataRow(GlobalConstants.CountryCodes.Japan, GlobalConstants.Partners.Cart, "paypay", false, "PayPay", true, false, false, false, true)]
        [DataRow(GlobalConstants.CountryCodes.Japan, GlobalConstants.Partners.Cart, "paypay", true, "PayPay", false, true, false, true, true)]
        [DataRow(GlobalConstants.CountryCodes.HongKong, GlobalConstants.Partners.Cart, "alipayhk", false, "AlipayHK", true, false, false, false, true)]
        [DataRow(GlobalConstants.CountryCodes.HongKong, GlobalConstants.Partners.Cart, "alipayhk", true, "AlipayHK", false, true, false, true, true)]
        [DataRow(GlobalConstants.CountryCodes.Philippines, GlobalConstants.Partners.Cart, "gcash", false, "GCash", true, false, false, false, true)]
        [DataRow(GlobalConstants.CountryCodes.Philippines, GlobalConstants.Partners.Cart, "gcash", true, "GCash", false, true, false, true, true)]
        [DataRow(GlobalConstants.CountryCodes.Thailand, GlobalConstants.Partners.Cart, "truemoney", false, "TrueMoney", true, false, false, false, true)]
        [DataRow(GlobalConstants.CountryCodes.Thailand, GlobalConstants.Partners.Cart, "truemoney", true, "TrueMoney", false, true, false, true, true)]
        [DataRow(GlobalConstants.CountryCodes.Malaysia, GlobalConstants.Partners.Cart, "touchngo", false, "Touch 'n Go", true, false, false, false, true)]
        [DataRow(GlobalConstants.CountryCodes.Malaysia, GlobalConstants.Partners.Cart, "touchngo", true, "Touch 'n Go", false, true, false, true, true)]
        [DataRow(GlobalConstants.CountryCodes.Japan, GlobalConstants.Partners.NorthstarWeb, "paypay", true, "PayPay", false, true, true, true, true)]
        [DataRow(GlobalConstants.CountryCodes.HongKong, GlobalConstants.Partners.NorthstarWeb, "alipayhk", true, "AlipayHK", false, true, true, true, true)]
        [DataRow(GlobalConstants.CountryCodes.Philippines, GlobalConstants.Partners.NorthstarWeb, "gcash", true, "GCash", false, true, true, true, true)]
        [DataRow(GlobalConstants.CountryCodes.Thailand, GlobalConstants.Partners.NorthstarWeb, "truemoney", true, "TrueMoney", false, true, true, true, true)]
        [DataRow(GlobalConstants.CountryCodes.Malaysia, GlobalConstants.Partners.NorthstarWeb, "touchngo", true, "Touch 'n Go", false, true, true, true, true)]
        [DataTestMethod]
        public async Task GetPaymentMethodDescriptions_Add_ANTBatchPI_AreAsExpected(string country, string partner, string type, bool enabledPSS, string expectedType, bool isChangeSettingsExpected, bool isLogoExpected, bool isHeadingExpected, bool isPaymentOptionExpected, bool enabelPI)
        {
            var flights = "enablePaymentMethodGrouping,vnext";
            string ipAddress = "111.111.111.111";
            string xBoxDeviceInfo = "ipAddress={0},xboxLiveDeviceId={1}";
            string otherDeviceInfo = "ipAddress={0},userAgent={1}";

            List<Tuple<string, string, string>> userAgents = new List<Tuple<string, string, string>>
            {
                { new Tuple<string, string, string>("windows", "{Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/11.11 (KHTML, like Gecko) Chrome/111.111.111.111 Safari/11.11 Edg/111.111.111.111}", "Web") },
                { new Tuple<string, string, string>("iPhone", "{Mozilla/5.0 (iPhone; CPU iPhone OS 17_3 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.3 Mobile/15E148 Safari/604.1}", "MobileApp") },
                { new Tuple<string, string, string>("Android", "{Mozilla/5.0 (Linux; Android 10; K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Mobile Safari/537.36}", "MobileApp") },
                { new Tuple<string, string, string>("xBox", "18230582934452242973", "GameConsole") }
            };

            if (enabelPI)
            {
                flights += ",PXEnable" + (expectedType.Equals("Touch 'n Go") ? "TouchNGo" : expectedType);
            }

            foreach (var userAgent in userAgents)
            {
                string encodedIpAddress = System.Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(ipAddress));
                string encodedUserAgent = System.Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(userAgent.Item2));

                string requestUrl = string.Format("/v7.0/Account002/paymentMethodDescriptions?country={0}&partner={1}&operation=add&family=ewallet&type={2}", country, partner, type);

                if (enabledPSS)
                {
                    flights += ",PXUsePartnerSettingsService";
                    string expectedPSSResponse = "{\"add\":{\"template\":\"" + partner + "\",\"redirectionPattern\":\"inline\",\"resources\":{\"paymentMethod\":{\"credit_card\":{\"template\":\"" + partner + "\",\"redirectionPattern\":\"inline\"},\"ewallet." + type + "\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\"}}},\"features\":{\"threeDSOne\":{\"applicableMarkets\":[\"in\"],\"displayCustomizationDetail\":null},\"hideElement\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"hidepaymentOptionSaveText\":" + (!isPaymentOptionExpected).ToString().ToLower() + ",\"hideChangeSettingText\":" + (!isChangeSettingsExpected).ToString().ToLower() + ",\"hidePaymentMethodHeading\":" + (!isHeadingExpected).ToString().ToLower() + ",\"moveCardNumberBeforeCardHolderName\":false,\"setSaveButtonDisplayContentAsNext\":false,\"updateCvvChallengeTextForGCO\":false,\"enableCountryAddorUpdateCC\":false,\"hideFirstAndLastNameForCompletePrerequisites\":false,\"hideAddCreditDebitCardHeading\":false,\"setGroupedSelectOptionTextBeforeLogo\":false,\"enableSecureFieldAddCC\":false,\"setSaveButtonDisplayContentAsBook\":false,\"removeCancelButton\":false,\"setBackButtonDisplayContentAsCancel\":false,\"hidePaymentSummaryText\":false,\"addressSuggestionMessage\":false}]}}}, \"selectinstance\":{\"template\":\"listpidropdown\",\"redirectionPattern\":null,\"resources\":null,\"features\":null},\"select\":{\"template\":\"selectpmbuttonlist\",\"features\":{\"paymentMethodGrouping\":{\"applicableMarkets\":[]}}},\"default\":{\"template\":\"" + partner + "\",\"redirectionPattern\":null,\"resources\":null,\"features\":null}}";
                    PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);
                }

                string deviceInfo = userAgent.Item1.Equals("xBox") ? string.Format(xBoxDeviceInfo, encodedIpAddress, encodedUserAgent) : string.Format(otherDeviceInfo, encodedIpAddress, encodedUserAgent);

                Dictionary<string, string> headers = new Dictionary<string, string>
                {
                    { "x-ms-flight", flights },
                    { "x-ms-deviceinfo", deviceInfo },
                    { "x-ms-clientcontext-encoding", "base64" }
                };

                List<PIDLResource> pidls = await GetPidlFromPXService(requestUrl, statusCode: enabelPI ? System.Net.HttpStatusCode.OK : System.Net.HttpStatusCode.BadRequest, additionaHeaders: headers);

                // Assert
                Assert.IsNotNull(pidls, "Pidl is expected to be not null");

                foreach (var pidl in pidls)
                {
                    var displayHint = pidl.GetDisplayHintById("antBatchPiText3") as TextDisplayHint;
                    Assert.AreEqual(displayHint.DisplayContent, $", you'll be redirected to {expectedType} to scan a QR code with a mobile device or log in to verify your account. We'll collect your {expectedType} account info, but won't use it without your permission.");

                    var headingDisplayHint = pidl.GetDisplayHintById("paymentMethodHeading") as HeadingDisplayHint;
                    var actualHeadingExpected = headingDisplayHint == null ? false : !(headingDisplayHint.IsHidden.HasValue && headingDisplayHint.IsHidden.Value);
                    Assert.AreEqual(isHeadingExpected, actualHeadingExpected, "Add account heading should not be expected for cart partner");

                    var amcDisplayHint = pidl.GetDisplayHintById("paymentChangeSettingsTextGroup") as TextGroupDisplayHint;
                    var actualChangeSettingsExpected = amcDisplayHint == null ? false : !(amcDisplayHint.IsHidden.HasValue && amcDisplayHint.IsHidden.Value);
                    Assert.AreEqual(isChangeSettingsExpected, actualChangeSettingsExpected, "account.microsoft.com should be expected for cart partner");

                    var logoDisplayHint = pidl.GetDisplayHintById(type + "Logo") as LogoDisplayHint;
                    var actualLogoExpected = logoDisplayHint == null ? false : !(logoDisplayHint.IsHidden.HasValue && logoDisplayHint.IsHidden.Value);
                    Assert.AreEqual(isLogoExpected, actualLogoExpected, "logo should not be expected for cart partner");

                    var paymentOptionDisplayHint = pidl.GetDisplayHintById("paymentOptionSaveText") as TextDisplayHint;
                    var actualPaymentOptionExpected = paymentOptionDisplayHint == null ? false : !(paymentOptionDisplayHint.IsHidden.HasValue && paymentOptionDisplayHint.IsHidden.Value);
                    Assert.AreEqual(isPaymentOptionExpected, actualPaymentOptionExpected, "Payment option text should not be expected for cart partner");

                    var channel = pidl.GetPropertyDescriptionByPropertyName("channel");
                    Assert.AreEqual(userAgent.Item3, channel.DefaultValue, "Device class is expected to be as per user agent");

                    if (string.Equals(type, "paypay", StringComparison.OrdinalIgnoreCase))
                    {
                        var purchaseTextHint = pidl.GetDisplayHintById("purchaseText") as TextDisplayHint;

                        if (enabledPSS)
                                                    {
                            Assert.IsNotNull(purchaseTextHint, "PayPay purchase text should be present");
                        }
                        else
                        {
                            Assert.IsNull(purchaseTextHint, "PayPay purchase text should not be present when PSS is not enabled");
                        }
                    }
                }
            }
        }

        [DataRow("commercialstores", "virtual", "invoice_basic")]
        [DataRow("commercialstores", "virtual", "invoice_check")]
        [DataRow("northstarweb", "invoice_credit", "klarna")]
        [DataRow("storify", "invoice_credit", "klarna")]
        [DataRow("amcweb", "invoice_credit", "klarna")]
        [DataTestMethod]
        public async Task GetPaymentMethodDescriptions_ValidateRegEx_PhoneNumber(string partner, string family, string type)
        {
            // Arrange
            string[] countries = type.Equals("klarna") ? new string[] { "dk" } : new string[] { "us", "cn", "ca", "de", "gb" };
            string propertyName = type.Equals("klarna") ? "phone" : "phone_number";

            foreach (string country in countries)
            {
                string url = $"/v7.0/Account001/paymentMethodDescriptions?country={country}&family={family}&type={type}&language=en-US&partner={partner}&operation=add";

                // Act
                List<PIDLResource> pidls = await GetPidlFromPXService(url);

                // Assert
                Assert.IsNotNull(pidls, "Pidl is expected to be not null");

                // Not checking if the key exists before accessing is intentional. If the country key is not,
                // the test should fail and dictoionary should be updated with country value.
                List<string> validPhoneNumbers = Constants.TestValidPhoneNumbersByCountry[country];
                foreach (string validPhoneNumber in validPhoneNumbers)
                {
                    ValidatePidlPropertyRegex(pidls[0], propertyName, validPhoneNumber, true);
                }

                // If the country key is not present then use the common invalid phone numbers
                List<string> invalidPhoneNumbers = Constants.TestInvalidPhoneNumbersByCountry.ContainsKey(country) ? Constants.TestInvalidPhoneNumbersByCountry[country] : Constants.TestInvalidPhoneNumbersByCountry["common"];
                foreach (string invalidPhoneNumber in invalidPhoneNumbers)
                {
                    ValidatePidlPropertyRegex(pidls[0], propertyName, invalidPhoneNumber, false);
                }
            }
        }

        [DataRow(GlobalConstants.Partners.Azure, false, false, true, false)]
        [DataRow(GlobalConstants.Partners.Azure, false, false, true, true)]
        [DataRow(GlobalConstants.Partners.Azure, true, true, true, false)]
        [DataRow(GlobalConstants.Partners.Azure, true, true, true, true)]
        [DataRow(GlobalConstants.Partners.CommercialStores, false, false, true, false)]
        [DataRow(GlobalConstants.Partners.CommercialStores, false, false, true, true)]
        [DataRow(GlobalConstants.Partners.CommercialStores, true, true, true, true)]
        [DataRow(GlobalConstants.Partners.CommercialStores, true, true, true, false)]
        [DataRow(GlobalConstants.Partners.MacManage, true, true, true, true)]
        [DataRow(GlobalConstants.Partners.MacManage, true, true, true, false)]
        [DataTestMethod]
        public async Task GetPaymentMethodDescriptions_Add_AlipayCN_AreAsExpected(string partner, bool enabledPSS, bool isHeadingExpected, bool enabelPI, bool limitTextExpected)
        {
            var flights = "enablePaymentMethodGrouping,vnext";
            string ipAddress = "111.111.111.111";
            string xBoxDeviceInfo = "ipAddress={0},xboxLiveDeviceId={1}";
            string otherDeviceInfo = "ipAddress={0},userAgent={1}";

            if (enabelPI)
            {
                flights += ",PXEnableAlipayCN";
            }

            if (limitTextExpected)
            {
                flights += ",PXEnableAlipayCNLimitText";
            }

            List<Tuple<string, string, string>> userAgents = new List<Tuple<string, string, string>>
            {
                { new Tuple<string, string, string>("windows", "{Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/11.11 (KHTML, like Gecko) Chrome/111.111.111.111 Safari/11.11 Edg/111.111.111.111}", "Web") },
                { new Tuple<string, string, string>("iPhone", "{Mozilla/5.0 (iPhone; CPU iPhone OS 17_3 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.3 Mobile/15E148 Safari/604.1}", "MobileApp") },
                { new Tuple<string, string, string>("Android", "{Mozilla/5.0 (Linux; Android 10; K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Mobile Safari/537.36}", "MobileApp") },
                { new Tuple<string, string, string>("xBox", "18230582934452242973", "GameConsole") }
            };

            foreach (var userAgent in userAgents)
            {
                string encodedIpAddress = System.Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(ipAddress));
                string encodedUserAgent = System.Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(userAgent.Item2));

                string requestUrl = string.Format("/v7.0/Account002/paymentMethodDescriptions?country=cn&partner={0}&operation=add&family=ewallet&type=alipaycn", partner);

                if (enabledPSS)
                {
                    flights += ",PXDisablePSSCache,PXUsePartnerSettingsService";
                    var settingPartner = partner.Equals(GlobalConstants.Partners.MacManage, System.StringComparison.OrdinalIgnoreCase) ? GlobalConstants.Partners.DefaultTemplate : partner;
                    string expectedPSSResponse = "{\"add\":{\"template\":\"" + settingPartner + "\",\"redirectionPattern\":\"inline\",\"resources\":{\"paymentMethod\":{\"credit_card\":{\"template\":\"" + settingPartner + "\",\"redirectionPattern\":\"inline\"},\"ewallet.alipaycn\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\"}}},\"features\":{\"threeDSOne\":{\"applicableMarkets\":[\"in\"],\"displayCustomizationDetail\":null},\"hideElement\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"hidePaymentMethodHeading\":" + (!isHeadingExpected).ToString().ToLower() + ",\"moveCardNumberBeforeCardHolderName\":false,\"setSaveButtonDisplayContentAsNext\":false,\"updateCvvChallengeTextForGCO\":false,\"enableCountryAddorUpdateCC\":false,\"hideFirstAndLastNameForCompletePrerequisites\":false,\"hideAddCreditDebitCardHeading\":false,\"setGroupedSelectOptionTextBeforeLogo\":false,\"enableSecureFieldAddCC\":false,\"setSaveButtonDisplayContentAsBook\":false,\"removeCancelButton\":false,\"setBackButtonDisplayContentAsCancel\":false,\"hidePaymentSummaryText\":false,\"addressSuggestionMessage\":false}]}}}, \"selectinstance\":{\"template\":\"listpidropdown\",\"redirectionPattern\":null,\"resources\":null,\"features\":null},\"select\":{\"template\":\"selectpmbuttonlist\",\"features\":{\"paymentMethodGrouping\":{\"applicableMarkets\":[]}}},\"default\":{\"template\":\"" + settingPartner + "\",\"redirectionPattern\":null,\"resources\":null,\"features\":null}}";
                    PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);
                }

                string deviceInfo = userAgent.Item1.Equals("xBox") ? string.Format(xBoxDeviceInfo, encodedIpAddress, encodedUserAgent) : string.Format(otherDeviceInfo, encodedIpAddress, encodedUserAgent);

                Dictionary<string, string> headers = new Dictionary<string, string>
                {
                    { "x-ms-flight", flights },
                    { "x-ms-deviceinfo", deviceInfo },
                    { "x-ms-clientcontext-encoding", "base64" }
                };

                List<PIDLResource> pidls = await GetPidlFromPXService(requestUrl, statusCode: enabelPI ? System.Net.HttpStatusCode.OK : System.Net.HttpStatusCode.BadRequest, additionaHeaders: headers);

                // Assert
                Assert.IsNotNull(pidls, "Pidl is expected to be not null");

                foreach (var pidl in pidls)
                {
                    var displayHint = pidl.GetDisplayHintById("ewalletPiText3") as TextDisplayHint;
                    Assert.AreEqual(displayHint.DisplayContent, $", you'll be redirected to Alipay to scan a QR code with a mobile device to verify your account.");

                    var headingDisplayHint = pidl.GetDisplayHintById("paymentMethodHeading") as HeadingDisplayHint;
                    var actualHeadingExpected = headingDisplayHint == null ? false : !(headingDisplayHint.IsHidden.HasValue && headingDisplayHint.IsHidden.Value);
                    Assert.AreEqual(isHeadingExpected, actualHeadingExpected, "Add account heading should not be expected for template partner");

                    var channel = pidl.GetPropertyDescriptionByPropertyName("channel");
                    Assert.AreEqual(userAgent.Item3, channel.DefaultValue, "Device class is expected to be as per user agent");

                    if (limitTextExpected)
                    {
                        var limitText = pidl.GetDisplayHintById("alipaycnLimitText") as TextDisplayHint;
                        Assert.IsNotNull(limitText, "Limit text is expected to be present");
                        Assert.AreEqual(limitText.DisplayContent, "Alipay has a max transaction limit 30000.0 RMB");
                    }
                }
            }

            PXSettings.PartnerSettingsService.Responses.Clear();
        }

        [DataRow("commercialstores", true, true, new string[] { "acceptedMCCardGroup", "acceptedVisaCardGroup" }, null)] // commercialstores IS in the chinaAllowVisaMasterCard partner list
        [DataRow("commercialstores", true, false, new string[] { "acceptedMCCardGroup", "acceptedVisaCardGroup" }, null)] // commercialstores IS in the chinaAllowVisaMasterCard partner list
        [DataRow("commercialstores", false, false, new string[] { "creditCardMCLogo", "creditCardVisaLogo" }, null)] // commercialstores IS in the chinaAllowVisaMasterCard partner list
        [DataRow("cart", true, true, new string[] { "acceptedMCCardGroup", "acceptedVisaCardGroup" }, null)] // cart is NOT in the chinaAllowVisaMasterCard partner list
        [DataRow("cart", true, false, null, new string[] { "acceptedMCCardGroup", "acceptedVisaCardGroup" })] // cart is NOT in the chinaAllowVisaMasterCard partner list
        [DataRow("cart", false, false, null, new string[] { "acceptedMCCardGroup", "acceptedVisaCardGroup" })] // cart is NOT in the chinaAllowVisaMasterCard partner list
        [DataTestMethod]
        public async Task GetPaymentMethodDescriptions_Extensibility_AddPI_ChinaAllowVisaMasterCard(string partner, bool usePSS, bool useFeature, string[] expectedDisplayHints, string[] expectedMissingDisplayHints)
        {
            string requestUrl = $"/v7.0/Account001/paymentMethodDescriptions?country=cn&family=credit_card&language=en-US&partner={partner}&operation=add";
            var headers = new Dictionary<string, string>();

            // Arrange
            if (usePSS)
            {
                string expectedPSSResponse;
                if (useFeature)
                {
                    expectedPSSResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\",\"resources\":{\"paymentMethod\":{\"credit_card\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\"}}},\"features\":{\"chinaAllowVisaMasterCard\":{\"applicableMarkets\":[\"cn\"],\"displayCustomizationDetail\":null}}},\"update\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\",\"resources\":{\"paymentMethod\":{\"credit_card\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\"}}},\"features\":{\"chinaAllowVisaMasterCard\":{\"applicableMarkets\":[\"cn\"],\"displayCustomizationDetail\":null}}}}";
                }
                else
                {
                    expectedPSSResponse = "{\"add\":{\"template\":\"defaulttemplate\"}}";
                }

                headers["x-ms-flight"] = "PXUsePartnerSettingsService,PXDisablePSSCache";
                headers["x-ms-test"] = "{\"scenarios\":\"px.pims.cc.add.success\", \"contact\": \"test\"}";

                // Arrange
                PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);
            }

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(requestUrl, additionaHeaders: headers);

            // Assert
            Assert.IsNotNull(pidls, "Pidls expected to not be null");

            if (expectedDisplayHints != null)
            {
                var foundDisplayHints = new List<string>();
                foreach (var pidl in pidls)
                {
                    foreach (var expectedDisplayHint in expectedDisplayHints)
                    {
                        var displayHint = pidl.GetDisplayHintById(expectedDisplayHint);
                        if (displayHint != null)
                        {
                            foundDisplayHints.Add(displayHint.HintId);
                        }
                    }
                }

                foreach (var expectedDisplayHint in expectedDisplayHints)
                {
                    Assert.IsTrue(foundDisplayHints.Contains(expectedDisplayHint), $"DisplayHint with id of \"{expectedDisplayHint}\", SHOULD be present in at least one of the PIDLs.");
                }
            }

            if (expectedMissingDisplayHints != null)
            {
                foreach (var pidl in pidls)
                {
                    foreach (var expectedMissingDisplayHint in expectedMissingDisplayHints)
                    {
                        Assert.IsNull(pidl.GetDisplayHintById(expectedMissingDisplayHint), $"DisplayHint with id of \"{expectedMissingDisplayHint}\", should NOT be present in any of the PIDLs.");
                    }
                }
            }
        }

        /// <summary>
        /// CIT to test the PSS feature GroupAddressFields when enabled or disabled
        /// </summary>
        [DataRow("macmanage", "credit_card", null)]
        [DataRow("macmanage", "credit_card", Constants.PaymentMethodFamilyType.Verve)]
        [DataRow("macmanage", "credit_card", Constants.PaymentMethodFamilyType.Hipercard)]
        [DataRow("macmanage", "credit_card", Constants.PaymentMethodFamilyType.Elo)]
        [DataRow("macmanage", "credit_card", Constants.PaymentMethodFamilyType.Rupay)]
        [DataRow("macmanage", "credit_card", Constants.PaymentMethodFamilyType.AmexVisaMcDiscoverJcb)]
        [DataRow("officesmb", "credit_card", null)]
        [DataRow("officesmb", "credit_card", Constants.PaymentMethodFamilyType.Verve)]
        [DataRow("officesmb", "credit_card", Constants.PaymentMethodFamilyType.Hipercard)]
        [DataRow("officesmb", "credit_card", Constants.PaymentMethodFamilyType.Elo)]
        [DataRow("officesmb", "credit_card", Constants.PaymentMethodFamilyType.Rupay)]
        [DataRow("officesmb", "credit_card", Constants.PaymentMethodFamilyType.AmexVisaMcDiscoverJcb)]
        [DataRow("officesmb", "direct_debit", Constants.PaymentMethodFamilyType.Sepa)]
        [DataRow("officesmb", "direct_debit", Constants.PaymentMethodFamilyType.Ach)]
        [DataRow("officesmb", "invoice_credit", Constants.PaymentMethodFamilyType.Klarna)]
        [DataTestMethod]
        public async Task GetPaymentMethodDescriptions_PSS_GroupAddressFields(string partner, string family, string type = null)
        {
            // Arrange
            string addressFieldOrderByCountryFeatureEnabled;
            string addressFieldOrderByCountryFeatureDisabled;
            List<string> operations = new List<string> { Constants.OperationTypes.Add, Constants.OperationTypes.Update };
            List<string> countries = type != null && Constants.PaymentMethodCountries.TryGetValue(type, out var specificCountries)
                                        ? specificCountries
                                        : Constants.Countries.ToList();

            bool[] featureStatus = new bool[] { true, false };
            string exposedFlightFeatures = null;

            if (string.Equals(type, Constants.PaymentMethodFamilyType.Rupay, StringComparison.OrdinalIgnoreCase))
            {
                exposedFlightFeatures = "PXEnableRupayForIN,vnext";
            }

            // groupAddressFields featureStatus, country, enableZipCodeStateGrouping featureStatus, addressFields
            Dictionary<bool, Dictionary<string, Dictionary<bool, List<string>>>> addressGroupFieldsOrderByFeatureForAdd = new Dictionary<bool, Dictionary<string, Dictionary<bool, List<string>>>>();
            Dictionary<bool, Dictionary<string, Dictionary<bool, List<string>>>> addressGroupFieldsOrderByFeatureForUpdate = new Dictionary<bool, Dictionary<string, Dictionary<bool, List<string>>>>();

            foreach (var operation in operations)
            {
                if (string.Equals(type, Constants.PaymentMethodFamilyType.Klarna, StringComparison.OrdinalIgnoreCase) && string.Equals(operation, Constants.OperationTypes.Update, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                foreach (bool useGroupAddressFieldsFeature in featureStatus)
                {
                    Dictionary<string, Dictionary<bool, List<string>>> addressFieldsOrderByCountry = new Dictionary<string, Dictionary<bool, List<string>>>();

                    foreach (string country in countries)
                    {
                        Dictionary<bool, List<string>> addressFieldsOrderByZipCodeStateGrouping = new Dictionary<bool, List<string>>();

                        foreach (bool enableZipCodeStateGrouping in featureStatus)
                        {
                            string requestUrl = $"/v7.0/Account001/paymentMethodDescriptions?partner={partner}&operation={operation}&country={country}&language=en-US&family={family}&type={type}";
                            Dictionary<string, string> headers = new Dictionary<string, string>() { { "x-ms-flight", "PXDisablePSSCache" } };

                            string features = useGroupAddressFieldsFeature ? "\"groupAddressFields\":{\"applicableMarkets\":[]}" : string.Empty;

                            if (string.Equals(country, GlobalConstants.CountryCodes.China, StringComparison.OrdinalIgnoreCase))
                            {
                                // Changing the type for China as per pims supported types.
                                string chinaAllowedVisaMcFeature = "\"chinaAllowVisaMasterCard\":{\"applicableMarkets\":[\"cn\"],\"displayCustomizationDetail\":null}";
                                features = string.IsNullOrEmpty(features) ? chinaAllowedVisaMcFeature : $"{features},{chinaAllowedVisaMcFeature}";
                            }

                            if (enableZipCodeStateGrouping)
                            {
                                string zipCodeStateGroupingFeature = "\"enableZipCodeStateGrouping\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]}";
                                features = string.IsNullOrEmpty(features) ? zipCodeStateGroupingFeature : $"{features},{zipCodeStateGroupingFeature}";
                            }

                            string pssTemplatePartner = partner.Equals(Constants.VirtualPartnerNames.Macmanage, StringComparison.OrdinalIgnoreCase) ? Constants.PartnerNames.TwoPage : Constants.PartnerNames.DefaultTemplate;
                            string expectedPSSResponse = "{\"" + operation + "\":{\"template\":\"" + pssTemplatePartner + "\",\"features\":{" + features + "}}}";
                            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

                            // Act
                            List<PIDLResource> pidls = await GetPidlFromPXService(requestUrl, flightNames: exposedFlightFeatures, additionaHeaders: headers);

                            // Assert
                            Assert.IsNotNull(pidls, "Pidls expected to not be null");

                            List<string> addressFieldsWithOrderByPidl = new List<string>();

                            foreach (var pidl in pidls)
                            {
                                Assert.IsNotNull(pidl.DisplayPages, "DisplayPages is expected to be not null");

                                foreach (PageDisplayHint pidlDisplayPage in pidl.DisplayPages)
                                {
                                    SortedDictionary<int, string> addressFieldsWithOrder = new SortedDictionary<int, string>();

                                    GroupDisplayHint addressGroup = pidlDisplayPage?.Members
                                        .FirstOrDefault(displayHint => displayHint.HintId.Equals(Constants.DisplayHintIds.AddressGroup, StringComparison.OrdinalIgnoreCase)) as GroupDisplayHint;

                                    var addressFieldHints = pidlDisplayPage?.Members
                                        .Where(displayHint => Constants.AddressFields.Contains(displayHint.HintId, StringComparer.OrdinalIgnoreCase))
                                        .ToList();

                                    if (useGroupAddressFieldsFeature && addressGroup != null)
                                    {
                                        var groupAddressFieldHints = addressGroup.Members
                                            .Where(displayHint => Constants.AddressFields.Contains(displayHint.HintId, StringComparer.OrdinalIgnoreCase))
                                            .ToList();

                                        foreach (var addressFieldDisplayHint in groupAddressFieldHints)
                                        {
                                            Assert.IsNotNull(addressGroup, "addressGroup is not expected to be null");
                                            Assert.IsTrue(addressGroup.Members.Contains(addressFieldDisplayHint), $"DisplayHint \"{addressFieldDisplayHint.HintId}\" is expected to be inside addressGroup. Test Country: {country}");
                                            int index = addressGroup.Members.IndexOf(addressFieldDisplayHint);
                                            if (!addressFieldsWithOrder.ContainsKey(index))
                                            {
                                                addressFieldsWithOrder.Add(index, addressFieldDisplayHint.HintId);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        foreach (var addressFieldDisplayHint in addressFieldHints)
                                        {
                                            Assert.IsNull(addressGroup, "addressGroup is expected to be null");
                                            int index = pidlDisplayPage.Members.IndexOf(addressFieldDisplayHint);
                                            if (!addressFieldsWithOrder.ContainsKey(index))
                                            {
                                                addressFieldsWithOrder.Add(index, addressFieldDisplayHint.HintId);
                                            }
                                        }
                                    }

                                    addressFieldsWithOrderByPidl.Add(string.Join(",", addressFieldsWithOrder.Values));
                                }
                            }

                            addressFieldsOrderByZipCodeStateGrouping.Add(enableZipCodeStateGrouping, addressFieldsWithOrderByPidl);

                            PXSettings.PartnerSettingsService.ResetToDefaults();
                        }

                        addressFieldsOrderByCountry.Add(country, addressFieldsOrderByZipCodeStateGrouping);
                    }

                    if (string.Equals(operation, Constants.OperationTypes.Add))
                    {
                        addressGroupFieldsOrderByFeatureForAdd.Add(useGroupAddressFieldsFeature, addressFieldsOrderByCountry);
                    }
                    else
                    {
                        addressGroupFieldsOrderByFeatureForUpdate.Add(useGroupAddressFieldsFeature, addressFieldsOrderByCountry);
                    }
                }

                // Assert address fields order when feature groupAddressFields is enabled and disabled
                if (string.Equals(operation, Constants.OperationTypes.Add))
                {
                    addressFieldOrderByCountryFeatureEnabled = JsonConvert.SerializeObject(addressGroupFieldsOrderByFeatureForAdd[true]);
                    addressFieldOrderByCountryFeatureDisabled = JsonConvert.SerializeObject(addressGroupFieldsOrderByFeatureForAdd[false]);
                }
                else
                {
                    addressFieldOrderByCountryFeatureEnabled = JsonConvert.SerializeObject(addressGroupFieldsOrderByFeatureForUpdate[true]);
                    addressFieldOrderByCountryFeatureDisabled = JsonConvert.SerializeObject(addressGroupFieldsOrderByFeatureForUpdate[false]);
                }

                // Compare both strings manually to find the exact difference for country's address fields order
                Assert.AreEqual(addressFieldOrderByCountryFeatureEnabled, addressFieldOrderByCountryFeatureDisabled, "Address fields order should be same when feature enabled and disabled");
            }
        }

        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", "add", true, true)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", "update", true, true)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", "add", false, false)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "cn", "add", true, true)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "cn", "update", true, true)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "cn", "add", true, false)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "cn", "add", false, false)]
        [DataTestMethod]
        public async Task GetPaymentMethodDescriptions_AddUpdatePI_EncryptAndTokenize(string partner, string country, string operation, bool enableFlight, bool enableCaching)
        {
            string requestUrl = $"/v7.0/Account001/paymentMethodDescriptions?country={country}&family=credit_card&language=en-US&partner={partner}&operation={operation}";
            var headers = new Dictionary<string, string>();

            // Arrange
            if (enableFlight)
            {
                PXFlightHandler.AddToEnabledFlights("PXEnableTokenizationEncryptionAddUpdateCC");
            }

            if (enableCaching)
            {
                PXFlightHandler.AddToEnabledFlights("PXEnableCachingTokenizationEncryption");
            }

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(requestUrl, additionaHeaders: headers);

            // Assert
            Assert.IsNotNull(pidls, "Pidls expected to not be null");
            foreach (var pidl in pidls)
            {
                if (enableFlight)
                {
                    if (operation.Equals("add"))
                    {
                        Assert.AreEqual("MSREncrypt", pidl.GetPropertyDescriptionByPropertyNameWithFullPath("details.accountToken").DataProtection.ProtectionType, "Data protection type should be MSREncrypt");
                        Assert.IsNotNull(pidl.GetPropertyDescriptionByPropertyNameWithFullPath("details.accountToken").DataProtection.Parameters["publicKey"], "Public key should not be null");
                    }

                    if (pidl.Identity["type"] != "unionpay_debitcard")
                    {
                        Assert.AreEqual("MSREncrypt", pidl.GetPropertyDescriptionByPropertyNameWithFullPath("details.cvvToken").DataProtection.ProtectionType, "Data protection type should be MSREncrypt");
                        Assert.IsNotNull(pidl.GetPropertyDescriptionByPropertyNameWithFullPath("details.cvvToken").DataProtection.Parameters["publicKey"], "Public key should not be null");
                    }
                }
                else
                {
                    if (operation.Equals("add"))
                    {
                        Assert.IsNull(pidl.GetPropertyDescriptionByPropertyNameWithFullPath("details.accountToken").DataProtection, "DataProtection should be null");
                    }

                    if (pidl.Identity["type"] != "unionpay_debitcard")
                    {
                        Assert.IsNull(pidl.GetPropertyDescriptionByPropertyNameWithFullPath("details.cvvToken").DataProtection, "DataProtection should be null");
                    }
                }
            }
        }

        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", "add", true, true, true, true, true)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", "add", true, true, true, false, true)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", "add", true, false, true, true, true)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", "add", true, true, true, false, false)]
        [DataRow(GlobalConstants.Partners.DefaultTemplate, "us", "update", true, true, true, true, true)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", "update", true, true, true, false, false)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", "add", false, false, false, false, false)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", "add", false, false, false, false, true)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", "update", false, false, false, false, false)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "cn", "add", true, true, true, true, false)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "cn", "add", true, true, true, false, false)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "cn", "update", true, true, true, false, false)]
        [DataRow(GlobalConstants.Partners.DefaultTemplate, "cn", "update", true, true, true, false, false)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "cn", "add", true, false, false, false, false)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "cn", "add", false, false, false, false, false)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "cn", "update", false, false, false, false, false)]
        [DataTestMethod]
        public async Task GetPaymentMethodDescriptions_AddUpdatePI_EncryptAndTokenize_FetchConfig(string partner, string country, string operation, bool enableFlight, bool enableCaching, bool enableScript, bool enablePiAuthKey, bool enableSecureField)
        {
            string requestUrl = $"/v7.0/Account001/paymentMethodDescriptions?country={country}&family=credit_card&language=en-US&partner={partner}&operation={operation}";
            var headers = new Dictionary<string, string>();

            if (partner == "defaultTemplate" && operation == "update")
            {
                requestUrl += "&scenario=includecvv";
            }

            // Arrange
            if (enableFlight)
            {
                PXFlightHandler.AddToEnabledFlights("PXEnableTokenizationEncryptionFetchConfigAddUpdateCC");
            }

            if (enableSecureField)
            {
                if (operation.Equals("add"))
                {
                    PXFlightHandler.AddToEnabledFlights("PXEnableSecureFieldAddCreditCard");
                }
                else
                {
                    PXFlightHandler.AddToEnabledFlights("PXEnableSecureFieldUpdateCreditCard");
                }
            }

            if (enableCaching)
            {
                PXFlightHandler.AddToEnabledFlights("PXEnableCachingTokenizationEncryption");
            }

            if (enableScript)
            {
                PXFlightHandler.AddToEnabledFlights("PXEnableTokenizationEncryptionFetchConfigWithScript");
            }

            if (enablePiAuthKey)
            {
                PXFlightHandler.AddToEnabledFlights("PXEnableTokenizationEncFetchConfigAddCCPiAuthKey");
            }

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(requestUrl, additionaHeaders: headers);

            // Assert
            Assert.IsNotNull(pidls, "Pidls expected to not be null");
            foreach (var pidl in pidls)
            {
                if ((enableFlight || enablePiAuthKey) && !enableSecureField)
                {
                    if (operation.Equals("add"))
                    {
                        var accountToken = pidl.GetPropertyDescriptionByPropertyNameWithFullPath("details.accountToken");

                        if (enableFlight)
                        {
                            Assert.AreEqual("TokenizeMSREncrypt", accountToken.DataProtection.ProtectionType, "Data protection type should be MSREncrypt");
                            Assert.IsNotNull(accountToken.DataProtection.Parameters["publicKey"], "Public key should not be null");
                            Assert.IsNotNull(accountToken.DataProtection.FetchConfig, "Fetch config should not be null");
                            Assert.AreEqual(4, accountToken.DataProtection.FetchConfig.FetchOrder.Count, "Fetch config retry order count should be 4");

                            if (enableScript)
                            {
                                Assert.AreEqual("encryptAndTokenize.js", accountToken.DataProtection.Parameters["encryptionScript"], "Function should not be null");
                            }
                            else
                            {
                                Assert.AreEqual("encrypt", accountToken.DataProtection.Parameters["encryptionFunction"], "Function should not be null");
                            }
                        }
                        else
                        {
                            Assert.AreEqual("MSREncrypt", accountToken.DataProtection.ProtectionType, "Data protection type should be MSREncrypt");
                            Assert.IsNull(accountToken.DataProtection.Parameters, "Public key should be null for MSREncrypt");
                            Assert.IsNull(accountToken.DataProtection.FetchConfig, "Fetch config should be null for MSREncrypt");
                        }

                        if (pidl.Identity["type"] != "unionpay_debitcard" && pidl.Identity["type"] != "unionpay_creditcard")
                        {
                            var piAuthKeyToken = pidl.GetPropertyDescriptionByPropertyNameWithFullPath("details.permission.hmac");
                            Assert.AreEqual(enablePiAuthKey ? "HMACSignatureMSREncrypt" : "HMACSignature", piAuthKeyToken.DataProtection.ProtectionType, "Data protection type should be MSREncrypt");

                            if (enablePiAuthKey)
                            {
                                Assert.IsNotNull(piAuthKeyToken.DataProtection.Parameters["publicKey"], "Public key should not be null");
                                Assert.IsNotNull(piAuthKeyToken.DataProtection.FetchConfig, "Fetch config should not be null");
                            }
                            else
                            {
                                Assert.IsNull(piAuthKeyToken.DataProtection.FetchConfig, "Fetch config should be null for HMACSignature");
                                Assert.IsNull(piAuthKeyToken.DataProtection.Parameters, "Public key should be null for HMACSignature");
                            }
                        }
                    }

                    if (pidl.Identity["type"] != "unionpay_debitcard")
                    {
                        var cvvToken = pidl.GetPropertyDescriptionByPropertyNameWithFullPath("details.cvvToken");

                        Assert.AreEqual("TokenizeMSREncrypt", cvvToken.DataProtection.ProtectionType, "Data protection type should be MSREncrypt");
                        Assert.IsNotNull(cvvToken.DataProtection.Parameters["publicKey"], "Public key should not be null");
                        Assert.IsNotNull(cvvToken.DataProtection.FetchConfig, "Fetch config should not be null");
                        Assert.AreEqual(4, cvvToken.DataProtection.FetchConfig.FetchOrder.Count, "Fetch config retry order count should be 4");

                        if (enableScript)
                        {
                            Assert.AreEqual("encryptAndTokenize.js", cvvToken.DataProtection.Parameters["encryptionScript"], "Function should not be null");
                        }
                        else
                        {
                            Assert.AreEqual("encrypt", cvvToken.DataProtection.Parameters["encryptionFunction"], "Function should not be null");
                        }
                    }
                }
                else
                {
                    if (operation.Equals("add"))
                    {
                        Assert.IsNull(pidl.GetPropertyDescriptionByPropertyNameWithFullPath("details.accountToken").DataProtection, "DataProtection should be null");

                        if (pidl.Identity["type"] != "unionpay_debitcard" && pidl.Identity["type"] != "unionpay_creditcard")
                        {
                            Assert.AreEqual("HMACSignature", pidl.GetPropertyDescriptionByPropertyNameWithFullPath("details.permission.hmac").DataProtection.ProtectionType, "DataProtection for hmac should be HMACSignature");
                        }

                        Assert.AreEqual(enableSecureField ? "secureproperty" : "property", pidl.GetDisplayHintByPropertyName("accountToken").DisplayHintType, "DisplayHintType for accountToken should be secure property if secure filed enabled");
                    }

                    if (pidl.Identity["type"] != "unionpay_debitcard")
                    {
                        Assert.IsNull(pidl.GetPropertyDescriptionByPropertyNameWithFullPath("details.cvvToken").DataProtection, "DataProtection should be null");

                        Assert.AreEqual(enableSecureField ? "secureproperty" : "property", pidl.GetDisplayHintByPropertyName("cvvToken").DisplayHintType, "DisplayHintType for cvvToken should be secure property if secure filed enabled");
                    }
                }
            }
        }

        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", true, true, true, true, false, false, false)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", true, true, true, false, false, false, false)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", true, true, false, false, false, false, false)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", false, true, true, false, false, false, false)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", true, false, false, false, false, false, false)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", false, false, false, false, false, false, false)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", true, true, true, true, true, false, false)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", true, true, true, false, true, false, false)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", true, true, false, false, true, false, false)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", false, true, true, false, true, false, false)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", true, false, false, false, true, false, false)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", false, false, false, false, true, false, false)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", true, true, true, true, false, true, false)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", true, true, true, false, false, true, false)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", true, true, false, false, false, true, false)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", false, true, true, false, false, true, false)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", true, false, false, false, false, true, false)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", false, false, false, false, false, true, false)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", true, true, true, true, true, true, false)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", true, true, true, false, true, true, false)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", true, true, false, false, true, true, false)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", false, true, true, false, true, true, false)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", true, false, false, false, true, true, false)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", false, false, false, false, true, true, false)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", true, true, true, true, false, true, true)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", true, true, true, false, false, true, true)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", true, true, false, false, false, true, true)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", false, true, true, false, false, true, true)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", true, false, false, false, false, true, true)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", false, false, false, false, false, true, true)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", true, true, true, true, true, true, true)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", true, true, true, false, true, true, true)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", true, true, false, false, true, true, true)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", false, true, true, false, true, true, true)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", true, false, false, false, true, true, true)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", false, false, false, false, true, true, true)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", true, true, true, true, false, false, true)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", true, true, true, false, false, false, true)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", true, true, false, false, false, false, true)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", false, true, true, false, false, false, true)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", true, false, false, false, false, false, true)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", false, false, false, false, false, false, true)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", true, true, true, true, true, false, true)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", true, true, true, false, true, false, true)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", true, true, false, false, true, false, true)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", false, true, true, false, true, false, true)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", true, false, false, false, true, false, true)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", false, false, false, false, true, false, true)]
        [DataTestMethod]
        public async Task GetPaymentMethodDescriptions_AddPI_EncryptAndTokenize_PiAuthKeyFetchConfig_Payload(string partner, string country, bool enablePiAuthKey, bool disableEncryptedPayload, bool enablePanAndCvv, bool enableSecureField, bool enableRemoveUseFallback, bool enableAlwaysEncryptedTokenization, bool enableRemoveUseFallbackForAlwaysEncryptedTokenization)
        {
            string requestUrl = $"/v7.0/Account001/paymentMethodDescriptions?country={country}&family=credit_card&language=en-US&partner={partner}&operation=add";
            var headers = new Dictionary<string, string>();

            // Arrange
            if (enablePiAuthKey)
            {
                PXFlightHandler.AddToEnabledFlights("PXEnableTokenizationEncFetchConfigAddCCPiAuthKey");
            }

            if (disableEncryptedPayload)
            {
                PXFlightHandler.AddToEnabledFlights("PXDisableTokenizationEncPiAuthKeyFetchConfigtEncPayload");
            }

            if (enablePanAndCvv)
            {
                PXFlightHandler.AddToEnabledFlights("PXEnableTokenizationEncryptionFetchConfigAddUpdateCC");
            }

            if (enableSecureField)
            {
                PXFlightHandler.AddToEnabledFlights("PXEnableSecureFieldAddCreditCard");
            }

            if (enableRemoveUseFallback)
            {
                PXFlightHandler.AddToEnabledFlights("PXRemoveUseFallbackForSubtleImportKey");
            }

            if (enableAlwaysEncryptedTokenization)
            {
                PXFlightHandler.AddToEnabledFlights("PXEncryptedTokenizationOnlyForPanCvvPiAuthKey");
            }

            if (enableRemoveUseFallbackForAlwaysEncryptedTokenization)
            {
                PXFlightHandler.AddToEnabledFlights("PXRemoveUseFallbackWhenEncryptedTokenizationOnlyForPanCvvPiAuthKey");
            }

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(requestUrl, additionaHeaders: headers);

            // Assert
            Assert.IsNotNull(pidls, "Pidls expected to not be null");
            foreach (var pidl in pidls)
            {
                var piAuthKeyToken = pidl.GetPropertyDescriptionByPropertyNameWithFullPath("details.permission.hmac");

                if (enableSecureField)
                {
                    Assert.AreEqual("secureproperty", pidl.GetDisplayHintByPropertyName("accountToken").DisplayHintType, "DisplayHintType for accountToken should be secure property if secure filed enabled");
                    Assert.AreEqual("secureproperty", pidl.GetDisplayHintByPropertyName("cvvToken").DisplayHintType, "DisplayHintType for cvvToken should be secure property if secure filed enabled");
                    Assert.IsNull(pidl.GetPropertyDescriptionByPropertyNameWithFullPath("details.accountToken").DataProtection, "DataProtection should be null");
                    Assert.IsNull(pidl.GetPropertyDescriptionByPropertyNameWithFullPath("details.cvvToken").DataProtection, "DataProtection should be null");
                    Assert.AreEqual("HMACSignature", pidl.GetPropertyDescriptionByPropertyNameWithFullPath("details.permission.hmac").DataProtection.ProtectionType, "DataProtection for hmac should be HMACSignature");
                    Assert.IsNull(piAuthKeyToken.DataProtection.FetchConfig, "Fetch config should be null for HMACSignature");
                    Assert.IsNull(piAuthKeyToken.DataProtection.Parameters, "Public key should be null for HMACSignature");
                }

                if (enablePiAuthKey && !enableSecureField)
                {
                    Assert.AreEqual("HMACSignatureMSREncrypt", piAuthKeyToken.DataProtection.ProtectionType, "Data protection type should be HMACSignatureMSREncrypt");
                    Assert.IsNotNull(piAuthKeyToken.DataProtection.Parameters["publicKey"], "Public key should not be null for HMACSignatureMSREncrypt");
                    Assert.IsNotNull(piAuthKeyToken.DataProtection.FetchConfig, "Fetch config should not be null for HMACSignatureMSREncrypt");

                    if (disableEncryptedPayload && !enableAlwaysEncryptedTokenization)
                    {
                        Assert.IsFalse(piAuthKeyToken.DataProtection.FetchConfig.FetchOrder[2].UseSecondaryPayload, "UseSecondaryPayload should be false");
                        Assert.IsFalse(piAuthKeyToken.DataProtection.FetchConfig.FetchOrder[2].Endpoint.Contains("/GetTokenFromEncryptedValue"), "Endpoint should be GetToken");
                        Assert.IsFalse(piAuthKeyToken.DataProtection.FetchConfig.FetchOrder[3].UseSecondaryPayload, "UseSecondaryPayload should be false");
                        Assert.IsFalse(piAuthKeyToken.DataProtection.FetchConfig.FetchOrder[3].Endpoint.Contains("/GetTokenFromEncryptedValue"), "Endpoint should be GetToken");
                    }
                    else
                    {
                        Assert.IsTrue(piAuthKeyToken.DataProtection.FetchConfig.FetchOrder[2].UseSecondaryPayload, "UseSecondaryPayload should be true");
                        Assert.IsTrue(piAuthKeyToken.DataProtection.FetchConfig.FetchOrder[2].Endpoint.Contains("/GetTokenFromEncryptedValue"), "Endpoint should be GetTokenFromEncryptedValue");
                        Assert.IsTrue(piAuthKeyToken.DataProtection.FetchConfig.FetchOrder[3].UseSecondaryPayload, "UseSecondaryPayload should be true");
                        Assert.IsTrue(piAuthKeyToken.DataProtection.FetchConfig.FetchOrder[3].Endpoint.Contains("/GetTokenFromEncryptedValue"), "Endpoint should be GetTokenFromEncryptedValue");
                    }

                    if (enableRemoveUseFallback || (enableAlwaysEncryptedTokenization && enableRemoveUseFallbackForAlwaysEncryptedTokenization))
                    {
                        Assert.IsNotNull(piAuthKeyToken.DataProtection.Parameters["removeUseFallback"], "removeUseFallback should not be null");
                    }
                    else
                    {
                        string removeUseFallbackValue = null;
                        Assert.IsFalse(piAuthKeyToken.DataProtection.Parameters.TryGetValue("removeUseFallback", out removeUseFallbackValue), "removeUseFallback should not exist");
                    }
                }
                else
                {
                    Assert.AreEqual("HMACSignature", pidl.GetPropertyDescriptionByPropertyNameWithFullPath("details.permission.hmac").DataProtection.ProtectionType, "DataProtection for hmac should be HMACSignature");
                    Assert.IsNull(piAuthKeyToken.DataProtection.FetchConfig, "Fetch config should be null for HMACSignature");
                    Assert.IsNull(piAuthKeyToken.DataProtection.Parameters, "Public key should be null for HMACSignature");
                }

                if (enablePanAndCvv && !enableSecureField)
                {
                    var accountToken = pidl.GetPropertyDescriptionByPropertyNameWithFullPath("details.accountToken");
                    Assert.AreEqual("TokenizeMSREncrypt", accountToken.DataProtection.ProtectionType, "Data protection type should be MSREncrypt");
                    Assert.IsNotNull(accountToken.DataProtection.Parameters["publicKey"], "Public key should not be null");
                    Assert.IsNotNull(accountToken.DataProtection.FetchConfig, "Fetch config should not be null");
                    Assert.AreEqual(4, accountToken.DataProtection.FetchConfig.FetchOrder.Count, "Fetch config retry order count should be 4");

                    var cvvToken = pidl.GetPropertyDescriptionByPropertyNameWithFullPath("details.cvvToken");

                    Assert.AreEqual("TokenizeMSREncrypt", cvvToken.DataProtection.ProtectionType, "Data protection type should be MSREncrypt");
                    Assert.IsNotNull(cvvToken.DataProtection.Parameters["publicKey"], "Public key should not be null");
                    Assert.IsNotNull(cvvToken.DataProtection.FetchConfig, "Fetch config should not be null");

                    if (enableRemoveUseFallback || (enableAlwaysEncryptedTokenization && enableRemoveUseFallbackForAlwaysEncryptedTokenization))
                    {
                        Assert.IsNotNull(accountToken.DataProtection.Parameters["removeUseFallback"], "removeUseFallback should not be null");
                        Assert.IsNotNull(cvvToken.DataProtection.Parameters["removeUseFallback"], "removeUseFallback should not be null");
                    }
                    else
                    {
                        string removeUseFallbackValue = null;
                        Assert.IsFalse(accountToken.DataProtection.Parameters.TryGetValue("removeUseFallback", out removeUseFallbackValue), "removeUseFallback should not exist");
                        Assert.IsFalse(cvvToken.DataProtection.Parameters.TryGetValue("removeUseFallback", out removeUseFallbackValue), "removeUseFallback should not exist");
                    }
                }
                else
                {
                    var accountToken = pidl.GetPropertyDescriptionByPropertyNameWithFullPath("details.accountToken");
                    Assert.IsNull(accountToken.DataProtection, "DataProtection should be null for PAN");

                    var cvvToken = pidl.GetPropertyDescriptionByPropertyNameWithFullPath("details.cvvToken");

                    Assert.IsNull(cvvToken.DataProtection, "DataProtection should be null for CVV");
                }
            }
        }

        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", true, true, true, true)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", true, true, true, false)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", true, true, false, false)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", false, true, true, false)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", true, false, false, false)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "us", false, false, false, false)]
        [DataTestMethod]
        public async Task GetPaymentMethodDescriptions_AddPI_PXEncryptedTokenizationOnlyForPanCvvPiAuthKey(string partner, string country, bool enablePiAuthKey, bool encryptedTokenizationOnly, bool enablePanAndCvv, bool enableSecureField)
        {
            string requestUrl = $"/v7.0/Account001/paymentMethodDescriptions?country={country}&family=credit_card&language=en-US&partner={partner}&operation=add";
            var headers = new Dictionary<string, string>();

            // Arrange
            if (enablePiAuthKey)
            {
                PXFlightHandler.AddToEnabledFlights("PXEnableTokenizationEncFetchConfigAddCCPiAuthKey");
            }

            if (encryptedTokenizationOnly)
            {
                PXFlightHandler.AddToEnabledFlights("PXEncryptedTokenizationOnlyForPanCvvPiAuthKey");
            }

            if (enablePanAndCvv)
            {
                PXFlightHandler.AddToEnabledFlights("PXEnableTokenizationEncryptionFetchConfigAddUpdateCC");
            }

            if (enableSecureField)
            {
                PXFlightHandler.AddToEnabledFlights("PXEnableSecureFieldAddCreditCard");
            }

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(requestUrl, additionaHeaders: headers);

            // Assert
            Assert.IsNotNull(pidls, "Pidls expected to not be null");
            foreach (var pidl in pidls)
            {
                var piAuthKeyToken = pidl.GetPropertyDescriptionByPropertyNameWithFullPath("details.permission.hmac");

                if (enableSecureField)
                {
                    Assert.AreEqual("secureproperty", pidl.GetDisplayHintByPropertyName("accountToken").DisplayHintType, "DisplayHintType for accountToken should be secure property if secure filed enabled");
                    Assert.AreEqual("secureproperty", pidl.GetDisplayHintByPropertyName("cvvToken").DisplayHintType, "DisplayHintType for cvvToken should be secure property if secure filed enabled");
                    Assert.IsNull(pidl.GetPropertyDescriptionByPropertyNameWithFullPath("details.accountToken").DataProtection, "DataProtection should be null");
                    Assert.IsNull(pidl.GetPropertyDescriptionByPropertyNameWithFullPath("details.cvvToken").DataProtection, "DataProtection should be null");
                    Assert.AreEqual("HMACSignature", pidl.GetPropertyDescriptionByPropertyNameWithFullPath("details.permission.hmac").DataProtection.ProtectionType, "DataProtection for hmac should be HMACSignature");
                    Assert.IsNull(piAuthKeyToken.DataProtection.FetchConfig, "Fetch config should be null for HMACSignature");
                    Assert.IsNull(piAuthKeyToken.DataProtection.Parameters, "Public key should be null for HMACSignature");
                }

                if (enablePiAuthKey && !enableSecureField)
                {
                    Assert.AreEqual("HMACSignatureMSREncrypt", piAuthKeyToken.DataProtection.ProtectionType, "Data protection type should be HMACSignatureMSREncrypt");
                    Assert.IsNotNull(piAuthKeyToken.DataProtection.Parameters["publicKey"], "Public key should not be null for HMACSignatureMSREncrypt");
                    Assert.IsNotNull(piAuthKeyToken.DataProtection.FetchConfig, "Fetch config should not be null for HMACSignatureMSREncrypt");

                    if (encryptedTokenizationOnly)
                    {
                        Assert.IsTrue(piAuthKeyToken.DataProtection.FetchConfig.FetchOrder[0].UseSecondaryPayload, "UseSecondaryPayload should be true");
                        Assert.IsTrue(piAuthKeyToken.DataProtection.FetchConfig.FetchOrder[0].Endpoint.Contains("/GetTokenFromEncryptedValue"), "Endpoint should be GetTokenFromEncryptedValue");
                        Assert.IsTrue(piAuthKeyToken.DataProtection.FetchConfig.FetchOrder[1].UseSecondaryPayload, "UseSecondaryPayload should be true");
                        Assert.IsTrue(piAuthKeyToken.DataProtection.FetchConfig.FetchOrder[1].Endpoint.Contains("/GetTokenFromEncryptedValue"), "Endpoint should be GetTokenFromEncryptedValue");
                        Assert.IsTrue(piAuthKeyToken.DataProtection.FetchConfig.FetchOrder[2].UseSecondaryPayload, "UseSecondaryPayload should be true");
                        Assert.IsTrue(piAuthKeyToken.DataProtection.FetchConfig.FetchOrder[2].Endpoint.Contains("/GetTokenFromEncryptedValue"), "Endpoint should be GetTokenFromEncryptedValue");
                        Assert.IsTrue(piAuthKeyToken.DataProtection.FetchConfig.FetchOrder[3].UseSecondaryPayload, "UseSecondaryPayload should be true");
                        Assert.IsTrue(piAuthKeyToken.DataProtection.FetchConfig.FetchOrder[3].Endpoint.Contains("/GetTokenFromEncryptedValue"), "Endpoint should be GetTokenFromEncryptedValue");
                    }
                    else
                    {
                        Assert.IsFalse(piAuthKeyToken.DataProtection.FetchConfig.FetchOrder[0].UseSecondaryPayload, "UseSecondaryPayload should be false");
                        Assert.IsFalse(piAuthKeyToken.DataProtection.FetchConfig.FetchOrder[0].Endpoint.Contains("/GetTokenFromEncryptedValue"), "Endpoint should be GetToken");
                        Assert.IsFalse(piAuthKeyToken.DataProtection.FetchConfig.FetchOrder[1].UseSecondaryPayload, "UseSecondaryPayload should be false");
                        Assert.IsFalse(piAuthKeyToken.DataProtection.FetchConfig.FetchOrder[1].Endpoint.Contains("/GetTokenFromEncryptedValue"), "Endpoint should be GetToken");
                        Assert.IsTrue(piAuthKeyToken.DataProtection.FetchConfig.FetchOrder[2].UseSecondaryPayload, "UseSecondaryPayload should be true");
                        Assert.IsTrue(piAuthKeyToken.DataProtection.FetchConfig.FetchOrder[2].Endpoint.Contains("/GetTokenFromEncryptedValue"), "Endpoint should be GetTokenFromEncryptedValue");
                        Assert.IsTrue(piAuthKeyToken.DataProtection.FetchConfig.FetchOrder[3].UseSecondaryPayload, "UseSecondaryPayload should be true");
                        Assert.IsTrue(piAuthKeyToken.DataProtection.FetchConfig.FetchOrder[3].Endpoint.Contains("/GetTokenFromEncryptedValue"), "Endpoint should be GetTokenFromEncryptedValue");
                    }
                }
                else
                {
                    Assert.AreEqual("HMACSignature", pidl.GetPropertyDescriptionByPropertyNameWithFullPath("details.permission.hmac").DataProtection.ProtectionType, "DataProtection for hmac should be HMACSignature");
                    Assert.IsNull(piAuthKeyToken.DataProtection.FetchConfig, "Fetch config should be null for HMACSignature");
                    Assert.IsNull(piAuthKeyToken.DataProtection.Parameters, "Public key should be null for HMACSignature");
                }

                if (enablePanAndCvv && !enableSecureField)
                {
                    var accountToken = pidl.GetPropertyDescriptionByPropertyNameWithFullPath("details.accountToken");
                    Assert.AreEqual("TokenizeMSREncrypt", accountToken.DataProtection.ProtectionType, "Data protection type should be MSREncrypt");
                    Assert.IsNotNull(accountToken.DataProtection.Parameters["publicKey"], "Public key should not be null");
                    Assert.IsNotNull(accountToken.DataProtection.FetchConfig, "Fetch config should not be null");
                    Assert.AreEqual(4, accountToken.DataProtection.FetchConfig.FetchOrder.Count, "Fetch config retry order count should be 4");

                    var cvvToken = pidl.GetPropertyDescriptionByPropertyNameWithFullPath("details.cvvToken");

                    Assert.AreEqual("TokenizeMSREncrypt", cvvToken.DataProtection.ProtectionType, "Data protection type should be MSREncrypt");
                    Assert.IsNotNull(cvvToken.DataProtection.Parameters["publicKey"], "Public key should not be null");
                    Assert.IsNotNull(cvvToken.DataProtection.FetchConfig, "Fetch config should not be null");
                }
                else
                {
                    var accountToken = pidl.GetPropertyDescriptionByPropertyNameWithFullPath("details.accountToken");
                    Assert.IsNull(accountToken.DataProtection, "DataProtection should be null for PAN");

                    var cvvToken = pidl.GetPropertyDescriptionByPropertyNameWithFullPath("details.cvvToken");

                    Assert.IsNull(cvvToken.DataProtection, "DataProtection should be null for CVV");
                }
            }
        }

        [DataRow(Constants.PartnerNames.Azure, "saveNextButton")]
        [DataRow(Constants.PartnerNames.AzureSignup, "saveNextButton")]
        [DataRow(Constants.PartnerNames.AzureIbiza, "saveNextButton")]
        [DataTestMethod]
        public async Task GetPaymentMethodDescriptions_ClassicProductAndBillableAccountInCompletePrerequisitesFalse(string partner, string buttonHintId)
        {
            // Arrange
            string requestUrl = $"/v7.0/Account001/paymentMethodDescriptions?partner={partner}&operation=add&country=us&language=en-US&family=credit_card&classicProduct=azureClassic&billableAccountId=TGTAeQAAAAAAAAAA&completePrerequisites=false";

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(requestUrl);

            // Assert
            Assert.IsNotNull(pidls, "Pidls expected to not be null");
            foreach (var pidl in pidls)
            {
                var saveButton = pidl.GetDisplayHintById(buttonHintId);
                Assert.IsNotNull(saveButton);
                Assert.IsNotNull(saveButton.Action.Context);

                Assert.IsTrue(saveButton.Action.Context.ToString().Contains("classicProduct=azureClassic&billableAccountId=TGTAeQAAAAAAAAAA"));
            }
        }

        [DataRow("triggerSubmit", true, "0.99", "applepay", "en-US")]
        [DataRow("triggerSubmit", true, "0.99", "applepay", "it-IT")]
        [DataRow("triggerSubmit", true, "0.99", "googlepay", "en-US")]
        [DataRow("triggerSubmit", true, "0.99", "googlepay", "it-IT")]
        [DataRow("triggerSubmit", true, "5.99", "*", "en-US")]
        [DataRow("triggerSubmit", true, "5.99", null, "en-US")]
        [DataTestMethod]
        public async Task GetExpressCheckoutPaymentMethodDescriptions(string expectedValidation, bool isPSD2, string amount, string paymentMethodType, string language)
        {
            // add config change for email and address
            // Arrange
            int arrayIndexValue = 0;
            string requestUrl = $"/v7.0/Account001/paymentMethodDescriptions?partner=candycrush&operation=ExpressCheckout&country=US&currency=USD&expressCheckoutData=%7B%22amount%22%3A" + amount + "%2C%22country%22%3A%22US%22%2C%22language%22%3A%22" + language + "%22%2C%22currency%22%3A%22USD%22%2C%22topDomainUrl%22%3A%22%22%7D";
            var flights = "PXUsePartnerSettingsService";
            string expectedPSSResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\",\"resources\":null,\"features\":{\"singleMarketDirective\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null},\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"moveCardNumberBeforeCardHolderName\":false,\"moveOrganizationNameBeforeEmailAddress\":false,\"moveLastNameBeforeFirstName\":null,\"setSaveButtonDisplayContentAsNext\":false,\"removeStarRequiredTextGroup\":null,\"updateCvvChallengeTextForGCO\":false,\"enableIsSelectPMskippedValue\":null,\"enableCountryAddorUpdateCC\":false,\"hideCountryDropdown\":null,\"hideFirstAndLastNameForCompletePrerequisites\":false,\"hideAddCreditDebitCardHeading\":false,\"updatePaymentMethodHeadingTypeToText\":null,\"removeAddCreditDebitCardHeading\":null,\"setGroupedSelectOptionTextBeforeLogo\":null,\"setSelectPMWithLogo\":null,\"useTextForCVVHelpLink\":null,\"cvvDisplayHelpPosition\":null,\"matchSelectPMMainPageStructureForSubPage\":null,\"useFixedSVGForMC\":null,\"enableIndia3dsForNonZeroPaymentTransaction\":null,\"pxEnableIndia3DS1Challenge\":null,\"india3dsEnableForBilldesk\":null,\"usePSSForPXFeatureFlighting\":null,\"enableSecureFieldAddCC\":false,\"setSaveButtonDisplayContentAsBook\":false,\"removeCancelButton\":false,\"removeSelectPiEditButton\":null,\"removeSelectPiNewPaymentMethodLink\":null,\"removeSpaceInPrivacyTextGroup\":null,\"setBackButtonDisplayContentAsCancel\":false,\"setPrivacyStatementHyperLinkDisplayToButton\":null,\"hidePaymentSummaryText\":false,\"hidepaymentOptionSaveText\":false,\"addressType\":\"billing\",\"dataSource\":\"jarvis\",\"submitActionType\":null,\"fieldsToBeHidden\":null,\"fieldsToBeEnabled\":null,\"fieldsToMakeRequired\":null,\"fieldsToBeRemoved\":null,\"removeOptionalTextFromFields\":null,\"psd2IgnorePIAuthorization\":null,\"addressSuggestionMessage\":false,\"updateAccessibilityNameWithPosition\":null,\"disableSelectPiRadioOption\":null,\"updateSelectPiButtonText\":null,\"removeGroupForExpiryMonthAndYear\":null,\"useAddressDataSourceForUpdate\":null,\"disableCountryDropdown\":null,\"ungroupAddressFirstNameLastName\":null,\"endPoint\":null,\"operation\":null,\"profileType\":null,\"dataFieldsToRemoveFromPayload\":null,\"dataFieldsToRemoveFullPath\":null,\"convertProfileTypeTo\":null,\"fieldsToBeDisabled\":null,\"hidePaymentMethodHeading\":null,\"hideChangeSettingText\":null,\"removeEwalletYesButtons\":null,\"removeEwalletBackButtons\":null,\"removeDefaultStyleHints\":null,\"useSuggestAddressesTradeAVSV1Scenario\":null,\"addCCAddressValidationPidlModification\":null,\"verifyAddressPidlModification\":null,\"displayTagsToBeRemoved\":null,\"replaceContextInstanceWithPaymentInstrumentId\":null,\"enablePlaceholder\":null,\"hideAcceptCardMessage\":null,\"addAccessibilityNameExpressionToNegativeValue\":null,\"removeAnotherDeviceTextFromShortUrlInstruction\":null,\"displayShortUrlAsHyperlink\":null,\"hideAddressCheckBoxIfAddressIsNotPrefilledFromServer\":null,\"fieldsToSetIsSubmitGroupFalse\":null,\"removeMicrosoftPrivacyTextGroup\":null,\"hideCardLogos\":null,\"hideAddress\":null,\"addCancelButton\":false,\"displayShortUrlAsVertical\":null,\"setSubmitURLToEmptyForTaxId\":null,\"addressSuggestion\":null,\"updateXboxElementsAccessibilityHints\":null,\"setCancelButtonDisplayContentAsBack\":false,\"setButtonDisplayContent\":null,\"updateConsumerProfileSubmitLinkToJarvisPatch\":null,\"removeDataSourceResources\":null,\"useIFrameForPiLogOn\":null,\"removeAddressFormHeading\":null,\"displayAccentBorderOnButtonFocus\":null,\"addStyleHints\":null,\"removeStyleHints\":null,\"removeAcceptCardMessage\":null,\"addAllFieldsRequiredText\":null}]},\"chinaAllowVisaMasterCard\":{\"applicableMarkets\":[\"cn\"],\"displayCustomizationDetail\":null}}},\"select\":{\"template\":\"selectpmradiobuttonlist\",\"redirectionPattern\":null,\"resources\":null,\"features\":{\"chinaAllowVisaMasterCard\":{\"applicableMarkets\":[\"cn\"],\"displayCustomizationDetail\":null}}},\"selectinstance\":{\"template\":\"listpidropdown\",\"redirectionPattern\":null,\"resources\":null,\"features\":{\"showPIExpirationInformation\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null},\"removeElement\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"moveCardNumberBeforeCardHolderName\":false,\"moveOrganizationNameBeforeEmailAddress\":false,\"moveLastNameBeforeFirstName\":null,\"setSaveButtonDisplayContentAsNext\":false,\"removeStarRequiredTextGroup\":null,\"updateCvvChallengeTextForGCO\":false,\"enableIsSelectPMskippedValue\":null,\"enableCountryAddorUpdateCC\":false,\"hideCountryDropdown\":null,\"hideFirstAndLastNameForCompletePrerequisites\":false,\"hideAddCreditDebitCardHeading\":false,\"updatePaymentMethodHeadingTypeToText\":null,\"removeAddCreditDebitCardHeading\":null,\"setGroupedSelectOptionTextBeforeLogo\":null,\"setSelectPMWithLogo\":null,\"useTextForCVVHelpLink\":null,\"cvvDisplayHelpPosition\":null,\"matchSelectPMMainPageStructureForSubPage\":null,\"useFixedSVGForMC\":null,\"enableIndia3dsForNonZeroPaymentTransaction\":null,\"pxEnableIndia3DS1Challenge\":null,\"india3dsEnableForBilldesk\":null,\"usePSSForPXFeatureFlighting\":null,\"enableSecureFieldAddCC\":false,\"setSaveButtonDisplayContentAsBook\":false,\"removeCancelButton\":false,\"removeSelectPiEditButton\":null,\"removeSelectPiNewPaymentMethodLink\":true,\"removeSpaceInPrivacyTextGroup\":null,\"setBackButtonDisplayContentAsCancel\":false,\"setPrivacyStatementHyperLinkDisplayToButton\":null,\"hidePaymentSummaryText\":false,\"hidepaymentOptionSaveText\":false,\"addressType\":null,\"dataSource\":null,\"submitActionType\":null,\"fieldsToBeHidden\":null,\"fieldsToBeEnabled\":null,\"fieldsToMakeRequired\":null,\"fieldsToBeRemoved\":null,\"removeOptionalTextFromFields\":null,\"psd2IgnorePIAuthorization\":null,\"addressSuggestionMessage\":false,\"updateAccessibilityNameWithPosition\":null,\"disableSelectPiRadioOption\":null,\"updateSelectPiButtonText\":null,\"removeGroupForExpiryMonthAndYear\":null,\"useAddressDataSourceForUpdate\":null,\"disableCountryDropdown\":null,\"ungroupAddressFirstNameLastName\":null,\"endPoint\":null,\"operation\":null,\"profileType\":null,\"dataFieldsToRemoveFromPayload\":null,\"dataFieldsToRemoveFullPath\":null,\"convertProfileTypeTo\":null,\"fieldsToBeDisabled\":null,\"hidePaymentMethodHeading\":null,\"hideChangeSettingText\":null,\"removeEwalletYesButtons\":null,\"removeEwalletBackButtons\":null,\"removeDefaultStyleHints\":null,\"useSuggestAddressesTradeAVSV1Scenario\":null,\"addCCAddressValidationPidlModification\":null,\"verifyAddressPidlModification\":null,\"displayTagsToBeRemoved\":null,\"replaceContextInstanceWithPaymentInstrumentId\":null,\"enablePlaceholder\":null,\"hideAcceptCardMessage\":null,\"addAccessibilityNameExpressionToNegativeValue\":null,\"removeAnotherDeviceTextFromShortUrlInstruction\":null,\"displayShortUrlAsHyperlink\":null,\"hideAddressCheckBoxIfAddressIsNotPrefilledFromServer\":null,\"fieldsToSetIsSubmitGroupFalse\":null,\"removeMicrosoftPrivacyTextGroup\":null,\"hideCardLogos\":null,\"hideAddress\":null,\"addCancelButton\":false,\"displayShortUrlAsVertical\":null,\"setSubmitURLToEmptyForTaxId\":null,\"addressSuggestion\":null,\"updateXboxElementsAccessibilityHints\":null,\"setCancelButtonDisplayContentAsBack\":false,\"setButtonDisplayContent\":null,\"updateConsumerProfileSubmitLinkToJarvisPatch\":null,\"removeDataSourceResources\":null,\"useIFrameForPiLogOn\":null,\"removeAddressFormHeading\":null,\"displayAccentBorderOnButtonFocus\":null,\"addStyleHints\":null,\"removeStyleHints\":null,\"removeAcceptCardMessage\":null,\"addAllFieldsRequiredText\":null}]},\"chinaAllowVisaMasterCard\":{\"applicableMarkets\":[\"cn\"],\"displayCustomizationDetail\":null}}},\"handlepaymentchallenge\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\",\"resources\":null,\"features\":{\"threeDSOne\":{\"applicableMarkets\":[\"in\"],\"displayCustomizationDetail\":[{\"moveCardNumberBeforeCardHolderName\":false,\"moveOrganizationNameBeforeEmailAddress\":false,\"moveLastNameBeforeFirstName\":null,\"setSaveButtonDisplayContentAsNext\":false,\"removeStarRequiredTextGroup\":null,\"updateCvvChallengeTextForGCO\":false,\"enableIsSelectPMskippedValue\":null,\"enableCountryAddorUpdateCC\":false,\"hideCountryDropdown\":null,\"hideFirstAndLastNameForCompletePrerequisites\":false,\"hideAddCreditDebitCardHeading\":false,\"updatePaymentMethodHeadingTypeToText\":null,\"removeAddCreditDebitCardHeading\":null,\"setGroupedSelectOptionTextBeforeLogo\":null,\"setSelectPMWithLogo\":null,\"useTextForCVVHelpLink\":null,\"cvvDisplayHelpPosition\":null,\"matchSelectPMMainPageStructureForSubPage\":null,\"useFixedSVGForMC\":null,\"enableIndia3dsForNonZeroPaymentTransaction\":false,\"pxEnableIndia3DS1Challenge\":true,\"india3dsEnableForBilldesk\":true,\"usePSSForPXFeatureFlighting\":null,\"enableSecureFieldAddCC\":false,\"setSaveButtonDisplayContentAsBook\":false,\"removeCancelButton\":false,\"removeSelectPiEditButton\":null,\"removeSelectPiNewPaymentMethodLink\":null,\"removeSpaceInPrivacyTextGroup\":null,\"setBackButtonDisplayContentAsCancel\":false,\"setPrivacyStatementHyperLinkDisplayToButton\":null,\"hidePaymentSummaryText\":false,\"hidepaymentOptionSaveText\":false,\"addressType\":null,\"dataSource\":null,\"submitActionType\":null,\"fieldsToBeHidden\":null,\"fieldsToBeEnabled\":null,\"fieldsToMakeRequired\":null,\"fieldsToBeRemoved\":null,\"removeOptionalTextFromFields\":null,\"psd2IgnorePIAuthorization\":null,\"addressSuggestionMessage\":false,\"updateAccessibilityNameWithPosition\":null,\"disableSelectPiRadioOption\":null,\"updateSelectPiButtonText\":null,\"removeGroupForExpiryMonthAndYear\":null,\"useAddressDataSourceForUpdate\":null,\"disableCountryDropdown\":null,\"ungroupAddressFirstNameLastName\":null,\"endPoint\":null,\"operation\":null,\"profileType\":null,\"dataFieldsToRemoveFromPayload\":null,\"dataFieldsToRemoveFullPath\":null,\"convertProfileTypeTo\":null,\"fieldsToBeDisabled\":null,\"hidePaymentMethodHeading\":null,\"hideChangeSettingText\":null,\"removeEwalletYesButtons\":null,\"removeEwalletBackButtons\":null,\"removeDefaultStyleHints\":null,\"useSuggestAddressesTradeAVSV1Scenario\":null,\"addCCAddressValidationPidlModification\":null,\"verifyAddressPidlModification\":null,\"displayTagsToBeRemoved\":null,\"replaceContextInstanceWithPaymentInstrumentId\":null,\"enablePlaceholder\":null,\"hideAcceptCardMessage\":null,\"addAccessibilityNameExpressionToNegativeValue\":null,\"removeAnotherDeviceTextFromShortUrlInstruction\":null,\"displayShortUrlAsHyperlink\":null,\"hideAddressCheckBoxIfAddressIsNotPrefilledFromServer\":null,\"fieldsToSetIsSubmitGroupFalse\":null,\"removeMicrosoftPrivacyTextGroup\":null,\"hideCardLogos\":null,\"hideAddress\":null,\"addCancelButton\":false,\"displayShortUrlAsVertical\":null,\"setSubmitURLToEmptyForTaxId\":null,\"addressSuggestion\":null,\"updateXboxElementsAccessibilityHints\":null,\"setCancelButtonDisplayContentAsBack\":false,\"setButtonDisplayContent\":null,\"updateConsumerProfileSubmitLinkToJarvisPatch\":null,\"removeDataSourceResources\":null,\"useIFrameForPiLogOn\":null,\"removeAddressFormHeading\":null,\"displayAccentBorderOnButtonFocus\":null,\"addStyleHints\":null,\"removeStyleHints\":null,\"removeAcceptCardMessage\":null,\"addAllFieldsRequiredText\":null}]},\"PSD2\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"moveCardNumberBeforeCardHolderName\":false,\"moveOrganizationNameBeforeEmailAddress\":false,\"moveLastNameBeforeFirstName\":null,\"setSaveButtonDisplayContentAsNext\":false,\"removeStarRequiredTextGroup\":null,\"updateCvvChallengeTextForGCO\":false,\"enableIsSelectPMskippedValue\":null,\"enableCountryAddorUpdateCC\":false,\"hideCountryDropdown\":null,\"hideFirstAndLastNameForCompletePrerequisites\":false,\"hideAddCreditDebitCardHeading\":false,\"updatePaymentMethodHeadingTypeToText\":null,\"removeAddCreditDebitCardHeading\":null,\"setGroupedSelectOptionTextBeforeLogo\":null,\"setSelectPMWithLogo\":null,\"useTextForCVVHelpLink\":null,\"cvvDisplayHelpPosition\":null,\"matchSelectPMMainPageStructureForSubPage\":null,\"useFixedSVGForMC\":null,\"enableIndia3dsForNonZeroPaymentTransaction\":null,\"pxEnableIndia3DS1Challenge\":null,\"india3dsEnableForBilldesk\":null,\"usePSSForPXFeatureFlighting\":null,\"enableSecureFieldAddCC\":false,\"setSaveButtonDisplayContentAsBook\":false,\"removeCancelButton\":false,\"removeSelectPiEditButton\":null,\"removeSelectPiNewPaymentMethodLink\":null,\"removeSpaceInPrivacyTextGroup\":null,\"setBackButtonDisplayContentAsCancel\":false,\"setPrivacyStatementHyperLinkDisplayToButton\":null,\"hidePaymentSummaryText\":false,\"hidepaymentOptionSaveText\":false,\"addressType\":null,\"dataSource\":null,\"submitActionType\":null,\"fieldsToBeHidden\":null,\"fieldsToBeEnabled\":null,\"fieldsToMakeRequired\":null,\"fieldsToBeRemoved\":null,\"removeOptionalTextFromFields\":null,\"psd2IgnorePIAuthorization\":true,\"addressSuggestionMessage\":false,\"updateAccessibilityNameWithPosition\":null,\"disableSelectPiRadioOption\":null,\"updateSelectPiButtonText\":null,\"removeGroupForExpiryMonthAndYear\":null,\"useAddressDataSourceForUpdate\":null,\"disableCountryDropdown\":null,\"ungroupAddressFirstNameLastName\":null,\"endPoint\":null,\"operation\":null,\"profileType\":null,\"dataFieldsToRemoveFromPayload\":null,\"dataFieldsToRemoveFullPath\":null,\"convertProfileTypeTo\":null,\"fieldsToBeDisabled\":null,\"hidePaymentMethodHeading\":null,\"hideChangeSettingText\":null,\"removeEwalletYesButtons\":null,\"removeEwalletBackButtons\":null,\"removeDefaultStyleHints\":null,\"useSuggestAddressesTradeAVSV1Scenario\":null,\"addCCAddressValidationPidlModification\":null,\"verifyAddressPidlModification\":null,\"displayTagsToBeRemoved\":null,\"replaceContextInstanceWithPaymentInstrumentId\":null,\"enablePlaceholder\":null,\"hideAcceptCardMessage\":null,\"addAccessibilityNameExpressionToNegativeValue\":null,\"removeAnotherDeviceTextFromShortUrlInstruction\":null,\"displayShortUrlAsHyperlink\":null,\"hideAddressCheckBoxIfAddressIsNotPrefilledFromServer\":null,\"fieldsToSetIsSubmitGroupFalse\":null,\"removeMicrosoftPrivacyTextGroup\":null,\"hideCardLogos\":null,\"hideAddress\":null,\"addCancelButton\":false,\"displayShortUrlAsVertical\":null,\"setSubmitURLToEmptyForTaxId\":null,\"addressSuggestion\":null,\"updateXboxElementsAccessibilityHints\":null,\"setCancelButtonDisplayContentAsBack\":false,\"setButtonDisplayContent\":null,\"updateConsumerProfileSubmitLinkToJarvisPatch\":null,\"removeDataSourceResources\":null,\"useIFrameForPiLogOn\":null,\"removeAddressFormHeading\":null,\"displayAccentBorderOnButtonFocus\":null,\"addStyleHints\":null,\"removeStyleHints\":null,\"removeAcceptCardMessage\":null,\"addAllFieldsRequiredText\":null}]}}},\"default\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":null,\"resources\":null,\"features\":null}}";
            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            string expectedAPPIMSResponse = "[{\"paymentMethodType\":\"applepay\",\"properties\":{\"offlineRecurring\":true,\"userManaged\":false,\"chargeThresholds\":null,\"redirectRequired\":null,\"soldToAddressRequired\":true,\"splitPaymentSupported\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"providerCountry\":null,\"nonStoredPaymentMethodId\":\"be4de87d-7e38-4b2d-8836-9237eb32848e\",\"isNonStoredPaymentMethod\":true},\"paymentMethodGroup\":\"ewallet\",\"groupDisplayName\":\"eWallet\",\"exclusionTags\":null,\"paymentMethodFamily\":\"ewallet\",\"display\":{\"name\":\"ApplePay\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_applepay.svg\",\"logos\":[{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_applepay.svg\"}]},\"AdditionalDisplayText\":null}]";
            string expectedGPPIMSResponse = "[{\"paymentMethodType\":\"googlepay\",\"properties\":{\"offlineRecurring\":true,\"userManaged\":false,\"chargeThresholds\":null,\"redirectRequired\":null,\"soldToAddressRequired\":true,\"splitPaymentSupported\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"providerCountry\":null,\"nonStoredPaymentMethodId\":\"cdc85313-9b57-4052-81fb-dea336132cbf\",\"isNonStoredPaymentMethod\":true},\"paymentMethodGroup\":\"ewallet\",\"groupDisplayName\":\"eWallet\",\"exclusionTags\":null,\"paymentMethodFamily\":\"ewallet\",\"display\":{\"name\":\"GooglePay\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_googlepay.svg\",\"logos\":[{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_googlepay.svg\"}]},\"AdditionalDisplayText\":null}]";
            string expectedPIMSResponse = "[{\"paymentMethodType\":\"applepay\",\"properties\":{\"offlineRecurring\":true,\"userManaged\":false,\"chargeThresholds\":null,\"redirectRequired\":null,\"soldToAddressRequired\":true,\"splitPaymentSupported\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"providerCountry\":null,\"nonStoredPaymentMethodId\":\"be4de87d-7e38-4b2d-8836-9237eb32848e\",\"isNonStoredPaymentMethod\":true},\"paymentMethodGroup\":\"ewallet\",\"groupDisplayName\":\"eWallet\",\"exclusionTags\":null,\"paymentMethodFamily\":\"ewallet\",\"display\":{\"name\":\"ApplePay\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_applepay.svg\",\"logos\":[{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_applepay.svg\"}]},\"AdditionalDisplayText\":null},{\"paymentMethodType\":\"googlepay\",\"properties\":{\"offlineRecurring\":true,\"userManaged\":false,\"chargeThresholds\":null,\"redirectRequired\":null,\"soldToAddressRequired\":true,\"splitPaymentSupported\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"providerCountry\":null,\"nonStoredPaymentMethodId\":\"cdc85313-9b57-4052-81fb-dea336132cbf\",\"isNonStoredPaymentMethod\":true},\"paymentMethodGroup\":\"ewallet\",\"groupDisplayName\":\"eWallet\",\"exclusionTags\":null,\"paymentMethodFamily\":\"ewallet\",\"display\":{\"name\":\"GooglePay\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_googlepay.svg\",\"logos\":[{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_googlepay.svg\"}]},\"AdditionalDisplayText\":null}]";

            if (paymentMethodType?.Equals("applepay", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                arrayIndexValue = 0;
                PXSettings.PimsService.ArrangeResponse(expectedAPPIMSResponse);
            }
            else if (paymentMethodType?.Equals("googlepay", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                arrayIndexValue = 1;
                PXSettings.PimsService.ArrangeResponse(expectedGPPIMSResponse);
            }
            else if (paymentMethodType?.Equals("*", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                PXSettings.PimsService.ArrangeResponse(expectedPIMSResponse);
            }

            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "x-ms-flight", flights },
            };

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(requestUrl, paymentMethodType == null ? HttpStatusCode.BadRequest : HttpStatusCode.OK, additionaHeaders: headers);

            // Assert
            Assert.IsNotNull(pidls, "Pidls expected to not be null");
            foreach (var pidl in pidls)
            {
                Assert.IsNotNull(pidl.DataSources, "PIDL data source should not be null for express checkout");

                var dataSource = pidl.DataSources["walletConfig"];
                Assert.IsNotNull(dataSource, "PIDL data source should not be null for express checkout");

                var walletConfig = JsonConvert.DeserializeObject<WalletConfig>(JsonConvert.SerializeObject(dataSource.Members.FirstOrDefault()));
                Assert.IsNotNull(walletConfig, "WalletConfig should not be null for express checkout");
                Assert.IsNotNull(walletConfig.PIDLConfig, "PIDLConfig should not be null for express checkout");
                Assert.IsTrue(walletConfig.PaymentInstrumentHandlers[0].EnableEmail, "Googlepay handler EnableEmail should be true");
                Assert.IsTrue(walletConfig.PaymentInstrumentHandlers[1].EnableEmail, "Applepay handler EnableEmail should be true");
                Assert.IsTrue(walletConfig.PaymentInstrumentHandlers[0].EnableBillingAddress, "Googlepay handler EnableBillingAddress should be true");
                Assert.IsTrue(walletConfig.PaymentInstrumentHandlers[1].EnableBillingAddress, "Applepay handler EnableBillingAddress should be true");
                Assert.IsNotNull(walletConfig.PaymentInstrumentHandlers[arrayIndexValue].DisableGeoFencing);
                Assert.IsFalse(walletConfig.PaymentInstrumentHandlers[arrayIndexValue].DisableGeoFencing, "DisableGeoFencing should be false for express checkout");
                Assert.IsNotNull(walletConfig.PaymentInstrumentHandlers[arrayIndexValue].SingleMarkets);
                Assert.IsTrue(walletConfig.PaymentInstrumentHandlers[arrayIndexValue].SingleMarkets.Count > 0, "SmdMarkets should not be empty for express checkout");

                if (language?.Equals("it-IT", StringComparison.OrdinalIgnoreCase) ?? false)
                {
                    Assert.AreEqual("importo dovuto più imposte applicabili", walletConfig.PaymentInstrumentHandlers[0].PayLabel, "Googlepay handler Paylabel should be importo dovuto più le tasse applicabili");
                    Assert.AreEqual("importo dovuto più imposte applicabili", walletConfig.PaymentInstrumentHandlers[1].PayLabel, "Applepay handler Paylabel should be importo dovuto più le tasse applicabili");
                }
                else
                {
                    Assert.AreEqual("amount due plus applicable taxes", walletConfig.PaymentInstrumentHandlers[0].PayLabel, "Googlepay handler Paylabel should be amount due plus applicable taxes");
                    Assert.AreEqual("amount due plus applicable taxes", walletConfig.PaymentInstrumentHandlers[1].PayLabel, "Applepay handler Paylabel should be amount due plus applicable taxes");
                }

                if (isPSD2)
                {
                    Assert.IsTrue(walletConfig.PaymentInstrumentHandlers?.FirstOrDefault()?.AllowedAuthMethodsPerCountry?.ContainsKey("gb"));
                    Assert.IsTrue(walletConfig.PaymentInstrumentHandlers?.LastOrDefault()?.AllowedAuthMethodsPerCountry?.ContainsKey("gb"));
                }

                Assert.IsNotNull(pidl.DisplayPages, "PIDL display pages should not be null for express checkout");

                var googlePayButton = pidl.GetDisplayHintById("googlepayExpressCheckoutFrame") as ExpressCheckoutButtonDisplayHint;
                var applePayButton = pidl.GetDisplayHintById("applepayExpressCheckoutFrame") as ExpressCheckoutButtonDisplayHint;

                if ((paymentMethodType?.Equals("*", StringComparison.OrdinalIgnoreCase) ?? false)
                    || (paymentMethodType?.Equals("applepay", StringComparison.OrdinalIgnoreCase) ?? false))
                {
                    Assert.IsNotNull(applePayButton, "Apple pay button should not be null");
                    Assert.AreEqual(expectedValidation, Convert.ToString(applePayButton.Payload["actionType"]), "Action type for express checkout should be success");
                    Assert.AreEqual(amount, Convert.ToString(applePayButton.Payload["amount"]), "Apple pay amount for express checkout is not expected");
                    Assert.AreEqual(language, Convert.ToString(applePayButton.Payload["language"]), "language should be en-US");

                    Assert.IsNull(paymentMethodType.Equals("applepay", StringComparison.OrdinalIgnoreCase) ? googlePayButton : null, "Google pay button should be null");
                }

                if ((paymentMethodType?.Equals("*", StringComparison.OrdinalIgnoreCase) ?? false)
                    || (paymentMethodType?.Equals("googlepay", StringComparison.OrdinalIgnoreCase) ?? false))
                {
                    Assert.IsNotNull(googlePayButton, "Google pay button should not be null");
                    Assert.AreEqual(expectedValidation, Convert.ToString(googlePayButton.Payload["actionType"]), "Action type for express checkout should be success");
                    Assert.AreEqual(amount, Convert.ToString(googlePayButton.Payload["amount"]), "Google pay amount for express checkout is not expected");
                    Assert.AreEqual(language.Split('-').FirstOrDefault(), Convert.ToString(googlePayButton.Payload["language"]), "language should be en");

                    Assert.IsNull(paymentMethodType.Equals("googlepay", StringComparison.OrdinalIgnoreCase) ? applePayButton : null, "Apple pay button should be null");
                }
            }
        }

        [DataRow("triggerSubmit", true, "0.99", "applepay", "en-US", "true")]
        [DataRow("triggerSubmit", true, "0.99", "applepay", "en-US", "false")]
        [DataRow("triggerSubmit", true, "0.99", "googlepay", "en-US", "true")]
        [DataRow("triggerSubmit", true, "0.99", "googlepay", "en-US", "false")]
        [DataRow("triggerSubmit", true, "5.99", "*", "en-US", "true")]
        [DataRow("triggerSubmit", true, "5.99", "*", "en-US", "false")]
        [DataRow("triggerSubmit", true, "5.99", null, "en-US", "true")]
        [DataRow("triggerSubmit", true, "5.99", null, "en-US", "false")]
        [DataTestMethod]
        public async Task GetExpressCheckoutPaymentMethodDescriptions_isTaxIncluded(string expectedValidation, bool isPSD2, string amount, string paymentMethodType, string language, string isTaxIncluded)
        {
            // Arrange
            int arrayIndexValue = 0;
            string requestUrl = $"/v7.0/Account001/paymentMethodDescriptions?partner=candycrush&operation=ExpressCheckout&country=US&currency=USD&expressCheckoutData=%7B%22amount%22%3A" + amount + "%2C%22country%22%3A%22US%22%2C%22language%22%3A%22" + language + "%22%2C%22currency%22%3A%22USD%22%2C%22isTaxIncluded%22%3A%22" + isTaxIncluded + "%22%2C%22topDomainUrl%22%3A%22%22%7D";
            var flights = "PXUsePartnerSettingsService";
            string expectedPSSResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\",\"resources\":null,\"features\":{\"singleMarketDirective\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null},\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"moveCardNumberBeforeCardHolderName\":false,\"moveOrganizationNameBeforeEmailAddress\":false,\"moveLastNameBeforeFirstName\":null,\"setSaveButtonDisplayContentAsNext\":false,\"removeStarRequiredTextGroup\":null,\"updateCvvChallengeTextForGCO\":false,\"enableIsSelectPMskippedValue\":null,\"enableCountryAddorUpdateCC\":false,\"hideCountryDropdown\":null,\"hideFirstAndLastNameForCompletePrerequisites\":false,\"hideAddCreditDebitCardHeading\":false,\"updatePaymentMethodHeadingTypeToText\":null,\"removeAddCreditDebitCardHeading\":null,\"setGroupedSelectOptionTextBeforeLogo\":null,\"setSelectPMWithLogo\":null,\"useTextForCVVHelpLink\":null,\"cvvDisplayHelpPosition\":null,\"matchSelectPMMainPageStructureForSubPage\":null,\"useFixedSVGForMC\":null,\"enableIndia3dsForNonZeroPaymentTransaction\":null,\"pxEnableIndia3DS1Challenge\":null,\"india3dsEnableForBilldesk\":null,\"usePSSForPXFeatureFlighting\":null,\"enableSecureFieldAddCC\":false,\"setSaveButtonDisplayContentAsBook\":false,\"removeCancelButton\":false,\"removeSelectPiEditButton\":null,\"removeSelectPiNewPaymentMethodLink\":null,\"removeSpaceInPrivacyTextGroup\":null,\"setBackButtonDisplayContentAsCancel\":false,\"setPrivacyStatementHyperLinkDisplayToButton\":null,\"hidePaymentSummaryText\":false,\"hidepaymentOptionSaveText\":false,\"addressType\":\"billing\",\"dataSource\":\"jarvis\",\"submitActionType\":null,\"fieldsToBeHidden\":null,\"fieldsToBeEnabled\":null,\"fieldsToMakeRequired\":null,\"fieldsToBeRemoved\":null,\"removeOptionalTextFromFields\":null,\"psd2IgnorePIAuthorization\":null,\"addressSuggestionMessage\":false,\"updateAccessibilityNameWithPosition\":null,\"disableSelectPiRadioOption\":null,\"updateSelectPiButtonText\":null,\"removeGroupForExpiryMonthAndYear\":null,\"useAddressDataSourceForUpdate\":null,\"disableCountryDropdown\":null,\"ungroupAddressFirstNameLastName\":null,\"endPoint\":null,\"operation\":null,\"profileType\":null,\"dataFieldsToRemoveFromPayload\":null,\"dataFieldsToRemoveFullPath\":null,\"convertProfileTypeTo\":null,\"fieldsToBeDisabled\":null,\"hidePaymentMethodHeading\":null,\"hideChangeSettingText\":null,\"removeEwalletYesButtons\":null,\"removeEwalletBackButtons\":null,\"removeDefaultStyleHints\":null,\"useSuggestAddressesTradeAVSV1Scenario\":null,\"addCCAddressValidationPidlModification\":null,\"verifyAddressPidlModification\":null,\"displayTagsToBeRemoved\":null,\"replaceContextInstanceWithPaymentInstrumentId\":null,\"enablePlaceholder\":null,\"hideAcceptCardMessage\":null,\"addAccessibilityNameExpressionToNegativeValue\":null,\"removeAnotherDeviceTextFromShortUrlInstruction\":null,\"displayShortUrlAsHyperlink\":null,\"hideAddressCheckBoxIfAddressIsNotPrefilledFromServer\":null,\"fieldsToSetIsSubmitGroupFalse\":null,\"removeMicrosoftPrivacyTextGroup\":null,\"hideCardLogos\":null,\"hideAddress\":null,\"addCancelButton\":false,\"displayShortUrlAsVertical\":null,\"setSubmitURLToEmptyForTaxId\":null,\"addressSuggestion\":null,\"updateXboxElementsAccessibilityHints\":null,\"setCancelButtonDisplayContentAsBack\":false,\"setButtonDisplayContent\":null,\"updateConsumerProfileSubmitLinkToJarvisPatch\":null,\"removeDataSourceResources\":null,\"useIFrameForPiLogOn\":null,\"removeAddressFormHeading\":null,\"displayAccentBorderOnButtonFocus\":null,\"addStyleHints\":null,\"removeStyleHints\":null,\"removeAcceptCardMessage\":null,\"addAllFieldsRequiredText\":null}]},\"chinaAllowVisaMasterCard\":{\"applicableMarkets\":[\"cn\"],\"displayCustomizationDetail\":null}}},\"select\":{\"template\":\"selectpmradiobuttonlist\",\"redirectionPattern\":null,\"resources\":null,\"features\":{\"chinaAllowVisaMasterCard\":{\"applicableMarkets\":[\"cn\"],\"displayCustomizationDetail\":null}}},\"selectinstance\":{\"template\":\"listpidropdown\",\"redirectionPattern\":null,\"resources\":null,\"features\":{\"showPIExpirationInformation\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null},\"removeElement\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"moveCardNumberBeforeCardHolderName\":false,\"moveOrganizationNameBeforeEmailAddress\":false,\"moveLastNameBeforeFirstName\":null,\"setSaveButtonDisplayContentAsNext\":false,\"removeStarRequiredTextGroup\":null,\"updateCvvChallengeTextForGCO\":false,\"enableIsSelectPMskippedValue\":null,\"enableCountryAddorUpdateCC\":false,\"hideCountryDropdown\":null,\"hideFirstAndLastNameForCompletePrerequisites\":false,\"hideAddCreditDebitCardHeading\":false,\"updatePaymentMethodHeadingTypeToText\":null,\"removeAddCreditDebitCardHeading\":null,\"setGroupedSelectOptionTextBeforeLogo\":null,\"setSelectPMWithLogo\":null,\"useTextForCVVHelpLink\":null,\"cvvDisplayHelpPosition\":null,\"matchSelectPMMainPageStructureForSubPage\":null,\"useFixedSVGForMC\":null,\"enableIndia3dsForNonZeroPaymentTransaction\":null,\"pxEnableIndia3DS1Challenge\":null,\"india3dsEnableForBilldesk\":null,\"usePSSForPXFeatureFlighting\":null,\"enableSecureFieldAddCC\":false,\"setSaveButtonDisplayContentAsBook\":false,\"removeCancelButton\":false,\"removeSelectPiEditButton\":null,\"removeSelectPiNewPaymentMethodLink\":true,\"removeSpaceInPrivacyTextGroup\":null,\"setBackButtonDisplayContentAsCancel\":false,\"setPrivacyStatementHyperLinkDisplayToButton\":null,\"hidePaymentSummaryText\":false,\"hidepaymentOptionSaveText\":false,\"addressType\":null,\"dataSource\":null,\"submitActionType\":null,\"fieldsToBeHidden\":null,\"fieldsToBeEnabled\":null,\"fieldsToMakeRequired\":null,\"fieldsToBeRemoved\":null,\"removeOptionalTextFromFields\":null,\"psd2IgnorePIAuthorization\":null,\"addressSuggestionMessage\":false,\"updateAccessibilityNameWithPosition\":null,\"disableSelectPiRadioOption\":null,\"updateSelectPiButtonText\":null,\"removeGroupForExpiryMonthAndYear\":null,\"useAddressDataSourceForUpdate\":null,\"disableCountryDropdown\":null,\"ungroupAddressFirstNameLastName\":null,\"endPoint\":null,\"operation\":null,\"profileType\":null,\"dataFieldsToRemoveFromPayload\":null,\"dataFieldsToRemoveFullPath\":null,\"convertProfileTypeTo\":null,\"fieldsToBeDisabled\":null,\"hidePaymentMethodHeading\":null,\"hideChangeSettingText\":null,\"removeEwalletYesButtons\":null,\"removeEwalletBackButtons\":null,\"removeDefaultStyleHints\":null,\"useSuggestAddressesTradeAVSV1Scenario\":null,\"addCCAddressValidationPidlModification\":null,\"verifyAddressPidlModification\":null,\"displayTagsToBeRemoved\":null,\"replaceContextInstanceWithPaymentInstrumentId\":null,\"enablePlaceholder\":null,\"hideAcceptCardMessage\":null,\"addAccessibilityNameExpressionToNegativeValue\":null,\"removeAnotherDeviceTextFromShortUrlInstruction\":null,\"displayShortUrlAsHyperlink\":null,\"hideAddressCheckBoxIfAddressIsNotPrefilledFromServer\":null,\"fieldsToSetIsSubmitGroupFalse\":null,\"removeMicrosoftPrivacyTextGroup\":null,\"hideCardLogos\":null,\"hideAddress\":null,\"addCancelButton\":false,\"displayShortUrlAsVertical\":null,\"setSubmitURLToEmptyForTaxId\":null,\"addressSuggestion\":null,\"updateXboxElementsAccessibilityHints\":null,\"setCancelButtonDisplayContentAsBack\":false,\"setButtonDisplayContent\":null,\"updateConsumerProfileSubmitLinkToJarvisPatch\":null,\"removeDataSourceResources\":null,\"useIFrameForPiLogOn\":null,\"removeAddressFormHeading\":null,\"displayAccentBorderOnButtonFocus\":null,\"addStyleHints\":null,\"removeStyleHints\":null,\"removeAcceptCardMessage\":null,\"addAllFieldsRequiredText\":null}]},\"chinaAllowVisaMasterCard\":{\"applicableMarkets\":[\"cn\"],\"displayCustomizationDetail\":null}}},\"handlepaymentchallenge\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\",\"resources\":null,\"features\":{\"threeDSOne\":{\"applicableMarkets\":[\"in\"],\"displayCustomizationDetail\":[{\"moveCardNumberBeforeCardHolderName\":false,\"moveOrganizationNameBeforeEmailAddress\":false,\"moveLastNameBeforeFirstName\":null,\"setSaveButtonDisplayContentAsNext\":false,\"removeStarRequiredTextGroup\":null,\"updateCvvChallengeTextForGCO\":false,\"enableIsSelectPMskippedValue\":null,\"enableCountryAddorUpdateCC\":false,\"hideCountryDropdown\":null,\"hideFirstAndLastNameForCompletePrerequisites\":false,\"hideAddCreditDebitCardHeading\":false,\"updatePaymentMethodHeadingTypeToText\":null,\"removeAddCreditDebitCardHeading\":null,\"setGroupedSelectOptionTextBeforeLogo\":null,\"setSelectPMWithLogo\":null,\"useTextForCVVHelpLink\":null,\"cvvDisplayHelpPosition\":null,\"matchSelectPMMainPageStructureForSubPage\":null,\"useFixedSVGForMC\":null,\"enableIndia3dsForNonZeroPaymentTransaction\":false,\"pxEnableIndia3DS1Challenge\":true,\"india3dsEnableForBilldesk\":true,\"usePSSForPXFeatureFlighting\":null,\"enableSecureFieldAddCC\":false,\"setSaveButtonDisplayContentAsBook\":false,\"removeCancelButton\":false,\"removeSelectPiEditButton\":null,\"removeSelectPiNewPaymentMethodLink\":null,\"removeSpaceInPrivacyTextGroup\":null,\"setBackButtonDisplayContentAsCancel\":false,\"setPrivacyStatementHyperLinkDisplayToButton\":null,\"hidePaymentSummaryText\":false,\"hidepaymentOptionSaveText\":false,\"addressType\":null,\"dataSource\":null,\"submitActionType\":null,\"fieldsToBeHidden\":null,\"fieldsToBeEnabled\":null,\"fieldsToMakeRequired\":null,\"fieldsToBeRemoved\":null,\"removeOptionalTextFromFields\":null,\"psd2IgnorePIAuthorization\":null,\"addressSuggestionMessage\":false,\"updateAccessibilityNameWithPosition\":null,\"disableSelectPiRadioOption\":null,\"updateSelectPiButtonText\":null,\"removeGroupForExpiryMonthAndYear\":null,\"useAddressDataSourceForUpdate\":null,\"disableCountryDropdown\":null,\"ungroupAddressFirstNameLastName\":null,\"endPoint\":null,\"operation\":null,\"profileType\":null,\"dataFieldsToRemoveFromPayload\":null,\"dataFieldsToRemoveFullPath\":null,\"convertProfileTypeTo\":null,\"fieldsToBeDisabled\":null,\"hidePaymentMethodHeading\":null,\"hideChangeSettingText\":null,\"removeEwalletYesButtons\":null,\"removeEwalletBackButtons\":null,\"removeDefaultStyleHints\":null,\"useSuggestAddressesTradeAVSV1Scenario\":null,\"addCCAddressValidationPidlModification\":null,\"verifyAddressPidlModification\":null,\"displayTagsToBeRemoved\":null,\"replaceContextInstanceWithPaymentInstrumentId\":null,\"enablePlaceholder\":null,\"hideAcceptCardMessage\":null,\"addAccessibilityNameExpressionToNegativeValue\":null,\"removeAnotherDeviceTextFromShortUrlInstruction\":null,\"displayShortUrlAsHyperlink\":null,\"hideAddressCheckBoxIfAddressIsNotPrefilledFromServer\":null,\"fieldsToSetIsSubmitGroupFalse\":null,\"removeMicrosoftPrivacyTextGroup\":null,\"hideCardLogos\":null,\"hideAddress\":null,\"addCancelButton\":false,\"displayShortUrlAsVertical\":null,\"setSubmitURLToEmptyForTaxId\":null,\"addressSuggestion\":null,\"updateXboxElementsAccessibilityHints\":null,\"setCancelButtonDisplayContentAsBack\":false,\"setButtonDisplayContent\":null,\"updateConsumerProfileSubmitLinkToJarvisPatch\":null,\"removeDataSourceResources\":null,\"useIFrameForPiLogOn\":null,\"removeAddressFormHeading\":null,\"displayAccentBorderOnButtonFocus\":null,\"addStyleHints\":null,\"removeStyleHints\":null,\"removeAcceptCardMessage\":null,\"addAllFieldsRequiredText\":null}]},\"PSD2\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"moveCardNumberBeforeCardHolderName\":false,\"moveOrganizationNameBeforeEmailAddress\":false,\"moveLastNameBeforeFirstName\":null,\"setSaveButtonDisplayContentAsNext\":false,\"removeStarRequiredTextGroup\":null,\"updateCvvChallengeTextForGCO\":false,\"enableIsSelectPMskippedValue\":null,\"enableCountryAddorUpdateCC\":false,\"hideCountryDropdown\":null,\"hideFirstAndLastNameForCompletePrerequisites\":false,\"hideAddCreditDebitCardHeading\":false,\"updatePaymentMethodHeadingTypeToText\":null,\"removeAddCreditDebitCardHeading\":null,\"setGroupedSelectOptionTextBeforeLogo\":null,\"setSelectPMWithLogo\":null,\"useTextForCVVHelpLink\":null,\"cvvDisplayHelpPosition\":null,\"matchSelectPMMainPageStructureForSubPage\":null,\"useFixedSVGForMC\":null,\"enableIndia3dsForNonZeroPaymentTransaction\":null,\"pxEnableIndia3DS1Challenge\":null,\"india3dsEnableForBilldesk\":null,\"usePSSForPXFeatureFlighting\":null,\"enableSecureFieldAddCC\":false,\"setSaveButtonDisplayContentAsBook\":false,\"removeCancelButton\":false,\"removeSelectPiEditButton\":null,\"removeSelectPiNewPaymentMethodLink\":null,\"removeSpaceInPrivacyTextGroup\":null,\"setBackButtonDisplayContentAsCancel\":false,\"setPrivacyStatementHyperLinkDisplayToButton\":null,\"hidePaymentSummaryText\":false,\"hidepaymentOptionSaveText\":false,\"addressType\":null,\"dataSource\":null,\"submitActionType\":null,\"fieldsToBeHidden\":null,\"fieldsToBeEnabled\":null,\"fieldsToMakeRequired\":null,\"fieldsToBeRemoved\":null,\"removeOptionalTextFromFields\":null,\"psd2IgnorePIAuthorization\":true,\"addressSuggestionMessage\":false,\"updateAccessibilityNameWithPosition\":null,\"disableSelectPiRadioOption\":null,\"updateSelectPiButtonText\":null,\"removeGroupForExpiryMonthAndYear\":null,\"useAddressDataSourceForUpdate\":null,\"disableCountryDropdown\":null,\"ungroupAddressFirstNameLastName\":null,\"endPoint\":null,\"operation\":null,\"profileType\":null,\"dataFieldsToRemoveFromPayload\":null,\"dataFieldsToRemoveFullPath\":null,\"convertProfileTypeTo\":null,\"fieldsToBeDisabled\":null,\"hidePaymentMethodHeading\":null,\"hideChangeSettingText\":null,\"removeEwalletYesButtons\":null,\"removeEwalletBackButtons\":null,\"removeDefaultStyleHints\":null,\"useSuggestAddressesTradeAVSV1Scenario\":null,\"addCCAddressValidationPidlModification\":null,\"verifyAddressPidlModification\":null,\"displayTagsToBeRemoved\":null,\"replaceContextInstanceWithPaymentInstrumentId\":null,\"enablePlaceholder\":null,\"hideAcceptCardMessage\":null,\"addAccessibilityNameExpressionToNegativeValue\":null,\"removeAnotherDeviceTextFromShortUrlInstruction\":null,\"displayShortUrlAsHyperlink\":null,\"hideAddressCheckBoxIfAddressIsNotPrefilledFromServer\":null,\"fieldsToSetIsSubmitGroupFalse\":null,\"removeMicrosoftPrivacyTextGroup\":null,\"hideCardLogos\":null,\"hideAddress\":null,\"addCancelButton\":false,\"displayShortUrlAsVertical\":null,\"setSubmitURLToEmptyForTaxId\":null,\"addressSuggestion\":null,\"updateXboxElementsAccessibilityHints\":null,\"setCancelButtonDisplayContentAsBack\":false,\"setButtonDisplayContent\":null,\"updateConsumerProfileSubmitLinkToJarvisPatch\":null,\"removeDataSourceResources\":null,\"useIFrameForPiLogOn\":null,\"removeAddressFormHeading\":null,\"displayAccentBorderOnButtonFocus\":null,\"addStyleHints\":null,\"removeStyleHints\":null,\"removeAcceptCardMessage\":null,\"addAllFieldsRequiredText\":null}]}}},\"default\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":null,\"resources\":null,\"features\":null}}";
            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            string expectedAPPIMSResponse = "[{\"paymentMethodType\":\"applepay\",\"properties\":{\"offlineRecurring\":true,\"userManaged\":false,\"chargeThresholds\":null,\"redirectRequired\":null,\"soldToAddressRequired\":true,\"splitPaymentSupported\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"providerCountry\":null,\"nonStoredPaymentMethodId\":\"be4de87d-7e38-4b2d-8836-9237eb32848e\",\"isNonStoredPaymentMethod\":true},\"paymentMethodGroup\":\"ewallet\",\"groupDisplayName\":\"eWallet\",\"exclusionTags\":null,\"paymentMethodFamily\":\"ewallet\",\"display\":{\"name\":\"ApplePay\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_applepay.svg\",\"logos\":[{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_applepay.svg\"}]},\"AdditionalDisplayText\":null}]";
            string expectedGPPIMSResponse = "[{\"paymentMethodType\":\"googlepay\",\"properties\":{\"offlineRecurring\":true,\"userManaged\":false,\"chargeThresholds\":null,\"redirectRequired\":null,\"soldToAddressRequired\":true,\"splitPaymentSupported\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"providerCountry\":null,\"nonStoredPaymentMethodId\":\"cdc85313-9b57-4052-81fb-dea336132cbf\",\"isNonStoredPaymentMethod\":true},\"paymentMethodGroup\":\"ewallet\",\"groupDisplayName\":\"eWallet\",\"exclusionTags\":null,\"paymentMethodFamily\":\"ewallet\",\"display\":{\"name\":\"GooglePay\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_googlepay.svg\",\"logos\":[{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_googlepay.svg\"}]},\"AdditionalDisplayText\":null}]";
            string expectedPIMSResponse = "[{\"paymentMethodType\":\"applepay\",\"properties\":{\"offlineRecurring\":true,\"userManaged\":false,\"chargeThresholds\":null,\"redirectRequired\":null,\"soldToAddressRequired\":true,\"splitPaymentSupported\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"providerCountry\":null,\"nonStoredPaymentMethodId\":\"be4de87d-7e38-4b2d-8836-9237eb32848e\",\"isNonStoredPaymentMethod\":true},\"paymentMethodGroup\":\"ewallet\",\"groupDisplayName\":\"eWallet\",\"exclusionTags\":null,\"paymentMethodFamily\":\"ewallet\",\"display\":{\"name\":\"ApplePay\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_applepay.svg\",\"logos\":[{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_applepay.svg\"}]},\"AdditionalDisplayText\":null},{\"paymentMethodType\":\"googlepay\",\"properties\":{\"offlineRecurring\":true,\"userManaged\":false,\"chargeThresholds\":null,\"redirectRequired\":null,\"soldToAddressRequired\":true,\"splitPaymentSupported\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"providerCountry\":null,\"nonStoredPaymentMethodId\":\"cdc85313-9b57-4052-81fb-dea336132cbf\",\"isNonStoredPaymentMethod\":true},\"paymentMethodGroup\":\"ewallet\",\"groupDisplayName\":\"eWallet\",\"exclusionTags\":null,\"paymentMethodFamily\":\"ewallet\",\"display\":{\"name\":\"GooglePay\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_googlepay.svg\",\"logos\":[{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_googlepay.svg\"}]},\"AdditionalDisplayText\":null}]";

            if (paymentMethodType?.Equals("applepay", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                arrayIndexValue = 0;
                PXSettings.PimsService.ArrangeResponse(expectedAPPIMSResponse);
            }
            else if (paymentMethodType?.Equals("googlepay", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                arrayIndexValue = 1;
                PXSettings.PimsService.ArrangeResponse(expectedGPPIMSResponse);
            }
            else if (paymentMethodType?.Equals("*", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                PXSettings.PimsService.ArrangeResponse(expectedPIMSResponse);
            }

            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "x-ms-flight", flights },
            };

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(requestUrl, paymentMethodType == null ? HttpStatusCode.BadRequest : HttpStatusCode.OK, additionaHeaders: headers);

            // Assert
            Assert.IsNotNull(pidls, "Pidls expected to not be null");
            foreach (var pidl in pidls)
            {
                Assert.IsNotNull(pidl.DataSources, "PIDL data source should not be null for express checkout");

                var dataSource = pidl.DataSources["walletConfig"];
                Assert.IsNotNull(dataSource, "PIDL data source should not be null for express checkout");

                var walletConfig = JsonConvert.DeserializeObject<WalletConfig>(JsonConvert.SerializeObject(dataSource.Members.FirstOrDefault()));
                Assert.IsNotNull(walletConfig, "WalletConfig should not be null for express checkout");
                Assert.IsNotNull(walletConfig.PIDLConfig, "PIDLConfig should not be null for express checkout");
                Assert.IsNotNull(walletConfig.PaymentInstrumentHandlers[arrayIndexValue].DisableGeoFencing);
                Assert.IsFalse(walletConfig.PaymentInstrumentHandlers[arrayIndexValue].DisableGeoFencing, "DisableGeoFencing should be false for express checkout");
                Assert.IsNotNull(walletConfig.PaymentInstrumentHandlers[arrayIndexValue].SingleMarkets);
                Assert.IsTrue(walletConfig.PaymentInstrumentHandlers[arrayIndexValue].SingleMarkets.Count > 0, "SmdMarkets should not be empty for express checkout");

                if (string.Equals(isTaxIncluded, "true", StringComparison.OrdinalIgnoreCase))
                {
                    Assert.AreEqual("amount due", walletConfig.PaymentInstrumentHandlers[0].PayLabel, "Googlepay handler Paylabel should be amount due");
                    Assert.AreEqual("amount due", walletConfig.PaymentInstrumentHandlers[1].PayLabel, "Applepay handler Paylabel should be amount due");
                }
                else
                {
                    Assert.AreEqual("amount due plus applicable taxes", walletConfig.PaymentInstrumentHandlers[0].PayLabel, "Googlepay handler Paylabel should be amount due plus applicable taxes");
                    Assert.AreEqual("amount due plus applicable taxes", walletConfig.PaymentInstrumentHandlers[1].PayLabel, "Applepay handler Paylabel should be amount due plus applicable taxes");
                }

                if (isPSD2)
                {
                    Assert.IsTrue(walletConfig.PaymentInstrumentHandlers?.FirstOrDefault()?.AllowedAuthMethodsPerCountry?.ContainsKey("gb"));
                    Assert.IsTrue(walletConfig.PaymentInstrumentHandlers?.LastOrDefault()?.AllowedAuthMethodsPerCountry?.ContainsKey("gb"));
                }
            }
        }

        [DataRow("triggerSubmit", true, "0.99", "applepay", "&allowedPaymentMethods=%5B%22ewallet.applepay%22%5D")]
        [DataRow("triggerSubmit", true, "0.99", "googlepay", "&allowedPaymentMethods=%5B%22ewallet.googlepay%22%5D")]
        [DataRow("triggerSubmit", true, "5.99", "*", null)]
        [DataRow("triggerSubmit", true, "5.99", null, null)]
        [DataTestMethod]
        public async Task GetExpressCheckoutPaymentMethodDescriptions_allowedPMs(string expectedValidation, bool isPSD2, string amount, string paymentMethodType, string allowedPMs)
        {
            // Arrange
            int arrayIndexValue = 0;
            string requestUrl = $"/v7.0/Account001/paymentMethodDescriptions?partner=candycrush&operation=ExpressCheckout&country=US&language=en-US&currency=USD&expressCheckoutData=%7B%22amount%22%3A" + amount + "%2C%22country%22%3A%22US%22%2C%22language%22%3A%22en-us%22%2C%22currency%22%3A%22USD%22%2C%22topDomainUrl%22%3A%22%22%7D" + allowedPMs;
            var flights = "PXUsePartnerSettingsService";
            string expectedPSSResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\",\"resources\":null,\"features\":{\"singleMarketDirective\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null},\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"moveCardNumberBeforeCardHolderName\":false,\"moveOrganizationNameBeforeEmailAddress\":false,\"moveLastNameBeforeFirstName\":null,\"setSaveButtonDisplayContentAsNext\":false,\"removeStarRequiredTextGroup\":null,\"updateCvvChallengeTextForGCO\":false,\"enableIsSelectPMskippedValue\":null,\"enableCountryAddorUpdateCC\":false,\"hideCountryDropdown\":null,\"hideFirstAndLastNameForCompletePrerequisites\":false,\"hideAddCreditDebitCardHeading\":false,\"updatePaymentMethodHeadingTypeToText\":null,\"removeAddCreditDebitCardHeading\":null,\"setGroupedSelectOptionTextBeforeLogo\":null,\"setSelectPMWithLogo\":null,\"useTextForCVVHelpLink\":null,\"cvvDisplayHelpPosition\":null,\"matchSelectPMMainPageStructureForSubPage\":null,\"useFixedSVGForMC\":null,\"enableIndia3dsForNonZeroPaymentTransaction\":null,\"pxEnableIndia3DS1Challenge\":null,\"india3dsEnableForBilldesk\":null,\"usePSSForPXFeatureFlighting\":null,\"enableSecureFieldAddCC\":false,\"setSaveButtonDisplayContentAsBook\":false,\"removeCancelButton\":false,\"removeSelectPiEditButton\":null,\"removeSelectPiNewPaymentMethodLink\":null,\"removeSpaceInPrivacyTextGroup\":null,\"setBackButtonDisplayContentAsCancel\":false,\"setPrivacyStatementHyperLinkDisplayToButton\":null,\"hidePaymentSummaryText\":false,\"hidepaymentOptionSaveText\":false,\"addressType\":\"billing\",\"dataSource\":\"jarvis\",\"submitActionType\":null,\"fieldsToBeHidden\":null,\"fieldsToBeEnabled\":null,\"fieldsToMakeRequired\":null,\"fieldsToBeRemoved\":null,\"removeOptionalTextFromFields\":null,\"psd2IgnorePIAuthorization\":null,\"addressSuggestionMessage\":false,\"updateAccessibilityNameWithPosition\":null,\"disableSelectPiRadioOption\":null,\"updateSelectPiButtonText\":null,\"removeGroupForExpiryMonthAndYear\":null,\"useAddressDataSourceForUpdate\":null,\"disableCountryDropdown\":null,\"ungroupAddressFirstNameLastName\":null,\"endPoint\":null,\"operation\":null,\"profileType\":null,\"dataFieldsToRemoveFromPayload\":null,\"dataFieldsToRemoveFullPath\":null,\"convertProfileTypeTo\":null,\"fieldsToBeDisabled\":null,\"hidePaymentMethodHeading\":null,\"hideChangeSettingText\":null,\"removeEwalletYesButtons\":null,\"removeEwalletBackButtons\":null,\"removeDefaultStyleHints\":null,\"useSuggestAddressesTradeAVSV1Scenario\":null,\"addCCAddressValidationPidlModification\":null,\"verifyAddressPidlModification\":null,\"displayTagsToBeRemoved\":null,\"replaceContextInstanceWithPaymentInstrumentId\":null,\"enablePlaceholder\":null,\"hideAcceptCardMessage\":null,\"addAccessibilityNameExpressionToNegativeValue\":null,\"removeAnotherDeviceTextFromShortUrlInstruction\":null,\"displayShortUrlAsHyperlink\":null,\"hideAddressCheckBoxIfAddressIsNotPrefilledFromServer\":null,\"fieldsToSetIsSubmitGroupFalse\":null,\"removeMicrosoftPrivacyTextGroup\":null,\"hideCardLogos\":null,\"hideAddress\":null,\"addCancelButton\":false,\"displayShortUrlAsVertical\":null,\"setSubmitURLToEmptyForTaxId\":null,\"addressSuggestion\":null,\"updateXboxElementsAccessibilityHints\":null,\"setCancelButtonDisplayContentAsBack\":false,\"setButtonDisplayContent\":null,\"updateConsumerProfileSubmitLinkToJarvisPatch\":null,\"removeDataSourceResources\":null,\"useIFrameForPiLogOn\":null,\"removeAddressFormHeading\":null,\"displayAccentBorderOnButtonFocus\":null,\"addStyleHints\":null,\"removeStyleHints\":null,\"removeAcceptCardMessage\":null,\"addAllFieldsRequiredText\":null}]},\"chinaAllowVisaMasterCard\":{\"applicableMarkets\":[\"cn\"],\"displayCustomizationDetail\":null}}},\"select\":{\"template\":\"selectpmradiobuttonlist\",\"redirectionPattern\":null,\"resources\":null,\"features\":{\"chinaAllowVisaMasterCard\":{\"applicableMarkets\":[\"cn\"],\"displayCustomizationDetail\":null}}},\"selectinstance\":{\"template\":\"listpidropdown\",\"redirectionPattern\":null,\"resources\":null,\"features\":{\"showPIExpirationInformation\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null},\"removeElement\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"moveCardNumberBeforeCardHolderName\":false,\"moveOrganizationNameBeforeEmailAddress\":false,\"moveLastNameBeforeFirstName\":null,\"setSaveButtonDisplayContentAsNext\":false,\"removeStarRequiredTextGroup\":null,\"updateCvvChallengeTextForGCO\":false,\"enableIsSelectPMskippedValue\":null,\"enableCountryAddorUpdateCC\":false,\"hideCountryDropdown\":null,\"hideFirstAndLastNameForCompletePrerequisites\":false,\"hideAddCreditDebitCardHeading\":false,\"updatePaymentMethodHeadingTypeToText\":null,\"removeAddCreditDebitCardHeading\":null,\"setGroupedSelectOptionTextBeforeLogo\":null,\"setSelectPMWithLogo\":null,\"useTextForCVVHelpLink\":null,\"cvvDisplayHelpPosition\":null,\"matchSelectPMMainPageStructureForSubPage\":null,\"useFixedSVGForMC\":null,\"enableIndia3dsForNonZeroPaymentTransaction\":null,\"pxEnableIndia3DS1Challenge\":null,\"india3dsEnableForBilldesk\":null,\"usePSSForPXFeatureFlighting\":null,\"enableSecureFieldAddCC\":false,\"setSaveButtonDisplayContentAsBook\":false,\"removeCancelButton\":false,\"removeSelectPiEditButton\":null,\"removeSelectPiNewPaymentMethodLink\":true,\"removeSpaceInPrivacyTextGroup\":null,\"setBackButtonDisplayContentAsCancel\":false,\"setPrivacyStatementHyperLinkDisplayToButton\":null,\"hidePaymentSummaryText\":false,\"hidepaymentOptionSaveText\":false,\"addressType\":null,\"dataSource\":null,\"submitActionType\":null,\"fieldsToBeHidden\":null,\"fieldsToBeEnabled\":null,\"fieldsToMakeRequired\":null,\"fieldsToBeRemoved\":null,\"removeOptionalTextFromFields\":null,\"psd2IgnorePIAuthorization\":null,\"addressSuggestionMessage\":false,\"updateAccessibilityNameWithPosition\":null,\"disableSelectPiRadioOption\":null,\"updateSelectPiButtonText\":null,\"removeGroupForExpiryMonthAndYear\":null,\"useAddressDataSourceForUpdate\":null,\"disableCountryDropdown\":null,\"ungroupAddressFirstNameLastName\":null,\"endPoint\":null,\"operation\":null,\"profileType\":null,\"dataFieldsToRemoveFromPayload\":null,\"dataFieldsToRemoveFullPath\":null,\"convertProfileTypeTo\":null,\"fieldsToBeDisabled\":null,\"hidePaymentMethodHeading\":null,\"hideChangeSettingText\":null,\"removeEwalletYesButtons\":null,\"removeEwalletBackButtons\":null,\"removeDefaultStyleHints\":null,\"useSuggestAddressesTradeAVSV1Scenario\":null,\"addCCAddressValidationPidlModification\":null,\"verifyAddressPidlModification\":null,\"displayTagsToBeRemoved\":null,\"replaceContextInstanceWithPaymentInstrumentId\":null,\"enablePlaceholder\":null,\"hideAcceptCardMessage\":null,\"addAccessibilityNameExpressionToNegativeValue\":null,\"removeAnotherDeviceTextFromShortUrlInstruction\":null,\"displayShortUrlAsHyperlink\":null,\"hideAddressCheckBoxIfAddressIsNotPrefilledFromServer\":null,\"fieldsToSetIsSubmitGroupFalse\":null,\"removeMicrosoftPrivacyTextGroup\":null,\"hideCardLogos\":null,\"hideAddress\":null,\"addCancelButton\":false,\"displayShortUrlAsVertical\":null,\"setSubmitURLToEmptyForTaxId\":null,\"addressSuggestion\":null,\"updateXboxElementsAccessibilityHints\":null,\"setCancelButtonDisplayContentAsBack\":false,\"setButtonDisplayContent\":null,\"updateConsumerProfileSubmitLinkToJarvisPatch\":null,\"removeDataSourceResources\":null,\"useIFrameForPiLogOn\":null,\"removeAddressFormHeading\":null,\"displayAccentBorderOnButtonFocus\":null,\"addStyleHints\":null,\"removeStyleHints\":null,\"removeAcceptCardMessage\":null,\"addAllFieldsRequiredText\":null}]},\"chinaAllowVisaMasterCard\":{\"applicableMarkets\":[\"cn\"],\"displayCustomizationDetail\":null}}},\"handlepaymentchallenge\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\",\"resources\":null,\"features\":{\"threeDSOne\":{\"applicableMarkets\":[\"in\"],\"displayCustomizationDetail\":[{\"moveCardNumberBeforeCardHolderName\":false,\"moveOrganizationNameBeforeEmailAddress\":false,\"moveLastNameBeforeFirstName\":null,\"setSaveButtonDisplayContentAsNext\":false,\"removeStarRequiredTextGroup\":null,\"updateCvvChallengeTextForGCO\":false,\"enableIsSelectPMskippedValue\":null,\"enableCountryAddorUpdateCC\":false,\"hideCountryDropdown\":null,\"hideFirstAndLastNameForCompletePrerequisites\":false,\"hideAddCreditDebitCardHeading\":false,\"updatePaymentMethodHeadingTypeToText\":null,\"removeAddCreditDebitCardHeading\":null,\"setGroupedSelectOptionTextBeforeLogo\":null,\"setSelectPMWithLogo\":null,\"useTextForCVVHelpLink\":null,\"cvvDisplayHelpPosition\":null,\"matchSelectPMMainPageStructureForSubPage\":null,\"useFixedSVGForMC\":null,\"enableIndia3dsForNonZeroPaymentTransaction\":false,\"pxEnableIndia3DS1Challenge\":true,\"india3dsEnableForBilldesk\":true,\"usePSSForPXFeatureFlighting\":null,\"enableSecureFieldAddCC\":false,\"setSaveButtonDisplayContentAsBook\":false,\"removeCancelButton\":false,\"removeSelectPiEditButton\":null,\"removeSelectPiNewPaymentMethodLink\":null,\"removeSpaceInPrivacyTextGroup\":null,\"setBackButtonDisplayContentAsCancel\":false,\"setPrivacyStatementHyperLinkDisplayToButton\":null,\"hidePaymentSummaryText\":false,\"hidepaymentOptionSaveText\":false,\"addressType\":null,\"dataSource\":null,\"submitActionType\":null,\"fieldsToBeHidden\":null,\"fieldsToBeEnabled\":null,\"fieldsToMakeRequired\":null,\"fieldsToBeRemoved\":null,\"removeOptionalTextFromFields\":null,\"psd2IgnorePIAuthorization\":null,\"addressSuggestionMessage\":false,\"updateAccessibilityNameWithPosition\":null,\"disableSelectPiRadioOption\":null,\"updateSelectPiButtonText\":null,\"removeGroupForExpiryMonthAndYear\":null,\"useAddressDataSourceForUpdate\":null,\"disableCountryDropdown\":null,\"ungroupAddressFirstNameLastName\":null,\"endPoint\":null,\"operation\":null,\"profileType\":null,\"dataFieldsToRemoveFromPayload\":null,\"dataFieldsToRemoveFullPath\":null,\"convertProfileTypeTo\":null,\"fieldsToBeDisabled\":null,\"hidePaymentMethodHeading\":null,\"hideChangeSettingText\":null,\"removeEwalletYesButtons\":null,\"removeEwalletBackButtons\":null,\"removeDefaultStyleHints\":null,\"useSuggestAddressesTradeAVSV1Scenario\":null,\"addCCAddressValidationPidlModification\":null,\"verifyAddressPidlModification\":null,\"displayTagsToBeRemoved\":null,\"replaceContextInstanceWithPaymentInstrumentId\":null,\"enablePlaceholder\":null,\"hideAcceptCardMessage\":null,\"addAccessibilityNameExpressionToNegativeValue\":null,\"removeAnotherDeviceTextFromShortUrlInstruction\":null,\"displayShortUrlAsHyperlink\":null,\"hideAddressCheckBoxIfAddressIsNotPrefilledFromServer\":null,\"fieldsToSetIsSubmitGroupFalse\":null,\"removeMicrosoftPrivacyTextGroup\":null,\"hideCardLogos\":null,\"hideAddress\":null,\"addCancelButton\":false,\"displayShortUrlAsVertical\":null,\"setSubmitURLToEmptyForTaxId\":null,\"addressSuggestion\":null,\"updateXboxElementsAccessibilityHints\":null,\"setCancelButtonDisplayContentAsBack\":false,\"setButtonDisplayContent\":null,\"updateConsumerProfileSubmitLinkToJarvisPatch\":null,\"removeDataSourceResources\":null,\"useIFrameForPiLogOn\":null,\"removeAddressFormHeading\":null,\"displayAccentBorderOnButtonFocus\":null,\"addStyleHints\":null,\"removeStyleHints\":null,\"removeAcceptCardMessage\":null,\"addAllFieldsRequiredText\":null}]},\"PSD2\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"moveCardNumberBeforeCardHolderName\":false,\"moveOrganizationNameBeforeEmailAddress\":false,\"moveLastNameBeforeFirstName\":null,\"setSaveButtonDisplayContentAsNext\":false,\"removeStarRequiredTextGroup\":null,\"updateCvvChallengeTextForGCO\":false,\"enableIsSelectPMskippedValue\":null,\"enableCountryAddorUpdateCC\":false,\"hideCountryDropdown\":null,\"hideFirstAndLastNameForCompletePrerequisites\":false,\"hideAddCreditDebitCardHeading\":false,\"updatePaymentMethodHeadingTypeToText\":null,\"removeAddCreditDebitCardHeading\":null,\"setGroupedSelectOptionTextBeforeLogo\":null,\"setSelectPMWithLogo\":null,\"useTextForCVVHelpLink\":null,\"cvvDisplayHelpPosition\":null,\"matchSelectPMMainPageStructureForSubPage\":null,\"useFixedSVGForMC\":null,\"enableIndia3dsForNonZeroPaymentTransaction\":null,\"pxEnableIndia3DS1Challenge\":null,\"india3dsEnableForBilldesk\":null,\"usePSSForPXFeatureFlighting\":null,\"enableSecureFieldAddCC\":false,\"setSaveButtonDisplayContentAsBook\":false,\"removeCancelButton\":false,\"removeSelectPiEditButton\":null,\"removeSelectPiNewPaymentMethodLink\":null,\"removeSpaceInPrivacyTextGroup\":null,\"setBackButtonDisplayContentAsCancel\":false,\"setPrivacyStatementHyperLinkDisplayToButton\":null,\"hidePaymentSummaryText\":false,\"hidepaymentOptionSaveText\":false,\"addressType\":null,\"dataSource\":null,\"submitActionType\":null,\"fieldsToBeHidden\":null,\"fieldsToBeEnabled\":null,\"fieldsToMakeRequired\":null,\"fieldsToBeRemoved\":null,\"removeOptionalTextFromFields\":null,\"psd2IgnorePIAuthorization\":true,\"addressSuggestionMessage\":false,\"updateAccessibilityNameWithPosition\":null,\"disableSelectPiRadioOption\":null,\"updateSelectPiButtonText\":null,\"removeGroupForExpiryMonthAndYear\":null,\"useAddressDataSourceForUpdate\":null,\"disableCountryDropdown\":null,\"ungroupAddressFirstNameLastName\":null,\"endPoint\":null,\"operation\":null,\"profileType\":null,\"dataFieldsToRemoveFromPayload\":null,\"dataFieldsToRemoveFullPath\":null,\"convertProfileTypeTo\":null,\"fieldsToBeDisabled\":null,\"hidePaymentMethodHeading\":null,\"hideChangeSettingText\":null,\"removeEwalletYesButtons\":null,\"removeEwalletBackButtons\":null,\"removeDefaultStyleHints\":null,\"useSuggestAddressesTradeAVSV1Scenario\":null,\"addCCAddressValidationPidlModification\":null,\"verifyAddressPidlModification\":null,\"displayTagsToBeRemoved\":null,\"replaceContextInstanceWithPaymentInstrumentId\":null,\"enablePlaceholder\":null,\"hideAcceptCardMessage\":null,\"addAccessibilityNameExpressionToNegativeValue\":null,\"removeAnotherDeviceTextFromShortUrlInstruction\":null,\"displayShortUrlAsHyperlink\":null,\"hideAddressCheckBoxIfAddressIsNotPrefilledFromServer\":null,\"fieldsToSetIsSubmitGroupFalse\":null,\"removeMicrosoftPrivacyTextGroup\":null,\"hideCardLogos\":null,\"hideAddress\":null,\"addCancelButton\":false,\"displayShortUrlAsVertical\":null,\"setSubmitURLToEmptyForTaxId\":null,\"addressSuggestion\":null,\"updateXboxElementsAccessibilityHints\":null,\"setCancelButtonDisplayContentAsBack\":false,\"setButtonDisplayContent\":null,\"updateConsumerProfileSubmitLinkToJarvisPatch\":null,\"removeDataSourceResources\":null,\"useIFrameForPiLogOn\":null,\"removeAddressFormHeading\":null,\"displayAccentBorderOnButtonFocus\":null,\"addStyleHints\":null,\"removeStyleHints\":null,\"removeAcceptCardMessage\":null,\"addAllFieldsRequiredText\":null}]}}},\"default\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":null,\"resources\":null,\"features\":null}}";
            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            string expectedAPPIMSResponse = "[{\"paymentMethodType\":\"applepay\",\"properties\":{\"offlineRecurring\":true,\"userManaged\":false,\"chargeThresholds\":null,\"redirectRequired\":null,\"soldToAddressRequired\":true,\"splitPaymentSupported\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"providerCountry\":null,\"nonStoredPaymentMethodId\":\"be4de87d-7e38-4b2d-8836-9237eb32848e\",\"isNonStoredPaymentMethod\":true},\"paymentMethodGroup\":\"ewallet\",\"groupDisplayName\":\"eWallet\",\"exclusionTags\":null,\"paymentMethodFamily\":\"ewallet\",\"display\":{\"name\":\"ApplePay\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_applepay.svg\",\"logos\":[{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_applepay.svg\"}]},\"AdditionalDisplayText\":null}]";
            string expectedGPPIMSResponse = "[{\"paymentMethodType\":\"googlepay\",\"properties\":{\"offlineRecurring\":true,\"userManaged\":false,\"chargeThresholds\":null,\"redirectRequired\":null,\"soldToAddressRequired\":true,\"splitPaymentSupported\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"providerCountry\":null,\"nonStoredPaymentMethodId\":\"cdc85313-9b57-4052-81fb-dea336132cbf\",\"isNonStoredPaymentMethod\":true},\"paymentMethodGroup\":\"ewallet\",\"groupDisplayName\":\"eWallet\",\"exclusionTags\":null,\"paymentMethodFamily\":\"ewallet\",\"display\":{\"name\":\"GooglePay\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_googlepay.svg\",\"logos\":[{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_googlepay.svg\"}]},\"AdditionalDisplayText\":null}]";
            string expectedPIMSResponse = "[{\"paymentMethodType\":\"applepay\",\"properties\":{\"offlineRecurring\":true,\"userManaged\":false,\"chargeThresholds\":null,\"redirectRequired\":null,\"soldToAddressRequired\":true,\"splitPaymentSupported\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"providerCountry\":null,\"nonStoredPaymentMethodId\":\"be4de87d-7e38-4b2d-8836-9237eb32848e\",\"isNonStoredPaymentMethod\":true},\"paymentMethodGroup\":\"ewallet\",\"groupDisplayName\":\"eWallet\",\"exclusionTags\":null,\"paymentMethodFamily\":\"ewallet\",\"display\":{\"name\":\"ApplePay\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_applepay.svg\",\"logos\":[{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_applepay.svg\"}]},\"AdditionalDisplayText\":null},{\"paymentMethodType\":\"googlepay\",\"properties\":{\"offlineRecurring\":true,\"userManaged\":false,\"chargeThresholds\":null,\"redirectRequired\":null,\"soldToAddressRequired\":true,\"splitPaymentSupported\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"providerCountry\":null,\"nonStoredPaymentMethodId\":\"cdc85313-9b57-4052-81fb-dea336132cbf\",\"isNonStoredPaymentMethod\":true},\"paymentMethodGroup\":\"ewallet\",\"groupDisplayName\":\"eWallet\",\"exclusionTags\":null,\"paymentMethodFamily\":\"ewallet\",\"display\":{\"name\":\"GooglePay\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_googlepay.svg\",\"logos\":[{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_googlepay.svg\"}]},\"AdditionalDisplayText\":null}]";

            if (paymentMethodType?.Equals("applepay", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                arrayIndexValue = 0;
                PXSettings.PimsService.ArrangeResponse(expectedAPPIMSResponse);
            }
            else if (paymentMethodType?.Equals("googlepay", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                arrayIndexValue = 1;
                PXSettings.PimsService.ArrangeResponse(expectedGPPIMSResponse);
            }
            else if (paymentMethodType?.Equals("*", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                PXSettings.PimsService.ArrangeResponse(expectedPIMSResponse);
            }

            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "x-ms-flight", flights },
            };

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(requestUrl, paymentMethodType == null ? HttpStatusCode.BadRequest : HttpStatusCode.OK, additionaHeaders: headers);

            // Assert
            Assert.IsNotNull(pidls, "Pidls expected to not be null");
            foreach (var pidl in pidls)
            {
                Assert.IsNotNull(pidl.DataSources, "PIDL data source should not be null for express checkout");

                var dataSource = pidl.DataSources["walletConfig"];
                Assert.IsNotNull(dataSource, "PIDL data source should not be null for express checkout");

                var walletConfig = JsonConvert.DeserializeObject<WalletConfig>(JsonConvert.SerializeObject(dataSource.Members.FirstOrDefault()));
                Assert.IsNotNull(walletConfig, "WalletConfig should not be null for express checkout");
                Assert.IsNotNull(walletConfig.PIDLConfig, "PIDLConfig should not be null for express checkout");
                Assert.IsNotNull(walletConfig.PaymentInstrumentHandlers[arrayIndexValue].DisableGeoFencing);
                Assert.IsFalse(walletConfig.PaymentInstrumentHandlers[arrayIndexValue].DisableGeoFencing, "DisableGeoFencing should be false for express checkout");
                Assert.IsNotNull(walletConfig.PaymentInstrumentHandlers[arrayIndexValue].SingleMarkets);
                Assert.IsTrue(walletConfig.PaymentInstrumentHandlers[arrayIndexValue].SingleMarkets.Count > 0, "SingleMarkets should not be empty for express checkout");

                if (isPSD2)
                {
                    Assert.IsTrue(walletConfig.PaymentInstrumentHandlers?.FirstOrDefault()?.AllowedAuthMethodsPerCountry?.ContainsKey("gb"));
                    Assert.IsTrue(walletConfig.PaymentInstrumentHandlers?.LastOrDefault()?.AllowedAuthMethodsPerCountry?.ContainsKey("gb"));
                }

                var googlePayButton = pidl.GetDisplayHintById("googlepayExpressCheckoutFrame") as ExpressCheckoutButtonDisplayHint;
                var applePayButton = pidl.GetDisplayHintById("applepayExpressCheckoutFrame") as ExpressCheckoutButtonDisplayHint;

                if (paymentMethodType?.Equals("*", StringComparison.OrdinalIgnoreCase) ?? false)
                {
                    Assert.IsNotNull(applePayButton, "Apple pay button should not be null");
                    Assert.IsNotNull(googlePayButton, "Google pay button should not be null");
                }

                if (paymentMethodType?.Equals("applepay", StringComparison.OrdinalIgnoreCase) ?? false)
                {
                    Assert.IsNotNull(applePayButton, "Apple pay button should not be null");
                    Assert.IsNull(paymentMethodType.Equals("applepay", StringComparison.OrdinalIgnoreCase) ? googlePayButton : null, "Google pay button should be null");
                }

                if (paymentMethodType?.Equals("googlepay", StringComparison.OrdinalIgnoreCase) ?? false)
                {
                    Assert.IsNotNull(googlePayButton, "Google pay button should not be null");
                    Assert.IsNull(paymentMethodType.Equals("googlepay", StringComparison.OrdinalIgnoreCase) ? applePayButton : null, "Apple pay button should be null");
                }
            }
        }

        [DataRow("battlenet", true)]
        [DataRow("battlenet", false)]
        [DataRow("otherthan_battlenet", true)]
        [DataRow("otherthan_battlenet", false)]
        [DataTestMethod]
        public async Task GePaymentMethodDescriptions_changeExpiryStyleToTextBox(string partner, bool enableFeatureWithPSS)
        {
            // Arrange
            string requestUrl = $"/v7.0/Account001/paymentMethodDescriptions?partner={partner}&operation=add&country=US&language=en-US&family=credit_card&type=amex%2Cvisa%2Cmc%2Cdiscover%2Cjcb";
            string expectedPSSResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\",\"resources\":null,\"features\":{\"customizeStructure\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"removeGroupForExpiryMonthAndYear\":true}]},\"removeElement\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"removeAddCreditDebitCardHeading\":true,\"removeStarRequiredTextGroup\":true,\"removeMicrosoftPrivacyTextGroup\":true}]},\"hideElement\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"hideAcceptCardMessage\":true,\"hideCardLogos\":true,\"hideAddress\":true,\"hidepaymentOptionSaveText\":true}]},\"singleMarketDirective\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null},\"enableElement\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"enableCountryAddorUpdateCC\":true}]}}}}";

            if (enableFeatureWithPSS)
            {
                expectedPSSResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\",\"resources\":null,\"features\":{\"changeExpiryStyleToTextBox\":{\"applicableMarkets\":[]},\"customizeStructure\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"removeGroupForExpiryMonthAndYear\":true}]},\"removeElement\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"removeAddCreditDebitCardHeading\":true,\"removeStarRequiredTextGroup\":true,\"removeMicrosoftPrivacyTextGroup\":true}]},\"hideElement\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"hideAcceptCardMessage\":true,\"hideCardLogos\":true,\"hideAddress\":true,\"hidepaymentOptionSaveText\":true}]},\"singleMarketDirective\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null},\"enableElement\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"enableCountryAddorUpdateCC\":true}]}}}}";
            }

            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(requestUrl);

            // Assert
            Assert.IsNotNull(pidls, "Pidls expected to not be null");
            foreach (var pidl in pidls)
            {
                var expiryGroup = pidl.GetDisplayHintById("expiryGroup");
                PropertyDisplayHint expiryMonth = pidl.GetDisplayHintById("expiryMonth") as PropertyDisplayHint;
                PropertyDisplayHint expiryYear = pidl.GetDisplayHintById("expiryYear") as PropertyDisplayHint;

                PropertyDescription expiryMonthDataDescription = pidl.GetPropertyDescriptionByPropertyName("expiryMonth");
                PropertyDescription expiryYearDataDescription = pidl.GetPropertyDescriptionByPropertyName("expiryYear");

                Assert.IsNotNull(expiryGroup, "expiryGroup is expected to be not null");
                Assert.IsNotNull(expiryMonth, "expiryMonth is expected to be not null");
                Assert.IsNotNull(expiryYear, "expiryYear is expected to be not null");
                Assert.IsNotNull(expiryMonthDataDescription, "expiryMonthDataDescription is expected to be not null");
                Assert.IsNotNull(expiryYearDataDescription, "expiryYearDataDescription is expected to be not null");

                if (enableFeatureWithPSS)
                {
                    Assert.IsTrue(expiryMonth.PossibleOptions == null && expiryMonth.PossibleValues == null, "PossibleOptions and PossibleValues of expiry Monthare expected to be null");
                    Assert.IsTrue(expiryYear.PossibleOptions == null && expiryYear.PossibleValues == null, "PossibleOptions and PossibleValues of expiry Year are expected to be null");
                    Assert.AreEqual("YY", expiryYear.DisplayDescription);
                    Assert.AreEqual("MM", expiryMonth.DisplayDescription);

                    Assert.IsTrue(expiryYearDataDescription.Validations.Any(v => v.ValidationType == "regex" && v.ErrorCode == "expiry_year_invalid"));
                    Assert.IsTrue(expiryMonthDataDescription.Validations.Any(v => v.ValidationType == "regex" && v.ErrorCode == "expiry_month_invalid"));

                    Assert.AreEqual(2, expiryYearDataDescription.Transformation.Count, "Transformation should contain two entries");
                    Assert.IsNotNull(expiryYearDataDescription.Transformation["forSubmit"]);
                    Assert.IsNotNull(expiryYearDataDescription.Transformation["forDisplay"]);

                    Assert.AreEqual(2, expiryMonthDataDescription.Transformation.Count, "Transformation should contain two entries");
                    Assert.IsNotNull(expiryMonthDataDescription.Transformation["forSubmit"]);
                    Assert.IsNotNull(expiryMonthDataDescription.Transformation["forDisplay"]);

                    ValidatePidlPropertyRegex(pidl, "expiryYear", "25", true);
                    ValidatePidlPropertyRegex(pidl, "expiryYear", "48", true);
                    ValidatePidlPropertyRegex(pidl, "expiryYear", "32", true);
                    ValidatePidlPropertyRegex(pidl, "expiryYear", "24", false);
                    ValidatePidlPropertyRegex(pidl, "expiryYear", "49", false);
                }
                else
                {
                    Assert.IsTrue(expiryMonth.PossibleOptions != null && expiryMonth.PossibleValues != null, "PossibleOptions and PossibleValues of expiry Monthare expected to be not null");
                    Assert.IsTrue(expiryYear.PossibleOptions != null && expiryYear.PossibleValues != null, "PossibleOptions and PossibleValues of expiry Year are expected to be not null");
                }
            }

            PXSettings.PartnerSettingsService.ResetToDefaults();
        }

        [DataRow("battlenet", true, "PXChangeExpiryMonthYearToExpiryDateTextBox", 1)]
        [DataRow("battlenet", false, "PXChangeExpiryMonthYearToExpiryDateTextBox", 1)]
        [DataRow("battlenet", true, null, 2)]
        [DataRow("battlenet", false, null, 0)]
        [DataTestMethod]
        public async Task GePaymentMethodDescriptions_ExpiryDateTextBox(string partner, bool enableFeatureWithPSS, string flights, int count)
        {
            // Arrange
            string requestUrl = $"/v7.0/Account001/paymentMethodDescriptions?partner={partner}&operation=add&country=US&language=en-US&family=credit_card&type=amex%2Cvisa%2Cmc%2Cdiscover%2Cjcb";
            string expectedPSSResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\",\"resources\":null,\"features\":{\"customizeStructure\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"removeGroupForExpiryMonthAndYear\":true}]},\"removeElement\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"removeAddCreditDebitCardHeading\":true,\"removeStarRequiredTextGroup\":true,\"removeMicrosoftPrivacyTextGroup\":true}]},\"hideElement\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"hideAcceptCardMessage\":true,\"hideCardLogos\":true,\"hideAddress\":true,\"hidepaymentOptionSaveText\":true}]},\"singleMarketDirective\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null},\"enableElement\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"enableCountryAddorUpdateCC\":true}]}}}}";

            if (enableFeatureWithPSS)
            {
                expectedPSSResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\",\"resources\":null,\"features\":{\"changeExpiryStyleToTextBox\":{\"applicableMarkets\":[]},\"customizeStructure\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"removeGroupForExpiryMonthAndYear\":true}]},\"removeElement\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"removeAddCreditDebitCardHeading\":true,\"removeStarRequiredTextGroup\":true,\"removeMicrosoftPrivacyTextGroup\":true}]},\"hideElement\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"hideAcceptCardMessage\":true,\"hideCardLogos\":true,\"hideAddress\":true,\"hidepaymentOptionSaveText\":true}]},\"singleMarketDirective\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null},\"enableElement\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"enableCountryAddorUpdateCC\":true}]}}}}";
            }

            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "x-ms-flight", flights },
            };

            // Act            
            List<PIDLResource> pidls = await GetPidlFromPXService(requestUrl, additionaHeaders: headers);

            // Assert
            Assert.IsNotNull(pidls, "Pidls expected to not be null");
            foreach (var pidl in pidls)
            {
                var expiryGroup = pidl.GetDisplayHintById("expiryGroup");
                PropertyDisplayHint expiryMonth = pidl.GetDisplayHintById("expiryMonth") as PropertyDisplayHint;
                PropertyDisplayHint expiryYear = pidl.GetDisplayHintById("expiryYear") as PropertyDisplayHint;

                PropertyDisplayHint expiryDate = pidl.GetDisplayHintById("expiryDate") as PropertyDisplayHint;

                PropertyDescription expiryMonthDataDescription = pidl.GetPropertyDescriptionByPropertyName("expiryMonth");
                PropertyDescription expiryYearDataDescription = pidl.GetPropertyDescriptionByPropertyName("expiryYear");
                PropertyDescription expiryDateDataDescription = pidl.GetPropertyDescriptionByPropertyName("expiryDate");

                Assert.IsNotNull(expiryGroup, "expiryGroup is expected to be not null");
                Assert.IsNotNull(expiryMonth, "expiryMonth is expected to be not null");
                Assert.IsNotNull(expiryYear, "expiryYear is expected to be not null");
                Assert.IsNotNull(expiryMonthDataDescription, "expiryMonthDataDescription is expected to be not null");
                Assert.IsNotNull(expiryYearDataDescription, "expiryYearDataDescription is expected to be not null");

                if (enableFeatureWithPSS || flights != null)
                {
                    if (enableFeatureWithPSS)
                    {
                        Assert.IsTrue(expiryMonth.PossibleOptions == null && expiryMonth.PossibleValues == null, "PossibleOptions and PossibleValues of expiry Monthare expected to be null");
                        Assert.IsTrue(expiryYear.PossibleOptions == null && expiryYear.PossibleValues == null, "PossibleOptions and PossibleValues of expiry Year are expected to be null");
                        Assert.AreEqual("YY", expiryYear.DisplayDescription);
                        Assert.AreEqual("MM", expiryMonth.DisplayDescription);

                        Assert.IsTrue(expiryYearDataDescription.Validations.Any(v => v.ValidationType == "regex" && v.ErrorCode == "expiry_year_invalid"));
                        Assert.IsTrue(expiryMonthDataDescription.Validations.Any(v => v.ValidationType == "regex" && v.ErrorCode == "expiry_month_invalid"));
                    }

                    if (flights != null)
                    {
                        Assert.IsNotNull(expiryDate, "expiryDate is expected to be not null");
                        Assert.IsNotNull(expiryDateDataDescription, "expiryDateDataDescription is expected to be not null");
                        Assert.AreEqual("MM/YY", expiryDate.DisplayDescription);
                        Assert.IsNotNull(expiryDateDataDescription.SideEffects, "expiryDateDataDescription side effects is expected to be not null");
                    }

                    Assert.AreEqual(count, expiryYearDataDescription.Transformation.Count, "Transformation should contain two entries");
                    Assert.IsNotNull(expiryYearDataDescription.Transformation["forSubmit"]);

                    Assert.AreEqual(count, expiryMonthDataDescription.Transformation.Count, "Transformation should contain two entries");
                    Assert.IsNotNull(expiryMonthDataDescription.Transformation["forSubmit"]);
                }
                else
                {
                    Assert.IsNull(expiryDate, "expiryDate is expected to be null");
                    Assert.IsNull(expiryDateDataDescription, "expiryDateDataDescription is expected to be null");

                    Assert.IsTrue(expiryMonth.PossibleOptions != null && expiryMonth.PossibleValues != null, "PossibleOptions and PossibleValues of expiry Monthare expected to be not null");
                    Assert.IsTrue(expiryYear.PossibleOptions != null && expiryYear.PossibleValues != null, "PossibleOptions and PossibleValues of expiry Year are expected to be not null");
                }
            }

            PXSettings.PartnerSettingsService.ResetToDefaults();
        }

        [DataRow("battlenet", true)]
        [DataRow("battlenet", false)]
        [DataTestMethod]
        public async Task GePaymentMethodDescriptions_CombineExpiryToExpiryDateTextBox(string partner, bool enableFeatureWithPSS)
        {
            // Arrange
            string requestUrl = $"/v7.0/Account001/paymentMethodDescriptions?partner={partner}&operation=add&country=US&language=en-US&family=credit_card&type=amex%2Cvisa%2Cmc%2Cdiscover%2Cjcb";
            string expectedPSSResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\",\"resources\":null,\"features\":{\"customizeStructure\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"removeGroupForExpiryMonthAndYear\":true}]},\"removeElement\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"removeAddCreditDebitCardHeading\":true,\"removeStarRequiredTextGroup\":true,\"removeMicrosoftPrivacyTextGroup\":true}]},\"hideElement\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"hideAcceptCardMessage\":true,\"hideCardLogos\":true,\"hideAddress\":true,\"hidepaymentOptionSaveText\":true}]},\"singleMarketDirective\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null},\"enableElement\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"enableCountryAddorUpdateCC\":true}]}}}}";

            if (enableFeatureWithPSS)
            {
                expectedPSSResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\",\"resources\":null,\"features\":{\"combineExpiryMonthYearToDateTextBox\":{\"applicableMarkets\":[]},\"customizeStructure\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"removeGroupForExpiryMonthAndYear\":true}]},\"removeElement\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"removeAddCreditDebitCardHeading\":true,\"removeStarRequiredTextGroup\":true,\"removeMicrosoftPrivacyTextGroup\":true}]},\"hideElement\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"hideAcceptCardMessage\":true,\"hideCardLogos\":true,\"hideAddress\":true,\"hidepaymentOptionSaveText\":true}]},\"singleMarketDirective\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null},\"enableElement\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"enableCountryAddorUpdateCC\":true}]}}}}";
            }

            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(requestUrl);

            // Assert
            Assert.IsNotNull(pidls, "Pidls expected to not be null");
            foreach (var pidl in pidls)
            {
                var expiryGroup = pidl.GetDisplayHintById("expiryGroup");
                PropertyDisplayHint expiryMonth = pidl.GetDisplayHintById("expiryMonth") as PropertyDisplayHint;
                PropertyDisplayHint expiryYear = pidl.GetDisplayHintById("expiryYear") as PropertyDisplayHint;

                PropertyDisplayHint expiryDate = pidl.GetDisplayHintById("expiryDate") as PropertyDisplayHint;

                PropertyDescription expiryMonthDataDescription = pidl.GetPropertyDescriptionByPropertyName("expiryMonth");
                PropertyDescription expiryYearDataDescription = pidl.GetPropertyDescriptionByPropertyName("expiryYear");
                PropertyDescription expiryDateDataDescription = pidl.GetPropertyDescriptionByPropertyName("expiryDate");

                Assert.IsNotNull(expiryGroup, "expiryGroup is expected to be not null");
                Assert.IsNotNull(expiryMonth, "expiryMonth is expected to be not null");
                Assert.IsNotNull(expiryYear, "expiryYear is expected to be not null");
                Assert.IsNotNull(expiryMonthDataDescription, "expiryMonthDataDescription is expected to be not null");
                Assert.IsNotNull(expiryYearDataDescription, "expiryYearDataDescription is expected to be not null");

                if (enableFeatureWithPSS)
                {
                    Assert.IsNotNull(expiryDate, "expiryDate is expected to be not null");
                    Assert.IsNotNull(expiryDateDataDescription, "expiryDateDataDescription is expected to be not null");
                    Assert.AreEqual("MM/YY", expiryDate.DisplayDescription);
                    Assert.IsTrue(expiryDate.PossibleOptions == null && expiryDate.PossibleValues == null, "PossibleOptions and PossibleValues of expiry Monthare expected to be null");

                    ValidatePidlPropertyRegex(pidl, "expiryDate", "02/2049", true);
                    ValidatePidlPropertyRegex(pidl, "expiryDate", "02/2050", false);
                    ValidatePidlPropertyRegex(pidl, "expiryDate", "00/2029", false);
                    ValidatePidlPropertyRegex(pidl, "expiryDate", "12/29", true);

                    // Checks for last year expiry date, for which the regex is expected to fail,
                    // This test will fail next year unless regex is updated, this failure is expected.
                    // Update regex in PidFactory Constants for expiryDate to allow years starting from current only
                    string lastYear = DateTime.Now.AddYears(-1).ToString("yyyy");
                    ValidatePidlPropertyRegex(pidl, "expiryDate", $"12/{lastYear}", false);
                }
                else
                {
                    Assert.IsNull(expiryDate, "expiryDate is expected to be null");
                    Assert.IsNull(expiryDateDataDescription, "expiryDateDataDescription is expected to be null");
                }
            }

            PXSettings.PartnerSettingsService.ResetToDefaults();
        }

        [DataRow("macmanage", "direct_debit", Constants.PaymentMethodFamilyType.Sepa)]
        [DataTestMethod]
        public async Task GetPaymentMethodDescriptions_SEPA_NCE(string partner, string family, string type = null)
        {
            // Arrange
            string requestUrl = $"/v7.0/my-ba/paymentMethodDescriptions?partner={partner}&operation=add&country=de&language=en-US&family=direct_debit&type=sepa";
            string expectedPSSResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\",\"resources\":null,\"features\":{\"customizeSEPAForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"customizeNCESEPA\":true}]},\"singleMarketDirective\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null}}}}";

            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "x-ms-flight", "EnableModern,PxEnableRiskEligibilityCheck" },
                { "x-ms-billingAccountId", "commerceRootId:organizationId" },
            };

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(requestUrl, additionaHeaders: headers);

            // Assert
            Assert.IsNotNull(pidls, "Pidls expected to not be null");
            foreach (var pidl in pidls)
            {
                var addDirectDebitSepaHeading = pidl.GetDisplayHintById("addDirectDebitSepaNewHeading") as HeadingDisplayHint;
                Assert.IsNotNull(addDirectDebitSepaHeading, "Direct Debit Sepa Heading is expected to be not null");
                Assert.AreEqual("Set up to pay by SEPA direct debit", addDirectDebitSepaHeading.DisplayContent);

                var directDebitSepaUpdateLine1 = pidl.GetDisplayHintById("directDebitSepaUpdateLine1") as TextDisplayHint;
                Assert.IsNotNull(directDebitSepaUpdateLine1, "Direct Debit Sepa allow popup is expected to be not null");
                Assert.AreEqual("Allow popups on your browser to be redirected to the Direct debit agreement.", directDebitSepaUpdateLine1.DisplayContent);
            }

            PXSettings.PartnerSettingsService.ResetToDefaults();
        }
    }
}