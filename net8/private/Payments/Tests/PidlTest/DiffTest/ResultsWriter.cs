// <copyright file="ResultsWriter.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace PidlTest.Diff
{
    using System;
    using System.IO;
    using System.Threading;

    internal class ResultsWriter
    {
        private readonly bool writeTriagedDiffsToCSV;
        private StreamWriter overviewWriter;
        private StreamWriter failuresWriter;
        private Mutex mut;

        public ResultsWriter(string outputPath, string baseEnv, string testEnv)
        {
            this.Progress = 0;
            var timeNow = DateTime.Now;
            string fileNameWithoutExt = string.Format("{0}{1}_vs_{2}-{3:yyMMdd-HHmm}", outputPath, testEnv.ToUpper(), baseEnv.ToUpper(), timeNow);
            if (File.Exists(fileNameWithoutExt + ".csv") || File.Exists(fileNameWithoutExt + ".txt"))
            {
                fileNameWithoutExt += timeNow.ToString("ss");
            }

            this.FailuresFilePath = fileNameWithoutExt + ".csv";
            this.OverviewFilePath = fileNameWithoutExt + ".txt";

            Directory.CreateDirectory(outputPath);
            this.Header = string.Format("sep=~\nUrl~Resource Name~Identity~Country~Language~Partner~Operation~Scenario~Filter~AllowedPaymentMethods~DeltaType/Description~Path~In {0}~In {1}~{0} Response~{1} Response~PI State~PI Scenario~Data~Triage", baseEnv, testEnv);
            this.failuresWriter = File.AppendText(this.FailuresFilePath);
            this.overviewWriter = File.AppendText(this.OverviewFilePath);

            this.failuresWriter.WriteLine(this.Header);
            this.mut = new Mutex();
            this.BaseEnv = baseEnv;
            this.TestEnv = testEnv;

            // Writes TriagedDiffs only for non selfhost run i.e. Env vs Env / PPE vs PROD
            this.writeTriagedDiffsToCSV = !HostingUtility.IsSelfHostRun(baseEnv, testEnv);
        }

        public string FailuresFilePath { get; private set; }

        public string OverviewFilePath { get; private set; }

        public int Progress { get; private set; }

        public string Header { get; private set; }

        public string BaseEnv { get; private set; }

        public string TestEnv { get; private set; }

        public void Write(TestRun testRun)
        {
            this.Write(new OutputDescription()
            {
                IsSuccess = testRun.IsComparisonSuccess,
                WriteTriagedDiffs = this.writeTriagedDiffsToCSV,
                Identity = testRun.Test.Path.GetPidlIdentity(),
                Criteria = testRun.Test,
                BaseLineResponse = (testRun.BaseLineTestResponse != null) ? ((int)testRun.BaseLineTestResponse.StatusCode).ToString() : "N/A",
                UnderTestResponse = (testRun.UnderTestResponse != null) ? ((int)testRun.UnderTestResponse.StatusCode).ToString() : "N/A",
                ComparisionErrors = testRun.UnexpectedDiffs,
                ExecutionError = testRun.FailedExecution
            });
        }

        public void Write(OutputDescription entry)
        {
            this.mut.WaitOne();

            this.overviewWriter.WriteLine(
                 "{0} - {1} | {2}",
                 entry.IsSuccess,
                 entry.Criteria.Path.ToString(this.BaseEnv.Equals("Local", StringComparison.InvariantCultureIgnoreCase) && this.TestEnv.Equals("Local", StringComparison.InvariantCultureIgnoreCase), entry.Criteria.State, entry.Criteria.PIID),
                 (entry.Criteria.Content != null) ? entry.Criteria.Content.Name : "No Content");
            this.overviewWriter.Flush();

            if (!entry.IsSuccess)
            {
                string outputDiff = entry.OutputDiff(this.BaseEnv, this.TestEnv);

                if (!string.IsNullOrEmpty(outputDiff))
                {
                    this.failuresWriter.WriteLine(outputDiff);
                    this.failuresWriter.Flush();
                }
            }

            this.Progress++;

            this.mut.ReleaseMutex();
        }
    }
}