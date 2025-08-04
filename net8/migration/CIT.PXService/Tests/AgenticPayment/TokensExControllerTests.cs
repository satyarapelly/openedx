// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2025. All rights reserved.</copyright>

namespace CIT.PXService.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    [TestClass]
    public class TokensExControllerTests : TestBase
    {
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

        [DataRow("Account002", "19c6f6df-8b31-4d29-b594-67621438e8d2", "c6f4cba0-2c09-4e4c-a6cf-20b3a17ff0dd", GlobalConstants.Partners.XPay, "us", "en-US")]
        [DataTestMethod]
        public async Task TokensEx_PostChallenge_Success(string accountId, string ntid, string challengeId, string partner, string country, string language)
        {
            // Arrange
            string url = $"/v7.0/{accountId}/TokensEx/{ntid}/challenges/{challengeId}?partner={partner}&country={country}&language={language}";

            // Set up PSS response
            string expectedPSSResponse = "{\"default\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":null,\"resources\":null,\"features\":null}}";
            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            var payload = new
            {
                challengeMethodId = "Zjg5YjY2NjhlMzU3ZjdkN2UzZDQxZmRkZmE1NDU2MDI=",
                accountHolderEmail = "emailaddress@email.com",
                currencyCode = "USD",
                mandateJsonString = "[{\"mandateId\":\"1234\",\"preferredMerchantName\":\"Microsoft\",\"merchantCategory\":\"Commercial\",\"merchantCategoryCode\":\"1234\",\"declineThreshold\":{\"amount\":10,\"currencyCode\":\"USD\"},\"effectiveUntilTime\":\"2026\"}]",
                applicationUrl = "https://expedia.com",
                merchantName = "Microsoft",
                totalAuthenticationAmount = "10",
                browserDataJsonString = "{\"browserJavaEnabled\":false,\"browserJavascriptEnabled\":true,\"browserLanguage\":\"en-US\",\"browserColorDepth\":\"24\",\"browserScreenHeight\":\"960\",\"browserScreenWidth\":\"1707\",\"browserTimeZone\":\"420\",\"userAgent\":\"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36\",\"browserHeader\":\"text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7\",\"ipAddress\":\"104.28.3.199\"}",
                sessionContextJsonString = "{\"secureToken\":\"ezAwMX06AARNNDJBwQVMbbkb-uInwlSASDZMRFvcGsVnJ1bB8gv_TDWaz5uVmGTMA3gGU_7io7J6LyIRnSrLKx-EsP4Cqgv5NJ1z70UqW9iQQ7AnerLOXqGwMQH_yh8Doq2U1ksLXoZN8j61ztUGAVZpTUj1IeH0TNnifq5u9SBgaa-YOz9GfryNiX08AiVouUBgKKeG_BZtiRICjoJXjzzAJHbVdyW10ocslMAn0iNyzGo6Ehs1Q2EEMDk2DCjiJf5LUQ-5uuTG7Nn2YvkKstPbxdwv19jZMR_zqiBaxBAqd325Z4COzrACmnthGsbWSWcaxPSwxt6NN3igCpqzX3TT4eZd9RS6mMZrijek6Gu43PROZlI4fCtfe73EX3cV0RvX69RTI73sTAIhgD00mQ6D-l5NeZiW-FvlI0ptBEDIjxh09TGbbeSls9B9pNS6yRAIQFFY73gNiVMXx5-AfuYm_rFRC7mwnDX8IoxoQqKTG7pCkN5FehjBgy4ezX4It9blucYMam5LJi8RiUvqxe08hEvIqJz_ozKTGgoAtNC-aFDiSc1GyN-wD6BDTrExozmSDrXucuHI7FwVVLO90tA4p3kt61oJ8mLXVx5Zgb-drSKWkqHBooL5oY4UliDFcCbXBy6nWcmIj75pXcGVgSXpDrrD1n5Zh7AonyOBaGvuuENbCMj5MPQD9mRj8GShSI9SzblUu3SshrPioO18pERjSsLZxfVsPCwWJE7E81XAuGcG4mTRWESNGdIzGhPBRLRjrq3597iUAzmbcCPAHrKg8h6vyOQ0tZ3Ar5cAS5BZR--V1x4XpAKklsfBYo64YROzhzZKZH5oaEB3Ap81KqMIzZVSObW-1oaTEtP_bO0Zd4r0WrlFcXzcXs-G4qYtgHHqnwKmFT9A_-PrUm9TupXZC7UI1UF2T_gJrxAnkZPctNME6mnQHjgb3hdSCvvDTCs4E5-saDmLydTbMEF1Fy-XI17BqcY2gzAHSEeQfqNW1Hy9ImblvQj8mhLG9pwfpChkG3dv2yDl-uhV4xSMd6Vf7Qt_5u1oILqS-w\"}",
                dfpSessionID = "oWJWGa5fBPSBfNrPXL2qIS969YBiCvKB"
            };

            // Act
            var response = await SendRequestPXService(url, HttpMethod.Post, new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"), null);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();
            Assert.IsNotNull(responseContent);
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
    }
}