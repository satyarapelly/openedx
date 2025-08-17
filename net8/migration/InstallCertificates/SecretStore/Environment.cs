// <copyright file="Environment.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tools.InstallCertificates
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Web.Hosting;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Search.Autopilot;

    /// <summary>
    /// Environment settings for Payments that determines the current Environment.
    /// </summary>
    public class Environment
    {
        public const string EnvironmentSettingKey = "Environment";

        private const string ApplicationPerfCounterFileNameKey = "AppPerfCountersMemFile";
        private const string DefaultPaymentsPerfCounterFile = "Payments";

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

        public static bool IsProdEnvironment
        {
            get
            {
                return Current.EnvironmentType == EnvironmentType.Production;
            }
        }

        public EnvironmentType EnvironmentType { get; private set; }

        public string EnvironmentName { get; private set; }

        public string PaymentsDataFolderName { get; private set; }

        public ISecretStore SecretStore { get; private set; }
       
        /// <summary>
        /// Gets a value indicating whether this environment is an autopilot environment
        /// or not.
        /// </summary>
        /// <remarks>
        /// Don't read too much into the fact that this is defaulted to true.  True was chosen
        /// for the base class since:
        ///  (1) We are more likely to add new auto pilot environments than non-AP.
        ///  (2) The cost of figuring out that you forgot to set this to true for an AP
        ///      environment is higher than the cost of figuring out that you forgot to
        ///      set it to false in a non-AP environment.
        /// </remarks>
        private bool IsAutoPilotEnvironment
        {
            get
            {
                if (this.EnvironmentType == EnvironmentType.OneBox)
                {
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Initialize AutoPilot.  The purpose of putting this in a separate method is
        /// so that the JIT compiler doesn't attempt to load Microsoft.Search.AutoPilot
        /// during unit tests.  Unit tests currently run in 32 bit and AutoPilot requires
        /// 64 bit.  This is a temporary workaround until our enlistment supports 64
        /// bit tests.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void InitializeAutopilot()
        {
            if (APRuntime.IsInitialized)
            {
                return;
            }

            string applicationPath = HostingEnvironment.ApplicationPhysicalPath;
            if (applicationPath == null)
            {
                applicationPath = ".";
            }
            else 
            {
                // APRuntime.Initialize and w3wp dump report generation relies on 
                // setting the working directory.
                Directory.SetCurrentDirectory(applicationPath);
                System.Environment.SetEnvironmentVariable("approot", new DirectoryInfo(applicationPath).Parent.FullName);
            }

            APRuntime.Initialize(applicationPath + "\\Monitoring.ini");

            // Configuring Autopilot counters
            string applicationPerfCounterMemFile = DefaultPaymentsPerfCounterFile;
            try
            {
                applicationPerfCounterMemFile = ConfigurationManager.AppSettings[ApplicationPerfCounterFileNameKey];

                if (applicationPerfCounterMemFile != null)
                {
                }
                else
                {
                    applicationPerfCounterMemFile = DefaultPaymentsPerfCounterFile;
                }
            }
            catch (ConfigurationErrorsException)
            {
            }

            string counterFile = string.Format(@"{0}\perf\{1}.prf", APRuntime.DataDirectory, applicationPerfCounterMemFile);
            APRuntime.SetCounterFile(counterFile);
        }

        /// <summary>
        /// Initialize the environment name and type.
        /// </summary>
        private void InitializeEnvironment()
        {
            string environmentName = ConfigurationManager.AppSettings[EnvironmentSettingKey];
            
            if (string.IsNullOrWhiteSpace(environmentName))
            {
                // This is picked up by tests, which don't run as an executable
                // or as a Web application.
                ExeConfigurationFileMap map = new ExeConfigurationFileMap();
                map.ExeConfigFilename = "Payments.Common.config";
                Configuration config = null;
                try
                {
                    config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
                }
                catch
                {
                    throw TraceCore.TraceException<ArgumentException>(new ArgumentException("Current Environment name could not be determined"));
                }

                if (config.AppSettings.Settings[EnvironmentSettingKey] != null)
                {
                    environmentName = config.AppSettings.Settings[EnvironmentSettingKey].Value;
                }

                if (string.IsNullOrWhiteSpace(environmentName))
                {
                    environmentName = System.Environment.GetEnvironmentVariable("environment");

                    if (string.IsNullOrWhiteSpace(environmentName))
                    {
                        throw TraceCore.TraceException<ArgumentException>(new ArgumentException("Current Environment name could not be determined"));
                    }
                }
            }

            this.EnvironmentName = environmentName.ToUpperInvariant();

            switch (this.EnvironmentName)
            {
                case EnvironmentNames.Production.PaymentCo1Ldc1:
                case EnvironmentNames.Production.PaymentCo1Ldc2:
                case EnvironmentNames.Production.PaymentDm2Ldc1:
                case EnvironmentNames.Production.PaymentDm2Ldc2:
                case EnvironmentNames.Production.PaymentPimsCo1Ldc1:
                case EnvironmentNames.Production.PaymentPimsCo1Ldc2:
                case EnvironmentNames.Production.PaymentPimsDm2Ldc1:
                case EnvironmentNames.Production.PaymentPimsDm2Ldc2:
                case EnvironmentNames.Production.PaymentSqlCo3:
                case EnvironmentNames.Production.PaymentSqlDm2:
                case EnvironmentNames.Production.PXCo1c:
                case EnvironmentNames.Production.PXDm2c:
                case EnvironmentNames.Production.PXCo4:
                case EnvironmentNames.Production.PIFDPpeBn2:
                case EnvironmentNames.Production.PIFDProdBn2:
                case EnvironmentNames.Production.PIFDProdCy2:
                case EnvironmentNames.Production.PIFDProdDb5:
                case EnvironmentNames.Production.PIFDProdHk2:
                case EnvironmentNames.Production.PIFDProdPFDm2p:
                case EnvironmentNames.Production.PIFDProdPFMw1p:
                case EnvironmentNames.Production.PMDCo1Ldc1:
                case EnvironmentNames.Production.PMDCo1Ldc2:
                case EnvironmentNames.Production.PMDDm2Ldc1:
                case EnvironmentNames.Production.PMDDm2Ldc2:
                    this.PaymentsDataFolderName = "Payments";
                    this.EnvironmentType = EnvironmentType.Production;
                    break;
                case EnvironmentNames.Production.PaymentReconCo1Ldc1:
                case EnvironmentNames.Production.PaymentReconCo1Ldc2:
                case EnvironmentNames.Production.PaymentReconDm2Ldc1:
                case EnvironmentNames.Production.PaymentReconDm2Ldc2:
                    this.EnvironmentType = EnvironmentType.Production;
                    this.PaymentsDataFolderName = "PaymentsRecon";
                    break;
                case EnvironmentNames.Integration.PaymentTestCo4:
                case EnvironmentNames.Integration.PaymentPimsIntCo4:
                case EnvironmentNames.Integration.PaymentDevCo4:
                case EnvironmentNames.Integration.PaymentSqlDevCo3:
                case EnvironmentNames.Integration.PaymentPimsDevCo4:
                case EnvironmentNames.Integration.PXTestCo4:
                case EnvironmentNames.Integration.PifdIntCo4:
                case EnvironmentNames.Integration.PMDIntCo4:
                    this.EnvironmentType = EnvironmentType.Integration;
                    this.PaymentsDataFolderName = "Payments";
                    break;
                case EnvironmentNames.Integration.PaymentReconIntCo4:
                case EnvironmentNames.Integration.PaymentReconTestCo4:
                    this.EnvironmentType = EnvironmentType.Integration;
                    this.PaymentsDataFolderName = "PaymentsRecon";
                    break;
                case EnvironmentNames.OneBox.PaymentOnebox:
                    this.EnvironmentType = EnvironmentType.OneBox;
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
            if (this.IsAutoPilotEnvironment)
            {
                InitializeAutopilot();

                this.SecretStore = new AutopilotSecretStore(SecretStoreSettings.CreateInstance(this.EnvironmentType, this.EnvironmentName));

                PerfCounter.RegisterPerfCounterFactory(RequestsCounterTechnology.AutoPilot, AutopilotPerfCounter.CreateCounter);
            }
            else
            {
                this.SecretStore = new NonAutopilotSecretStore(SecretStoreSettings.CreateInstance(this.EnvironmentType, this.EnvironmentName));
            }

            // turn real time logging on for PROD only
            if (this.EnvironmentType == EnvironmentType.Production)
            {
                SllLogger.SetRealtimeLogging();
            }
        }

        private class AutopilotPerfCounter : PerfCounter
        {
            private AutopilotPerfCounter(string category, string name, PerfCounterType perfCounterType)
            {
                UInt64Counter counter = new UInt64Counter(category, name, ToCounterFlag(perfCounterType));
            }

            public static PerfCounter CreateCounter(string category, string name, PerfCounterType perfCounterType)
            {
                return new AutopilotPerfCounter(category, name, perfCounterType);
            }

            private static CounterFlag ToCounterFlag(PerfCounterType perfCounterType)
            {
                switch (perfCounterType)
                {
                    case PerfCounterType.None:
                        return CounterFlag.None;
                    case PerfCounterType.Number:
                        return CounterFlag.Number;
                    case PerfCounterType.NumberPercentiles:
                        return CounterFlag.Number_Percentiles;
                    case PerfCounterType.Rate:
                        return CounterFlag.Rate;
                    default:
                        return CounterFlag.None;
                }
            }
        }
    }
}
