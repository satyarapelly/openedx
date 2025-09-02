// <copyright file="Environment.cs" company="Microsoft">Copyright (c) Microsoft 2023. All rights reserved.</copyright>

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Commerce.Payments.Common.Tracing;

namespace Microsoft.Commerce.Payments.Common.Environments
{
    /// <summary>
    /// Environment settings for Payments that determines the current Environment.
    /// </summary>
    public class Environment
    {
        public const string EnvironmentSettingKey = "Environment";

        private static readonly object objectLock = new();
        private static volatile Environment currentEnvironment = null;

        // Replace with your actual tracing ID if required.
        private static readonly EventTraceActivity environmentEventTracingId = new(new Guid("169DB7AA-EEF1-4A50-A748-4FF842F51A0E"));

        private IConfiguration Configuration { get; }

        private Environment()
        {
            // Use the standard configuration builder (appsettings.json + env vars, etc.)
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }

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
                            var instance = new Environment();
                            instance.Initialize();
                            currentEnvironment = instance;
                        }
                    }
                }
                return currentEnvironment;
            }
        }

        public static bool IsProdOrPPEEnvironment =>
            Current.EnvironmentType == EnvironmentType.Production || Current.EnvironmentType == EnvironmentType.PPE;

        public EnvironmentType EnvironmentType { get; private set; }

        public string EnvironmentName { get; private set; }

        public string ApplicationInsightInstrumentKey { get; private set; }

        private void Initialize()
        {
            InitializeEnvironment();

            if (EnvironmentType == EnvironmentType.Production || EnvironmentType == EnvironmentType.PPE)
            {
                SllLogger.SetRealtimeLogging();
            }
        }

        private void InitializeEnvironment()
        {
            var environmentName = Configuration[EnvironmentSettingKey];
            EnvironmentName = string.IsNullOrWhiteSpace(environmentName) ? EnvironmentNames.OneBox.PaymentOnebox : environmentName.ToUpperInvariant();
            ApplicationInsightInstrumentKey = Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];

            EnvironmentType = EnvironmentName switch
            {
                // Production
                EnvironmentNames.Production.PXPMEPRODEastUS2 or
                EnvironmentNames.Production.PXPMEPRODNorthCentralUS or
                EnvironmentNames.Production.PXPMEPRODWestCentralUS or
                EnvironmentNames.Production.PXPMEPRODWestUS2 or
                EnvironmentNames.Production.PXPMEPRODWestUS or
                EnvironmentNames.Production.PXPMEPRODCentralUS or
                EnvironmentNames.Production.PXPMEPRODEastUS or
                EnvironmentNames.Production.PXPMEPRODSouthCentralUS => EnvironmentType.Production,

                // PPE
                EnvironmentNames.PPE.PXPMEPPEEastUS2 or
                EnvironmentNames.PPE.PXPMEPPENorthCentralUS or
                EnvironmentNames.PPE.PXPMEPPEWestCentralUS or
                EnvironmentNames.PPE.PXPMEPPEEastUS or
                EnvironmentNames.PPE.PXPMEPPEWestUS => EnvironmentType.PPE,

                // Integration
                EnvironmentNames.Integration.PXPMEIntWestUS or
                EnvironmentNames.Integration.PXPMEIntWestUS2 => EnvironmentType.Integration,

                // OneBox
                EnvironmentNames.OneBox.PaymentOnebox => EnvironmentType.OneBox,

                // AirCapi
                EnvironmentNames.AirCapi.PXAirCapi1 => EnvironmentType.Aircapi,

                // Unknown
                _ => throw TraceCore.TraceException<InvalidOperationException>(
                        new InvalidOperationException($"The environment name '{EnvironmentName}' does not map to any known environment type."))
            };
        }
    }
}
