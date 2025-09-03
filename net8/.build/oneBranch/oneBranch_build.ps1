pushd $psScriptRoot\..\..

$msbuild = "${env:ProgramFiles}\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
& $msbuild .\private\Payments\Payments.sln /p:configuration=Release /t:Clean
& $msbuild .\private\Payments\Payments.sln /p:configuration=Release

popd