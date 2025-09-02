@echo off

if not exist %SYSTEMROOT%\System32\msvcp120.dll goto installVC12RT
if not exist %SYSTEMROOT%\System32\msvcr120.dll goto installVC12RT
if not exist %SYSTEMROOT%\System32\vccorlib120.dll goto installVC12RT
goto gotVC12RT
:installVC12RT
echo Installing VC12 runtime
vc12redist_x64.exe /q /norestart
:gotVC12RT

echo Creating HBI environment
echo This machine was marked HBI by MakeMachineHBI service.>C:\HBIMACHINE.DAT

echo Bootstrapping machine
ApBootstrap.exe -x
set EXITCODE=%ERRORLEVEL%
if %EXITCODE% NEQ 0 goto END

:INSTALL_CERTS
if "x%1x" EQU "xx" goto END
echo Installing certificate %1 into ROOT store
%SystemRoot%\System32\certutil.exe -f -addstore root %1
set EXITCODE=%ERRORLEVEL%
if %EXITCODE% NEQ 0 goto END
shift
goto INSTALL_CERTS

:END
exit /b %EXITCODE%
