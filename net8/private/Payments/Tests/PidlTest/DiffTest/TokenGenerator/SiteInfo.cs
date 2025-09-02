// <copyright file="SiteInfo.cs" company="Microsoft">Copyright (c) Microsoft 2016. All rights reserved.</copyright>

namespace PidlTest
{
    internal class SiteInfo
    {
        // Replace LiveIdEnvironment with a string or a custom enum if needed
        public string Environment { get; set; }

        public string SiteUrl { get; set; }

        public string SiteId { get; set; }

        public string AuthPolicy { get; set; }
    }
}
