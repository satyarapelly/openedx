Param 
(
 [string]$stampSource = "", #"d:\code\pm2"
 [string]$rootPath = "",
 [switch]$restoreNugets = $false,
 [switch]$resetToStamp = $false,
 [switch]$build = $false,
 [switch]$updateStamp,
 [string]$subFolder = "", #"\Castle\TestCommon"
 [string]$VsVersion = "11",
 [switch]$runCIT = $false,
 [switch]$parseCIT = $false,
 [string]$searchPath = ""
)

if ([string]::IsNullOrWhiteSpace($stampSource) -and [string]::IsNullOrWhiteSpace($rootPath)) { $rootPath = split-path (split-path -parent (split-path -parent $MyInvocation.MyCommand.Definition)) }
$stampPath = "$rootPath\stamp"
$testPath = "$rootPath\test"
$createStamp = ![string]::IsNullOrWhiteSpace($stampSource) -and ![string]::IsNullOrWhiteSpace($rootPath)

if ($createStamp)
{
    if (!$updateStamp)
    {
        "create stamp"
        #copy private
        robocopy $stampSource\private $stampPath\private /mir /nfl /ndl
        # remove developer
        rd $stampPath\private\developer -Recurse -Force
        #remove stylecop.cache file
        gci StyleCop.Cache -Recurse -Force | % { del $_.FullName -Force }
    }
    else
    {
        "update stamp"
        $absoluteStampSource = [System.IO.Path]::GetFullPath($stampSource)
        $absoluteStampPath = [System.IO.Path]::GetFullPath($stampPath)
        $absoluteStampSourceLen = $absoluteStampSource.Length - 2
        $absoluteStampPathLen = $absoluteStampPath.Length

        "Get all the SD files ..."
        $sdFiles = @{}
        sd files -l -d $absoluteStampSource\private\...#have | % { 
			$x = $_.Split("#")[1].Split(":")[1]
			$x = $x.Substring($absoluteStampSourceLen, $x.Length - $absoluteStampSourceLen).Trim().ToLower()
			if ($x -notlike '\private\developer\*') { $sdFiles.Add($x, 1) }  
		}

        "Get all the stamp files ..."
        $stampFiles = @{}
        gci -Recurse $absoluteStampPath\private | ?{ !$_.PSIsContainer } | % { 
			$x = $_.FullName
			$x = $x.Substring($absoluteStampPathLen, $x.Length - $absoluteStampPathLen).ToLower()
			if (!$sdFiles.ContainsKey($x)) { $stampFiles.Add($x,1) } 
		}

        "Delete extra files ..."
        $stampFiles.Keys | % { 
			if (Test-Path ($absoluteStampPath + $_)) { 
				Write-Host del ($absoluteStampPath + $_)
				del ($absoluteStampPath + $_) -Force 
			} 
		}
		
        "Copy updated and new files ..."
        $sdFiles.Keys | % { 
            $s = (Get-Item -LiteralPath ($absoluteStampSource + $_)).LastWriteTimeUtc
            $t = $null
            if (Test-Path -LiteralPath ($absoluteStampPath + $_)) { $t = (Get-Item -LiteralPath ($absoluteStampPath + $_)).LastWriteTimeUtc } 
            if ($t -eq $null -or $s -gt $t) { 
                $path = [System.IO.Path]::GetDirectoryName($absoluteStampPath + $_) + '\'
                Write-Host xcopy ($absoluteStampSource + $_) $path /D /Y /Q /R /K; 
                xcopy ($absoluteStampSource + $_) $path /D /Y /Q /R /K
            }
        }
    }

    robocopy $stampSource\msbuild $stampPath\msbuild /mir /nfl /ndl

    # copy required additional tools
    if (Test-Path $stampPath\build) { rd $stampPath\build  -Recurse -Force }
    if (Test-Path $stampPath\public) { rd $stampPath\public  -Recurse -Force }
    if (Test-Path $stampPath\tools) { rd $stampPath\tools  -Recurse -Force }

    md $stampPath\build
    cp $stampSource\build\OCPKey.snk $stampPath\build -Force
    md $stampPath\msbuild\nuget
    cp $stampSource\msbuild\nuget\* $stampPath\msbuild\nuget -Force
    md $stampPath\public\ext\tools\x86
    cp $stampSource\public\ext\tools\x86\tracelog.exe $stampPath\public\ext\tools\x86 -Force
    md $stampPath\public\ext\SQLizer\3.0_20160714
    cp $stampSource\public\ext\SQLizer\3.0_20160714\* $stampPath\public\ext\SQLizer\3.0_20160714 -Force -Recurse
    md $stampPath\tools\path1st
    cp $stampSource\msbuild\robocopy.cmd $stampPath\tools\path1st -Force
    cp $stampSource\nuget.config $stampPath -Force
    md $stampPath\build\msbuild\v4.0
    cp $stampSource\build\msbuild\v4.0\Settings.StyleCop $stampPath\build\msbuild\v4.0 -Force
    "done"
}

if ($restoreNugets -or ($createStamp -and !$updateStamp))
{
    "restore nugets"
    gci $stampPath -Include packages.config -Recurse | % { iex ("$stampPath\msbuild\nuget\nuget.exe restore " + $_.FullName + " -ConfigFile $stampPath\nuget.config") }

    #copy in the castle special folders
    $latestCastleDropPath = (cat \\unistore\build\SC.csPayments.Castle.BuildDevelop\latest.txt).Trim()
	if (Test-Path $stampPath\Drop\Debug\Castle\KeyService) { rd $stampPath\Drop\Debug\Castle\KeyService  -Recurse -Force }
    robocopy $latestCastleDropPath\Castle.Drop\Castle\KeyService $stampPath\Drop\Debug\Castle\KeyService /S /NP /NS /NC /NFL /NDL /NJS /NJH
	if (Test-Path $stampPath\Drop\Debug\Castle\TokenService) { rd $stampPath\Drop\Debug\Castle\TokenService  -Recurse -Force }
    robocopy $latestCastleDropPath\Castle.Drop\Castle\TokenService $stampPath\Drop\Debug\Castle\TokenService /S /NP /NS /NC /NFL /NDL /NJS /NJH
	if (Test-Path $stampPath\Drop\Debug\Castle\Setup) { rd $stampPath\Drop\Debug\Castle\Setup  -Recurse -Force }
    robocopy $latestCastleDropPath\Castle.Drop\Castle\Setup $stampPath\Drop\Debug\Castle\Setup /S /NP /NS /NC /NFL /NDL /NJS /NJH
	if (Test-Path $stampPath\Drop\Debug\Castle\Deployment) { rd $stampPath\Drop\Debug\Castle\Deployment  -Recurse -Force }
    robocopy $latestCastleDropPath\Castle.Drop\Castle\Deployment $stampPath\Drop\Debug\Castle\Deployment /S /NP /NS /NC /NFL /NDL /NJS /NJH
    "Done"
}

if ($resetToStamp)
{
    "Setup test folder"
    robocopy "$stampPath" "$testPath" /mir /nfl /ndl /np /ns | Out-Null
    "Completion: $LASTEXITCODE"
    "Done"
}


if ($build)
{
    "start build"
	Write-Output ("cmd /c cd /d $testPath & msbuild\msbuildset.cmd & `"%VS$VsVersion" + "0COMNTOOLS%\VsDevCmd.bat`" & cd private\$subFolder & msbuild /fl /v:m /maxcpucount")
    cmd ("/c cd /d $testPath & msbuild\msbuildset.cmd & `"%VS$VsVersion" + "0COMNTOOLS%\VsDevCmd.bat`" & cd private\$subFolder & msbuild /fl /v:m /maxcpucount")
    "done"
}

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
			$process = New-Object System.Diagnostics.Process
			$process.StartInfo.FileName = "$testPath\msbuild\MsbuildExtensions\msxsl\msxsl.exe"
			$process.StartInfo.Arguments = "$trxfile $testPath\msbuild\MsbuildExtensions\msxsl\Summary.xslt -u 3.0"
			$process.StartInfo.UseShellExecute = $false
			$process.StartInfo.RedirectStandardOutput = $true
			if ( $process.Start() ) 
			{
				$output = $process.StandardOutput.ReadToEnd()
			}
			$process.WaitForExit()
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

if ($runCIT)
{
    $CITlog = "$testPath\private\CIT_" + (get-date -f yyyy-MM-dd_hh-mm-ss) + ".log"
    Out-File $CITlog

	$CITdrop = "$testPath\Drop\Debug\IntegrationTests"
	$VSCOMNTOOLS = "env:VS$VsVersion" + "0COMNTOOLS"
	$mstestLocation = (type $VSCOMNTOOLS | Split-Path) + "\IDE\MSTest.exe"

	New-Item -ItemType Directory -Force -Path $CITdrop\logs | Out-Null
	Remove-Item $CITdrop\logs\* -Force -recurse | Out-Null

	foreach ($teamName in Get-ChildItem -Directory -Name -Exclude "logs" -Path $CITdrop)
	{
	    New-Item -ItemType Directory -Force -Path $CITdrop\logs\$teamName | Out-Null
	    Tee-Object -FilePath $CITlog -Append -InputObject "Scanning TEAM folder: '$teamName' for CITs."
		foreach ($CITDirectory in Get-ChildItem -Directory -Name -Path "$CITdrop\$teamName")
		{
		    $testDLL = "$CITdrop\$teamName\$CITDirectory\$CITDirectory.dll"
		    if(![System.IO.File]::Exists($testDLL))
            {
                Tee-Object -FilePath $CITlog -Append -InputObject "Error! Couldn't locate the CIT within $teamName\$CITDirectory"
                continue
            }

			Tee-Object -FilePath $CITlog -Append -InputObject "Running tests from $testDLL"
			
			$process = New-Object System.Diagnostics.Process
			$process.StartInfo.FileName = $mstestLocation
			$outfile = (Split-Path $testDLL -leaf) + ".trx"
			$process.StartInfo.Arguments = "/testcontainer:$testDLL /resultsfile:$CITdrop\logs\$teamName\$outfile"
			$process.StartInfo.UseShellExecute = $false
			$process.StartInfo.RedirectStandardOutput = $true
			if ( $process.Start() ) 
			{
				$output = $process.StandardOutput.ReadToEnd()
			}
			$process.WaitForExit()
			Tee-Object -FilePath CIT.log -Append -InputObject $output
		}
	}
    
	Parse-CIT -logfile $CITlog -searchPath $CITdrop\logs
}

if ($parseCIT)
{
    $CITlog = "$testPath\private\CIT_" + (get-date -f yyyy-MM-dd_hh-mm-ss) + ".log"
    Out-File $CITlog
	
    Parse-CIT -logfile $CITlog -searchPath $searchPath
}
