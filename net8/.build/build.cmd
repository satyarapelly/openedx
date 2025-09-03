
ECHO Zipping workflow files

REM This is just an example of how you could get the build resources into the ServiceGroupRoot for the Ev2 deployment
SET workflowZipFolder="%~dp0..\ServiceGroupRoot\GenevaAutomationResources"
mkdir -p %workflowZipFolder%
powershell Compress-Archive -Force "%~dp0..\private\Payments\GenevaWorkflow\workflows", "%~dp0..\private\Payments\GenevaWorkflow\connections" "%workflowZipFolder%\workflows.zip"

REM Check return value of msbuild.exe from previous step.
if %errorlevel% neq 0 (
    ECHO Failed to build Geneva Automation package.

    REM Exit with non-zero error code so build will fail.
    exit /B 1
)

exit /B 0