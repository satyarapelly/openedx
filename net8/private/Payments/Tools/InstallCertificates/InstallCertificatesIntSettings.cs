// <copyright file="InstallCertificatesIntSettings.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tools.InstallCertificates
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography.X509Certificates;
    using Common;
    using Common.Environments;
    using Common.Tracing;

    internal class InstallCertificatesIntSettings : InstallCertificatesSettings
    {
        public InstallCertificatesIntSettings()
        {
            const string BaseDir = @"d:\data\payments\certificate\";

            // all certificate friendly names must be lower case for comparison purposes
            this.CertificateFiles = new Dictionary<string, string>
                {
                    { "accountspxclientauth",               BaseDir + "accounts-pxclientauth.cp.microsoft-int.com_20210907.pfx.encr" },
                    { "pimspxclientauth",                   BaseDir + "pims-pxclientauth-int.microsoft-int.com_20210716.pfx.encr" },
                    { "taxidpxclientauth",                  BaseDir + "taxid-pxclientauth.microsoft-int.com_20210922.pfx.encr" },
                    { "riskpxclientauth",                   BaseDir + "risk-pxclientauth.microsoft-int.com_20210914.pfx.encr" },
                    { "flighttowerpxclientauth",            BaseDir + "flighttower-pxclientauth.microsoft-int.com_20210907.pfx.encr" },
                    { "pxpxclientauth",                     BaseDir + "px-pxclientauth.microsoft-int.com_20210730.pfx.encr" },
                    { "aadpxclientauth",                    BaseDir + "aad-pxclientauth-int.cp.microsoft.com_20211127.pfx.encr" },
                    { "sessionservicepxclientauth",         BaseDir + "sessionservice-pxclientauth.microsoft-int.com_20220504.pfx.encr" },
                    { "ctpcommerceservicepxclientauth",     BaseDir + "ctpcommerce-pxclientauth-int.cp.microsoft-int.com.pfx.encr" },
                    { "payerauthservicepxclientauth",       BaseDir + "payerauthservice-pxclientauth-int.cp.microsoft.com_13Nov2020.pfx.encr" },
                    { "storedvaluepxclientauth",            BaseDir + "storedvalue-pxclientauth-int.cp.microsoft-int.com_20220131.pfx.encr" }
                };
        }
    }
}