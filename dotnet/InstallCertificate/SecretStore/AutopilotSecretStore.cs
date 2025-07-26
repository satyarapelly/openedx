// <copyright file="AutopilotSecretStore.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tools.InstallCertificates
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Helper;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Tracing;
    using Microsoft.Search.Autopilot;
    using Microsoft.Search.Autopilot.Security;
    
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
                throw TraceCore.TraceException<SecretStoreException>(traceActivityId, new SecretStoreException("Secret Store usage not supported: Attempting to use Autopilot Secret Store on One box"));
            }
                        
            string fullPathFileName = Path.Combine(APRuntime.DataDirectory, Environment.Current.PaymentsDataFolderName, fileName);
            if (File.Exists(fullPathFileName))
            {
                byte[] encryptedBytes = await FileHelper.ReadAllBytesAsync(fullPathFileName).ConfigureAwait(false);
                return AutopilotSecretStore.Decrypt(encryptedBytes, traceActivityId);
            }
            else
            {
                string message = string.Format("File:{0} doesnt exist", fullPathFileName);
                throw TraceCore.TraceException<SecretStoreException>(traceActivityId, new SecretStoreException(message));
            }
        }

        private static byte[] Decrypt(byte[] encryptedBytes, EventTraceActivity traceActivityId)
        {
            try
            {
                using (ApSecretProtection autopilotSecretStore = new ApSecretProtection())
                {
                    return autopilotSecretStore.Decrypt(encryptedBytes);
                }
            }
            catch (Exception ex)
            {
                throw TraceCore.TraceException<SecretStoreException>(traceActivityId, new SecretStoreException("Got exception in decrypt", ex));
            }
        }
    }
}
