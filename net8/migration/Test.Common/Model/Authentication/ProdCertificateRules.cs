// <copyright file="ProdCertificateRules.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Tests.Common.Model.Authentication
{
	using System.Collections.Generic;
	using Test.Common.Model.Authentication;

	/// <summary>
	/// Certificate rules for the production environment.
	/// </summary>
	public static partial class CertificateRules
	{
	    private static readonly IReadOnlyDictionary<string, IEnumerable<IVerificationRule>> prodRules =
	        new Dictionary<string, IEnumerable<IVerificationRule>>(System.StringComparer.OrdinalIgnoreCase)
	        {
	            [Partner.Name.PIFDService.ToString()] = new List<IVerificationRule>
	            {
	                new VerifyBySubjectIssuerThumbprint(
	                    "CN=clientauth-pifd.pims.azclient.ms",
	                    new[] { IssuerGroup.AME }
	                )
	            },
	            [Partner.Name.PXCOT.ToString()] = new List<IVerificationRule>
	            {
	                new VerifyBySubjectIssuerThumbprint(
	                    "CN=pxtest-pxclientauth.paymentexperience.azclient.ms, OU=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=WA, C=US",
	                    new[] { IssuerGroup.AME }
	                )
	            }
	        };

	    /// <summary>
	    /// Gets the certificate verification rules for production partners.
	    /// </summary>
	    public static IReadOnlyDictionary<string, IEnumerable<IVerificationRule>> Production => prodRules;
	}
}
