Couple supported scenarios:
MSBuild an enlistment
---------------------
1. Go to the enlistment root folder
2. Invoke msbuild\msbuildset.cmd
3. Setup visual studio developer console %VSXX0COMNTOOLS%\VsDevCmd.bat
4. Restore nuget packages if neccessary: msbuild\nugetrestore.cmd
Now you are ready to invoke msbuild from any folder

Test building private folder only in isolation
----------------------------------------------
A. Create a stamp
1. Goto the root of the enlistment
2. Build nuke
3. powershell -File msbuild\runtest.ps1 -stampSource enlistment_root -rootPath new_folder

B. Build clean
1. powershell -File new_folder\stamp\msbuild\runtest.ps1 -resetToStamp -build [-VsVersion XX]
   Where VsVersion is optional parameter for visual studio version (11 (default) - 2012, 12 - 2013, 14 - 2015)
2. You can see the build log in new_folder\test\private\msbuild.log

C. Compare enlistment to the test
1. Build the enlistment
2. powershell -File new_folder\stamp\msbuild\comparer.ps1 elistment_root\drop\debug new_folder\test\drop\debug
3. open diff.txt

D. Verify CIT Results
1. Invoke msbuild\msbuildset.cmd
2. powershell -File new_folder\stamp\msbuild\runtest.ps1 -runCIT
3. powershell -File new_folder\stamp\msbuild\runtest.ps1 -parseCIT -searchPath enlistment\private
4. compare results from 1 & 2
