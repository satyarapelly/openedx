@ECHO OFF
del d:\data\Xpert.State.Internal\debug\* /F /Q
if not exist "d:\data\Xpert.Agent.Data" (
mkdir "d:\data\Xpert.Agent.Data"
)
copy /y NUL d:\data\Xpert.Agent.Data\enforced
Xpert.Agent /a