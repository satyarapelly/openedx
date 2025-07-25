// <copyright file="FileHelper.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Helper
{
    using System.IO;
    using System.Threading.Tasks;

    internal static class FileHelper
    {
        public static async Task<byte[]> ReadAllBytesAsync(string fileName)
        {
            using (FileStream fileStream = File.OpenRead(fileName))
            {
                byte[] buffer = new byte[fileStream.Length];
                await fileStream.ReadAsync(buffer, 0, (int)fileStream.Length).ConfigureAwait(false);
                return buffer;
            }
        }
    }
}
