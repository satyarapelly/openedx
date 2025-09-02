// <copyright file="Environment.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

using Microsoft.Commerce.Payments.Common;
using Microsoft.Commerce.Payments.Common.Tracing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.Metrics;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Diagnostics.Tracing;
using System.IO;
using System.Runtime.CompilerServices;

namespace Microsoft.Commerce.Payments.Tools.InstallCertificates
{
    public class Environment
    {
        public const string EnvironmentSettingKey = "Environment";

        private const string DefaultPaymentsPerfCounterName = "PaymentsRequests";

        private static object objectLock = new object();
        private static volatile Environment currentEnvironment = null;

        public static Environment Current
        {
            get
            {
                if (currentEnvironment == null)
                {
                    lock (objectLock)
                    {
                        if (currentEnvironment == null)
                        {
                            Environment current = new Environment();
                            current.Initialize();
                            currentEnvironment = current;
                        }
                    }
                }
                return currentEnvironment;
            }
        }

        public static bool IsProdEnvironment => Current.EnvironmentType == EnvironmentType.Production;

        public EnvironmentType EnvironmentType { get; private set; }
        public string EnvironmentName { get; private set; }
        public string PaymentsDataFolderName { get; private set; }
        public ISecretStore SecretStore { get; private set; }

        private Meter meter;
        private Counter<long> requestCounter;

        private void Initialize()
        {
            InitializeEnvironment();
            InitializeTelemetry();

            this.SecretStore = new NonAutopilotSecretStore(SecretStoreSettings.CreateInstance(this.EnvironmentType, this.EnvironmentName));

            if (this.EnvironmentType == EnvironmentType.Production)
            {
                SllLogger.SetRealtimeLogging();
            }
        }

        private void InitializeEnvironment()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            string environmentName = config[EnvironmentSettingKey];

            if (string.IsNullOrWhiteSpace(environmentName))
            {
                environmentName = System.Environment.GetEnvironmentVariable("environment") ?? throw new ArgumentException("Current Environment name could not be determined");
            }

            this.EnvironmentName = environmentName.ToUpperInvariant();

            switch (this.EnvironmentName)
            {
                case EnvironmentNames.Production.PaymentCo1Ldc1:
                case EnvironmentNames.Production.PaymentCo1Ldc2:
                case EnvironmentNames.Production.PaymentDm2Ldc1:
                case EnvironmentNames.Production.PaymentDm2Ldc2:
                    this.PaymentsDataFolderName = "Payments";
                    this.EnvironmentType = EnvironmentType.Production;
                    break;
                case EnvironmentNames.Production.PaymentReconCo1Ldc1:
                case EnvironmentNames.Production.PaymentReconCo1Ldc2:
                    this.EnvironmentType = EnvironmentType.Production;
                    this.PaymentsDataFolderName = "PaymentsRecon";
                    break;
                case EnvironmentNames.Integration.PaymentTestCo4:
                case EnvironmentNames.Integration.PaymentDevCo4:
                    this.EnvironmentType = EnvironmentType.Integration;
                    this.PaymentsDataFolderName = "Payments";
                    break;
                case EnvironmentNames.Integration.PaymentReconTestCo4:
                    this.EnvironmentType = EnvironmentType.Integration;
                    this.PaymentsDataFolderName = "PaymentsRecon";
                    break;
                case EnvironmentNames.OneBox.PaymentOnebox:
                    this.EnvironmentType = EnvironmentType.OneBox;
                    break;
                default:
                    throw new InvalidOperationException($"The environment name '{this.EnvironmentName}' does not map to any known environment type.");
            }
        }

        private void InitializeTelemetry()
        {
            this.meter = new Meter("Microsoft.Commerce.Payments", "1.0.0");
            this.requestCounter = meter.CreateCounter<long>(DefaultPaymentsPerfCounterName, unit: "requests", description: "Tracks payment requests");
        }

        public void TrackRequest(string label = "default")
        {
            this.requestCounter?.Add(1, new KeyValuePair<string, object>("label", label));
        }
    }
}
