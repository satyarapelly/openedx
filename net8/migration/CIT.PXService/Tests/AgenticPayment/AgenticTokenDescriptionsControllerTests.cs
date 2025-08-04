// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2025. All rights reserved.</copyright>

namespace CIT.PXService.Tests
{
    using global::Tests.Common.Model.Pidl;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    [TestClass]
    public class AgenticTokenDescriptionsControllerTests : TestBase
    {
        [TestInitialize]
        public void Startup()
        {
            // Reset any settings or emulators if needed
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
        }

        [DataRow(GlobalConstants.Partners.XPay, "get", "visa", 1, "Account002-Pi001-Visa-AgenticPayment", "en-US")]
        [DataRow(GlobalConstants.Partners.XPay, "get", "visa", 1, null, "en-US")]
        [DataTestMethod]
        public async Task GetAgenticTokenDescriptions(string partner, string operation, string type, int expectedPidlCount, string piid, string language)
        {
            // Arrange
            var requestUrl = $"/v7.0/Account002/agenticTokenDescriptions?country=us&type={type}&operation={operation}&partner={partner}&language={language}";

            if (!string.IsNullOrEmpty(piid))
            {
                requestUrl += $"&piid={piid}";
            }

            string expectedPSSResponse = "{\"default\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":null,\"resources\":null,\"features\":null}}";
            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(requestUrl);

            // Assert
            Assert.IsNotNull(pidls);
            Assert.AreEqual(expectedPidlCount, pidls.Count, $"Expected {expectedPidlCount} PIDL resources, but got {pidls.Count}");
        }
    }
}