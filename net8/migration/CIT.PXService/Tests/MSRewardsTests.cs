// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2019. All rights reserved.</copyright>

namespace CIT.PXService.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Metrics;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using global::Tests.Common.Model.Pidl;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel;
    using Microsoft.Commerce.Payments.PXService.Accessors.MSRewardsService.DataModel;
    using Microsoft.Commerce.Payments.PXService.Model.RewardsService;
    using Microsoft.Commerce.Payments.PXService.V7;
    using Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks;
    using Microsoft.Telemetry.Extensions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xbox.Experimentation.Contracts.GroupsAdmin;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using static CIT.PXService.Tests.AddressTestsUtil;
    using Constants = global::Tests.Common.Model.Pidl.Constants;

    [TestClass]
    public class MSRewardsTests : TestBase
    {
        [TestInitialize]
        public void Startup()
        {
            PXSettings.MSRewardsService.Responses.Clear();
            PXSettings.MSRewardsService.ResetToDefaults();
            PXSettings.PimsService.Responses.Clear();
            PXSettings.PimsService.ResetToDefaults();
        }

        [DataRow(0, "Completed", true)]
        [DataRow(2011, "PendingRiskReview", true)]
        [DataRow(0, "Completed", false)]
        [DataRow(2011, "PendingRiskReview", false)]
        [TestMethod]
        public async Task RedeemRewards_NoChallenge_Success(int redeemResultcode, string redeemStatus, bool isFlightEnabled)
        {
            // Arrange
            string userPuid = "844426234689219";
            string deviceId = "1234567890";
            string country = "us";

            if (isFlightEnabled)
            {
                PXFlightHandler.AddToEnabledFlights("PXEnableMSRewardsChallenge");
            }

            var rewardsServiceResponse = new
            {
                response = new
                {
                    result_message = "OrderId d2edf745-0909-4487-839d-f8d217d18c32 processed successfully.",
                    order = new
                    {
                        id = "d2edf745-0909-4487-839d-f8d217d18c32",
                        sku = "000400000253",
                        item_snapshot = new
                        {
                            name = "000400000253",
                            price = 1600,
                            config = new
                            {
                                amount = "1.25",
                                currencyCode = "USD"
                            }
                        }
                    }
                },
                correlationId = "4b6773bf40ff45eeae08c180ac33a0c9",
                code = redeemResultcode
            };

            var redeemRequestData = new
            {
                catalogItem = "000400000253"
            };

            // Arrange RewardsService response
            PXSettings.MSRewardsService.ArrangeResponse(JsonConvert.SerializeObject(rewardsServiceResponse), HttpStatusCode.OK, HttpMethod.Post);

            // Arrange PIMS response
            var expectedPIs = new List<global::Tests.Common.Model.Pims.PaymentInstrument>() { PimsMockResponseProvider.GetPaymentInstrument("Account011", "Account011-Pi010-StoredValue") };
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPIs));

            PXSettings.MSRewardsService.PreProcess = async (rewardsRequest) =>
            {
                string uri = rewardsRequest.RequestUri.ToString();
                Assert.IsTrue(uri.Contains($"api/users({userPuid})/orders"), $"Uri should contain api/users({userPuid})/orders");
                Assert.AreEqual(HttpMethod.Post, rewardsRequest.Method, "Method should be POST");

                string requestContent = await rewardsRequest.Content.ReadAsStringAsync();
                RedemptionRequest redemtionRequest = JsonConvert.DeserializeObject<RedemptionRequest>(requestContent);

                rewardsRequest.Headers.TryGetValues("X-Rewards-HasPI", out IEnumerable<string> hasPIValues);
                rewardsRequest.Headers.TryGetValues("X-Rewards-Country", out IEnumerable<string> countryValues);
                if (isFlightEnabled)
                {
                    Assert.IsNull(hasPIValues, "X-Rewards-HasPI should not be sent");
                }
                else
                {
                    Assert.AreEqual("true", hasPIValues.FirstOrDefault(), true, "X-Rewards-HasPI should be true");
                }

                Assert.AreEqual(country, countryValues.FirstOrDefault(), true, "X-Rewards-Country should be US");
                Assert.AreEqual(redeemRequestData.catalogItem, redemtionRequest.CatalogItem, "CatalogItem should match");
                Assert.AreEqual(deviceId, redemtionRequest.RiskContext.DeviceId, "DeviceId should match");
            };

            // Act
            Dictionary<string, string> requestHeaders = new Dictionary<string, string>
            {
                { "x-ms-msaprofile", $"PUID={userPuid}" },
                { "x-ms-deviceinfo", $"ipAddress=111.111.111.111,deviceId={deviceId}" }
            };

            HttpResponseMessage result = await SendRequestPXService($"/v7.0/Account011/msRewards?country={country}&language=en-US&partner=windowsstore", HttpMethod.Post, new StringContent(JsonConvert.SerializeObject(redeemRequestData), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType), requestHeaders);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode, "Expected statuscode is not found");

            if (result.Content != null)
            {
                var pidlResource = ReadSinglePidlResourceFromJson(await result.Content.ReadAsStringAsync());
                Assert.IsNotNull(pidlResource.ClientAction);
                Assert.AreEqual(ClientActionType.ReturnContext, pidlResource.ClientAction.ActionType);
                Assert.IsNotNull(pidlResource.ClientAction.Context);

                JObject contextData = JObject.Parse(pidlResource.ClientAction.Context.ToString());
                Assert.AreEqual(redeemStatus, contextData["status"].ToString());
                Assert.AreEqual("1.25", contextData["redeemAmount"].ToString());
                Assert.AreEqual("1600", contextData["redeemPoints"].ToString());
                if (string.Equals(redeemStatus, "Completed", StringComparison.OrdinalIgnoreCase))
                {
                    Assert.AreEqual(expectedPIs.FirstOrDefault().PaymentInstrumentId, contextData["csvPIID"].ToString());
                    Assert.AreEqual(expectedPIs.FirstOrDefault().PaymentInstrumentDetails.Balance.ToString(), contextData["csvBalance"].ToString());
                }
            }

            PXSettings.MSRewardsService.ResetToDefaults();
        }

        [DataRow(0, "Completed")]
        [DataRow(2011, "PendingRiskReview")]
        [TestMethod]
        public async Task RedeemRewards_NoChallenge_Success_variableRedemptionRequest(int redeemResultcode, string redeemStatus)
        {
            // Arrange
            string userPuid = "844426234689219";
            string deviceId = "1234567890";
            string country = "us";

            var rewardsServiceResponse = new
            {
                response = new
                {
                    result_message = "OrderId d2edf745-0909-4487-839d-f8d217d18c32 processed successfully.",
                    order = new
                    {
                        id = "d2edf745-0909-4487-839d-f8d217d18c32",
                        sku = "000400000253",
                        item_snapshot = new
                        {
                            name = "000400000253",
                            price = 1600,
                            config = new
                            {
                                amount = "1.25",
                                currencyCode = "USD"
                            }
                        },
                        a = new Dictionary<string, string>()
                        {
                            { "amount", "10" }, { "isVariableAmount", "true" }
                        },
                        p = 1000
                    }
                },
                correlationId = "4b6773bf40ff45eeae08c180ac33a0c9",
                code = redeemResultcode
            };

            var redeemRequestData = new
            {
                catalogItem = "000400000253",
                catalogItemAmount = "10"
            };

            // Arrange RewardsService response
            PXSettings.MSRewardsService.ArrangeResponse(JsonConvert.SerializeObject(rewardsServiceResponse), HttpStatusCode.OK, HttpMethod.Post);

            // Arrange PIMS response
            var expectedPIs = new List<global::Tests.Common.Model.Pims.PaymentInstrument>() { PimsMockResponseProvider.GetPaymentInstrument("Account011", "Account011-Pi010-StoredValue") };
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPIs));

            PXSettings.MSRewardsService.PreProcess = async (rewardsRequest) =>
            {
                string uri = rewardsRequest.RequestUri.ToString();
                Assert.IsTrue(uri.Contains($"api/users({userPuid})/orders"), $"Uri should contain api/users({userPuid})/orders");
                Assert.AreEqual(HttpMethod.Post, rewardsRequest.Method, "Method should be POST");

                string requestContent = await rewardsRequest.Content.ReadAsStringAsync();
                RedemptionRequest redemtionRequest = JsonConvert.DeserializeObject<RedemptionRequest>(requestContent);

                rewardsRequest.Headers.TryGetValues("X-Rewards-HasPI", out IEnumerable<string> hasPIValues);
                rewardsRequest.Headers.TryGetValues("X-Rewards-Country", out IEnumerable<string> countryValues);

                Assert.AreEqual("true", hasPIValues.FirstOrDefault(), true, "X-Rewards-HasPI should be true");
                Assert.AreEqual(country, countryValues.FirstOrDefault(), true, "X-Rewards-Country should be US");
                Assert.AreEqual(redeemRequestData.catalogItem, redemtionRequest.CatalogItem, "CatalogItem should match");
                Assert.AreEqual(redeemRequestData.catalogItemAmount, redemtionRequest.VariableRedemptionRequest.VariableAmount.ToString(), "catalogItemAmount should match");
                Assert.AreEqual(deviceId, redemtionRequest.RiskContext.DeviceId, "DeviceId should match");
            };

            // Act
            Dictionary<string, string> requestHeaders = new Dictionary<string, string>
            {
                { "x-ms-msaprofile", $"PUID={userPuid}" },
                { "x-ms-deviceinfo", $"ipAddress=111.111.111.111,deviceId={deviceId}" }
            };

            HttpResponseMessage result = await SendRequestPXService($"/v7.0/Account011/msRewards?country={country}&language=en-US&partner=windowsstore", HttpMethod.Post, new StringContent(JsonConvert.SerializeObject(redeemRequestData), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType), requestHeaders);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode, "Expected statuscode is not found");

            if (result.Content != null)
            {
                var pidlResource = ReadSinglePidlResourceFromJson(await result.Content.ReadAsStringAsync());
                Assert.IsNotNull(pidlResource.ClientAction);
                Assert.AreEqual(ClientActionType.ReturnContext, pidlResource.ClientAction.ActionType);
                Assert.IsNotNull(pidlResource.ClientAction.Context);

                JObject contextData = JObject.Parse(pidlResource.ClientAction.Context.ToString());
                Assert.AreEqual(redeemStatus, contextData["status"].ToString());
                Assert.AreEqual("10", contextData["redeemAmount"].ToString());
                Assert.AreEqual("1000", contextData["redeemPoints"].ToString());
                if (string.Equals(redeemStatus, "Completed", StringComparison.OrdinalIgnoreCase))
                {
                    Assert.AreEqual(expectedPIs.FirstOrDefault().PaymentInstrumentId, contextData["csvPIID"].ToString());
                    Assert.AreEqual(expectedPIs.FirstOrDefault().PaymentInstrumentDetails.Balance.ToString(), contextData["csvBalance"].ToString());
                }
            }

            PXSettings.MSRewardsService.ResetToDefaults();
        }

        [DataRow(2010, "E_RISK_FAILURE")]
        [DataRow(2042, "E_EXISTING_RISK_REVIEW")]
        [TestMethod]
        public async Task RedeemRewards_FailureWithErrorCode(int errorCode, string errorCodeName)
        {
            // Arrange
            string userPuid = "844426234689219";
            string deviceId = "1234567890";

            var rewardsServiceResponse = new
            {
                response = new
                {
                    result_message = "RandomErrorMessage",
                },
                correlationId = "4b6773bf40ff45eeae08c180ac33a0c9",
                code = errorCode
            };

            var redeemRequestData = new
            {
                catalogItem = "000400000253"
            };

            PXSettings.MSRewardsService.ArrangeResponse(JsonConvert.SerializeObject(rewardsServiceResponse), HttpStatusCode.OK, HttpMethod.Post);

            PXSettings.MSRewardsService.PreProcess = async (rewardsRequest) =>
            {
                string uri = rewardsRequest.RequestUri.ToString();
                Assert.IsTrue(uri.Contains($"api/users({userPuid})/orders"), $"Uri should contain api/users({userPuid})/orders");
                Assert.AreEqual(HttpMethod.Post, rewardsRequest.Method, "Method should be POST");

                string requestContent = await rewardsRequest.Content.ReadAsStringAsync();
                RedemptionRequest redemtionRequest = JsonConvert.DeserializeObject<RedemptionRequest>(requestContent);

                Assert.AreEqual(redeemRequestData.catalogItem, redemtionRequest.CatalogItem, "CatalogItem should match");
            };

            // Act
            Dictionary<string, string> requestHeaders = new Dictionary<string, string>
            {
                { "x-ms-msaprofile", $"PUID={userPuid}" },
                { "x-ms-deviceinfo", $"ipAddress=111.111.111.111,xboxLiveDeviceId={deviceId}" }
            };

            HttpResponseMessage result = await SendRequestPXService($"/v7.0/Account011/msRewards?country=us&language=en-US&partner=windowsstore", HttpMethod.Post, new StringContent(JsonConvert.SerializeObject(redeemRequestData), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType), requestHeaders);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode, "Expected statuscode is not found");

            if (result.Content != null)
            {
                var pidlResource = ReadSinglePidlResourceFromJson(await result.Content.ReadAsStringAsync());
                Assert.IsNotNull(pidlResource.ClientAction);
                Assert.AreEqual(ClientActionType.Failure, pidlResource.ClientAction.ActionType);
                Assert.IsNotNull(pidlResource.ClientAction.Context);

                JObject contextData = JObject.Parse(pidlResource.ClientAction.Context.ToString());
                Assert.AreEqual(errorCode.ToString(), contextData["errorCode"].ToString());
                Assert.AreEqual(errorCodeName, contextData["errorCodeName"].ToString());
                Assert.AreEqual("RandomErrorMessage", contextData["errorMessage"].ToString());
            }

            PXSettings.MSRewardsService.ResetToDefaults();
        }

        [DataRow(2007, "E_CHALLENGE_FIRST", true)]
        [DataRow(2007, "E_CHALLENGE_FIRST", false)]
        [TestMethod]
        public async Task RedeemRewards_ChallengeFirst(int errorCode, string errorCodeName, bool isVariableAmount)
        {
            // Arrange
            string userPuid = "844426234689219";
            string deviceId = "1234567890";

            var rewardsServiceResponse = new
            {
                response = new
                {
                    result_message = "Must challenge user id: E30F394398F1DF71549020FAC562B0F63B7A9042.",
                    msa_phone_number = "15735376488"
                },
                correlationId = "4b6773bf40ff45eeae08c180ac33a0c9",
                code = errorCodeName
            };

            var redeemRequestData = new Dictionary<string, string>
            {
                { "catalogItem", "000400000253" }
            };

            if (isVariableAmount)
            {
                redeemRequestData["catalogItemAmount"] = "10";
            }

            PXSettings.MSRewardsService.ArrangeResponse(JsonConvert.SerializeObject(rewardsServiceResponse), HttpStatusCode.OK, HttpMethod.Post);

            PXSettings.MSRewardsService.PreProcess = async (rewardsRequest) =>
            {
                string uri = rewardsRequest.RequestUri.ToString();
                Assert.IsTrue(uri.Contains($"api/users({userPuid})/orders"), $"Uri should contain api/users({userPuid})/orders");
                Assert.AreEqual(HttpMethod.Post, rewardsRequest.Method, "Method should be POST");

                string requestContent = await rewardsRequest.Content.ReadAsStringAsync();
                RedemptionRequest redemtionRequest = JsonConvert.DeserializeObject<RedemptionRequest>(requestContent);

                Assert.IsFalse(redemtionRequest.IsPhoneNumberOnVerificationCodeRequest, "phone_number_on_verification should be false");
                Assert.AreEqual(redeemRequestData["catalogItem"], redemtionRequest.CatalogItem, "CatalogItem should match");
                if (isVariableAmount)
                {
                    Assert.AreEqual(redeemRequestData["catalogItemAmount"], redemtionRequest.VariableRedemptionRequest.VariableAmount.ToString(), "catalogItemAmount should match");
                }
            };

            // Act
            Dictionary<string, string> requestHeaders = new Dictionary<string, string>
            {
                { "x-ms-msaprofile", $"PUID={userPuid}" },
                { "x-ms-deviceinfo", $"ipAddress=111.111.111.111,xboxLiveDeviceId={deviceId}" }
            };

            HttpResponseMessage result = await SendRequestPXService($"/v7.0/Account011/msRewards?country=us&language=en-US&partner=windowsstore", HttpMethod.Post, new StringContent(JsonConvert.SerializeObject(redeemRequestData), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType), requestHeaders);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode, "Expected statuscode is not found");

            if (result.Content != null)
            {
                var pidlResource = ReadSinglePidlResourceFromJson(await result.Content.ReadAsStringAsync());
                Assert.IsNotNull(pidlResource.ClientAction);
                Assert.AreEqual(ClientActionType.Pidl, pidlResource.ClientAction.ActionType);
                Assert.IsNotNull(pidlResource.ClientAction.Context);

                var pidlList = pidlResource.ClientAction.Context as List<PIDLResource>;
                Assert.AreEqual(1, pidlList.Count);
                var resource = pidlList[0];
                Assert.AreEqual(2, resource.DisplayPages.Count);

                PageDisplayHint selectChallengeTypeMSRewardsPage = resource.DisplayPages[0] as PageDisplayHint;
                Assert.IsNotNull(selectChallengeTypeMSRewardsPage);
                Assert.AreEqual(selectChallengeTypeMSRewardsPage.DisplayName, "selectChallengeTypeMSRewardsPage");

                PageDisplayHint editPhoneQrCodePage = resource.DisplayPages[1] as PageDisplayHint;
                Assert.IsNotNull(editPhoneQrCodePage);
                Assert.AreEqual(editPhoneQrCodePage.DisplayName, "editPhoneNumberMSRewardsPage");

                var pageHeading = resource.GetDisplayHintById("selectChallengeTypeMSRewardsHeading");
                Assert.IsNotNull(pageHeading, "selectChallengeTypeMSRewardsHeading is missing");
            }

            PXSettings.MSRewardsService.ResetToDefaults();
        }

        [DataRow(2007, "E_CHALLENGE_FIRST", true)]
        [DataRow(2007, "E_CHALLENGE_FIRST", false)]
        [TestMethod]
        public async Task RedeemRewards_ChallengeFirst_NoPhoneNumber(int errorCode, string errorCodeName, bool isVariableAmount)
        {
            // Arrange
            string userPuid = "844426234689219";
            string deviceId = "1234567890";

            var rewardsServiceResponse = new
            {
                response = new
                {
                    result_message = "Must challenge user id: E30F394398F1DF71549020FAC562B0F63B7A9042.",
                },
                correlationId = "4b6773bf40ff45eeae08c180ac33a0c9",
                code = errorCodeName
            };

            var redeemRequestData = new Dictionary<string, string>
            {
                { "catalogItem", "000400000253" }
            };

            if (isVariableAmount)
            {
                redeemRequestData["catalogItemAmount"] = "10";
            }

            PXSettings.MSRewardsService.ArrangeResponse(JsonConvert.SerializeObject(rewardsServiceResponse), HttpStatusCode.OK, HttpMethod.Post);

            PXSettings.MSRewardsService.PreProcess = async (rewardsRequest) =>
            {
                string uri = rewardsRequest.RequestUri.ToString();
                Assert.IsTrue(uri.Contains($"api/users({userPuid})/orders"), $"Uri should contain api/users({userPuid})/orders");
                Assert.AreEqual(HttpMethod.Post, rewardsRequest.Method, "Method should be POST");

                string requestContent = await rewardsRequest.Content.ReadAsStringAsync();
                RedemptionRequest redemtionRequest = JsonConvert.DeserializeObject<RedemptionRequest>(requestContent);
                Assert.AreEqual(RiskVerificationType.SMS, redemtionRequest.RiskContext.ChallengePreference, "ChallengePreference should be set to SMS");
                Assert.IsFalse(redemtionRequest.IsPhoneNumberOnVerificationCodeRequest, "phone_number_on_verification should be false");
                Assert.AreEqual(redeemRequestData["catalogItem"], redemtionRequest.CatalogItem, "CatalogItem should match");
                if (isVariableAmount)
                {
                    Assert.AreEqual(redeemRequestData["catalogItemAmount"], redemtionRequest.VariableRedemptionRequest.VariableAmount.ToString(), "catalogItemAmount should match");
                }
            };

            // Act
            Dictionary<string, string> requestHeaders = new Dictionary<string, string>
            {
                { "x-ms-msaprofile", $"PUID={userPuid}" },
                { "x-ms-deviceinfo", $"ipAddress=111.111.111.111,xboxLiveDeviceId={deviceId}" }
            };

            HttpResponseMessage result = await SendRequestPXService($"/v7.0/Account011/msRewards?country=us&language=en-US&partner=windowsstore", HttpMethod.Post, new StringContent(JsonConvert.SerializeObject(redeemRequestData), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType), requestHeaders);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode, "Expected statuscode is not found");

            if (result.Content != null)
            {
                var pidlResource = ReadSinglePidlResourceFromJson(await result.Content.ReadAsStringAsync());
                Assert.IsNotNull(pidlResource.ClientAction);
                Assert.AreEqual(ClientActionType.Pidl, pidlResource.ClientAction.ActionType);
                Assert.IsNotNull(pidlResource.ClientAction.Context);

                var pidlList = pidlResource.ClientAction.Context as List<PIDLResource>;
                Assert.AreEqual(1, pidlList.Count);
                var resource = pidlList[0];
                PageDisplayHint editPhoneQrCodePage = resource.DisplayPages[0] as PageDisplayHint;
                Assert.IsNotNull(editPhoneQrCodePage);
                Assert.AreEqual(editPhoneQrCodePage.DisplayName, "editPhoneNumberMSRewardsPage");
            }

            PXSettings.MSRewardsService.ResetToDefaults();
        }

        [DataRow(2008, "E_SOLVE_FIRST", true)]
        [DataRow(2008, "E_SOLVE_FIRST", false)]
        [TestMethod]
        public async Task RedeemRewards_SolveFirst(int errorCode, string errorCodeName, bool isVariableAmount)
        {
            // Arrange
            string userPuid = "844426234689219";
            string deviceId = "1234567890";

            var rewardsServiceResponse = new
            {
                response = new
                {
                    result_message = "MjArhwDHadfasdf1AaS3Kasptd",
                    msa_phone_number = "15735376488"
                },
                correlationId = "4b6773bf40ff45eeae08c180ac33a0c9",
                code = errorCode
            };

            var redeemRequestData = new Dictionary<string, string>
            {
                { "catalogItem", "000400000253" }
            };

            if (isVariableAmount)
            {
                redeemRequestData["catalogItemAmount"] = "10";
            }

            PXSettings.MSRewardsService.ArrangeResponse(JsonConvert.SerializeObject(rewardsServiceResponse), HttpStatusCode.OK, HttpMethod.Post);

            PXSettings.MSRewardsService.PreProcess = async (rewardsRequest) =>
            {
                string uri = rewardsRequest.RequestUri.ToString();
                Assert.IsTrue(uri.Contains($"api/users({userPuid})/orders"), $"Uri should contain api/users({userPuid})/orders");
                Assert.AreEqual(HttpMethod.Post, rewardsRequest.Method, "Method should be POST");

                string requestContent = await rewardsRequest.Content.ReadAsStringAsync();
                RedemptionRequest redemtionRequest = JsonConvert.DeserializeObject<RedemptionRequest>(requestContent);

                Assert.IsTrue(redemtionRequest.IsPhoneNumberOnVerificationCodeRequest, "phone_number_on_verification should be true");
                Assert.AreEqual(redeemRequestData["catalogItem"], redemtionRequest.CatalogItem, "CatalogItem should match");
                if (isVariableAmount)
                {
                    Assert.AreEqual(redeemRequestData["catalogItemAmount"], redemtionRequest.VariableRedemptionRequest.VariableAmount.ToString(), "catalogItemAmount should match");
                }
            };

            // Act
            Dictionary<string, string> requestHeaders = new Dictionary<string, string>
            {
                { "x-ms-msaprofile", $"PUID={userPuid}" },
                { "x-ms-deviceinfo", $"ipAddress=111.111.111.111,xboxLiveDeviceId={deviceId}" }
            };

            HttpResponseMessage result = await SendRequestPXService($"/v7.0/Account011/msRewards?country=us&language=en-US&partner=windowsstore", HttpMethod.Post, new StringContent(JsonConvert.SerializeObject(redeemRequestData), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType), requestHeaders);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode, "Expected statuscode is not found");

            if (result.Content != null)
            {
                var pidlResource = ReadSinglePidlResourceFromJson(await result.Content.ReadAsStringAsync());
                Assert.IsNotNull(pidlResource.ClientAction);
                Assert.AreEqual(ClientActionType.Pidl, pidlResource.ClientAction.ActionType);
                Assert.IsNotNull(pidlResource.ClientAction.Context);

                var pidlList = pidlResource.ClientAction.Context as List<PIDLResource>;
                Assert.AreEqual(1, pidlList.Count);
                var resource = pidlList[0];
                var pageHeading = resource.GetDisplayHintById("submitChallengeCodeHeading");
                Assert.IsNotNull(pageHeading, "submitChallengeCodeHeading is missing");
            }

            PXSettings.MSRewardsService.ResetToDefaults();
        }
    }
}
