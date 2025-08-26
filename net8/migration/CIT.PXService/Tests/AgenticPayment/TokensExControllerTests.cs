// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2025. All rights reserved.</copyright>

namespace CIT.PXService.Tests
{
    using global::Tests.Common.Model.Pidl;
    using Microsoft.Commerce.Payments.PXService.Model.NetworkTokenizationService;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    [TestClass]
    public class TokensExControllerTests : TestBase
    {
        private const string TestAccountId = "Account002";
        private const string TestNtid = "19c6f6df-8b31-4d29-b594-67621438e8d2";
        private const string TestChallengeId = "c6f4cba0-2c09-4e4c-a6cf-20b3a17ff0dd";
        private const string TestPartner = "copilot";
        private const string TestCountry = "us";
        private const string TestLanguage = "en-US";
        private const string TestChallengeMethodPlaceholder = "{challengeMethod}";

        [TestInitialize]
        public void Startup()
        {
            // Reset emulator responses for each test
            PXSettings.AddressEnrichmentService.Responses.Clear();
            PXSettings.AccountsService.Responses.Clear();
            PXSettings.PimsService.Responses.Clear();
            PXSettings.TokenPolicyService.Responses.Clear();
            PXSettings.PurchaseService.Responses.Clear();
            PXSettings.PartnerSettingsService.Responses.Clear();
            PXSettings.OrchestrationService.Responses.Clear();
            PXSettings.IssuerService.Responses.Clear();
            PXSettings.CatalogService.Responses.Clear();

            PXSettings.AddressEnrichmentService.ResetToDefaults();
            PXSettings.AccountsService.ResetToDefaults();
            PXSettings.PimsService.ResetToDefaults();
            PXSettings.TokenPolicyService.ResetToDefaults();
            PXSettings.PurchaseService.ResetToDefaults();
            PXSettings.PartnerSettingsService.ResetToDefaults();
            PXSettings.OrchestrationService.ResetToDefaults();
            PXSettings.IssuerService.ResetToDefaults();
            PXSettings.CatalogService.ResetToDefaults();
            PXSettings.PaymentOrchestratorService.ResetToDefaults();
            PXSettings.NetworkTokenizationService.ResetToDefaults();
        }

        [DataRow("Account002", "Account002-Pi001-Visa-AgenticPayment", GlobalConstants.Partners.XPay, "us", "en-US")]
        [DataTestMethod]
        public async Task TokensEx_InitiateTokenization_Success(string accountId, string piid, string partner, string country, string language)
        {
            // Arrange
            string url = $"/v7.0/{accountId}/tokensEx?partner={partner}&piid={piid}&country={country}&language={language}";

            // Set up PSS response
            string expectedPSSResponse = "{\"default\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":null,\"resources\":null,\"features\":null}}";

            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            var payload = new
            {
                paymentMethodType = "visa",
                paymentInstrumentId = piid,
                accountHolderEmail = "TBD",
                currencyCode = "USD",
                totalAuthenticationAmount = "100",
                browserDataJsonString = "{\"browserJavaEnabled\":false,\"browserJavascriptEnabled\":true,\"browserLanguage\":\"en-US\",\"browserColorDepth\":\"24\",\"browserScreenHeight\":\"1080\",\"browserScreenWidth\":\"1920\",\"browserTimeZone\":\"420\",\"userAgent\":\"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/136.0.0.0 Safari/537.36\",\"browserHeader\":\"\",\"ipAddress\":\"104.28.3.141\"}",
                applicationUrl = "https://example.com",
                merchantName = "Test Merchant",
                sessionContextJsonString = "{\"secureToken\":\"<tokengeneratedByIframe>\"}",
                mandateJsonString = "[{\"mandateId\":\"1234\",\"preferredMerchantName\":\"Microsoft\",\"merchantCategory\":\"Commercial\",\"merchantCategoryCode\":\"1234\",\"declineThreshold\":{\"amount\":10,\"currencyCode\":\"USD\"},\"effectiveUntilTime\":\"2026\"}]",
                dfpSessionID = "oE9SWOglVqNLjcxMcAWjlXAXCNVqw0uJ"
            };

            // Act
            var response = await SendRequestPXService(url, HttpMethod.Post, new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"), null);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();
            Assert.IsNotNull(responseContent);
        }

        [DataRow("Account002", "Account002-Pi001-Visa-AgenticPayment", GlobalConstants.Partners.XPay, "us", "en-US", true)]
        [DataRow("Account002", "Account002-Pi001-Visa-AgenticPayment", GlobalConstants.Partners.XPay, "us", "en-US", false)]
        [DataTestMethod]
        public async Task TokensEx_InitiateTokenization_Success_UseNTSIntUrl(string accountId, string piid, string partner, string country, string language, bool enableUseIntUrlflight)
        {
            // Arrange
            string url = $"/v7.0/{accountId}/tokensEx?partner={partner}&piid={piid}&country={country}&language={language}";

            // Set up PSS response
            string expectedPSSResponse = "{\"default\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":null,\"resources\":null,\"features\":null}}";
            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            var payload = new
            {
                paymentMethodType = "visa",
                paymentInstrumentId = piid,
                accountHolderEmail = "TBD",
                currencyCode = "USD",
                totalAuthenticationAmount = "100",
                browserDataJsonString = "{\"browserJavaEnabled\":false,\"browserJavascriptEnabled\":true,\"browserLanguage\":\"en-US\",\"browserColorDepth\":\"24\",\"browserScreenHeight\":\"1080\",\"browserScreenWidth\":\"1920\",\"browserTimeZone\":\"420\",\"userAgent\":\"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/136.0.0.0 Safari/537.36\",\"browserHeader\":\"\",\"ipAddress\":\"104.28.3.141\"}",
                applicationUrl = "https://example.com",
                merchantName = "Test Merchant",
                sessionContextJsonString = "{\"secureToken\":\"<tokengeneratedByIframe>\"}",
                mandateJsonString = "[{\"mandateId\":\"1234\",\"preferredMerchantName\":\"Microsoft\",\"merchantCategory\":\"Commercial\",\"merchantCategoryCode\":\"1234\",\"declineThreshold\":{\"amount\":10,\"currencyCode\":\"USD\"},\"effectiveUntilTime\":\"2026\"}]",
                dfpSessionID = "oE9SWOglVqNLjcxMcAWjlXAXCNVqw0uJ"
            };

            Dictionary<string, string> headers = new Dictionary<string, string>();
            if (enableUseIntUrlflight)
            {
                headers = new Dictionary<string, string>
                {
                    { "x-ms-flight", "PXUseNTSIntUrl" }
                };
            }

            PXSettings.TokenizationService.PreProcess = (request) =>
            {
                if (enableUseIntUrlflight)
                {
                    Assert.IsTrue(request.RequestUri.AbsoluteUri.Contains($"nts.cp.microsoft-int.com"));
                }
                else
                {
                    Assert.IsTrue(request.RequestUri.AbsoluteUri.Contains($"mockNetworkTokenizationService"));
                }
            };

            // Act
            var response = await SendRequestPXService(url, HttpMethod.Post, new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"), headers);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();
            Assert.IsNotNull(responseContent);
        }

        [DataRow("Account002", "19c6f6df-8b31-4d29-b594-67621438e8d2", "c6f4cba0-2c09-4e4c-a6cf-20b3a17ff0dd", GlobalConstants.Partners.XPay, "us", "en-US")]
        [DataTestMethod]
        public async Task TokensEx_PostChallenge_Success(string accountId, string ntid, string challengeId, string partner, string country, string language)
        {
            // Arrange
            string targetChallengeMethodId = "email-method-789";
            string targetChallengeValue = "test@domain.com";
            string url = $"/v7.0/{TestAccountId}/TokensEx/{TestNtid}/challenges/{TestChallengeId}?partner={TestPartner}&country={TestCountry}&language={TestLanguage}";

            // Set up standard PSS response
            ////string expectedPSSResponse = "{\"default\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":null,\"resources\":null,\"features\":null}}";
            string expectedPSSResponse = "{\"default\":{\"template\":\"defaultTemplate\",\"features\":null}}";
            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            // Set up Network Tokenization Service responses
            var requestChallengeResponse = new RequestChallengeResponse
            {
                MaxValidationAttempts = 3
            };
            PXSettings.NetworkTokenizationService.ArrangeResponse(JsonConvert.SerializeObject(requestChallengeResponse));

            var challengeMethods = new List<ChallengeMethod>
            {
                new ChallengeMethod
                {
                    ChallengeMethodId = "sms-method-111",
                    ChallengeValue = "+1-555-0000",
                    ChallengeMethodType = ChallengeMethodType.OtpSms
                },
                new ChallengeMethod
                {
                    ChallengeMethodId = targetChallengeMethodId,
                    ChallengeValue = targetChallengeValue,
                    ChallengeMethodType = ChallengeMethodType.OtpEmail
                }
            };

            var payload = new
            {
                challengeMethodId = targetChallengeMethodId,
                challengeMethodsJsonString = JsonConvert.SerializeObject(challengeMethods),
                accountHolderEmail = "test@example.com",
                currencyCode = "USD",
                totalAuthenticationAmount = "100",
                browserDataJsonString = "{\"browserJavaEnabled\":false,\"browserJavascriptEnabled\":true,\"browserLanguage\":\"en-US\",\"browserColorDepth\":\"24\",\"browserScreenHeight\":\"960\",\"browserScreenWidth\":\"1707\",\"browserTimeZone\":\"420\",\"userAgent\":\"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36\",\"browserHeader\":\"text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7\",\"ipAddress\":\"104.28.3.199\"}",
                sessionContextJsonString = "{\"secureToken\":\"test-secure-token\"}",
                applicationUrl = "https://example.com",
                merchantName = "Test Merchant",
                dfpSessionID = "test-dfp-session-id"
            };

            // Act
            var response = await SendRequestPXService(url, HttpMethod.Post, new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"), null);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();

            var pidlResource = ReadSinglePidlResourceFromJson(responseContent);
            var pidlResources = pidlResource.ClientAction.Context as List<PIDLResource>;
            Assert.IsNotNull(pidlResources);

            // Verify the correct challenge value was used (not the other values)
            bool correctValueFound = false;
            foreach (var resource in pidlResources)
            {
                TextDisplayHint textDisplayHint = resource.GetDisplayHintById("enterChallengeCodeText") as TextDisplayHint;
                if (textDisplayHint != null)
                {
                    correctValueFound = true;
                    Assert.IsTrue(textDisplayHint.DisplayContent.Contains(targetChallengeValue), $"Display content should contain the target challenge value: {targetChallengeValue}");
                    Assert.IsFalse(textDisplayHint.DisplayContent.Contains("+1-555-0000"), "Display content should not contain other challenge values");
                    Assert.IsFalse(textDisplayHint.DisplayContent.Contains("+1-555-9999"), "Display content should not contain other challenge values");
                    break;
                }
            }

            Assert.IsTrue(correctValueFound, "Should have found the correct challenge value replacement");
        }

        [DataRow("Account002", "19c6f6df-8b31-4d29-b594-67621438e8d2", "c6f4cba0-2c09-4e4c-a6cf-20b3a17ff0dd", "test-challenge-method", GlobalConstants.Partners.XPay, "us", "en-US")]
        [DataTestMethod]
        public async Task TokensEx_ValidateChallenge_Success(string accountId, string ntid, string challengeId, string challengeMethodId, string partner, string country, string language)
        {
            // Arrange
            string url = $"/v7.0/{accountId}/TokensEx/{ntid}/challenges/{challengeId}/validate?partner={partner}&country={country}&language={language}&challengeMethodId={challengeMethodId}";

            // Set up PSS response
            string expectedPSSResponse = "{\"default\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":null,\"resources\":null,\"features\":null}}";
            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            var payload = new
            {
                pin = "123456",
                paymentMethodType = "visa",
                paymentInstrumentId = "Account002-Pi001-Visa-AgenticPayment"
            };

            // Act
            var response = await SendRequestPXService(url, HttpMethod.Post, new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"), null);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();
            Assert.IsNotNull(responseContent);
        }

        [DataRow("Account002", "19c6f6df-8b31-4d29-b594-67621438e8d2", GlobalConstants.Partners.XPay, "us", "en-US")]
        [DataTestMethod]
        public async Task TokensEx_Mandates_Success(string accountId, string ntid, string partner, string country, string language)
        {
            // Arrange            
            string url = $"/v7.0/{accountId}/TokensEx/{ntid}/mandates?partner={partner}&country={country}&language={language}";

            // Set up PSS response
            string expectedPSSResponse = "{\"default\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":null,\"resources\":null,\"features\":null}}";
            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            var payload = new
            {
                paymentMethodType = "visa",
                paymentInstrumentId = "d3bd4ab5 - 984a - 446b - a88d - 3bf90a2712cf",
                accountHolderEmail = "emailaddress@email.com",
                currencyCode = "USD",
                mandateJsonString = "[{\"mandateId\":\"1234\",\"merchantName\":\"Microsoft\",\"merchantCategory\":\"Commercial\",\"merchantCategoryCode\":\"1234\",\"authenticationAmount\":10,\"effectiveUntil\":\"2026\",\"currencyCode\":\"USD\"}]",
                applicationUrl = "https://expedia.com",
                merchantName = "Microsoft",
                totalAuthenticationAmount = "10",
                browserDataJsonString = "{\"browserJavaEnabled\":false,\"browserJavascriptEnabled\":true,\"browserLanguage\":\"en-US\",\"browserColorDepth\":\"24\",\"browserScreenHeight\":\"960\",\"browserScreenWidth\":\"1707\",\"browserTimeZone\":\"420\",\"userAgent\":\"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36\",\"browserHeader\":\"text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7\",\"ipAddress\":\"104.28.3.203\"}",
                sessionContextJsonString = "{\"secureToken\":\"ezAwMX06AAM1NkGLYh1WB9M_ItI1i3DhI1fDTQaTXfIBcW8j8F0KC0ZbLuNWAwzo81js6zL-uc1ASdXV5Xh0TAJio8qFTbNX5HDP7Dqk1TtuKhRr-5oJiDFdKu58uQhK1LHgsAKL5L5pX0Pgvjb4P2DcTojw6Vg2lybjO0LRPXJWF07-0qrhaM1cfc6tZ6rKgZ41JtLfX0Q8QTNCEfT22c1x5G2n388MxeREX88RK8_8-h-ntLkWkW3s4cIxfFegyg1a41CXNDVwXbHLYVJipBCJyQLCcXngSeTU0GHBJ-tf5OTDtOee6lR_3Lq5oDg-zFhXmjvqSTs_4gDf7aH_DnBF9K17ZWaGzaqVbpFywyu5p76jbRvM3l_56KIUeYzK7WXM017u-SVARA19z-t37jtCRN6Xkcw0uokXEtgTHeCc3E3PWgT3zi3c3sQc3A_mFlIXFCXz5tcGgdIYMSwv9judCJInp01cfYMlbrveAgJVzMdY4-SlpGCni7EL4fz4R2f0e56wjBXDm3H060DALKZFVQYtdhABiOnsh1JSz01YWbeBjAZWbTFCY8YLWeo8Tnx9jzWp2DXefYFC2nH5GS6bJ2V8bkJVS_KOL-M-IePVr8faKmz-_JB6qxXZOFME6SMIx_vAiZT8AEovicljQlsnED28Zqb79NpaEdjKcyuwXwSnnqOs3flshfmjT8T_Lb1DOKSHroa9GMUaJD7iBamlxHp3OL8eeOuLe8rQBYSajir3NBchyQ6d4bDbxh9vSfaFm3Wv59MgH_kFTDVr9vMKaYDitckYwJcJ1TGqNi1LuGkTjrCh6bCYmTSF4MVO-ZFu1VSK3WMCxlfqPIh6jI1YVsDfPYMA-03KcezJ_NeRM8QvFcovrFTCw3Qnw9ApJn_BLy_G95FCvFzXvpvn1zDMSxPtMGT-WY1LgFaWQEsQuhnIi8-2BGrek9Yj-eE9FalyrbXvQHXYtDLJ7_4EqIkJEABbl94RC3UOwJ76Eee5thuOC2nhbZva141oj9WPf8E8c-dns9PhcIxLtXdK178UYzeHSzUB-M1A\"}",
                FIDOResponse = "{\"fidoBlob\":\"dj0xJmM9ezAwMX06QUFNMU5rSDN6MW1YbVB0QkM4Q1ZlbWlHbjdOeW14TXJGa1Bndk92MGhaU1oxbm81NUpsZkhyYzcrQmVmRmZzRUtNWmZSWUgrTnd5UFhKS0oyOVE3SUwvU0hEclNJeWwyOXF5ZmxrUnF2Z3h4VnFXdnlxSTZNSitBWDhPN3duOHU4dVp0OGpoY2l5N1FOV1N6c0lXbDRQWmRLVHVQNGFoSW0yZEJ3RWJ2Tkd0OW5PQVZ6NDFYUk1sL2RzZXA2aGExdnBrbGFVWkVCU1NKUFBGUU5MbzIwOHZXekd3bUIrL0lZV3BSNUpqZ0l4WEdXK0JzM3d2UDdKY2J6MG5PK1Y4dVBSTnl4OEl5T05JU2RJZGRZS0hrL0JiR2F1allYVDVCWHRtT2JNcm1abTh0aU5xbU1QTGJHUStteFBWOVZGcjZUMmdTWFJrZDQzbTE1c0J3ait2VjRCV2hBdXAvNEF6NkRtQlVRUjM5VDZGM1F3ZjZDdXRWaFpSTWcvYWZiNmtXSy9peklUYm5qT1pQamlDV2xMUjB6aXUxRnJWT1pWRC9ISGxIZVNXZjhwaURkb1NzeFMxTDd1YmhjRHUyQU9QUkQzV3dUa2VpNzluS2laUElmaTVWaWhhY1BVUTJGZTdkZ292NVorMWdyajZZYkZ4eUxSa0RtRFBPN1E5NUtwVjkwbVJpcWVsTUVqa3FlTzdLVkQvWFFWeGtSUmYvK01heHIrMEhGR0FrM0ZxUnFzSmMwdStoRU1wSnpVWXE2MEgyekNiQk40dnJvVzhYZ3ZpSmZmN0VxTmVDUHYwek5uRXA0M212UkFZTmVKZ2phOEhabHlFOEdZRXpUNUxRVGQ5TWVKLy9uOFNCaWl1YWhiczVSWTRaMFlVbHBjbzlZUXZaNVdaS2tML0tzRVBHOVBsM0EyelVBejZRVExRRDAreTZjc1Rhb3UxaVFxTG1pYVhLbHFyeWZSREwrSWFFK2gwd0NDemY0NWs5aDhvNi9vWHc2c21WVE9wVHNPbU5tdUpFMWVSemFsT2c3djNzUHhMUnBIYXpHcnRocjBxempYVXRNTTdycDhWeXpVeVArc1BYTktTbFRUMUd1V0ZHOXQ3aVVNZU56SHBTYVNiazlKWDc2U0xCVEVkRmc1ck1QYk84eldRWS9KeVltZzFrODVXdVV5TTJlMU1iM2pBN05mTit5Ylp0NEw2bnI5K0J4aHFQU216V3FiSkc3S0VmdjBOM09kbjlzZlduVzk1aEFzZDNHalBMQjlXeGJSNGk1bTNUQSs5MWg4UHl5OU9Rbjhpc2hyR3NKZUc4UnMxbk11bURNTVM1bURxQmFQMGNYZHN1di9XN1RRSEFLT1cxQVIzak9aUTVDQjEzVjVaUWZBbm9LTS9ER2tMSjFxQ2paUXYwZkpFWWV3enY1eDdUUHRCM2hPMHpXSnpSR3FxY1Q1ZTdEMEhFemlIU2Z1Um10NCtiMGY0NDZXS3E5Qzh6V1p1alQ3TTFyNGpuZmM5MWFnbjJUZEZtRVVlQnh4dFpQNlpOS3R1Tk1IRy9qNDgvTzJVOW1zMEdlUXdEWEYzcjlMd3Y5SHdqd0NPU0pTT0M3N0dVWjkzUjJTOEtkZHJKMUF6eThNVFQzK1pwc0ZTL1lMeTdHUVlTK29HV29waU1GelBuWCt0Y0o5SThwWTROV093cmgyNEwzNWZLYWJjaTEyVVhUbHYyVHZmbmIxeWlKSFgyR0VMS21qYzRHSGltVzk2QlFDOW5lYTRHSlcxS0xYL1NkT1E0TGZyZzJHOWhTeExVVHZjRjN0QjB4aVBwdjdmTFlnR3VKYi9mQlE4UG1jZ3lna0hueUJLKzJaMG15cnBKUTNDMm5nK293UHdOdEUra0MveE0rZlBCWXRYQ1FYNmdZcFl3S0owSk00SVJtdE9yTUhnZWE4RE84TnltOXhVWDg5M3Y0SzBZTjhsQ1lqdWZqQ3paOE0rM2ZQVWhrV0ZONGFvdTd6RUhOMlRUdUw2V2F5QjN2Sk1QZFI2R2tTZktCUkdyL3lsYmV3R2ZlVTZYWDZKN3BsRlZ6eFZWelB5d3JZdmwrck1kdUFyUnVPNHBRSHlrTG1OZzV2SWpINFJSaGZ6LzE4KzZBbHk1STdMN0R1dWJZTU1URWd5Nnh1dkl3QzZ3ZnR4Q2NORkNjeHNaZzQ0ZTg3MmlFNGVOdkovTmM3YmZVRFNZREZacDRFd2duTEdXYVdyelVHZ0FmR0p4ZklpK2V3dFQ1YU02QlJFRHowNWJSYmMrVzNUdDB5Z0JrdFlBeHArTWFtWGpwdXl6NkhpOVhxRXFreEQ3Q2FXK3BYTGJ5S1pLT2w0TitJSWd2cHlIZU40a0kza2V4WlZUcmxJRUU2b002NXBMbm1jUERjalhPS0hISU1mSWVGa1pNS0ZVOHZWSEM4QXBiNEtGQzQ0QU11L0xjV09hY3N5ajJoV3B0VHhFUVphQ0R0TDFTaGFHbUhWUWZwYjgwNS9HUHhBZXpaZUg2ZzFaVEJOb1lOZGRNUVZXWElDdWR4OEdKUk5OYjcrSGFTemxoRzRwdHZsbmlyeGpZOUhzVllsNE5Va2VxODZSTTNHU2NJZTd6eXRNWmd0NTlCQXBjTU01dWVWcDcyOVNQbVljeHVaeVlUaGdkWUNGbWJpSnBYenBWMmJCMkNxSVZzZFY4STBNUmIwNGpWZU9qdmZIOGtES3ZhWlkwN09RbStRN3pUTDZSalBHdFRXR3FBSkFyTGthY2pQWWZqM3RKa2FPemkwT3EyM2l2RGt2SW1LcUs5Zlo0NXRWb3hLVm1XYkk0dHpsdnZQN0VnYWp5b1hmOHpFUEExamtoU3BXOGVDNEw1NDB3UWFXUHVlUWNReS8rYXh5MTZ2cEU1Z3B0Z3BpcTNQb2M5ZnJsR2NBMWJsZnp4VGdOTGtvZytYVndWV29xdDZWMnU4RitBMjBWNEFwQmx4Tlg0cVRsd3RpOWp6M1VRMkUzajBSWlBjam9CZ2kwcFZZV2hXRmZ6Z1F0VUhkMDUxZ1ZzKy9pMVZVcFNHS3d1aUQ1QVZpajB6YVNiaUl5RHRzTnhLdVp2aWVNY0RKL3RYaUdDYnRKZ1AzdkFubkdxcWFuQlhNa2w4blJuWExUU2sxOGxIVEdRS0I1RktlQ0dJSXc5L2tPZ05YN1JyYzVhMkNXcVpwQjlRZGNrMmRzcTZtelV4U0xtbEFvbUF0S3JlVkdZUkFKOXpoV2R2eTV2SC9RL3psc2Y0UEZHYWRVVURscmdaeVhYbEJzTDBqS0xZU1hyTktmUzNlSEVNOVdUbG5jUi9sb2JUSS9wc282TlVTSmt5emNldTRqTlEvaVhwc2N4MUo1ajJ1RkVwM0h2dFhlUE5kQzlqOTlUSWVaMjlmMmI1Wkd6OElqZVlnZjhDUm9rdTBhWWtDRThlcFdBc3ZIV3NBUE01MVJPZ2tqOGlTbnJwT3NCZ1JTVkJOVzBwRXVYY0dCT1p5NklTeGlRL1JiR21MaDZPdXE2Yk5CbUVhN21ZeGxDVkgrWU1uZTNtdGplbE5sSWNINFV2cGNHN2tkclBrNVE1WTVmUnlkYVF1Vnp1YjhzaUI3dzNIM09jdHdnV1lyaGw3WCt2bTVEQjJiUVN5bkNHNk0zdjhjOWlUdW9JcWNwS2hZUFkweGlxdU14M0QxNW9jeHZKa0RoUGxhR2hjcGIwZlZYdk8vc0ZXMkg0Q2JQaVV0VnUxd09CSjFFQUtXaU16YytHR2l4UytnWlBFTUVBTVJRa0lRMm1WZStPQ1BtZkVhV243YjVsczNJZjFJMHJqc25qLzlaWEpBQkw4M0dHL0hnblFUa21FK1ZkVk9KODJ4N0VvUU1zVURUNWFQN2lmQ1Z2K0FQaE5Ea1ljc1BKcGdLeGxtQ2JKOCsvbDd6NlByN25QejAvalorSld5aFBXdjlSSzJKWXBGTW5ET0N4N1hhS3Q4NVJ3Rkt6dHdtMDZ6ZjJiUEI3bWo1V0hERHVKaklFSUIyeEMrUmpDbk1Ia2hXdjBmbWJiRmY4cEtJcjY3Wk9KZFZtcEU5U3IyaWorQjlmM1JlbmdZcWpyYnRwb1ZhdEJ5aCtVVTJyTm5pL0NCV1A5bWhBeE5iTElMN0JWbm5PRkpSelYwWTRvVXlidWh4N0pQenByOHdaaHYwYWdXZUtOSk5VTFBtcWVOanF5WHROREVoZjZlWVhmbytqaTFlOWVyVlRjWHRqR29sRGRpcEVSOC9yclZxOHdUVktnRE1PUmZNb0dybFB1b3RSb3pmcStOZXhWeDcvK25IeFhxNWt3YjRsKzhMcDJBTDFRclhTQUtOdlI3b3h3ckFqNk45RHdXQS9wd2s1Rkl6SWQ4dFd5ZXMxcFlTMVZiOS90LzBNWGJXekh0TzczaUtWczJYZmo2R09MQUpScU1RaW9Gc3c2bUdjYVlOVHZuZkVvTW9ZZ1hjUE9HN2VFQk5jemtiTzd6UFNSWnVFazl5SnFvamhXd21ISS95TmxHTXBRdG9wamdyTWY2bVFzM0FJNmd6OGRCa1E5THRYWkczZUZjd2xmaCtSQVo2blpmcVUyS0R2dXBqWFNvM1J3UnkwRFBvdG5BVTRoYkJBNkFxY2p0TjIxSmNCQnpEck9pRGdqNWFSaE9xOG50dnZEV2hFaC9qRzNyTE1hdGlGVTBib1B3TVBBeG5nd25pTmFsY1JvRmtuTFVib2JhWXl6d1pXMmNQcENKZTUwM1haeEJXU1VPZDJWa0ZGSmZ3a0RBM1lGS0FGcU9lZHRDVFJYQkZYeDgyTC91VjJTTlRWNDJFcGJNM2NOTExCdFFYQ2M4SFljY1d4ZGNiR0FrcHZRR2lYOS90MW5OUkZOemVVb0tjV2JpV0lGRThPT3FaRXVmdlRhZ1pZdnpEVFdZQ0tqWUN5cUFVaFQ2RWkrdGVpbHVaM1ZzQTIxd3RUaG1CUWJ6Q2tjZkpWNTNqWkRRS2lCQXVmQ2dTMkhUNWxkcDVZY1dMVHBRcGZXQ1cyc3Y3ZWhjWWs1dDNURlN0MXJ4UXRHMG1EejFVdlBSQVNzS1JBamNBMEZzcVJlYlJRaEh5VjA1S1ErMnJNU0Z3VlJWdmZDdFVZTDRQQlJ1NEUzRzEyUDRlemtuc2RBVFpWS3Q5Sjk0bVpyUTJlVUMzZTUvTmdNa0NyRnBtL3NpYlVsSWpsSXZ2cjR2NDlYdmp2M1hhcmVWZGNkdnIvb2RQSU52QUFKL0hoR1c3cml6bjRjeFZtL0tFQXNqTHVhTnZFZDJKLzlPUVFKZjBOV21zeE5nVTFiTnZKRDBPSU9HWmphSFUrVGtGNnpObTB5R1BCbVZkSExuZEI3QnQ1S3QyekdCb25WL0IveUdvanFKdDc4YnlxWXcwUWdXUkVyNkJtM1ZseUtwdFFzV0VrNXZ2eUZRYXNXL2ZWNGlYVUhDV2JxUzRpd2dzY0V2VDNBS3VTVFM0ajZTT1JWYTFJbGFoWEhiVUFDZ1dPTFVEaHF5N1RickJsOFFFTFY0b2dmek93MGE4cUpySmQ4L3MwZ1lDd3dPaUE4bndObzI3aWJ0T0JmL3I4cnJ6OXdQWnhEbjdON3dGak5jN2ZiUmJQT21kOTNhRk1aT3ZEM1pUdG5GZWx5L3B0aG8xT2ZFMytFdWtSbmFXZmdzTVJqNDlHOW9SSDR5My9RL3MxeVh1OGNtYnAzdnFPMnIxck1IQXp3M0RkOHV2Z2JDdlc0MWh0N3A2MVJqRTdUd2xqWnJpNnptdEI5RDd0Q2FjZzJ2VGtaRVovTTQzRGhxaTc4eUhzYTRWdlozcXZOTmxmYkpmdUQ3ZXdVSUZDWVh5V1BGQUJGSE5hZ0oxN1IzNmYveWdaWk5TNnhIZDNJTlZ0K0R3SW5LQWY2WXIxS0VKSXc5YVltUzlEY1pEU3Q1Syt4NjhuUXZ1U3BkWXUzV2V6L1dsUXQzQTQ4aER4dGtKL3RGRTIybW5WSThMcFkyMC9XdmFkSHRDdVBGRUQ2NFhFa1Nwakw4Y2l6Zk1CNExsVVdtaVRURmo2TDA1QTQ3cStXUFc1K0lqdVRyc0J2OFkxM2ZJeDBWajhlSWJIWWI2VFJ2dXkyUkZXbFJYOHNsMDRqblREenI4UkxReDBPL042VThrTENkMkJEbjBNSjZ0RmRHbUVkd1R3NEc2aVlCRTIzL1VWL1RGLzJnblB3NFJocVFydW4vQ0h1bmRNUkUyWWxRWjRWekdYdlVXcG9rNFh0bXR6YlROWWRSbGNtWWhoSEU1SmxaYjZtOE9iZUJ4SmlUMFBJQ0JGSURqVTZsMG9qMFRGaFN3UjRPemlUSFBCb2pJMVhyU01leG41UXVPSVpCaGp6YS9uSUJ3Rk1iTmNKOW5NeVZUMWdzZmg4VExDdTVjMmxlUTFYakxVbkxtVXVPZDYxOVBkNkR0NnFVQW1aRzIraExJN21abHpJNE5BUXZtWUdFVnlXUjJLOFhieGxDcm5hVVlMWW9zamFxSVhoWmVSZ1kvbHNkZ3RkNzBMb2pKa3FkUGZZL1poZTdwdEVwQ1JmSXgxVENVNDRJb204T1V4eVhWallJVmpmTUUwTXJITFFhTGdoNEFzQk0rWWx3SjNqdnVrODdzVjBUTC9HR1NsRjQxWVVneHhEMkRRZStZUTk2eXppcGk3dnpKeGtQSEhLQzN2aEJjcnJNTFBRekJDMmkvaVNxdmJrVXFoZFJsdytXaG1tdlVpdFBqVE1LZFF2Ync1UjBMN1UzVzlQRzNJYktoY2E3UFlwdlVVS3p1VW9xUzhGQmRvNGgwdXNzZ3JDS1VoU0xsUmRDSjRpOE5DRlVydjJ6WThTQ0crdUV0Y0x4WUdGOUVLTlFhbkpCbWlvT25iYXkrZEZ6cTFXUndxbzJyM01wV29IYnJuOC92bGtONXJrNVVjbG5LUk8vOEFsOHJjMzNnPT0maD0yXzgtYTBEZw\",\"rpID\":\"dnRzLmF1dGgudmlzYS5jb20=\",\"identifier\":\"e1cb73fb13c24e02785a118914893502\"}",
                dfpSessionID = "R65Ffo073QkhcN6ZLKowHNCp6unoaBqt"
            };

            // Act
            var response = await SendRequestPXService(url, HttpMethod.Post, new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"), null);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();
            Assert.IsNotNull(responseContent);
        }
    }
}