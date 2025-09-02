@echo off
set INETROOT=%CD%
for /R %~dp0.. %%i in (packages.config) do if exist %%i %INETROOT%\msbuild\nuget\nuget.exe restore %%i -ConfigFile %INETROOT%\nuget.config -FallbackSource "https://microsoft.pkgs.visualstudio.com/_packaging/Universal.Store/nuget/v3/index.json" -NoCache
