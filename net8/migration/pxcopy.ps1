<# Backup-PXServiceFiles.ps1
Copies a fixed list of PXService files to a backup directory, preserving folders.

Usage:
  .\Backup-PXServiceFiles.ps1 `
    -RepoRoot "C:\Users\v-srapelly\source\repos\openedx\net8\migration" `
    -BackupRoot "C:\Users\v-srapelly\source\repos\_backup_PXService"

Optional:
  -SkipIfSame   # skip copy when destination file already exists with same hash
  -DryRun       # show what would be copied without writing files
#>

[CmdletBinding()]
param(
  [Parameter(Mandatory=$true)]
  [string]$RepoRoot,                           # root that contains 'net8\migration\PXService\...'

  [Parameter(Mandatory=$true)]
  [string]$BackupRoot,                         # where to place the backup

  [switch]$SkipIfSame,
  [switch]$DryRun
)

# The files to back up (relative to $RepoRoot)
$Files = @(
"/PXService/ClientActionFilter.cs"
"/PXService/ConfigurationComponentRule.cs"
"/PXService/ContextHelper.cs"
"/PXService/ErrorDetailsFilter.cs"
"/PXService/HttpRequestHelper.cs"
"/PXService/HttpRequestMessageOptionsExtensions.cs"
"/PXService/PXServiceApiVersionHandler.cs"
"/PXService/PXServiceInputValidationHandler.cs"
"/PXService/Telemetry/ApplicationInsightsProvider.cs"
"/PXService/V7/GuestAccountHelper.cs"
"/PXService/V7/PaymentClient/ComponentDescriptions/ComponentDescription.cs"
"/PXService/V7/PaymentClient/ComponentDescriptions/PaymentRequestConfirmDescription.cs"
)

function New-ParentDir {
  param([string]$Path)
  $dir = Split-Path -Parent $Path
  if ($dir -and -not (Test-Path -LiteralPath $dir)) {
    if ($DryRun) { Write-Host "[DryRun] mkdir $dir" -ForegroundColor Yellow }
    else { New-Item -ItemType Directory -Force -Path $dir | Out-Null }
  }
}

function Same-File {
  param([string]$A, [string]$B)
  if (-not (Test-Path -LiteralPath $A -PathType Leaf) -or -not (Test-Path -LiteralPath $B -PathType Leaf)) {
    return $false
  }
  try {
    $ha = Get-FileHash -LiteralPath $A -Algorithm SHA256
    $hb = Get-FileHash -LiteralPath $B -Algorithm SHA256
    return $ha.Hash -eq $hb.Hash
  } catch { return $false }
}

$results = @()

foreach ($rel in $Files) {
  $src  = Join-Path -Path $RepoRoot -ChildPath $rel
  $dest = Join-Path -Path $BackupRoot -ChildPath $rel

  if (-not (Test-Path -LiteralPath $src -PathType Leaf)) {
    Write-Warning "Missing source: $src"
    $results += [pscustomobject]@{ File=$rel; Status="MissingSource"; Source=$src; Dest=$dest }
    continue
  }

  if ($SkipIfSame -and (Test-Path -LiteralPath $dest -PathType Leaf) -and (Same-File $src $dest)) {
    Write-Host "Skip (same): $rel"
    $results += [pscustomobject]@{ File=$rel; Status="SkippedSame"; Source=$src; Dest=$dest }
    continue
  }

  New-ParentDir -Path $dest

  if ($DryRun) {
    Write-Host "[DryRun] Copy $src -> $dest" -ForegroundColor Yellow
    $results += [pscustomobject]@{ File=$rel; Status="Planned"; Source=$src; Dest=$dest }
  } else {
    try {
      Copy-Item -LiteralPath $src -Destination $dest -Force
      Write-Host "Copied: $rel"
      $results += [pscustomobject]@{ File=$rel; Status="Copied"; Source=$src; Dest=$dest }
    } catch {
      Write-Error "Failed to copy $rel : $($_.Exception.Message)"
      $results += [pscustomobject]@{ File=$rel; Status="Error"; Source=$src; Dest=$dest }
    }
  }
}

# Summary
$results | Sort-Object File | Format-Table -AutoSize
