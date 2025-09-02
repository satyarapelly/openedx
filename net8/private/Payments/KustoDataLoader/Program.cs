// <copyright file="Program.cs" company="Microsoft">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace KustoDataLoader
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using Kusto.Ingest;
    using Microsoft.Commerce.Payments.Management.Utilities;
    using Microsoft.Commerce.Payments.Monitoring.KustoDataLoaderCore;

    public class Program
    {
        private static CancellationTokenSource cancellationTokenSource;
        private static EnvironmentType environmentType;

        public static int Main(string[] args)
        {
            cancellationTokenSource = new CancellationTokenSource();
            environmentType = GetEnvironmentType();
            InitializeTracingListeners(environmentType);
            PXKustoDataLoaderSettings settings = PXKustoDataLoaderSettings.Create(environmentType);

            try
            {
                if (args.Length > 0)
                {
                    switch (args[0].ToLower())
                    {
                        case "/updateschema":
                        case "-updateschema":
                            KustoHelpers.SetupKustoTable(settings, false);
                            return 0;

                        case "/replaceschema":
                        case "-replaceschema":
                            KustoHelpers.SetupKustoTable(settings, true);
                            return 0;

                        case "/listingestionerrors":
                        case "-listingestionerrors":
                            IEnumerable<IngestionFailure> errors = KustoHelpers.QueryIngestionErrorsAsync(settings).Result;
                            foreach (IngestionFailure error in errors)
                            {
                                Trace.TraceError($"Ingestion Error: {error.Info.Details}");
                            }

                            return 0;

                        default:
                            throw new ArgumentException($"Unknown argument { args[0] }");
                    }
                }

                Console.CancelKeyPress += new ConsoleCancelEventHandler(WaitForKeyPress);
                Console.WriteLine("KustoDataLoader is running. Press Ctrl + C to stop.");

                var cancellationToken = cancellationTokenSource.Token;

                IProcessor[] processors = new IProcessor[]
                {
                    new SllProcessor(settings, Path.Combine(settings.DataDirectory, "LogCollector", "PaymentsSll"), "PXServiceSll_*.log")
                };

                ProcessingPipeline.Run(
                    processors,
                    settings,
                    cancellationToken,
                    Path.Combine(settings.DataDirectory, "Logs", "local"),
                    Kusto.Cloud.Platform.Utils.TraceVerbosity.Warning);
            }
            catch (Exception e)
            {
                Trace.TraceError($"Exception: { e }");
                return 1;
            }

            return 0;
        }

        private static void WaitForKeyPress(object sender, ConsoleCancelEventArgs args)
        {
            Trace.TraceInformation("Cancellation key was pressed.");
            args.Cancel = true;
            cancellationTokenSource.Cancel();
        }

        private static void InitializeTracingListeners(EnvironmentType environmentType)
        {
            switch (environmentType)
            {
                case EnvironmentType.Int:
                case EnvironmentType.Ppe:
                case EnvironmentType.Prod:
                    Trace.Listeners.Add(new AutopilotTraceListener());
                    break;

                default:
                    Trace.Listeners.Add(new ConsoleTraceListener());
                    break;
            }
            
            Trace.AutoFlush = true;
        }

        private static EnvironmentType GetEnvironmentType()
        {
            // Autopilot defines the environment environment variable for all services.
            // See http://sharepoint/sites/autopilot/wiki/Environment%20Variables.aspx
            string currentEnvironment = Environment.GetEnvironmentVariable("Environment");

            switch (currentEnvironment?.ToUpperInvariant())
            {
                case "CPPAYMENTEXPERIENCESERVICE-TEST-CO4":
                    return EnvironmentType.Int;
                case "CPPAYMENTEXPERIENCESERVICE-INT-CO4":
                    return EnvironmentType.Ppe;
                case "CPPAYMENTEXPERIENCESERVICE-PROD-CO1C":
                case "CPPAYMENTEXPERIENCESERVICE-PROD-DM2C":
                    return EnvironmentType.Prod;
                default:
                    return EnvironmentType.Onebox;
            }
        }
    }
}
