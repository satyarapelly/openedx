// <copyright file="EnvironmentType.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tools.InstallCertificates
{
    public enum EnvironmentType
    {
        /// <summary>
        /// Environment is unknown
        /// </summary>
        None,

        /// <summary>
        /// One box environment
        /// </summary>
        OneBox,

        /// <summary>
        /// AP One box environment
        /// </summary>
        APOneBox,

        /// <summary>
        /// Autopilot Integration Environment
        /// </summary>
        Integration,

        /// <summary>
        /// Autopilot Production Environment
        /// </summary>
        Production
    }
}
