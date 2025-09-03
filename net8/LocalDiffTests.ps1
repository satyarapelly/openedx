[CmdletBinding()]
param
(
  	$full = $false
)

if ($full){
	Write-Output "Cloning repo"
	.\clone-release-tag.ps1

	If ($lastExitCode -ne "0" -or $lastExitCode -ne 0 -or ! $?) {
		throw "Clone failed."
	}
}

$rootLocation = (Get-Location).Path
$repoExists = Test-Path -Path $rootLocation\SC.CSPayments.PX.Release

If (-Not $repoExists){
	throw "Need to clone release code. Run .\LocalDiffTests.ps1 -full $true";
}

Set-Location $rootLocation\SC.CSPayments.PX.Release

if($full){
	Write-Output "Restoring NuGet packages"
	$repoLocation = (Get-Location).Path
	.\msbuild\nuget\nuget.exe restore .\private\Payments\Payments.sln -ConfigFile .\nuget.config

	If ($lastExitCode -ne "0" -or $lastExitCode -ne 0 -or ! $?) {
		throw "Restore failed."
	}
	
	Write-Output "Build taget release code"
	msbuild /p:Configuration=Release .\private\Payments\Payments.sln

	If ($lastExitCode -ne "0" -or $lastExitCode -ne 0 -or ! $?) {
		throw "Build failed."
	}	
}

Set-Location $rootLocation

.\DiffTests.ps1