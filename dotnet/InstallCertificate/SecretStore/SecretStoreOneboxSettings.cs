// <copyright file="SecretStoreOneboxSettings.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tools.InstallCertificates
{
    using System.IO;

    public class SecretStoreOneboxSettings : SecretStoreSettings
    {
        public SecretStoreOneboxSettings()
        {
           this.KeyValueSecretFileName = "KeyValueSecrets.xml";
        }
    }
}
