@echo off
setlocal

SET ISSUER=-r

:params
IF "%~1"=="" goto start
IF /I "%1"=="/Subject" ( SET Subject=%~2&&SHIFT 
) ELSE IF /I "%1"=="/File" ( SET File=%~2&&SHIFT 
) ELSE IF /I "%1"=="/EKU" ( 
	REM The EKUs can be found in wincrypt.h
	IF /I "%~2"=="SERVER" (SET EKU=%EKU%,1.3.6.1.5.5.7.3.1&&SHIFT
	) ELSE IF /I "%~2"=="CLIENT" (SET EKU=%EKU%,1.3.6.1.5.5.7.3.2&&SHIFT
	) ELSE IF /I "%~2"=="SIGN" (SET EKU=%EKU%,1.3.6.1.5.5.7.3.3&&SHIFT
	) ELSE IF /I "%~2"=="ENCRYPT" (SET SKY=-pe -r -sky exchange&&SHIFT
	) ELSE ( echo Unknown EKU value '%2' && exit /b 1 )
) ELSE IF /I "%1"=="/?" ( goto Usage 
) ELSE IF /I "%1"=="/Password" ( SET Password=%~2&&SHIFT 
) ELSE IF /I "%1"=="/CA" (SET CA=-cy authority
) ELSE IF /I "%1"=="/IssuerCA" (SET IssuerCA=-ic "%~2.cer" -iv "%~2.pvk"&&SHIFT
) ELSE ( echo Unknown option '%1' && exit /b 1 )
SHIFT
goto params

:start
IF "%Subject%"=="" ( echo ERROR: Subject is not specified. && goto Usage )
IF "%File%"=="" ( echo ERROR: File is not specified. && goto Usage )
IF "%Password%"=="" ( echo ERROR: Password is not specified. && goto Usage )

IF DEFINED EKU SET EKU=-eku %EKU:~1%

echo makecert -n "CN=%Subject%" -b 01/01/2000 -e 01/01/2099 %EKU% %CA% %IssuerCA% %SKY% -sv %File%.pvk %File%.cer
makecert -n "CN=%Subject%" -b 01/01/2000 -e 01/01/2099 %EKU% %CA% %IssuerCA% %SKY% -sv %File%.pvk %File%.cer
if errorlevel 1 (
	echo ERROR: makacert Failed
	exit /b 1
)

echo pvk2pfx -pvk %File%.pvk -spc %File%.cer -pfx %File%.pfx -po %Password%
pvk2pfx -pvk %File%.pvk -spc %File%.cer -pfx %File%.pfx -po %Password%
if errorlevel 1 (
	echo ERROR: pvk2pfx Failed
	exit /b 1
)
exit /b 0

:Usage
echo Usage: %0 /Option Value...
echo.
echo Options:
echo     Subject  - certificate subject
echo     IssuerCA - Issuer CA file (PaymentsOneBoxCA)
echo     File     - certificate file name
echo     CA       - indicates that this is CA cert (no value needed)
echo     Password - certificate install password
echo     EKU      - multiple options can be specified
echo                (Server, Client, Sign, Encrypt)
exit /b 1
