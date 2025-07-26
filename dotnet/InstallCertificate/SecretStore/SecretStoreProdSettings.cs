// <copyright file="SecretStoreProdSettings.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tools.InstallCertificates
{
    public class SecretStoreProdSettings : SecretStoreSettings
    {
        public SecretStoreProdSettings(string environmentName)
        {
            switch (environmentName)
            {
                case EnvironmentNames.Production.PaymentCo1Ldc1:
                case EnvironmentNames.Production.PaymentCo1Ldc2:
                case EnvironmentNames.Production.PaymentDm2Ldc1:
                case EnvironmentNames.Production.PaymentDm2Ldc2:
                case EnvironmentNames.Production.PaymentPimsCo1Ldc1:
                case EnvironmentNames.Production.PaymentPimsCo1Ldc2:
                case EnvironmentNames.Production.PaymentPimsDm2Ldc1:
                case EnvironmentNames.Production.PaymentPimsDm2Ldc2:
                case EnvironmentNames.Production.PMDCo1Ldc1:
                case EnvironmentNames.Production.PMDCo1Ldc2:
                case EnvironmentNames.Production.PMDDm2Ldc1:
                case EnvironmentNames.Production.PMDDm2Ldc2:
                    this.KeyValueSecretFileName = "KeyValueSecrets.xml.encr";
                    break;
                default:
                    this.KeyValueSecretFileName = "PaymentsReconKeyValueSecrets.xml.encr";
                    break;
            }
        }
    }
}
