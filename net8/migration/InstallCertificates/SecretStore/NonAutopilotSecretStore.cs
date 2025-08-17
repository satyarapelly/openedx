// <copyright file="NonAutopilotSecretStore.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tools.InstallCertificates
{
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Helper;
    using Microsoft.Commerce.Payments.Common.Tracing;
   
    public class NonAutopilotSecretStore : SecretStoreBase
    {
        public NonAutopilotSecretStore(SecretStoreSettings settings)
            : base(settings)
        {
        }
        
        public override Task<byte[]> ReadFileBytes(string fileName, EventTraceActivity traceActivityId)
        {
            if (!File.Exists(fileName))
            {
                string message = string.Format("File:{0} doesnt exist", fileName);
                throw TraceCore.TraceException<SecretStoreException>(traceActivityId, new SecretStoreException(message));
            }

            return FileHelper.ReadAllBytesAsync(fileName);
        }
    }
}
