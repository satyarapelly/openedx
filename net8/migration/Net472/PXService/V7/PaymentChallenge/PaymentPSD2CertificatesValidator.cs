// <copyright file="PaymentPSD2CertificatesValidator.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using Common.Transaction;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Tracing;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class PaymentPSD2CertificatesValidator
    {
        private string jws;
        private string header;
        private string payload;
        private string signature;
        private List<string> dsRootCerts;
        private TestContext tc;
        private string sessionId;

        public PaymentPSD2CertificatesValidator(string jws, List<string> dsRootCerts, TestContext tc, string sessionId)
        {
            this.jws = jws;
            this.dsRootCerts = dsRootCerts;
            this.tc = tc;
            this.sessionId = sessionId;
        }

        private byte[] SignatureBytes
        {
            get
            {
                string signData = this.signature;
                byte[] signatureBytes = Decode(signData);
                return signatureBytes;
            }
        }

        private byte[] ContentBytes
        {
            get
            {
                string content = this.header + "." + this.payload;
                byte[] contentBytes = Encoding.Default.GetBytes(content);
                return contentBytes;
            }
        }

        /// <summary>
        /// As per the PSD2 3DS 2.1 Protocol Spec (Ref EMVCo_3DS_Spec_210, section 6.2.3.3), the SDK must validate the JWS from the ACS. Please read the description below from the spec.
        /// Using the CA public key of the DS CA identified from information provided by the 3DS Server, Validate the JWS from the ACS according to JWS (RFC7515). The 3DS SDK is 
        /// required to support both “alg” parameters PS256 and ES256. If validation fails, ceases processing and report error.
        /// Refer to the EMVCo_3DS_Spec_210_1017_0318.pdf public document for further information about the signature verfication steps. 
        /// VerifySigature() is called during the PSD2 authentication flow where the appropriate banks need to return data to the sdk. Before the sdk can consume this data, this 
        /// function validates that it is coming from a trusted source. 
        /// </summary>
        /// <param name="traceActivityId">EventTraceActivity for logging</param>
        /// <returns>
        /// Returns a bool confirming the verification status of the RSA signature to PaymentSessionsHandler.cs
        /// </returns>
        public bool VerifySignature(
            EventTraceActivity traceActivityId)
        {
            //// 1. Validate and chain all certificates in the DsCertificates.json
            X509Chain chain = new X509Chain();
            IDictionary<string, X509Certificate2> dsCertAuthorites = new Dictionary<string, X509Certificate2>();
            string dsCertAuthoritiesLoggingDetails = string.Empty;
            foreach (string individualCert in this.dsRootCerts)
            {
                if (string.IsNullOrEmpty(individualCert))
                {
                    SllWebLogger.TracePXServiceIntegrationError(
                        V7.Constants.ServiceNames.PayerAuth,
                        IntegrationErrorCode.InvalidBuild,
                        "Failed in PaymentPSD2CertificatesValidator. Null dsCertAuthority. " +
                        $"SessionId: {sessionId}",
                        traceActivityId.ToString());
                    return false;
                }

                X509Certificate2 dsx509 = new X509Certificate2(Convert.FromBase64String(individualCert));
                //// Check for duplicates
                if (!dsCertAuthorites.ContainsKey(dsx509.SubjectName.Name))
                {
                    dsCertAuthorites.Add(dsx509.SubjectName.Name, dsx509);
                    chain.ChainPolicy.ExtraStore.Add(dsx509);
                    dsCertAuthoritiesLoggingDetails += $"Issuer: {dsx509.Issuer}, Subject: {dsx509.Subject}, Expiration Date: {dsx509.NotBefore}; ";
                }
            }

            //// 2. Validate and split the ACS(this.jws) string into 3 parts
            string[] tokenComponents = new string[] { };

            if (!string.IsNullOrEmpty(this.jws))
            {
               tokenComponents = this.jws.Split('.');
            }
            
            if (string.IsNullOrEmpty(this.jws) || tokenComponents.Length != 3 || string.IsNullOrEmpty(tokenComponents[0]) || string.IsNullOrEmpty(tokenComponents[1]) || string.IsNullOrEmpty(tokenComponents[2]))
            {
                SllWebLogger.TracePXServiceIntegrationError(
                    V7.Constants.ServiceNames.PayerAuth,
                    IntegrationErrorCode.AcsSignedContentInvalid,
                    "Failed in PaymentPSD2CertificatesValidator. ACS signed content is null or not the appropriate format. " +
                    $"SessionId: {sessionId}",
                    traceActivityId.ToString());
                return false; 
            }

            this.header = tokenComponents[0];
            this.payload = tokenComponents[1];
            this.signature = tokenComponents[2];

            //// 3. Decode and deserialize the header. Certificates will be under x5c
            var signatureCerts = Encoding.UTF8.GetString(Decode(this.header));
            var sigJson = JsonConvert.DeserializeObject<Dictionary<string, JToken>>(signatureCerts);
            var sigcerts = sigJson["x5c"];

            //// 4. Set ACSCertificate to be verified later
            X509Certificate2 acsCertificate = null;
            foreach (JToken c2 in sigcerts)
            {
                var rawData = Convert.FromBase64String(c2.Value<string>());
                var cert = new X509Certificate2(rawData);
                cert.Verify();

                if (acsCertificate == null)
                {
                    acsCertificate = cert;
                }
                else if (!dsCertAuthorites.ContainsKey(cert.SubjectName.Name))
                {
                    chain.ChainPolicy.ExtraStore.Add(cert);
                }
            }

            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;

            //// 5. Confirm the chain is built correctly
            bool status = chain.Build(acsCertificate);

            //// 6. Verify ACS is signed by proper dsRootCert
            bool validChain = false;
            foreach (var authority in dsCertAuthorites.Values)
            {
                validChain = validChain | chain.ChainElements
                .Cast<X509ChainElement>()
                .Any(x => x.Certificate.Thumbprint == authority.Thumbprint);
            }

            if ((!status || !validChain) && !this.tc.ScenariosContain("px-service-psd2-e2e-emulator"))
            {
                string chainLoggingDetails = string.Empty;
                if (chain.ChainStatus.Length > 1)
                {
                    for (int index = 0; index < chain.ChainStatus.Length; index++)
                    {
                        chainLoggingDetails += $"{index+1}. Chain Status: {chain.ChainStatus[index].Status}, Chain Status Error Message: {chain.ChainStatus[index].StatusInformation}; ";
                    }
                }

                SllWebLogger.TracePXServiceIntegrationError(
                        V7.Constants.ServiceNames.PayerAuth,
                        IntegrationErrorCode.InvalidBuild,
                        "Failed in PaymentPSD2CertificatesValidator. Could not validate either ACSCert chain build or dsCertAuthorities chain. " +
                        $"ACS Certificate Details: ACS Issuer: {acsCertificate.Issuer}, ACS Subject: {acsCertificate.Subject}, ACS Expiration Date: {acsCertificate.NotAfter}, " +
                        $"DsCertAuthorities Details: {dsCertAuthoritiesLoggingDetails}, Certificate Chain Status: {chainLoggingDetails}, " +
                        $"SessionId: {sessionId}",
                        traceActivityId.ToString());
                return false;
            }

            //// 7. Verify the signature of the content
            bool signatureVerified = false;
            if (string.Equals(sigJson["alg"].Value<string>(), "ES256"))
            {
                //// EC Signature
                using (ECDsaCng ds = ECDsaCertificateExtensions.GetECDsaPublicKey(acsCertificate) as ECDsaCng)
                {
                    ds.HashAlgorithm = CngAlgorithm.Sha256;
                    signatureVerified = ds.VerifyData(this.ContentBytes, this.SignatureBytes);
                }
            }
            else
            {
                //// RSA signature
                using (RSA rsa = acsCertificate.GetRSAPublicKey() as RSA)
                {
                    signatureVerified = rsa.VerifyData(this.ContentBytes, this.SignatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
                }
            }

            if (!signatureVerified)
            {
                SllWebLogger.TracePXServiceIntegrationError(
                    V7.Constants.ServiceNames.PayerAuth,
                    IntegrationErrorCode.SignatureFailure,
                    "Failed in PaymentPSD2CertificatesValidator. Signature could not be verified.",
                    traceActivityId.ToString());
            }

            return signatureVerified;
        }

        private static string ToBase64(string arg)
        {
            if (arg == null)
            {
                throw new ArgumentNullException("arg");
            }

            var s = arg
                    .PadRight(arg.Length + ((4 - (arg.Length % 4)) % 4), '=')
                    .Replace("_", "/")
                    .Replace("-", "+");

            return s;
        }

        private static byte[] Decode(string arg)
        {
            var decrypted = ToBase64(arg);
            var base64Value = Convert.FromBase64String(decrypted);
            return base64Value;
        }
    }
}