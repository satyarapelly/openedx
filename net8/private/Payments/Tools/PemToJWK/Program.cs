// <copyright file="Program.cs" company="Microsoft">Copyright (c) Microsoft 2018. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.Tools.PemToJWK
{
    using System;
    using System.Text;
    using Newtonsoft.Json;

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1 || string.Compare(args[0], "/?", true) == 0 || string.Compare(args[0], "/help", true) == 0)
            {
                ShowHelp();
                return;
            }
            else
            {
                PayPalCrypto x509 = PayPalCrypto.FromCertificate(args[0]);
                PayPalJWK jwk = new PayPalJWK(x509);
                string serialNumber = x509.GetSanitizedSerialNumber();
                string jwkJson = JsonConvert.SerializeObject(jwk);

                StringBuilder builder = new StringBuilder();

                builder.AppendLine(string.Empty);
                builder.AppendLine("--------------------------------------------------------------------------------------------------------------");
                builder.AppendLine(string.Empty);
                builder.AppendLine("serialNumber");
                builder.AppendLine(serialNumber);
                builder.AppendLine(string.Empty);
                builder.AppendLine("publicKey");
                builder.AppendLine(jwkJson);
                builder.AppendLine(string.Empty);
                builder.AppendLine("--------------------------------------------------------------------------------------------------------------");
                builder.AppendLine(string.Empty);

                Console.WriteLine(builder.ToString());
            }
        }

        public static void ShowHelp()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine(string.Empty);
            builder.AppendLine("--------------------------------------------------------------------------------------------------------------");
            builder.AppendLine("This tool builds a JWK object and an attendent serialNumber from a PEM string. This is used for the publicKey and serialNumber of the DataProtection parameters of encryptedPassword property for Paypal while using XBOX. Note that a new certificate requires an application restart from paypal as of the date of this comment.");
            builder.AppendLine(string.Empty);
            builder.AppendLine("To run this tool, type the following command:");
            builder.AppendLine("PemToJWK.exe <pemString>");
            builder.AppendLine(string.Empty);
            builder.AppendLine("Where <pemString> is the base64 string from a pem certificate. A valid pem file will have the following format");
            builder.AppendLine("-----BEGIN CERTIFICATE-----<pemString>-----END CERTIFICATE-----");
            builder.AppendLine("Strip off the header and footer and take only the base64 string");
            builder.AppendLine("--------------------------------------------------------------------------------------------------------------");
            builder.AppendLine(string.Empty);

            Console.WriteLine(builder.ToString());
        }
    }
}
