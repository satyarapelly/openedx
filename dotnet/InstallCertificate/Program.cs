// <copyright file="Program.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tools.InstallCertificates
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using Common.Tracing;
    using Tracing;

    public class Program
    {
        private const string ArgInstall = "install";
        private const string ArgVerify = "verify";
        private const string ArgInstallIssuersFromFile = "installIssuersFromFile";
        private const string ArgInstallIssuersFromChain = "installIssuersFromChain";
        private const string ArgGrantAccess = "grantAccess";
        private const string ArgCertsDir = "certsDir:";

        private static string containerName;
        private static bool install;
        private static bool installIssuersFromFile;
        private static bool verify;
        private static bool installIssuersFromChain;
        private static bool grantAccess;
        private static string certsDir;

        public static int Main(string[] args)
        {
            bool retVal = true;
            try
            {
                Trace.Listeners.Add(new ConsoleTraceListener());
                EventTraceActivity traceActivityId = new EventTraceActivity();
                if (ProcessInputArgs(args) == false)
                {
                    ShowHelp();
                    return -1;
                }

                CertificateContainer container = new CertificateContainer(
                    certsDir: certsDir,
                    containerName: containerName,
                    traceActivityId: traceActivityId);

                container.Initialize();

                if (install)
                {
                    retVal = retVal && container.InstallCertificate();
                }

                if (installIssuersFromFile)
                {
                    retVal = retVal && container.InstallIssuersFromFile();
                }

                if (verify)
                {
                    retVal = retVal && container.VerifyCertificate();
                }

                if (installIssuersFromChain)
                {
                    retVal = retVal && container.InstallIssuersFromChain();
                }

                if (grantAccess)
                {
                    retVal = retVal && container.GrantNetworkServiceRead();
                    retVal = retVal && container.LogKeyFilePermissions();
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Exception: {0}", ex);
                return 1;
            }
            
            return retVal ? 0 : 1;
        }

        public static bool ProcessInputArgs(string[] inputArgs)
        {
            // Trim leading/trailing spaces and remove supported prefixes
            char[] prefixesToTrim = new char[] { '/', '-' };
            for (int i = 0; i < inputArgs.Length; i++)
            {
                inputArgs[i] = inputArgs[i].Trim().TrimStart(prefixesToTrim);
            }

            string[] helpArgs = new string[] { "?", "help" };
            if (inputArgs.Length == 0 || inputArgs.Intersect(helpArgs).Any())
            {
                return false;
            }

            containerName = inputArgs[0];
            for (int i = 1; i < inputArgs.Length; i++)
            {
                if (inputArgs[i].Equals(ArgInstall, StringComparison.OrdinalIgnoreCase))
                {
                    install = true;
                }
                else if (inputArgs[i].Equals(ArgVerify, StringComparison.OrdinalIgnoreCase))
                {
                    verify = true;
                }
                else if (inputArgs[i].Equals(ArgInstallIssuersFromFile, StringComparison.OrdinalIgnoreCase))
                {
                    installIssuersFromFile = true;
                }
                else if (inputArgs[i].Equals(ArgInstallIssuersFromChain, StringComparison.OrdinalIgnoreCase))
                {
                    installIssuersFromChain = true;
                }
                else if (inputArgs[i].Equals(ArgGrantAccess, StringComparison.OrdinalIgnoreCase))
                {
                    grantAccess = true;
                }
                else if (inputArgs[i].StartsWith(ArgCertsDir, StringComparison.OrdinalIgnoreCase))
                {
                    certsDir = inputArgs[i].Substring(ArgCertsDir.Length);
                }
                else
                {
                    Logger.Log("Unknown parameter: {0}", inputArgs[i]);
                    return false;
                }
            }

            return true;
        }

        private static void ShowHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("InstallCertificates.exe <fileName> [-{0}] [-{1}] [-{2}] [-{3}] [-{4}<certsDir>]", ArgInstall, ArgInstallIssuersFromFile, ArgVerify, ArgInstallIssuersFromChain, ArgGrantAccess, ArgCertsDir);
            Console.WriteLine("  <fileName>                Name of the certificate file.  Supported names in PROD are:");
            var prodSettings = new InstallCertificatesProdSettings();
            foreach (string certName in prodSettings.CertificateFiles.Keys)
            {
                Console.WriteLine("   {0,-26} {1}", " ", certName);
            }

            Console.WriteLine("  -{0,-24} Installs the certificate in the specified file.", ArgInstall);
            Console.WriteLine("  -{0,-24} Installs issuer certificates from the specified file.", ArgInstallIssuersFromFile);
            Console.WriteLine("  -{0,-24} Verifies the certificate chain and logs reasons for any failures.", ArgVerify);
            Console.WriteLine("  -{0,-24} Installs issuer certificates from the certificate chain.", ArgInstallIssuersFromChain);
            Console.WriteLine("  -{0,-24} Grant read access to NETWORK SERVICE.", ArgGrantAccess);
        }
    }
}