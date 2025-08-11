// <copyright file="Environment.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Environments
{
    using System;
    using System.Configuration;
    using Microsoft.Commerce.Payments.Common.Tracing;

    /// <summary>
    /// Environment settings for Payments that determines the current Environment.
    /// </summary>
    public class Environment
    {
        public const string EnvironmentSettingKey = "Environment";

        private static object objectLock = new object();
        private static volatile Environment currentEnvironment = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="Environment"/> class.
        /// </summary>
        protected Environment()
        {
        }

        /// <summary>
        /// Gets the singleton instance of the environment.  
        /// </summary>
        public static Environment Current
        {
            get
            {
                if (Environment.currentEnvironment == null)
                {
                    lock (Environment.objectLock)
                    {
                        if (Environment.currentEnvironment == null)
                        {
                            Environment current = new Environment();
                            current.Initialize();
                            Environment.currentEnvironment = current;
                        }
                    }
                }

                return Environment.currentEnvironment;
            }
        }

        public static bool IsProdOrPPEEnvironment
        {
            get
            {
                return Current.EnvironmentType == EnvironmentType.Production || Current.EnvironmentType == EnvironmentType.PPE;
            }
        }

        public EnvironmentType EnvironmentType { get; private set; }

        public string EnvironmentName { get; private set; }

        public string ApplicationInsightInstrumentKey { get; private set; }

        /// <summary>
        /// Initialize the environment name and type.
        /// </summary>
        private void InitializeEnvironment()
        {
            string environmentName = ConfigurationManager.AppSettings[EnvironmentSettingKey];
            this.EnvironmentName = environmentName == null ? EnvironmentNames.OneBox.PaymentOnebox : environmentName.ToUpper();
            this.ApplicationInsightInstrumentKey = ConfigurationManager.AppSettings["APPINSIGHTS_INSTRUMENTATIONKEY"];

            switch (this.EnvironmentName)
            {
                case EnvironmentNames.Production.PXPMEPRODEastUS2:
                case EnvironmentNames.Production.PXPMEPRODNorthCentralUS:
                case EnvironmentNames.Production.PXPMEPRODWestCentralUS:
                case EnvironmentNames.Production.PXPMEPRODWestUS2:
                case EnvironmentNames.Production.PXPMEPRODWestUS:
                case EnvironmentNames.Production.PXPMEPRODCentralUS:
                case EnvironmentNames.Production.PXPMEPRODEastUS:
                case EnvironmentNames.Production.PXPMEPRODSouthCentralUS:
                    this.EnvironmentType = EnvironmentType.Production;
                    break;
                case EnvironmentNames.PPE.PXPMEPPEEastUS2:
                case EnvironmentNames.PPE.PXPMEPPENorthCentralUS:
                case EnvironmentNames.PPE.PXPMEPPEWestCentralUS:
                case EnvironmentNames.PPE.PXPMEPPEEastUS:
                case EnvironmentNames.PPE.PXPMEPPEWestUS:
                    this.EnvironmentType = EnvironmentType.PPE;
                    break;
                case EnvironmentNames.Integration.PXPMEIntWestUS:
                case EnvironmentNames.Integration.PXPMEIntWestUS2:
                    this.EnvironmentType = EnvironmentType.Integration;
                    break;
                case EnvironmentNames.OneBox.PaymentOnebox:
                    this.EnvironmentType = EnvironmentType.OneBox;
                    break;
                case EnvironmentNames.AirCapi.PXAirCapi1:
                    this.EnvironmentType = EnvironmentType.Aircapi;
                    break;
                default:
                    throw TraceCore.TraceException<InvalidOperationException>(new InvalidOperationException(string.Format("The environment name '{0}' does not map to any known environment type.", this.EnvironmentName)));
            }
        }

        /// <summary>
        /// Initialize the Autopilot Environment
        /// </summary>
        private void Initialize()
        {
            this.InitializeEnvironment();

            // turn real time logging on for PROD only
            if (this.EnvironmentType == EnvironmentType.Production || this.EnvironmentType == EnvironmentType.PPE)
            {
                SllLogger.SetRealtimeLogging();
            }
        }
    }
}
