// <copyright file="UpdateStaticResourceServiceEndpoint.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Tracing;

    /**
     * This feature is used to update all the logo urls in pidl display hints to point to staticresources CDN
     * Eg. url - https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_visa.svg
     * is replaced with https://staticresources.payments.microsoft-int.com/staticresourceservice/images/v4/logo_visa.svg
     */
    public class UpdateStaticResourceServiceEndpoint : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                ReplaceStaticResourceServiceEndpoint
            };
        }

        internal static void ReplaceStaticResourceServiceEndpoint(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            try
            {
                var staticResourcesCDNEndpoint = Microsoft.Commerce.Payments.Common.Environments.Environment.IsProdOrPPEEnvironment
                                ? Constants.PidlUrlConstants.StaticResourceServiceProdCDNEndpoint
                                : Constants.PidlUrlConstants.StaticResourceServiceIntCDNEndpoint;
                var staticResourcesAFDEndpoint = Microsoft.Commerce.Payments.Common.Environments.Environment.IsProdOrPPEEnvironment
                                ? Constants.PidlUrlConstants.StaticResourceServiceProdAFDEndpoint
                                : Constants.PidlUrlConstants.StaticResourceServiceIntAFDEndpoint;
                foreach (PIDLResource pidl in inputResources)
                {
                    pidl.GetAllDisplayHints()?.ToList().ForEach(displayHint =>
                    {
                        var logoDisplayHint = displayHint as LogoDisplayHint;

                        if (logoDisplayHint != null)
                        {
                            var logoUri = new Uri(logoDisplayHint.SourceUrl);
                            if (logoUri != null && logoUri.GetLeftPart(UriPartial.Authority).Equals(staticResourcesAFDEndpoint))
                            {
                                logoDisplayHint.SourceUrl = staticResourcesCDNEndpoint + logoUri.PathAndQuery;
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                if (LoggingConfig.Mode == LoggingMode.Sll)
                {
                    SllWebLogger.TracePXServiceException("UpdateStaticResourceServiceEndpoint.ReplaceStaticResourceServiceEndpoint: " + ex.ToString(), EventTraceActivity.Empty);
                }
                else if (LoggingConfig.Mode == LoggingMode.OpenTelemetry)
                {
                    Logger.Qos.TracePXServiceException("UpdateStaticResourceServiceEndpoint.ReplaceStaticResourceServiceEndpoint: " + ex.ToString(), EventTraceActivity.Empty);
                }
                else
                {
                    SllWebLogger.TracePXServiceException("UpdateStaticResourceServiceEndpoint.ReplaceStaticResourceServiceEndpoint: " + ex.ToString(), EventTraceActivity.Empty);
                    Logger.Qos.TracePXServiceException("UpdateStaticResourceServiceEndpoint.ReplaceStaticResourceServiceEndpoint: " + ex.ToString(), EventTraceActivity.Empty);
                }
            }
        }
    }
}
