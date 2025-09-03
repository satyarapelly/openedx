<#
.SYNOPSIS
This file contains a set of variables that will be used to control/drive the CDPX build operations.  The goal is to simplify the 
onboarding process for teams moving to CDPX as well as enabling an easier migration path from CDPX to ADO governed pipelines in the future.
Power shell scripts supplied by the Nuget package will consume these variables and execute logic required for the following CDPX Stages
Restore
Build
test
package

.Net core and .Net Framework projects require different command invocations and hence there are separate variables for these classes of 
projects.
Variables and associated types will be defined at the beginning to help teams understand the usage of the variables in the scripts
Several variables are powershell hash tables.  Usage can be found at https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_hash_tables?view=powershell-7
#>


# The Configuration to build in (generally Release)

[string] $buildConfiguration = "Release"

# The project referencing the Commerce.Shared.AppService.CDPxBuild nuget

[string]$libraryContainingProject = "private/Payments/CDPx/CDPx.csproj"

# =============================
# Projects to build and are not published to a build artifact. 
# The projects in this list will be output to the folder  build_output_nonsigned.  This is NOT a build artifact and not available
# for Release access
# =============================
# Hashtable of paths to solution or project files to build (relative to source root, linux syntax) coupled with a drop folder name. 
# example @{ "xyz.sln" = "foo"; }
# example @{ "xyz.csproj" = "foo"; "abc.csproj" = "bar"; "def.csproj" = "bar"}

# $projectsToBuild = @{"private/Payments/Payments.sln" = "Payments";}

# =============================
# .Net Core Web apps to build and publish
# The projects in this list will be published to the CDPx build artifact build_output
# =============================
# Hashtable of paths to .Net Core Web apps/Web jobs projects to build/publish(relative to source root, linux syntax), 
# coupled with the name of the zip packages to be generated. The zip packages  will be dropped into the ev2/bin output directory.
# (must be lower case).
# example @{ "xyz.csproj" = "foo"; "abc.csproj" = "bar"; "def.csproj" = "bar"}

# $netCoreProjectsToPublish = @{}


# =============================
# .Net Core Web jobs Projects to build and publish
# The projects in this list will be published to the CDPx build artifact build_output
# =============================
# Hashtable of paths to .Net Core Web jobs projects to build/publish(relative to source root, linux syntax, 
# coupled with the name of the zip packages and the web job type.
# Web jobs must be placed in specific folder paths relative to the web app root include the type of web job either continuous or triggered.  
# The Web Job type must be included for each web job. We will be using a naming convention for the 2nd variable to allow correct placement.
# Format:   WebJobName:webjobType
# Example: @{"WebJobTriggered.csproj" = "WebJobTriggered:triggered"; "WebJobContinous.csproj" = "WebJobContinous:continuous}

# $netCoreWebJobProjectsToPublish =@{}



# =============================
# .Net Framewok Web apps  Projects to build and publish
# The projects in this list will be published to the CDPx build artifact build_output
# =============================
# Hashtable of paths to .Net Framewok Web apps projects to build/publish(relative to source root, linux syntax), 
# coupled with the name of the zip packages to be generated. The zip packages  will be dropped into the ev2/bin output directory.
# (must be lower case).
# example @{ "xyz.csproj" = "foo"; "abc.csproj" = "bar"; "def.csproj" = "bar"}

$netFrameworkProjectsToPublish = @{"private/Payments/PXService/PXService.csproj" = "PXService";
                                   "private/Payments/Tests/Emulators/PXDependencyEmulators/PXDependencyEmulators.csproj" = "PXDependencyEmulators";}

# =============================
# .Net Framewok Web jobs Projects to build and publish
# The projects in this list will be published to the CDPx build artifact build_output
# =============================
# Hashtable of paths to .Net Framework Web jobs projects to build/publish(relative to source root, linux syntax, 
# coupled with the name of the zip packages and the web job type.
# Wweb jobs must be placed in specific folder paths relative to the web app root include the type of web job either continuous or triggered.  
# The Web Job type must be included for each web job. We will be using a naming convention for the 2nd variable to allow correct placement.
# Format:   WebJobName:webjobType
# Example: @{"WebJobTriggered.csproj" = "WebJobTriggered:triggered"; "WebJobContinous.csproj" = "WebJobContinous:continuous}

# $netFrameworkWebJobProjectsToPublish =@{}

# =====
# .Net Core Test Projects
# =====
# Paths to .Net Core test projects to run (relative to source root, linux syntax).   Do not include .Net Framework projects as they will not execute
# Example: $netCoreTestProjects = @("AppServiceQuickStarts/AppServiceNetCoreHttps.UnitTests/AppServiceNetCoreHttps.UnitTests.csproj")

# [string[]] $netCoreTestProjects = @("")


# =====
# Test Projects to publish to artifacts folder for external ADO test flow as well as Framework test execution.
# In the CDPx build, the artifact name where these projects are published is test_output_external.  CDPx is enforcing a requirement
# that any files in the build artifacts has to be signed, regardless of whether the files are going to be deployed. 
# There are cases where just the unittest dll will not run because of missing dependencies.  If you run into this case, you can 
# Add the test projects here and then specify the folder to include the artifact folder.
# =====
# Hashtable of paths to test projects that you want published to an artifact folder.  Test project path (relative to source root, linux syntax), 
# coupled with the name of a drop folder name for the project. The artifact folder will be test_output_external.
# example @{ "xyz.csproj" = "foo"; "abc.csproj" = "bar"; "def.csproj" = "bar"}
# If you run 
$testProjectsToPublishForExternalAdo = @{"private/Payments/Tests/COT.PXService/COT.PXService.csproj" = "COT.PXService"
                                         "private/Payments/Tests/CIT.PidlFactory/CIT.PidlFactory.csproj" = "CIT.PidlFactory"; 
                                         "private/Payments/Tests/CIT.PidlTest/CIT.PidlTest.csproj" = "CIT.PidlTest";
                                         "private/Payments/Tests/CIT.PXService/CIT.PXService.csproj" = "CIT.PXService";
										 "private/Payments/Tests/Spec.PXService/Spec.PXService.csproj" = "Spec.PXService";
                                        }
                                            

# =====
# .Net Framework Test Assemblies
# =====
# Hashtable of Test assembly File specifiers and  associated directory path filters. 
# These will be used to generate a list of framework test assemblies to excecute tests.  
# To simplify the discover of test assemblies, framework tests may be listed in the $testProjectsToPublishForExternalAdo variable.
# This will ensure that binaries required are dropped to the test_output_external build artifact.  The drawback to this approach is
# that CDPx is enforcing a requirement that any files listed in a build artifact MUST be signed.  Indivdual teams can decide the best
# way to access/find their framework unit test assemblies.
# If you are including test framework files in this build artificat, your directory filter should normally be test_output_external
# These two elements will be used in the following command in the provided powershell scripts.
# Get-ChildItem -Path . -Include <FileSpecifier> -File -Recurse -ErrorAction SilentlyContinue |Where-Object { $_.DirectoryName.Contains(<DirectoryPathFilter)} |ForEach-Object { $fullFileNames += "$_" }
# Example: All desired test files match a naming convention of  *.FrameworkUnitTests.dll  and we only want the dlls that are include in the test_output_external build artifact folder
# @{"*.FrameworkUnitTests.dll" = "test_output_external";}
# Example: Multiple definitions
# @{"*.FrameworkUnitTests.dll" = "test_output_external";"*.WorkerJobUnitTests.dll" = "test_output_external";}

$netFrameworkTestAssemblies =@{"CIT.PidlTest.dll" = "test_output_external"; "CIT.PidlFactory.dll" = "test_output_external"; "CIT.PXService.dll" = "test_output_external"; "Spec.PXService.dll" = "test_output_external"}

# If you'd like to call out to another ADO Build for testing from CDPx (see CDPx official documentation: https://onebranch.visualstudio.com/Pipeline/_wiki/wikis/Pipeline.wiki/320/External-Test-Workfow),
# place a file in the same location as variables.ps1 (this file) with the vstsbuildcommand yaml (https://onebranch.visualstudio.com/Pipeline/_wiki/wikis/Pipeline.wiki/308/Pipeline-Specification-YAML?anchor=yaml-format)
# and reference it below.  If these values are blank (or the file is blank), nothing is injected.
#
# The selected yaml file is injected during the Windows Test phase (after binaries are built but containers are not)
# See examples of how to specify the test file selection below
#
# This file could have the following content to run definition 12345
#######################################################################################################################
# - !!vstsbuildcommand        # REQUIRED: This maps the command data to a concrete type in the CDPX orchestrator.
#   name: 'My Custom Tests'   # REQUIRED: All commands have a name field. All console output captured when 
#                             #           this command runs is tagged with the value of this field.
#   definition_id: 12345      # REQUIRED: The ID of the VSTS definition to launch.
#                             # Can be found in the URL of the definition.
#   wait_mode: 'WaitNow'      # REQUIRED: Determines behavior with respect to waiting for the client build.
#                             # 'WaitNow': Block execution until the child build completes.
#                             # 'NoWait': Launch the child build, then ignore it and move on.
#   wait_minutes: 20          # OPTIONAL: Maximum time to wait for the child build, if wait_mode is set to 'WaitNow'.
#                             # If this limit is hit, cancel the child build and consider it failed.
#   continue_on_error: false  # REQUIRED: Determines whether to ignore failures in the child build, if wait_mode is set to 'WaitNow'.
#                             # true: ignore child build results.
#                             # false: write an error and treat this step as failed if the child build does not complete successfully.
#   source_branch: master     #OPTIONAL: Sets the branch to checkout in the child build. If this is not set, it defaults to the same branch as the parent build.
#######################################################################################################################
#
# To learn more about how to set this up, see the CDPx documentation links above.

# =====
# External Test definitions mapped to CDPx pipeline type and tag
# =====
# Hashtable of CDPx pipeline build type and CDPx build tag with a corresponding  yaml file.
# We will be utilizing the CDPX environment variables CDP_BUILD_TYPE and CDP_BUILD_TAG as the specifier for the build pipeline.
# CDPx build types must be one of the following:  PullRequest, Buddy, or Official
# The CDPx tag is the tag you created for your pipeline.
# For Local validation:  You will want to set the following environment variables:  CDP_BUILD_TYPE  and or CDP_BUILD_TAG
# Examples:
# Simple case.  You want to use one yaml files for all build types and you do not have any tags.
#               $adoTestsYamlWindowsFiles = @{"Buddy" = ".build/customtests.yaml"; "PullRequest" = ".build/customtests.yaml"; "Official" = ".build/customtests.yaml"; }

# Flexible case. You have 2 buddy builds.  One with no tag and one with the tag ExtraTests.  You want different tests run for the PullRequest and Official Build 
#               $adoTestsYamlWindowsFiles =
#                  @{"Buddy"           = ".build/buddytests.yaml"; 
#                    "BuddyExtraTests" = ".build/buddyExtratests.yaml" 
#                    "PullRequest"     = ".build/prtests.yaml"; 
#                    "Official"        = ".build/officialtests.yaml"; }

# $adoTestsYamlFiles = @{}

# =============
# Nuget Restore
# =============
# The follow options are provided to support projects using packages.config nuget management format (typically .NET Framework projects)
# https://docs.microsoft.com/en-us/nuget/reference/packages-config

# Nuget restore for these projects requires a package directory (default is "packages" directory at project repo root).
# Also, nuget restore command for these projects does not support recursively restoring referenced projects. If you projects use
# packages.config, referenced projects should be listed in $additionalProjectsToRestore so that the build script will individually 
# restore each additional project.

# If your projects use PackageReference format (default for .NET Core projects), you will not need to include additional projects
# to restore since nuget restore will recursively restore all referenced projects.
# Recommend to migrate your projects from packages.config to PackageReference format if possible.
# https://docs.microsoft.com/en-us/nuget/consume-packages/migrate-packages-config-to-package-reference

[string[]] $additionalProjectsToRestore = @("private/shared/Library/Tracing/SllLogging/SllLoggingSchema.csproj";
                                            "private/Payments/Pidl/LocalizationRepository/LocalizationRepository.csproj";
                                            "private/Payments/External/libphonenumber-22765/libphonenumber/libphonenumber.csproj";
                                            "private/Payments/External/QRCoder/QRCoder.csproj";
                                            "private/Payments/PXService.ApiSurface/PXService.ApiSurface.csproj";
                                            "private/Payments/Tests/PidlTest.JsonDiff/PidlTest.JsonDiff.csproj";
                                            "private/Payments/Tools/PemToJWK/PemToJWK.csproj";
                                            "private/Payments/CDPx/CDPx.csproj";
                                            "private/Payments/Tests/PidlTest/PidlTest.csproj";
                                            "private/Payments/Common/Common.csproj";
                                            "private/Payments/Pidl/PXCommon/PXCommon.csproj";
                                            "private/Payments/Tests/CIT.Payments.Common/CIT.Payments.Common.csproj";
                                            "private/Payments/Tools/InstallCertificates/InstallCertificates.csproj";
                                            "private/Payments/Pidl/PimsModel/PimsModel.csproj";
                                            "private/Payments/Tests/CIT.PimsModel/CIT.PimsModel.csproj";
                                            "private/Payments/Pidl/PidlModel/PidlModel.csproj";
                                            "private/Payments/Pidl/PidlFactory/PidlFactory.csproj";
                                            "private/Payments/Tests/Test.Common/Test.Common.csproj";
                                            "private/Payments/Tests/CIT.PidlTest/CIT.PidlTest.csproj";
                                            "private/Payments/Tests/CIT.PXService.ApiSurface/CIT.PXService.ApiSurface.csproj";
                                            "private/Payments/PXService/PXService.csproj";
                                            "private/Payments/Tests/CIT.PidlFactory/CIT.PidlFactory.csproj";
                                            "private/Payments/Tests/CIT.Localization/CIT.Localization.csproj";
                                            "private/Payments/Tests/CIT.PXService/CIT.PXService.csproj";
                                            "private/Payments/Tests/Spec.PXService/Spec.PXService.csproj";
                                            "private/Payments/Tests/COT.PXService/COT.PXService.csproj";
                                            "private/Payments/Tests/Emulators/PXDependencyEmulators/PXDependencyEmulators.csproj";
                                             )

 # Path for package directory (relative to source root, linux syntax)
[string] $packagesDirectoryForPackagesConfigProjects = "NugetPackages"

# ==============
# EV2 Deployment
# ==============

# The folder which contains all of your ev2 files. 
# If you used the Commerce.Shared.AppServicesAppPlatform NuGet you do not need to update this path unless you move the folder.
# If you provided your own ev2 files update this path to point to your ev2 files.  
[string] $ev2RootPath = "private\Payments\Deployment.PME"

# If specified, write the build version of the current CDPx build as EV2 version file in the EV2 deployment artificats.
# File name should be BuildVer.txt if you used the Commerce.Shared.AppServicesAppPlatform NuGet.
# Set to $null if you would like build script to skip writing this file.
[string] $ev2BuildVersionFileName = "BuildVer.txt"

# ===========
# SDL Options
# ===========
# Files to skip from SDL policy scans. (Just the file name, no path needed)
# Only required if your build fails for the specific policy and you can't remove the files that fail
# https://onebranch.visualstudio.com/Pipeline/_wiki/wikis/Pipeline.wiki/315/SDL-Options
#Example: $binskimSkipFiles = @("MyFirst.dll", "MySecond.dll")

[string[]] $binskimSkipFiles = @("ApSecretStoreInvoke.dll", "Bond.Attributes.dll", "Bond.IO.dll", "Bond.Reflection.dll", "Microsoft.Web.Infrastructure.dll", "System.Net.Http.Formatting.dll", "System.Web.Http.dll", "System.Web.Http.SelfHost.dll", "System.Web.Http.WebHost.dll", "KernelTraceControl.dll", "msdia140.dll", "ApBootstrap.exe", "Microsoft.Practices.EnterpriseLibrary.Validation.dll", "Microsoft.Practices.ServiceLocation.dll", "Microsoft.Practices.Unity.Configuration.dll", "Microsoft.Practices.Unity.dll", "Microsoft.Practices.Unity.RegistrationByConvention.dll", "DirectApLauncher.exe", "MakeMachineHBI.exe", "Microsoft.Search.Autopilot.dll", "signtool.exe", "vc12redist_x64.exe", "vcredist_x64.exe", "vc_redist.x64.exe", "makecat.exe", "tracelog.exe", "msxsl.exe", "gbc.exe", "Bond.dll", "Bond.JSON.dll", "DocumentDB.Spatial.Sql.dll", "Microsoft.Azure.Documents.ServiceInterop.dll", "msdia120.dll", "KernelTraceControl.Win61.dll", "Microsoft.IdentityModel.dll", "MonitoringWebClientWrapper.dll", "mssp7en.dll", "Microsoft.Data.Edm.dll", "lz4X64.dll", "Microsoft.Data.OData.dll", "Xpert.Agent.DataLauncher.exe", "ZLib1VC.x64.dll", "KernelTraceControl.dll", "xsd.exe", "Microsoft.Data.Services.Client.dll", "Microsoft.Data.Services.dll", "System.Spatial.dll", "Microsoft.WindowsAzure.Storage.dll", "Microsoft.WindowsAzure.StorageClient.dll", "Xpert.Agent.exe", "DirSigner.exe", "StyleCopSettingsEditor.exe", "nuget.exe", "Microsoft.Web.Services3.dll")


# [string[]] $fxcopSkipFiles = @("")

