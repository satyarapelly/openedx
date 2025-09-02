MOVE "%1" "%1.tmp"
"%INETROOT%\public\ext\sdk\vs10sp1\sdk\bin\NETFX_4.0_Tools\ildasm.exe" "%1.tmp" /output:"%1.il"
"%INETROOT%\public\ext\sdk\vs10sp1\sdk\bin\NETFX_4.0_Tools\ilasm.exe" /dll "%1.il" /key="%INETROOT%\build\OCPKey.snk" /OUTPUT="%1"