REM DO NOT MODIFY THIS FILE
REM CDPx cannot call directly into powershell, so .cmd files are used on Windows and .sh files on Linux to initiate powershell scripts
setlocal enabledelayedexpansion
powershell.exe -NoProfile -ExecutionPolicy Unrestricted -File "%~dp0bootstrap.ps1"
endlocal
exit /B %ERRORLEVEL%