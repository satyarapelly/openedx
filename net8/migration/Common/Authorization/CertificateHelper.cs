// <copyright file="CertificateHelper.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Authorization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Text.RegularExpressions;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;

    public static class CertificateHelper
    {
        /// <summary>  
        /// Distinguished name part name, e.g.: ou, cn, o  
        /// </summary> 
        private const string DnName = @"[\w\.]+";

        /// <summary>  
        /// Special characters that need escaping: = , " \  
        /// </summary>  
        private const string DnSpecial = @"=,""\\";

        /// <summary>  
        /// Quoted value like o="Microsoft, Corp".  
        /// </summary>  
        /// <remarks>  
        /// Values that contains dnSpecial  
        /// characters can either be enclosed in quotes or the special  
        /// characters can be escaped (see dnUnquotedValue). Quotes in quoted  
        /// values are escaped by double quotes (by Windows, according to RFC2253  
        /// should be \" but we don't support that.  
        /// </remarks>  
        private const string DnQuotedValue = @"(?:[^""]|"""")*";

        /// <summary>  
        /// Unquoted value like o=Microsoft\, Corp.  
        /// </summary>  
        /// <remarks>  
        /// <c>dnSpecial</c> characters need to be escaped by <c>\</c>  
        /// </remarks>  
        private static readonly string DnUnquotedValue = string.Format("(?:[^{0}]|\\[{0}])*", DnSpecial);

        private static readonly Regex CNInSubjectRegex = new Regex(@"CN=(?<CN>([\*@\w.:-]+\s*)+)[,$]?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>  
        /// Distinguished name tokenizer. Matches one name/value pair.  
        /// </summary>
        private static readonly Regex DnTokenizer = new Regex(
            @"(?:^|,) \s* (?<name>" + DnName + ")" +  // start w/ beginning of string or comma  
            @"\s* = \s*" +
            @"(?: (?<unquoted>" + DnUnquotedValue + @") " +
            @"  | "" (?<quoted>" + DnQuotedValue + @") "" " +
            @")\s*(?=,|$)", // skip space and make sure comma or enf of string follows  
            RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        /// <summary> 
        /// Regular expression to escape special characters.  
        /// </summary>
        private static readonly Regex DnSpecialRepl = new Regex(
              "([" + DnSpecial + "])",
              RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        public static X509Certificate GetCertificateByName(string certificateStore, string certificateName)
        {
            return GetCertificateByName(certificateStore, certificateName, true);
        }

        public static X509Certificate GetCertificateByName(string certificateStore, string certificateName, bool validOnly)
        {
            return GetCertificate(certificateStore, certificateName, validOnly, null);
        }

        public static X509Certificate GetCertificate(string certificateStore, string certificateName, bool validOnly, string certificateThumbprint = null)
        {
            StringBuilder getCertInfoStringBuilder = new StringBuilder();
            getCertInfoStringBuilder.Append(string.Format("At {0}, starting get certificate {1} from CurrentUser X509Store {2}, validOnly value is set as {3}. ", DateTime.UtcNow.ToString("s"), certificateName, certificateStore, validOnly ? "True" : "False"));
            X509Certificate2 foundCert = null;
            X509Store store = new X509Store(certificateStore, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            X509Certificate2Collection x509CertCollection = store.Certificates.Find(X509FindType.FindBySubjectName, certificateName, validOnly);
            getCertInfoStringBuilder.Append(string.Format("At {0}, number for certificate {1} found in CurrentUser X509Store is {2}. ", DateTime.UtcNow.ToString("s"), certificateName, x509CertCollection.Count));

            // Work around local setup. IIS can access current user cert store only when application pool is setup as current user. 
            // If local box can be acccess then try to access the StoreLocation.LocalMachine to get certificate
            if (x509CertCollection.Count == 0) 
            {
                getCertInfoStringBuilder.Append(string.Format("At {0}, because number for certificate {1} found in CurrentUser X509Store is 0, staring get the certificate from LocalMachine X509Store. ", DateTime.UtcNow.ToString("s"), certificateName));
                store = new X509Store(certificateStore, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadOnly);
                x509CertCollection = store.Certificates.Find(X509FindType.FindBySubjectName, certificateName, validOnly);
            }

            if (x509CertCollection.Count > 0)
            {
                foreach (X509Certificate2 cert in x509CertCollection)
                {
                    if (MatchExactSubjectForCNPart(cert.Subject, certificateName))
                    {
                        getCertInfoStringBuilder.Append(string.Format("At {0}, found certificate {1} in X509Store, with thumbprint {2} and valid date until {3}. ", DateTime.UtcNow.ToString("s"), certificateName, cert.Thumbprint, cert.NotAfter.ToString()));
                        if (string.IsNullOrWhiteSpace(certificateThumbprint))
                        {
                            if (foundCert == null || cert.NotAfter > foundCert.NotAfter)
                            {
                                foundCert = cert;
                                getCertInfoStringBuilder.Append(string.Format("At {0}, set the found certificate {1} as the certificate with thumbprint {2} and valid date until {3}", DateTime.UtcNow.ToString("s"), certificateName, cert.Thumbprint, cert.NotAfter.ToString()));
                            }
                        }
                        else if (certificateThumbprint.Equals(cert.Thumbprint, StringComparison.OrdinalIgnoreCase))
                        {
                            foundCert = cert;
                            break;
                        }
                    }
                }
            }

            store.Close();

            if (foundCert == null)
            {
                string message = string.Format("Certificate {0} is not found in store {1}", certificateName, certificateStore);
                getCertInfoStringBuilder.Append(message);
                throw TraceCore.TraceException(new CertificateException(message));
            }

            if (LoggingConfig.Mode == LoggingMode.Sll)
            {
                SllWebLogger.TracePXServiceException(getCertInfoStringBuilder.ToString(), new EventTraceActivity());
            }
            else if (LoggingConfig.Mode == LoggingMode.OpenTelemetry)
            {
                Logger.Qos.TracePXServiceException(getCertInfoStringBuilder.ToString(), new EventTraceActivity());
            }
            else
            {
                SllWebLogger.TracePXServiceException(getCertInfoStringBuilder.ToString(), new EventTraceActivity());
                Logger.Qos.TracePXServiceException(getCertInfoStringBuilder.ToString(), new EventTraceActivity());
            }

            return foundCert;
        }

        /// <summary>  
        /// Normalize a certificate's distinguished name to make it comparable.  
        /// </summary>  
        /// <remarks>According to RFC 2459 we cannot just do string comparison on DNs but  
        /// need to compare each attribute. This method will remove spaces between attributes and sort  
        /// attributes alphabetically. Code copied from classic.</remarks>  
        /// <param name="distinguishedName">distinguished name string</param>  
        /// <returns>normalized DN</returns> 
        public static string NormalizeDistinguishedName(string distinguishedName)
        {
            if (string.IsNullOrEmpty(distinguishedName))
            {
                throw new ArgumentException("distinguishedName must not be null or empty");
            }

            // make sure it's separated by commas  
            string dn = new X500DistinguishedName(distinguishedName).Decode(X500DistinguishedNameFlags.UseCommas);

            Match match = DnTokenizer.Match(dn);
            if (!match.Success)
            {
                throw new ArgumentException(string.Format("cannot parse dn: {0}", dn));
            }

            List<string> attributes = new List<string>();

            // we want to make sure that no characters are skipped between the regex  
            // applications; this would happen if the regex is wrong  
            int nextStart = 0;
            while (match != null && match.Success)
            {
                if (match.Index != nextStart)
                {
                    throw new ArgumentException(string.Format("cannot parse dn='{0}' match.Index={1} nextStart={2}", dn, match.Index, nextStart));
                }

                string name = match.Groups["name"].Value;
                string value;
                if (match.Groups["unquoted"].Success)
                {
                    value = match.Groups["unquoted"].Value;
                }
                else
                {
                    value = match.Groups["quoted"].Value;

                    // quotes (") in quoted strings are escaped by two quotes ("")， replace "" --> " (unescape)
                    value = value.Replace("\"\"", "\"");

                    // escape specials characters, e.g. ' --> \,  
                    value = DnSpecialRepl.Replace(value, @"\$1");
                }

                value = value.Trim();

                attributes.Add(name + "=" + value);
                nextStart += match.Length;
                match = match.NextMatch();
            }

            if (dn.Length != nextStart)
            {
                throw new ArgumentException(string.Format("cannot parse dn='{0}' dn.Length={1} nextStart={2}", dn, dn.Length, nextStart));
            }

            attributes.Sort(StringComparer.OrdinalIgnoreCase);
            return string.Join(",", attributes.ToArray());
        }

        /// <summary>
        /// This method ensures that all given thumbprints are sanitized/normalized to not include any special characters.
        /// </summary>
        /// <param name="thumbprint">A potentially 'dirty' thumbprint string.</param>
        /// <returns>The sanitized thumbprint string.</returns>
        public static string NormalizeThumbprint(string thumbprint)
        {
            if (string.IsNullOrWhiteSpace(thumbprint))
            {
                throw new ArgumentException("Thumbprint is null or empty.");
            }

            return string.Concat(thumbprint.Where(c => char.IsLetter(c) || char.IsNumber(c)));
        }

        /// <summary>
        /// X509FindType.FindBySubjectName would use partial match to find cert, which is less specified.
        /// Thus the result from certstore might be more than we expected, in SCS, we care about CN only in general.
        /// We cannot use X509FindType.FindBySubjectDistinguishedName because it only get the cert whose entire subject matches.
        /// This method would match(equals, not contains) the CN= part in the subject, to ensure the cert is what we want
        /// </summary>
        /// <param name="subjectName">The certificate subject</param>
        /// <param name="expectedCN">The expected matching CN</param>
        /// <returns>whether matching the CN</returns>
        private static bool MatchExactSubjectForCNPart(string subjectName, string expectedCN)
        {
            Match m = CNInSubjectRegex.Match(subjectName);
            if (!m.Success)
            {
                return false;
            }

            string actualCN = m.Groups["CN"].Value;
            return actualCN.Equals(expectedCN, StringComparison.OrdinalIgnoreCase);
        }
    }
}
