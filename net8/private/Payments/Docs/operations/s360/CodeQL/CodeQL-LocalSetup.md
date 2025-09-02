# How to run Code QL locally

## Why CodeQL

Microsoft's SDL policy requires that all released product and service source code be analyzed by static analysis tools to detect security defects in source and that all security bugs are resolved prior to release.  
[More info on SDL policy](https://osgwiki.com/wiki/Task_-_Services_Security:_Run_Source_Code_Security_Analysis_Tools)

| Programming Language | Approved SDL Tool |
|----------------------|-------------------|
| C#                   | CodeQL            |

---

## How it works

CodeQL will execute your exact build commands with analysis enabled — a snapshot will be built for compiled languages (C# and C++), and then queries will be run against the snapshot to check for security issues.

## Prerequisites

- **Install dotnet:**  
  [https://dotnet.microsoft.com/en-us/download/dotnet](https://dotnet.microsoft.com/en-us/download/dotnet)

- **Install Guardian tool and set environment variable path:**  
  Visit [Installers | Guardian On EngHub](https://eng.ms/docs/cloud-ai-platform/devdiv/one-engineering-system-1es/1es-mohanb/security-integration/guardian-wiki/microsoft-guardian/installation)

---

## How to run in local machine (CodeQL is not supported in CDPx yet)

1. **Create a "Guardian" folder** in your drive where you have your repos
2. **And add the downloaded tool to the Guardian folder**
3. **Run the Guardian installer**

   Double click on `guardian-installer-win-x64` and install Guardian. The rest of the files in this folder will appear post installation.

4. **Update Guardian path in environment variables**

   Add the installed Guardian path (e.g. `D:\Guardian`) to your system environment variables.

5. **Run commands in PowerShell (admin mode) from your repo path**

    ```powershell
    cd D:\PXRepo\SC.CSPayments.PX

    # Run this only if gdn folder is already present in your repo
    1. guardian deinit --confirm

    2. guardian init
    3. guardian configure -t semmle

    ## Update the config in semmle file

    - The config file is located here:  
     `SC.CSPayments.PX\.gdn\e\semmle`

    - Config for reference:

    {
	    "fileVersion": "1.0.0",
	    "tools": [
            {
                "fileVersion": "1.0.0",
                "tool": {
                    "name": "semmle",
                    "version": "2.8.4.92"
                },
                "arguments": {
                    "SourceCodeDirectory": "$(WorkingDirectory)",
                    "Language": "csharp",
                    "TypeScript": false,
                    "IncludeNodeModules": false,
                    "BuildCommands": "\\\"%ProgramFiles%\\Microsoft Visual Studio\\2022\\Enterprise\\Common7\\Tools\\VsMSBuildCmd.bat\\\" && MSBuild.exe private\\Payments\\Payments.sln /t:Clean#\\\"%ProgramFiles%\\Microsoft Visual Studio\\2022\\Enterprise\\Common7\\Tools\\VsMSBuildCmd.bat\\\" && MSBuild.exe private\\Payments\\Payments.sln",
                    "Suite": "$(SuiteSDLRecommended)",
                    "ResultsDirectory": "$(GuardianRawResultsDirectory)\\Semmle\\$(Language)",
                    "ProjectName": "$(ProjectNameValue)",
                    "ProjectDirectory": "$(TEMP)\\Semmle\\Projects\\$(ProjectNameValue)",
                    "MakeOutputPathsUnique": true,
                    "Force": true,
                    "QLInstallationPath": "$(InstallDirectory)\\odasa",
                    "Timeout": 1800,
                    "RAM": 16384,
                    "AddAntivirusExclusion": "$(TEMP)",
                    "AllowAntivirusScanning": false
                },
                "outputExtension": "sarif",
                "successfulExitCodes": [
                    0
                ],
                "errorExitCodes": {
                    "1": "Semmle run failed."
                }
            }
	    ]
    }
    
    4. guardian run -c semmle
    ```            
    - View the results: SC.CSPayments.PX\\.gdn\\.r\Semmle\csharp
    
    - To view the logs, install Sarif Viewer extension in Visual Studio Code.
---

> ✅ **Note:** Ensure PowerShell is run as administrator and that the environment variable changes are applied before executing commands.
