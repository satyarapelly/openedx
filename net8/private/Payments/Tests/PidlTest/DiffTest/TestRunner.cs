// <copyright file="TestRunner.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace PidlTest.Diff
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using JsonDiff;
    using Microsoft.Commerce.Payments.PXService.ApiSurface.Diff;
    using Microsoft.VisualBasic.FileIO;
    using Newtonsoft.Json.Linq;

    internal class TestRunner : ITestRunner
    {
        #region Constants
        // Command line arguments supported by this test
        private const string ArgBaseUrl = "/BaseUrl";
        private const string ArgTestUrl = "/TestUrl";
        private const string ArgBaseEnv = "/BaseEnv";
        private const string ArgTestEnv = "/TestEnv";
        private const string ArgOutputPath = "/OutputPath";
        private const string ArgBaseAcc = "/BaseAcc";
        private const string ArgTestAcc = "/TestAcc";
        private const string ArgTriagedFile = "/TriagedDiffFilePath";
        private const string TestScenarioName3ds = "px-service-3ds1-test-emulator,px.pims.3ds";
        private const string ArgBatchRun = "/BatchRun";

        // app.config references
        private const string UnderTestEnv = "UnderTestEnvironment";
        private const string BaselineEnv = "BaselineEnvironment";
        private const string OutputPath = "OutputPath";
        private const string TriagedFile = "TriagedDiffFilePath";

        // Number of sockets
        private const int MaxSockets = 100;

        // Keep-alive duration in milliseconds(15 Minutes)
        private const int KeepAliveDuration = 1500000;

        // Time interval (in ms) between printing
        private const int PrintInterval = 100;

        private const string RunTimespanFormat = @"hh\:mm\:ss";
        #endregion

        #region Member Variables

        private static HashSet<string[]> triagedDiffSet;

        // Instance of HttpClient used to send all requests for this test
        private readonly HttpClient pidlClientBaseline = new HttpClient();

        // Instance of HttpClient used to send all requests for this test
        private readonly HttpClient pidlClientUnderTest = new HttpClient();

        // The list of all tests
        private readonly List<Task<TestResult>> comparisonTasks = new List<Task<TestResult>>();

        private List<string> errorLog = new List<string>();

        private TestGenerator testScenarios;

        private TestGeneratorConfig testScenariosConfig;

        private ResultsWriter writer;

        // Base address of the server to run this test against
        private string baseUrl = string.Empty;
        private string testUrl = string.Empty;
        private string baseEnvironment = string.Empty;
        private string testEnvironment = string.Empty;
        private string baseAddressBaseline = string.Empty;
        private string localBaselineUser = string.Empty;
        private string localBaselineUserWithConsumerProfile = string.Empty;
        private string baseAddressUnderTest = string.Empty;
        private string localUnderTestUser = string.Empty;
        private string localUnderTestUserWithConsumerProfile = string.Empty;
        private string authTokenBaseline = string.Empty;
        private string authTokenUnderTest = string.Empty;
        private string knownDiffsFilePath = string.Empty;
        private string outputPath = string.Empty;
        private string triagedFile = string.Empty;
        private KnownDiffsConfig diffConfig;
        private bool runInBatches = false;

        // display related variables
        private Stages stage = Stages.Setup;
        private int width;
        private Stopwatch runtimeStopWatch = new Stopwatch();

        private int symbolIndex = 0;
        private string[] symbols = new string[] { "\\", "|", "/", "-" };
        #endregion

        private enum Stages
        {
            Setup, Running, Complete
        }

        public static HashSet<string[]> TriagedDiffSet
        {
            get { return triagedDiffSet; }
        }

        /// <summary>
        /// DiffTest prerun configuration.
        /// Collects required test data from command line arguments and config file
        /// parsable options [/BaseEnv (INT, PPE, PROD) /BaseAcc (email) (password) /TestEnv (INT, PPE, PROD, Local) /TestAcc (email) (password) /OutputPath (output path) /help /?]
        /// </summary>
        /// <param name="args">Arguments for configuration</param>
        /// <returns>the success state of applying configurations</returns>
        public bool ParseArguments(string[] args)
        {
            string baselineUsername = string.Empty;
            string baselinePassword = string.Empty;
            string underTestUsername = string.Empty;
            string underTestPassword = string.Empty;

            for (int i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], ArgBaseEnv, StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 >= args.Length)
                    {
                        Console.WriteLine(Constants.ErrorMessages.ArgValueMissingFormat, ArgBaseEnv);
                        return false;
                    }
                    else
                    {
                        this.baseEnvironment = args[i + 1];
                    }
                }
                else if (string.Equals(args[i], ArgTestEnv, StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 >= args.Length)
                    {
                        Console.WriteLine(Constants.ErrorMessages.ArgValueMissingFormat, ArgTestEnv);
                        return false;
                    }
                    else
                    {
                        this.testEnvironment = args[i + 1];
                    }
                }
                else if (string.Equals(args[i], ArgOutputPath, StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 >= args.Length)
                    {
                        Console.WriteLine(Constants.ErrorMessages.ArgValueMissingFormat, ArgOutputPath);
                        return false;
                    }
                    else
                    {
                        this.outputPath = args[i + 1];
                    }
                }
                else if (string.Equals(args[i], ArgBaseAcc, StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 2 >= args.Length)
                    {
                        Console.WriteLine(Constants.ErrorMessages.ArgValueMissingFormat, ArgBaseAcc);
                        return false;
                    }
                    else
                    {
                        baselineUsername = args[i + 1];
                        baselinePassword = args[i + 2];
                    }
                }
                else if (string.Equals(args[i], ArgTestAcc, StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 2 >= args.Length)
                    {
                        Console.WriteLine(Constants.ErrorMessages.ArgValueMissingFormat, ArgTestAcc);
                        return false;
                    }
                    else
                    {
                        underTestUsername = args[i + 1];
                        underTestPassword = args[i + 2];
                    }
                }
                else if (string.Equals(args[i], ArgTriagedFile, StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 >= args.Length)
                    {
                        Console.WriteLine(Constants.ErrorMessages.ArgValueMissingFormat, ArgTestAcc);
                        return false;
                    }
                    else
                    {
                        this.triagedFile = args[i + 1];
                    }
                }
                else if (string.Equals(args[i], ArgBaseUrl, StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 >= args.Length)
                    {
                        Console.WriteLine(Constants.ErrorMessages.ArgValueMissingFormat, ArgBaseUrl);
                        return false;
                    }
                    else
                    {
                        this.baseUrl = args[i + 1];
                    }
                }
                else if (string.Equals(args[i], ArgTestUrl, StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 >= args.Length)
                    {
                        Console.WriteLine(Constants.ErrorMessages.ArgValueMissingFormat, ArgTestUrl);
                        return false;
                    }
                    else
                    {
                        this.testUrl = args[i + 1];
                    }
                }
                else if (string.Equals(args[i], ArgBatchRun, StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 >= args.Length)
                    {
                        Console.WriteLine(Constants.ErrorMessages.ArgValueMissingFormat, ArgBatchRun);
                        return false;
                    }
                    else
                    {
                        bool.TryParse(args[i + 1], out this.runInBatches);
                    }
                }
                else if (string.Compare(args[i], "/?", true) == 0 || string.Compare(args[i], "/help", true) == 0)
                {
                    ShowHelp();
                    return false;
                }
            }

            if (string.IsNullOrEmpty(this.baseEnvironment))
            {
                if (!string.IsNullOrEmpty(AppConfig.Configuration[BaselineEnv]))
                {
                    this.baseEnvironment = AppConfig.Configuration[BaselineEnv];
                }
                else
                {
                    Console.WriteLine(Constants.ErrorMessages.ConfigValueMissing, BaselineEnv);
                    return false;
                }
            }

            if (string.IsNullOrEmpty(this.testEnvironment))
            {
                if (!string.IsNullOrEmpty(AppConfig.Configuration[UnderTestEnv]))
                {
                    this.testEnvironment = AppConfig.Configuration[UnderTestEnv];
                }
                else
                {
                    Console.WriteLine(Constants.ErrorMessages.ConfigValueMissing, UnderTestEnv);
                    return false;
                }
            }

            if (string.IsNullOrEmpty(this.outputPath))
            {
                if (!string.IsNullOrEmpty(AppConfig.Configuration[OutputPath]))
                {
                    this.outputPath = AppConfig.Configuration[OutputPath];
                }
                else
                {
                    Console.WriteLine(Constants.ErrorMessages.ConfigValueMissing, OutputPath);
                    return false;
                }
            }

            if (string.IsNullOrEmpty(this.triagedFile))
            {
                if (!string.IsNullOrEmpty(AppConfig.Configuration[TriagedFile]))
                {
                    this.triagedFile = AppConfig.Configuration[TriagedFile];
                }
            }

            try
            {
                this.testScenariosConfig = new TestGeneratorConfig();
                this.testScenariosConfig.RunDiffTestsForPSSFeatures = Convert.ToBoolean(AppConfig.Configuration["Run_DiffTestsForPSSFeatures"]);
                this.testScenariosConfig.AddressDescription = Convert.ToBoolean(AppConfig.Configuration["Run_AddressDescription"]);
                this.testScenariosConfig.BillingGroupDescription = Convert.ToBoolean(AppConfig.Configuration["Run_BillingGroupDescription"]);
                this.testScenariosConfig.ProfileDescriptionWithEmulator = Convert.ToBoolean(AppConfig.Configuration["Run_ProfileDescriptionWithEmulator"]);
                this.testScenariosConfig.ProfileDescriptionWithoutEmulator = Convert.ToBoolean(AppConfig.Configuration["Run_ProfileDescriptionWithoutEmulator"]);
                this.testScenariosConfig.ChallengeDescription = Convert.ToBoolean(AppConfig.Configuration["Run_ChallengeDescription"]);
                this.testScenariosConfig.CheckoutDescriptions = Convert.ToBoolean(AppConfig.Configuration["Run_CheckoutDescriptions"]);
                this.testScenariosConfig.TaxIdDescription = Convert.ToBoolean(AppConfig.Configuration["Run_TaxIdDescription"]);
                this.testScenariosConfig.PaymentMethodDescription = Convert.ToBoolean(AppConfig.Configuration["Run_PaymentMethodDescription"]);
                this.testScenariosConfig.RewardsDescriptions = Convert.ToBoolean(AppConfig.Configuration["Run_RewardsDescriptions"]);
                this.testScenariosConfig.PaymentInstrumentEx = Convert.ToBoolean(AppConfig.Configuration["Run_PaymentInstrumentEx"]);
            }
            catch
            {
                Console.WriteLine("\"Run_\" entries in config file must be convertable to type boolean");
                return false;
            }

            this.knownDiffsFilePath = string.Format(".\\DiffTest\\ConfigFiles\\KnownDiffs\\Test_{0}_AgainstBaseline_{1}.csv", this.testEnvironment.ToUpper(), this.baseEnvironment.ToUpper());

            const string INT = "int";
            const string PPE = "ppe";
            const string Prod = "prod";
            const string Feature = "feature";
            const string Local = "local";

            if (this.baseEnvironment.Equals(INT, StringComparison.InvariantCultureIgnoreCase))
            {
                this.baseAddressBaseline = Constants.Environment.INT;

                if (string.IsNullOrEmpty(baselineUsername) || string.IsNullOrEmpty(baselinePassword))
                {
                    baselineUsername = AppConfig.Configuration["INT_Email"];
                    baselinePassword = AppConfig.Configuration["INT_Password"];
                }
            }
            else if (this.baseEnvironment.Equals(PPE, StringComparison.InvariantCultureIgnoreCase))
            {
                this.baseAddressBaseline = Constants.Environment.PPE;

                if (string.IsNullOrEmpty(baselineUsername) || string.IsNullOrEmpty(baselinePassword))
                {
                    baselineUsername = AppConfig.Configuration["PPE_Email"];
                    baselinePassword = AppConfig.Configuration["PPE_Password"];
                }
            }
            else if (this.baseEnvironment.Equals(Prod, StringComparison.InvariantCultureIgnoreCase)
                     || this.baseEnvironment.Equals(Feature, StringComparison.InvariantCultureIgnoreCase))
            {
                this.baseAddressBaseline = this.baseEnvironment.Equals(Prod, StringComparison.InvariantCultureIgnoreCase) ? Constants.Environment.Prod : Constants.Environment.Feature;

                if (string.IsNullOrEmpty(baselineUsername) || string.IsNullOrEmpty(baselinePassword))
                {
                    baselineUsername = AppConfig.Configuration["PROD_Email"];
                    baselinePassword = AppConfig.Configuration["PROD_Password"];
                }
            }
            else if (this.baseEnvironment.Equals(Local, StringComparison.InvariantCultureIgnoreCase))
            {
                this.baseAddressBaseline = AppConfig.Configuration["LocalBaseline"];
                this.localBaselineUser = AppConfig.Configuration["LocalBaselineUser"];
                this.localBaselineUserWithConsumerProfile = AppConfig.Configuration["LocalBaselineUserWithConsumerProfile"];

                if (string.IsNullOrEmpty(baselineUsername) || string.IsNullOrEmpty(baselinePassword))
                {
                    baselineUsername = AppConfig.Configuration["PROD_Email"];
                    baselinePassword = AppConfig.Configuration["PROD_Password"];
                }
            }
            else if (this.baseEnvironment.Equals(Constants.Environment.SelfHost, StringComparison.InvariantCultureIgnoreCase))
            {
                if (string.IsNullOrEmpty(this.baseUrl))
                {
                    Console.WriteLine(Constants.ErrorMessages.ArgValueMissingFormat, ArgBaseUrl);
                    return false;
                }

                this.baseAddressBaseline = this.baseUrl;
            }
            else
            {
                Console.WriteLine("Baseline environment {0} is not INT, PPE, PROD, LOCAL SELFHOST", this.baseEnvironment);
                return false;
            }

            if (this.testEnvironment.Equals(INT, StringComparison.InvariantCultureIgnoreCase))
            {
                this.baseAddressUnderTest = Constants.Environment.INT;

                if (string.IsNullOrEmpty(underTestUsername) || string.IsNullOrEmpty(underTestPassword))
                {
                    underTestUsername = AppConfig.Configuration["INT_Email"];
                    underTestPassword = AppConfig.Configuration["INT_Password"];
                }
            }
            else if (this.testEnvironment.Equals(PPE, StringComparison.InvariantCultureIgnoreCase))
            {
                this.baseAddressUnderTest = Constants.Environment.PPE;

                if (string.IsNullOrEmpty(underTestUsername) || string.IsNullOrEmpty(underTestPassword))
                {
                    underTestUsername = AppConfig.Configuration["PPE_Email"];
                    underTestPassword = AppConfig.Configuration["PPE_Password"];
                }
            }
            else if (this.testEnvironment.Equals(Prod, StringComparison.InvariantCultureIgnoreCase)
                || this.testEnvironment.Equals(Feature, StringComparison.InvariantCultureIgnoreCase))
            {
                this.baseAddressUnderTest = this.testEnvironment.Equals(Prod, StringComparison.InvariantCultureIgnoreCase) ? Constants.Environment.Prod : Constants.Environment.Feature;

                if (string.IsNullOrEmpty(underTestUsername) || string.IsNullOrEmpty(underTestPassword))
                {
                    underTestUsername = AppConfig.Configuration["PROD_Email"];
                    underTestPassword = AppConfig.Configuration["PROD_Password"];
                }
            }
            else if (this.testEnvironment.Equals(Local, StringComparison.InvariantCultureIgnoreCase))
            {
                this.baseAddressUnderTest = AppConfig.Configuration["LocalUnderTest"];
                this.localUnderTestUser = AppConfig.Configuration["LocalUnderTestUser"];
                this.localUnderTestUserWithConsumerProfile = AppConfig.Configuration["LocalUnderTestUserWithConsumerProfile"];

                if (string.IsNullOrEmpty(underTestUsername) || string.IsNullOrEmpty(underTestPassword))
                {
                    underTestUsername = AppConfig.Configuration["PROD_Email"];
                    underTestPassword = AppConfig.Configuration["PROD_Password"];
                }
            }
            else if (this.testEnvironment.Equals(Constants.Environment.SelfHost, StringComparison.InvariantCultureIgnoreCase))
            {
                if (string.IsNullOrEmpty(this.testUrl))
                {
                    Console.WriteLine(Constants.ErrorMessages.ArgValueMissingFormat, ArgTestUrl);
                    return false;
                }

                this.baseAddressUnderTest = this.testUrl;
            }
            else
            {
                Console.WriteLine("Undertest environment {0} is not INT, PPE, PROD, LOCAL", this.baseAddressUnderTest);
                return false;
            }

            const string TokenWrapper = "WLID1.0=\"{0}\"";

            Console.Write("Generate baseline token...");
            try
            {
                if (!this.baseEnvironment.Equals(Local, StringComparison.InvariantCultureIgnoreCase) && !this.baseEnvironment.Equals(Constants.Environment.SelfHost, StringComparison.InvariantCultureIgnoreCase))
                {
                    var token = Generator.GenerateAsync(this.baseEnvironment.ToLower(), baselineUsername, baselinePassword).Result;
                    this.authTokenBaseline = string.Format(TokenWrapper, token);
                    Console.WriteLine("DONE");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to generate baseline token");
                Console.WriteLine(e.Message);
                Console.Write(e.StackTrace);
                return false;
            }

            Console.Write("Generate undertest token...");
            try
            {
                if (!this.testEnvironment.Equals(Local, StringComparison.InvariantCultureIgnoreCase) && !this.testEnvironment.Equals(Constants.Environment.SelfHost, StringComparison.InvariantCultureIgnoreCase))
                {
                    var tokenList = Generator.GenerateAsync(this.testEnvironment.ToLower(), underTestUsername, underTestPassword).Result;
                    this.authTokenUnderTest = string.Format(TokenWrapper, tokenList);
                }

                Console.WriteLine("DONE");
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to generate undertest token");
                Console.WriteLine(e.Message);
                Console.Write(e.StackTrace);
                this.stage = Stages.Complete;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Initializes environment variables and starts testing
        /// </summary>
        /// <returns>The testing task</returns>
        public async Task<int> StartTestAsync()
        {
            this.runtimeStopWatch.Start();
            ServicePointManager.DefaultConnectionLimit = MaxSockets;
            this.pidlClientBaseline.Timeout = TimeSpan.FromMilliseconds(KeepAliveDuration);
            this.pidlClientUnderTest.Timeout = TimeSpan.FromMilliseconds(KeepAliveDuration);
            this.pidlClientBaseline.BaseAddress = new Uri(this.baseAddressBaseline);
            this.pidlClientUnderTest.BaseAddress = new Uri(this.baseAddressUnderTest);
            this.testScenarios = new TestGenerator(config: this.testScenariosConfig);
            this.writer = new ResultsWriter(this.outputPath, this.baseEnvironment, this.testEnvironment);

            if (!HostingUtility.IsSelfHostRun(this.baseEnvironment, this.testEnvironment))
            {
                Task display = Task.Run(() => this.PrintResultsAsync());
            }

            this.testScenarios.GenerateTestSet();
            this.stage = Stages.Running;

            if (!string.IsNullOrEmpty(this.knownDiffsFilePath))
            {
                this.diffConfig = new KnownDiffsConfig(this.knownDiffsFilePath);

                try
                {
                    this.diffConfig.Initialize();
                }
                catch (PidlConfigException configException)
                {
                    this.errorLog.Add("Diff Config file failed to be parsed");
                    this.errorLog.Add(string.Format("Exception Detail : {0}", configException.Message));
                    this.stage = Stages.Complete;
                    return 2;
                }
                catch (Exception ex)
                {
                    this.errorLog.Add("Diff Config file failed to be parsed");
                    this.errorLog.Add(string.Format("Exception Detail : {0}", ex.Message));
                    this.stage = Stages.Complete;
                    return 3;
                }
            }

            if (!string.IsNullOrEmpty(this.triagedFile))
            {
                triagedDiffSet = LoadTriagedDiff(this.triagedFile);
            }

            List<TestResult> testResults;

            // Populates the test queue with static pidls and Add PI tests
            if (HostingUtility.IsSelfHostRun(this.baseEnvironment, this.testEnvironment) || this.runInBatches)
            {
                //// SelfHostConfiguration uses "100 times the number of CPU cores" as the max value for "MaxConcurrentRequests".
                var batchSize = Environment.ProcessorCount * (HostingUtility.IsPipelineRun() ? 80 : 90);

                Console.WriteLine("Starting executing difftest.");

                await this.ExecuteTestsInBatchesAsync(
                    this.testScenarios.Set,
                    batchSize,
                    (percentageDone, total, failed) =>
                    {
                        if (!this.runInBatches)
                        {
                            Console.WriteLine($"{percentageDone}% complete. {failed}/{total} failed.");
                        }
                    });

                Console.WriteLine("Finished executing difftest.");
                testResults = this.comparisonTasks.Select(x => x.Result).ToList();
            }
            else
            {
                foreach (Test criteria in this.testScenarios.Set)
                {
                    this.ExecuteTest(criteria);
                }

                await Task.WhenAll(this.comparisonTasks);
                testResults = this.comparisonTasks.Select(x => x.Result).ToList();
            }

            this.stage = Stages.Complete;
            this.runtimeStopWatch.Stop();

            var failedComparisons = testResults.Where(x => !x.IsComparisonSuccess).ToList();
            var successComparisons = testResults.Where(x => x.IsComparisonSuccess).ToList();

            if (HostingUtility.IsSelfHostRun(this.baseEnvironment, this.testEnvironment))
            {
                if (failedComparisons.Any())
                {
                    foreach (var failedComparison in failedComparisons)
                    {
                        Console.WriteLine($"ERROR: {failedComparison.Url}{Environment.NewLine}");
                    }
                }

                Console.Write(
                    this.PrintResults(
                        testResults.Count,
                        successComparisons.Count,
                        failedComparisons.Count));
            }

            if (failedComparisons.Any())
            {
                return 1;
            }

            return 0;
        }

        public async Task StopTestAsync()
        {
            await Task.FromResult<object>(null);
        }

        /// <summary>
        /// Returns true if the diff tests are being run with local environment as baseline
        /// </summary>
        /// <returns>true / false depending on the diff tests being run with local environment as baseline</returns>
        public bool IsBaseEnvironmentLocal()
        {
            return this.baseEnvironment.Equals("Local", StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Returns true if the diff tests are being run with local environment as under test
        /// </summary>
        /// <returns>true / false depending on the diff tests being run with local environment as under test</returns>
        public bool IsTestEnvironmentLocal()
        {
            return this.testEnvironment.Equals("Local", StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Write help details to the console
        /// </summary>
        private static void ShowHelp()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("--------------------------------------------------------------------------------------------------------------");
            stringBuilder.AppendLine("PIDLTest.exe /testType DiffTest /baseEnv <INT, PPE, PROD> /testEnv <INT, PPE, PROD, Local> /outputPath <output path> /baseAcc <email> <password> /testAcc <email> <password>");
            stringBuilder.AppendLine(string.Empty);
            stringBuilder.AppendLine("  /baseEnv    - Environment name of PX that forms the baseline to diff against.  Values can be INT, PPE or PROD");
            stringBuilder.AppendLine("  /testEnv    - Environment name of PX that is being tested. Values can be Local, INT, PPE or PROD");
            stringBuilder.AppendLine("  /outputPath - Path where the output should be written to");
            stringBuilder.AppendLine("  /baseAcc    - Email and password of account that forms the baseline to diff against. Ensure the account matches the specified base environment");
            stringBuilder.AppendLine("  /testAcc    - Email and password of account that is being tested. Ensure the account matches the specified test environment");
            stringBuilder.AppendLine("  /triagedDiffFilePath    - Check and copy the comments string of triaged diff results when generate new diff result file");
            stringBuilder.AppendLine("--------------------------------------------------------------------------------------------------------------");

            Console.WriteLine(stringBuilder.ToString());
        }

        /// <summary>
        /// Read and load all the triaged diff files which already has comments string to a HashSet
        /// </summary>
        /// <param name="triagedDiffFileFullpath">The path of diff result csv file</param>
        /// <returns>Return a HashSet contains all the triaged diff results</returns>
        private static HashSet<string[]> LoadTriagedDiff(string triagedDiffFileFullpath)
        {
            string triagedDiffPath = Path.GetDirectoryName(triagedDiffFileFullpath);
            string triagedDiffFileName = Path.GetFileNameWithoutExtension(triagedDiffFileFullpath);
            string searchFilter = triagedDiffFileName + "*" + ".csv";

            string[] triagedDiffFiles = Directory.Exists(triagedDiffPath) ? Directory.GetFiles(triagedDiffPath, searchFilter, System.IO.SearchOption.TopDirectoryOnly) : new string[0];

            if (triagedDiffFiles.Length == 0)
            {
                // Check for full path
                triagedDiffPath = Directory.GetCurrentDirectory() + triagedDiffPath;
                triagedDiffFiles = Directory.Exists(triagedDiffPath) ? Directory.GetFiles(triagedDiffPath, searchFilter, System.IO.SearchOption.TopDirectoryOnly) : new string[0];

                if (triagedDiffFiles.Length == 0)
                {
                    Console.WriteLine(string.Format("Triaged diff files on path {0} does not exists.", triagedDiffPath));
                    return null;
                }
            }

            triagedDiffSet = new HashSet<string[]>();
            string comments = string.Empty;

            try
            {
                foreach (string triagedDiffFile in triagedDiffFiles)
                {
                    using (TextFieldParser parser = new TextFieldParser(triagedDiffFile))
                    {
                        parser.TextFieldType = FieldType.Delimited;
                        parser.SetDelimiters(",");
                        parser.HasFieldsEnclosedInQuotes = true;

                        parser.ReadLine();

                        while (parser.PeekChars(1) != null)
                        {
                            try
                            {
                                string[] rowCells = parser.ReadFields();

                                // When testname is provided then csv file contains url as (test name)url
                                // So, only read the url and skip test name for triaged diffs
                                rowCells[0] = rowCells[0].Split(')').LastOrDefault();
                                triagedDiffSet.Add(rowCells);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return triagedDiffSet;
        }

        private static void AddHeaders(HttpRequestMessage request, Dictionary<string, string> headers)
        {
            if (headers != null && request != null)
            {
                foreach (var headerName in headers.Keys)
                {
                    string existingHeaderValue = GetRequestHeader(request, headerName);
                    string additionalHeaderValue = headers[headerName];

                    if (string.Equals(headerName, "x-ms-flight", StringComparison.OrdinalIgnoreCase))
                    {
                        // Adding duplicate HTTP headers is concatinating values with a comma and a space.  Downstream PIMS service is not parsing 
                        // the additonal space to identify the flights.
                        string newFlightValue = GetNewFlightValue(existingHeaderValue, additionalHeaderValue);

                        if (!string.IsNullOrWhiteSpace(newFlightValue))
                        {
                            request.Headers.Remove(headerName);
                            request.Headers.Add(headerName, newFlightValue);
                        }
                    }
                    else if (string.Equals(headerName, "x-ms-test", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!string.IsNullOrWhiteSpace(existingHeaderValue) &&
                            !string.Equals(existingHeaderValue, additionalHeaderValue, StringComparison.OrdinalIgnoreCase))
                        {
                            // Concatenate the scenarios values from the existing and additional headers
                            var concatenatedScenarios = $"{JObject.Parse(existingHeaderValue)["scenarios"].ToString()},{JObject.Parse(additionalHeaderValue)["scenarios"].ToString()}";

                            // Remove the existing header and add the new concatenated value
                            request.Headers.Remove(headerName);
                            request.Headers.Add(headerName, string.Format("{{scenarios: \"{0}\", contact: \"{1}\"}}", concatenatedScenarios, "DiffTest"));
                        }
                        else
                        {
                            // this else case is to handle the scenario where the header is not present in the request.
                            request.Headers.Add(headerName, headers[headerName]);
                        }
                    }
                    else
                    {
                        request.Headers.Add(headerName, headers[headerName]);
                    }
                }
            }
        }

        private static string GetNewFlightValue(string existingFlightValue, string additionalFlightValue)
        {
            string newFlightValue = null;

            if (string.IsNullOrWhiteSpace(existingFlightValue))
            {
                if (!string.IsNullOrWhiteSpace(additionalFlightValue))
                {
                    newFlightValue = additionalFlightValue;
                }
            }
            else if (string.IsNullOrWhiteSpace(additionalFlightValue))
            {
                newFlightValue = existingFlightValue;
            }
            else
            {
                newFlightValue = string.Join(",", existingFlightValue, additionalFlightValue);
            }

            return newFlightValue;
        }

        private static string GetRequestHeader(HttpRequestMessage request, string headerName)
        {
            IEnumerable<string> headerValues;
            if (request.Headers.TryGetValues(headerName, out headerValues))
            {
                return headerValues.FirstOrDefault();
            }

            return null;
        }

        /// <summary>
        /// Runs a new test and adds it to the collection
        /// </summary>
        /// <param name="criteria">All required data for running a new test</param>
        private void ExecuteTest(Test criteria)
        {
            lock (this.comparisonTasks)
            {
                this.comparisonTasks.Add(Task.Run(() => this.GetDiffTestRequestResponseAsync(criteria)));
            }
        }

        /// <summary>
        /// Constructs and sends PXService request for both the baseline and undertest
        /// Identifies and creates new tests for multistep test scenarios
        /// </summary>
        /// <param name="criteria">All required data for running a new test</param>
        /// <returns>the results of the test on completion</returns>
        private async Task<TestResult> GetDiffTestRequestResponseAsync(Test criteria)
        {
            string baseLinePath = string.Empty;
            string underTestPath = string.Empty;
            HttpStatusCode code = criteria.GetStatusCode();
            HttpMethod method = criteria.GetHttpMethod();

            baseLinePath = criteria.Path.ToString(this.IsBaseEnvironmentLocal(), criteria.State, criteria.PIID);
            underTestPath = criteria.Path.ToString(this.IsTestEnvironmentLocal(), criteria.State, criteria.PIID);

            if (criteria.Path.UserType != Constants.UserTypes.Anonymous)
            {
                if (this.IsBaseEnvironmentLocal())
                {
                    // AddressDescriptions for OneDrive needs a consumer profile to be present
                    if (string.Equals(criteria.Path.Partner, Constants.PartnerNames.OneDrive, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(criteria.Path.ResourceName, "addressDescriptions", StringComparison.OrdinalIgnoreCase)
                        && string.Equals(criteria.Path.PIType, "billing", StringComparison.OrdinalIgnoreCase))
                    {
                        baseLinePath = this.localBaselineUserWithConsumerProfile + "/" + baseLinePath;
                    }
                    else
                    {
                        baseLinePath = this.localBaselineUser + "/" + baseLinePath;
                    }
                }

                if (this.IsTestEnvironmentLocal())
                {
                    // AddressDescriptions for OneDrive needs a consumer profile to be present
                    if (string.Equals(criteria.Path.Partner, Constants.PartnerNames.OneDrive, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(criteria.Path.ResourceName, "addressDescriptions", StringComparison.OrdinalIgnoreCase)
                        && string.Equals(criteria.Path.PIType, "billing", StringComparison.OrdinalIgnoreCase))
                    {
                        underTestPath = this.localUnderTestUserWithConsumerProfile + "/" + underTestPath;
                    }
                    else
                    {
                        underTestPath = this.localUnderTestUser + "/" + underTestPath;
                    }
                }
            }

            if (!this.IsBaseEnvironmentLocal() && !this.IsTestEnvironmentLocal())
            {
                if (string.Equals(criteria.Path.ResourceName, Constants.ResourceName.ChallengeDescriptions, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(criteria.Path.Operation, Constants.Operation.RenderPidlPage, StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrEmpty(criteria.Path.PaymentSessionOrData)
                    && !string.Equals(criteria.TestScenarioName, TestScenarioName3ds, StringComparison.OrdinalIgnoreCase))
                {
                    return new TestResult(true, underTestPath);
                }
            }

            if (HostingUtility.IsSelfHostRun(this.baseEnvironment, this.testEnvironment))
            {
                if (underTestPath.Contains("completePrerequisites=true"))
                {
                    underTestPath = underTestPath.Replace("users/me", "EmpAccountNoAddress");
                    baseLinePath = baseLinePath.Replace("users/me", "EmpAccountNoAddress");
                }
                else
                {
                    underTestPath = underTestPath.Replace("users/me", "DiffTestUser");
                    baseLinePath = baseLinePath.Replace("users/me", "DiffTestUser");
                }

                underTestPath = underTestPath.Replace("users/my-org", "DiffOrgUser");
                baseLinePath = baseLinePath.Replace("users/my-org", "DiffOrgUser");
            }

            TestRun comparisonDetails;
            HttpRequestMessage requestMessageBaseline = new HttpRequestMessage(method, this.pidlClientBaseline.BaseAddress + baseLinePath);
            HttpRequestMessage requestMessageUnderTest = new HttpRequestMessage(method, this.pidlClientUnderTest.BaseAddress + underTestPath);

            if (criteria.Path.UserType == Constants.UserTypes.Anonymous)
            {
                if (!this.IsBaseEnvironmentLocal())
                {
                    requestMessageBaseline.Headers.Add("x-ms-flight", "pxpidl");
                }

                if (!this.IsTestEnvironmentLocal())
                {
                    requestMessageUnderTest.Headers.Add("x-ms-flight", "pxpidl");
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(this.authTokenBaseline))
                {
                    requestMessageBaseline.Headers.TryAddWithoutValidation("Authorization", this.authTokenBaseline);
                }

                if (!string.IsNullOrEmpty(this.authTokenUnderTest))
                {
                    requestMessageUnderTest.Headers.TryAddWithoutValidation("Authorization", this.authTokenUnderTest);
                }
            }

            if (criteria.TestScenarioName != null && criteria.TestScenarioName.ToLower().Contains("px-service-3ds1-test-emulator,px.pims.3ds"))
            {
                requestMessageBaseline.Headers.Add("x-ms-flight", "EnableThreeDSOne");

                requestMessageUnderTest.Headers.Add("x-ms-flight", "EnableThreeDSOne");
            }
            else if (string.Equals(criteria.Path.PIFamily, Constants.PaymentMethodFamilyNames.MobileBillingNonSim, StringComparison.OrdinalIgnoreCase))
            {
                requestMessageBaseline.Headers.Add("x-ms-flight", "vNext");

                requestMessageUnderTest.Headers.Add("x-ms-flight", "vNext");
            }

            requestMessageBaseline = this.UpdateXMsCustomerHeaderInRequest(criteria, requestMessageBaseline);
            requestMessageUnderTest = this.UpdateXMsCustomerHeaderInRequest(criteria, requestMessageUnderTest);

            if (criteria.Content != null)
            {
                if (!string.Equals(criteria.Content.Name, "px.tops.csvtoken.success", StringComparison.CurrentCultureIgnoreCase))
                {
                    requestMessageBaseline.Headers.Add("x-ms-test", string.Format("{{scenarios: \"{0}\", contact: \"{1}\"}}", criteria.Content.Name, "DiffTest"));
                    requestMessageUnderTest.Headers.Add("x-ms-test", string.Format("{{scenarios: \"{0}\", contact: \"{1}\"}}", criteria.Content.Name, "DiffTest"));
                }

                if (method == HttpMethod.Post)
                {
                    requestMessageBaseline.Content = new StringContent(criteria.Content.Body, Encoding.UTF8, "application/json");
                    requestMessageUnderTest.Content = new StringContent(criteria.Content.Body, Encoding.UTF8, "application/json");
                }
            }

            if (criteria.AdditionalHeaders != null)
            {
                AddHeaders(requestMessageBaseline, criteria.AdditionalHeaders);
                AddHeaders(requestMessageUnderTest, criteria.AdditionalHeaders);
            }

            if (criteria.TestScenarioName != null && (criteria.AdditionalHeaders == null || !criteria.AdditionalHeaders.ContainsKey("x-ms-test")))
            {
                requestMessageBaseline.Headers.Add("x-ms-test", string.Format("{{scenarios: \"{0}\", contact: \"{1}\"}}", criteria.TestScenarioName, "DiffTest"));
                requestMessageUnderTest.Headers.Add("x-ms-test", string.Format("{{scenarios: \"{0}\", contact: \"{1}\"}}", criteria.TestScenarioName, "DiffTest"));
            }

            // Allows to use the PSS emulator PartnerSettingsByPartner.json mock for DiffTest
            requestMessageBaseline.Headers.Add("x-ms-flight", "PXUsePSSPartnerMockForDiffTest");
            requestMessageUnderTest.Headers.Add("x-ms-flight", "PXUsePSSPartnerMockForDiffTest");

            HttpResponseMessage baselineResponse = null;
            HttpResponseMessage underTestResponse = null;
            string baselineResponseContent = null;
            string undertestResponseContent = null;

            try
            {
                baselineResponse = await this.SendRequestAsyncWithRetry(this.pidlClientBaseline, requestMessageBaseline);
                underTestResponse = await this.SendRequestAsyncWithRetry(this.pidlClientUnderTest, requestMessageUnderTest);

                baselineResponseContent = await baselineResponse?.Content?.ReadAsStringAsync();
                undertestResponseContent = await underTestResponse?.Content?.ReadAsStringAsync();

                comparisonDetails = new TestRun(
                    criteria,
                    baselineResponse,
                    underTestResponse,
                    string.IsNullOrEmpty(baselineResponseContent)
                        ? string.Empty
                        : JToken.Parse(baselineResponseContent),
                    string.IsNullOrEmpty(undertestResponseContent)
                        ? string.Empty
                        : JToken.Parse(undertestResponseContent));
            }
            catch (Exception ex)
            {
                comparisonDetails = new TestRun(criteria)
                {
                    FailedExecution = new DiffDetails()
                    {
                        Description = ex.Message,
                        Data = ex.StackTrace
                    }
                };
            }

            if (comparisonDetails.FailedExecution == null && baselineResponse != null && underTestResponse != null)
            {
                if (baselineResponse.StatusCode != code)
                {
                    comparisonDetails.FailedExecution = new DiffDetails()
                    {
                        Description = string.Format("Baseline Failed: {0}", baselineResponse.StatusCode),
                        Data = comparisonDetails.BaselineJson.ToString(Newtonsoft.Json.Formatting.None)
                    };
                }
                else if (underTestResponse.StatusCode != code)
                {
                    comparisonDetails.FailedExecution = new DiffDetails()
                    {
                        Description = string.Format("UnderTest Failed: {0}", underTestResponse.StatusCode),
                        Data = comparisonDetails.UnderTestJson.ToString(Newtonsoft.Json.Formatting.None)
                    };
                }
                else
                {
                    List<PIState> excludedStates = new List<PIState>()
                    {
                        PIState.None,
                        PIState.IssuerServiceApply,
                        PIState.IssuerServiceApplyEligibility,
                        PIState.IssuerServiceInitialize
                    };

                    // creates a new test if they exist
                    if (!excludedStates.Contains(criteria.State))
                    {
                        PIState childState = criteria.GetNextState();
                        if (childState != PIState.None)
                        {
                            string piid;
                            if (string.IsNullOrWhiteSpace(criteria.PIID))
                            {
                                IEnumerable<JToken> list = comparisonDetails.BaselineJson.SelectTokens("$..id");
                                piid = list.First().Value<string>();
                            }
                            else
                            {
                                piid = criteria.PIID;
                            }

                            this.ExecuteTest(new Test(childState, piid, criteria.Path, criteria.Content));
                        }
                    }

                    this.AssertPidlsAreEqual(ref comparisonDetails);
                }
            }

            this.writer.Write(comparisonDetails);
            return new TestResult(comparisonDetails.IsComparisonSuccess, underTestPath);
        }

        private HttpRequestMessage UpdateXMsCustomerHeaderInRequest(Test criteria, HttpRequestMessage requestMessage)
        {
            // UPI commercial PI is returned only for AAD customers call only because if x-ms-customer header is passed, it is responsible for identity resolution in the PIMS API call.
            // Since, diff tests using MSA credentials, removing the x-ms-customer header from the call.
            if (criteria.TestScenarioName != null && criteria.TestScenarioName.ToLower().Contains("px-get-upi-commercial-pi"))
            {
                requestMessage.Headers.Remove("x-ms-customer");
            }

            return requestMessage;
        }

        private async Task<HttpResponseMessage> SendRequestAsyncWithRetry(HttpClient httpClient, HttpRequestMessage httpRequestMessage)
        {
            HttpResponseMessage response = null;
            try
            {
                response = await httpClient.SendAsync(httpRequestMessage);
            }
            catch (Exception)
            {
                using (HttpRequestMessage newHttpRequestMessage = new HttpRequestMessage(httpRequestMessage.Method, httpRequestMessage.RequestUri))
                {
                    newHttpRequestMessage.Content = httpRequestMessage.Content;
                    newHttpRequestMessage.Version = httpRequestMessage.Version;

                    foreach (var header in httpRequestMessage.Headers)
                    {
                        if (string.Equals(header.Key, "Authorization"))
                        {
                            newHttpRequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
                        }
                        else
                        {
                            newHttpRequestMessage.Headers.Add(header.Key, header.Value);
                        }
                    }

                    response = await httpClient.SendAsync(newHttpRequestMessage);
                }
            }

            return response;
        }

        private async Task ExecuteTestsInBatchesAsync(List<Test> tests, int throttle, Action<int, int, int> reportProgress)
        {
            var progress = 0;
            var totalTests = tests.Count;

            while (progress < totalTests)
            {
                var testBatch = tests.Skip(progress).Take(throttle).ToList();
                var taskTestBatch = testBatch.Select(this.GetDiffTestRequestResponseAsync).ToList();

                await Task.WhenAll(taskTestBatch);

                var failedTest = 0;
                lock (this.comparisonTasks)
                {
                    this.comparisonTasks.AddRange(taskTestBatch);
                    failedTest = this.comparisonTasks.Count(x => x.IsCanceled || !x.Result.IsComparisonSuccess);
                }

                progress = progress + testBatch.Count;
                var percentageDone = decimal.Divide(progress, totalTests) * 100;

                reportProgress(decimal.ToInt32(percentageDone), totalTests, failedTest);
            }
        }

        /// <summary>
        /// Runs json comparison and applies known differences
        /// </summary>
        /// <param name="testRun">all required test elements</param>
        private void AssertPidlsAreEqual(ref TestRun testRun)
        {
            List<DiffDetails> foundDiffs = DiffFinder.GetPidlDiffs(testRun.BaselineJson, testRun.UnderTestJson);
            List<KnownDiffsDescription> knownDiffForTestSet = this.diffConfig.GetDiffConfig(testRun.Test.Path.GetPidlIdentity());

            if (foundDiffs.Count != 0 && knownDiffForTestSet.Count != 0)
            {
                foreach (KnownDiffsDescription knownDiff in knownDiffForTestSet)
                {
                    string[] effectedBaselinePaths = null;
                    string[] effectedUnderTestPaths = null;

                    DiffType delta = (DiffType)Enum.Parse(typeof(DiffType), knownDiff.DeltaType);

                    switch (delta)
                    {
                        case DiffType.add:
                            effectedUnderTestPaths = testRun.UnderTestJson.SelectTokens(knownDiff.NewJPath).Select(t => t.Path).ToArray();
                            break;
                        case DiffType.delete:
                            effectedBaselinePaths = testRun.BaselineJson.SelectTokens(knownDiff.BaselineJPath).Select(t => t.Path).ToArray();
                            break;
                        case DiffType.edit:
                            effectedBaselinePaths = testRun.BaselineJson.SelectTokens(knownDiff.BaselineJPath).Select(t => t.Path).ToArray();
                            break;
                        case DiffType.move:
                            effectedBaselinePaths = testRun.BaselineJson.SelectTokens(knownDiff.BaselineJPath).Select(t => t.Path).ToArray();
                            effectedUnderTestPaths = testRun.UnderTestJson.SelectTokens(knownDiff.NewJPath).Select(t => t.Path).ToArray();
                            break;
                    }

                    int errorIndex = 0;
                    DiffDetails error;
                    while (errorIndex < foundDiffs.Count)
                    {
                        error = foundDiffs[errorIndex];

                        if (delta == error.DiffType)
                        {
                            if ((delta == DiffType.add && effectedUnderTestPaths.Contains(error.JPath))
                                || (delta == DiffType.delete && effectedBaselinePaths.Contains(error.JPath))
                                || (delta == DiffType.edit && effectedBaselinePaths.Contains(error.JPath)
                                    && (knownDiff.NewValue == error.Actual || knownDiff.NewValue == Constants.DiffTest.Any))
                                || (delta == DiffType.move && effectedBaselinePaths.Contains(error.Expected) && effectedUnderTestPaths.Contains(error.Actual)))
                            {
                                foundDiffs.RemoveAt(errorIndex);
                            }
                        }

                        errorIndex++;
                    }
                }
            }

            testRun.UnexpectedDiffs.AddRange(foundDiffs);
        }

        /// <summary>
        /// Prints display on interval
        /// </summary>
        private void PrintResultsAsync()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Console.Clear();
            while (this.stage != Stages.Complete)
            {
                if (sw.ElapsedMilliseconds >= PrintInterval)
                {
                    this.PrintStatus();
                    sw.Restart();
                }
            }

            Task.Delay(1000);
            this.PrintStatus(true);
        }

        /// <summary>
        /// Write operation details to the console
        /// </summary>
        /// <param name="redraw">redraws the </param>
        private void PrintStatus(bool redraw = false)
        {
            if (this.stage == Stages.Setup)
            {
                Console.SetCursorPosition(0, 0);
                Console.Write("Generating tests... {0}", this.symbols[this.symbolIndex]);
                this.symbolIndex += (this.symbolIndex + 1 == this.symbols.Length) ? -this.symbolIndex : 1;
            }
            else
            {
                int passed = 0;
                int failed = 0;
                int completed = 0;
                int stopped = 0;
                int total = 0;
                TimeSpan averageTime;

                lock (this.comparisonTasks)
                {
                    foreach (Task<TestResult> task in this.comparisonTasks)
                    {
                        total++;
                        if (task.Status == TaskStatus.RanToCompletion)
                        {
                            completed++;
                            if (task.IsFaulted || task.IsCanceled)
                            {
                                stopped++;
                            }
                            else if (task.Result != null && task.Result.IsComparisonSuccess)
                            {
                                passed++;
                            }
                            else
                            {
                                failed++;
                            }
                        }
                    }
                }

                double doubleAverageTicks = (double)this.runtimeStopWatch.Elapsed.Ticks / ((completed > 0) ? (double)completed : 1d);
                averageTime = new TimeSpan(Convert.ToInt64(doubleAverageTicks));

                if (this.width != Console.WindowWidth - 1)
                {
                    this.width = Console.WindowWidth - 1;
                    Console.Clear();
                    redraw = true;
                }

                StringBuilder output = new StringBuilder();
                int batchSize = Environment.ProcessorCount * (HostingUtility.IsPipelineRun() ? 80 : 90);
                int totalBatchCount = (int)decimal.Ceiling(decimal.Divide(total > this.testScenarios.Set.Count ? total : this.testScenarios.Set.Count, batchSize));
                int completedBatchCount = (int)decimal.Ceiling(decimal.Divide(completed, batchSize));

                if (this.stage == Stages.Running || redraw)
                {
                    output.Append(this.DrawRunning(total, completed, passed, failed, stopped, averageTime, totalBatchCount, completedBatchCount));
                    output.AppendFormat("Current Runtime : {0}\n", this.runtimeStopWatch.Elapsed.ToString(RunTimespanFormat));
                }

                if (this.stage == Stages.Running || this.stage == Stages.Complete)
                {
                    output.Append(this.DrawProgressBar(this.runInBatches ? completedBatchCount : completed, this.runInBatches ? totalBatchCount : total));
                }

                Console.SetCursorPosition(0, 0);
                Console.Write(output);
                output.Clear();

                if (this.stage == Stages.Complete)
                {
                    output.Append('\n', 10);

                    if (failed > 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        output.Append("<- Failed -> Press any key to exit. Triage diffs in below file. \n");
                        output.Append(this.writer.FailuresFilePath).AppendLine();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        output.Append("<- Passed -> Press any key to exit. \n");
                    }

                    foreach (string error in this.errorLog)
                    {
                        output.Append(error + "/n");
                    }
                }
                else if (this.stage == Stages.Running)
                {
                    output.Append("<- Running ->  \n");
                }

                Console.SetCursorPosition(0, 13);
                Console.Write(output);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        private string DrawRunning(int total, int completed, int passed, int failed, int stopped, TimeSpan averageTime, int totalBatchCount, int completedBatchCount)
        {
            TimeSpan expectedTime = new TimeSpan(averageTime.Ticks * total);

            StringBuilder output = new StringBuilder();
            output.Append('#', this.width - 7);
            output.Append("\n");
            output.AppendFormat("Total comparisons            : {0}/{1}\n", completed, total);
            output.AppendFormat("Successful comparisons       : {0}/{1}\n", passed, completed);
            output.AppendFormat("Failed comparisons           : {0}/{1}\n", failed, completed);
            output.AppendFormat("Stopped comparisons          : {0}/{1}\n", stopped, completed);

            if (this.runInBatches)
            {
                output.AppendFormat("Completed Batches            : {0}/{1}\n", completedBatchCount, totalBatchCount);
            }

            output.Append('#', this.width - 7);
            output.Append("\n");
            output.AppendFormat("Baseline Environment  : {0}\n", this.baseEnvironment.ToUpper());
            output.AppendFormat("Undertest Environment : {0}\n", this.testEnvironment.ToUpper());
            output.AppendFormat("Expected Runtime: {0}\n", expectedTime.ToString(RunTimespanFormat));

            return output.ToString();
        }

        /// <summary>
        /// Creates a progress bar out of text
        /// </summary>
        /// <param name="progress">current completion value</param>
        /// <param name="max">total completion value</param>
        /// <returns>a string representing task progress</returns>
        private string DrawProgressBar(int progress, int max)
        {
            double percentComplete = (max > 0) ? (double)progress / (double)max : 0;
            int displayComplete = (int)(percentComplete * (this.width - 7));
            int padding = (this.width - 7) - displayComplete;
            return string.Format("\n[{0}{1}] {2}%\n", new string('-', displayComplete), new string(' ', padding), (int)(percentComplete * 100));
        }

        private string PrintResults(int total, int passed, int failed)
        {
            StringBuilder output = new StringBuilder();

            output.Append("\n");
            output.AppendFormat("Total comparisons            : {0}\n", total);
            output.AppendFormat("Successful comparisons       : {0}/{1}\n", passed, total);
            output.AppendFormat("Failed comparisons           : {0}/{1}\n", failed, total);
            output.Append("\n");
            output.AppendFormat("Baseline Environment  : {0}\n", this.baseEnvironment.ToUpper());
            output.AppendFormat("Undertest Environment : {0}\n", this.testEnvironment.ToUpper());
            output.AppendFormat("Runtime: {0}\n", TimeSpan.FromMilliseconds(this.runtimeStopWatch.ElapsedMilliseconds).ToString(RunTimespanFormat));

            return output.ToString();
        }
    }
}
