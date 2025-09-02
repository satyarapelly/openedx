$SQLServer = Invoke-Sqlcmd -Query "SELECT @@servername as Name"
$SQLServerName = $SQLServer.Name
	
"Configured SQL Server Name: $SQLServerName"
	
if($SQLServer.Name -ne $env:computername)
{
	"Renaming SQL Server to $env:computername"
	
	Invoke-Sqlcmd -Query "sp_dropserver '$SQLServerName'"
	Invoke-Sqlcmd -Query "sp_addserver '$env:computername', 'local'"

    net stop SQLSERVERAGENT
	net stop MSSQLServer 

	net start MSSQLServer
	net start SQLSERVERAGENT
}
