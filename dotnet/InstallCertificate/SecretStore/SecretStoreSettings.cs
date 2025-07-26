// <copyright file="SecretStoreSettings.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tools.InstallCertificates
{
    using System;
    using Microsoft.Commerce.Payments.Common.Environments;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Environment = Microsoft.Commerce.Payments.Common.Environments.Environment;

    public class SecretStoreSettings
    {
        public string KeyValueSecretFileName { get; protected set; }

        public static SecretStoreSettings CreateInstance(EnvironmentType environmentType, string environmentName)
        {
            switch (environmentType)
            {
                case EnvironmentType.Production:
                    return new SecretStoreProdSettings(environmentName);
                case EnvironmentType.Integration:
                    return new SecretStoreIntSettings(environmentName);
                case EnvironmentType.OneBox:
                    return new SecretStoreOneboxSettings();
                default:
                    throw TraceCore.TraceException<InvalidOperationException>(new InvalidOperationException(string.Format("Configuration not found for the environment: '{0}'", environmentType)));
            }
        }
    }
}
