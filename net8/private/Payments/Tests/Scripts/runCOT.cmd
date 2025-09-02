@echo off
setlocal
set autopilot=%inetroot%\drop\release\payments\autopilot
if not "%BUILDTYPE%"=="retail" (
  set autopilot=%inetroot%\drop\debug\payments\autopilot
)
set local=%_NTDRIVE%\paymod
if not exist %local%\logs (
  mkdir %local%\logs
  if %errorlevel% geq 1 goto abort  
)
set timestamp=%date:~12,2%%date:~7,2%%date:~4,2%%time:~0,2%%time:~3,2%%time:~6,2%
set timestamp=%timestamp: =0%
set testfx=%autopilot%\systemtestrunnerservice\mstest.exe
set environment=onebox

echo Results prefix: %local%\logs\%timestamp%_

set exitcode=0
for /f %%i in ('dir /b %autopilot%\systemtestrunnerservice\continuoustests\cot.*%1*.dll') do (
  IF NOT "%%i" == "COT.InstrumentManagementService.dll" IF NOT "%%i" == "COT.Providers.dll" (
    echo - Running tests in: %%i
	%testfx% /nologo /resultsfile:%local%\logs\%timestamp%_%%i.trx /testsettings:%autopilot%\systemtestrunnerservice\STR.TestSettings /testcontainer:%autopilot%\systemtestrunnerservice\continuoustests\%%i > %local%\logs\%timestamp%_%%i.log
    for /f %%j in ('findstr /c:"test(s) Passed," %local%\logs\%timestamp%_%%i.log') do (
      echo ERROR: Some tests did not pass.
	  echo %local%\logs\%timestamp%_%%i.log
	  echo %local%\logs\%timestamp%_%%i.trx
    )
  )
)    

set errorlevel=%exitcode%
goto :EOF
:abort
echo - Aborting post build with error code: %errorlevel%

