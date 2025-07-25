// <copyright file="InstallCertificatesProdSettings.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tools.InstallCertificates
{
    using System.Collections.Generic;
    using System.Security.Cryptography.X509Certificates;

    internal class InstallCertificatesProdSettings : InstallCertificatesSettings
    {
        public InstallCertificatesProdSettings()
        {
            const string BaseDir = @"d:\data\payments\certificate\";

            // all certificate friendly names must be lower case for comparison purposes
            this.CertificateFiles = new Dictionary<string, string>
                {
                    { "flighttowerpxclientauth",            BaseDir + "flighttower-pxclientauth-prod.cp.microsoft.com_20211002.pfx.encr" },
                    { "pxpxclientauth",                     BaseDir + "px-pxclientauth.cp.microsoft.com_20210730.pfx.encr" },
                    { "aadpxclientauth",                    BaseDir + "aad-pxclientauth.cp.microsoft.com_20211127.pfx.encr" },
                    { "ctpcommerceservicepxclientauth",     BaseDir + "ctpcommerce-pxclientauth-prod.cp.microsoft.com.pfx.encr" }
               };
        }
    }
}