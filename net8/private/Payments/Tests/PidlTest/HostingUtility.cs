// <copyright file="HostingUtility.cs" company="Microsoft">Copyright (c) Microsoft 2016. All rights reserved.</copyright>
namespace PidlTest
{
    using System;

    internal static class HostingUtility
    {
        public static bool IsSelfHostRun(string baseEnvironment, string testEnvironment)
        {
            var isBaseEnvironmentSelfHost = baseEnvironment.Equals(Constants.Environment.SelfHost, StringComparison.InvariantCultureIgnoreCase) ||
                   baseEnvironment.Equals(Constants.Environment.SelfHost, StringComparison.InvariantCultureIgnoreCase);

            var isTestEnvironmentSelfHost = testEnvironment.Equals(Constants.Environment.SelfHost, StringComparison.InvariantCultureIgnoreCase) ||
                   testEnvironment.Equals(Constants.Environment.SelfHost, StringComparison.InvariantCultureIgnoreCase);

            return isBaseEnvironmentSelfHost || isTestEnvironmentSelfHost;
        }
        
        public static bool IsPipelineRun()
        {
            // CDP_DEFAULT_CLIENT_PACKAGE_PAT is an environment variable that only exists on cdpx and onebranch pipeline agents.
            var pat = Environment.GetEnvironmentVariable("CDP_DEFAULT_CLIENT_PACKAGE_PAT");
            if (string.IsNullOrEmpty(pat))
            {
                return false;
            }

            return true;
        }
    }
}