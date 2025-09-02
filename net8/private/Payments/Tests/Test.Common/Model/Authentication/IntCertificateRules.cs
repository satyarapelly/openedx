// <copyright file="IntCertificateRules.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Tests.Common.Model.Authentication
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Test.Common.Extensions;
    using Test.Common.Model.Authentication;

    /// <summary>
    /// Certificate rules for int environment
    /// </summary>
    public static partial class CertificateRules
    {
        private static readonly Dictionary<string, IEnumerable<IVerificationRule>> intRules = new Dictionary<string, IEnumerable<IVerificationRule>>
        {
            {
                Partner.Name.PIFDService.ToString(), new List<IVerificationRule>
                {
                    new VerifyBySubjectIssuerThumbprint(
                        "CN=clientauth-pifd.pims-int.azclient.ms",
                        new List<IssuerGroup>()
                        {
                            IssuerGroup.AME
                        })
                }
            },
            {
                Partner.Name.PXCOT.ToString(), new List<IVerificationRule>
                {
                    new VerifyBySubjectIssuerThumbprint(
                        "CN=pxtest-pxclientauth.paymentexperience.azclient-int.ms, OU=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=WA, C=US",
                        new List<IssuerGroup>()
                        {
                            IssuerGroup.AME
                        })
                }
            }
        };

        /// <summary>
        /// Gets Dictionary maps rules to partners
        /// </summary>
        public static ReadOnlyDictionary<string, IEnumerable<IVerificationRule>> Integration 
        { 
            get 
            {
                return new ReadOnlyDictionary<string, IEnumerable<IVerificationRule>>(intRules); 
            } 
        }
    }
}