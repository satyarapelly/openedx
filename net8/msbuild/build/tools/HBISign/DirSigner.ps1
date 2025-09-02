param(
  [string]$pfxFileName,
  [string]$pfxPassword,
  [string]$dirPath
)

function ProcessDir
{
  param (
    [System.IO.StreamWriter] $cdf, 
    [string] $dirName
  )
  foreach ($dir in [System.IO.Directory]::EnumerateDirectories($dirName))
  {
    ProcessDir $cdf $dir
  }

  foreach ($filepath in [System.IO.Directory]::EnumerateFiles($dirName))
  {
    $filename = [System.IO.Path]::GetFileName($filepath)
    $fn = $filepath
    if ($fn.StartsWith(".\"))
    {
      $fn = $fn.Substring(2)
    }
    if ([System.IO.Path]::GetFullPath($filepath) -ne $catFileName)
    {
      $cdf.WriteLine("<HASH>{0}={0}`n<HASH>{0}ATTR1=0x10010001:File:{0}", $fn)
    }
  }
}

function GenerateCDF
{
  write-host ("Generating CDF file: {0}" -f $cdfFileName);
  $fs = new-object "System.IO.FileStream" $cdfFileName, "Create"
  $cdf = new-object "System.IO.StreamWriter" $fs

  $cdf.Write(("`r`n[CatalogHeader]`r`nName={0}`r`nPublicVersion=1.0`r`nEncodingType=PKCS_7_ASN_ENCODING`r`n`r`n[CatalogFiles]`r`n" -f $catFileName))
  ProcessDir $cdf "."

  $cdf.Flush()
  $fs.Close()
}

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition


# set the last error code to fail the script if the init steps fail
$global:LASTEXITCODE = 1

# set the current folder to the path that was passed
$originalDir = Get-Location
Set-Location $dirPath
[System.Environment]::CurrentDirectory = $dirPath

write-host "generate the cdf file"
$catFileName = [System.IO.Path]::GetFullPath([System.IO.Path]::Combine($dirPath, [System.IO.Path]::GetFileName($dirPath) + ".cat"))
$cdfFileName = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), [System.IO.Path]::GetRandomFileName() + ".cdf")
GenerateCDF $cdfFileName $catFileName

write-host "Running makecat.exe -v -r `"$cdfFileName`""
$makeCatPath = [System.IO.Path]::Combine($scriptDir, "makecat.exe")
$process = (Start-Process $makeCatPath "-v -r `"$cdfFileName`"" -Wait -PassThru -NoNewWindow -WorkingDirectory $dirPath)
if ($process.ExitCode -ne 0)
{
  throw ("makecat.exe exited with exit code {0}" -f $process.ExitCode)
}

write-host "Running signtool.exe sign /f `"$pfxFileName`" /p `"$pfxPassword`" `"$catFileName`""
$signToolPath = [System.IO.Path]::Combine($scriptDir, "signtool.exe")
$process = (Start-Process $signToolPath "sign /f `"$pfxFileName`" /p `"$pfxPassword`" `"$catFileName`"" -Wait -PassThru -NoNewWindow -WorkingDirectory $dirPath)
if ($process.ExitCode -ne 0)
{
  throw ("signtool.exe exited with exit code {0}" -f $process.ExitCode)
}

# Reset the current folder to the original
Set-Location $originalDir
[System.Environment]::CurrentDirectory = $originalDir

write-host "Successfull"
$global:LASTEXITCODE = 0