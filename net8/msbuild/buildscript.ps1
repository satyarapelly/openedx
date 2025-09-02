# Note that this script assumes that the the following have already run:
# msbuild/msbuildset.cmd
# msbuild/nugetrestore.cmd
# msbuild/setupvstools.cmd 
# If you don't run these to scripts before running this one it won't work.

# Run msbuild from private
$buildRootPath = [System.Environment]::ExpandEnvironmentVariables("%INETROOT%") + '\private'
Set-Location $buildRootPath
& "msbuild"