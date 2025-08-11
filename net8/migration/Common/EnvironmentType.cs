// <copyright file="EnvironmentType.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Environments
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
        /// Integration Environment
        /// </summary>
        Integration,

        /// <summary>
        /// PPE Environment
        /// </summary>
        PPE,

        /// <summary>
        /// Production Environment
        /// </summary>
        Production,

        /// <summary>
        /// Aircapi Environment
        /// </summary>
        Aircapi
    }
}
