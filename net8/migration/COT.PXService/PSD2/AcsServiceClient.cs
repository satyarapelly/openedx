// <copyright file="AcsServiceClient.cs" company="Microsoft">Copyright (c) Microsoft 2019 - 2020. All rights reserved.</copyright>

namespace COT.PXService.PSD2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using System.Net;
    using System.IO;
    using Newtonsoft.Json.Linq;

    public class AcsServiceClient
    {
        private HttpClient acsHttpClient;

        public AcsServiceClient()
        {
            this.acsHttpClient = new HttpClient();
            this.acsHttpClient.Timeout = TimeSpan.FromSeconds(60);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; // lgtm[cs/hard-coded-security-protocol] -Suppressing because of a false positive from Semmle // DevSkim: ignore DS440000,DS440020 as old protocols are being explicitly removed
        }

        public async Task SendChallengeStatus(string acsSignedContent, AcsStatusPayload payload)
        {
            //// NOTE: This is only applicable to the tests, not used in real world
            string[] jwtTokens = acsSignedContent.Split('.');

            string jwtBody = DecodeBase64Url(jwtTokens[1]);
            JObject jwtPayload = JsonConvert.DeserializeObject(jwtBody) as JObject;
            string url = jwtPayload["acsURL"].Value<string>();

            Uri acsUri = new Uri(url);
            Uri newAcsUri = new UriBuilder(acsUri.Scheme, acsUri.Host, acsUri.Port, "acs/setstatus").Uri;

            string requestContent = JsonConvert.SerializeObject(payload);
            await SendDataToACS(requestContent, newAcsUri);
        }

        public async Task SendChallengeStatusData(string url, AcsStatusPayload payload)
        {
            Uri acsUri = new Uri(url);
            Uri newAcsUri = new UriBuilder(acsUri.Scheme, acsUri.Host, acsUri.Port, "acs/setstatus").Uri;

            string requestContent = JsonConvert.SerializeObject(payload);
            await SendDataToACS(requestContent, newAcsUri);
        }

        private async Task SendDataToACS(string requestContent, Uri acsUri)
        {
            const int MaxRetryCount = 3;
            int retryCount = 0;
            do
            {
                try
                {
                    using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, acsUri.AbsoluteUri))
                    {
                        requestMessage.Content = new StringContent(requestContent, Encoding.UTF8, "application/json");
                        using (HttpResponseMessage responseMessage = await this.acsHttpClient.SendAsync(requestMessage))
                        {
                            if (!responseMessage.IsSuccessStatusCode)
                            {
                                throw new Exception("Communication to the ACS Server has failed");
                            }

                            retryCount = MaxRetryCount; // exit the loop if reached here
                        }
                    }
                }
                catch (HttpRequestException hre)
                {
                    retryCount++;
                    if (retryCount == MaxRetryCount)
                    {
                        throw hre;
                    }
                }
                catch (TaskCanceledException tce)
                {
                    retryCount++;
                    if (retryCount == MaxRetryCount)
                    {
                        throw tce;
                    }
                }
            }
            while (retryCount < MaxRetryCount);
        }

        public static string DecodeBase64Url(string encodedValue)
        {
            string base64 = encodedValue.Replace('_', '/').Replace('-', '+');
            switch (encodedValue.Length % 4)
            {
                case 2:
                    base64 += "==";
                    break;

                case 3:
                    base64 += "=";
                    break;
            }

            byte[] bytes = Convert.FromBase64String(base64);
            return Encoding.ASCII.GetString(bytes);
        }
    }
}
