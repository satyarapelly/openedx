pushd $psScriptRoot\..\..

nuget restore -ConfigFile .\nuget.config .\private\Payments\Payments.sln

popd