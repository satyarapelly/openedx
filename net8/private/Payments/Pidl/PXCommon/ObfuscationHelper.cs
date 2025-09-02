// <copyright file="ObfuscationHelper.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXCommon
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    public static class ObfuscationHelper
    {
        public static Guid JarvisAccountIdHashSalt
        {
            get
            {
                return new Guid("DDE5AF1A-C088-4F57-93A7-67245D9FD3B3");
            }
        }

        /// <summary>
        /// Get hash value for the given string and hash/guid salt
        /// </summary>
        /// <param name="strInput">Input string to be converted</param>
        /// <param name="hashSalt">Hash/Guid salt</param>
        /// <returns>Hash value</returns>
        public static string GetHashValue(string strInput, Guid hashSalt)
        {
            byte[] salt = hashSalt.ToByteArray();
            var inputData = Encoding.UTF8.GetBytes(strInput);
            byte[] message = new byte[inputData.Length + salt.Length];
            inputData.CopyTo(message, 0);
            salt.CopyTo(message, inputData.Length);

            byte[] derivedData = SHA256.Create().ComputeHash(message);
            return BitConverter.ToString(derivedData).Replace("-", string.Empty);
        }
    }
}