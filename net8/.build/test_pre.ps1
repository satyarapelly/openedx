# Pre build file to allow service teams to either add functionality before the standard Nuget supplied logic or to completely override the default logic
# If you need to override the base Nuget functionality, please reach out to the CRE to to either file a bug or a feature request if you believe this is a common need


# Service teams can add logic to would be called before the standard Nuget build logic if required.
#If Service teams need to completely skip the Nuget logic, comment out the call to the base script

$NugetFilePath = join-path $baseDirectory ".buildCDPx\test.ps1"

if (Test-Path $NugetFilePath)
{
    . $NugetFilePath
}