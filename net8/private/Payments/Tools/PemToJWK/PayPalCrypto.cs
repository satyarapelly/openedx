// <copyright file="PayPalCrypto.cs" company="Microsoft">Copyright (c) Microsoft 2018. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.Tools.PemToJWK
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;   

    /// <summary>Microsoft's implementation of PayPal credential encryption.</summary>
    /// <remarks>
    /// PayPal uses RSA PKCS#1v1.5, AES-256-CBC, and PKCS#7 message padding schemes to encrypt a user credential (e.g., password).
    /// Generate a cryptographically random session key, encrypt the session key with the public key,
    /// and a message symmetrically encrypted with the session key.
    /// <p>
    /// Encryption format: Header | Encrypted Exchange Key | Encrypted Message
    /// * Header: Version(WORD) = 0x05 | CertSerialNum[BYTE[8]] | LenEncryptedText[WORD]
    /// * Encrypted Exchange Key: SessionKey[BYTE[]] | Separator[BYTE] = 0 | Random[BYTE[221]] | Version[BYTE] = 2 | Reserved[BYTE] = 0
    /// * Encrypted Message: Message[BYTE[]] | Padding[BYTE[1..16]]
    /// If the certificate serial number is longer than 8 bytes, use the least significant 8 bytes of the certificate serial number in big endian.
    /// Length of the encrypted message in WORD = Len(Encrypted Exchange Key) + Len(Encrypted Message)
    /// The encrypted exchange key contains a PKCS#1v1.5 formatted encrypted session key.
    /// The final output is base64 encoded.</p>
    /// <p>
    /// Authentication Message: TimeStamp[DWORD] | Data-DeviceId[BYTE | DeviceId[BYTE[Len]] | Data-PIN-Password[WORD[Len]]
    /// Timestamp is measured in seconds since 1/1/1970 UTC (Unix time), in 32-bit unsigned integer, little endian.
    /// Password is in unicode, in little endian.</p>
    /// </remarks>
    public class PayPalCrypto
    {
        public const int DefaultAESKeySizeInBits = 256;
        public RSAParameters rsaParameters;
        public byte[] serialNumberBytes;

        /// <summary>
        /// Gets and sets the AES key size in bits.
        /// </summary>
        public int AESKeySizeInBits { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PayPalCrypto()
        {
            AESKeySizeInBits = DefaultAESKeySizeInBits;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="certificatePEM">The string is not a true PEM because it expects no prefix and suffix. Just provide the base64 string and remove any text at the beginning and end that looks like "-----BEGIN CERTIFICATE-----" or "-----END CERTIFICATE-----"</param>
        /// <returns></returns>
        public static PayPalCrypto FromCertificate(string certificatePEM)
        {
            byte[] certificate = Convert.FromBase64String(certificatePEM);
            RSAParameters rsaParameters;
            X509Certificate2 cert = new X509Certificate2(certificate);
            RSA rsaKey = RSA.Create(2048);
            rsaKey.FromXmlString(cert.PublicKey.Key.ToXmlString(false));
            rsaParameters = rsaKey.ExportParameters(false);

            PayPalCrypto ppc = new PayPalCrypto();
            ppc.SetPublicKey(rsaParameters, cert.SerialNumber);
            return ppc;
        }

        /// <summary>
        /// Although the spec states that only the 8 least significant bytes are used(16 digits in hex), 9 byte serial numbers with a leading 0x00 byte are valid
        /// </summary>
        /// <returns></returns>
        public string GetSanitizedSerialNumber()
        {
            return BitConverter.ToString(serialNumberBytes).Replace("-", string.Empty);
        }

        /// <summary>
        /// Set the encryption public key and the certificate serial number.
        /// </summary>
        /// <param name="publicKey">RSA Public key.</param>
        /// <param name="serialNumber">Certificate serial number.</param>
        public void SetPublicKey(RSAParameters publicKey, string serialNumber)
        {
            this.rsaParameters = publicKey;
            this.serialNumberBytes = StringToByteArray(serialNumber);
        }

        /// <summary>
        /// Convert a string to byte array
        /// </summary>
        /// <param name="hex">Input string in hex, big endian.</param>
        /// <returns></returns>
        private static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
    }
}