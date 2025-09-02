// <copyright file="LoadTest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace LoadTests
{
   
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using PidlTest;
    using Tests.Common.Model.Pidl;

    [TestClass]
    public class LoadTest
    {
        private HttpClient pidlClient = new HttpClient();

        private static bool UseEastUSPifdUrl { get; set; }

        private static string BaseEnvironment { get; set; }

        private static string AuthTokenUserName { get; set; }

        private static string AuthTokenPassword { get; set; }

        private static string AuthToken { get; set; }

        private static string NewAuthToken { get; set; }

        private const string TokenWrapper = "WLID1.0=\"{0}\"";

        // API Key refresh interval every 1 hours
        private static int authTokenRefreshInternvalInSec = 3600;
        private static int authTokenUpdateInternvalInSec = 3600;
        private static object authTokenLockObj = new object();
        private static DateTime authTokenLastUpdatedTime;

        private static string pifdUrl = "https://pifd.cp.microsoft-int.com{0}";
        private static string pxUrl = "https://paymentexperience.cp.microsoft.com{0}";
        private static string securePxServiceUrl = "https://securepxservice.cp.microsoft-int.com{0}";

        [AssemblyInitialize]
        public static void Initialize(TestContext context)
        {
            UseEastUSPifdUrl = bool.Parse(ConfigurationManager.AppSettings["UseEastUSPifdUrl"]);
            BaseEnvironment = ConfigurationManager.AppSettings["BaselineEnvironment"].ToLower();

            switch (BaseEnvironment)
            {
                case "ppe":
                    AuthTokenUserName = ConfigurationManager.AppSettings["PPE_Email"];
                    AuthTokenPassword = ConfigurationManager.AppSettings["PPE_Password"];
                    pifdUrl = "https://paymentinstruments.mp.microsoft.com{0}";
                    pxUrl = "https://paymentexperience.cp.microsoft.com{0}";
                    securePxServiceUrl = "https://securepxservice.cp.microsoft-int.com{0}";
                    break;
                case "prod":
                    AuthTokenUserName = ConfigurationManager.AppSettings["PROD_Email"];
                    AuthTokenPassword = ConfigurationManager.AppSettings["PROD_Password"];
                    pxUrl = "https://paymentexperience.cp.microsoft.com{0}";
                    securePxServiceUrl = "https://securepxservice.cp.microsoft.com{0}";
                    pifdUrl = UseEastUSPifdUrl ? "https://st-pifd-prod-eus2.azurewebsites.net{0}" : "https://paymentinstruments.mp.microsoft.com{0}";                    
                    break;
                case "feature":
                    AuthTokenUserName = ConfigurationManager.AppSettings["FEATURE_Email"];
                    AuthTokenPassword = ConfigurationManager.AppSettings["FEATURE_Password"];
                    pifdUrl = "https://st-pifd-prod-wcus.azurewebsites.net{0}";
                    break;
                case "int":
                default:
                    AuthTokenUserName = ConfigurationManager.AppSettings["INT_Email"];
                    AuthTokenPassword = ConfigurationManager.AppSettings["INT_Password"];
                    pifdUrl = "https://pifd.cp.microsoft-int.com{0}";
                    pxUrl = "https://paymentexperience-int-westus2.azurewebsites.net{0}";
                    securePxServiceUrl = "https://securepxservice.cp.microsoft-int.com{0}";
                    break;
            }

            AuthToken = string.Format(TokenWrapper, Generator.GenerateAsync(BaseEnvironment, AuthTokenUserName, AuthTokenPassword).Result);
            authTokenLastUpdatedTime = authTokenLastUpdatedTime = DateTime.UtcNow;
        }

        /// <summary>
        /// GetAuthToken - Generate security token, return new token every 60 mintues, beucase token validity is 60 minutes.
        /// </summary>
        /// <returns>Auth token</returns>
        private static string GetAuthToken()
        {
            if (!string.IsNullOrEmpty(AuthToken) &&
                    DateTime.UtcNow.Subtract(authTokenLastUpdatedTime).TotalSeconds <= authTokenRefreshInternvalInSec)
            {
                return AuthToken;
            }

            NewAuthToken = string.Format(TokenWrapper, Generator.GenerateAsync(BaseEnvironment, AuthTokenUserName, AuthTokenPassword).Result);

            if (DateTime.UtcNow.Subtract(authTokenLastUpdatedTime).TotalSeconds >= authTokenUpdateInternvalInSec)
            {
                lock (authTokenLockObj)
                {
                    AuthToken = NewAuthToken;
                    authTokenLastUpdatedTime = DateTime.UtcNow;
                }
            }

            return AuthToken;
        }

        [TestMethod]
        public async Task PaymentMethodDescriptionController_GET_Add()
        {
            this.pidlClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", GetAuthToken());
            var paymentMethodDescription = string.Format(pifdUrl, "/v6.0/users/me/paymentMethodDescriptions?partner=cart&operation=Add&country=US&language=en-US&family=credit_card");
            //var paymentMethodDescription = "https://st-pifd-prod-eus2.azurewebsites.net/v6.0/users/me/paymentMethodDescriptions?partner=cart&operation=Add&country=US&language=en-US&family=credit_card";
            this.pidlClient.DefaultRequestHeaders.Add("x-ms-flight", "PXUsePartnerSettingsService,PXEnablePIMSGetPaymentMethodsCache");
            HttpResponseMessage responseMessage = await pidlClient.GetAsync(paymentMethodDescription);
            int statusCode = (int)responseMessage.StatusCode;
            string message = responseMessage.Content != null ? await responseMessage.Content.ReadAsStringAsync() : "Empty Content";

            lock (this)
            {
                Assert.AreEqual(200, statusCode, message);
            }
            await Task.Delay(TimeSpan.FromMilliseconds(1000));
        }

        [TestMethod]
        public async Task PaymentMethodDescriptionController_GET_Select()
        {
            this.pidlClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", GetAuthToken());
            var paymentMethodDescription = string.Format(pifdUrl, "/v6.0/users/me/paymentMethodDescriptions?partner=webblends&operation=select&country=US&language=en-US");
            //var paymentMethodDescription = "https://st-pifd-prod-ncus.azurewebsites.net/v6.0/users/me/paymentMethodDescriptions?partner=webblends&operation=select&country=US&language=en-US";

            this.pidlClient.DefaultRequestHeaders.Add("x-ms-flight", "PXUsePartnerSettingsService,PXEnablePIMSGetPaymentMethodsCache");
            HttpResponseMessage responseMessage = await pidlClient.GetAsync(paymentMethodDescription);
            int statusCode = (int)responseMessage.StatusCode;
            string message = responseMessage.Content != null ? await responseMessage.Content.ReadAsStringAsync() : "Empty Content";

            lock (this)
            {
                Assert.AreEqual(200, statusCode, message);
            }
            await Task.Delay(TimeSpan.FromMilliseconds(1000));
        }

        [TestMethod]
        public async Task AddressDescriptionController_GET()
        {
            this.pidlClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", GetAuthToken());
            var addressDescription = string.Format(pifdUrl, "/v6.0/users/me/AddressDescriptions?type=billing&partner=cart&operation=Add&country=US&language=en-US");
            //var addressDescription = "https://st-pifd-prod-wus2.azurewebsites.net/v6.0/users/me/AddressDescriptions?type=billing&partner=cart&operation=Add&country=US&language=en-US";
            HttpResponseMessage responseMessage = await pidlClient.GetAsync(addressDescription);
            int statusCode = (int)responseMessage.StatusCode;
            string message = responseMessage.Content != null ? await responseMessage.Content.ReadAsStringAsync() : "Empty Content";
            
            lock (this)
            {
                Assert.AreEqual(200, statusCode, message);
            }
            await Task.Delay(TimeSpan.FromMilliseconds(1000));
        }

        [TestMethod]
        public async Task PaymentSessionDescriptionController_GET()
        {
            this.pidlClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", GetAuthToken());
            //With help of test portal and fiddler to get the URL with the test account which is created for load test.
            //Below URL will be used for INT environment
            //var paymentSessionDescription = string.Format(pifdUrl, "/v6.0/users/me/PaymentSessionDescriptions?paymentSessionData=%7B%22id%22%3A%22%22%2C%22isChallengeRequired%22%3Afalse%2C%22challengeStatus%22%3A%22%22%2C%22signature%22%3A%22%22%2C%22amount%22%3A0.99%2C%22challengeScenario%22%3A%22PaymentTransaction%22%2C%22challengeWindowSize%22%3A%2205%22%2C%22currency%22%3A%22USD%22%2C%22country%22%3A%22US%22%2C%22language%22%3A%22en-US%22%2C%22partner%22%3A%22cart%22%2C%22piid%22%3A%2244d2db38-6bf2-435a-9941-83f3ac73be6e%22%7D&operation=Add");
            var paymentSessionDescription = string.Format(pifdUrl, "/v6.0/users/me/PaymentSessionDescriptions?paymentSessionData=%7B%22id%22%3A%22%22%2C%22isChallengeRequired%22%3Afalse%2C%22challengeStatus%22%3A%22%22%2C%22signature%22%3A%22%22%2C%22amount%22%3A0.99%2C%22challengeScenario%22%3A%22PaymentTransaction%22%2C%22challengeWindowSize%22%3A%2205%22%2C%22currency%22%3A%22USD%22%2C%22country%22%3A%22US%22%2C%22language%22%3A%22en-US%22%2C%22partner%22%3A%22cart%22%2C%22piid%22%3A%22%2BRtjUgIAAAABAACA%22%7D&operation=Add");
            //var paymentSessionDescription = "https://st-pifd-prod-wcus.azurewebsites.net/v6.0/users/me/PaymentSessionDescriptions?paymentSessionData=%7B%22id%22%3A%22%22%2C%22isChallengeRequired%22%3Afalse%2C%22challengeStatus%22%3A%22%22%2C%22signature%22%3A%22%22%2C%22amount%22%3A0.99%2C%22challengeScenario%22%3A%22PaymentTransaction%22%2C%22challengeWindowSize%22%3A%2205%22%2C%22currency%22%3A%22USD%22%2C%22country%22%3A%22US%22%2C%22language%22%3A%22en-US%22%2C%22partner%22%3A%22cart%22%2C%22piid%22%3A%22%2BRtjUgIAAAABAACA%22%7D&operation=Add";
            HttpResponseMessage responseMessage = await pidlClient.GetAsync(paymentSessionDescription);
            int statusCode = (int)responseMessage.StatusCode;
            string message = responseMessage.Content != null ? await responseMessage.Content.ReadAsStringAsync() : "Empty Content";

            lock (this)
            {
                Assert.AreEqual(200, statusCode, message);
            }
            await Task.Delay(TimeSpan.FromMilliseconds(1000));
        }

        [TestMethod]
        public async Task ChallengeDescriptionController_GET()
        {
            this.pidlClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", GetAuthToken());
            //Below URL will be used for INT environment
            //var challengeDescription = string.Format(pifdUrl, "/v6.0/users/me/challengeDescriptions?timezoneOffset=420&paymentSessionOrData=%7B%22id%22%3A%22%22%2C%22isChallengeRequired%22%3Afalse%2C%22challengeStatus%22%3A%22%22%2C%22signature%22%3A%22%22%2C%22amount%22%3A0.99%2C%22challengeScenario%22%3A%22PaymentTransaction%22%2C%22challengeWindowSize%22%3A%2205%22%2C%22currency%22%3A%22USD%22%2C%22country%22%3A%22US%22%2C%22language%22%3A%22en-US%22%2C%22partner%22%3A%22cart%22%2C%22piid%22%3A%2244d2db38-6bf2-435a-9941-83f3ac73be6e%22%7D&operation=RenderPidlPage");
            var challengeDescription = string.Format(pifdUrl, "/v6.0/users/me/challengeDescriptions?timezoneOffset=420&paymentSessionOrData=%7B%22id%22%3A%22%22%2C%22isChallengeRequired%22%3Afalse%2C%22challengeStatus%22%3A%22%22%2C%22signature%22%3A%22%22%2C%22amount%22%3A0.99%2C%22challengeScenario%22%3A%22PaymentTransaction%22%2C%22challengeWindowSize%22%3A%2205%22%2C%22currency%22%3A%22USD%22%2C%22country%22%3A%22US%22%2C%22language%22%3A%22en-US%22%2C%22partner%22%3A%22cart%22%2C%22piid%22%3A%22%2BRtjUgIAAAABAACA%22%7D&operation=RenderPidlPage");
            //var challengeDescription = "https://st-pifd-prod-wus2.azurewebsites.net/v6.0/users/me/challengeDescriptions?timezoneOffset=420&paymentSessionOrData=%7B%22id%22%3A%22%22%2C%22isChallengeRequired%22%3Afalse%2C%22challengeStatus%22%3A%22%22%2C%22signature%22%3A%22%22%2C%22amount%22%3A0.99%2C%22challengeScenario%22%3A%22PaymentTransaction%22%2C%22challengeWindowSize%22%3A%2205%22%2C%22currency%22%3A%22USD%22%2C%22country%22%3A%22US%22%2C%22language%22%3A%22en-US%22%2C%22partner%22%3A%22cart%22%2C%22piid%22%3A%22%2BRtjUgIAAAABAACA%22%7D&operation=RenderPidlPage";
            HttpResponseMessage responseMessage = await pidlClient.GetAsync(challengeDescription);
            int statusCode = (int)responseMessage.StatusCode;
            string message = responseMessage.Content != null ? await responseMessage.Content.ReadAsStringAsync() : "Empty Content";

            lock (this)
            {
                Assert.AreEqual(200, statusCode, message);
            }
            await Task.Delay(TimeSpan.FromMilliseconds(1000));
        }

        [TestMethod]
        public async Task TaxIdDescriptionController_GET()
        {
            this.pidlClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", GetAuthToken());
            var taxIdDescription = string.Format(pifdUrl, "/v6.0/users/me/TaxIdDescriptions?partner=commercialstores&type=commercial_tax_id&scenario=withCountryDropdown&operation=Add&country=in&language=en-US");
            //var taxIdDescription = "https://st-pifd-prod-seas.azurewebsites.net/v6.0/users/me/TaxIdDescriptions?partner=commercialstores&type=commercial_tax_id&scenario=withCountryDropdown&operation=Add&country=in&language=en-US";
            HttpResponseMessage responseMessage = await pidlClient.GetAsync(taxIdDescription);
            int statusCode = (int)responseMessage.StatusCode;
            string message = responseMessage.Content != null ? await responseMessage.Content.ReadAsStringAsync() : "Empty Content";

            lock (this)
            {
                Assert.AreEqual(200, statusCode, message);
            }
            await Task.Delay(TimeSpan.FromMilliseconds(1000));
        }

        [TestMethod]
        public async Task ProfileDescriptionController_GET()
        {
            this.pidlClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", GetAuthToken());
            var profileDescription = string.Format(pifdUrl, "/v6.0/users/me/ProfileDescriptions?partner=commercialstores&type=employee&operation=Add&country=us&language=en-US");
            //var profileDescription = "https://st-pifd-prod-seas.azurewebsites.net/v6.0/users/me/ProfileDescriptions?partner=commercialstores&type=employee&operation=Add&country=us&language=en-US";
            
            HttpResponseMessage responseMessage = await pidlClient.GetAsync(profileDescription);
            int statusCode = (int)responseMessage.StatusCode;
            string message = responseMessage.Content != null ? await responseMessage.Content.ReadAsStringAsync() : "Empty Content";

            lock (this)
            {
                Assert.AreEqual(200, statusCode, message);
            }
            await Task.Delay(TimeSpan.FromMilliseconds(1000));
        }

        [TestMethod]
        public async Task PaymentMethodDescriptionController_UsingPss_GET()
        {
            this.pidlClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", GetAuthToken());
            this.pidlClient.DefaultRequestHeaders.Add("x-ms-flight", "PXUsePartnerSettingsService");
            var paymentMethodDescription = string.Format(pifdUrl, "/v6.0/users/me/paymentMethodDescriptions?partner=northstarweb&operation=Add&country=US&language=en-US&family=credit_card");
            HttpResponseMessage responseMessage = await pidlClient.GetAsync(paymentMethodDescription);
            int statusCode = (int)responseMessage.StatusCode;
            string message = responseMessage.Content != null ? await responseMessage.Content.ReadAsStringAsync() : "Empty Content";

            lock (this)
            {
                Assert.AreEqual(200, statusCode, message);
            }
            await Task.Delay(TimeSpan.FromMilliseconds(1000));
        }

        [TestMethod]
        public async Task ShortServiceUrl_CreateAndGet()
        {
            int getStatusCode = 200;
            bool isShortUrl = true;
            Guid guId = Guid.NewGuid();
            this.pidlClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", GetAuthToken());
            string paymentInstrumentsEx = string.Format(pifdUrl, "/v6.0/users/me/paymentInstrumentsEx?country=us&language=en-US&partner=storify&scenario=paypalQrCode");
            StringContent requestContent = new StringContent("{\"paymentMethodFamily\":\"ewallet\",\"paymentMethodType\":\"paypal\",\"paymentMethodOperation\":\"add\",\"paymentMethodCountry\":\"us\",\"paymentMethodResource_id\":\"ewallet.paypalQrCode\",\"sessionId\":\"" + guId + "\",\"riskData\":{\"dataType\":\"payment_method_riskData\",\"dataOperation\":\"add\",\"dataCountry\":\"us\"},\"details\":{\"dataType\":\"ewallet_paypalQrCode_details\",\"dataOperation\":\"add\",\"dataCountry\":\"us\",\"authenticationMode\":\"Redirect\"}}", Encoding.UTF8, "application/json");
            HttpResponseMessage responseMessage = await pidlClient.PostAsync(paymentInstrumentsEx, requestContent);
            int createStatusCode = (int)responseMessage.StatusCode;
            string message = responseMessage.Content != null ? await responseMessage.Content.ReadAsStringAsync() : "Empty Content";

            string resultContent = await responseMessage.Content.ReadAsStringAsync();
            PIDLResource pidlResource = JsonConvert.DeserializeObject<PIDLResource>(resultContent, new JsonConverter[] { new Tests.Common.Model.DisplayHintDeserializer(), new Tests.Common.Model.PidlObjectDeserializer() });
            List<PIDLResource> pidlList = pidlResource.ClientAction.Context as List<PIDLResource>;
            DisplayHint shortUrlDisplayHint = pidlList[0].GetDisplayHintById("paypalPIShortUrl");

            if (shortUrlDisplayHint != null && !string.IsNullOrWhiteSpace(shortUrlDisplayHint.DisplayText()))
            {
                isShortUrl = true;
                string shortServiceUrl = shortUrlDisplayHint.DisplayText();
                responseMessage = await pidlClient.GetAsync(shortServiceUrl);
                getStatusCode = (int)responseMessage.StatusCode;
            }
            else
            {
                isShortUrl = false;
            }

            lock (this)
            {
                Assert.AreEqual(200, createStatusCode, message);
                Assert.AreEqual(200, getStatusCode);
                Assert.IsTrue(isShortUrl);
            }
            await Task.Delay(TimeSpan.FromMilliseconds(1000));
        }

        [TestMethod]
        public async Task SecureFieldAppResource_GET()
        {
            string appResource = string.Format(securePxServiceUrl, "/resources/app.js");
            HttpResponseMessage responseMessage = await pidlClient.GetAsync(appResource);
            int statusCode = (int)responseMessage.StatusCode;
            string message = responseMessage.Content != null ? await responseMessage.Content.ReadAsStringAsync() : "Empty Content";

            lock (this)
            {
                Assert.AreEqual(200, statusCode, message);
            }
            await Task.Delay(TimeSpan.FromMilliseconds(1000));
        }

        [TestMethod]
        public async Task SecureFieldResource_GET()
        {
            string secureFieldResource = string.Format(securePxServiceUrl, "/resources/securefield.html");
            HttpResponseMessage responseMessage = await pidlClient.GetAsync(secureFieldResource);
            int statusCode = (int)responseMessage.StatusCode;
            string message = responseMessage.Content != null ? await responseMessage.Content.ReadAsStringAsync() : "Empty Content";

            lock (this)
            {
                Assert.AreEqual(200, statusCode, message);
            }
            await Task.Delay(TimeSpan.FromMilliseconds(1000));
        }
    }
}