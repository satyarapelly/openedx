// <copyright file="InstallCertificatesOneBoxSettings.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tools.InstallCertificates
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using Microsoft.Commerce.Payments.Common.Tracing;

    internal class InstallCertificatesOneBoxSettings : InstallCertificatesSettings
    {
        public InstallCertificatesOneBoxSettings(string baseDir)
        {
            // all certificate friendly names must be lower case for comparison purposes
            this.CertificateFiles = new Dictionary<string, string>
                {
                    { "pxtestpxclientauth",                 baseDir + "px-testkv-int-pxtest-pxclientauth-paymentexperience-azclient-int-ms-20230313.pfx" },
                    { "aadpxclientauth",                    baseDir + "px-kv-int-aad-pxclientauth-paymentexperience-azclient-int-ms-20240102.pfx" },
                    { "ctpcommerceservicepxclientauth",     baseDir + "px-kv-int-ctpcommerce-pxclientauth-paymentexperience-azclient-int-ms-20240102.pfx" },
                    { "pxpxclientauth",                     baseDir + "px-kv-int-px-pxclientauth-paymentexperience-azclient-int-ms-20240102.pfx" }
                };
        }

        [SuppressMessage("Microsoft.Performance", "CA1801:ReviewUnusedParameters", MessageId = "traceActivityId", Justification = "Not used yet")]
        protected override string GetPasswordFileName(string certificateName, EventTraceActivity traceActivityId)
        {
            certificateName = certificateName.ToLowerInvariant();
            if (this.CertificateFiles != null && this.CertificateFiles.ContainsKey(certificateName))
            {
                string certificateFile = this.CertificateFiles[certificateName];
                string ext = Path.GetExtension(certificateFile);
                string file = Path.GetFileNameWithoutExtension(certificateFile);

                if (string.Equals(ext, ".pfx"))
                {
                    return Path.Combine(Path.GetDirectoryName(certificateFile), file) + ".dat";
                }
                else
                {
                    throw new InvalidDataException("Invalid InstallCertificatesOneBoxSettings data.  On OneBox, cert files are expected to have a .pfx extension.");
                }
            }

            return null;
        }
    }
}