# Start Android Emulator in Background
Write-Host "Starting Android Emulator in background..." -ForegroundColor Cyan

$androidSdk = "C:\Users\AnVnt\AppData\Local\Android\Sdk"
$emulatorExe = "$androidSdk\emulator\emulator.exe"
$avdName = "Medium_Phone_API_36.0"

# Start emulator as background process
$process = Start-Process -FilePath $emulatorExe `
    -ArgumentList "-avd", $avdName, "-netdelay", "none", "-netspeed", "full" `
    -PassThru `
    -WindowStyle Normal

Write-Host "Emulator started (PID: $($process.Id))" -ForegroundColor Green
Write-Host "AVD: $avdName" -ForegroundColor Yellow
Write-Host ""
Write-Host "Waiting for emulator to boot (30-60s)..." -ForegroundColor Cyan
Write-Host "Check status: adb devices" -ForegroundColor White

# Wait for device
$maxWait = 60
$waited = 0
Write-Host "Waiting for device" -NoNewline

while ($waited -lt $maxWait) {
    $devices = & "$androidSdk\platform-tools\adb.exe" devices 2>$null | Select-String "emulator"
    if ($devices) {
        Write-Host ""
        Write-Host "Emulator is online!" -ForegroundColor Green
        & "$androidSdk\platform-tools\adb.exe" devices
        break
    }
    Write-Host "." -NoNewline
    Start-Sleep -Seconds 2
    $waited += 2
}

if ($waited -ge $maxWait) {
    Write-Host ""
    Write-Host "Emulator not online after ${maxWait}s" -ForegroundColor Yellow
    Write-Host "Check manually: adb devices" -ForegroundColor White
}

Write-Host ""
Write-Host "=== Next Steps ===" -ForegroundColor Cyan
Write-Host "1. Emulator is running in background" -ForegroundColor White
Write-Host "2. Deploy MAUI app: dotnet build -t:Run -f net8.0-android" -ForegroundColor White
Write-Host "3. Kill emulator: adb emu kill" -ForegroundColor White
