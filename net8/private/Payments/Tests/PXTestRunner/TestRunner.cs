using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;

namespace PXTestRunner
{
    public static class TestRunner
    {
        private static TraceWriter Log;
       

        [FunctionName("TestRunner")]
        public static async Task Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, Microsoft.Azure.WebJobs.ExecutionContext context, TraceWriter log)
        {
            Log = log;
            EnvironmentInfo envInfo = EnvironmentInfo.Create();
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");
            string root = Path.Combine(context?.FunctionDirectory, @"..\");
            ExecuteTest(root, envInfo.TestsRunnerPath, envInfo.TestDllsFolder, envInfo.TestDlls, envInfo.TestSettingFile, envInfo.TestFilter, log);
            await StoreLog(Path.Combine(root, "TestResult"));
        }

        private static async Task StoreLog(string testEvidenceDirectory)
        {
            string connectionString = string.Empty;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("cotlog");
            await container.CreateIfNotExistsAsync();
            DirectoryInfo resultsDirectory = new DirectoryInfo(testEvidenceDirectory);
            Dictionary<string, FileInfo> evidenceFiles = resultsDirectory.GetFiles("*.trx", SearchOption.AllDirectories).ToDictionary(f => f.Name);
            foreach (FileInfo file in evidenceFiles.Values)
            {

                var blob = container.GetBlockBlobReference(file.Name);
                blob.Properties.ContentType = "application/xml";

                using (FileStream fileStream = File.OpenRead(file.FullName))
                {
                    MemoryStream memStream = new MemoryStream();
                    memStream.SetLength(fileStream.Length);
                    fileStream.Read(memStream.GetBuffer(), 0, (int)fileStream.Length);
                    blob.UploadFromStream(fileStream);
                }
            }
        }

        private static void ExecuteTest(
            string testRoot, 
            string testRunnerPath, 
            string testDllFolder, 
            string[] testDlls,
            string testSettingFile,
            string testFilter,
            TraceWriter log)
        {
            Array.ForEach(testDlls, testDll =>
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(testRoot, testRunnerPath),
                    Arguments = $"{Path.Combine(testRoot, testDllFolder, testDll)} /Logger:trx /settings:{testSettingFile} /Tests:{testFilter}",
                    WorkingDirectory = testRoot,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };
                log.Info(startInfo.FileName);
                log.Info(startInfo.Arguments);

                Process process = new Process();
                process.StartInfo = startInfo;
                process.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
                process.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);
                process.Start();
                process.BeginOutputReadLine();
                process.WaitForExit();
                process.Close();

                log.Info("Finished executing VSTest");
            });
         }

        static void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            //* Do your stuff with the output (write to console/log/StringBuilder)
            if (!String.IsNullOrEmpty(outLine?.Data))
            {
                Log.Info(outLine?.Data);
            }
        }

    }
}
