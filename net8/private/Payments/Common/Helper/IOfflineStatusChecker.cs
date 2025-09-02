// <copyright file="IOfflineStatusChecker.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Helper
{
    public interface IOfflineStatusChecker
    {
        /// <summary>
        ///     Get offline status for current machine
        /// </summary>
        /// <returns>Whether a machine is marked for offlining</returns>
        bool GetOfflineState();
    }
}