param(
  [string]$pfxFileName,
  [string]$pfxPassword,
  [string]$dirPath,
  [string]$solutionDir
)
$signtoolFilepath = [System.IO.Path]::Combine($solutionDir, "msbuild\Build\Signing\signtool\signtool.exe")
$catFiles = Get-ChildItem -File -Name -Recurse -Include "*.cat" -Path $dirPath
forEach ($catFile in $catFiles)
{
    $catFileName = "$dirPath\$catFile"
    write-host "Running signtool.exe sign /f `"$pfxFileName`" /p `"$pfxPassword`" `"$catFileName`""
    $process = (Start-Process "`"$signtoolFilepath`"" "sign /f `"$pfxFileName`" /p `"$pfxPassword`" `"$catFileName`"" -Wait -PassThru -NoNewWindow)

    if ($process.ExitCode -ne 0)
    {
        throw ("signtool.exe exited with exit code {0}" -f $process.ExitCode)
    } 
    else
    {
        write-host "Signing completed for $catFileName"
    }
} 

write-host "Successfull"
$global:LASTEXITCODE = 0