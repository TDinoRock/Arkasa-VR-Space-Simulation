# Generates offline voice clips for the AI guide from tools/guide_lines.txt
# Output: Assets/Arkasa/Audio/Guide/<id>.wav   (Windows built-in speech, no internet needed)
# Usage:  powershell -ExecutionPolicy Bypass -File tools/generate_guide_audio.ps1 [-Voice "Microsoft Zira Desktop"] [-Rate 0]
param([string]$Voice = "Microsoft Zira Desktop", [int]$Rate = 0)
Add-Type -AssemblyName System.Speech
$root = Split-Path -Parent $PSScriptRoot
$lines = Join-Path $root "tools/guide_lines.txt"
$out = Join-Path $root "Assets/Arkasa/Audio/Guide"
New-Item -ItemType Directory -Force $out | Out-Null
$s = New-Object System.Speech.Synthesis.SpeechSynthesizer
try { $s.SelectVoice($Voice) } catch { Write-Warning "Voice '$Voice' not found, using default." }
$s.Rate = $Rate
$n = 0
foreach ($l in Get-Content $lines) {
    $l = $l.Trim()
    if ($l -eq "" -or $l.StartsWith("#")) { continue }
    $i = $l.IndexOf("|"); if ($i -lt 1) { continue }
    $id = $l.Substring(0, $i).Trim(); $text = $l.Substring($i + 1).Trim()
    $fmt = New-Object System.Speech.AudioFormat.SpeechAudioFormatInfo(22050, [System.Speech.AudioFormat.AudioBitsPerSample]::Sixteen, [System.Speech.AudioFormat.AudioChannel]::Mono)
    $s.SetOutputToWaveFile((Join-Path $out "$id.wav"), $fmt)
    $s.Speak($text); $n++
}
$s.SetOutputToNull(); $s.Dispose()
Write-Host "Generated $n clips in $out"
