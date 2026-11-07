# Arkasa operator console: pause / resume / EMERGENCY STOP / reset the headset session over Wi-Fi.
# The headset app listens on UDP 47770. PC and Quest must be on the same network.
# Usage:  powershell -ExecutionPolicy Bypass -File tools/operator.ps1 -HeadsetIp 192.168.1.50
#         (find the IP in Quest Settings > Wi-Fi > your network; use 127.0.0.1 when testing in the Editor)
param([Parameter(Mandatory = $true)][string]$HeadsetIp, [int]$Port = 47770)

$udp = New-Object System.Net.Sockets.UdpClient
$udp.Client.ReceiveTimeout = 1500

function Send([string]$cmd) {
    $b = [Text.Encoding]::UTF8.GetBytes($cmd)
    [void]$udp.Send($b, $b.Length, $HeadsetIp, $Port)
    try {
        $ep = New-Object System.Net.IPEndPoint([Net.IPAddress]::Any, 0)
        $reply = [Text.Encoding]::UTF8.GetString($udp.Receive([ref]$ep))
        Write-Host "  headset: $reply" -ForegroundColor Green
    } catch { Write-Host "  no reply (headset unreachable or app not running)" -ForegroundColor Yellow }
}

Write-Host "Arkasa operator -> $HeadsetIp`:$Port"
Write-Host "  [S] or [Space] EMERGENCY STOP   [P] pause   [R] resume   [X] reset session   [I] status   [Q] quit"
while ($true) {
    $k = [Console]::ReadKey($true)
    switch ($k.Key) {
        'S'        { Write-Host "EMERGENCY STOP" -ForegroundColor Red; Send "ESTOP" }
        'Spacebar' { Write-Host "EMERGENCY STOP" -ForegroundColor Red; Send "ESTOP" }
        'P'        { Write-Host "pause";  Send "PAUSE" }
        'R'        { Write-Host "resume"; Send "RESUME" }
        'X'        { Write-Host "reset";  Send "RESET" }
        'I'        { Send "STATUS" }
        'Q'        { $udp.Close(); return }
    }
}
