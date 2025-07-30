// <copyright file="PXServiceAuthorizationFilterAttribute.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using Microsoft.Commerce.Payments.Common.Authorization;
    using Microsoft.Commerce.Payments.PXCommon;
    using Properties = Common.PaymentConstants.Web.Properties;

    public sealed class PXServiceAuthorizationFilterAttribute : AuthorizeAttribute
    {
        private static readonly IIdentity NullIdentity = new GenericIdentity("NULL", "NULL"); 

        public bool AllowUnauthenticatedHttpCalls { get; set; }

        public bool AllowUnauthenticatedHttpsCalls { get; set; }

        public Management.CertificateVerificationCore.UserDirectory CertificateAuthenticator { get; set; }

        public UberUserDirectory UberUserDirectory { get; set; }

        public TokenMiseValidator TokenMiseValidator { get; set; }

        public override async Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            actionContext.Request.Properties[Properties.CallerName] = GlobalConstants.ClientNames.Unknown;
            string cV = PXTraceCorrelationHandler.GetOrCreateCorrelationIdFromHeader(actionContext.Request).CorrelationVectorV4.ToString();
            if (actionContext.Request.RequestUri.Scheme == Uri.UriSchemeHttps)
            {
                if (this.AllowUnauthenticatedHttpsCalls || IsProbeUri(actionContext.Request.RequestUri.PathAndQuery))
                {
                    actionContext.RequestContext.Principal = new AdminPrincipal(NullIdentity);
                    actionContext.Request.Properties[Properties.CallerName] = GlobalConstants.ClientNames.AnonymousCaller;
                    await base.OnAuthorizationAsync(actionContext, cancellationToken);
                }
                else
                {
                    string certAuthResult = GlobalConstants.AuthResult.ByPass;
                    string tokenAuthResult = GlobalConstants.AuthResult.Succeed;

                    var partner = await this.AuthenticateTokenMISE(actionContext, cancellationToken, cV);

                    // Fail back to certificate authentication, once PIFD migration out certificate, 
                    // the following lines should be removed.
                    if (partner == null) 
                    {
                        tokenAuthResult = GlobalConstants.AuthResult.Failed;
                        partner = this.AuthenticateByCert(actionContext);
                        certAuthResult = partner == null ? 
                            GlobalConstants.AuthResult.Failed : 
                            GlobalConstants.AuthResult.Succeed;
                    }

                    // Authorize partners
                    if (partner != null) 
                    {
                        actionContext.Request.Properties[Properties.CertConfig] = string.Format("role {0}, requestUri {1}, authrizedCert allow path {2}, allow account {3}, AllowedUnAuthenticatedPath {4} ", partner.Role, actionContext.Request.RequestUri.AbsoluteUri, partner.AllowedAuthenticatedPathTemplate, partner.AllowedAccounts, partner.AllowedUnAuthenticatedPaths);
                        if (UserInformation.IsAuthorized(actionContext.Request.RequestUri, partner))
                        {
                            // TODO: update with user principal
                            actionContext.Request.Properties[Properties.CertPrinciple] = "having principle";
                            actionContext.RequestContext.Principal = new AdminPrincipal(NullIdentity);
                        }

                        actionContext.Request.Properties[Properties.CallerName] = partner.PartnerName;
                        await base.OnAuthorizationAsync(actionContext, cancellationToken);
                    }
                    else
                    {
                        // consider adding certRequired error message
                        HandleDecline(actionContext, HttpStatusCode.Unauthorized, ErrorConstants.ErrorMessages.CertRequired);
                    }

                    actionContext.Request.Properties[Properties.CertAuthResult] = certAuthResult;
                    actionContext.Request.Properties[Properties.TokenAuthResult] = tokenAuthResult;
                }
            }
            else if (actionContext.Request.RequestUri.Scheme == Uri.UriSchemeHttp)
            {
                if (this.AllowUnauthenticatedHttpCalls || IsProbeUri(actionContext.Request.RequestUri.PathAndQuery))
                {
                    actionContext.RequestContext.Principal = new AdminPrincipal(NullIdentity);
                    actionContext.Request.Properties[Properties.CallerName] = GlobalConstants.ClientNames.AnonymousCaller;
                    await base.OnAuthorizationAsync(actionContext, cancellationToken);
                }
                else
                {
                    actionContext.Request.Properties[Properties.CertAuthError] = string.Format("{0} {1}", this.AllowUnauthenticatedHttpCalls, actionContext.Request.RequestUri.PathAndQuery);
                    HandleDecline(actionContext, HttpStatusCode.Forbidden, ErrorConstants.ErrorMessages.ProtocolNotSupport);
                }
            }
        }

        private static bool IsProbeUri(string relativePath)
        {
            return string.Equals(relativePath.Remove(0, 1), GlobalConstants.EndPointNames.V7Probe, StringComparison.OrdinalIgnoreCase);
        }

        private static void HandleDecline(HttpActionContext actionContext, HttpStatusCode statusCode, string errorMessage)
        {
            actionContext.Response = new HttpResponseMessage(statusCode)
            {
                ReasonPhrase = errorMessage
            };
        }

        private UserInformation GetCertificateBySubjectName(string subject)
        {
            return this.UberUserDirectory.UserInformation.SingleOrDefault(
                p => p?.CertificateVerificationRule != null 
                && string.Equals(CertificateHelper.NormalizeDistinguishedName(p.CertificateVerificationRule.Subject), subject, StringComparison.OrdinalIgnoreCase));
        }

        private IIdentity[] ValidateCertificate(X509Certificate2 cert, HttpActionContext actionContext)
        {
            Management.CertificateVerificationCore.IdentitiesResult identitiesResult = null;
            Stopwatch sw = new Stopwatch();
            sw.Start();

            try
            {
                //// Fetch certificate's identity, return cert's identity if already stored in cache (expiry time 1 hour), otherwise fetch from dSMS endpoint and store the result in cache
                identitiesResult = this.CertificateAuthenticator.FetchIdentities(cert, true);
                string certAuthInfo = string.Format(
                                        "[VerifyCertificateEvent] subject = {0}, issuer = {1}, thumbprint = {2}, expirationDate = {3}, reason = {4}, verificationResult = {5}, Latency = {6}, partner = {7}",
                                        cert?.Subject,
                                        cert?.Issuer,
                                        cert?.Thumbprint,
                                        cert?.GetExpirationDateString(),
                                        string.Format("Verification for {0} completed, Result: {1}, Message: {2}", cert?.Subject, identitiesResult?.Result, identitiesResult?.Message),
                                        identitiesResult?.Result,
                                        sw.ElapsedMilliseconds,
                                        string.Join(",", identitiesResult?.Identities?.Select(x => x.Name)));

                actionContext.Request.Properties[Properties.CertAuthInfo] = certAuthInfo;
            }
            catch (Exception ex)
            {
                actionContext.Request.Properties[Properties.CertAuthError] = string.Format("Fetch cert's identities failed : Subject: '{0}' Issuer: '{1}' Thumbprint: '{2}' is not configured as allowed client certificates for {3}. Error: {4}", cert == null ? "<none>" : cert.Subject, cert == null ? "<none>" : cert.Issuer, cert == null ? "<none>" : cert.Thumbprint, actionContext.Request.RequestUri.AbsoluteUri, ex.ToString());
            }

            return identitiesResult?.Identities?.ToArray();
        }

        private UserInformation AuthenticateByCert(HttpActionContext actionContext)
        {
            X509Certificate2 cert = actionContext.Request.GetClientCertificate();
            if (cert == null)
            {
                string certStr = null;
                IEnumerable<string> certHeaders;
                if (actionContext.Request.Headers.TryGetValues("X-ARR-ClientCert", out certHeaders))
                {
                    certStr = certHeaders.FirstOrDefault();
                }

                if (!string.IsNullOrEmpty(certStr))
                {
                    try
                    {
                        cert = new X509Certificate2(Convert.FromBase64String(certStr));
                        actionContext.Request.Properties[Properties.CertInfo] = string.Format("find certificate Subject: '{0}' Issuer: '{1}' Thumbprint: '{2}'  {3}", cert == null ? "<none>" : cert.Subject, cert == null ? "<none>" : cert.Issuer, cert == null ? "<none>" : cert.Thumbprint, actionContext.Request.RequestUri.AbsoluteUri);
                    }
                    catch (Exception ex)
                    {
                        actionContext.Request.Properties[Properties.CertInfo] = string.Format("Can't convert X-ARR-ClientCert into valid cert because: {0}", ex.ToString());
                    }
                }
            }

            UserInformation partner = null;
            if (cert != null)
            {
                IIdentity[] identities = this.ValidateCertificate(cert, actionContext);

                if (identities == null || identities.Length == 0 || !identities[0].IsAuthenticated)
                {
                    // TODO: clean up logging to reflect new auth story.
                    actionContext.Request.Properties[Properties.CertAuthError] = actionContext.Request.Properties[Properties.CertAuthError] ?? string.Format("Certificate's subject or issuer is not in the allowed list: Subject: '{0}' Issuer: '{1}' Thumbprint: '{2}' is not configured as allowed client certificates for {3}", cert == null ? "<none>" : cert.Subject, cert == null ? "<none>" : cert.Issuer, cert == null ? "<none>" : cert.Thumbprint, actionContext.Request.RequestUri.AbsoluteUri);
                    HandleDecline(actionContext, HttpStatusCode.Unauthorized, ErrorConstants.ErrorMessages.CertRequired);
                }

                string callerThumbprint = CertificateHelper.NormalizeThumbprint(cert.Thumbprint);
                partner = this.GetCertificateBySubjectName(CertificateHelper.NormalizeDistinguishedName(cert.Subject));
            }

            return partner;
        }

        private async Task<UserInformation> AuthenticateTokenMISE(
            HttpActionContext actionContext,
            CancellationToken cancellationToken,
            string incomingRequestId)
        {
            try
            {
                AuthenticationHeaderValue authenticationHeader = actionContext.Request.Headers.Authorization;
                MiseTokenValidationResult miseResult = await this.TokenMiseValidator.ValidateAsync(authenticationHeader, incomingRequestId, cancellationToken);

                if (miseResult != null)
                {
                    if (miseResult.Success)
                    {
                        var tokenPrincipal = miseResult.TokenPrincipal;
                        var appId = miseResult.ApplicationId;
                        UserInformation caller = this.UberUserDirectory.UserInformation.SingleOrDefault(p => string.Equals(p?.ApplicationId, appId, StringComparison.OrdinalIgnoreCase));

                        if (caller == null)
                        {
                            actionContext.Request.Properties[Properties.TokenAuthError] = string.Format("{0} isn't whitelisted", appId);
                        }
                        else
                        {
                            actionContext.Request.Properties[Properties.TokenAuthWarning] = string.Format("Successfully authenticated AAD with appId {0} using MISE+SAL", appId);
                        }

                        return caller;
                    }
                    else
                    {
                        actionContext.Request.Properties[Properties.TokenAuthWarning] = string.Format("Failed to validate AAD token using MISE+SAL. Message: {0}", miseResult.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                actionContext.Request.Properties[Properties.TokenAuthError] = string.Format("Token auth failed unexpected {0}", ex.ToString());
            }

            return null;
        }
    }
}
