// <copyright file="SllEnvironmentSetting.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Tracing
{
    public class SllEnvironmentSetting
    {
        public string SllLogPath { get; set; }

        public string SllLogNamePrefix { get; set; }

        public long SllMaxFileSizeBytes { get; set; }

        public int SllMaxFileCount { get; set; }
    }
}
