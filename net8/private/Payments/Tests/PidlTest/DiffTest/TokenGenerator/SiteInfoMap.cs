// <copyright file="SiteInfoMap.cs" company="Microsoft">Copyright (c) Microsoft 2016. All rights reserved.</copyright>
namespace PidlTest
{
    using System;
    using System.Collections.Generic;

    internal class SiteInfoMap
    {
        private const string AuthPolicyMbiSsl = "MBI_SSL";

        private static SiteInfoMap instance = new SiteInfoMap();
        private Dictionary<string, SiteInfo> map = null;

        private SiteInfoMap()
        {
            this.map = new Dictionary<string, SiteInfo>
            {
                {
                    "ppe",
                    new SiteInfo
                    {
                        Environment = "Production",
                        AuthPolicy = AuthPolicyMbiSsl,
                        SiteUrl = "https://pifd.cp.microsoft.com",
                        SiteId = "294486"
                    }
                },
                {
                    "prod",
                    new SiteInfo
                    {
                        Environment = "Production",
                        AuthPolicy = AuthPolicyMbiSsl,
                        SiteUrl = "https://www.microsoft.com",
                        SiteId = "74335"
                    }
                },
                {
                    "feature",
                    new SiteInfo
                    {
                        Environment = "Production",
                        AuthPolicy = AuthPolicyMbiSsl,
                        SiteUrl = "https://www.microsoft.com",
                        SiteId = "74335"
                    }
                },
                {
                    "int",
                    new SiteInfo
                    {
                        Environment = "INT",
                        AuthPolicy = AuthPolicyMbiSsl,
                        SiteUrl = "https://pifd.cp.microsoft-int.com",
                        SiteId = "294486"
                    }
                },
                {
                    "prodint",
                    new SiteInfo
                    {
                        Environment = "Production",
                        AuthPolicy = AuthPolicyMbiSsl,
                        SiteUrl = "https://commerce.windows-int.net",
                        SiteId = "293745"
                    }
                }
            };
        }

        public static SiteInfoMap Instance => instance;

        public SiteInfo GetSiteInfo(string env)
        {
            if (!this.map.TryGetValue(env, out SiteInfo siteInfo))
            {
                throw new ArgumentException("Unsupported environment " + env);
            }

            return siteInfo;
        }
    }
}
