Param 
(
 [string]$enlistmentPath = ".\",
 [string]$VsVersion = "14",
 [string]$CITsearchPath = ".\Drop\Debug\IntegrationTests"
)

$env:INETROOT=$enlistmentPath
$env:Environment="OneBox"

function Parse-CIT
{
    param( [string]$logfile, [string]$searchPath )
	
    $dict = @{"TestsRun"=0; "Failures"=0; "NotRun"=0}
	$trxFiles = Get-ChildItem -File -Name -Recurse -Include "*CIT*.dll.trx" -Path $searchPath

	foreach ($file in $trxFiles)
	{
		$trxfile = "$searchPath\$file"
		if($trxfile -Like "*\logs\*")
		{
			$startInfo = New-Object System.Diagnostics.ProcessStartInfo
			$startInfo.Arguments = "$trxfile $enlistmentPath\msbuild\MsbuildExtensions\msxsl\Summary.xslt -u 3.0"
			$startInfo.FileName = "$enlistmentPath\msbuild\MsbuildExtensions\msxsl\msxsl.exe"
			$startInfo.UseShellExecute = $false
			$startInfo.RedirectStandardOutput = $true
			$startInfo.CreateNoWindow = $true
			$output = ""
			$process = [System.Diagnostics.Process]::Start($startInfo)

			if ( !$process )
			{
				Tee-Object -FilePath $CITlog -Append -InputObject "Error! No process could be created from $enlistmentPath\msbuild\MsbuildExtensions\msxsl\msxsl.exe"
				continue
			}

			$output = $process.StandardOutput.ReadToEnd()

			if ( !$process.HasExited ) 
			{
				$process.WaitForExit()
			}

			Tee-Object -FilePath $logfile -Append -InputObject "Analyzing test results from $trxfile -> $output"

			foreach($str in $output.split(" "))
			{
			  $s = $str.split(":")
			  switch ($s[0]) 
			  {
				"TestsRun" {$dict."TestsRun" += $s[1]}
				"Failures" {$dict."Failures" += $s[1]}
				"NotRun" {$dict."NotRun" += $s[1]}

				default {"Unexpected content $s from $file"}
			  }
			}
		}
	}

	$TestsRun=$dict."TestsRun"; $Failures=$dict."Failures"; $NotRun=$dict."NotRun"
	Tee-Object -FilePath $logfile -Append -InputObject "Test Execution Summary:"
	Tee-Object -FilePath $logfile -Append -InputObject "    Total Tests run = $TestsRun"
	Tee-Object -FilePath $logfile -Append -InputObject "    Failures = $Failures"
	Tee-Object -FilePath $logfile -Append -InputObject "    Not run = $NotRun"
}

$CITlog = "$CITsearchPath\CIT_" + (get-date -f yyyy-MM-dd_HH-mm-ss) + ".log"
Out-File $CITlog

$VSCOMNTOOLS = "env:VS$VsVersion" + "0COMNTOOLS"
$mstestLocation = (type $VSCOMNTOOLS | Split-Path) + "\IDE\MSTest.exe"

New-Item -ItemType Directory -Force -Path $CITsearchPath\logs | Out-Null
Remove-Item $CITsearchPath\logs\* -Force -recurse | Out-Null
$exitCode = 0

foreach ($teamName in Get-ChildItem -Directory -Name -Exclude "logs" -Path $CITsearchPath)
{
	New-Item -ItemType Directory -Force -Path $CITsearchPath\logs\$teamName | Out-Null
	Tee-Object -FilePath $CITlog -Append -InputObject "Scanning TEAM folder: '$teamName' for CITs."
	foreach ($CITDirectory in Get-ChildItem -Directory -Name -Path "$CITsearchPath\$teamName")
	{
		$testDLL = "$CITsearchPath\$teamName\$CITDirectory\$CITDirectory.dll"
		if(![System.IO.File]::Exists($testDLL))
		{
			Tee-Object -FilePath $CITlog -Append -InputObject "Error! Couldn't locate the CIT within $teamName\$CITDirectory"
			continue
		}

		Tee-Object -FilePath $CITlog -Append -InputObject "Running tests from $testDLL"
		
		$startInfo = New-Object System.Diagnostics.ProcessStartInfo
		$outfile = (Split-Path $testDLL -leaf) + ".trx"
		$startInfo.Arguments = "/testcontainer:$testDLL /resultsfile:$CITsearchPath\logs\$teamName\$outfile"
		$startInfo.FileName = $mstestLocation
		$startInfo.UseShellExecute = $false
		$startInfo.RedirectStandardOutput = $true
		$startInfo.CreateNoWindow = $true
		$output = ""
		$process = [System.Diagnostics.Process]::Start($startInfo)

		if ( !$process )
		{
			Tee-Object -FilePath $CITlog -Append -InputObject "Error! No process could be created from $testDLL"
			continue
		}

		$output = $process.StandardOutput.ReadToEnd()

		if ( !$process.HasExited ) 
		{
			$process.WaitForExit()
		}

		Tee-Object -FilePath CIT.log -Append -InputObject $output
		if ($process.ExitCode -ne 0)
		{
			$exitCode = $process.ExitCode
			$errorlog = "mstest.exe exited with non-zero exit code {0} for $testDLL" -f $process.ExitCode
			Tee-Object -FilePath CIT.log -Append -InputObject $errorlog
		}
	}
}

Parse-CIT -logfile $CITlog -searchPath $CITsearchPath\logs
Exit $exitCode