$vstest = "${env:ProgramFiles}\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe"
Write-Output "CITs found:"
dir ".\Debug\UnitTests\*CIT*.dll" -Recurse | foreach { & Write-Output $_ }
dir ".\Debug\UnitTests\*CIT*.dll" -Recurse | foreach { & $vstest /logger:trx /Platform:x64 /InIsolation /Enablecodecoverage /ResultsDirectory:".\Debug\TestResults" /Settings:"..\CodeCoverage.runsettings" $_ }

Write-Output "Specs found:"
dir ".\Debug\UnitTests\*Spec*.dll" -Recurse | foreach { & Write-Output $_ }
dir ".\Debug\UnitTests\*Spec*.dll" -Recurse | foreach { & $vstest /logger:trx /Platform:x64 /InIsolation /Enablecodecoverage /ResultsDirectory:".\Debug\TestResults" /Settings:"..\CodeCoverage.runsettings" $_ }
