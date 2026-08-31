#Port Scan utility to verify open ports to On-Premises Servers
# Following are examples of how to use the tool
# Scan first 1024 ports .\Invoke-NmapStylePortScan.ps1 -Target "server01"
# Full 1-65535 scan .\Invoke-NmapStylePortScan.ps1 -Target "10.0.0.12" -Ports (1..65535)
# Max out threads for extreme speed .\Invoke-NmapStylePortScan.ps1 -Target "server01" -Ports (1..65535) -Threads 500
# Scan specific high value ports .\Invoke-NmapStylePortScan.ps1 -Target "server01" -Ports 22,80,443,3389,5985,5986

#$Target = "zuse-dc01"
param(
    [Parameter(Mandatory=$true)]
    [string]$Target,

    [int[]]$Ports = 1..1024,

    [int]$Timeout = 1500,

    [int]$Threads = 200
)

# Create runspace pool
$sessionState = [System.Management.Automation.Runspaces.InitialSessionState]::CreateDefault()
$pool = [runspacefactory]::CreateRunspacePool(1, $Threads, $sessionState, $host)
$pool.Open()

$tasks = @()

foreach ($port in $Ports) {
    $ps = [powershell]::Create()
    $ps.RunspacePool = $pool

    $null = $ps.AddScript({
        param($Target,$Port,$Timeout)

        $client = New-Object System.Net.Sockets.TcpClient
        $async = $client.BeginConnect($Target,$Port,$null,$null)
        $wait  = $async.AsyncWaitHandle.WaitOne($Timeout,$false)

        if ($wait -and $client.Connected) {
            $client.EndConnect($async)
            $client.Close()

            [PSCustomObject]@{
                Port = $Port
                State = "open"
            }
        } else {
            $client.Close()
        }
    }).AddArgument($Target).AddArgument($Port).AddArgument($Timeout)

    $tasks += [PSCustomObject]@{
        Port = $port
        Handle = $ps.BeginInvoke()
        PowerShell = $ps
    }
}

Write-Host "Starting scan of $Target" -ForegroundColor Cyan
Write-Host "Scanning $($Ports.Count) ports using $Threads threads..." -ForegroundColor Cyan

$openPorts = @()

foreach ($task in $tasks) {
    $result = $task.PowerShell.EndInvoke($task.Handle)
    $task.PowerShell.Dispose()

    if ($result) { $openPorts += $result }
}

$pool.Close()

# ---- NMAP‑STYLE OUTPUT ----
Write-Host ""
Write-Host "Port scan report for $Target"
Write-Host "Host is up."

if ($openPorts.Count -eq 0) {
    Write-Host "No open ports found" -ForegroundColor Yellow
    return
}

Write-Host ""
Write-Host "PORT     STATE"
foreach ($p in $openPorts | Sort-Object Port) {
    Write-Host ("{0}/tcp   {1}" -f $p.Port, $p.State)
}