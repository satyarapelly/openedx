// <copyright file="Program.cs" company="Microsoft">Copyright (c) Microsoft 2016. All rights reserved.</copyright>

namespace SelfHostedPXService
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using SelfHostedPXServiceCore;

    public class Program
    {
        /// <summary>
        /// Application start method
        /// Tasks:
        /// Determine the test type from arguments [LoadTest, DiffTest, help]
        /// Initialize test
        /// Begin testing process
        /// Command line example: DiffTest.exe /test DiffTest /BaseEnv PPE /TestEnv INT /DiffFilePath C:\Path\To\Known\Delta.csv
        /// </summary>
        /// <param name="args">Console arguments for configuring tests</param>
        /// <returns>The exit code of the test run.</returns>
        public static async Task Main(string[] args)
        {
            string baseUrl = null;
            if (args.Length > 0)
            {
                baseUrl = args[0];
                Console.WriteLine("Initializing server on " + baseUrl);
            }
            else
            {
                Console.WriteLine("Initializing server");
            }
            
            using (var host = new SelfHostedPxService(baseUrl, true, false))
            {
                Console.WriteLine("Server initialized");

                try
                {
                    Console.WriteLine("Testing server");

                    // Testing server on /paymentMethodDescriptions so PX service does a full warm up, instead of calling /probe which doesn't warm up the pidl configurations
                    var relativeUrl = "users/me/paymentMethodDescriptions?country=tr&family=credit_card&type=mc&language=en-US&partner=storify&operation=add";
                    var url = string.Format("/v7.0/{0}", relativeUrl);
                    
                    var response = await GetPidlFromPXService(url);
                    var content = FormatJson(await response.Content.ReadAsStringAsync());

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Server successfully tested");
                    }
                    else
                    {
                        Console.WriteLine("Server successfully tested but returned: " + response.StatusCode);
                        Console.WriteLine("Response content: " + content);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                
                while (true)
                {
                    Console.Write("Listening...");
                    await Task.Delay(1000);
                }
            }
        }

        public static async Task<HttpResponseMessage> GetPidlFromPXService(string url)
        {
            var fullUrl = SelfHostedPxService.GetPXServiceUrl(url);
            if (fullUrl.Contains("completePrerequisites=true"))
            {
                fullUrl = fullUrl.Replace("users/me", "EmpAccountNoAddress");
            }
            else
            {
                fullUrl = fullUrl.Replace("users/me", "DiffTestUser");
            }

            fullUrl = fullUrl.Replace("users/my-org", "DiffOrgUser");

            return await SelfHostedPxService.PxHostableService.HttpSelfHttpClient.GetAsync(fullUrl);
        }

        private static string FormatJson(string json)
        {
            dynamic parsedJson = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        }
    }
}
