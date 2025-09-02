// <copyright file="Constants.cs" company="Microsoft">Copyright (c) Microsoft 2019. All rights reserved.</copyright>
namespace PidlTest.JsonDiff
{
    using System.Collections.Generic;

    internal static class Constants
    {
        internal static class DiffTest
        {
            //// Used to build a paths for KnownDiffs (See KnownDiffsConfig.cs)
            //// Identifies any value for a given parameter
            //// i.e. for sets where Country = Any, Language = Any, partner = "webblends" apply KnownDiffs
            public const string Any = "_Any_";
            //// Used by SkipPidlCombinations to identify pidls that are missing a parameter
            //// i.e. family: "virtual", type: NotSent
            public const string NotSent = "_NotSent_";

            // Properties that would vary from version to version with-in PIDL and can be skipped
            public static List<string> SkipPropertyComparision
            {
                get
                {
                    return new List<string>
                    {
                        "ipAddress", // this will never be the same between test sites
                        "x-ms-correlation-id",
                        "x-ms-tracking-id"
                    };
                }
            }
        }
    }
}
