// <copyright file="PXKustoDataLoaderSettings.cs" company="Microsoft">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace KustoDataLoader
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using Microsoft.Commerce.Payments.Management.Utilities;
    using Microsoft.Commerce.Payments.Management.Utilities.Autopilot;
    using Microsoft.Commerce.Payments.Monitoring.KustoDataLoaderCore;
    using Microsoft.Search.Autopilot;

    public class PXKustoDataLoaderSettings : KustoDataLoaderSettings
    {
        private const string AadApplicationId = "bbd9e236-61d5-4518-bf90-5a4868fea8d1";

        private PXKustoDataLoaderSettings()
        {
        }

        public string MachineFunction { get; private set; }

        public string DataDirectory { get; private set; }

        public static PXKustoDataLoaderSettings Create(EnvironmentType environmentType)
        {
            var s = new PXKustoDataLoaderSettings();
            switch (environmentType)
            {
                case EnvironmentType.Onebox:
                    s.ClusterName = "pst";
                    s.Database = "Test";
                    s.Environment = "ONEBOX";
                    s.ApplicationKey = null;
                    s.TableName = System.Environment.UserName + "_Events";
                    s.DoneProcessingCallback = DoneProcessing;

                    s.MachineFunction = "ONEBOX";
                    s.DataDirectory = Directory.GetCurrentDirectory();
                    break;

                case EnvironmentType.Int:
                case EnvironmentType.Ppe:
                    if (!APRuntime.IsInitialized)
                    {
                        APRuntime.Initialize("ServiceConfig.ini");
                    }

                    s.ClusterName = "pst";
                    s.Database = "Int";
                    s.Environment = APRuntime.EnvironmentName;
                    s.ApplicationKey = LoadSecretStoreKey(@"Payments\Secrets\PstKustoAADApplicationKey1.dat.encr");
                    s.AADApplicationId = AadApplicationId;
                    s.TableName = "Events";
                    s.DoneProcessingCallback = DoneProcessing;

                    s.MachineFunction = APRuntime.MachineFunction;
                    s.DataDirectory = APRuntime.DataDirectory;
                    break;

                case EnvironmentType.Prod:
                    if (!APRuntime.IsInitialized)
                    {
                        APRuntime.Initialize("ServiceConfig.ini");
                    }

                    s.ClusterName = "pst";
                    s.Database = "Prod";
                    s.Environment = APRuntime.EnvironmentName;
                    s.ApplicationKey = LoadSecretStoreKey(@"Payments\Secrets\PstKustoAADApplicationKey1.dat.encr");
                    s.AADApplicationId = AadApplicationId;
                    s.TableName = "Events";
                    s.DoneProcessingCallback = DoneProcessing;

                    s.MachineFunction = APRuntime.MachineFunction;
                    s.DataDirectory = APRuntime.DataDirectory;
                    break;

                default:
                    throw new NotSupportedException($"Environment type: '{environmentType}' is not supported.");
            }

            return s;
        }

        private static string LoadSecretStoreKey(string filename)
        {
            var sskeyloader = new SecretStoreSecretLoader();
            return sskeyloader.Load(filename);
        }

        private static void DoneProcessing()
        {
            Thread.Sleep(1000);
        }
    }
}
