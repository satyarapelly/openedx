rem cmd wrapper for building px service and release code base. CDPx pipeline requires a cmd wrapper around ps files.

echo Executiong build stage

echo Ensuring that development environment tools provided by VS2022 are available in the context of the command shell. For instance, access to msbuild
call "%ProgramFiles%\Microsoft Visual Studio\2022\Enterprise\Common7\Tools\VsDevCmd.bat" -arch=amd64 -host_arch=amd64

pushd "%~dp0"

set cwd=%CD%
set RepoRoot="%cwd%/"

REM build in parallel on both branches
call powershell "%RepoRoot%\build.ps1" -path "%RepoRoot%"

set EX=%ERRORLEVEL%
if "%EX%" neq "0" (
    echo "Failed to build projects in parallel"
)

popd

exit /B %EX%
