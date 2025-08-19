// <copyright file="ServiceClientSettings.cs" company="Microsoft">Copyright (c) Microsoft 2017. All rights reserved.</copyright>

namespace Test.Common
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography.X509Certificates;
    using Tests.Common;

    public class ServiceClientSettings
    {
        public ServiceClientSettings()
        {
            this.MaximumRequestRetryCount = 5;
            this.RetryDelay = TimeSpan.FromSeconds(2);
            this.ServiceApiVersion = "2015-02-16";
            this.VersionInHeader = true;
            this.AadTokenProviders = new Dictionary<Constants.AADClientType, AadTokenProvider>();
        }

        public string ServiceEndpoint { get; set; }

        public int MaximumRequestRetryCount { get; set; }

        public TimeSpan RetryDelay { get; set; }

        public string ServiceApiVersion { get; set; }

        public X509Certificate2 ClientCertificate { get; set; }

        public bool VersionInHeader { get; set; }

        public Dictionary<Constants.AADClientType, AadTokenProvider> AadTokenProviders { get; }

        public string HostName { get; set; }
    }
}