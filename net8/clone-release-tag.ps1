Write-Output "Fetching git tags"
git fetch --all --tags

$latestTag = git tag -l "release*" --sort=-taggerdate | Select-Object -first 1
$commitId = git rev-list -n 1 $latestTag

$adoOrgName="microsoft"
$adoProjectName="Universal%20Store"
$adoRepoName="SC.CSPayments.PX"
$outputDir="SC.CSPayments.PX.Release"

if ($Env:CDP_DEFAULT_CLIENT_PACKAGE_PAT) {
	$repoUrl="https://patuser:$Env:CDP_DEFAULT_CLIENT_PACKAGE_PAT@$adoOrgName.visualstudio.com/$adoProjectName/_git/$adoRepoName"
	Write-Output "Attempting to git clone"
	git clone --branch $latestTag --progress --verbose --recursive $repoUrl $outputDir
} else {
	$rootLocation = (Get-Location).Path
	$repoExists = Test-Path -Path $rootLocation\SC.CSPayments.PX.Release
	
	if($repoExists){
		Write-Output "checking out latest release code tag"
		Set-Location  $rootLocation\$outputDir
		
		git checkout $commitId
		
		Set-Location  $rootLocation
	} else{
		$repoUrl="https://microsoft.visualstudio.com/DefaultCollection/$adoProjectName/_git/$adoRepoName"
		Write-Output "Cloning master from https://microsoft.visualstudio.com/DefaultCollection/Universal%%20Store/_git/SC.CSPayments.PX"
		
		git clone --branch $latestTag --progress --verbose --recursive $repoUrl $outputDir
	}
}

if (! $?) {
    throw "Clone failed."
}

If ($lastExitCode -ne "0" -or $lastExitCode -ne 0) {
    Write-Output "Exit code: " $lastExitCode
    $exitCode = 1
}
