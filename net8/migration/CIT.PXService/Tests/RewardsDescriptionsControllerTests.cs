// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2019. All rights reserved.</copyright>

namespace CIT.PXService.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using global::Tests.Common.Model.Pidl;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PXService.Accessors.MSRewardsService.DataModel;
    using Microsoft.Commerce.Payments.PXService.Model.RewardsService;
    using Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    [TestClass]
    public class RewardsDescriptionsControllerTests : TestBase
    {
        [TestInitialize]
        public void Startup()
        {
            PXSettings.MSRewardsService.Responses.Clear();
            PXSettings.MSRewardsService.ResetToDefaults();
        }

        [DataRow("US", "en-US", "windowsstore", 10, "usd", "2.5.5")]
        [DataRow("US", "en-US", "windowsstore", 10, "usd", "2.5.7")]
        [TestMethod]
        public async Task SelectMSRewards(string country, string language, string partner, int orderAmount, string currency, string pidlSdkVersion)
        {
            // Arrange
            string userPuid = "844426234689219";
            string deviceId = "1234567890";
            var rewardsContextData = new RewardsContextData
            {
                OrderAmount = orderAmount,
                Currency = currency
            };
            string rewardsContext = JsonConvert.SerializeObject(rewardsContextData);

            Version fullPidlSdkVersion = new Version(pidlSdkVersion + ".0");
            Version lowestCompatiblePidlVersion = new Version(2, 5, 7, 0);
            string pointsRedemptionContentLine2Value = fullPidlSdkVersion < lowestCompatiblePidlVersion ? " Microsoft Rewards points\n" : " Microsoft Rewards points";

            var rewardsServiceResponse = new
            {
                response = new
                {
                    Balance = 10000,
                    catalog = new List<UserFacingCatalogItem>()
                    {
                        new UserFacingCatalogItem()
                        {
                            Price = 9000,
                            Provider = "csv",
                            Attributes = new Dictionary<string, string>()
                            {
                                { "isMicrosoftCSVGiftCard", "true" }
                            },
                            Configuration = new Dictionary<string, string>()
                            {
                                { "amount", "9" }, { "currencyCode", "usd" }
                            }
                        }
                    }
                }
            };

            PXSettings.MSRewardsService.ArrangeResponse(JsonConvert.SerializeObject(rewardsServiceResponse), HttpStatusCode.OK, HttpMethod.Get);

            var expectedPIs = PimsMockResponseProvider.ListPaymentInstruments("Account011");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPIs));

            string expectedPSSResponse = Constants.PSSMockResponses.PXPartnerSettingsWindowsStore;
            PXSettings.PartnerSettingsService.ResetToDefaults();
            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            // Act
            Dictionary<string, string> requestHeaders = new Dictionary<string, string>
            {
                { "x-ms-msaprofile", $"PUID={userPuid}" },
                { "x-ms-deviceinfo", $"ipAddress=111.111.111.111,xboxLiveDeviceId={deviceId}" },
                { "x-ms-flight", "PXUsePartnerSettingsService" },
                { "x-ms-pidlsdk-version", pidlSdkVersion },
            };

            string url = $"/v7.0/Account001/rewardsDescriptions?type=MSRewards&operation=Select&country={country}&language={language}&partner={partner}&rewardsContextData={rewardsContext}";
            List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: requestHeaders);

            foreach (var pidl in pidls)
            {
                var csvTotalDataDescription = pidl.DataDescription["formattedCsvTotal"] as PropertyDescription;
                Assert.IsNotNull(csvTotalDataDescription);

                var csvBalanceDataDescription = pidl.DataDescription["csvBalance"] as PropertyDescription;
                Assert.IsNotNull(csvBalanceDataDescription);
                Assert.IsNotNull(csvBalanceDataDescription.DefaultValue, "csvBalance should be set");

                var pointsRedemptionContentLineGroup = pidl.GetDisplayHintById("pointsRedemptionContentLineGroup") as GroupDisplayHint;
                Assert.IsNotNull(pointsRedemptionContentLineGroup);
                Assert.AreEqual(pointsRedemptionContentLineGroup.Members.Count, 3);
                Assert.AreEqual(pointsRedemptionContentLineGroup.StyleHints.Count, 1);

                var styleHint = pointsRedemptionContentLineGroup.StyleHints[0];
                Assert.AreEqual(styleHint, "display-plain-textgroup");

                var pointsRedemptionContentLine1 = pidl.GetDisplayHintById("PointsRedemptionContentLine1") as TextDisplayHint;
                Assert.IsNotNull(pointsRedemptionContentLine1);
                Assert.AreEqual("9,000", pointsRedemptionContentLine1.DisplayContent);

                var pointsRedemptionContentLine2 = pidl.GetDisplayHintById("PointsRedemptionContentLine2") as TextDisplayHint;
                Assert.IsNotNull(pointsRedemptionContentLine2);
                Assert.AreEqual(pointsRedemptionContentLine2Value, pointsRedemptionContentLine2.DisplayContent);

                var useCsvContentLine1 = pidl.GetDisplayHintById("useCsvContentLine1") as TextDisplayHint;
                Assert.IsNotNull(useCsvContentLine1);
                Assert.AreEqual("Apply gift card balance\n", useCsvContentLine1.DisplayContent);

                var pointsRedemptionContentLineGrayTextGroup = pidl.GetDisplayHintById("pointsRedemptionContentLineGrayTextGroup") as GroupDisplayHint;
                Assert.IsNotNull(pointsRedemptionContentLineGrayTextGroup);
                Assert.AreEqual(pointsRedemptionContentLineGrayTextGroup.Members.Count, 3);

                var pointsRedemptionContentLine3 = pidl.GetDisplayHintById("pointsRedemptionContentLine3") as TextDisplayHint;
                Assert.IsNotNull(pointsRedemptionContentLine3);
                Assert.AreEqual("Points converted to gift card & applied with existing balance. ", pointsRedemptionContentLine3.DisplayContent);

                var useCsvImage = pidl.GetDisplayHintById("useCsvImage") as ImageDisplayHint;
                Assert.IsNotNull(useCsvImage);
                Assert.AreEqual("0xEB8E", useCsvImage.Codepoint);

                var pointsRedemptionImage = pidl.GetDisplayHintById("pointsRedemptionImage") as ImageDisplayHint;
                Assert.IsNotNull(pointsRedemptionImage);
                Assert.AreEqual("0xED4E", pointsRedemptionImage.Codepoint);

                var formattedCsvTotal = pidl.GetDisplayHintById("formattedCsvTotal") as PropertyDisplayHint;
                Assert.IsNotNull(formattedCsvTotal);
                Assert.AreEqual("<|ternary|{useRedeemPoints};$1.00;$10.00|>", formattedCsvTotal.ConditionalFields["value"]);

                DisplayHint pointsRedemptionNegativeFormattedPointsValueTotalExpression = pidl.GetDisplayHintById("pointsRedemptionNegativeFormattedPointsValueTotalExpression");
                DisplayHint pointsRedemptionNegativeFormattedPointsValueTotalAccentedExpression = pidl.GetDisplayHintById("pointsRedemptionNegativeFormattedPointsValueTotalAccentedExpression");
                DisplayHint useCsvNegativeFormattedCsvTotalExpression = pidl.GetDisplayHintById("useCsvNegativeFormattedCsvTotalExpression");
                DisplayHint useCsvNegativeFormattedCsvTotalAccentedExpression = pidl.GetDisplayHintById("useCsvNegativeFormattedCsvTotalAccentedExpression");

                Assert.IsTrue(pointsRedemptionNegativeFormattedPointsValueTotalExpression.DisplayTags["accessibilityNameExpression"] == "(negative {formattedPointsValueTotal})");
                Assert.IsTrue(pointsRedemptionNegativeFormattedPointsValueTotalAccentedExpression.DisplayTags["accessibilityNameExpression"] == "(negative {formattedPointsValueTotal})");
                Assert.IsTrue(useCsvNegativeFormattedCsvTotalExpression.DisplayTags["accessibilityNameExpression"] == "(negative {formattedCsvTotal})");
                Assert.IsTrue(useCsvNegativeFormattedCsvTotalAccentedExpression.DisplayTags["accessibilityNameExpression"] == "(negative {formattedCsvTotal})");
            }
        }

        [DataRow("US", "en-US", "windowsstore", 10, "usd", "2.5.5")]
        [DataRow("US", "en-US", "windowsstore", 10, "usd", "2.5.7")]
        [TestMethod]
        public async Task SelectMSRewardsVariableSku(string country, string language, string partner, int orderAmount, string currency, string pidlSdkVersion)
        {
            // Arrange
            string userPuid = "844426234689219";
            string deviceId = "1234567890";
            var rewardsContextData = new RewardsContextData
            {
                OrderAmount = orderAmount,
                Currency = currency
            };
            string rewardsContext = JsonConvert.SerializeObject(rewardsContextData);

            Version fullPidlSdkVersion = new Version(pidlSdkVersion + ".0");
            Version lowestCompatiblePidlVersion = new Version(2, 5, 7, 0);
            string pointsRedemptionContentLine2Value = fullPidlSdkVersion < lowestCompatiblePidlVersion ? " Microsoft Rewards points\n" : " Microsoft Rewards points";

            var rewardsServiceResponse = new
            {
                response = new
                {
                    Balance = 10000,
                    catalog = new List<UserFacingCatalogItem>()
                    {
                        new UserFacingCatalogItem()
                        {
                            Price = 10000,
                            Provider = "csv",
                            Attributes = new Dictionary<string, string>()
                            {
                                { "isMicrosoftCSVGiftCard", "true" },
                                { "variableItemPointsToCurrencyConversionRatio", "10" }
                            },
                            Configuration = new Dictionary<string, string>()
                            {
                                { "amount", "10" },
                                { "currencyCode", "usd" },
                                { "isVariableAmount", "true" },
                                { "variableRangeMax", "10000" },
                                { "variableRangeMin", "10" }
                            }
                        }
                    }
                }
            };

            PXSettings.MSRewardsService.ArrangeResponse(JsonConvert.SerializeObject(rewardsServiceResponse), HttpStatusCode.OK, HttpMethod.Get);

            var expectedPIs = PimsMockResponseProvider.ListPaymentInstruments("Account011");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPIs));

            string expectedPSSResponse = Constants.PSSMockResponses.PXPartnerSettingsWindowsStore;
            PXSettings.PartnerSettingsService.ResetToDefaults();
            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            // Act
            Dictionary<string, string> requestHeaders = new Dictionary<string, string>
            {
                { "x-ms-msaprofile", $"PUID={userPuid}" },
                { "x-ms-deviceinfo", $"ipAddress=111.111.111.111,xboxLiveDeviceId={deviceId}" },
                { "x-ms-flight", "PXUsePartnerSettingsService" },
                { "x-ms-pidlsdk-version", pidlSdkVersion },
            };

            string url = $"/v7.0/Account001/rewardsDescriptions?type=MSRewards&operation=Select&country={country}&language={language}&partner={partner}&rewardsContextData={rewardsContext}";
            List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: requestHeaders);

            foreach (var pidl in pidls)
            {
                var csvTotalDataDescription = pidl.DataDescription["formattedCsvTotal"] as PropertyDescription;
                Assert.IsNotNull(csvTotalDataDescription);

                var csvBalanceDataDescription = pidl.DataDescription["csvBalance"] as PropertyDescription;
                Assert.IsNotNull(csvBalanceDataDescription);
                Assert.IsNotNull(csvBalanceDataDescription.DefaultValue, "csvBalance should be set");

                var pointsRedemptionContentLineGroup = pidl.GetDisplayHintById("pointsRedemptionContentLineGroup") as GroupDisplayHint;
                Assert.IsNotNull(pointsRedemptionContentLineGroup);
                Assert.AreEqual(pointsRedemptionContentLineGroup.Members.Count, 3);
                Assert.AreEqual(pointsRedemptionContentLineGroup.StyleHints.Count, 1);

                var styleHint = pointsRedemptionContentLineGroup.StyleHints[0];
                Assert.AreEqual(styleHint, "display-plain-textgroup");

                var pointsRedemptionContentLine1 = pidl.GetDisplayHintById("PointsRedemptionContentLine1") as TextDisplayHint;
                Assert.IsNotNull(pointsRedemptionContentLine1);
                Assert.AreEqual("100", pointsRedemptionContentLine1.DisplayContent);

                var pointsRedemptionContentLine2 = pidl.GetDisplayHintById("PointsRedemptionContentLine2") as TextDisplayHint;
                Assert.IsNotNull(pointsRedemptionContentLine2);
                Assert.AreEqual(pointsRedemptionContentLine2Value, pointsRedemptionContentLine2.DisplayContent);

                var useCsvContentLine1 = pidl.GetDisplayHintById("useCsvContentLine1") as TextDisplayHint;
                Assert.IsNotNull(useCsvContentLine1);
                Assert.AreEqual("Apply gift card balance\n", useCsvContentLine1.DisplayContent);

                var pointsRedemptionContentLineGrayTextGroup = pidl.GetDisplayHintById("pointsRedemptionContentLineGrayTextGroup") as GroupDisplayHint;
                Assert.IsNotNull(pointsRedemptionContentLineGrayTextGroup);
                Assert.AreEqual(pointsRedemptionContentLineGrayTextGroup.Members.Count, 3);

                var pointsRedemptionContentLine3 = pidl.GetDisplayHintById("pointsRedemptionContentLine3") as TextDisplayHint;
                Assert.IsNotNull(pointsRedemptionContentLine3);
                Assert.AreEqual("Points converted to gift card & applied with existing balance. ", pointsRedemptionContentLine3.DisplayContent);

                var useCsvImage = pidl.GetDisplayHintById("useCsvImage") as ImageDisplayHint;
                Assert.IsNotNull(useCsvImage);
                Assert.AreEqual("0xEB8E", useCsvImage.Codepoint);

                var pointsRedemptionImage = pidl.GetDisplayHintById("pointsRedemptionImage") as ImageDisplayHint;
                Assert.IsNotNull(pointsRedemptionImage);
                Assert.AreEqual("0xED4E", pointsRedemptionImage.Codepoint);

                var formattedCsvTotal = pidl.GetDisplayHintById("formattedCsvTotal") as PropertyDisplayHint;
                Assert.IsNotNull(formattedCsvTotal);
                Assert.AreEqual("<|ternary|{useRedeemPoints};$0.00;$10.00|>", formattedCsvTotal.ConditionalFields["value"]);

                DisplayHint pointsRedemptionNegativeFormattedPointsValueTotalExpression = pidl.GetDisplayHintById("pointsRedemptionNegativeFormattedPointsValueTotalExpression");
                DisplayHint pointsRedemptionNegativeFormattedPointsValueTotalAccentedExpression = pidl.GetDisplayHintById("pointsRedemptionNegativeFormattedPointsValueTotalAccentedExpression");
                DisplayHint useCsvNegativeFormattedCsvTotalExpression = pidl.GetDisplayHintById("useCsvNegativeFormattedCsvTotalExpression");
                DisplayHint useCsvNegativeFormattedCsvTotalAccentedExpression = pidl.GetDisplayHintById("useCsvNegativeFormattedCsvTotalAccentedExpression");

                Assert.IsTrue(pointsRedemptionNegativeFormattedPointsValueTotalExpression.DisplayTags["accessibilityNameExpression"] == "(negative {formattedPointsValueTotal})");
                Assert.IsTrue(pointsRedemptionNegativeFormattedPointsValueTotalAccentedExpression.DisplayTags["accessibilityNameExpression"] == "(negative {formattedPointsValueTotal})");
                Assert.IsTrue(useCsvNegativeFormattedCsvTotalExpression.DisplayTags["accessibilityNameExpression"] == "(negative {formattedCsvTotal})");
                Assert.IsTrue(useCsvNegativeFormattedCsvTotalAccentedExpression.DisplayTags["accessibilityNameExpression"] == "(negative {formattedCsvTotal})");
            }
        }

        [DataRow("US", "en-US", "windowsstore", 10, "usd")]
        [TestMethod]
        public async Task SelectMSRewardsVariableSku_EmptyRewards(string country, string language, string partner, int orderAmount, string currency)
        {
            // Arrange
            string userPuid = "844426234689219";
            string deviceId = "1234567890";
            var rewardsContextData = new RewardsContextData
            {
                OrderAmount = orderAmount,
                Currency = currency
            };
            string rewardsContext = JsonConvert.SerializeObject(rewardsContextData);

            var rewardsServiceResponse = new
            {
                response = new
                {
                    Balance = 0,
                    catalog = new List<UserFacingCatalogItem>()
                    {
                        new UserFacingCatalogItem()
                        {
                            Price = 0,
                            Provider = "csv",
                            Attributes = new Dictionary<string, string>()
                            {
                                { "isMicrosoftCSVGiftCard", "true" },
                                { "variableItemPointsToCurrencyConversionRatio", "10" }
                            },
                            Configuration = new Dictionary<string, string>()
                            {
                                { "amount", "10" },
                                { "currencyCode", "usd" },
                                { "isVariableAmount", "true" },
                                { "variableRangeMax", "10000" },
                                { "variableRangeMin", "10" }
                            }
                        }
                    }
                }
            };

            PXSettings.MSRewardsService.ArrangeResponse(JsonConvert.SerializeObject(rewardsServiceResponse), HttpStatusCode.OK, HttpMethod.Get);

            var expectedPIs = PimsMockResponseProvider.ListPaymentInstruments("Account011");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPIs));

            string expectedPSSResponse = Constants.PSSMockResponses.PXPartnerSettingsWindowsStore;
            PXSettings.PartnerSettingsService.ResetToDefaults();
            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            // Act
            Dictionary<string, string> requestHeaders = new Dictionary<string, string>
            {
                { "x-ms-msaprofile", $"PUID={userPuid}" },
                { "x-ms-deviceinfo", $"ipAddress=111.111.111.111,xboxLiveDeviceId={deviceId}" },
                { "x-ms-flight", "PXUsePartnerSettingsService" }
            };

            string url = $"/v7.0/Account001/rewardsDescriptions?type=MSRewards&operation=Select&country={country}&language={language}&partner={partner}&rewardsContextData={rewardsContext}";
            List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: requestHeaders);

            foreach (var pidl in pidls)
            {
                var csvTotalDataDescription = pidl.DataDescription["formattedCsvTotal"] as PropertyDescription;
                Assert.IsNotNull(csvTotalDataDescription);

                var csvBalanceDataDescription = pidl.DataDescription["csvBalance"] as PropertyDescription;
                Assert.IsNotNull(csvBalanceDataDescription);
                Assert.IsNotNull(csvBalanceDataDescription.DefaultValue, "csvBalance should be set");

                var useCsvContentLine1 = pidl.GetDisplayHintById("useCsvContentLine1") as TextDisplayHint;
                Assert.IsNotNull(useCsvContentLine1);
                Assert.AreEqual("Apply gift card balance\n", useCsvContentLine1.DisplayContent);

                var useCsvImage = pidl.GetDisplayHintById("useCsvImage") as ImageDisplayHint;
                Assert.IsNotNull(useCsvImage);
                Assert.AreEqual("0xEB8E", useCsvImage.Codepoint);

                var formattedCsvTotal = pidl.GetDisplayHintById("formattedCsvTotal") as PropertyDisplayHint;
                Assert.IsNotNull(formattedCsvTotal);
                Assert.AreEqual("<|ternary|{useRedeemPoints};$10.00;$10.00|>", formattedCsvTotal.ConditionalFields["value"]);

                DisplayHint useCsvNegativeFormattedCsvTotalExpression = pidl.GetDisplayHintById("useCsvNegativeFormattedCsvTotalExpression");
                DisplayHint useCsvNegativeFormattedCsvTotalAccentedExpression = pidl.GetDisplayHintById("useCsvNegativeFormattedCsvTotalAccentedExpression");

                Assert.IsTrue(useCsvNegativeFormattedCsvTotalExpression.DisplayTags["accessibilityNameExpression"] == "(negative {formattedCsvTotal})");
                Assert.IsTrue(useCsvNegativeFormattedCsvTotalAccentedExpression.DisplayTags["accessibilityNameExpression"] == "(negative {formattedCsvTotal})");
            }
        }

        [DataRow("US", "en-US", "windowsstore", 10, "usd", "2.5.5")]
        [DataRow("US", "en-US", "windowsstore", 10, "usd", "2.5.7")]
        [TestMethod]
        public async Task SelectMSRewardsVariableSku_disableVariableAmountWithFlight(string country, string language, string partner, int orderAmount, string currency, string pidlSdkVersion)
        {
            // Arrange
            string userPuid = "844426234689219";
            string deviceId = "1234567890";
            var rewardsContextData = new RewardsContextData
            {
                OrderAmount = orderAmount,
                Currency = currency
            };
            string rewardsContext = JsonConvert.SerializeObject(rewardsContextData);

            Version fullPidlSdkVersion = new Version(pidlSdkVersion + ".0");
            Version lowestCompatiblePidlVersion = new Version(2, 5, 7, 0);
            string pointsRedemptionContentLine2Value = fullPidlSdkVersion < lowestCompatiblePidlVersion ? " Microsoft Rewards points\n" : " Microsoft Rewards points";

            var rewardsServiceResponse = new
            {
                response = new
                {
                    Balance = 10000,
                    catalog = new List<UserFacingCatalogItem>()
                    {
                        new UserFacingCatalogItem()
                        {
                            Price = 10000,
                            Provider = "csv",
                            Attributes = new Dictionary<string, string>()
                            {
                                { "isMicrosoftCSVGiftCard", "true" },
                                { "variableItemPointsToCurrencyConversionRatio", "10" }
                            },
                            Configuration = new Dictionary<string, string>()
                            {
                                { "amount", "10" },
                                { "currencyCode", "usd" },
                                { "isVariableAmount", "true" },
                                { "variableRangeMax", "10000" },
                                { "variableRangeMin", "10" }
                            }
                        },
                        new UserFacingCatalogItem()
                        {
                            Price = 9000,
                            Provider = "csv",
                            Attributes = new Dictionary<string, string>()
                            {
                                { "isMicrosoftCSVGiftCard", "true" }
                            },
                            Configuration = new Dictionary<string, string>()
                            {
                                { "amount", "9" }, { "currencyCode", "usd" }
                            }
                        }
                    }
                }
            };

            PXSettings.MSRewardsService.ArrangeResponse(JsonConvert.SerializeObject(rewardsServiceResponse), HttpStatusCode.OK, HttpMethod.Get);

            var expectedPIs = PimsMockResponseProvider.ListPaymentInstruments("Account011");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPIs));

            string expectedPSSResponse = Constants.PSSMockResponses.PXPartnerSettingsWindowsStore;
            PXSettings.PartnerSettingsService.ResetToDefaults();
            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            // Act
            Dictionary<string, string> requestHeaders = new Dictionary<string, string>
            {
                { "x-ms-msaprofile", $"PUID={userPuid}" },
                { "x-ms-deviceinfo", $"ipAddress=111.111.111.111,xboxLiveDeviceId={deviceId}" },
                { "x-ms-flight", "PXUsePartnerSettingsService,PXDisableMSRewardsVariableAmount" },
                { "x-ms-pidlsdk-version", pidlSdkVersion },
            };

            string url = $"/v7.0/Account001/rewardsDescriptions?type=MSRewards&operation=Select&country={country}&language={language}&partner={partner}&rewardsContextData={rewardsContext}";
            List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: requestHeaders);

            foreach (var pidl in pidls)
            {
                var pointsRedemptionContentLine1 = pidl.GetDisplayHintById("PointsRedemptionContentLine1") as TextDisplayHint;
                Assert.IsNotNull(pointsRedemptionContentLine1);
                Assert.AreEqual("9,000", pointsRedemptionContentLine1.DisplayContent);

                var pointsRedemptionContentLine2 = pidl.GetDisplayHintById("PointsRedemptionContentLine2") as TextDisplayHint;
                Assert.IsNotNull(pointsRedemptionContentLine2);
                Assert.AreEqual(pointsRedemptionContentLine2Value, pointsRedemptionContentLine2.DisplayContent);

                var formattedCsvTotal = pidl.GetDisplayHintById("formattedCsvTotal") as PropertyDisplayHint;
                Assert.IsNotNull(formattedCsvTotal);
                Assert.AreEqual("<|ternary|{useRedeemPoints};$1.00;$10.00|>", formattedCsvTotal.ConditionalFields["value"]);
            }
        }

        [DataRow("US", "en-US", "windowsstore", 10, "usd", "2.5.5")]
        [DataRow("US", "en-US", "windowsstore", 10, "usd", "2.5.7")]
        [TestMethod]
        public async Task SelectMSRewardsEmptyRewards(string country, string language, string partner, int orderAmount, string currency, string pidlSdkVersion)
        {
            // Arrange
            string userPuid = "844426234689219";
            string deviceId = "1234567890";
            var rewardsContextData = new RewardsContextData
            {
                OrderAmount = orderAmount,
                Currency = currency
            };
            string rewardsContext = JsonConvert.SerializeObject(rewardsContextData);

            var rewardsServiceResponse = new
            {
                response = new
                {
                    Balance = 0,
                    catalog = new List<UserFacingCatalogItem>()
                    {
                        new UserFacingCatalogItem()
                        {
                            Price = 0,
                            Provider = "csv",
                            Attributes = new Dictionary<string, string>()
                            {
                                { "isMicrosoftCSVGiftCard", "true" }
                            },
                            Configuration = new Dictionary<string, string>()
                            {
                                { "amount", "0" }, { "currencyCode", "usd" }
                            }
                        }
                    }
                }
            };

            PXSettings.MSRewardsService.ArrangeResponse(JsonConvert.SerializeObject(rewardsServiceResponse), HttpStatusCode.OK, HttpMethod.Get);

            var expectedPIs = PimsMockResponseProvider.ListPaymentInstruments("Account011");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPIs));

            string expectedPSSResponse = Constants.PSSMockResponses.PXPartnerSettingsWindowsStore;
            PXSettings.PartnerSettingsService.ResetToDefaults();
            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            // Act
            Dictionary<string, string> requestHeaders = new Dictionary<string, string>
            {
                { "x-ms-msaprofile", $"PUID={userPuid}" },
                { "x-ms-deviceinfo", $"ipAddress=111.111.111.111,xboxLiveDeviceId={deviceId}" },
                { "x-ms-flight", "PXUsePartnerSettingsService" },
                { "x-ms-pidlsdk-version", pidlSdkVersion },
            };

            string url = $"/v7.0/Account001/rewardsDescriptions?type=MSRewards&operation=Select&country={country}&language={language}&partner={partner}&rewardsContextData={rewardsContext}";
            List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: requestHeaders);

            foreach (var pidl in pidls)
            {
                var csvTotalDataDescription = pidl.DataDescription["formattedCsvTotal"] as PropertyDescription;
                Assert.IsNotNull(csvTotalDataDescription);

                var csvPiidDataDescription = pidl.DataDescription["csvPiid"] as PropertyDescription;
                Assert.IsNotNull(csvPiidDataDescription);

                var pointsRedemptionGroup = pidl.GetDisplayHintById("pointsRedemptionGroup") as GroupDisplayHint;
                Assert.IsNull(pointsRedemptionGroup);

                var pointsRedemptionContentLine2 = pidl.GetDisplayHintById("PointsRedemptionContentLine2") as TextDisplayHint;
                Assert.IsNull(pointsRedemptionContentLine2);

                var useCsvGroup = pidl.GetDisplayHintById("useCsvGroup") as GroupDisplayHint;
                Assert.IsNotNull(useCsvGroup);

                var formattedCsvTotal = pidl.GetDisplayHintById("formattedCsvTotal") as PropertyDisplayHint;
                Assert.IsNotNull(formattedCsvTotal);
                Assert.AreEqual("<|ternary|{useRedeemPoints};$10.00;$10.00|>", formattedCsvTotal.ConditionalFields["value"]);

                DisplayHint useCsvNegativeFormattedCsvTotalExpression = pidl.GetDisplayHintById("useCsvNegativeFormattedCsvTotalExpression");
                DisplayHint useCsvNegativeFormattedCsvTotalAccentedExpression = pidl.GetDisplayHintById("useCsvNegativeFormattedCsvTotalAccentedExpression");

                Assert.IsTrue(useCsvNegativeFormattedCsvTotalExpression.DisplayTags["accessibilityNameExpression"] == "(negative {formattedCsvTotal})");
                Assert.IsTrue(useCsvNegativeFormattedCsvTotalAccentedExpression.DisplayTags["accessibilityNameExpression"] == "(negative {formattedCsvTotal})");
            }
        }

        [DataRow("US", "en-US", "windowsstore", 10, "usd", "2.5.5")]
        [DataRow("US", "en-US", "windowsstore", 10, "usd", "2.5.7")]
        [TestMethod]
        public async Task SelectMSRewardsEmptyCSV(string country, string language, string partner, int orderAmount, string currency, string pidlSdkVersion)
        {
            // Arrange
            string userPuid = "844426234689219";
            string deviceId = "1234567890";
            var rewardsContextData = new RewardsContextData
            {
                OrderAmount = orderAmount,
                Currency = currency
            };
            string rewardsContext = JsonConvert.SerializeObject(rewardsContextData);

            Version fullPidlSdkVersion = new Version(pidlSdkVersion + ".0");
            Version lowestCompatiblePidlVersion = new Version(2, 5, 7, 0);
            string pointsRedemptionContentLine2Value = fullPidlSdkVersion < lowestCompatiblePidlVersion ? " Microsoft Rewards points\n" : " Microsoft Rewards points";

            var rewardsServiceResponse = new
            {
                response = new
                {
                    Balance = 100,
                    catalog = new List<UserFacingCatalogItem>()
                    {
                        new UserFacingCatalogItem()
                        {
                            Price = 90,
                            Provider = "csv",
                            Attributes = new Dictionary<string, string>()
                            {
                                { "isMicrosoftCSVGiftCard", "true" }
                            },
                            Configuration = new Dictionary<string, string>()
                            {
                                { "amount", "9" }, { "currencyCode", "usd" }
                            }
                        }
                    }
                }
            };

            PXSettings.MSRewardsService.ArrangeResponse(JsonConvert.SerializeObject(rewardsServiceResponse), HttpStatusCode.OK, HttpMethod.Get);

            var expectedPIs = PimsMockResponseProvider.ListPaymentInstruments("Account001");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPIs));

            string expectedPSSResponse = Constants.PSSMockResponses.PXPartnerSettingsWindowsStore;
            PXSettings.PartnerSettingsService.ResetToDefaults();
            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            // Act
            Dictionary<string, string> requestHeaders = new Dictionary<string, string>
            {
                { "x-ms-msaprofile", $"PUID={userPuid}" },
                { "x-ms-deviceinfo", $"ipAddress=111.111.111.111,xboxLiveDeviceId={deviceId}" },
                { "x-ms-flight", "PXUsePartnerSettingsService" },
                { "x-ms-pidlsdk-version", pidlSdkVersion },
            };

            string url = $"/v7.0/Account001/rewardsDescriptions?type=MSRewards&operation=Select&country={country}&language={language}&partner={partner}&rewardsContextData={rewardsContext}";
            List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: requestHeaders);

            foreach (var pidl in pidls)
            {
                var csvTotalDataDescription = pidl.DataDescription["formattedCsvTotal"] as PropertyDescription;
                Assert.IsNotNull(csvTotalDataDescription);

                var pointsRedemptionGroup = pidl.GetDisplayHintById("pointsRedemptionGroup") as GroupDisplayHint;
                Assert.IsNotNull(pointsRedemptionGroup);

                var pointsRedemptionContentLineGroup = pidl.GetDisplayHintById("pointsRedemptionContentLineGroup") as GroupDisplayHint;
                Assert.IsNotNull(pointsRedemptionContentLineGroup);
                Assert.AreEqual(pointsRedemptionContentLineGroup.Members.Count, 3);

                var pointsRedemptionContentLine2 = pidl.GetDisplayHintById("PointsRedemptionContentLine2") as TextDisplayHint;
                Assert.IsNotNull(pointsRedemptionContentLine2);
                Assert.AreEqual(pointsRedemptionContentLine2Value, pointsRedemptionContentLine2.DisplayContent);

                var useCsvGroup = pidl.GetDisplayHintById("useCsvGroup") as GroupDisplayHint;
                Assert.IsNull(useCsvGroup);

                var formattedCsvTotal = pidl.GetDisplayHintById("formattedCsvTotal") as PropertyDisplayHint;
                Assert.IsNotNull(formattedCsvTotal);
                Assert.AreEqual("<|ternary|{useRedeemPoints};$0.00;$0.00|>", formattedCsvTotal.ConditionalFields["value"]);

                DisplayHint pointsRedemptionNegativeFormattedPointsValueTotalExpression = pidl.GetDisplayHintById("pointsRedemptionNegativeFormattedPointsValueTotalExpression");
                DisplayHint pointsRedemptionNegativeFormattedPointsValueTotalAccentedExpression = pidl.GetDisplayHintById("pointsRedemptionNegativeFormattedPointsValueTotalAccentedExpression");

                Assert.IsTrue(pointsRedemptionNegativeFormattedPointsValueTotalExpression.DisplayTags["accessibilityNameExpression"] == "(negative {formattedPointsValueTotal})");
                Assert.IsTrue(pointsRedemptionNegativeFormattedPointsValueTotalAccentedExpression.DisplayTags["accessibilityNameExpression"] == "(negative {formattedPointsValueTotal})");
            }
        }

        [DataRow("US", "en-US", "windowsstore", 10, "usd")]
        [TestMethod]
        public async Task SelectMSRewardsEmptyCSVAndEmptyRewards(string country, string language, string partner, int orderAmount, string currency)
        {
            // Arrange
            string userPuid = "844426234689219";
            string deviceId = "1234567890";
            var rewardsContextData = new RewardsContextData
            {
                OrderAmount = orderAmount,
                Currency = currency
            };
            string rewardsContext = JsonConvert.SerializeObject(rewardsContextData);

            var rewardsServiceResponse = new
            {
                response = new
                {
                    Balance = 0,
                    catalog = new List<UserFacingCatalogItem>()
                    {
                        new UserFacingCatalogItem()
                        {
                            Price = 0,
                            Provider = "csv",
                            Attributes = new Dictionary<string, string>()
                            {
                                { "isMicrosoftCSVGiftCard", "true" }
                            },
                            Configuration = new Dictionary<string, string>()
                            {
                                { "amount", "0" }, { "currencyCode", "usd" }
                            }
                        }
                    }
                }
            };

            PXSettings.MSRewardsService.ArrangeResponse(JsonConvert.SerializeObject(rewardsServiceResponse), HttpStatusCode.OK, HttpMethod.Get);

            var expectedPIs = PimsMockResponseProvider.ListPaymentInstruments("Account001");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPIs));

            string expectedPSSResponse = Constants.PSSMockResponses.PXPartnerSettingsWindowsStore;
            PXSettings.PartnerSettingsService.ResetToDefaults();
            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            // Act
            Dictionary<string, string> requestHeaders = new Dictionary<string, string>
            {
                { "x-ms-msaprofile", $"PUID={userPuid}" },
                { "x-ms-deviceinfo", $"ipAddress=111.111.111.111,xboxLiveDeviceId={deviceId}" },
                { "x-ms-flight", "PXUsePartnerSettingsService" }
            };

            string url = $"/v7.0/Account001/rewardsDescriptions?type=MSRewards&operation=Select&country={country}&language={language}&partner={partner}&rewardsContextData={rewardsContext}";
            List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: requestHeaders);

            foreach (var pidl in pidls)
            {
                var csvTotalDataDescription = pidl.DataDescription["formattedCsvTotal"] as PropertyDescription;
                Assert.IsNotNull(csvTotalDataDescription);

                PageDisplayHint selectMSRewardsPage = pidl.DisplayPages[0] as PageDisplayHint;
                Assert.IsNotNull(selectMSRewardsPage);
                Assert.AreEqual(selectMSRewardsPage.DisplayName, "selectMSRewardsPage");

                Assert.AreEqual(selectMSRewardsPage.Members.Count, 1);
                var emptyText = selectMSRewardsPage.Members[0] as TextDisplayHint;
                Assert.IsNotNull(emptyText);
            }
        }

        [DataRow("US", "en-US", "windowsstore", 10, "usd")]
        [TestMethod]
        public async Task RedeemMSRewards(string country, string language, string partner, int orderAmount, string currency)
        {
            // Arrange
            string userPuid = "844426234689219";
            string deviceId = "1234567890";
            var rewardsContextData = new RewardsContextData
            {
                OrderAmount = orderAmount,
                Currency = currency
            };
            string rewardsContext = JsonConvert.SerializeObject(rewardsContextData);

            var rewardsServiceResponse = new
            {
                response = new
                {
                    Balance = 100,
                    catalog = new List<UserFacingCatalogItem>()
                    {
                        new UserFacingCatalogItem()
                        {
                            Name = "00001",
                            Price = 90,
                            Provider = "csv",
                            Attributes = new Dictionary<string, string>()
                            {
                                { "isMicrosoftCSVGiftCard", "true" }
                            },
                            Configuration = new Dictionary<string, string>()
                            {
                                { "amount", "9" }, { "currencyCode", "usd" }
                            }
                        }
                    }
                }
            };

            PXSettings.MSRewardsService.ArrangeResponse(JsonConvert.SerializeObject(rewardsServiceResponse), HttpStatusCode.OK, HttpMethod.Get);

            string expectedPSSResponse = Constants.PSSMockResponses.PXPartnerSettingsWindowsStore;
            PXSettings.PartnerSettingsService.ResetToDefaults();
            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            // Act
            Dictionary<string, string> requestHeaders = new Dictionary<string, string>
            {
                { "x-ms-msaprofile", $"PUID={userPuid}" },
                { "x-ms-deviceinfo", $"ipAddress=111.111.111.111,xboxLiveDeviceId={deviceId}" },
                { "x-ms-flight", "PXUsePartnerSettingsService" }
            };

            string url = $"/v7.0/Account001/rewardsDescriptions?type=MSRewards&operation=Redeem&country={country}&language={language}&partner={partner}&rewardsContextData={rewardsContext}";
            List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: requestHeaders);

            foreach (var pidl in pidls)
            {
                PageDisplayHint redeemMSRewardsPage = pidl.DisplayPages[0] as PageDisplayHint;
                Assert.IsNotNull(redeemMSRewardsPage);
                Assert.AreEqual(redeemMSRewardsPage.DisplayName, "redeemMSRewardsPage");

                // check if the page has a valid DisplayHintAction
                DisplayHintAction action = redeemMSRewardsPage.Action as DisplayHintAction;
                Assert.IsNotNull(action);
                Assert.AreEqual(action.ActionType, DisplayHintActionType.restAction.ToString());
            }
        }

        [DataRow("US", "en-US", "storify", 10, "usd", "00001", 2000, true)]
        [DataRow("US", "en-US", "storify", 10, "usd", "00001", 2000, false)]
        [TestMethod]
        public async Task RedeemMSRewardsXboxNative(string country, string language, string partner, int orderAmount, string currency, string catalogSku, long rewardsPoints, bool isVariableAmountSku)
        {
            // Arrange
            string userPuid = "844426234689219";
            string deviceId = "1234567890";
            decimal catalogSkuAmount = 2.00m;
            var rewardsContextData = new RewardsContextData
            {
                OrderAmount = orderAmount,
                Currency = currency,
                CatalogSku = catalogSku,
                CatalogSkuAmount = catalogSkuAmount,
                RewardsPoints = rewardsPoints,
                IsVariableAmountSku = isVariableAmountSku
            };
            string rewardsContext = JsonConvert.SerializeObject(rewardsContextData);

            // Act
            Dictionary<string, string> requestHeaders = new Dictionary<string, string>
            {
                { "x-ms-msaprofile", $"PUID={userPuid}" },
                { "x-ms-deviceinfo", $"ipAddress=111.111.111.111,xboxLiveDeviceId={deviceId}" },
                { "x-ms-flight", "PXEnableXboxNativeRewards" }
            };

            string url = $"/v7.0/Account001/rewardsDescriptions?type=MSRewards&operation=Redeem&country={country}&language={language}&partner={partner}&rewardsContextData={rewardsContext}";
            List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: requestHeaders);

            foreach (var pidl in pidls)
            {
                PageDisplayHint redeemMSRewardsPage = pidl.DisplayPages[0] as PageDisplayHint;
                Assert.IsNotNull(redeemMSRewardsPage);
                Assert.AreEqual(redeemMSRewardsPage.DisplayName, "redeemRewardsPointsPage");

                ButtonDisplayHint redeemButton = pidl.GetDisplayHintById("redeemButton") as ButtonDisplayHint;
                DisplayHintAction action = redeemButton.Action;
                Assert.IsNotNull(action);
                Assert.AreEqual(action.ActionType, DisplayHintActionType.restAction.ToString());
                var actionContext = JsonConvert.DeserializeObject<RestLink>(JsonConvert.SerializeObject(action.Context));
                Assert.IsNotNull(actionContext);
                var payload = JsonConvert.SerializeObject(actionContext.Payload);
                Assert.IsNotNull(payload);
                JObject payloadData = JObject.Parse(payload);
                Assert.AreEqual(catalogSku, payloadData["catalogItem"].ToString());

                if (isVariableAmountSku)
                {
                    Assert.AreEqual("2", payloadData["catalogItemAmount"].ToString());
                }
            }
        }

        [DataRow("US", "en-US", "windowsstore", 10, "usd")]
        [TestMethod]
        public async Task RedeemMSRewardsVariableSku(string country, string language, string partner, int orderAmount, string currency)
        {
            // Arrange
            string userPuid = "844426234689219";
            string deviceId = "1234567890";
            var rewardsContextData = new RewardsContextData
            {
                OrderAmount = orderAmount,
                Currency = currency
            };
            string rewardsContext = JsonConvert.SerializeObject(rewardsContextData);

            var rewardsServiceResponse = new
            {
                response = new
                {
                    Balance = 10000,
                    catalog = new List<UserFacingCatalogItem>()
                    {
                        new UserFacingCatalogItem()
                        {
                            Name = "00001",
                            Price = 10000,
                            Provider = "csv",
                            Attributes = new Dictionary<string, string>()
                            {
                                { "isMicrosoftCSVGiftCard", "true" },
                                { "variableItemPointsToCurrencyConversionRatio", "10" }
                            },
                            Configuration = new Dictionary<string, string>()
                            {
                                { "amount", "10" },
                                { "currencyCode", "usd" },
                                { "isVariableAmount", "true" },
                                { "variableRangeMax", "10000" },
                                { "variableRangeMin", "10" }
                            }
                        }
                    }
                }
            };

            PXSettings.MSRewardsService.ArrangeResponse(JsonConvert.SerializeObject(rewardsServiceResponse), HttpStatusCode.OK, HttpMethod.Get);

            string expectedPSSResponse = Constants.PSSMockResponses.PXPartnerSettingsWindowsStore;
            PXSettings.PartnerSettingsService.ResetToDefaults();
            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            // Act
            Dictionary<string, string> requestHeaders = new Dictionary<string, string>
            {
                { "x-ms-msaprofile", $"PUID={userPuid}" },
                { "x-ms-deviceinfo", $"ipAddress=111.111.111.111,xboxLiveDeviceId={deviceId}" },
                { "x-ms-flight", "PXUsePartnerSettingsService" }
            };

            string url = $"/v7.0/Account001/rewardsDescriptions?type=MSRewards&operation=Redeem&country={country}&language={language}&partner={partner}&rewardsContextData={rewardsContext}";
            List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: requestHeaders);

            foreach (var pidl in pidls)
            {
                PageDisplayHint redeemMSRewardsPage = pidl.DisplayPages[0] as PageDisplayHint;
                Assert.IsNotNull(redeemMSRewardsPage);
                Assert.AreEqual(redeemMSRewardsPage.DisplayName, "redeemMSRewardsPage");

                // check if the page has a valid DisplayHintAction
                DisplayHintAction action = redeemMSRewardsPage.Action as DisplayHintAction;
                Assert.IsNotNull(action);

                var actionContext = JsonConvert.DeserializeObject<RestLink>(JsonConvert.SerializeObject(redeemMSRewardsPage.Action.Context));
                var payload = JsonConvert.SerializeObject(actionContext.Payload);
                Assert.IsNotNull(payload);

                JObject payloadData = JObject.Parse(payload);
                Assert.AreEqual("10", payloadData["catalogItemAmount"].ToString());

                Assert.AreEqual(action.ActionType, DisplayHintActionType.restAction.ToString());
            }
        }

        [DataRow("US", "en-US", "windowsstore", 10, "usd")]
        [TestMethod]
        public async Task RedeemMSRewardsInsufficientBalance(string country, string language, string partner, int orderAmount, string currency)
        {
            // Arrange
            string userPuid = "844426234689219";
            string deviceId = "1234567890";
            var rewardsContextData = new RewardsContextData
            {
                OrderAmount = orderAmount,
                Currency = currency
            };
            string rewardsContext = JsonConvert.SerializeObject(rewardsContextData);

            var rewardsServiceResponse = new
            {
                response = new
                {
                    Balance = 0,
                    catalog = new List<UserFacingCatalogItem>()
                    {
                        new UserFacingCatalogItem()
                        {
                            Price = 0,
                            Provider = "csv",
                            Attributes = new Dictionary<string, string>()
                            {
                                { "isMicrosoftCSVGiftCard", "true" }
                            },
                            Configuration = new Dictionary<string, string>()
                            {
                                { "amount", "0" }, { "currencyCode", "usd" }
                            }
                        }
                    }
                }
            };

            PXSettings.MSRewardsService.ArrangeResponse(JsonConvert.SerializeObject(rewardsServiceResponse), HttpStatusCode.OK, HttpMethod.Get);

            string expectedPSSResponse = Constants.PSSMockResponses.PXPartnerSettingsWindowsStore;
            PXSettings.PartnerSettingsService.ResetToDefaults();
            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            // Act
            Dictionary<string, string> requestHeaders = new Dictionary<string, string>
            {
                { "x-ms-msaprofile", $"PUID={userPuid}" },
                { "x-ms-deviceinfo", $"ipAddress=111.111.111.111,xboxLiveDeviceId={deviceId}" },
                { "x-ms-flight", "PXUsePartnerSettingsService" }
            };

            string url = $"/v7.0/Account001/rewardsDescriptions?type=MSRewards&operation=Redeem&country={country}&language={language}&partner={partner}&rewardsContextData={rewardsContext}";
            List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: requestHeaders);

            foreach (var pidl in pidls)
            {
                Assert.IsNotNull(pidl.ClientAction);
                Assert.AreEqual(ClientActionType.Failure, pidl.ClientAction.ActionType);
                Assert.IsNotNull(pidl.ClientAction.Context);

                JObject contextData = JObject.Parse(pidl.ClientAction.Context.ToString());
                Assert.AreEqual("2002", contextData["errorCode"].ToString());
                Assert.AreEqual(RewardsErrorCode.E_INSUFFICIENT_BALANCE.ToString(), contextData["errorCodeName"].ToString());
                Assert.AreEqual("Insufficient balance", contextData["errorMessage"].ToString());
            }
        }
    }
}
