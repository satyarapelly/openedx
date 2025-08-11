// <copyright file="ApiVersionHandler.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Web
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;    

    /// <summary>
    /// Delegating handler which validates that an appropriate api-version is
    /// passed with the request.
    /// </summary>
    public class ApiVersionHandler : DelegatingHandler
    {
        public ApiVersionHandler()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionHandler"/> class.
        /// </summary>
        /// <param name="supportedVersions">A dictionary from api-version string to internal
        /// version number which represents the set of supported versions.</param>
        public ApiVersionHandler(IDictionary<string, ApiVersion> supportedVersions)
        {
            Debug.Assert(supportedVersions != null, "supported versions cannot be none");

            this.SupportedVersions = supportedVersions;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public IDictionary<string, ApiVersion> SupportedVersions { get; set; }

        public void SetSupportedVersions(IDictionary<string, ApiVersion> versions)
        {
            this.SupportedVersions = versions;
        }

        /// <summary>
        /// Extracts the api-version from the request and validates it.  If validation
        /// fails, a response will be returned immediately.
        /// </summary>
        /// <param name="request">The inbound request.</param>
        /// <param name="cancellationToken">A token which may be used to listen
        /// for cancellation.</param>
        /// <returns>The outbound response.</returns>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string externalVersion = null;

            IEnumerable<string> apiVersionHeaderValues;
            if (request.Headers.TryGetValues(PaymentConstants.PaymentExtendedHttpHeaders.ApiVersion, out apiVersionHeaderValues))
            {
                // NOTE: headerValue may be a comma separated list of passed in
                // values.  We don't bother to parse this since we do not support
                // multiple api-version values.  The result is that some "multiple
                // version" errors will be reported as "invalid version".
                foreach (string headerValue in apiVersionHeaderValues)
                {
                    if (externalVersion == null)
                    {
                        externalVersion = headerValue;
                    }
                    else
                    {
                        return Task.FromResult(request.CreateMultipleApiVersionsResponse());
                    }
                }
            }

            // NOTE: For V3.0 or upper version, we use api-version in query parameters as version and ignore the version in header
            // Sample: https://paymentsinstrumentservice.cp.microsoft-int.com/InstrumentManagementService/v3.0/paymentMethods
            // Sample: https://paymentsinstrumentservice.cp.microsoft-int.com/InstrumentManagementService/v3.0/{caid}/paymentInstruments/{id}?country=us
            string versionPattern = @"^v\d+\.\d+/$";
            Regex rgx = new Regex(versionPattern, RegexOptions.IgnoreCase);

            foreach (string seg in request.RequestUri.Segments)
            {
                try
                {
                    MatchCollection matches = rgx.Matches(seg);
                    if (matches.Count == 1)
                    {
                        string versionInUrl = matches[0].Value.Substring(0, matches[0].Value.Length - 1);
                        decimal.Parse(versionInUrl.Substring(1));
                        externalVersion = versionInUrl;
                        break;
                    }
                }
                catch
                {
                }
            }

            IEnumerable<KeyValuePair<string, string>> queryStrings = null;
            if (request.Properties.ContainsKey(PaymentConstants.Web.Properties.QueryParameters))
            {
                queryStrings = request.Properties[PaymentConstants.Web.Properties.QueryParameters] as IEnumerable<KeyValuePair<string, string>>;
            }

            if (queryStrings == null)
            {
                queryStrings = request.GetQueryNameValuePairs();
                if (queryStrings != null)
                {
                    request.Properties.Add(new KeyValuePair<string, object>(PaymentConstants.Web.Properties.QueryParameters, queryStrings));
                }
            }

            if (externalVersion == null)
            {
                return Task.FromResult(request.CreateNoApiVersionResponse());
            }

            ApiVersion apiVersion;
            if (!this.SupportedVersions.TryGetValue(externalVersion, out apiVersion))
            {
                return Task.FromResult(request.CreateInvalidApiVersionResponse(externalVersion));
            }

            if (request.Properties.ContainsKey(PaymentConstants.Web.Properties.Version))
            {
                request.Properties[PaymentConstants.Web.Properties.Version] = apiVersion;
            }
            else
            {
                request.Properties.Add(PaymentConstants.Web.Properties.Version, apiVersion);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}