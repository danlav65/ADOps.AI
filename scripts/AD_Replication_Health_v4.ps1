<# =================================================================================================
  FLEX GLOBAL DOMAIN AD REPLICATION /showrepl ERROR MONITOR (v4)
  - Sends email only when repadmin /showrepl reports failures (unless -SendAlways)
  - On failures, also runs:
      * Invoke-PortScan (captures output to log + attaches)
      * ADHealth_Check (attaches C:\Logs\ADReplicationHealthCheck.log + execution output)
  ================================================================================================= #>

[CmdletBinding()]
param(
    # Targets for repadmin /showrepl (default: local computer)
    [string[]]$Targets = @($env:COMPUTERNAME),

    # Send a report even if there are no failures
    [bool]$SendAlways = $true,

    # Use /errorsonly for repadmin (recommended)
    [bool]$UseErrorsOnly = $true,

    # Folder where v4 writes its own logs (repadmin output, portscan output, execution logs)
    [string]$LogRoot = "C:\Logs\AD_Replication_Health",

    # Paths to helper scripts (defaults: same directory as this v4 script)
    [string]$InvokePortScanPath = "",
    [string]$ADHealthCheckPath  = "",

    # PortScan settings (used only when failures detected)
    [int[]]$PortScanPorts = @(53, 88, 135, 139, 389, 445, 464, 636, 3268, 3269, 3389, 5985, 5986),
    [int]$PortScanTimeout = 1500,
    [int]$PortScanThreads = 200
)

# ---------------------------
# Mail config
# ---------------------------
$SMTPServer = "mail.ads.sita.net"
$From = $env:COMPUTERNAME + "@apcflex.aero"
$To = @("sameh.bashatly@sita.aero","Sabin.Georgescu@sita.aero","Carl.Dufresne@sita.aero","Paul.Topan@sita.aero","CR.Lakshmikanthan@sita.aero","Mohammed_Jane.Alam@sita.aero",
#    "Cluj.Infra.Team@sita.aero",
#     "Adrian.Costea@sita.aero",

"Danny.Lavardera@sita.aero"
)

# ---------------------------
# Helpers
# ---------------------------
function Ensure-Folder([string]$Path) {
    if (-not (Test-Path $Path)) {
        New-Item -ItemType Directory -Path $Path -Force | Out-Null
    }
}

function Resolve-ScriptPath {
    param(
        [string]$ProvidedPath,
        [string[]]$CandidateNames
    )
    if ($ProvidedPath -and (Test-Path $ProvidedPath)) { return (Resolve-Path $ProvidedPath).Path }

    $root = $PSScriptRoot
    foreach ($name in $CandidateNames) {
        $p = Join-Path $root $name
        if (Test-Path $p) { return (Resolve-Path $p).Path }
    }
    return $null
}

function Write-Log {
    param(
        [string]$Message,
        [string]$Path
    )
    $ts = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $line = "[$ts] $Message"
    $line | Add-Content -Path $Path
    #Write-Host $line
}

# ---------------------------
# Init log folder + filenames
# ---------------------------
Ensure-Folder -Path $LogRoot
$RunStamp = Get-Date -Format "yyyyMMdd_HHmmss"
$MainLog  = Join-Path $LogRoot "AD_Replication_Health_v4_$RunStamp.log"

# Compute SHA256 hash of this script
$ScriptPath = $MyInvocation.MyCommand.Path
$ScriptHash = Get-FileHash -Path $ScriptPath -Algorithm SHA256
Write-Log "Script SHA256: $($ScriptHash.Hash)" $MainLog


#if ($Host.Name -like "*ISE*") {
#    Write-Host "ISE detected → skipping PortScan to avoid crash"
    $RunPortScan = $false
#} else {
#    $RunPortScan = $true
#}


Write-Log "Starting AD Replication Health v4. Computer=$env:COMPUTERNAME Targets=$($Targets -join ', ')" $MainLog

# Resolve helper scripts (prefer .ps1, fallback to .txt if that’s what you keep)
$ResolvedPortScan = Resolve-ScriptPath -ProvidedPath $InvokePortScanPath -CandidateNames @("Invoke-PortScan.ps1")
$ResolvedADHealth  = Resolve-ScriptPath -ProvidedPath $ADHealthCheckPath  -CandidateNames @("ADHealth_Check.ps1")

Write-Log "Resolved Invoke-PortScan path: $ResolvedPortScan" $MainLog
Write-Log "Resolved ADHealth_Check path : $ResolvedADHealth"  $MainLog

# ---------------------------
# Run repadmin and parse failures
# ---------------------------
$allFailures  = @()
$rawOutputAll = New-Object System.Collections.Generic.List[string]

foreach ($t in $Targets) {
    $args = @("/showrepl", $t, "/verbose")
    if ($UseErrorsOnly) { $args = @("/showrepl", $t,"/errorsonly", "/verbose") }

    Write-Log "Running: repadmin $($args -join ' ')" $MainLog

    $raw = & repadmin @args 2>&1
    foreach ($line in $raw) { $rawOutputAll.Add([string]$line) }

    # Basic failure detection: look for "Last error:" and a non-zero code
    foreach ($line in $raw) {

    # DC header often appears like "DOMAIN\DCNAME"
        if ($line -match '^\s*([A-Za-z0-9._-]+\\[A-Za-z0-9._-]+)\s*$') {
            $currentDC = $matches[1].Trim()            
            continue
        }

        # Naming context line (starts with DC= or CN=)
        if ($line -match '^\s*(DC=|CN=).+$') {
            $currentNC = $line.Trim()
            $currentPartner = $null            
            continue
        }

        # Partner line (e.g. "Azure-ZUSE\ZUSE-DC01 via RPC")
        if ($line -match '^\s*(?<partner>\S+)\s+via\s+\S+\s*$') {
            $currentPartner = $matches.partner.Trim()
            continue
        }

        # Failure line
        if ($line -match '^\s*Last attempt\s+@\s+(?<time>.+?)\s+failed,\s+result\s+(?<result>.+)$') {
            $allFailures += [pscustomobject]@{
                DC        = $currentDC
                NamingCtx = $currentNC
                Partner   = $currentPartner
                Time      = $matches.time.Trim()
                Result    = $matches.result.Trim()
               }
        }

        if ($line -match 'Last\s+error:\s*(\d+)\s*\(0x[0-9a-fA-F]+\)\s*:\s*(.+)$') {
            $code = [int]$Matches[1]
            $msg  = $Matches[2].Trim()
            if ($code -ne 0) {
                $allFailures += [PSCustomObject]@{
                    Target   = $t
                    Code     = $code
                    Message  = $msg
                    RawLine  = [string]$line
                }
            }
        }
    }
}

# Create raw repadmin output attachment
$RepadminLog = Join-Path $LogRoot "repadmin_showrepl_$RunStamp.log"
$rawOutputAll | Set-Content -Path $RepadminLog -Encoding UTF8
Write-Log "Saved repadmin raw output to: $RepadminLog" $MainLog

# Summary text
$summary = ""
if ($allFailures.Count -gt 0) {
    $summaryLines = $allFailures |
        Group-Object -Property Target,Code,Message |
        Sort-Object Count -Descending |
        ForEach-Object {
            $p = $_.Group[0]
            "Target=$($p.Target)  Code=$($p.Code)  Message=$($p.Message)  Occurrences=$($_.Count)"
        }
    $summary = ($summaryLines -join "`r`n")
} else {
    $summary = "No replication errors detected by parsing."
}

# ---------------------------
# When failures: run helpers + gather attachments
# ---------------------------
$attachments = New-Object System.Collections.Generic.List[string]
$attachments.Add($MainLog)     | Out-Null
$attachments.Add($RepadminLog) | Out-Null

# Attempt to extract partner/DC names from repadmin output for targeted port scans
function Get-PartnerTargetsFromRepadminOutput {
    param([string[]]$Lines)

    $partners = New-Object System.Collections.Generic.HashSet[string]
    foreach ($l in $Lines) {
        # Common patterns include "SITE\DCNAME via RPC"
        if ($l -match '\\([A-Za-z0-9\-_\.]+)\s+via\s+') {
            $null = $partners.Add($Matches[1])
        }
        # Sometimes repadmin shows "Source: DCNAME" or similar
        if ($l -match 'Source:\s*([A-Za-z0-9\-_\.]+)') {
            $null = $partners.Add($Matches[1])
        }
    }
    return $partners.ToArray()
}

$FailureDetected = ($allFailures.Count -gt 0)

#if ($FailureDetected) {
    Write-Log "Failures detected: $($allFailures.Count). Running additional diagnostics for attachments..." $MainLog
    
    Write-Host "Diagnostics"
    # --- Run ADHealth_Check ---
    if ($ResolvedADHealth) {
        $ADHealthExecLog = Join-Path $LogRoot "ADHealth_Check_exec_$RunStamp.log"
        try {
            Write-Log "Executing ADHealth_Check: $ResolvedADHealth" $MainLog
            # Capture all streams. (If helper uses Write-Host, it may not fully redirect, but errors/output will.)
            & $ResolvedADHealth *>&1 | Tee-Object -FilePath $ADHealthExecLog | Out-Null

            if (Test-Path $ADHealthExecLog) {
                $attachments.Add($ADHealthExecLog) | Out-Null
                Write-Log "Attached ADHealth_Check execution log: $ADHealthExecLog" $MainLog
            }

            # Attach the log file that ADHealth_Check writes
            $ADHealthMainLog = "C:\Logs\ADReplicationHealthCheck.log"
            if (Test-Path $ADHealthMainLog) {
                $attachments.Add($ADHealthMainLog) | Out-Null
                Write-Log "Attached ADHealth_Check main log: $ADHealthMainLog" $MainLog
            } else {
                Write-Log "ADHealth_Check main log not found at $ADHealthMainLog" $MainLog
            }
        }

        catch {
            Write-Log "ERROR running ADHealth_Check: $($_.Exception.Message)" $MainLog
            if (Test-Path $ADHealthExecLog) { $attachments.Add($ADHealthExecLog) | Out-Null }
        }
    } else {
        Write-Log "ADHealth_Check script not found. Skipping." $MainLog
    }

    # --- Run Invoke-PortScan ---
    if ($ResolvedPortScan -and $RunPortScan) {
        # Prefer partner targets found in repadmin output; fallback to $Targets
        $partnerTargets = Get-PartnerTargetsFromRepadminOutput -Lines $rawOutputAll.ToArray()
        if (-not $partnerTargets -or $partnerTargets.Count -eq 0) {
            $partnerTargets = $Targets
        }

        foreach ($pt in ($partnerTargets | Sort-Object -Unique)) {
            $PortScanLog = Join-Path $LogRoot ("PortScan_{0}_{1}.log" -f $pt, $RunStamp)
            try {
                Write-Log "Running port scan against $pt (Ports=$($PortScanPorts -join ','), Timeout=$PortScanTimeout, Threads=$PortScanThreads)" $MainLog

                # Capture Information (Write-Host) + Error + Output streams to file where possible.
                $InformationPreference = "Continue"
                & $ResolvedPortScan -Target $pt -Ports $PortScanPorts -Timeout $PortScanTimeout -Threads $PortScanThreads 6>&1 2>&1 |
                    Tee-Object -FilePath $PortScanLog | Out-Null

                if (Test-Path $PortScanLog) {
                    $attachments.Add($PortScanLog) | Out-Null
                    Write-Log "Attached PortScan log: $PortScanLog" $MainLog
                }
            }
            catch {
                Write-Log "ERROR running port scan for $pt : $($_.Exception.Message)" $MainLog
                if (Test-Path $PortScanLog) { $attachments.Add($PortScanLog) | Out-Null }
            }
        }
    } else {
        Write-Log "Invoke-PortScan script not found or Skipped." $MainLog
    }
#}

# ---------------------------
# Build email body
# ---------------------------
$rawText = ($rawOutputAll.ToArray() -join "`r`n")

$body = @"
=====================================
ACTIVE DIRECTORY REPLICATION ALERT (v4)
Command: repadmin /showrepl $(if ($UseErrorsOnly) {"/errorsonly"} else {""}) /verbose

Server running check : $env:COMPUTERNAME
Time : $(Get-Date)

Failures found : $($allFailures.Count)

--- Summary (parsed) ---
$summary

--- Raw repadmin output ---
$rawText

--- Attachments included ---
$($attachments | ForEach-Object { " - $_" } | Out-String)

=====================================
"@

# ---------------------------
# Send mail: only on failures, unless SendAlways
# ---------------------------
if ($FailureDetected) {
    $subject = "CRITICAL Flex Domain AD Replication errors on $env:COMPUTERNAME ($($allFailures.Count) failures)"

    Write-Host "Sending email"
    Write-Log "Sending email to: $($To -join ', ')" $MainLog
    Write-Log "Subject: $subject" $MainLog

    try {
        Send-MailMessage -SmtpServer $SMTPServer `
            -Priority High `
            -From $From `
            -To $To `
            -Subject $subject `
            -Body $body `
            -Attachments $attachments.ToArray()

        Write-Log "Email sent successfully." $MainLog
    }
    catch {
        Write-Log "ERROR sending email: $($_.Exception.Message)" $MainLog
        throw
    }
} #else {
  #  Write-Log "No failures detected and -SendAlways is false. No email sent." $MainLog
  elseif ($SendAlways) {
    $subject = "Flex Domain AD Replication report on $env:COMPUTERNAME ($($allFailures.Count) failures)"

    Write-Host "Sending email"
    Write-Log "Sending email to: $($To -join ', ')" $MainLog
    Write-Log "Subject: $subject" $MainLog

    try {
        Send-MailMessage -SmtpServer $SMTPServer -From $From `
            -To $To `
            -Subject $subject `
            -Body $body `
            -Attachments $attachments.ToArray()

        Write-Log "Email sent successfully." $MainLog
    }
    catch {
        Write-Log "ERROR sending email: $($_.Exception.Message)" $MainLog
        throw
    }
}

Write-Log "Completed AD Replication Health v4 run." $MainLog
Remove-Item -Path "C:\Logs\*" -Include ADReplicationHealthCheck.log -Recurse -Force