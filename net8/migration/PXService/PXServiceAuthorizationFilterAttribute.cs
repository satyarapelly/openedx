// <copyright file="PXServiceAuthorizationFilterAttribute.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
	using System;
	using System.Diagnostics;
	using System.Linq;
	using System.Net;
	using System.Security.Cryptography.X509Certificates;
	using System.Security.Principal;
	using System.Threading;
	using System.Threading.Tasks;
	using Microsoft.ApplicationInsights.AspNetCore.Extensions;
	using Microsoft.AspNetCore.Authorization;
	using Microsoft.AspNetCore.Http;
	using Microsoft.AspNetCore.Mvc.Filters;
	using Microsoft.Commerce.Payments.Common.Authorization;
	using Microsoft.Commerce.Payments.PXCommon;
	using Microsoft.Extensions.Primitives;

    public sealed class PXServiceAuthorizationFilterAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private static readonly IIdentity NullIdentity = new GenericIdentity("NULL", "NULL");

        public bool AllowUnauthenticatedHttpCalls { get; set; }

        public bool AllowUnauthenticatedHttpsCalls { get; set; }

        public Management.CertificateVerificationCore.UserDirectory CertificateAuthenticator { get; set; }

        public UberUserDirectory UberUserDirectory { get; set; }

        public TokenMiseValidator TokenMiseValidator { get; set; }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var request = context.HttpContext.Request;
            var properties = context.HttpContext.Items;

            properties["CallerName"] = GlobalConstants.ClientNames.Unknown;
            string cV = string.Empty; // PXTraceCorrelationHandler.GetOrCreateCorrelationIdFromHeader(context.Result).CorrelationVectorV4.ToString();

            if (request.IsHttps)
            {
                if (AllowUnauthenticatedHttpsCalls || IsProbeUri(request.Path))
                {
                    context.HttpContext.User = new GenericPrincipal(NullIdentity, null);
                    properties["CallerName"] = GlobalConstants.ClientNames.AnonymousCaller;
                    return;
                }

                string certAuthResult = GlobalConstants.AuthResult.ByPass;
                string tokenAuthResult = GlobalConstants.AuthResult.Succeed;

                var partner = await AuthenticateTokenMISE(context.HttpContext, cV);

                if (partner == null)
                {
                    tokenAuthResult = GlobalConstants.AuthResult.Failed;
                    partner = AuthenticateByCert(context.HttpContext);
                    certAuthResult = partner == null ? GlobalConstants.AuthResult.Failed : GlobalConstants.AuthResult.Succeed;
                }

                if (partner != null)
                {
                    properties["CertConfig"] = $"role {partner.Role}, requestUri {request.Path}, allow path {partner.AllowedAuthenticatedPathTemplate}";
                    if (UserInformation.IsAuthorized(request.GetUri(), partner))
                    {
                        properties["CertPrinciple"] = "having principle";
                        context.HttpContext.User = new GenericPrincipal(NullIdentity, null);
                    }
                    properties["CallerName"] = partner.PartnerName;
                }
                else
                {
                    context.Result = new Microsoft.AspNetCore.Mvc.ContentResult
                    {
                        StatusCode = (int)HttpStatusCode.Unauthorized,
                        Content = ErrorConstants.ErrorMessages.CertRequired
                    };
                }

                properties["CertAuthResult"] = certAuthResult;
                properties["TokenAuthResult"] = tokenAuthResult;
            }
            else
            {
                if (AllowUnauthenticatedHttpCalls || IsProbeUri(request.Path))
                {
                    context.HttpContext.User = new GenericPrincipal(NullIdentity, null);
                    properties["CallerName"] = GlobalConstants.ClientNames.AnonymousCaller;
                }
                else
                {
                    properties["CertAuthError"] = $"{AllowUnauthenticatedHttpCalls} {request.Path}";
                    context.Result = new Microsoft.AspNetCore.Mvc.ContentResult
                    {
                        StatusCode = (int)HttpStatusCode.Forbidden,
                        Content = ErrorConstants.ErrorMessages.ProtocolNotSupport
                    };
                }
            }
        }

        private static bool IsProbeUri(PathString path)
        {
            if (!path.HasValue)
            {
                return false;
            }

            var trimmed = path.Value.TrimStart('/');

            if (trimmed.Equals(GlobalConstants.EndPointNames.V7Probe, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var segments = trimmed.Split('/', StringSplitOptions.RemoveEmptyEntries);
            return segments.Length == 2
                && segments[1].Equals(GlobalConstants.EndPointNames.V7Probe, StringComparison.OrdinalIgnoreCase)
                && segments[0].StartsWith("v", StringComparison.OrdinalIgnoreCase);
        }

        private UserInformation AuthenticateByCert(HttpContext httpContext)
        {
            X509Certificate2 cert = httpContext.Connection.ClientCertificate;
            if (cert == null && httpContext.Request.Headers.TryGetValue("X-ARR-ClientCert", out StringValues certHeaders))
            {
                string certStr = certHeaders.FirstOrDefault();
                if (!string.IsNullOrEmpty(certStr))
                {
                    try
                    {
                        cert = new X509Certificate2(Convert.FromBase64String(certStr));
                        httpContext.Items["CertInfo"] = $"Subject: '{cert.Subject}' Issuer: '{cert.Issuer}' Thumbprint: '{cert.Thumbprint}'";
                    }
                    catch (Exception ex)
                    {
                        httpContext.Items["CertInfo"] = $"Can't parse cert: {ex}";
                    }
                }
            }

            if (cert != null)
            {
                var identities = ValidateCertificate(cert, httpContext);
                if (identities == null || identities.Length == 0 || !identities[0].IsAuthenticated)
                {
                    httpContext.Items["CertAuthError"] = $"Certificate not valid: {cert.Subject}";
                    return null;
                }

                return UberUserDirectory.UserInformation.SingleOrDefault(p =>
                    p?.CertificateVerificationRule != null &&
                    CertificateHelper.NormalizeDistinguishedName(p.CertificateVerificationRule.Subject).Equals(
                        CertificateHelper.NormalizeDistinguishedName(cert.Subject), StringComparison.OrdinalIgnoreCase));
            }

            return null;
        }

        private IIdentity[] ValidateCertificate(X509Certificate2 cert, HttpContext httpContext)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var result = CertificateAuthenticator.FetchIdentities(cert, true);
                httpContext.Items["CertAuthInfo"] = $"subject = {cert.Subject}, result = {result?.Result}, latency = {sw.ElapsedMilliseconds}";
                return result?.Identities?.ToArray();
            }
            catch (Exception ex)
            {
                httpContext.Items["CertAuthError"] = $"Cert identity fetch failed: {ex}";
                return null;
            }
        }

        private async Task<UserInformation> AuthenticateTokenMISE(HttpContext httpContext, string incomingRequestId)
        {
            try
            {
                var authHeader = httpContext.Request.Headers["Authorization"].ToString();
                var headerValue = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authHeader.Split(' ').Last());
                var result = await TokenMiseValidator.ValidateAsync(headerValue, incomingRequestId, CancellationToken.None);
                if (result?.Success == true)
                {
                    var caller = UberUserDirectory.UserInformation.SingleOrDefault(p =>
                        string.Equals(p?.ApplicationId, result.ApplicationId, StringComparison.OrdinalIgnoreCase));
                    if (caller == null)
                    {
                        httpContext.Items["TokenAuthError"] = $"{result.ApplicationId} isn't whitelisted";
                    }
                    else
                    {
                        httpContext.Items["TokenAuthWarning"] = $"Authenticated AAD with appId {result.ApplicationId}";
                    }
                    return caller;
                }
                else
                {
                    httpContext.Items["TokenAuthWarning"] = $"Token validation failed: {result?.Message}";
                }
            }
            catch (Exception ex)
            {
                httpContext.Items["TokenAuthError"] = $"Token auth failed: {ex}";
            }

            return null;
        }
    }
}
