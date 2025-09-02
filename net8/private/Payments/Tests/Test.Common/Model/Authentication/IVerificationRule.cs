using System.Security.Cryptography.X509Certificates;

namespace Test.Common.Model.Authentication
{
    public interface IVerificationRule
    {
        /// <summary>
        /// Validates the given certificate against the rule.
        /// </summary>
        /// <param name="certificate">The certificate to verify.</param>
        /// <returns>True if valid, otherwise false.</returns>
        bool IsValid(X509Certificate2 certificate);
    }
}
