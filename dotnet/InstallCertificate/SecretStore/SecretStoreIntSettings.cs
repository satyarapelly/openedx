// <copyright file="SecretStoreIntSettings.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tools.InstallCertificates
{
    public class SecretStoreIntSettings : SecretStoreSettings
    {
        public SecretStoreIntSettings(string environmentName)
        {
            switch (environmentName)
            {
                case EnvironmentNames.Integration.PaymentDevCo4:
                case EnvironmentNames.Integration.PaymentTestCo4:
                case EnvironmentNames.Integration.PaymentPimsDevCo4:
                case EnvironmentNames.Integration.PaymentPimsIntCo4:
                case EnvironmentNames.Integration.PMDIntCo4:
                    this.KeyValueSecretFileName = "KeyValueSecrets.xml.encr";
                    break;
                default:
                    this.KeyValueSecretFileName = "PaymentsReconKeyValueSecrets.xml.encr";
                    break;
            }
        }
    }
}
