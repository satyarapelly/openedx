// <copyright file="XmlDataMasker.cs" company="Microsoft">Copyright (c) Microsoft 2019. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Tracing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

    public class XmlDataMasker
    {
        private static readonly Dictionary<string, Func<string, string>> ReplacementFuncs = new Dictionary<string, Func<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            // Address
            { "FirstName",                  DelegateMaskKeepFirst1Char },
            { "FirstNamePronunciation",     DelegateMaskKeepFirst1Char },
            { "LastName",                   DelegateMaskKeepFirst1Char },
            { "LastNamePronunciation",      DelegateMaskKeepFirst1Char },
            { "FriendlyName",               DelegateMaskAll },
            { "Street1",                    DelegateMaskAll },
            { "Street2",                    DelegateMaskAll },
            { "Street3",                    DelegateMaskAll },
            { "UnitNumber",                 DelegateMaskAll },

            // AccountInfo
            { "PhoneNumber",                DelegateMaskKeepFirst1Char },
            { "PhonePrefix",                DelegateMaskAll },
            { "PhoneExtension",             DelegateMaskAll },
            { "Email",                      DelegateMaskEmail },
            { "CompanyName",                DelegateMaskAll },
            { "CompanyNamePronunciation",   DelegateMaskAll },
            { "CorporateIdentity",          DelegateMaskAll },
            { "CorporateLegalEntity",       DelegateMaskAll },
            { "CorporateVatId",             DelegateMaskAll },
            { "HCI",                        DelegateMaskAll },
            { "HCIValid",                   DelegateMaskAll },
            { "Value",                      DelegateMaskAll },  // For Birthday, Nationality

            // TaxExemptionInfo
            { "CertificateNumber",          DelegateMaskAll },

            // IdentityProperty
            { "IdentityValue",              DelegateMaskAll } // PUID or OrgPUID
        };

        public static string Mask(string xmlData)
        {
            if (string.IsNullOrWhiteSpace(xmlData))
            {
                return xmlData;
            }

            try
            {
                XElement xmlElem = XElement.Parse(xmlData);

                if (xmlElem.Descendants().Any())
                {
                    foreach (var el in xmlElem.Descendants())
                    {
                        if (!el.Descendants().Any())
                        {
                            string elName = el.Name.LocalName;

                            if (ReplacementFuncs.ContainsKey(elName))
                            {
                                el.Value = ReplacementFuncs[elName](el.Value);
                            }
                        }
                    }
                }

                return xmlElem.ToString(SaveOptions.DisableFormatting);
            }
            catch
            {
                return string.Format("MASKED({0})", xmlData.Length);
            }
        }
        
        private static string DelegateMaskAll(string original)
        {
            return string.Format("MASKED({0})", original.Length);
        }

        private static string DelegateMaskKeepFirst1Char(string original)
        {
            return original.Length > 1
                ? string.Format("{0}...({1})", original.Substring(0, 1), original.Length)
                : "MASKED";
        }

        private static string DelegateMaskEmail(string emailAddress)
        {
            string[] strs = emailAddress.Split('@');
            if (strs.Length != 2)
            {
                // an email without an @ is not an email
                return string.Format("MASKED({0})", emailAddress.Length);
            }

            return (strs[0].Length > 3 ? string.Format("{0}...{1}({2})", strs[0].Substring(0, 2), strs[0].Substring(strs[0].Length - 1), strs[0].Length) : "MASKED") + "@" + strs[1];
        }
    }
}
