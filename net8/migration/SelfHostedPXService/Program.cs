// <copyright file="Program.cs" company="Microsoft">Copyright (c) Microsoft 2016. All rights reserved.</copyright>

namespace SelfHostedPXService
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using Newtonsoft.Json;
    using SelfHostedPXServiceCore;

    internal static class Program
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
            // optional base URL from args, e.g. http://localhost:49152
            string? baseUrl = args.Length > 0 ? args[0] : "http://localhost:49152";
            Console.WriteLine(baseUrl is null
                ? "Initializing server..."
                : $"Initializing server on {baseUrl}...");

            // Start the self-host
            var host = new SelfHostedPxService(baseUrl, true, false);
                var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

            Console.WriteLine("Server initialized.");

            // Do one warmup call; don't crash app if it fails
            try
            {
                Console.WriteLine("Warming up server...");
                var url = "v7.0/bc81f231-268a-4b9f-897a-43b7397302cc/paymentMethodDescriptions?type=amex%2Cvisa%2Cmc%2Cdiscover%2Cjcb&partner=commercialstores&operation=Add&country=US&language=en-US&family=credit_card&currency=USD";

                var response = await GetPidlFromPXService(url);
                var text = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Warmup status: {(int)response.StatusCode} {response.ReasonPhrase}");
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine(FormatJsonSafe(text));
                }

                // Additional request to verify that endpoint resolution is functioning. The diagnostic
                // middleware in SelfHostedPxService will print the resolved controller name for this
                // call to the console.
                Console.WriteLine("Verifying endpoint resolution via /v7.0/probe...");
                var verifyResp = await GetPidlFromPXService("v7.0/probe");
                Console.WriteLine($"Verification status: {(int)verifyResp.StatusCode} {verifyResp.ReasonPhrase}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Warmup failed (continuing to run):");
                Console.WriteLine(ex);
            }

            Console.WriteLine("Listening... Press Ctrl+C to stop.");
            try
            {
                await Task.Delay(Timeout.Infinite, cts.Token);
            }
            catch (OperationCanceledException) { /* Ctrl+C */ }
            finally
            {
                host.Dispose();
                Console.WriteLine("Server stopped.");
            }
        }

        private static async Task<HttpResponseMessage> GetPidlFromPXService(string url)
        {
            var fullUrl = SelfHostedPxService.GetPXServiceUrl(url);
            fullUrl = fullUrl.Contains("completePrerequisites=true", StringComparison.OrdinalIgnoreCase)
                ? fullUrl.Replace("users/me", "EmpAccountNoAddress", StringComparison.Ordinal)
                : fullUrl.Replace("users/me", "DiffTestUser", StringComparison.Ordinal);

            fullUrl = fullUrl.Replace("users/my-org", "DiffOrgUser", StringComparison.Ordinal);

            return await SelfHostedPxService.PxHostableService.HttpSelfHttpClient.GetAsync(fullUrl);
        }

        private static string FormatJsonSafe(string json)
        {
            try
            {
                var parsed = JsonConvert.DeserializeObject(json);
                return JsonConvert.SerializeObject(parsed, Newtonsoft.Json.Formatting.Indented);
            }
            catch
            {
                return json;
            }
        }
    }
}
