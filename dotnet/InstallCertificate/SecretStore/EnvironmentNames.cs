// <copyright file="EnvironmentNames.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tools.InstallCertificates
{
    using System;
    using Microsoft.Commerce.Payments.Common.Tracing;

    public static class EnvironmentNames
    {
        public static string GetFqdn(string environmentName, string dnsName)
        {
            string fqdn = null;
            switch (environmentName)
            {
                case Production.PaymentCo1Ldc1:
                case Production.PaymentPimsCo1Ldc1:
                    fqdn = string.Format("LDC1_{0}.CO1.prod.sd.net", dnsName);
                    break;
                case Production.PaymentCo1Ldc2:
                case Production.PaymentPimsCo1Ldc2:
                    fqdn = string.Format("LDC2_{0}.CO1.prod.sd.net", dnsName);
                    break;
                case Production.PaymentDm2Ldc1:
                case Production.PaymentPimsDm2Ldc1:
                    fqdn = string.Format("LDC1_{0}.DM2.prod.sd.net", dnsName);
                    break;
                case Production.PaymentDm2Ldc2:
                case Production.PaymentPimsDm2Ldc2:
                    fqdn = string.Format("LDC2_{0}.DM2.prod.sd.net", dnsName);
                    break;
                case Integration.PaymentTestCo4:
                    fqdn = string.Format("{0}.{1}.co4.ap.phx.gbl", dnsName, Integration.PaymentTestCo4.ToLower());
                    break;
                case Integration.PaymentDevCo4:
                    fqdn = string.Format("{0}.{1}.co4.ap.phx.gbl", dnsName, Integration.PaymentDevCo4.ToLower());
                    break;
                case Integration.PaymentPimsDevCo4:
                    fqdn = string.Format("{0}.{1}.co4.ap.phx.gbl", dnsName, Integration.PaymentPimsDevCo4.ToLower());
                    break;
                case Integration.PaymentPimsIntCo4:
                    fqdn = string.Format("{0}.{1}.co4.ap.phx.gbl", dnsName, Integration.PaymentPimsIntCo4.ToLower());
                    break;
                case OneBox.PaymentOnebox:
                    fqdn = "localhost";
                    break;
            }

            if (fqdn == null)
            {
                throw TraceCore.TraceException(new InvalidOperationException(string.Format("The environment name '{0}' cannot be used in GetFqdn.", environmentName)));
            }

            return fqdn;
        }

        public static string GetCluster(string environmentName)
        {
            string clusterName = null;
            switch (environmentName)
            {
                case Production.PaymentCo1Ldc1:
                case Production.PaymentCo1Ldc2:
                case Production.PaymentReconCo1Ldc1:
                case Production.PaymentReconCo1Ldc2:
                case Production.PaymentPimsCo1Ldc1:
                case Production.PaymentPimsCo1Ldc2:
                    clusterName = "CO1C";
                    break;
                case Production.PaymentDm2Ldc1:
                case Production.PaymentDm2Ldc2:
                case Production.PaymentReconDm2Ldc1:
                case Production.PaymentReconDm2Ldc2:
                case Production.PaymentPimsDm2Ldc1:
                case Production.PaymentPimsDm2Ldc2:
                    clusterName = "DM2C";
                    break;
                case Integration.PaymentTestCo4:
                case Integration.PaymentDevCo4:
                case Integration.PaymentReconTestCo4:
                case Integration.PaymentReconIntCo4:
                case Integration.PaymentPimsDevCo4:
                case Integration.PaymentPimsIntCo4:
                    clusterName = "CO4";
                    break;
                case OneBox.PaymentOnebox:
                    clusterName = "ONEBOXCLUSTER";
                    break;
            }

            if (clusterName == null)
            {
                throw TraceCore.TraceException(new InvalidOperationException(string.Format("The environment name '{0}' cannot be used in GetFqdn.", environmentName)));
            }

            return clusterName;
        }

        public static string GetDataCenter(string environmentName)
        {
            string dataCenter = null;
            switch (environmentName)
            {
                case Production.PaymentCo1Ldc1:
                case Production.PaymentDm2Ldc1:
                case Production.PaymentReconCo1Ldc1:
                case Production.PaymentReconDm2Ldc1:
                case Production.PaymentPimsCo1Ldc1:
                case Production.PaymentPimsDm2Ldc1:
                    dataCenter = "LDC1";
                    break;
                case Production.PaymentCo1Ldc2:
                case Production.PaymentDm2Ldc2:
                case Production.PaymentReconCo1Ldc2:
                case Production.PaymentReconDm2Ldc2:
                case Production.PaymentPimsCo1Ldc2:
                case Production.PaymentPimsDm2Ldc2:
                    dataCenter = "LDC2";
                    break;
                case Integration.PaymentTestCo4:
                case Integration.PaymentDevCo4:
                case Integration.PaymentReconIntCo4:
                case Integration.PaymentReconTestCo4:
                case Integration.PaymentPimsDevCo4:
                case Integration.PaymentPimsIntCo4:
                    dataCenter = "DataCenter";
                    break;
                case OneBox.PaymentOnebox:
                    dataCenter = "ONEBOXDATACENTER";
                    break;
            }

            if (dataCenter == null)
            {
                throw TraceCore.TraceException(new InvalidOperationException(string.Format("The environment name '{0}' cannot be used in GetFqdn.", environmentName)));
            }

            return dataCenter;
        }

        public static string GetCertificateName(string environmentName)
        {
            string certificateName = null;
            switch (environmentName)
            {
                case Integration.PaymentTestCo4:
                    certificateName = string.Format("{0}.co4.ap.phx.gbl", Integration.PaymentTestCo4.ToLower());
                    break;
                case Integration.PaymentDevCo4:
                    certificateName = string.Format("{0}.co4.ap.phx.gbl", Integration.PaymentDevCo4.ToLower());
                    break;
                case Integration.PaymentReconIntCo4:
                    certificateName = string.Format("{0}.co4.ap.phx.gbl", Integration.PaymentReconIntCo4.ToLower());
                    break;
                case Integration.PaymentReconTestCo4:
                    certificateName = string.Format("{0}.co4.ap.phx.gbl", Integration.PaymentReconTestCo4.ToLower());
                    break;
                case Integration.PaymentPimsDevCo4:
                    certificateName = string.Format("{0}.co4.ap.phx.gbl", Integration.PaymentTestCo4.ToLower());
                    break;
                case Integration.PaymentPimsIntCo4:
                    certificateName = string.Format("{0}.co4.ap.phx.gbl", Integration.PaymentPimsIntCo4.ToLower());
                    break;
                case Integration.PXTestCo4:
                    certificateName = string.Format("{0}.co4.ap.phx.gbl", Integration.PXTestCo4.ToLower());
                    break;
                case Integration.PifdIntCo4:
                    certificateName = string.Format("{0}.co4.ap.phx.gbl", Integration.PifdIntCo4.ToLower());
                    break;
                case Integration.PMDIntCo4:
                    certificateName = string.Format("{0}.co4.ap.phx.gbl", Integration.PMDIntCo4.ToLower());
                    break;
            }

            if (certificateName == null)
            {
                throw TraceCore.TraceException(new InvalidOperationException(string.Format("The environment name '{0}' cannot be used in GetCertificateName.", environmentName)));
            }

            return certificateName;
        }

        public static class Production
        {
            public const string PaymentCo1Ldc1 = "CPPAYMENTSLDC1-PROD-CO1C";
            public const string PaymentCo1Ldc2 = "CPPAYMENTSLDC2-PROD-CO1C";
            public const string PaymentDm2Ldc1 = "CPPAYMENTSLDC1-PROD-DM2C";
            public const string PaymentDm2Ldc2 = "CPPAYMENTSLDC2-PROD-DM2C";
            public const string PaymentPimsCo1Ldc1 = "CPPAYMENTSPIMSLDC1-PROD-CO1C";
            public const string PaymentPimsCo1Ldc2 = "CPPAYMENTSPIMSLDC2-PROD-CO1C";
            public const string PaymentPimsDm2Ldc1 = "CPPAYMENTSPIMSLDC1-PROD-DM2C";
            public const string PaymentPimsDm2Ldc2 = "CPPAYMENTSPIMSLDC2-PROD-DM2C";
            public const string PaymentSqlCo3 = "CPPAYMENTSSQL-PROD-CO3";
            public const string PaymentSqlDm2 = "CPPAYMENTSSQL-PROD-DM2C";
            public const string PaymentReconCo1Ldc1 = "CPPAYMENTSRECONLDC1-PROD-CO1C";
            public const string PaymentReconCo1Ldc2 = "CPPAYMENTSRECONLDC2-PROD-CO1C";
            public const string PaymentReconDm2Ldc1 = "CPPAYMENTSRECONLDC1-PROD-DM2C";
            public const string PaymentReconDm2Ldc2 = "CPPAYMENTSRECONLDC2-PROD-DM2C";
            public const string PXDm2c = "CPPAYMENTEXPERIENCESERVICE-PROD-DM2C";
            public const string PXCo1c = "CPPAYMENTEXPERIENCESERVICE-PROD-CO1C";
            public const string PXCo4 = "CPPAYMENTEXPERIENCESERVICE-INT-CO4";
            public const string PIFDPpeBn2 = "CPPAYMENTSPIFD-PPE-BN2";
            public const string PIFDProdCy2 = "CPPAYMENTSPIFD-PROD-CY2";
            public const string PIFDProdBn2 = "CPPAYMENTSPIFD-PROD-BN2";
            public const string PIFDProdHk2 = "CPPAYMENTSPIFD-PROD-HK2";
            public const string PIFDProdDb5 = "CPPAYMENTSPIFD-PROD-DB5";
            public const string PIFDProdPFDm2p = "CPPAYMENTSPIFDPF-PROD-DM2P";
            public const string PIFDProdPFMw1p = "CPPAYMENTSPIFDPF-PROD-MW1P";
            public const string PMDCo1Ldc1 = "CPPAYMENTSPMDLDC1-PROD-CO1C";
            public const string PMDCo1Ldc2 = "CPPAYMENTSPMDLDC2-PROD-CO1C";
            public const string PMDDm2Ldc1 = "CPPAYMENTSPMDLDC1-PROD-DM2C";
            public const string PMDDm2Ldc2 = "CPPAYMENTSPMDLDC2-PROD-DM2C";
        }

        public static class Integration
        {
            public const string PaymentTestCo4 = "CPPAYMENTS-TEST-CO4";
            public const string PaymentPimsIntCo4 = "CPPAYMENTSPIMS-INT-CO4";
            public const string PaymentDevCo4 = "CPPAYMENTS-DEV-CO4";
            public const string PaymentSqlDevCo3 = "PAYMENTSQL-DEV-CO3";
            public const string PaymentReconTestCo4 = "CPPAYMENTSRECON-TEST-CO4";
            public const string PaymentReconIntCo4 = "CPPAYMENTSRECON-INT-CO4";
            public const string PaymentPimsDevCo4 = "CPPAYMENTSPIMS-DEV-CO4";
            public const string PXTestCo4 = "CPPAYMENTEXPERIENCESERVICE-TEST-CO4";
            public const string PifdIntCo4 = "CPPAYMENTSPIFD-INT-CO4";
            public const string PMDIntCo4 = "CPPAYMENTSPMD-INT-CO4";
        }

        public static class OneBox
        {
            public const string PaymentOnebox = "ONEBOX";
        }
    }
}
