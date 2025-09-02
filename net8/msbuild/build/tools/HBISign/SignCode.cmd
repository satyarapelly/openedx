@echo off

rem ---------------------------------------------------
rem
rem Automation process to sign build files
rem
set BuildTracker.BuildShareGroup.DropShare = %1
echo %1
set buildNumber= %2
echo %2
set INETROOT= %3
echo %3
set pfxPassword=%4
echo %4
rem set INETROOT=%INETROOT:~0,-1%
set EnvironmentConfig=%~dp0Environment.props
set path=%path%;%INETROOT%\tools\path1st

if "%inetroot%"=="" (
	echo Error: set INETROOT to the root of your enlistment.
	exit /b 1
)

setlocal ENABLEDELAYEDEXPANSION ENABLEEXTENSIONS

set date=%date:~4%
set time=%time:~0,-3%
@echo.
@echo Signing Starting: %date% %time%
@echo.
set signError=0
set srvReleaseShare=%BuildTracker.BuildShareGroup.DropShare%
echo srvReleaseShare

rem @echo Drop = %srvReleaseShare%\%buildNumber%

cd %INETROOT%\Drop\Debug

findstr /sim "runonhbi *= *true" ServiceConfig.ini > signFolderswithfileName.txt

rem del x.txt
for /F %%i in (signFolderswithfileName.txt) do call :trimit %%i
goto :donetrim

:trimit
set name=%1
set trim=%name:\Serviceconfig.ini=%
for /f %%i in ("%1") do echo %trim% >> signFolders.txt
rem del signFolderswithfileName.txt
goto :eof
rem perl -pi.original -e  "s/\\ServiceConfig.ini//ig;" signFolders.txt

rem del signFolderswithfileName.txt
:donetrim

for /f %%a in (signFolders.txt) do (
	rem %INETROOT%\msbuild\build\tools\HBISign\dirsigner.exe -d %%a -n CommerceHBICodeSign
	powershell %INETROOT%\msbuild\build\tools\HBISign\DirSigner.ps1 -pfxFileName %INETROOT%\msbuild\build\Signing\CommerceHBICodeSign.pfx -pfxPassword %pfxPassword% -dirPath %INETROOT%\Drop\Debug\%%a
	if not %errorlevel% == 0 (
		set /A signError+=1
	)
)

rem delete signFolders
del signFolders.txt

if not %signError% == 0 (
	@echo.
	@echo [ERROR] Signing process has detected %signError% errors . Please investigate.
	@echo [ERROR] Now Exiting.
	echo.
	exit /b 1
) else (
	echo.
        echo Signing Successfully Completed: %date% %time%
	echo.
	echo Copying signed bins to release drop: %INETROOT%\Drop\Release_signed
	echo.
	call robocopy %INETROOT%\Drop\Debug %INETROOT%\Drop\Release_signed /s /R:3 /NP /MT:38
	call robocopy %INETROOT%\Drop\Release_signed  %INETROOT%\Drop\Release_signed\Logs signFolders.txt /R:3 /MOV
)
echo.
echo Process Complete: %date% %time%
echo.

endlocal & exit /b 0

