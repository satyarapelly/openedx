// <copyright file="UberUserDirectory.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.Management.CertificateVerificationCore;

    public class UberUserDirectory
    {
        public UberUserDirectory(IList<UserInformation> userInformation)
        {
            this.UserInformation = userInformation;
            this.CertificateVerificationRules = new Dictionary<string, IEnumerable<IVerificationRule>>();
            foreach (UserInformation partnerInfo in this.UserInformation)
            {
                IEnumerable<IVerificationRule> verificationRuleList;
                if (this.CertificateVerificationRules.TryGetValue(partnerInfo.PartnerName, out verificationRuleList))
                {
                    List<IVerificationRule> currentList = verificationRuleList.ToList();
                    currentList.Add(partnerInfo.CertificateVerificationRule);
                    this.CertificateVerificationRules[partnerInfo.PartnerName] = currentList;
                }
                else if (partnerInfo.CertificateVerificationRule != null)
                {
                    this.CertificateVerificationRules.Add(partnerInfo.PartnerName, new List<IVerificationRule> { partnerInfo.CertificateVerificationRule });
                }
            }
        }

        public IList<UserInformation> UserInformation { get; }

        public Dictionary<string, IEnumerable<IVerificationRule>> CertificateVerificationRules { get; }
    }
}