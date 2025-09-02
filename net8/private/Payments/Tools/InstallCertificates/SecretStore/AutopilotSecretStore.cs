// <copyright file="AutopilotSecretStore.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tools.InstallCertificates
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Helper;
    using Microsoft.Commerce.Payments.Common.Tracing;
    public class AutopilotSecretStore : SecretStoreBase
    {
        public AutopilotSecretStore(SecretStoreSettings settings)
            : base(settings)
        {
        }

        public override async Task<byte[]> ReadFileBytes(string fileName, EventTraceActivity traceActivityId)
        {
            if (Environment.Current.EnvironmentType == EnvironmentType.OneBox)
            {
                throw TraceCore.TraceException<SecretStoreException>(
                    traceActivityId,
                    new SecretStoreException("Secret Store usage not supported: Attempting to use Autopilot Secret Store on OneBox environment"));
            }

            // Replace APruntime.DataDirectory with a standard location
            string baseDataPath = AppContext.BaseDirectory;
            string fullPathFileName = Path.Combine(baseDataPath, "data", Environment.Current.PaymentsDataFolderName, fileName);

            if (!File.Exists(fullPathFileName))
            {
                string message = $"File not found: {fullPathFileName}";
                throw TraceCore.TraceException<SecretStoreException>(
                    traceActivityId,
                    new SecretStoreException(message));
            }

            byte[] encryptedBytes = await FileHelper.ReadAllBytesAsync(fullPathFileName).ConfigureAwait(false);

            // NOTE: Decryption is currently stubbed out
            return Decrypt(encryptedBytes, traceActivityId);
        }

        private static byte[] Decrypt(byte[] encryptedBytes, EventTraceActivity traceActivityId)
        {
            try
            {
                // TODO: Replace this stub with real decryption logic if needed
                return encryptedBytes;
            }
            catch (Exception ex)
            {
                throw TraceCore.TraceException<SecretStoreException>(
                    traceActivityId,
                    new SecretStoreException("Exception occurred during decryption", ex));
            }
        }
    }
}
