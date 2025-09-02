namespace TestRunner
{
    using System;

    class EnvironmentInfo
    {
        public string TestSettingFile { get; set; }
        
        public string TestFilter { get; set; }

        public string[] TestDlls { get; set; }

        public string TestDllsFolder { get; set; }

        public string TestsRunnerPath { get; set; }

        public string StorageBlobUriTemplate { get; set; }

        public string TestResultsFolder { get; set; }

        public static EnvironmentInfo Create()
        {
            return new EnvironmentInfo()
            {
                TestSettingFile = Environment.GetEnvironmentVariable("TestSettingFile"),
                TestFilter = Environment.GetEnvironmentVariable("TestFilter"),
                TestDlls = Environment.GetEnvironmentVariable("TestDlls").Split(','),
                TestDllsFolder = Environment.GetEnvironmentVariable("TestDllsFolder"),
                TestsRunnerPath = Environment.GetEnvironmentVariable("TestRunnerPath"),
                StorageBlobUriTemplate = Environment.GetEnvironmentVariable("StorageBlobUriTemplate"),
                TestResultsFolder = Environment.GetEnvironmentVariable("TestResultsFolder")
            };
        }
    }
}
