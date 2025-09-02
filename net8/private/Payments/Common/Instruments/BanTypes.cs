// <copyright file="BanTypes.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Instruments
{
    using System;

    [Flags]
    public enum BanTypes
    {
        // These enum values should be in sync with the one from
        // private\payments\instrumentstore\datamodel\bantype.cs
        None      = 0x0,
        All       = 0x1,
        OneTime   = 0x2,
        Recurring = 0x4
    }
}
