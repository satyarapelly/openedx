# Creates 2 self hosted servers as child processes. One for the current code and the other one for the release tagged code.
# Once the servers are running, it executes the difftest tool pointing to those servers.

$code = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

namespace PXAutomation
{
    public static class Tools
    {
        public static List<int> PreRegisteredPorts = new List<int>();

        public static string GetAvailablePort()
        {
            var netProperties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpListeners = netProperties.GetActiveTcpListeners();
            var udpListeners = netProperties.GetActiveUdpListeners();

            var portsInUse = new List<int>();
            portsInUse.AddRange(tcpListeners.Select(tl => tl.Port));
            portsInUse.AddRange(udpListeners.Select(ul => ul.Port));

            int firstAvailablePort = 0;
            for (int port = 49152; port < 65535; port++)
            {
                if (!portsInUse.Contains(port) && !PreRegisteredPorts.Contains(port))
                {
                    firstAvailablePort = port;
                    break;
                }
            }

            return firstAvailablePort.ToString();
        }
    }
}
"@

$scriptBlock = {
    param($workingDirectory, $url)
    Write-Output "Starting selfhosted server"
    $workingDirectory = $workingDirectory + "\Drop\Debug\UnitTests\SelfHostedPXService"
    Set-Location $workingDirectory
    .\SelfHostedPXService.exe $url
}

Add-Type -TypeDefinition $code -Language CSharp

$rootLocation = (Get-Location).Path
$appPath = (Get-Location).Path

Write-Output "Creating BASELINE HttpSelfHosted Server Thread for release tag:"
git tag -l "release*" --sort=-taggerdate | Select-Object -first 1

$port = Invoke-Expression "[PXAutomation.Tools]::GetAvailablePort()"

# We need to reserve this port so if the baseline server fails, the target server doesn't take it,
# it would cause to pass all the test when in reallity they all should fail
Invoke-Expression "[PXAutomation.Tools]::PreRegisteredPorts.Add($port)"

$url = "http://localhost:" + $port + "/"

$serverJob = Start-Job $scriptBlock -ArgumentList $appPath\SC.CSPayments.PX.Release, $url

Write-Output "Wait for server to start"
Start-Sleep 15 # Need to wait so it can pick up the port and use it
Receive-Job -Job $serverJob
If ($serverJob.State -eq "Completed") {
    throw "Unable to create baseline server."
}

Write-Output "Created BASELINE HttpSelfHosted Server Thread on " $url

Write-Output "Creating TARGET HttpSelfHosted Server Thread"
$targetPort = Invoke-Expression "[PXAutomation.Tools]::GetAvailablePort()"
$targetUrl = "http://localhost:" + $targetPort + "/"
$releaseServerJob = Start-Job $scriptBlock -ArgumentList $appPath, $targetUrl
Write-Output "Wait for server to start"
Start-Sleep 15 # Need to wait so it can pick up the port and use it
Receive-Job -Job $releaseServerJob
 If ($releaseServerJob.State -eq "Completed") {
     throw "Unable to create targer server."
 }
Write-Output "Created TARGET HttpSelfHosted Server Thread on " $targetUrl

Write-Output "Running DiffTests"
Set-Location -Path "private\Payments\Tests\PidlTest\bin\Debug\net8.0\"
$url = $url + "v7.0/"
$targetUrl = $targetUrl + "v7.0/"

.\PidlTest.exe /test DiffTest /BaseEnv SelfHost /TestEnv SelfHost /BaseUrl $url /TestUrl $targetUrl /TriagedDiffFilePath "\DiffTest\ConfigFiles\TriagedDiffs\SELFHOST_vs_SELFHOST.csv"
if ($Env:CDP_DEFAULT_CLIENT_PACKAGE_PAT) {
	$src = $rootLocation + "\private\Payments\Tests\PidlTest\bin\Debug\net8.0\logs"
	$dest = $rootLocation + "\out\DiffTest"
	Copy-Item $src -Destination  $dest -Recurse
}

$exitCode = 0;
If ($lastExitCode -ne "0" -or $lastExitCode -ne 0) {
    Write-Output "Exit code: " $lastExitCode
    $exitCode = 1
}

Set-Location -Path $rootLocation

#Kill server job and reset location
Stop-job -Job $serverJob
Remove-Job $serverJob -Force

Stop-job -Job $releaseServerJob
Remove-Job $releaseServerJob -Force

Set-Location -Path $rootLocation

If ($exitCode -ne 0) {
    throw "End to end tests have failed."
}

Write-Output "End to end tests passed!"
exit 0