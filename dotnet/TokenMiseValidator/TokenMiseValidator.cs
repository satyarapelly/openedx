// <copyright file="TokenMiseValidator.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXCommon
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the <see cref="TokenMiseValidator" />.
    /// Validate token using MISE+SAL library (modified from PIFD code).
    /// </summary>
    public class TokenMiseValidator
    {
        private dynamic miseHost;
        private IAuthenticationLogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenMiseValidator"/> class.
        /// </summary>
        /// <param name="aadAuthenticationOptions">The AadAuthenticationOptions.</param>
        /// <param name="logger">Used for logging. </param>
        public TokenMiseValidator(dynamic aadAuthenticationOptions, IAuthenticationLogger logger)
        {
            // Dynamically locate the original authentication types if available
            var factoryType = Type.GetType("Microsoft.IdentityModel.S2S.S2SAuthenticationManagerFactory, Microsoft.IdentityModel.S2S");
            var builderType = Type.GetType("Microsoft.IdentityModel.S2S.Configuration.MiseBuilder, Microsoft.IdentityModel.S2S");
            var appInfoType = Type.GetType("Microsoft.IdentityModel.S2S.Configuration.ApplicationInformationContainer, Microsoft.IdentityModel.S2S");

            if (factoryType != null && builderType != null && appInfoType != null)
            {
                dynamic factory = factoryType.GetProperty("Default")!.GetValue(null, null);
                dynamic s2sManager = factory.BuildS2SAuthenticationManager(aadAuthenticationOptions);
                dynamic appInfo = Activator.CreateInstance(appInfoType, aadAuthenticationOptions.ClientId);
                dynamic builder = builderType.Create(appInfo)
                    .WithDefaultAuthentication(s2sManager)
                    .ConfigureDefaultModuleCollection((Action<dynamic>)(_ => { }));
                this.miseHost = builder.Build();
            }

            this.logger = logger;
        }

        /// <summary>
        /// Get Claims using MISE+SAL library.
        /// </summary>
        /// <param name="authenticationHeaderValue">The authentication header.</param>
        /// <param name="incomingRequestId">The unique id of incoming request and it can be CV if available.</param>
        /// <param name="cancellationToken">The cancellationToken.</param>
        /// <returns>The <see cref="Task{MiseTokenValidationResult}"/>.</returns>
        public async Task<MiseTokenValidationResult> ValidateAsync(
            AuthenticationHeaderValue authenticationHeaderValue,
            string incomingRequestId,
            CancellationToken cancellationToken)
        {
            Stopwatch latency = Stopwatch.StartNew();
            MiseTokenValidationResult result = new MiseTokenValidationResult();
            result.Success = true;
            string message;
            string token;

            if (authenticationHeaderValue == null)
            {
                result.Failed(Constants.AuthenticationErrorCode.NoSecurityTokenFound, "Security Token not obtainable.", new ArgumentException("authenticationHeaderValue is null"));
            }
            else if (!authenticationHeaderValue.Scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase))
            {
                result.Failed(Constants.AuthenticationErrorCode.InvalidSchema, "Non Bearer schema");
            }
            else if (string.IsNullOrEmpty(authenticationHeaderValue.Parameter))
            {
                result.Failed(Constants.AuthenticationErrorCode.NoSecurityTokenFound, "Invalid Token Provided", new ArgumentException("Authentication Token cannot be null or empty."));
            }

            if (!result.Success)
            {
                latency.Stop();
                result.Latency = (int)latency.Elapsed.TotalMilliseconds;

                this.logger.LogMiseTokenValidationResult(
                    result,
                    latency.ElapsedMilliseconds,
                    result.Exception,
                    incomingRequestId);

                return result;
            }

            token = authenticationHeaderValue.Parameter;

            try
            {
                // Use dynamic for HttpRequestData and MiseHttpContext
                dynamic httpRequestData = new HttpRequestData();
                httpRequestData.Headers.Add("Authorization", "Bearer " + token);

                dynamic context = new MiseHttpContext(httpRequestData)
                {
                    CorrelationId = incomingRequestId
                };

                dynamic miseResult = await this.miseHost.HandleAsync(context, cancellationToken).ConfigureAwait(false);

                if (miseResult != null && miseResult.Succeeded)
                {
                    string instanceName = MiseResultExtensions.GetInstanceName(miseResult);
                    message = MiseResultExtensions.GetAdditionalHttpResponseInformation(miseResult);

                    ClaimsPrincipal claims = new ClaimsPrincipal(miseResult.AuthenticationTicket.SubjectIdentity ?? miseResult.AuthenticationTicket.ActorIdentity);
                    result.Succeed(claims, instanceName, message);

                    result.ApplicationId = GetValueFromClaims(result.TokenPrincipal, "appid");
                }
                else
                {
                    var moduleCreatedFailureResponse = GetPropertyValue(miseResult, "MiseContext.ModuleFailureResponse");
                    if (moduleCreatedFailureResponse != null)
                    {
                        message = MiseResultExtensions.GetModuleFailureResponseInformation(miseResult);
                        string statusCode = GetPropertyValue(moduleCreatedFailureResponse, "StatusCode")?.ToString();
                        result.Failed(Constants.AuthenticationErrorCode.SecurityTokenValidationFailed, $"HTTP status code: {statusCode}. Message {message}", GetPropertyValue(miseResult, "Failure"));
                    }
                    else
                    {
                        result.Failed(Constants.AuthenticationErrorCode.EmptyModuleCreatedFailureResponse, "Get empty moduleCreatedFailureResponse.", GetPropertyValue(miseResult, "Failure"));
                    }
                }
            }
            catch (Exception ex)
            {
                result.Failed(Constants.AuthenticationErrorCode.UnexpectedMiseValidaionFailure, "Hit unknown error.", ex);
            }
            finally
            {
                latency.Stop();
                result.Latency = (int)latency.Elapsed.TotalMilliseconds;
            }

            this.logger.LogMiseTokenValidationResult(
                    result,
                    latency.ElapsedMilliseconds,
                    result.Exception,
                    incomingRequestId);

            return result;
        }

        private static string GetValueFromClaims(ClaimsPrincipal tokenPrincipal, string claimType) =>
            tokenPrincipal?.Claims.FirstOrDefault(claim => claim.Type.Equals(claimType, StringComparison.OrdinalIgnoreCase))?.Value;

        // Helper to get nested property value using reflection
        private static object GetPropertyValue(object obj, string propertyPath)
        {
            if (obj == null || string.IsNullOrEmpty(propertyPath)) return null;
            var props = propertyPath.Split('.');
            object current = obj;
            foreach (var prop in props)
            {
                if (current == null) return null;
                var type = current.GetType();
                var property = type.GetProperty(prop);
                if (property == null) return null;
                current = property.GetValue(current);
            }
            return current;
        }
    }

    // Stub interfaces and classes to allow compilation without the original
    // Microsoft authentication libraries. These provide just enough structure
    // for the sample TokenMiseValidator to build on .NET 8.
    public interface IAuthenticationLogger
    {
        void LogMiseTokenValidationResult(
            MiseTokenValidationResult result,
            long latency,
            Exception? exception,
            string incomingRequestId);
    }

    public static class Constants
    {
        public static class AuthenticationErrorCode
        {
            public const string NoSecurityTokenFound = nameof(NoSecurityTokenFound);
            public const string InvalidSchema = nameof(InvalidSchema);
            public const string SecurityTokenValidationFailed = nameof(SecurityTokenValidationFailed);
            public const string EmptyModuleCreatedFailureResponse = nameof(EmptyModuleCreatedFailureResponse);
            public const string UnexpectedMiseValidaionFailure = nameof(UnexpectedMiseValidaionFailure);
        }
    }

    public class MiseTokenValidationResult
    {
        public bool Success { get; set; }
        public ClaimsPrincipal? TokenPrincipal { get; private set; }
        public string? ApplicationId { get; set; }
        public int Latency { get; set; }
        public Exception? Exception { get; private set; }

        public void Failed(string code, string message, Exception? ex = null)
        {
            Success = false;
            Exception = ex;
        }

        public void Succeed(ClaimsPrincipal claimsPrincipal, string instanceName, string message)
        {
            Success = true;
            TokenPrincipal = claimsPrincipal;
        }
    }

    public class HttpRequestData
    {
        public Dictionary<string, string> Headers { get; } = new();
    }

    public class MiseHttpContext
    {
        public HttpRequestData Request { get; }
        public string? CorrelationId { get; set; }

        public MiseHttpContext(HttpRequestData request)
        {
            Request = request;
        }
    }

    public static class MiseResultExtensions
    {
        public static string GetInstanceName(dynamic result) => string.Empty;

        public static string GetAdditionalHttpResponseInformation(dynamic result) => string.Empty;

        public static string GetModuleFailureResponseInformation(dynamic result) => string.Empty;
    }
}
