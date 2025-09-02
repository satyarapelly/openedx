using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;

namespace TestRunner
{
    public static class TestRunner
    {
        private static ILogger Log;

        [FunctionName("TestRunner")]
        public static async Task Run([TimerTrigger("0 */5 * * * *", RunOnStartup = true)]TimerInfo myTimer, Microsoft.Azure.WebJobs.ExecutionContext context, ILogger log)
        {
            Log = log;
            EnvironmentInfo envInfo = EnvironmentInfo.Create();
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            string root = Path.Combine(context?.FunctionDirectory, @"..\");
            // ExecuteTest(root, envInfo.TestsRunnerPath, envInfo.TestDllsFolder, envInfo.TestDlls, envInfo.TestSettingFile, envInfo.TestFilter, log);
            await StoreLog(Path.Combine(root, "TestResults"), envInfo.StorageBlobUriTemplate);
        }

        private static async Task<NewTokenAndFrequency> TokenRenewerAsync(Object state, CancellationToken cancellationToken)
        {
            // Specify the resource ID for requesting Azure AD tokens for Azure Storage.
            // Note that you can also specify the root URI for your storage account as the resource ID.
            const string StorageResource = "https://storage.azure.com/";

            // Use the same token provider to request a new token.
            var authResult = await ((AzureServiceTokenProvider)state).GetAuthenticationResultAsync(StorageResource);

            // Renew the token 5 minutes before it expires.
            var next = (authResult.ExpiresOn - DateTimeOffset.UtcNow) - TimeSpan.FromMinutes(5);
            if (next.Ticks < 0)
            {
                next = default(TimeSpan);
                Console.WriteLine("Renewing token...");
            }

            // Return the new token and the next refresh time.
            return new NewTokenAndFrequency(authResult.AccessToken, next);
        }

        private static async Task StoreLog(string testEvidenceDirectory, string blobUriTemplate)
        {
            AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
            var tokenAndFrequency = await TokenRenewerAsync(azureServiceTokenProvider, CancellationToken.None);
            TokenCredential tokenCredential = new TokenCredential(tokenAndFrequency.Token,
                                                        TokenRenewerAsync,
                                                        azureServiceTokenProvider,
                                                        tokenAndFrequency.Frequency.Value);

            StorageCredentials storageCredentials = new StorageCredentials(tokenCredential);
            DirectoryInfo resultsDirectory = new DirectoryInfo(testEvidenceDirectory);
            Dictionary<string, FileInfo> evidenceFiles = resultsDirectory.GetFiles("*.trx", SearchOption.AllDirectories).ToDictionary(f => f.Name);
            foreach (FileInfo file in evidenceFiles.Values)
            {

                var blob = new CloudBlockBlob(new Uri(string.Format(blobUriTemplate, file.Name)), storageCredentials);
                blob.Properties.ContentType = "application/xml";
                using (FileStream fileStream = File.OpenRead(file.FullName))
                {
                    MemoryStream memStream = new MemoryStream();
                    memStream.SetLength(fileStream.Length);
                    fileStream.Read(memStream.GetBuffer(), 0, (int)fileStream.Length);
                    await blob.UploadFromStreamAsync(fileStream);
                }
            }

            foreach (FileInfo file in evidenceFiles.Values)
            {
                File.Delete(file.FullName);
            }
        }

        private static void ExecuteTest(
            string testRoot,
            string testRunnerPath,
            string testDllFolder,
            string[] testDlls,
            string testSettingFile,
            string testFilter,
            ILogger log)
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
                log.LogInformation(startInfo.FileName);
                log.LogInformation(startInfo.Arguments);

                Process process = new Process();
                process.StartInfo = startInfo;
                process.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
                process.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);
                process.Start();
                process.BeginOutputReadLine();
                process.WaitForExit();
                process.Close();

                log.LogInformation("Finished executing VSTest");
            });
        }

        static void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            //* Do your stuff with the output (write to console/log/StringBuilder)
            if (!String.IsNullOrEmpty(outLine?.Data))
            {
                Log.LogInformation(outLine?.Data);
            }
        }
    }
}
