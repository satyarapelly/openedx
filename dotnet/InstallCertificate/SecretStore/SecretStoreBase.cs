// <copyright file="SecretStoreBase.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tools.InstallCertificates
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Tracing;
    
    public abstract class SecretStoreBase : ISecretStore
    {
        private static ComponentBasedKeyValueSet keyValueStore = null;

        private readonly SecretStoreSettings secretStoreSettings;

        private ReaderWriterLockSlim keyValueStoreLock = new ReaderWriterLockSlim();

        public SecretStoreBase(SecretStoreSettings settings)
        {
            this.secretStoreSettings = settings;
        }

        public abstract Task<byte[]> ReadFileBytes(string fileName, EventTraceActivity traceActivityId);

        public virtual async Task<string> ReadFileAsText(string fileName, EventTraceActivity traceActivityId)
        {
            byte[] fileBuffer = await this.ReadFileBytes(fileName, traceActivityId);
            using (MemoryStream memoryStream = new MemoryStream(fileBuffer))
            {
                using (StreamReader streamReader = new StreamReader(memoryStream))
                {
                    return await streamReader.ReadToEndAsync();
                }
            }
        }

        public virtual Task<string> GetValue(string componentName, string key, EventTraceActivity traceActivityId)
        {
            if (keyValueStore == null)
            {
                this.keyValueStoreLock.EnterWriteLock();
                try
                {
                    if (keyValueStore == null)
                    {
                        byte[] decryptedKeyValueData = this.ReadFileBytes(this.secretStoreSettings.KeyValueSecretFileName, traceActivityId).Result;
                        if (decryptedKeyValueData == null)
                        {
                            string message = string.Format("Could not Decrypt key value secret file:{0}", this.secretStoreSettings.KeyValueSecretFileName);
                            throw TraceCore.TraceException<SecretStoreException>(traceActivityId, new SecretStoreException(message)); 
                        }

                        ComponentBasedKeyValueSet newKeyValueStore = null;
                        try
                        {
                            using (MemoryStream memoryStream = new MemoryStream(decryptedKeyValueData))
                            {
                                using (StreamReader streamReader = new StreamReader(memoryStream))
                                {
                                    newKeyValueStore = ComponentBasedKeyValueSet.Deserialize(streamReader.ReadToEnd());
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            string message = string.Format("Error deserializing key value date, exception message:{0}, stack trace:{1}", e.Message, e.StackTrace);
                            throw TraceCore.TraceException<SecretStoreException>(traceActivityId, new SecretStoreException(message, e));
                        }

                        if (newKeyValueStore == null)
                        {
                            string message = string.Format("Could not Find key {0} in key value secret file:", this.secretStoreSettings.KeyValueSecretFileName);
                            throw TraceCore.TraceException<SecretStoreException>(traceActivityId, new SecretStoreException(message));
                        }

                        keyValueStore = newKeyValueStore;
                    }
                }
                finally
                {
                    if (this.keyValueStoreLock.IsWriteLockHeld)
                    {
                        this.keyValueStoreLock.ExitWriteLock();
                    }
                }
            }

            string value;
            this.keyValueStoreLock.EnterReadLock();
            try
            {
                if (!keyValueStore.TryGetValue(componentName, key, out value))
                {
                    string message = string.Format("Could not Find key {0} in key value secret file:{1}", key, this.secretStoreSettings.KeyValueSecretFileName);
                    throw TraceCore.TraceException<SecretStoreException>(traceActivityId, new SecretStoreException(message));
                }
            }
            finally
            {
                if (this.keyValueStoreLock.IsReadLockHeld)
                {
                    this.keyValueStoreLock.ExitReadLock();
                }
            }

            return Task.FromResult<string>(value);
        }
    }
}