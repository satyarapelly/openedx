# DO NOT MODIFY THIS FILE
$baseDirectory = Split-Path (Split-Path -Path $MyInvocation.MyCommand.Definition -Parent) -Parent

. $baseDirectory/.build/variables.ps1

Write-Host ("restoring infra project " + $libraryContainingProject)
nuget restore (Join-Path $baseDirectory $libraryContainingProject)
if ($LastExitCode -ne 0) {
	$msg = @"
EXE RETURNED EXIT CODE $LastExitCode
CALLSTACK:$(Get-PSCallStack | Out-String)
"@
	throw $msg
}else{
	Write-Host ("finished restoring infra project " + $libraryContainingProject)
}

#Note that get-childitem's return objects have different base string representations in linux and windows, so make sure to use FullName
$packageVersionFolders = @()

$packageVersionFolders = get-childitem $Home/.nuget/packages/Commerce.Shared.AppService.CDPxBuild -Directory

if ($packageVersionFolders.count -eq 1)
{
	$file = $packageVersionFolders[0].FullName
	Write-Host $file
} elseif ($packageVersionFolders.count -ge 1) {
	Write-Host "ERROR: More than one version of Commerce.Shared.AppService.CDPxBuild Directory in Nuget cache"
	exit 1
} else {
	Write-Host "ERROR: Unable to find Commerce.Shared.AppService.CDPxBuild Directory in Nuget cache"
	exit 1
}

# Add build files from the nuget package
Copy-Item -Path $file/.buildCDPx -Destination $baseDirectory -Recurse

if ($allowYamlCustomizations -eq $false){
	# Replace all pipelines files with those in the nuget package
	Write-Host "allowYamlCustomizations is False. Will replace all file in .pipelines folder with ones from nuget"
	Remove-Item $baseDirectory/.pipelines -recurse -force -confirm:$false
	Copy-Item -Path $file/.pipelinesReplacement -Destination $baseDirectory/.pipelines -Recurse
} else {
	# Replace only the pipeline.user.windows.yml file with those in the nuget package
	Write-Host "allowYamlCustomizations is True. Will replace only the pipeline.user.windows.yml file in .pipelines folder with one from nuget"
	Remove-Item $baseDirectory/.pipelines/pipeline.user.windows.yml -force -confirm:$false
	Copy-Item -Path $file/.pipelinesReplacement/* -Destination $baseDirectory/.pipelines/. -Recurse
}

. $baseDirectory/.buildCDPx/restore.ps1
