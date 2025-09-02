@echo off
setlocal EnableDelayedExpansion

REM --------------------------------------------------------------------
REM -- FIND INETROOT
REM --------------------------------------------------------------------
pushd
:findInetroot
if exist build.root (
  set inetroot=%cd%
) else (
  cd..
  goto :findInetroot
)
popd

ECHO %inetroot%

set drop=%inetroot%\drop\debug
set local=%drop%

if not exist %local%\logs (
  mkdir %local%\logs
  if errorlevel 1 goto abort
)

set Environment=onebox
set AppRoot=%local%
set formatted_date=%date:/=-%
set timestamp=%formatted_date%_%time:~0,2%%time:~3,2%%time:~6,2%
set timestamp=%timestamp: =0%
set inetsrv=%systemroot%\system32\inetsrv
set logfile=%local%\logs\setupLocalPXService_%timestamp%.log

set services=PXService PXDependencyEmulators
set certificates=pxpxclientauth ctpcommerceservicepxclientauth pxtestpxclientauth aadpxclientauth

if /i "%1"=="/uninstall" goto uninstall

REM --------------------------------------------------------------------
REM -- INSTALL
REM --------------------------------------------------------------------
echo Build: %drop%
echo Log:   %logfile%

echo - Setting up AppPool and identity
%inetsrv%\appcmd add apppool /name:PXAppPool /managedRuntimeVersion:"v4.0" /managedPipelineMode:"Integrated" >> %logfile%
%inetsrv%\appcmd set config /section:applicationPools /"[name='PXAppPool']".processModel.identityType:NetworkService >> %logfile%
if errorlevel 1 goto abort

echo - Setting up Web Applications
for %%i in (%services%) do (
  call :setupIIS %%i
  if errorlevel 1 goto abort
)

echo - Resetting IIS
iisreset >> %logfile%
goto :EOF

REM --------------------------------------------------------------------
REM -- UNINSTALL
REM --------------------------------------------------------------------
:uninstall
echo Build: %local%
echo Log:   %logfile%

echo - Resetting IIS
iisreset  >> %logfile%

for %%i in (%services%) do (
  echo   - Deleting %%i
  %inetsrv%\appcmd delete app /app.name:"Default Web Site/%%i" >>  %logfile%
  if errorlevel 1 call :failed
)

if "%abort%"=="1" goto abort
goto :EOF

REM --------------------------------------------------------------------
REM -- ABORT THE SCRIPT
REM --------------------------------------------------------------------
:abort
echo - Aborting setup with error code: %errorlevel%
exit /b %errorlevel%

REM --------------------------------------------------------------------
REM -- FAILED TO UNINSTALL
REM --------------------------------------------------------------------
:failed
echo Failed with error code: %errorlevel% >>  %logfile%
echo Failed with error code: %errorlevel%
set abort=1
goto :eof

REM --------------------------------------------------------------------
REM -- SETUP IIS WEB SITE
REM --------------------------------------------------------------------
:setupIIS
echo   - %1
set sitename=%1

if exist %local%\%1\web.onebox.config (
  ren %local%\%1\web.config web.config.original >> %logfile%
  copy %local%\%1\web.onebox.config %local%\%1\web.config >> %logfile%
)

%inetsrv%\appcmd add app /site.name:"Default Web Site" /path:"/%sitename%" /physicalPath:"%local%\%1" >> %logfile%
if errorlevel 1 (
  if %errorlevel% neq 183 goto :eof
)

%inetsrv%\appcmd set app /app.name:"Default Web Site/%sitename%" /applicationPool:"PXAppPool" >> %logfile%
if errorlevel 1 goto :eof
goto :eof