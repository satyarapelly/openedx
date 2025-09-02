// <copyright file="TestResult.cs" company="Microsoft">Copyright (c) Microsoft 2016. All rights reserved.</copyright>

namespace PidlTest.Diff
{
    internal class TestResult
    {
        public readonly bool IsComparisonSuccess;

        public readonly string Url;

        public TestResult(bool isComparisonSuccess, string url)
        {
            this.IsComparisonSuccess = isComparisonSuccess;
            this.Url = url;
        }
    }
}