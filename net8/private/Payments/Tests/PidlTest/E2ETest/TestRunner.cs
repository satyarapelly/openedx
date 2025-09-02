// <copyright file="TestRunner.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace PidlTest.E2E
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;

    internal class TestRunner : ITestRunner
    {
        #region Constants
        private const string ArgTestEnv = "/testEnv";
        private const string ArgTestAcc = "/account";
        private const string ArgTestPwd = "/pwd";
        private const string ArgCCNum = "/cc";
        private const string ArgCVV = "/cvv";
        private const string Authorization = "Authorization";
        private const string LogPath = "./Logs/";
        private const string PPEBaseUrl = "https://paymentinstruments-int.mp.microsoft.com/V6.0/";
        private const string PRODBaseUrl = "https://paymentinstruments.mp.microsoft.com/V6.0/";
        private const string TokenWrapper = "WLID1.0=\"{0}\"";
        #endregion

        #region Variables
        private string testEnv;
        private string baseAddress;

        private string testAcc;
        private string testPwd;
        private string authToken;

        private string cardNum;
        private string cvvNum;
        private string panToken;
        private string cvvToken;

        private StreamWriter sw;
        #endregion

        private string[] partners = new string[] { "oxowebdirect", "webblends", "cart", "xbox", "webblends_inline", "amcxbox", "amcweb", "bing", "bingtravel", "webpay", "officeoobe", "oxooobe", "smboobe", "commercialstores", "azure", "azuresignup", "azureibiza", "mseg" };

        public bool ParseArguments(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], ArgTestEnv, StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 >= args.Length)
                    {
                        Console.WriteLine(Constants.ErrorMessages.ArgValueMissingFormat, ArgTestEnv);
                        return false;
                    }
                    else
                    {
                        this.testEnv = args[i + 1];
                    }
                }
                else if (string.Equals(args[i], ArgTestAcc, StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 >= args.Length)
                    {
                        Console.WriteLine(Constants.ErrorMessages.ArgValueMissingFormat, ArgTestAcc);
                        return false;
                    }
                    else
                    {
                        this.testAcc = args[i + 1];
                    }
                }
                else if (string.Equals(args[i], ArgTestPwd, StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 >= args.Length)
                    {
                        Console.WriteLine(Constants.ErrorMessages.ArgValueMissingFormat, ArgTestPwd);
                        return false;
                    }
                    else
                    {
                        this.testPwd = args[i + 1];
                    }
                }
                else if (string.Equals(args[i], ArgCCNum, StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 >= args.Length)
                    {
                        Console.WriteLine(Constants.ErrorMessages.ArgValueMissingFormat, ArgCCNum);
                        return false;
                    }
                    else
                    {
                        this.cardNum = args[i + 1];
                    }
                }
                else if (string.Equals(args[i], ArgCVV, StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 >= args.Length)
                    {
                        Console.WriteLine(Constants.ErrorMessages.ArgValueMissingFormat, ArgCVV);
                        return false;
                    }
                    else
                    {
                        this.cvvNum = args[i + 1];
                    }
                }
                else if (string.Compare(args[i], "/?", true) == 0 || string.Compare(args[i], "/help", true) == 0)
                {
                    ShowHelp();
                    return false;
                }
            }

            Console.WriteLine(@"Please check and update card information in bin\debug\E2ETest\postCC.json and hit enter to continue.");
            Console.Read();

            Console.WriteLine("Generate auth token...");
            this.authToken = string.Format(TokenWrapper, Generator.GenerateAsync(this.testEnv.ToLower(), this.testAcc, this.testPwd).Result);
            Console.WriteLine("Generate auth token done");

            switch (this.testEnv)
            {
                case "PPE":
                    this.baseAddress = PPEBaseUrl;
                    break;
                case "PROD":
                    this.baseAddress = PRODBaseUrl;
                    break;
            }

            return true;
        }

        public async Task<int> StartTestAsync()
        {
            try
            {
                JToken postCCPI = JObject.Parse(File.ReadAllText(@".\E2ETest\postCC.json"));
                string testLog = LogPath + "TestEvidence" + DateTime.Now.Ticks.ToString() + ".txt";

                foreach (string partner in this.partners)
                {
                    Console.WriteLine(string.Format("Start testing postCC for partner {0}.", partner));

                    using (HttpClient client = new HttpClient())
                    {
                        HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, this.baseAddress + string.Format("users/me/paymentMethodDescriptions?family={0}&country={1}&language={2}&partner={3}&operation={4}", "credit_card", "us", "en-us", partner, "Add"));
                        client.DefaultRequestHeaders.TryAddWithoutValidation(Authorization, this.authToken);
                        HttpResponseMessage response = await client.SendAsync(requestMessage);
                        string addPIPidl = await response.Content.ReadAsStringAsync();

                        Console.WriteLine(string.Format("Add cc for partner {0} response: {1}", partner, response.StatusCode.ToString()));

                        if (string.Compare(HttpStatusCode.OK.ToString(), response.StatusCode.ToString()) != 0)
                        {
                            Console.WriteLine(string.Format("Test addCC for partner {0} failed.", partner));
                            Console.WriteLine(addPIPidl);
                            Console.WriteLine();
                            continue;
                        }

                        ////verify pidl
                        bool.Equals(addPIPidl.Contains("credit_card.visa"), bool.TrueString);
                        bool.Equals(addPIPidl.Contains("credit_card.mc"), bool.TrueString);
                        bool.Equals(addPIPidl.Contains("credit_card.amex"), bool.TrueString);
                        bool.Equals(addPIPidl.Contains("credit_card.discover"), bool.TrueString);

                        ////Generate panToken and cvvToken
                        await this.GenerateAccoutCvvToken();

                        ////Update the post content with generated panToken and cvvToken
                        JObject details = (JObject)postCCPI["details"];
                        details["accountToken"] = this.panToken;
                        details["cvvToken"] = this.cvvToken;

                        ////Update sessionId for each post
                        postCCPI["sessionId"] = Guid.NewGuid();

                        StringContent httpContent = new StringContent(postCCPI.ToString(), Encoding.UTF8);
                        response = await client.PostAsync(this.baseAddress + string.Format("users/me/paymentInstrumentsEx?country={0}&language={1}&partner={2}", "us", "en-us", partner), httpContent);
                        string postCCPidl = await response.Content.ReadAsStringAsync();

                        Console.WriteLine(string.Format("Post cc for partner {0} response: {1}", partner, response.StatusCode.ToString()));
                        if (string.Compare(HttpStatusCode.OK.ToString(), response.StatusCode.ToString()) != 0)
                        {
                            Console.WriteLine(string.Format("Test postCC for partner {0} failed.", partner));
                            Console.WriteLine(postCCPidl);
                            Console.WriteLine();
                            continue;
                        }

                        ////Generate test evidence
                        this.sw = File.AppendText(testLog);
                        this.sw.WriteLine("Add Credit Card for {0}:", partner);
                        this.sw.WriteLine(postCCPidl);
                        this.sw.WriteLine();
                        this.sw.Flush();
                        this.sw.Close();
                    }

                    Console.WriteLine();
                }

                Console.WriteLine(string.Format("Testing postCC for all partners done in {0}.", this.testEnv));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return 0;
        }

        public async Task StopTestAsync()
        {
            await Task.FromResult<object>(null);
        }

        private static void ShowHelp()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("--------------------------------------------------------------------------------------------------------------");
            stringBuilder.AppendLine("PIDLTest.exe /testType E2ETest /testEnv <PPE, PROD> /account <msa account> /pwd <password> /cc <credit card number> /cvv <cvv>");
            stringBuilder.AppendLine(string.Empty);
            stringBuilder.AppendLine("  /testEnv    -   Environment name of PX that is being tested. Values can be PPE or PROD");
            stringBuilder.AppendLine("  /account    -   msa account");
            stringBuilder.AppendLine("  /pwd    -   msa account password");
            stringBuilder.AppendLine("  /cc    -   credit card number used for testing");
            stringBuilder.AppendLine("  /cvv    -   credit card cvv number");
            stringBuilder.AppendLine("--------------------------------------------------------------------------------------------------------------");

            Console.WriteLine(stringBuilder.ToString());
        }

        private async Task GenerateAccoutCvvToken()
        {
            using (HttpClient client = new HttpClient())
            {
                HttpRequestMessage accountRequestMessage = new HttpRequestMessage(HttpMethod.Post, "https://tokenization.cp.microsoft.com/tokens/pan/getToken");
                HttpRequestMessage cvvRequestMessage = new HttpRequestMessage(HttpMethod.Post, "https://tokenization.cp.microsoft.com/tokens/cvv/getToken");

                accountRequestMessage.Content = new StringContent("{ \"data\": \"" + this.cardNum + "\" }", Encoding.UTF8, "application/json");
                cvvRequestMessage.Content = new StringContent("{ \"data\": \"" + this.cvvNum + "\" }", Encoding.UTF8, "application/json");

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; // lgtm[cs/hard-coded-security-protocol] -Suppressing because of a false positive from Semmle // DevSkim: ignore DS440000,DS440020 as old protocols are being explicitly removed

                HttpResponseMessage accountResponse = await client.SendAsync(accountRequestMessage);
                HttpResponseMessage cvvResponse = await client.SendAsync(cvvRequestMessage);

                if (string.Compare(HttpStatusCode.OK.ToString(), accountResponse.StatusCode.ToString()) != 0 || string.Compare(HttpStatusCode.OK.ToString(), cvvResponse.StatusCode.ToString()) != 0)
                {
                    Console.WriteLine("generate panToken or cvvToken failed.");
                }

                JToken account = JToken.Parse(await accountResponse.Content.ReadAsStringAsync());
                JToken cvv = JToken.Parse(await cvvResponse.Content.ReadAsStringAsync());

                this.panToken = (string)account["data"];
                this.cvvToken = (string)cvv["data"];
            }
        }
    }
}
