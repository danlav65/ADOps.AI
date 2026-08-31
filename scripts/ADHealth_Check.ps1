# ==============================================================================
# ACTIVE DIRECTORY REPLICATION HEALTH CHECK
# Enterprise Replication Monitoring Script
#
# Author:  NA
# Date:  NA
#
# Update: Adrian Costea
# Date: 20.may.2026
# Notes: Optimization of the script to use as much as possible PowerShell cmdlets
#        instead of old commands like repadmin, dcdiag etc...
#        Some extra checks were added along with some fine tunes and errors handling
#
#
# Version: 1
# Revision: 2
#
# ==============================================================================

$RunStamp = Get-Date -Format "yyyyMMdd_HHmmss"
# Output Log File
$LogFile = "C:\Logs\ADReplicationHealthCheck.log"

# Create log folder if it does not exist
$LogFolder = Split-Path $LogFile
if (!(Test-Path $LogFolder)) {
    New-Item -ItemType Directory -Path $LogFolder -Force | Out-Null
}

# Timestamp setup
$TimeStamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss" 

# Initialize log file
@("=================================================",
  "AD Replication Health Check Started : $TimeStamp",
  "=================================================") | Add-Content -Path $LogFile

#Write-Host "`n=========================================" -ForegroundColor Cyan
#Write-Host " AD REPLICATION HEALTH CHECK" -ForegroundColor Cyan
#Write-Host "=========================================`n" -ForegroundColor Cyan

# ------------------------------------------------------------------------------
# Get Domain Controllers
# ------------------------------------------------------------------------------
try {
    # Ensure the ActiveDirectory module is loaded
    Import-Module ActiveDirectory -ErrorAction Stop
    #$DCs = Get-ADDomainController -Filter *
    $MainDC = "ZUSW-DC02"
    $DCs = $env:COMPUTERNAME #+ "@apcflex.aero"
}
catch {
    #Write-Host "CRITICAL: Failed to load AD Module or query Domain Controllers. Exiting script safely." -ForegroundColor Red
    Add-Content -Path $LogFile "CRITICAL: Failed to retrieve Domain Controllers. Error: $_"
    return # Safely stops this script without killing a parent PowerShell session
}

# ------------------------------------------------------------------------------
# Passive Replication Summary
# ------------------------------------------------------------------------------
#Write-Host "Running Replication Summary..." -ForegroundColor Yellow
Add-Content -Path $LogFile "`n--- PASSIVE REPLICATION SUMMARY ---"

# Capture summary data safely
$RepSummary = repadmin /replsummary $env:COMPUTERNAME 2>&1
$RepSummary | Add-Content -Path $LogFile
#Write-Host $RepSummary

# ------------------------------------------------------------------------------
# Detailed Status Per Domain Controller
# ------------------------------------------------------------------------------
foreach ($DC in $DCs) {
    #$DCName = $DC.HostName
    
    #Write-Host "`nChecking DC: $DCName" -ForegroundColor Green
    @("`n=========================================",
      "Checking DC: $DC",
      "=========================================") | Add-Content -Path $LogFile

    # 1. Network Availability Check (Prevents script hangs on offline/dead DCs)
    #if (-not (Test-Connection -ComputerName $DCName -Count 1 -Quiet)) {
        #Write-Host "CRITICAL: $DCName is unreachable via network ping! Skipping further diagnostics." -ForegroundColor Red
    #    Add-Content -Path $LogFile "CRITICAL: DC is offline or blocking ICMP ping. Skipping further checks."
    #    continue # Instantly jumps to the next DC in the loop
    #}

    # 2. Native Replication Failure Query (Accurate, object-based querying)
    #Write-Host "Querying replication metadata..." -ForegroundColor Yellow
    $ReplFailures = Get-ADReplicationFailure -Target $DC -ErrorAction SilentlyContinue

    if ($ReplFailures) {
        #Write-Host "Replication issues detected on $DCName" -ForegroundColor Red
        foreach ($Failure in $ReplFailures) {
            $ErrorMsg = "Failure: Partner -> $($Failure.Partner) | Code -> $($Failure.LastErrorCode) | Count -> $($Failure.FailureCount) | Last Attempt -> $($Failure.LastFailureTime)"
            Add-Content -Path $LogFile $ErrorMsg
            #Write-Host " -> $ErrorMsg" -ForegroundColor DarkRed
        }
    } else {
        #Write-Host "Native replication engine reports healthy on $DCName" -ForegroundColor Green
        Add-Content -Path $LogFile "Native replication engine reports healthy."
    }

    # 3. Fallback Detailed Log (Captures exact topology status for the log file)
    $ShowRepl = repadmin /showrepl $DC 2>&1
    $ShowRepl | Add-Content -Path $LogFile

    # 4. Target Diagnostic Tests (Combined into one dcdiag call to save time)
    #Write-Host "Running DCDiag (SYSVOL & Advertising)..." -ForegroundColor Yellow
    $DcDiagResult = dcdiag /test:sysvolcheck /test:advertising /s:$DC 2>&1
    $DcDiagResult | Add-Content -Path $LogFile
}

# ------------------------------------------------------------------------------
# DFSR Backlog Check (SYSVOL replication health)
# ------------------------------------------------------------------------------
#Write-Host "`nChecking DFSR Backlog..." -ForegroundColor Yellow
Add-Content -Path $LogFile "`n--- DFSR BACKLOG CHECK ---"

try {
    # Note: dfsrdiag requires RSAT DFSR Management tools installed on the machine running the script
    $DFSR = dfsrdiag backlog /ReceivingMember:$DC /SendingMember:$MainDC /rgname:"Domain System Volume" /rfname:"SYSVOL Share" 2>&1
    $DFSR | Add-Content -Path $LogFile
    
    #Write-Host "DFSR backlog check completed. Review log file for volume metrics." -ForegroundColor Green
}
catch {
    #Write-Host "DFSR backlog check threw an execution error. Check if RSAT DFSR tools are installed." -ForegroundColor Red
    Add-Content -Path $LogFile "DFSR check skipped or failed execution: $_"
}

# ------------------------------------------------------------------------------
# Wrap Up
# ------------------------------------------------------------------------------
$EndTime = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
Add-Content -Path $LogFile "`nAD Replication Health Check Completed : $EndTime"

#Write-Host "`n=========================================" -ForegroundColor Cyan
#Write-Host " AD REPLICATION CHECK COMPLETED" -ForegroundColor Cyan
#Write-Host " Log File : $LogFile" -ForegroundColor Cyan
#Write-Host "=========================================`n" -ForegroundColor Cyan