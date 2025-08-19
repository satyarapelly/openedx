// <copyright file="Constants.cs" company="Microsoft">Copyright (c) Microsoft 2017. All rights reserved.</copyright>

namespace Test.Common
{
    using System;
    using System.Collections.Generic;

    public class Constants
    {
        public static readonly Type TestScenarioManagerType = typeof(TestScenarioManager);
        public static readonly Type TestScenarioManagersType = typeof(Dictionary<string, TestScenarioManager>);

        public enum AuthenticationType
        {
            Certificate,
            AAD,
            TestAADFallsBackToCert,
            NONE
        }

        public enum AADClientType
        {
            FirstParty,
            PME
        }

        public static class HeaderValues
        {
            public const string JsonContent = "application/json";
            public const string FormContent = "application/x-www-form-urlencoded";
            public const string HtmlContent = "text/html";
            public const string ExtendedFlightName = "x-ms-flight";
            public const string TestHeader = "x-ms-test";
        }

        public static class FlightValues
        {
            public const string PXUsePSSPartnerMockForDiffTest = "PXUsePSSPartnerMockForDiffTest";
            public const string AccountEmulatorValidateAddressWithAVS = "AccountEmulatorValidateAddressWithAVS";
        }

        internal static class PXDependencyEmulatorsMockResponseProviders
        {
            public const string Account = "AccountServiceMockResponseProvider";
            public const string PartnerSettings = "PartnerSettingsServiceMockResponseProvider";
            public const string PIMS = "PimsMockResponseProviderAugmented";
            public const string MSRewards = "MSRewardsServiceMockResponseProvider";
            public const string Catalog = "CatalogServiceMockResponseProvider";
            public const string IssuerService = "IssuerServiceMockResponseProvider";
            public const string ChallengeManagement = "ChallengeManagementServiceMockResponseProvider";
            public const string Purchase = "PurchaseServiceMockResponseProvider";
            public const string Risk = "RiskServiceMockResponseProvider";
            public const string TokenPolicy = "TokenPolicyServiceMockResponseProvider";
            public const string StoredValue = "StoredValueServiceMockResponseProvider";
            public const string Wallet = "WalletServiceMockResponseProvider";
            public const string SellerMarketPlace = "SellerMarketPlaceServiceMockResponseProvider";
            public const string TransactionData = "TransactionDataServiceMockResponseProvider";
            public const string TransactionService = "TransactionServiceMockResponseProvider";
            public const string PaymentOrchestrator = "PaymentOrchestratorServiceMockResponseProvider";
            public const string PayerAuth = "PayerAuthServiceMockResponseProvider";
            public const string FraudDetection = "FraudDetectionMockResponseProvider";
        }
    }
}