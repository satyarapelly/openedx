// <copyright file="StringCompressHelper.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Sql
{
    using System.IO;
    using System.IO.Compression;
    using System.Text;

    public static class StringCompressHelper
    {
        private const int BufferSize = 4096;

        public static string GZipDecompress(byte[] input)
        {
            using (MemoryStream sourceStream = new MemoryStream(input))
            {
                using (GZipStream decompressionStream = new GZipStream(sourceStream, CompressionMode.Decompress, true))
                {
                    using (StreamReader decompressionStreamReader = new StreamReader(decompressionStream, Encoding.UTF8, true, BufferSize, true))
                    {
                        return decompressionStreamReader.ReadToEnd();
                    }
                }
            }
        }

        public static byte[] GZipCompress(string input)
        {
            using (MemoryStream targetStream = new MemoryStream())
            {
                using (GZipStream compressionStream = new GZipStream(targetStream, CompressionMode.Compress, true))
                {
                    using (StreamWriter compressionStreamWriter = new StreamWriter(compressionStream, Encoding.UTF8, BufferSize, true))
                    {
                        compressionStreamWriter.Write(input);
                    }
                }

                return targetStream.ToArray();
            }
        }
    }
}
