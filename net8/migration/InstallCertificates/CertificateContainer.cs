// <copyright file="CertificateContainer.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tools.InstallCertificates
{
    using System;
    using System.IO;
    using System.Security.AccessControl;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using SE = System.Environment;

    public class CertificateContainer
    {
        private EventTraceActivity traceActivityId;

        // These are the raw bytes read from the container file
        private byte[] data;

        // This is the password read from the password file
        private string password;

        // This is the name used in Settings and maps to a container file (either a .cer or a .pfx)
        // and a password file if applicable.
        private string name;

        // The directory that contains certs to be installed.  To reduce risk of regression, this is required and honored 
        // only for OneBox environment.  For all other environments, since the certsDir is fixed, they are hardcoded in 
        // the corresponding settings files.
        private string certsDir;

        private X509Certificate2 certificate;
        private X509Chain chain;

        public CertificateContainer(string certsDir, string containerName, EventTraceActivity traceActivityId)
        {
            this.certsDir = certsDir;
            this.name = containerName;
            this.traceActivityId = traceActivityId;
        }

        public void Initialize()
        {
            const string FunctionName = "Initialize";
            Logger.LogBegin(FunctionName);

            Logger.Log("Container name: {0}", this.name);
            InstallCertificatesSettings settings = InstallCertificatesSettings.Create(this.certsDir, this.traceActivityId);

            // retrieve the password from the password file
            this.password = settings.GetPassword(this.name, this.traceActivityId);
            Logger.Log("Password: {0}", this.password == null ? "is null" : "was read successfully");

            // retrieve the certificate from the certificate file
            this.data = settings.GetCertificateData(this.name, this.traceActivityId);
            Logger.Log("Data: {0}", this.data == null ? "is null" : "was read successfully");

            this.certificate = this.password != null ?
                new X509Certificate2(this.data, this.password, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet) :
                new X509Certificate2(this.data);

            Logger.Log("Certificate SimpleName: {0}", this.certificate.GetNameInfo(X509NameType.SimpleName, false));
            Logger.Log("Certificate Thumbprint: {0}", this.certificate.Thumbprint);
            Logger.Log("Certificate PrivateKey: {0}", this.certificate.HasPrivateKey);
            Logger.Log("Issuer SimpleName: {0}", this.certificate.GetNameInfo(X509NameType.SimpleName, true));

            Logger.LogEnd(FunctionName);
        }

        public bool InstallCertificate()
        {
            return InstallCertificate(this.certificate, this.password, true);
        }

        public bool InstallIssuersFromFile()
        {
            bool retVal = true;
            const string FunctionName = "InstallIssuersFromFile";
            Logger.LogBegin(FunctionName);

            try
            {
                X509Certificate2Collection certs = new X509Certificate2Collection();
                Logger.Log("Importing data into certificate collection");
                if (this.password == null)
                {
                    certs.Import(this.data);
                }
                else
                {
                    certs.Import(this.data, this.password, X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.MachineKeySet);
                }

                Logger.Log("Certificates count: {0}", certs.Count);
                foreach (var cert in certs)
                {
                    if (this.IsLeafCert(cert))
                    {
                        Logger.Log("Skipping leaf certificate");
                        continue;
                    }

                    retVal = retVal && InstallCertificate(cert);
                }
            }
            catch (Exception ex)
            {
                retVal = false;
                Logger.Log("Exception: {0}", ex);
            }
            finally
            {
                Logger.LogEnd(FunctionName);
            }

            return retVal;
        }

        public bool VerifyCertificate()
        {
            bool retVal = true;
            const string FunctionName = "VerifyCertificate";
            Logger.LogBegin(FunctionName);

            try
            {
                this.chain = new X509Chain();
                bool isValid = this.chain.Build(this.certificate);
                Logger.Log("Chain is valid: {0}", isValid);
                Logger.Log("Chain elements count: {0}", this.chain.ChainElements.Count);

                if (isValid == false)
                {
                    Logger.Log("Chain status count: {0}", this.chain.ChainStatus.Length);
                    foreach (X509ChainStatus chainStatus in this.chain.ChainStatus)
                    {
                        Logger.Log("Status: {0}", chainStatus.Status);
                        Logger.Log("StatusInformation: {0}", chainStatus.StatusInformation);
                    }
                }
            }
            catch (Exception ex)
            {
                retVal = false;
                Logger.Log("Exception: {0}", ex);
            }
            finally
            {
                Logger.LogEnd(FunctionName);
            }

            return retVal;
        }

        public bool InstallIssuersFromChain()
        {
            bool retVal = true;
            const string FunctionName = "InstallIssuersFromChain";
            Logger.LogBegin(FunctionName);

            try
            {
                if (this.chain == null)
                {
                    this.chain = new X509Chain();
                    bool isValid = this.chain.Build(this.certificate);
                    Logger.Log("Chain is valid: {0}", isValid);
                }

                Logger.Log("Chain elements count: {0}", this.chain.ChainElements.Count);
                foreach (X509ChainElement chainElement in this.chain.ChainElements)
                {
                    if (this.IsLeafCert(chainElement.Certificate))
                    {
                        Logger.Log("Skipping leaf certificate");
                        continue;
                    }

                    retVal = retVal && InstallCertificate(chainElement.Certificate);
                }
            }
            catch (Exception ex)
            {
                retVal = false;
                Logger.Log("Exception: {0}", ex);
            }
            finally
            {
                Logger.LogEnd(FunctionName);
            }

            return retVal;
        }

        public bool GrantNetworkServiceRead()
        {
            bool retVal = true;
            const string FunctionName = "GrantNetworkServiceRead";
            Logger.LogBegin(FunctionName);
            RSACryptoServiceProvider rsa = null;

            try
            {
                rsa = this.certificate.PrivateKey as RSACryptoServiceProvider;
                if (rsa == null)
                {
                    Logger.Log("Could not cast private key to RSACryptoServiceProvider");
                    return false;
                }

                Logger.Log("Creating CspParameters");
                var cspParams = new CspParameters(rsa.CspKeyContainerInfo.ProviderType, rsa.CspKeyContainerInfo.ProviderName, rsa.CspKeyContainerInfo.KeyContainerName)
                {
                    Flags = CspProviderFlags.UseExistingKey | CspProviderFlags.UseMachineKeyStore,
                    CryptoKeySecurity = rsa.CspKeyContainerInfo.CryptoKeySecurity
                };

                Logger.Log("Creating security identifier and adding access rule");
                var networkServiceSid = new SecurityIdentifier(WellKnownSidType.NetworkServiceSid, null);
                cspParams.CryptoKeySecurity.AddAccessRule(new CryptoKeyAccessRule(networkServiceSid, CryptoKeyRights.GenericRead, AccessControlType.Allow));

                Logger.Log("Creating an instance of RSACryptoServiceProvider");
                using (var rsa2 = new RSACryptoServiceProvider(2048, cspParams))
                {
                    // Creating a provider instance is necessary to presist ACL changes
                }
            }
            catch (Exception ex)
            {
                retVal = false;
                Logger.Log("Exception: {0}", ex);
            }
            finally
            {
                if (rsa != null)
                {
                    rsa.Dispose();
                }

                Logger.LogEnd(FunctionName);
            }

            return retVal;
        }

        // Log permissions of a give key by looking at ACLs of the underlying key file
        public bool LogKeyFilePermissions()
        {
            bool retVal = true;
            const string FunctionName = "LogKeyFilePermissions";
            Logger.LogBegin(FunctionName);
            RSACryptoServiceProvider rsa = null;

            try
            {
                rsa = this.certificate.PrivateKey as RSACryptoServiceProvider;
                if (rsa == null)
                {
                    Logger.Log("Could not cast private key to RSACryptoServiceProvider");
                    return false;
                }

                string keyFileName = rsa.CspKeyContainerInfo.UniqueKeyContainerName;
                Logger.Log("Key file name: {0}", keyFileName);

                string rsaKeysDir = Path.Combine(SE.GetFolderPath(SE.SpecialFolder.CommonApplicationData), @"Microsoft\Crypto\RSA\MachineKeys");
                string[] rsaKeyFiles = Directory.GetFiles(rsaKeysDir, keyFileName, SearchOption.TopDirectoryOnly);
                if (rsaKeyFiles.Length == 0)
                {
                    rsaKeysDir = Path.Combine(SE.GetFolderPath(SE.SpecialFolder.ApplicationData), @"Microsoft\Crypto\RSA");
                    rsaKeyFiles = Directory.GetFiles(rsaKeysDir, keyFileName, SearchOption.AllDirectories);
                }

                Logger.Log("Key files directory: {0}", rsaKeysDir);
                Logger.Log("Key files count: {0}", rsaKeyFiles.Length);

                if (rsaKeyFiles.Length == 0)
                {
                    return false;
                }

                foreach (string rsaKeyFile in rsaKeyFiles)
                {
                    Logger.Log("Getting ACLs for key file: {0}", rsaKeyFile);
                    FileInfo file = new FileInfo(rsaKeyFile);
                    FileSecurity fileSecurity = file.GetAccessControl();
                    var accessRules = fileSecurity.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount));
                    Logger.Log("ACLs count: {0}", accessRules.Count);
                    foreach (FileSystemAccessRule accessRule in accessRules)
                    {
                        Logger.Log("{0,-30}{1,-8}{2}", accessRule.IdentityReference.Value, accessRule.AccessControlType, accessRule.FileSystemRights);
                    }
                }
            }
            catch (Exception ex)
            {
                retVal = false;
                Logger.Log("Exception: {0}", ex);
            }
            finally
            {
                if (rsa != null)
                {
                    rsa.Dispose();
                }

                Logger.LogEnd(FunctionName);
            }

            return retVal;
        }

        private static bool InstallCertificate(X509Certificate2 certToInstall, string password = null, bool removeExisting = false)
        {
            bool retVal = true;
            const string FunctionName = "InstallCertificate";
            Logger.LogBegin(FunctionName);

            X509Store localStore = null;
            try
            {
                Logger.Log("SimpleName: {0}", certToInstall.GetNameInfo(X509NameType.SimpleName, false));
                Logger.Log("Thumbprint: {0}", certToInstall.Thumbprint);

                bool isMyCert = password != null && certToInstall.HasPrivateKey;
                bool isRootCert = string.Equals(certToInstall.Issuer, certToInstall.Subject, StringComparison.OrdinalIgnoreCase);
                StoreName storeName = isMyCert ? StoreName.My : isRootCert ? StoreName.Root : StoreName.CertificateAuthority;
                Logger.Log("Store name: {0}", storeName);

                localStore = new X509Store(storeName, StoreLocation.LocalMachine);
                localStore.Open(OpenFlags.ReadWrite);

                string simpleName = certToInstall.GetNameInfo(X509NameType.SimpleName, false);
                X509Certificate2Collection existingCerts = localStore.Certificates.Find(X509FindType.FindBySubjectName, simpleName, true);
                if (existingCerts != null && existingCerts.Count > 0)
                {
                    Logger.Log("Existing certificates count: {0}", existingCerts.Count);
                    foreach (X509Certificate2 existingCert in existingCerts)
                    {
                        Logger.Log("Existing certificate simplename: {0}", existingCert.GetNameInfo(X509NameType.SimpleName, false));
                        Logger.Log("Existing certificate thumbprint: {0}", existingCert.Thumbprint);

                        if (removeExisting)
                        {
                            Logger.Log("Removing existing certificate");
                            localStore.Remove(existingCert);
                        }
                    }
                }

                if (!localStore.Certificates.Contains(certToInstall))
                {
                    Logger.Log("Adding certficate");
                    localStore.Add(certToInstall);
                }
            }
            catch (Exception ex)
            {
                retVal = false;
                Logger.Log("Exception: {0}", ex);
            }
            finally
            {
                if (localStore != null)
                {
                    // Based on testing, Close() can be called safely even if the store 
                    // was not opened successfully. (There is no documentation around this on msdn.
                    // also, there is no .IsOpen function to check)
                    localStore.Close();
                }

                Logger.LogEnd(FunctionName);
            }

            return retVal;
        }

        private bool IsLeafCert(X509Certificate2 cert)
        {
            bool retVal = false;
            if (string.Equals(cert.SubjectName.Name, this.certificate.SubjectName.Name, StringComparison.OrdinalIgnoreCase))
            {
                retVal = true;
            }

            return retVal;
        }
    }
}
