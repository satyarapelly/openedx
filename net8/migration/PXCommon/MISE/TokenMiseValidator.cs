// <copyright file="TokenMiseValidator.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXCommon
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Identity.ServiceEssentials;
    using Microsoft.IdentityModel.S2S;
    using Microsoft.IdentityModel.S2S.Configuration;

    /// <summary>
    /// Defines the <see cref="TokenMiseValidator" />.
    /// Validate token using MISE+SAL library (modified from PIFD code).
    /// MISE+SAL migration doc: https://identitydivision.visualstudio.com/DevEx/_git/MISE?path=/docs/MigrationGuides/JwtBearerHandler/Readme.md
    /// MISE doc: https://identitydivision.visualstudio.com/DevEx/_git/MISE?path=/readme.md
    /// SAL doc: https://identitydivision.visualstudio.com/DevEx/_wiki/wikis/DevEx.wiki/56/ServiceAuthLibrary-SAL-
    /// </summary>
    public class TokenMiseValidator
    {
        private MiseHost<MiseHttpContext> miseHost;
        private IAuthenticationLogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenMiseValidator"/> class.
        /// </summary>
        /// <param name="aadAuthenticationOptions">The AadAuthenticationOptions<see cref="AadAuthenticationOptions"/>.</param>
        /// <param name="logger">Used for logging. </param>
        public TokenMiseValidator(AadAuthenticationOptions aadAuthenticationOptions, IAuthenticationLogger logger)
        {
            // Initialize SAL
            S2SAuthenticationManager s2sAuthenticationManager = S2SAuthenticationManagerFactory.Default.BuildS2SAuthenticationManager(aadAuthenticationOptions);

            // Initialize MISE
            this.miseHost = MiseBuilder.Create(new ApplicationInformationContainer(aadAuthenticationOptions.ClientId))
                   .WithDefaultAuthentication(s2sAuthenticationManager)
                   .ConfigureDefaultModuleCollection(builder => 
                   {
                   })
                   .Build();

            // Initialize Logger
            this.logger = logger;
        }

        /// <summary>
        /// Get Claims using MISE+SAL library.
        /// </summary>
        /// <param name="authenticationHeaderValue">The authentication header.</param>
        /// <param name="incomingRequestId">The unique id of incoming request and it can be CV if available.</param>
        /// <param name="cancellationToken">The cancellationToken.</param>
        /// <returns>The <see cref="Task{ClaimsPrincipal}"/>.</returns>
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
                // https://datatracker.ietf.org/doc/html/rfc6749 section 5.1
                // token_type
                // REQUIRED.The type of the token issued as described in
                // Section 7.1.Value is case insensitive.
                result.Failed(Constants.AuthenticationErrorCode.InvalidSchema, "Non Bearer schema");
            }
            else if (string.IsNullOrEmpty(authenticationHeaderValue.Parameter))
            {
                result.Failed(Constants.AuthenticationErrorCode.NoSecurityTokenFound, "Invalid Token Provided", new ArgumentException("Authentication Token cannot be null or empty."));
            }

            // Return early failure if authentication header is malformed or token is missing.
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
                // Obtain http request data from your stack
                var httpRequestData = new HttpRequestData();
                httpRequestData.Headers.Add("Authorization", "Bearer " + token);

                // 1. create mise http context object (for each request)
                var context = new MiseHttpContext(httpRequestData)
                {
                    CorrelationId = incomingRequestId
                };

                // 2. execute mise (for each request)
                var miseResult = await this.miseHost.HandleAsync(context, cancellationToken).ConfigureAwait(false);

                // 3. examine results (for each request)
                if (miseResult != null && miseResult.Succeeded)
                {
                    // 3.1 examine additional headers and cookies produced by modules
                    string instanceName = miseResult.GetInstanceName();
                    message = miseResult.GetAdditionalHttpResponseInformation();

                    ClaimsPrincipal claims = new ClaimsPrincipal(miseResult.AuthenticationTicket.SubjectIdentity ?? miseResult.AuthenticationTicket.ActorIdentity);
                    result.Succeed(claims, instanceName, message);

                    result.ApplicationId = GetValueFromClaims(result.TokenPrincipal, "appid");
                }
                else
                {
                    // 3.2 examine failure, and/or http response produced by a module that failed to handle the request
                    var moduleCreatedFailureResponse = miseResult?.MiseContext?.ModuleFailureResponse;
                    if (moduleCreatedFailureResponse != null)
                    {
                        message = miseResult.GetModuleFailureResponseInformation();
                        string statusCode = moduleCreatedFailureResponse.StatusCode.ToString();
                        result.Failed(Constants.AuthenticationErrorCode.SecurityTokenValidationFailed, $"HTTP status code: {statusCode}. Message {message}", miseResult.Failure);
                    }
                    else
                    {
                        result.Failed(Constants.AuthenticationErrorCode.EmptyModuleCreatedFailureResponse, "Get empty moduleCreatedFailureResponse.", miseResult.Failure);
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

        private static string GetValueFromClaims(ClaimsPrincipal tokenPrincipal, string claimType) => tokenPrincipal.Claims.FirstOrDefault(claim => claim.Type.Equals(claimType, StringComparison.OrdinalIgnoreCase))?.Value;
    }
}
