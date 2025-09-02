$processInfo = $null;
$job = $null;


$EventIds = @{
 "StartingXpertTask"=22001;
 "ProcessStarted"=22002;
 "ProcessDidNotStart"=22003;
 "UpgradeInProgress"=22004;
 "ProcessAlreadyRunning"=22005;
 "TooManyProcessesRunning"=22006;
 "ProcessVerificationFailed"=2207;
 "ProcessVerificationSucceeded"=2208;
 "UnhandledError"=2209;
 }

$XpertTaskName = "XpertAgent";
$ProcessName = "Xpert.Agent";
$EventSource = "NONAP_XPERT_AGENT";
$ProcessCheckDuration = 60;
$DataDir = "%DATADIR%"


function StartProcess()
{
    #set the environment variable DATADIR
    $env:DATADIR = $DataDir;


        #check for Eventlog Source of $EventSrouce if no then create it.
        if ([System.Diagnostics.EventLog]::SourceExists($Eventsource) -eq $false) {
        [System.Diagnostics.EventLog]::CreateEventSource($Eventsource, "Application")
        }

    Write-EventLog -Logname Application -EntryType Information -EventId  $EventIds["StartingXpertTask"] -Source $EventSource -Message "Starting the task '$XpertTaskName'. If this event occurs more than a couple of times a day, the agent may not be running successfully."; 
    schtasks /run /tn $XpertTaskName

    #Now wait for 10 seconds before proceeding. This gives enough time for the task to start.
    Start-Sleep -s 10;

    try
    {
        $agentProcessInfo = Get-Process "$ProcessName" -ErrorAction Stop 

        if ($agentProcessInfo)
        {
            Write-EventLog -Logname Application -EntryType Information -EventId  $EventIds["ProcessStarted"] -Source $EventSource -Message "Verified that Process $ProcessName has launched."; 
            Start-Sleep -s $ProcessCheckDuration
            
			if ($agentProcessInfo.HasExited)
            {
                Write-EventLog -Logname Application -EntryType Error -EventId  $EventIds["ProcessVerificationFailed"] -Source $EventSource -Message "Process $ProcessName finished in less than $ProcessCheckDuration seconds."; 
                Exit;
            }
            Write-EventLog -Logname Application -EntryType Information -EventId  $EventIds["ProcessVerificationSucceeded"] -Source $EventSource -Message "Checked on the process after $ProcessCheckDuration seconds. Process $ProcessName started successfully."; 				
        }
        else
        {
            Write-EventLog -Logname Application -EntryType Error -EventId  $EventIds["ProcessDidNotStart"] -Source $EventSource -Message "Process $ProcessName failed to start. Either the task $XpertTaskName is not present, was present but failed, or the process failed to start."; 
        }
    }
    catch [exception]
    {
        Write-EventLog -Logname Application -EntryType Error -EventId  $EventIds["ProcessDidNotStart"] -Source $EventSource -Message "Process $ProcessName failed to start. Either the task $XpertTaskName is not present, was present but failed, or the process failed to start. Detailed error '$($_.Exception.Message)'."; 
    }
}

try
{
    $processInfo = get-process $ProcessName -ErrorAction Stop;

    if ($processInfo -is  "System.Collections.ICollection")
    {		
        if ($processInfo.Length -gt 10)
        {
            Write-EventLog -Logname Application -EntryType Error -EventId  $EventIds["TooManyProcessesRunning"] -Source $EventSource -Message "Too many processes ($($processInfo.Length)) found. Investigate.";
        }
        else
        {
            Write-EventLog -Logname Application -EntryType Information -EventId  $EventIds["UpgradeInProgress"] -Source $EventSource -Message "Multiple processes ($($processInfo.Length)) found. An upgrade may be in progresss.";
        }
    }
    else
    {
        $runtime = (New-Timespan -Start($processInfo).StartTime);
        Write-EventLog -Logname Application -EntryType Information -EventId  $EventIds["ProcessAlreadyRunning"] -Source $EventSource -Message "Xpert agent version '$($processInfo.ProductVersion)' running since '$($processInfo.StartTime)' ($($runtime.Days) Days,  $($runtime.Hours) Hours, $($runtime.Minutes) Minutes, $($runtime.Seconds) Seconds)";
    }
}
catch  
{
    StartProcess;
}