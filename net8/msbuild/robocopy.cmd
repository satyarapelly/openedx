::
:: Robowrap.bat
::     script by michkim, added to corext by zackrun
::
:: Wraps call to robocopy utility and returns 1 if
:: there are errors from copying files. Returns 0 if
:: there are no copying errors.
::
:: This utility is used for making robocopy return
:: error codes that are compatible with the name utility.
::
@echo off
setlocal
set failed=0

@REM -- We need to watch out for dest beneath source when doing a recursive copy
set options= %* 
set options=%options:"=%
if /i not "%options: /? =%" == "%options%" robocopy.exe %* & set failed=1& ECHO [robocopy.cmd wrapper]& GOTO :EndOfScript
if /i not "%options: -? =%" == "%options%" robocopy.exe %* & set failed=1& ECHO [robocopy.cmd wrapper]& GOTO :EndOfScript
if /i "%options: /E =%" == "%options%" goto :RunRobocopy

call set src=%~f1
call set dest=%~f2

@REM -- don't allow copies to or from '\' since these are usually mistakes due to missing variables
if "%src:~0,1%"=="\" if not "%src:~1,1%"=="\" (
    echo robocopy : can not copy from '\'.  Use a proper path [%src%]
    set failed=1
    goto :EndOfScript
)
if "%dest:~0,1%"=="\" if not "%dest:~1,1%"=="\" (
    echo robocopy : can not copy to '\'.  Use a proper path [%dest%]
    set failed=1
    goto :EndOfScript
)

if not "%src:~-1%"=="\"  set src=%src%\
if not "%dest:~-1%"=="\" set dest=%dest%\
call set remainder=%%dest:%src%=%%

@REM if src isn't a parent of dest, then dest will not have changed and is == remainder
if /I not "%dest%" == "%remainder%" (
   echo robocopy : can not recursively copy source into a child destination. >&2
   set failed=1
   goto :EndOfScript
)

:RunRobocopy
@REM Make sure the _RoboCopy option environment variables are clean
set _RobocopyMaxPathLengthOption=
set _RobocopyReadonlyReset=

@REM Check if the environment specifies to use long paths for robocopy (RobocopyUseLongPaths == 1)
if not "%RobocopyUseLongPaths%" == "1" (
    @REM If using long paths is not explicitly specifed, then force /256 character max paths to avoid recursive copy mistakes
    set _RobocopyMaxPathLengthOption=/256
)
@REM Check if the environment specifies to respect readonly
if not "%RobocopyCopyReadonly%" == "1" (
    set _RobocopyReadonlyReset=/A-:R
)
robocopy.exe %_RobocopyReadonlyReset% %_RobocopyMaxPathLengthOption% %*
set /a failed=!!(%ERRORLEVEL%^&24)
if "%failed%"=="1" echo robocopy exit code %ERRORLEVEL%
goto :EndOfScript

:EndOfScript
IF "%bldEcho%"=="1" ECHO ON
@exit /b %failed%

--------------------------------------------------------------------------------
http://toolbox/sites/607/Documentation/Robocopy.doc -
The return code from Robocopy is a bit map, defined as follows:

Hex Bit  Decimal  Meaning If Set
  Value    Value
--------------------------------------------------------------------------------
   0x10       16  Serious error. Robocopy did not copy any files. This
                  is either a usage error or an error due to insufficient
                  access privileges on the source or destination directories.
   0x08        8  Some files or directories could not be copied (copy errors
                  occurred and the retry limit was exceeded). Check these errors
                  further.
   0x04        4  Some Mismatched files or directories were detected. Examine
                  the output log. Housekeeping is probably necessary.
   0x02        2  Some Extra files or directories were detected. Examine the
                  output log. Some housekeeping may be needed.
   0x01        1  One or more files were copied successfully (that is, new
                  files have arrived).
   0x00        0  No errors occurred, and no copying was done. The source and
                  destination directory trees are completely synchronized.
 --------------------------------------------------------------------------------
