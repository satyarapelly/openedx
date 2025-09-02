// <copyright file="CustomerHeader.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7.Contexts
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Net.Http;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PXService;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the class for customer header jwt token passed from pifd to px via x-ms-customer header.
    /// Referred from CustomerHeader.cs in PIMS.
    /// https://microsoft.visualstudio.com/Universal%20Store/_git/SC.csPayments.Pims.Main?path=%2Fprivate%2FInstrumentManagement%2FCommon%2FContexts%2FCustomerHeader.cs
    /// </summary>
    public class CustomerHeader
    {
        public CustomerHeader(string token, TargetCustomer targetCustomer, string authType)
        {
            this.TargetCustomer = targetCustomer;
            this.HeaderValue = token;
            this.AuthType = authType;
        }

        public TargetCustomer TargetCustomer { get; private set; }

        public string HeaderValue { get; private set; }

        public string AuthType { get; private set; }

        public static CustomerHeader Parse(HttpRequestMessage request)
        {
            CustomerHeader customerHeader = null;
            if (request.Headers.TryGetValues(GlobalConstants.HeaderValues.CustomerHeader, out IEnumerable<string> headerValues))
            {
                string customerInfoStr = headerValues.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(customerInfoStr))
                {
                    try
                    {
                        var jwt = new JwtSecurityToken(customerInfoStr);

                        string target = jwt?.Claims?.Where(c => string.Equals(c.Type, Constants.CustomerHeaderJwtTokenClaim.Target, StringComparison.InvariantCulture)).Select(x => x.Value).FirstOrDefault();
                        string authType = jwt?.Claims?.Where(c => string.Equals(c.Type, Constants.CustomerHeaderJwtTokenClaim.AuthType, StringComparison.InvariantCulture)).Select(x => x.Value).FirstOrDefault();

                        TargetCustomer targetCustomer = target == null ? null : JsonConvert.DeserializeObject<TargetCustomer>(target);
                        customerHeader = new CustomerHeader(customerInfoStr, targetCustomer, authType);
                    }
                    catch (Exception ex)
                    {
                        SllWebLogger.TracePXServiceException("CustomerHeader.TryParse: " + ex.ToString(), EventTraceActivity.Empty);                                                
                    }
                }
            }

            return customerHeader;
        }
    }
}
