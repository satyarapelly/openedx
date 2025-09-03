# Creates 2 child threads to build the current code base and the release code base

$scriptBlock = {
    param($workingDirectory, $url)
    msbuild /p:Configuration=Release $workingDirectory\private\Payments\Payments.sln
}

$rootLocation = (Get-Location).Path
$buildJob = Start-Job $scriptBlock -ArgumentList $rootLocation
$buildJobRelease = Start-Job $scriptBlock -ArgumentList $rootLocation\SC.CSPayments.PX.Release
Wait-Job $buildJobRelease,$buildJob

Write-Output "##############Baseline build output##############"
Receive-Job -Job $buildJob

Write-Output "##############Target build output##############"
Receive-Job -Job $buildJobRelease

if (! $?) {
    throw "Nuget restore failed."
}
