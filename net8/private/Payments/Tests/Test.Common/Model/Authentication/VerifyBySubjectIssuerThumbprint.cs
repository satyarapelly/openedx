using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Test.Common.Model.Authentication;

namespace Tests.Common.Model.Authentication
{
    public class VerifyBySubjectIssuerThumbprint : IVerificationRule
    {
        private readonly string expectedSubject;
        private readonly HashSet<IssuerGroup> allowedIssuerGroups;

        public VerifyBySubjectIssuerThumbprint(string subject, IEnumerable<IssuerGroup> issuerGroups)
        {
            expectedSubject = subject;
            allowedIssuerGroups = issuerGroups.ToHashSet();
        }

        public bool IsValid(X509Certificate2 certificate)
        {
            if (certificate.Subject != expectedSubject)
                return false;

            // Optional: you can use the issuer’s thumbprint, name, or O fields to map to IssuerGroup.
            // This is a stub. Customize as needed.
            return true;
        }
    }
}
