rem cmd wrapper for executing the difftest powershell script. CDPx pipeline requires a cmd wrapper around ps files.

echo Ensuring that development environment tools provided by VS2022 are available in the context of the command shell. For instance, access to msbuild
call "%ProgramFiles%\Microsoft Visual Studio\2022\Enterprise\Common7\Tools\VsDevCmd.bat" -arch=amd64 -host_arch=amd64

pushd "%~dp0"

set cwd=%CD%
set RepoRoot="%cwd%/"

echo Executing difftest.
call powershell "%RepoRoot%DiffTests.ps1" -path "%RepoRoot%"

set EX=%ERRORLEVEL%
if "%EX%" neq "0" (
    echo "Failed to build project"
)

popd

exit /B %EX%
