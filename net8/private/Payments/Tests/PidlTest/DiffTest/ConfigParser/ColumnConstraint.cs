// <copyright file="ColumnConstraint.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace PidlTest.Diff
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    [Flags]
    [SuppressMessage("Microsoft.Naming", "CA1714", Justification = "Used by the CSV reader across multiple projects")]
    public enum ColumnConstraint
    {
        Optional = 0x01,
        Required = 0x02,
        Unique = 0x04,
        DoNotTrimWhiteSpaces = 0x08
    }
}
