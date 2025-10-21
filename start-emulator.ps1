# Start Android Emulator - Medium Phone API 36
Write-Host "Starting Android Emulator..." -ForegroundColor Cyan

$androidSdk = "C:\Users\AnVnt\AppData\Local\Android\Sdk"
$emulatorExe = "$androidSdk\emulator\emulator.exe"
$avdName = "Medium_Phone_API_36.0"

# Check if emulator exists
if (!(Test-Path $emulatorExe)) {
    Write-Host "Emulator not found at: $emulatorExe" -ForegroundColor Red
    exit 1
}

Write-Host "Found emulator: $emulatorExe" -ForegroundColor Green
Write-Host "Starting AVD: $avdName" -ForegroundColor Yellow
Write-Host ""

# Start emulator with optimization
& $emulatorExe -avd $avdName -netdelay none -netspeed full -dns-server 8.8.8.8

# Note: Emulator will run in this terminal
# Close terminal = close emulator
