// <copyright file="InstallCertificatesSettings.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tools.InstallCertificates
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using Common;
    using Common.Environments;
    using Common.Tracing;

    internal abstract class InstallCertificatesSettings
    {
        public Dictionary<string, string> CertificateFiles { get; set; }

        public static InstallCertificatesSettings Create(string certsDir, EventTraceActivity traceActivityId)
        {
            EnvironmentType environmentType = Environment.Current.EnvironmentType;
            string environmentName = Environment.Current.EnvironmentName;

            Logger.Log("Environment: {0}", environmentName);

            switch (environmentType)
            {
                case EnvironmentType.OneBox:
                    return new InstallCertificatesOneBoxSettings(certsDir);
                case EnvironmentType.Integration:
                    return new InstallCertificatesIntSettings();
                case EnvironmentType.Production:
                    return new InstallCertificatesProdSettings();
                default:
                    throw TraceCore.TraceException(
                        traceActivityId,
                        new NotSupportedException(string.Format("Environment type: '{0}' is not supported.", environmentType)));
            }
        }

        public string GetPassword(string certificateName, EventTraceActivity traceActivityId)
        {
            string password = string.Empty;
            string passwordFileName = this.GetPasswordFileName(certificateName, traceActivityId);
            if (!string.IsNullOrWhiteSpace(passwordFileName))
            {
                password = Encoding.UTF8.GetString(ReadFileBytes(passwordFileName, traceActivityId));
                password = Regex.Match(password, @"^SSLImportKey\s*=(.*)$", RegexOptions.Multiline).Groups[1].Value.TrimEnd();
            }

            return password;
        }

        public byte[] GetCertificateData(string certificateName, EventTraceActivity traceActivityId)
        {
            string certificateFileName = this.GetCertificateFileName(certificateName, traceActivityId);
            byte[] certificateData = ReadFileBytes(certificateFileName, traceActivityId);

            return certificateData;
        }

        [SuppressMessage("Microsoft.Performance", "CA1801:ReviewUnusedParameters", MessageId = "traceActivityId", Justification = "Not used yet")]
        protected virtual string GetPasswordFileName(string certificateName, EventTraceActivity traceActivityId)
        {
            certificateName = certificateName.ToLowerInvariant();
            if (this.CertificateFiles != null && this.CertificateFiles.ContainsKey(certificateName))
            {
                string certificateFile = this.CertificateFiles[certificateName];
                string ext = Path.GetExtension(certificateFile);
                string file = Path.GetFileNameWithoutExtension(certificateFile);

                // if the extension is .encr then remove the real extension as well
                if (string.Equals(ext, ".encr", StringComparison.InvariantCultureIgnoreCase))
                {
                    ext = Path.GetExtension(file);
                    file = Path.GetFileNameWithoutExtension(file);
                }

                if (string.Equals(ext, ".pfx"))
                {
                    return Path.Combine(Path.GetDirectoryName(certificateFile), file) + ".dat.encr";
                }
            }

            return null;
        }

        private static byte[] ReadFileBytes(string file, EventTraceActivity traceActivityId)
        {
            ISecretStore secretStore = Environment.Current.SecretStore;
            Logger.Log("Read file: {0}", file);
            if (string.Equals(Path.GetExtension(file), ".encr", StringComparison.InvariantCultureIgnoreCase))
            {
                return secretStore.ReadFileBytes(file, traceActivityId).Result;
            }
            else
            {
                return File.ReadAllBytes(file);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1801:ReviewUnusedParameters", MessageId = "traceActivityId", Justification = "Not used yet")]
        private string GetCertificateFileName(string certificateName, EventTraceActivity traceActivityId)
        {
            certificateName = certificateName.ToLowerInvariant();
            if (this.CertificateFiles != null && this.CertificateFiles.ContainsKey(certificateName))
            {
                return this.CertificateFiles[certificateName];
            }

            return null;
        }
    }
}