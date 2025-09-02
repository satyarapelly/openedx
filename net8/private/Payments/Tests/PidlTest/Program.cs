// <copyright file="Program.cs" company="Microsoft">Copyright (c) Microsoft 2016. All rights reserved.</copyright>
namespace PidlTest
{
    using System;
    using System.Configuration;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Execute test sequence
    /// </summary>
    public class Program
    {
        private const string ArgTestType = "/testType";
        private const string ArgLoadTest = "LoadTest";
        private const string ArgDiffTest = "DiffTest";
        private const string ArgE2ETest = "E2ETest";

        // active test
        private static ITestRunner testRunner;

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
        public static async Task<int> Main(string[] args)
        {
            ServicePointManager.SecurityProtocol &= ~(SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11); // lgtm[cs/hard-coded-deprecated-security-protocol] lgtm[cs/hard-coded-security-protocol] -Suppressing because of a false positive from Semmle // DevSkim: ignore DS440000,DS440020,DS144436 as old protocols are being explicitly removed
            if (ServicePointManager.SecurityProtocol == 0)
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; // lgtm[cs/hard-coded-security-protocol] -Suppressing because of a false positive from Semmle // DevSkim: ignore DS440000,DS440020,DS144436 as old protocols are being explicitly removed
            }
            
            for (int i = 0; i < args.Length; i++)
            {
                if (string.Compare(args[i], ArgTestType, true) == 0)
                {
                    if (i + 1 >= args.Length)
                    {
                        Console.WriteLine(Constants.ErrorMessages.ArgValueMissingFormat, ArgTestType);
                        return await Task.FromResult(1);
                    }
                    else
                    {
                        if (string.Compare(args[i + 1], ArgLoadTest, true) == 0)
                        {
                            testRunner = new Load.TestRunner();
                            break;
                        }
                        else if (string.Compare(args[i + 1], ArgDiffTest, true) == 0)
                        {
                            testRunner = new Diff.TestRunner();
                            break;
                        }
                        else if (string.Compare(args[i + 1], ArgE2ETest, true) == 0)
                        {
                            testRunner = new E2E.TestRunner();
                            break;
                        }
                    }
                }
                else if (string.Compare(args[i], "/?", true) == 0 || string.Compare(args[i], "/help", true) == 0)
                {
                    ShowHelp();
                    return await Task.FromResult(1);
                }
            }

            if (testRunner == null)
            {
                if (!string.IsNullOrEmpty(AppConfig.Configuration["TestType"]))
                {
                    string type = AppConfig.Configuration["TestType"];
                    if (type.ToLower() == ArgLoadTest.ToLower())
                    {
                        testRunner = new Load.TestRunner();
                    }
                    else if (type.ToLower() == ArgDiffTest.ToLower())
                    {
                        testRunner = new Diff.TestRunner();
                    }
                }
                else
                {
                    Console.WriteLine("Failed to load test type. Enter a valid argument using '/test' or update the config file.");
                    return await Task.FromResult(1);
                }
            }

            if (testRunner == null)
            {
                Console.WriteLine(Constants.ErrorMessages.ArgMissingFormat, ArgTestType);
                return await Task.FromResult(1);
            }
            else if (!testRunner.ParseArguments(args))
            {
                return await Task.FromResult(1);
            }

            var result = await testRunner.StartTestAsync();

            if (!HostingUtility.IsPipelineRun())
            {
                Console.ReadKey(true);
            }

            Console.WriteLine("\nStopping...");
            await testRunner.StopTestAsync();

            Console.WriteLine("Stopped!");

            return result;
        }

        /// <summary>
        /// Writes program details to the console 
        /// </summary>
        public static void ShowHelp()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("--------------------------------------------------------------------------------------------------------------");
            builder.AppendLine("This tool heps test PX service.  It supports three types of tests");
            builder.AppendLine("  DiffTest - Detects regressions by comparing json responses of a new version of PX service with a known-good version.");
            builder.AppendLine("  LoadTest - Can help identify scale regressions during refactoring or other structural changes to PX service code.");
            builder.AppendLine("  E2ETest - Test scenarios of postCC for all the partners with real card in PPE and PROD.");
            builder.AppendLine(string.Empty);
            builder.AppendLine("For help on LoadTest, type the following command:");
            builder.AppendLine("PIDLTest.exe /testType LoadTest /?");
            builder.AppendLine(string.Empty);
            builder.AppendLine("For help on DiffTest, type the following command:");
            builder.AppendLine("PIDLTest.exe /testType DiffTest /?");
            builder.AppendLine(string.Empty);
            builder.AppendLine("For help on E2ETest, type the following command:");
            builder.AppendLine("PIDLTest.exe /testType E2ETest /?");
            builder.AppendLine("--------------------------------------------------------------------------------------------------------------");

            Console.WriteLine(builder.ToString());
        }
    }
}
