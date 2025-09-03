rem Clones the code with the latest release tag. Sets up nuget in the path env variable and calls a ps script to restore packages.

pushd "%~dp0"

echo Ensuring that development environment tools provided by VS2022 are available in the context of the command shell. For instance, access to msbuild
call "%ProgramFiles%\Microsoft Visual Studio\2022\Enterprise\Common7\Tools\VsDevCmd.bat" -arch=amd64 -host_arch=amd64

REM Clone repo's master branch
call powershell "%RepoRoot%\clone-release-tag.ps1"
set EX=%ERRORLEVEL%
if "%EX%" neq "0" (
    echo "Failed to restore projects in parallel"
)

REM add nuget's path to the path env variable
set RepoRoot=%CD%
set NUGET="%RepoRoot%\msbuild\nuget\nuget.exe"
echo %path%
set path=%RepoRoot%\msbuild\nuget\nuget.exe;%path%
echo %path%

REM Do nuget restore in parallel on both branches
call powershell "%RepoRoot%\restore.ps1" -path "%RepoRoot%"

set EX=%ERRORLEVEL%
if "%EX%" neq "0" (
    echo "Failed to restore projects in parallel"
)

popd

exit /B %EX%
