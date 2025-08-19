namespace Microsoft.Search.Autopilot
{
    using System.IO;

    public static class APRuntime
    {
        public static bool IsInitialized { get; private set; }

        public static string DataDirectory { get; private set; } = ".";

        public static void Initialize(string configPath)
        {
            IsInitialized = true;
            var directory = Path.GetDirectoryName(configPath);
            if (!string.IsNullOrEmpty(directory))
            {
                DataDirectory = directory;
            }
        }

        public static void SetCounterFile(string counterFile)
        {
            // Intentionally left blank.
        }
    }
}

namespace Microsoft.Search.Autopilot.Security
{
    using System;

    public sealed class ApSecretProtection : IDisposable
    {
        public byte[] Decrypt(byte[] data)
        {
            return data;
        }

        public void Dispose()
        {
        }
    }
}
