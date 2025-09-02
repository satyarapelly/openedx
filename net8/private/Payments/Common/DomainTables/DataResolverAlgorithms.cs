// <copyright file="DataResolverAlgorithms.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.DomainTables
{
    using System;

    [Flags]
    public enum DataResolverAlgorithms
    {
        /// <summary>
        /// Resolve from right to left. The most specific criteria is on the right. It is default behavior.
        /// </summary>
        RightToLeft = 1,

        /// <summary>
        /// Resolve from the max exact match numbers.
        /// </summary>
        MaxMatchNumber = 2,
    }
}
